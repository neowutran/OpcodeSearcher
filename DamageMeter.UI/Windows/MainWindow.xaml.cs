using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using DamageMeter.Sniffing;
using Tera.Game;
using Microsoft.Win32;

namespace DamageMeter.UI
{
    /// <summary>
    ///     Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly DispatcherTimer _dispatcherTimer;
        private bool _topMost = true;

        public MainWindow()
        {
            InitializeComponent();
            // Handler for exceptions in threads behind forms.
            TeraSniffer.Instance.Enabled = true;
            //TeraSniffer.Instance.Warning += PcapWarning;
            NetworkController.Instance.Connected += HandleConnected;
            NetworkController.Instance.GuildIconAction += InstanceOnGuildIconAction;
            Title = "Opcode finder V0";
            SystemEvents.SessionEnding += new SessionEndingEventHandler(SystemEvents_SessionEnding);
        }

        private void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            Exit();
        }


        private void InstanceOnGuildIconAction(Bitmap icon)
        {
            void ChangeUi(Bitmap bitmap)
            {
                //Icon = bitmap?.ToImageSource() ?? BasicTeraData.Instance.ImageDatabase.Icon;
                //NotifyIcon.Tray.Icon = bitmap?.GetIcon() ?? BasicTeraData.Instance.ImageDatabase.Tray;
            }

            Dispatcher.Invoke((NetworkController.GuildIconEvent) ChangeUi, icon);
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            Exit();
        }

        public void VerifyClose()
        {
            Close();
        }
        
        public void Exit()
        {
            _topMost = false;
            NetworkController.Instance.Exit();
        }

        public void Update(UiUpdateMessage nmessage)
        {
            void ChangeUi(UiUpdateMessage message)
            {
             
            }

            Dispatcher.Invoke((NetworkController.UpdateUiHandler) ChangeUi, nmessage);
        }


        public void HandleConnected(string serverName)
        {
            void ChangeTitle(string newServerName)
            {
                Title = newServerName;
            }

            Dispatcher.Invoke((ChangeTitle) ChangeTitle, serverName);
        }

        internal void StayTopMost()
        {
            if (!_topMost || !Topmost) {
                Debug.WriteLine("Not topmost");
                return;
            }
            foreach (Window window in System.Windows.Application.Current.Windows)
            {
                window.Topmost = false;
                window.Topmost = true;
            }
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            Top = 0;
            Left = 0;
        }


        private void ListEncounter_OnDropDownOpened(object sender, EventArgs e)
        {
            _topMost = false;
        }

        private void Close_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            VerifyClose();
        }

        private delegate void ChangeTitle(string servername);
    }

 
}