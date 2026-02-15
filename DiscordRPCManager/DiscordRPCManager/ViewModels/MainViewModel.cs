using DiscordRPCManager.Data;
using DiscordRPCManager.Models;
using DiscordRPCManager.Services;
using DiscordRPCManager.Views;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace DiscordRPCManager.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ConfigService _configService;
        private readonly RpcService _rpcService;
        private readonly SettingsService _settingsService;
        
        public ObservableCollection<RpcProfile> Profiles { get; set; }

        private RpcProfile _selectedProfile;
        public RpcProfile SelectedProfile
        {
            get => _selectedProfile;
            set { _selectedProfile = value; OnPropertyChanged(); }
        }

        private RpcProfile _runningProfile;
        public RpcProfile RunningProfile
        {
            get => _runningProfile;
            set { _runningProfile = value; OnPropertyChanged(); }
        }

        private bool _runAtStartup;
        public bool RunAtStartup
        {
            get => _runAtStartup;
            set { _runAtStartup = value; OnPropertyChanged(); }
        }

        private bool _autoStartRpc;
        public bool AutoStartRpc
        {
            get => _autoStartRpc;
            set { _autoStartRpc = value; OnPropertyChanged(); }
        }

        private int _detectionInterval;
        public int DetectionInterval
        {
            get => _detectionInterval;
            set 
            { 
                _detectionInterval = value; 
                if (_rpcService != null) _rpcService.DetectionIntervalSeconds = value;
                OnPropertyChanged(); 
            }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        private int _nextCheckCountdown;
        public int NextCheckCountdown
        {
            get => _nextCheckCountdown;
            set { _nextCheckCountdown = value; OnPropertyChanged(); }
        }

        private bool _isSettingsOpen;
        public bool IsSettingsOpen
        {
            get => _isSettingsOpen;
            set { _isSettingsOpen = value; OnPropertyChanged(); }
        }

        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand CloneCommand { get; }
        public ICommand BrowseExeCommand { get; }
        public ICommand ToggleSettingsCommand { get; }
        public ICommand SaveSettingsCommand { get; }


        public MainViewModel()
        {
            _configService = new ConfigService();
            _rpcService = new RpcService();
            _settingsService = new SettingsService();

            _rpcService.StatusChanged += (s, msg) => StatusMessage = msg;
            _rpcService.CountdownTick += (s, val) => NextCheckCountdown = val;
            _rpcService.ToastRequested += RpcService_ToastRequested;

            Profiles = new ObservableCollection<RpcProfile>(_configService.Load());
            
            var config = _settingsService.Load();
            RunAtStartup = config.RunAtStartup;
            AutoStartRpc = config.AutoStartRpc;
            DetectionInterval = config.DetectionIntervalSeconds;

            if (Profiles.Count > 0)
                SelectedProfile = Profiles[0];
            
            if (AutoStartRpc && !string.IsNullOrEmpty(config.LastProfileId))
            {
                var lastProfile = Profiles.FirstOrDefault(p => p.Name == config.LastProfileId);
                if (lastProfile != null)
                {
                    StartRpc(lastProfile);
                }
            }

            AddCommand = new RelayCommand(AddProfile);
            DeleteCommand = new RelayCommand(DeleteProfile, CanDeleteProfile);
            SaveCommand = new RelayCommand(SaveProfiles);
            StartCommand = new RelayCommand(StartRpc, CanStartRpc);
            StopCommand = new RelayCommand(StopRpc, CanStopRpc);
            CloneCommand = new RelayCommand(CloneProfile, CanCloneProfile);
            BrowseExeCommand = new RelayCommand(BrowseExe, CanBrowseExe);
            
            ToggleSettingsCommand = new RelayCommand(p => IsSettingsOpen = !IsSettingsOpen);
            SaveSettingsCommand = new RelayCommand(SaveSettings);
        }

        private void RpcService_ToastRequested(object sender, string message)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var toast = new ToastWindow(message);
                toast.Show();

                PlayNotificationSound();
            });
        }

        private System.Windows.Media.MediaPlayer _mediaPlayer;

        private void PlayNotificationSound()
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream("DiscordRPCManager.notification.wav"))
                {
                    if (stream != null)
                    {
                        using (var player = new System.Media.SoundPlayer(stream))
                        {
                            player.Play();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Sound error: {ex.Message}");
            }
        }

        private void SaveSettings(object parameter)
        {
            var config = new AppConfig
            {
                RunAtStartup = RunAtStartup,
                AutoStartRpc = AutoStartRpc,
                LastProfileId = _runningProfile?.Name ?? SelectedProfile?.Name,
                DetectionIntervalSeconds = DetectionInterval
            };
            _settingsService.Save(config);
            IsSettingsOpen = false;
        }

        private void AddProfile(object parameter)
        {
            var newProfile = new RpcProfile { Name = "New Profile" };
            Profiles.Add(newProfile);
            SelectedProfile = newProfile;
        }

        private bool CanDeleteProfile(object parameter) => SelectedProfile != null;

        private void DeleteProfile(object parameter)
        {
            if (SelectedProfile == null) return;

            if (System.Windows.MessageBox.Show($"Are you sure you want to delete '{SelectedProfile.Name}'?", "Confirm Delete", 
                System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning) == System.Windows.MessageBoxResult.Yes)
            {
                if (SelectedProfile == _runningProfile)
                {
                    StopRpc(null);
                }

                Profiles.Remove(SelectedProfile);
                SelectedProfile = null;
                SaveProfiles(null);
            }
        }

        private void SaveProfiles(object parameter)
        {
            _configService.Save(Profiles.ToList());
            System.Windows.MessageBox.Show("Profiles saved successfully!", "Success", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        private bool CanStartRpc(object parameter)
        {
            return SelectedProfile != null && 
                   !string.IsNullOrWhiteSpace(SelectedProfile.ClientId) &&
                   !_rpcService.IsScanning; 
        }

        private void StartRpc(object parameter)
        {
            var profileToStart = parameter as RpcProfile ?? SelectedProfile;
            if (profileToStart == null) return;

            if (_runningProfile != null && _runningProfile != profileToStart)
            {
                StopRpc(null);
            }

            RunningProfile = profileToStart;
            _rpcService.Start(RunningProfile);
            
            SaveSettings(null);
            CommandManager.InvalidateRequerySuggested();
        }

        private bool CanStopRpc(object parameter)
        {
            return _rpcService.IsScanning;
        }

        private void StopRpc(object parameter)
        {
            _rpcService.Stop();
            RunningProfile = null;
            StatusMessage = "Stopped";
            NextCheckCountdown = 0;
            CommandManager.InvalidateRequerySuggested();
        }

        private bool CanCloneProfile(object parameter) => SelectedProfile != null;

        private void CloneProfile(object parameter)
        {
            if (SelectedProfile == null) return;

            var clone = new RpcProfile
            {
                Name = $"{SelectedProfile.Name} (Copy)",
                ClientId = SelectedProfile.ClientId,
                Details = SelectedProfile.Details,
                State = SelectedProfile.State,
                LargeImageKey = SelectedProfile.LargeImageKey,
                SmallImageKey = SelectedProfile.SmallImageKey,
                TargetExePath = SelectedProfile.TargetExePath,
                TimestampMode = SelectedProfile.TimestampMode,
                CustomTimestampValue = SelectedProfile.CustomTimestampValue,
                TimestampUnit = SelectedProfile.TimestampUnit
            };

            Profiles.Add(clone);
            SelectedProfile = clone;
            SaveProfiles(null);
        }

        private bool CanBrowseExe(object parameter) => SelectedProfile != null;

        private void BrowseExe(object parameter)
        {
            if (SelectedProfile == null) return;

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Executable (*.exe)|*.exe"
            };

            if (dlg.ShowDialog() == true)
            {
                SelectedProfile.TargetExePath = dlg.FileName;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
