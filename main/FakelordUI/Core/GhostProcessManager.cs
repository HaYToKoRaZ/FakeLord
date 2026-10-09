using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace FakelordUI.Core
{
    public class GhostProcessManager
    {
        private static Process? _currentProcess;
        private static Process? _companionProcess;
        public static string? CurrentRunningGame { get; private set; }

        public static string DataDirectory => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
        private static string RunnerDirectory => Path.Combine(DataDirectory, "Runners");
        private static string MainMasterExePath => Path.Combine(RunnerDirectory, "HaYTooL.exe");

        // Uygulama açılışında çağrılır: Data\Runners klasörünü temizler, sadece HaYTooL.exe'yi tutar
        public static void InitializeRunnersDirectory()
        {
            try
            {
                Directory.CreateDirectory(DataDirectory);
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
                    string templatePath = Path.Combine(DataDirectory, "Runners", "HaYTooL.exe");
                    if (!File.Exists(templatePath))
                    {
                        templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Runners", "HaYTooL.exe");
                    }
                    if (!File.Exists(templatePath))
                    {
                        templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RunnersTemplate", "HaYTooL.exe");
                    }

                    if (File.Exists(templatePath) && !templatePath.Equals(MainMasterExePath, StringComparison.OrdinalIgnoreCase))
                    {
                        File.Copy(templatePath, MainMasterExePath, true);
                    }
                    else if (!File.Exists(MainMasterExePath))
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

        public static event Action? GameStarted;
        public static event Action? GameExited;

        public static bool StartGame(string exeName, out string error)
        {
            return StartGame(exeName, Path.GetFileNameWithoutExtension(exeName), "", out error);
        }

        public static bool StartGame(string exeName, string title, string imageUrl, out string error)
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
                else if (normalizedExe.Equals("rdr2.exe", StringComparison.OrdinalIgnoreCase) ||
                         normalizedExe.Equals("playrdr2.exe", StringComparison.OrdinalIgnoreCase))
                {
                    normalizedExe = Path.Combine("red dead redemption 2", "rdr2.exe");
                }
                else if (normalizedExe.Equals("lol.exe", StringComparison.OrdinalIgnoreCase) ||
                         normalizedExe.Equals("leagueclient.exe", StringComparison.OrdinalIgnoreCase) ||
                         normalizedExe.Equals("leagueclientux.exe", StringComparison.OrdinalIgnoreCase) ||
                         normalizedExe.Contains("garenalol", StringComparison.OrdinalIgnoreCase) ||
                         normalizedExe.Equals("league of legends.exe", StringComparison.OrdinalIgnoreCase))
                {
                    normalizedExe = "league of legends.exe";
                }
                else if (normalizedExe.Equals("cs2.exe", StringComparison.OrdinalIgnoreCase))
                {
                    normalizedExe = Path.Combine("win64", "cs2.exe");
                }
                else if (normalizedExe.Equals("eldenring.exe", StringComparison.OrdinalIgnoreCase))
                {
                    normalizedExe = Path.Combine("game", "eldenring.exe");
                }
                else if (normalizedExe.Equals("r5apex.exe", StringComparison.OrdinalIgnoreCase))
                {
                    normalizedExe = Path.Combine("apex", "r5apex.exe");
                }
                else if (normalizedExe.Equals("minecraft.exe", StringComparison.OrdinalIgnoreCase))
                {
                    normalizedExe = Path.Combine("content", "minecraft.exe");
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

                    // League of Legends için hem root hem game/ alt klasörünü hazırla (Discord tespiti için)
                    if (cleanRelativePath.Equals("league of legends.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            string gameSubDir = Path.Combine(RunnerDirectory, "game");
                            Directory.CreateDirectory(gameSubDir);
                            File.Copy(MainMasterExePath, Path.Combine(gameSubDir, "league of legends.exe"), true);
                            File.Copy(MainMasterExePath, Path.Combine(RunnerDirectory, "LeagueClientUx.exe"), true);
                        }
                        catch { }
                    }
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

                string safeTitle = (string.IsNullOrWhiteSpace(title) ? Path.GetFileNameWithoutExtension(exeName) : title).Replace("\"", "\\\"");
                string safeExe = Path.GetFileName(exeName).Replace("\"", "\\\"");
                string safeImage = (imageUrl ?? "").Replace("\"", "\\\"");

                // LoL ve Valorant için yerel simge dosyasını hazırla ve HaYTooL'a tam disk yolunu ilet
                if (safeImage.Contains("lol", StringComparison.OrdinalIgnoreCase) || safeTitle.Contains("League of Legends", StringComparison.OrdinalIgnoreCase))
                {
                    string lolDisk = Path.Combine(DataDirectory, "lol.png");
                    if (!File.Exists(lolDisk)) lolDisk = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lol.png");
                    
                    try
                    {
                        string runnerLol = Path.Combine(RunnerDirectory, "lol.png");
                        if (File.Exists(lolDisk)) File.Copy(lolDisk, runnerLol, true);
                        safeImage = File.Exists(runnerLol) ? runnerLol : (File.Exists(lolDisk) ? lolDisk : "lol.png");
                    }
                    catch { }
                }
                else if (safeImage.Contains("valorant", StringComparison.OrdinalIgnoreCase) || safeTitle.Contains("VALORANT", StringComparison.OrdinalIgnoreCase))
                {
                    string valoDisk = Path.Combine(DataDirectory, "valorant.png");
                    if (!File.Exists(valoDisk)) valoDisk = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "valorant.png");
                    try
                    {
                        string runnerValo = Path.Combine(RunnerDirectory, "valorant.png");
                        if (File.Exists(valoDisk)) File.Copy(valoDisk, runnerValo, true);
                        safeImage = File.Exists(runnerValo) ? runnerValo : (File.Exists(valoDisk) ? valoDisk : "valorant.png");
                    }
                    catch { }
                }

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = targetExePath,
                    WorkingDirectory = targetDir ?? RunnerDirectory,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    Arguments = $"--title \"{safeTitle}\" --exe \"{safeExe}\" --image \"{safeImage}\"",
                    WindowStyle = ProcessWindowStyle.Normal
                };

                _currentProcess = Process.Start(psi);
                if (_currentProcess != null)
                {
                    _currentProcess.EnableRaisingEvents = true;
                    _currentProcess.Exited += (s, e) =>
                    {
                        _currentProcess = null;
                        CurrentRunningGame = null;
                        if (_companionProcess != null && !_companionProcess.HasExited)
                        {
                            try { _companionProcess.Kill(); _companionProcess.Dispose(); } catch { }
                        }
                        _companionProcess = null;
                        CleanGameCopies();
                        GameExited?.Invoke();
                    };
                }

                // League of Legends için LeagueClientUx.exe eşlikçi sürecini de başlat (Discord tespiti için)
                if (cleanRelativePath.Equals("league of legends.exe", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        string companionPath = Path.Combine(RunnerDirectory, "LeagueClientUx.exe");
                        if (File.Exists(companionPath))
                        {
                            var compPsi = new ProcessStartInfo
                            {
                                FileName = companionPath,
                                WorkingDirectory = RunnerDirectory,
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                Arguments = $"--title \"{safeTitle}\" --exe \"LeagueClientUx.exe\" --image \"{safeImage}\"",
                                WindowStyle = ProcessWindowStyle.Hidden
                            };
                            _companionProcess = Process.Start(compPsi);
                        }
                    }
                    catch { }
                }

                CurrentRunningGame = cleanRelativePath;
                GameStarted?.Invoke();
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
            if (_companionProcess != null && !_companionProcess.HasExited)
            {
                try
                {
                    _companionProcess.Kill();
                    _companionProcess.Dispose();
                }
                catch { }
            }
            _companionProcess = null;

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
            GameExited?.Invoke();
        }

        private static void CleanGameCopies()
        {
            try
            {
                if (!Directory.Exists(RunnerDirectory)) return;

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
            }
            catch { }
        }
    }
}
