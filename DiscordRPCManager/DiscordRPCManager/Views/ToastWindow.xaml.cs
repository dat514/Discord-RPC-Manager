using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace DiscordRPCManager.Views
{
    public partial class ToastWindow : Window
    {
        public ToastWindow(string message)
        {
            InitializeComponent();
            MessageText.Text = message;

            var desktopWorkingArea = SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width - 10;
            this.Top = desktopWorkingArea.Bottom - this.Height - 10;

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(4) };
            timer.Tick += (s, e) => 
            {
                timer.Stop();
                CloseWithAnimation();
            };
            timer.Start();
        }

        private void CloseWithAnimation()
        {
            var anim = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.3));
            anim.Completed += (s, e) => this.Close();
            this.BeginAnimation(OpacityProperty, anim);
        }
    }
}
