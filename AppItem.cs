using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SysPilot.Models
{
    public class AppItem : INotifyPropertyChanged
    {
        private string _executablePath = string.Empty;
        private string _statusText = "Launch";

        public string Name { get; set; } = string.Empty;

        public string ExecutablePath
        {
            get => _executablePath;
            set
            {
                if (_executablePath != value)
                {
                    _executablePath = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Arguments { get; set; } = string.Empty;

        public string? IconPath { get; set; }

        public string? DownloadUrl { get; set; }

        public string DownloadExtension { get; set; } = ".zip";

        public string StatusText
        {
            get => _statusText;
            set
            {
                if (_statusText != value)
                {
                    _statusText = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString() => Name;
    }
}