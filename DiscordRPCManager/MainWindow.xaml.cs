using DiscordRPCManager.ViewModels;
using System.Windows;
using System;

namespace DiscordRPCManager
{
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
            }
            base.OnStateChanged(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }
    }
}