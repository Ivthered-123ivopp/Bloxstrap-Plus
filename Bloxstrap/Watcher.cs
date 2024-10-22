using Bloxstrap.Integrations;
using Bloxstrap.Models;

namespace Bloxstrap
{
    public class Watcher : IDisposable
    {
        private readonly InterProcessLock _lock = new("Watcher");

        private readonly WatcherData? _watcherData;
        
        private readonly NotifyIconWrapper? _notifyIcon;

        public readonly ActivityWatcher? ActivityWatcher;

        public readonly DiscordRichPresence? RichPresence;

        public Watcher()
        {
            const string LOG_IDENT = "Watcher";

            // Allow multiple watchers if multi instance launching is enabled (there are better ways of doing this but this is fine for now)
            if (!_lock.IsAcquired)
            {
                if (!App.Settings.Prop.MultiInstanceLaunching)
                {
                    App.Logger.WriteLine(LOG_IDENT, "Watcher instance already exists");
                    return;
                }
                else
                {
                    App.Logger.WriteLine(LOG_IDENT, "Launching new Watcher instance");
                }
            }

            string? watcherDataArg = App.LaunchSettings.WatcherFlag.Data;

#if DEBUG
            if (String.IsNullOrEmpty(watcherDataArg))
            {
                string path = Path.Combine(Paths.Roblox, "Player", "RobloxPlayerBeta.exe");
                using var gameClientProcess = Process.Start(path);

                _watcherData = new() { ProcessId = gameClientProcess.Id };
            }
#else
            if (String.IsNullOrEmpty(watcherDataArg))
                throw new Exception("Watcher data not specified");
#endif

            if (!String.IsNullOrEmpty(watcherDataArg))
                _watcherData = JsonSerializer.Deserialize<WatcherData>(Encoding.UTF8.GetString(Convert.FromBase64String(watcherDataArg)));

            if (_watcherData is null)
                throw new Exception("Watcher data is invalid");

            if (App.Settings.Prop.EnableActivityTracking)
            {
                ActivityWatcher = new(_watcherData.LogFile);

                ActivityWatcher.OnGameJoin += delegate
                {
                    Utilities.ApplyTeleportFix();
                };

                if (App.Settings.Prop.UseDisableAppPatch)
                {
                    ActivityWatcher.OnAppClose += delegate
                    {
                        App.Logger.WriteLine(LOG_IDENT, "Received desktop app exit, closing Roblox");
                        using var process = Process.GetProcessById(_watcherData.ProcessId);
                        process.CloseMainWindow();
                    };
                }

                // Only run rich presence for first watcher
                if (App.Settings.Prop.UseDiscordRichPresence && _lock.IsAcquired)
                    RichPresence = new(ActivityWatcher);
            }

            _notifyIcon = new(this);
        }

        public void KillRobloxProcess() => CloseProcess(_watcherData!.ProcessId, true);

        public void CloseProcess(int pid, bool force = false)
        {
            const string LOG_IDENT = "Watcher::CloseProcess";

            try
            {
                using var process = Process.GetProcessById(pid);

                App.Logger.WriteLine(LOG_IDENT, $"Killing process '{process.ProcessName}' (pid={pid}, force={force})");

                if (process.HasExited)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"PID {pid} has already exited");
                    return;
                }

                if (force)
                    process.Kill();
                else
                    process.CloseMainWindow();
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, $"PID {pid} could not be closed");
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        public async Task Run()
        {
            if ((!_lock.IsAcquired && !App.Settings.Prop.MultiInstanceLaunching) || _watcherData is null)
                return;

            ActivityWatcher?.Start();

            while (Utilities.GetProcessesSafe().Any(x => x.Id == _watcherData.ProcessId))
                await Task.Delay(1000);

            if (_watcherData.AutoclosePids is not null)
            {
                foreach (int pid in _watcherData.AutoclosePids)
                    CloseProcess(pid);
            }

            if (App.LaunchSettings.TestModeFlag.Active && _lock.IsAcquired)
                Process.Start(Paths.Process, "-settings -testmode");
        }

        public void Dispose()
        {
            App.Logger.WriteLine("Watcher::Dispose", "Disposing Watcher");

            _notifyIcon?.Dispose();
            if (_lock.IsAcquired)
                RichPresence?.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
