using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace FakelordUI.Core
{
    public class GhostProcessManager
    {
        private static Process? _currentProcess;
        public static string? CurrentRunningGame { get; private set; }

        private static string RunnerDirectory => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Runners");
        private static string MainMasterExePath => Path.Combine(RunnerDirectory, "HaYTooL.exe");

        // Uygulama açılışında çağrılır: Runners klasörünü temizler, sadece HaYTooL.exe'yi tutar
        public static void InitializeRunnersDirectory()
        {
            try
            {
                Directory.CreateDirectory(RunnerDirectory);

                // 1. HaYTooL.exe haricindeki tüm eski exe, dosya ve alt klasörleri temizle
                foreach (var file in Directory.GetFiles(RunnerDirectory, "*", SearchOption.AllDirectories))
                {
                    string fileName = Path.GetFileName(file);
                    if (!fileName.Equals("HaYTooL.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        try { File.Delete(file); } catch { }
                    }
                }
                foreach (var dir in Directory.GetDirectories(RunnerDirectory))
                {
                    try { Directory.Delete(dir, true); } catch { }
                }

                // 2. HaYTooL.exe yoksa veya bozuksa örnek ana exe olarak oluştur / kopyala
                if (!File.Exists(MainMasterExePath) || new FileInfo(MainMasterExePath).Length == 0)
                {
                    string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RunnersTemplate", "HaYTooL.exe");
                    if (File.Exists(templatePath))
                    {
                        File.Copy(templatePath, MainMasterExePath, true);
                    }
                    else
                    {
                        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("FakelordUI.RunnersTemplate.HaYTooL.exe");
                        if (stream != null)
                        {
                            using var fs = new FileStream(MainMasterExePath, FileMode.Create, FileAccess.Write);
                            stream.CopyTo(fs);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("InitializeRunnersDirectory Hata: " + ex.Message);
            }
        }

        public static bool StartGame(string exeName, out string error)
        {
            error = string.Empty;
            StopGame();

            try
            {
                InitializeRunnersDirectory();

                if (string.IsNullOrWhiteSpace(exeName))
                {
                    error = "Oyun dosya adı boş!";
                    return false;
                }

                // Göreceli klasör yolunu normalize et (Örn: command & conquer red alert ii/game.exe)
                string normalizedExe = exeName.Replace('/', Path.DirectorySeparatorChar).TrimStart('\\', '/');
                if (normalizedExe.Equals("valorant-win64-shipping.exe", StringComparison.OrdinalIgnoreCase))
                {
                    normalizedExe = Path.Combine("win64", "valorant-win64-shipping.exe");
                }

                var parts = normalizedExe.Split(Path.DirectorySeparatorChar);
                for (int i = 0; i < parts.Length; i++)
                {
                    parts[i] = string.Join("_", parts[i].Split(Path.GetInvalidFileNameChars()));
                }
                string cleanRelativePath = string.Join(Path.DirectorySeparatorChar.ToString(), parts);
                string targetExePath = Path.Combine(RunnerDirectory, cleanRelativePath);

                string? targetDir = Path.GetDirectoryName(targetExePath);
                if (!string.IsNullOrEmpty(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                if (File.Exists(MainMasterExePath))
                {
                    File.Copy(MainMasterExePath, targetExePath, true);
                }
                else
                {
                    using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("FakelordUI.RunnersTemplate.HaYTooL.exe");
                    if (stream != null)
                    {
                        using var fs = new FileStream(targetExePath, FileMode.Create, FileAccess.Write);
                        stream.CopyTo(fs);
                    }
                }

                if (!File.Exists(targetExePath))
                {
                    error = $"Oyun dosyası oluşturulamadı: {targetExePath}";
                    return false;
                }

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = targetExePath,
                    WorkingDirectory = targetDir ?? RunnerDirectory,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Normal
                };

                _currentProcess = Process.Start(psi);
                CurrentRunningGame = cleanRelativePath;
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Debug.WriteLine("StartGame Hata: " + ex.ToString());
                return false;
            }
        }

        public static void StopGame()
        {
            if (_currentProcess != null && !_currentProcess.HasExited)
            {
                try
                {
                    _currentProcess.Kill();
                    _currentProcess.Dispose();
                }
                catch { }
            }
            _currentProcess = null;
            CurrentRunningGame = null;

            // Oyun durdurulduğunda temizlik yap, sadece HaYTooL.exe kalsın
            CleanGameCopies();
        }

        private static void CleanGameCopies()
        {
            try
            {
                if (!Directory.Exists(RunnerDirectory)) return;

                foreach (var file in Directory.GetFiles(RunnerDirectory))
                {
                    string fileName = Path.GetFileName(file);
                    if (!fileName.Equals("HaYTooL.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        try { File.Delete(file); } catch { }
                    }
                }
            }
            catch { }
        }
    }
}
