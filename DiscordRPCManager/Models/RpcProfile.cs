using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace DiscordRPCManager.Models
{
    public class RpcProfile : INotifyPropertyChanged
    {
        private string _name;
        private string _clientId;
        private string _targetExePath;
        private string _details;
        private string _state;
        private string _largeImageKey;
        private string _smallImageKey;
        private bool _isRunning;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string ClientId
        {
            get => _clientId;
            set { _clientId = value; OnPropertyChanged(); }
        }

        public string TargetExePath
        {
            get => _targetExePath;
            set { _targetExePath = value; OnPropertyChanged(); }
        }

        public string Details
        {
            get => _details;
            set { _details = value; OnPropertyChanged(); }
        }

        public string State
        {
            get => _state;
            set { _state = value; OnPropertyChanged(); }
        }

        public string LargeImageKey
        {
            get => _largeImageKey;
            set { _largeImageKey = value; OnPropertyChanged(); }
        }

        public string SmallImageKey
        {
            get => _smallImageKey;
            set { _smallImageKey = value; OnPropertyChanged(); }
        }

        private int _timestampMode;
        public int TimestampMode
        {
            get => _timestampMode;
            set { _timestampMode = value; OnPropertyChanged(); }
        }

        private long? _customTimestampValue;
        public long? CustomTimestampValue
        {
            get => _customTimestampValue;
            set { _customTimestampValue = value; OnPropertyChanged(); }
        }

        private int _timestampUnit;
        public int TimestampUnit
        {
            get => _timestampUnit;
            set { _timestampUnit = value; OnPropertyChanged(); }
        }

        [JsonIgnore]
        public bool IsRunning
        {
            get => _isRunning;
            set { _isRunning = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}