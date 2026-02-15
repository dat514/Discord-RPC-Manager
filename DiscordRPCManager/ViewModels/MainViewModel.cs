using DiscordRPCManager.Data;
using DiscordRPCManager.Models;
using DiscordRPCManager.Services;
using Microsoft.Win32;
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

            Profiles = new ObservableCollection<RpcProfile>(_configService.Load());
            
            var config = _settingsService.Load();
            RunAtStartup = config.RunAtStartup;
            AutoStartRpc = config.AutoStartRpc;

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

        private void SaveSettings(object parameter)
        {
            var config = new AppConfig
            {
                RunAtStartup = RunAtStartup,
                AutoStartRpc = AutoStartRpc,
                LastProfileId = _runningProfile?.Name ?? SelectedProfile?.Name
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
                   (SelectedProfile != _runningProfile || !_runningProfile.IsRunning);
        }

        private void StartRpc(object parameter)
        {
            if (SelectedProfile == null) return;

            if (_runningProfile != null && _runningProfile != SelectedProfile)
            {
                StopRpc(null);
            }

            _rpcService.Start(SelectedProfile);
            
            if (SelectedProfile.IsRunning)
            {
                _runningProfile = SelectedProfile;
                SaveSettings(null); 
            }
            
            CommandManager.InvalidateRequerySuggested();
        }

        private bool CanStopRpc(object parameter)
        {
            return SelectedProfile != null && SelectedProfile.IsRunning;
        }

        private void StopRpc(object parameter)
        {
            _rpcService.Stop();
            if (_runningProfile != null)
            {
                _runningProfile.IsRunning = false;
                _runningProfile = null;
            }
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

            OpenFileDialog dlg = new OpenFileDialog
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
