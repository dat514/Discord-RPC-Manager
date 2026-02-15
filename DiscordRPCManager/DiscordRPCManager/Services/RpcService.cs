using DiscordRPC;
using DiscordRPCManager.Models;
using System.Diagnostics;
using System.Windows;
using System;
using System.IO;

namespace DiscordRPCManager.Services
{
    public class RpcService : IDisposable
    {
        private DiscordRpcClient _client;
        private System.Windows.Threading.DispatcherTimer _watchdogTimer;
        private System.Windows.Threading.DispatcherTimer _keepAliveTimer;
        
        public RpcProfile ActiveProfile { get; private set; }
        public bool IsScanning { get; private set; }
        public int DetectionIntervalSeconds { get; set; } = 5;
        
        public event EventHandler<string> StatusChanged;
        public event EventHandler<string> ToastRequested;
        public event EventHandler<int> CountdownTick;

        private int _currentCountdown;

        public void Start(RpcProfile profile)
        {
            if (profile == null) return;
            
            Stop(); 

            ActiveProfile = profile;
            ActiveProfile.IsRunning = true;
            IsScanning = true;

            StartWatchdog();
            
            CheckAndToggleRpc();
        }

        private void StartWatchdog()
        {
            _watchdogTimer = new System.Windows.Threading.DispatcherTimer();
            _watchdogTimer.Interval = TimeSpan.FromSeconds(1);
            _watchdogTimer.Tick += WatchdogTimer_Tick;
            _currentCountdown = 0; 
            _watchdogTimer.Start();
        }

        private void WatchdogTimer_Tick(object sender, EventArgs e)
        {
            if (ActiveProfile == null) return;

            if (_currentCountdown > 0)
            {
                _currentCountdown--;
                CountdownTick?.Invoke(this, _currentCountdown);
                return;
            }

            CheckAndToggleRpc();
            
            _currentCountdown = DetectionIntervalSeconds;
            CountdownTick?.Invoke(this, _currentCountdown);
        }

        private void CheckAndToggleRpc()
        {
            if (ActiveProfile == null) return;

            bool exeRunning = IsExeRunning(ActiveProfile.TargetExePath);

            if (exeRunning)
            {
                if (_client == null || !_client.IsInitialized)
                {
                    InitializeRpc();
                    StatusChanged?.Invoke(this, $"Attached to {Path.GetFileName(ActiveProfile.TargetExePath)}");
                    ToastRequested?.Invoke(this, $"Running RPC for {ActiveProfile.Name}");
                }
            }
            else
            {
                if (_client != null)
                {
                    DeinitializeRpc();
                    StatusChanged?.Invoke(this, "Target closed. Scanning...");
                    ToastRequested?.Invoke(this, $"Target closed. Pausing RPC...");
                }
                else
                {
                    StatusChanged?.Invoke(this, "Scanning for target...");
                }
            }
        }

        private void InitializeRpc()
        {
            try 
            {
                if (_client != null) DeinitializeRpc();

                _client = new DiscordRpcClient(ActiveProfile.ClientId);
                _client.Initialize();

                var presence = new RichPresence()
                {
                    Details = ActiveProfile.Details,
                    State = ActiveProfile.State,
                    Assets = new Assets()
                    {
                        LargeImageKey = ActiveProfile.LargeImageKey,
                        SmallImageKey = ActiveProfile.SmallImageKey
                    }
                };

                // Timestamp Logic
                if (ActiveProfile.TimestampMode == 1 && ActiveProfile.CustomTimestampValue.HasValue)
                {
                    long offsetSeconds = ActiveProfile.CustomTimestampValue.Value;
                    
                    switch (ActiveProfile.TimestampUnit)
                    {
                        case 1: offsetSeconds *= 60; break;       
                        case 2: offsetSeconds *= 3600; break;     
                        case 3: offsetSeconds *= 86400; break;    
                    }

                    presence.Timestamps = new Timestamps { Start = DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(offsetSeconds)) };
                }
                else
                {
                    presence.Timestamps = Timestamps.Now;
                }

                _client.SetPresence(presence);
                StartKeepAlive();
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke(this, $"Error: {ex.Message}");
            }
        }

        private void DeinitializeRpc()
        {
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
        }

        private void StartKeepAlive()
        {
            if (_keepAliveTimer != null) return;

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

        public void Stop()
        {
            if (_watchdogTimer != null)
            {
                _watchdogTimer.Stop();
                _watchdogTimer = null;
            }

            DeinitializeRpc();
            
            if (ActiveProfile != null)
            {
                ActiveProfile.IsRunning = false;
                ActiveProfile = null;
            }
            IsScanning = false;
        }

        private bool IsExeRunning(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return true; 

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