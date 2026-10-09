using System;
using System.Diagnostics;
using System.IO;

namespace FakelordUI.Core
{
    public class GhostProcessManager
    {
        private static Process? _currentProcess;
        public static string? CurrentRunningGame { get; private set; }

        // TestLab'da çalışan ve kanıtlanan saf bağımsız exe şablonu (Base64)
        private const string VerifiedConsoleRunnerBase64 = "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AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA====";

        private static string RunnerDirectory => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Runners");
        private static string MainMasterExePath => Path.Combine(RunnerDirectory, "main.exe");

        // Uygulama açılışında çağrılır: Runners klasörünü temizler, sadece main.exe'yi tutar
        public static void InitializeRunnersDirectory()
        {
            try
            {
                Directory.CreateDirectory(RunnerDirectory);

                // 1. main.exe haricindeki tüm eski exe, dosya ve alt klasörleri temizle
                foreach (var file in Directory.GetFiles(RunnerDirectory, "*", SearchOption.AllDirectories))
                {
                    string fileName = Path.GetFileName(file);
                    if (!fileName.Equals("main.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        try { File.Delete(file); } catch { }
                    }
                }
                foreach (var dir in Directory.GetDirectories(RunnerDirectory))
                {
                    try { Directory.Delete(dir, true); } catch { }
                }

                // 2. main.exe yoksa veya bozuksa örnek ana exe olarak oluştur
                if (!File.Exists(MainMasterExePath) || new FileInfo(MainMasterExePath).Length == 0)
                {
                    byte[] exeBytes = Convert.FromBase64String(VerifiedConsoleRunnerBase64);
                    File.WriteAllBytes(MainMasterExePath, exeBytes);
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
                    byte[] exeBytes = Convert.FromBase64String(VerifiedConsoleRunnerBase64);
                    File.WriteAllBytes(targetExePath, exeBytes);
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

            // Oyun durdurulduğunda temizlik yap, sadece main.exe kalsın
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
                    if (!fileName.Equals("main.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        try { File.Delete(file); } catch { }
                    }
                }
            }
            catch { }
        }
    }
}
