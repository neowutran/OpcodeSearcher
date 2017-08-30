using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using DamageMeter.Sniffing;
using DamageMeter.UI.Annotations;
using Tera.Game;
using Microsoft.Win32;
using Tera;
using Tera.Game.Messages;
using Brushes = System.Drawing.Brushes;
using Color = System.Windows.Media.Color;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace DamageMeter.UI
{
    /// <summary>
    ///     Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private readonly DispatcherTimer _dispatcherTimer;
        private bool _topMost = true;


        private PacketViewModel _packetDetails;
        public PacketViewModel PacketDetails
        {
            get => _packetDetails;
            set
            {
                if (_packetDetails == value) return;
                _packetDetails = value;
                OnPropertyChanged(nameof(PacketDetails));
            }
        }

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
            OpcodeFinder.Instance.OpcodeFound += (opc) =>
            {
                OnPropertyChanged(nameof(Known));
                foreach (var packetViewModel in All.Where(x => x.Message.OpCode == opc)) { packetViewModel.RefreshName(); }
            };
            OpcodeFinder.Instance.NewMessage += (msg) => Dispatcher.Invoke(() => HandleNewMessage(msg));
            All.CollectionChanged += All_CollectionChanged;
            AllSw.ScrollChanged += AllSw_ScrollChanged;
            DataContext = this;
        }

        private void HandleNewMessage(ParsedMessage msg)
        {
            if (msg.Direction == MessageDirection.ServerToClient && ServerCb.IsChecked == false) return;
            if (msg.Direction == MessageDirection.ClientToServer && ClientCb.IsChecked == false) return;
            if (WhiteListedOpcodes.Count > 0 && !WhiteListedOpcodes.Contains(msg.OpCode)) return;
            if (BlackListedOpcodes.Contains(msg.OpCode)) return;
            if (SpamCb.IsChecked == true && All.Count > 0 && All.Last().Message.OpCode == msg.OpCode) return;
            All.Add(new PacketViewModel(msg));
        }

        private void AllSw_ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
        {
            if (AllSw.VerticalOffset == AllSw.ScrollableHeight) _bottom = true;
            else _bottom = false;
        }

        private bool _bottom;

        private void All_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_bottom) Dispatcher.Invoke(() => AllSw.ScrollToBottom());
        }


        public ObservableDictionary<ushort, OpcodeEnum> Known => new ObservableDictionary<ushort, OpcodeEnum>(OpcodeFinder.Instance.KnownOpcode);
        public ObservableCollection<PacketViewModel> All { get; set; } = new ObservableCollection<PacketViewModel>();
        public ObservableCollection<ushort> BlackListedOpcodes { get; set; } = new ObservableCollection<ushort>();
        public ObservableCollection<ushort> WhiteListedOpcodes { get; set; } = new ObservableCollection<ushort>();
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

            Dispatcher.Invoke((NetworkController.GuildIconEvent)ChangeUi, icon);
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

            Dispatcher.Invoke((NetworkController.UpdateUiHandler)ChangeUi, nmessage);
        }


        public void HandleConnected(string serverName)
        {
            void ChangeTitle(string newServerName)
            {
                Title = newServerName;
            }

            Dispatcher.Invoke((ChangeTitle)ChangeTitle, serverName);
        }

        internal void StayTopMost()
        {
            if (!_topMost || !Topmost)
            {
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SelectPacketByDrag(object sender, MouseEventArgs mouseEventArgs)
        {
            if (mouseEventArgs.LeftButton == MouseButtonState.Pressed)
            {
                var s = ((Grid)sender);
                PacketDetails = s.DataContext as PacketViewModel;
                foreach (var packetViewModel in All) { packetViewModel.IsSelected = packetViewModel.Message == PacketDetails.Message; }
                OpcodeToWhitelist.Text = PacketDetails.Message.OpCode.ToString();
                OpcodeToBlacklist.Text = PacketDetails.Message.OpCode.ToString();
            }
        }
        private void SelectPacketByClick(object sender, MouseButtonEventArgs e)
        {
            var s = ((Grid)sender);
            PacketDetails = s.DataContext as PacketViewModel;
            foreach (var packetViewModel in All) { packetViewModel.IsSelected = packetViewModel.Message == PacketDetails.Message; }
            OpcodeToWhitelist.Text = PacketDetails.Message.OpCode.ToString();
            OpcodeToBlacklist.Text = PacketDetails.Message.OpCode.ToString();

        }
        private string FormatData(ArraySegment<byte> selectedPacketPayload)
        {
            var a = selectedPacketPayload.ToArray();
            var s = BitConverter.ToString(a).Replace("-", string.Empty);
            var sb = new StringBuilder();
            var i = 0;
            var count = 0;
            while (true)
            {
                if (s.Length > i + 8)
                {
                    sb.Append(s.Substring(i, 8));
                    sb.Append((count + 1) % 4 == 0 ? "\n" : " ");
                }
                else
                {
                    sb.Append(s.Substring(i));
                    break;
                }
                i += 8;
                count++;
            }
            return sb.ToString();

        }

        private void HexSwChanged(object sender, ScrollChangedEventArgs e)
        {
            var s = sender as ScrollViewer;
            if(s.Name == nameof(HexSw)) TextSw.ScrollToVerticalOffset(HexSw.VerticalOffset);
            else HexSw.ScrollToVerticalOffset(TextSw.VerticalOffset);
        }

        private void ChunkMouseEnter(object sender, MouseEventArgs e)
        {
            var s = sender as Border;
            var dc = (string) s.DataContext;
        }

        private void ClearAll(object sender, RoutedEventArgs e)
        {
            All.Clear();
        }

        private void Dump(object sender, RoutedEventArgs e)
        {
            var lines = new List<string>();
            foreach (KeyValuePair<ushort, OpcodeEnum> keyVal in Known)
            {
                var s = $"{keyVal.Value} = {keyVal.Key}";
                lines.Add(s);
            }
            File.WriteAllLines($"{Environment.CurrentDirectory}/opcodes {DateTime.Now.ToString().Replace('/','-').Replace(':', '-')}.txt", lines);
        }

        private void Load(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog { Filter = "Supported Formats (*.TeraLog)|*.TeraLog" };
            if (openFileDialog.ShowDialog() == false) return;
            NetworkController.Instance.FileName = openFileDialog.FileName;
        }

        private void RemoveBlacklistedOpcode(object sender, RoutedEventArgs e)
        {
            var s = (System.Windows.Controls.Button) sender;
            BlackListedOpcodes.Remove((ushort) s.DataContext);
        }

        private void RemoveWhitelistedOpcode(object sender, RoutedEventArgs e)
        {
            var s = (System.Windows.Controls.Button)sender;
            WhiteListedOpcodes.Remove((ushort)s.DataContext);
        }

        private void AddBlackListedOpcode(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(OpcodeToBlacklist.Text)) return;
            if (!ushort.TryParse(OpcodeToBlacklist.Text, out ushort result)) return;
            if (BlackListedOpcodes.Contains(result)) return;
            BlackListedOpcodes.Add(result);
            OpcodeToBlacklist.Text = "";

        }

        private void AddWhiteListedOpcode(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(OpcodeToWhitelist.Text)) return;
            if (!ushort.TryParse(OpcodeToWhitelist.Text, out ushort result)) return;
            if (WhiteListedOpcodes.Contains(result)) return;
            WhiteListedOpcodes.Add(result);
            OpcodeToWhitelist.Text = "";
        }
    }
    public class DirectionToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var v = (MessageDirection)value;
            var c = Colors.Gray;
            if (v == MessageDirection.ServerToClient) c = Color.FromArgb(0xcc, Colors.DodgerBlue.R, Colors.DodgerBlue.G, Colors.DodgerBlue.B);
            if (v == MessageDirection.ClientToServer) c = Color.FromArgb(0xcc, Colors.DarkOrange.R, Colors.DarkOrange.G, Colors.DarkOrange.B);
            return new SolidColorBrush(c);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class OpcodeNameConv : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var opn = (string)value;
            if (opn.Length == 4 && opn[1] != '_')
            {
                if (OpcodeFinder.Instance.KnownOpcode.TryGetValue(System.Convert.ToUInt16(opn, 16), out OpcodeEnum opc))
                {
                    return opc.ToString();
                }
                return opn;
            }
            return opn;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class HexPayloadConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var arr = (ArraySegment<byte>)value;
            var a = arr.ToArray();
            return BitConverter.ToString(a).Replace("-", string.Empty);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class TextPayloadConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var arr = (ArraySegment<byte>)value;
            var a = arr.ToArray();
            var sb = new StringBuilder();
            for (int i = 0; i < a.Length; i++)
            {
                var c = (char)a[i];
                if (c > 0x1f && c < 0x80) sb.Append(c);
                else sb.Append("⋅");
                if ((i + 1) % 16 == 0) sb.Append("\n");
            }
            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class BoolToVisibleHidden : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var v = (bool) value;
            return v ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PacketViewModel : INotifyPropertyChanged
    {
        public ParsedMessage Message { get; }
        private bool _isSelected = false;
        public List<List<string>> RowsHex => ParseDataHex(Message.Payload);
        public List<List<string>> RowsText => ParseDataText(Message.Payload);

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        private List<List<string>> ParseDataHex(ArraySegment<byte> p)
        {
            var a = p.ToArray();
            var s = BitConverter.ToString(a).Replace("-", string.Empty);
            var rows = (s.Length / 32) + 1;
            var result = new List<List<string>>();
            for (int i = 0; i < rows; i++)
            {
                var row = new List<string>();
                for (int j = 0; j < 32; j += 8)
                {
                    if (j + 32 * i >= s.Length) continue;

                    var chunk = s.Substring(j + (32 * i)).Length <= 8 ? s.Substring(j + (32 * i)) : s.Substring(j + (32 * i), 8);
                    row.Add(chunk);
                }
                for (int j = row.Count; j < 4; j++)
                {
                    row.Add("");
                }
                result.Add(row);
            }
            return result;
        }
        private List<List<string>> ParseDataText(ArraySegment<byte> p)
        {
            var a = p.ToArray();
            var sb = new StringBuilder();
            for (int i = 0; i < a.Length; i++)
            {
                var c = (char)a[i];
                if (c > 0x21 && c < 0x80) sb.Append(c);
                else sb.Append("⋅");
                //if ((i + 1) % 16 == 0) sb.Append("\n");
            }
            var s = sb.ToString();

            var rows = (s.Length / 16) + 1;
            var result = new List<List<string>>();
            for (int i = 0; i < rows; i++)
            {
                var row = new List<string>();
                for (int j = 0; j < 16; j += 4)
                {
                    if (j + 16 * i >= s.Length) continue;

                    var chunk = s.Substring(j + (16 * i)).Length <= 4 ? s.Substring(j + (16 * i)) : s.Substring(j + (16 * i), 4);
                    row.Add(chunk);
                }
                for (int j = row.Count; j < 4; j++)
                {
                    row.Add("");
                }
                result.Add(row);
            }
            return result;
        }

        public PacketViewModel(ParsedMessage message)
        {
            Message = message;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void RefreshName()
        {
            OnPropertyChanged(nameof(Message));
        }
    }
}