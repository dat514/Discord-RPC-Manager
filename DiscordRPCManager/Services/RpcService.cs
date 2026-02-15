using DiscordRPC;
using DiscordRPCManager.Models;
using System.Diagnostics;
using System.Windows;
using System;

namespace DiscordRPCManager.Services
{
    public class RpcService : IDisposable
    {
        private DiscordRpcClient _client;

        public void Start(RpcProfile profile)
        {
            if (profile == null)
            {
                System.Windows.MessageBox.Show("No profile selected.", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(profile.ClientId))
            {
                System.Windows.MessageBox.Show("Application ID is empty.", "Validation Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            Stop();

            if (!string.IsNullOrWhiteSpace(profile.TargetExePath) && !IsExeRunning(profile.TargetExePath))
            {
                var result = System.Windows.MessageBox.Show($"Target executable '{System.IO.Path.GetFileName(profile.TargetExePath)}' is not running. Start anyway?", 
                    "Target Not Found", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
                
                if (result == System.Windows.MessageBoxResult.No) return;
            }

            try 
            {
                _client = new DiscordRpcClient(profile.ClientId);
                _client.OnReady += (sender, e) => { Console.WriteLine($"Received Ready from user {e.User.Username}"); };
                _client.Initialize();

                var presence = new RichPresence()
                {
                    Details = profile.Details,
                    State = profile.State,
                    Assets = new Assets()
                    {
                        LargeImageKey = profile.LargeImageKey,
                        SmallImageKey = profile.SmallImageKey
                    }
                };

                if (profile.TimestampMode == 0) 
                {
                    presence.Timestamps = Timestamps.Now;
                }
                else if (profile.TimestampMode == 1 && profile.CustomTimestampValue.HasValue)
                {
                    long offsetSeconds = profile.CustomTimestampValue.Value;
                    
                    switch (profile.TimestampUnit)
                    {
                        case 1: offsetSeconds *= 60; break;       
                        case 2: offsetSeconds *= 3600; break;     
                        case 3: offsetSeconds *= 86400; break;    
                    }

                    presence.Timestamps = new Timestamps { Start = DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(offsetSeconds)) };
                }

                _client.SetPresence(presence);

                profile.IsRunning = true;
                StartWatchdog(profile);
                StartKeepAlive();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to start RPC: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                Stop(); 
            }
        }

        private System.Windows.Threading.DispatcherTimer _watchdogTimer;
        private System.Windows.Threading.DispatcherTimer _keepAliveTimer;
        private RpcProfile _activeProfile;

        private void StartWatchdog(RpcProfile profile)
        {
            if (string.IsNullOrWhiteSpace(profile.TargetExePath)) return;

            _activeProfile = profile;
            _watchdogTimer = new System.Windows.Threading.DispatcherTimer();
            _watchdogTimer.Interval = TimeSpan.FromSeconds(2);
            _watchdogTimer.Tick += WatchdogTimer_Tick;
            _watchdogTimer.Start();
        }

        private void StartKeepAlive()
        {
            _keepAliveTimer = new System.Windows.Threading.DispatcherTimer();
            _keepAliveTimer.Interval = TimeSpan.FromSeconds(15); 
            _keepAliveTimer.Tick += (s, e) => 
            {
                if (_client != null && _client.IsInitialized && _client.CurrentPresence != null)
                {
                    _client.SetPresence(_client.CurrentPresence);
                }
            };
            _keepAliveTimer.Start();
        }

        private void WatchdogTimer_Tick(object sender, EventArgs e)
        {
            if (_activeProfile != null && !IsExeRunning(_activeProfile.TargetExePath))
            {
                Stop();
            }
        }

        public void Stop()
        {
            if (_watchdogTimer != null)
            {
                _watchdogTimer.Stop();
                _watchdogTimer = null;
            }

            if (_keepAliveTimer != null)
            {
                _keepAliveTimer.Stop();
                _keepAliveTimer = null;
            }

            if (_client != null)
            {
                _client.Dispose();
                _client = null;
            }
            
            if (_activeProfile != null)
            {
                _activeProfile.IsRunning = false;
                _activeProfile = null;
            }
            
        }

        private bool IsExeRunning(string path)
        {
            try
            {
                var exeName = System.IO.Path.GetFileNameWithoutExtension(path);
                return Process.GetProcessesByName(exeName).Length > 0;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}