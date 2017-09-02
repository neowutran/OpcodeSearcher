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
using OpcodeId = System.UInt16;

namespace DamageMeter.UI
{
    /// <summary>
    ///     Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
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
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;
            Title = "Opcode finder V0";
            SystemEvents.SessionEnding += new SessionEndingEventHandler(SystemEvents_SessionEnding);
            NetworkController.Instance.TickUpdated += (msg) => Dispatcher.BeginInvoke(new Action(() => HandleNewMessage(msg)), DispatcherPriority.Background);
            All.CollectionChanged += All_CollectionChanged;
            AllSw.ScrollChanged += AllSw_ScrollChanged;
            DataContext = this;
            //((ItemsControl)KnownSw.Content).ItemsSource = Known;

        }

        private int _count;
        private int _queued;
        public int Queued
        {
            get => _queued;
            set
            {
                _queued = value;
                OnPropertyChanged(nameof(Queued));
            }
        }

        private void HandleNewMessage(Tuple<List<ParsedMessage>, Dictionary<OpcodeId, OpcodeEnum>, int> update)
        {
            Queued = update.Item3;
            if (update.Item2.Count != 0)
            {
                foreach (var opcode in update.Item2)
                {
                    Dispatcher.Invoke(() =>
                        Known.Add(opcode.Key, opcode.Value)
                    );
                    OpcodeNameConv.Instance.Known.Add(opcode.Key, opcode.Value);
                    foreach (var packetViewModel in All.Where(x => x.Message.OpCode == opcode.Key))
                    {
                        packetViewModel.RefreshName();
                    }
                }
                KnownSw.ScrollToBottom();
            }

            foreach (var msg in update.Item1)
            {
                _count++;
                if (msg.Direction == MessageDirection.ServerToClient && ServerCb.IsChecked == false) return;
                if (msg.Direction == MessageDirection.ClientToServer && ClientCb.IsChecked == false) return;
                if (WhiteListedOpcodes.Count > 0 && !WhiteListedOpcodes.Contains(msg.OpCode)) return;
                if (BlackListedOpcodes.Contains(msg.OpCode)) return;
                if (SpamCb.IsChecked == true && All.Count > 0 && All.Last().Message.OpCode == msg.OpCode) return;
                if (_sizeFilter != -1)
                {
                    if (msg.Payload.Count != _sizeFilter) return;
                }
                var vm = new PacketViewModel(msg, _count);
                All.Add(vm);
                if (SearchList.Count > 0)
                {
                    if (SearchList[0].Message.OpCode == msg.OpCode) UpdateSearch(msg.OpCode.ToString(), false); //could be performance intensive
                }
            }
        }

        private void AllSw_ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
        {
            if (AllSw.VerticalOffset == AllSw.ScrollableHeight)
            {
                _bottom = true;
                NewMessagesBelow = false;
            }
            else _bottom = false;
        }

        private bool _bottom;
        private bool _newMessagesBelow;
        public bool NewMessagesBelow
        {
            get => _newMessagesBelow;
            set
            {
                if (_newMessagesBelow == value) return;
                _newMessagesBelow = value;
                OnPropertyChanged(nameof(NewMessagesBelow));
            }
        }
        private void All_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_bottom) Dispatcher.Invoke(() => AllSw.ScrollToBottom());
            else
            {
                NewMessagesBelow = true;
            }
        }
        public ObservableDictionary<ushort, OpcodeEnum> Known { get; set; } = new ObservableDictionary<ushort, OpcodeEnum>();
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
            if (s.Name == nameof(HexSw)) TextSw.ScrollToVerticalOffset(HexSw.VerticalOffset);
            else HexSw.ScrollToVerticalOffset(TextSw.VerticalOffset);
        }

        private void ChunkMouseEnter(object sender, MouseEventArgs e)
        {
            var s = sender as Border;
            var dc = (string)s.DataContext;
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
            File.WriteAllLines($"{Environment.CurrentDirectory}/opcodes {DateTime.Now.ToString().Replace('/', '-').Replace(':', '-')}.txt", lines);
        }

        private void Load(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog { Filter = "Supported Formats (*.TeraLog)|*.TeraLog" };
            if (openFileDialog.ShowDialog() == false) return;
            NetworkController.Instance.LoadFileName = openFileDialog.FileName;
        }

        private void RemoveBlacklistedOpcode(object sender, RoutedEventArgs e)
        {
            var s = (System.Windows.Controls.Button)sender;
            BlackListedOpcodes.Remove((ushort)s.DataContext);
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

        private void Save(object sender, RoutedEventArgs e)
        {
            NetworkController.Instance.NeedToSave = true;
        }

        private void UIElement_OnMouseEnter(object sender, MouseEventArgs e)
        {
            var s = sender as FrameworkElement;
            var bvm = s.DataContext as ByteViewModel;
            bvm.IsHovered = true;
            int i = 0;
            i = HexIc.Items.IndexOf(bvm);
            if (i == -1) i = TextIc.Items.IndexOf(bvm);
            PacketDetails.RefreshData(i);
        }

        private void UIElement_OnMouseLeave(object sender, MouseEventArgs e)
        {
            var s = sender as FrameworkElement;
            var bvm = s.DataContext as ByteViewModel;
            bvm.IsHovered = false;

        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AllSw.ScrollToBottom();
        }

        private void LoadOpcode(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog { Filter = "Supported Formats, with / without '=' separator (*.txt)|*.txt" };
            if (openFileDialog.ShowDialog() == false) return;
            NetworkController.Instance.StrictCheck = false;
            NetworkController.Instance.LoadOpcodeCheck = openFileDialog.FileName;
        }

        private int _sizeFilter = -1;
        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var s = sender as System.Windows.Controls.TextBox;
            if (string.IsNullOrEmpty(s.Text))
            {
                _sizeFilter = -1;
                return;
            }
            try
            {
                _sizeFilter = Convert.ToInt32(s.Text);
            }
            catch (Exception exception)
            {
                _sizeFilter = -1;
                return;
            }
        }

        public List<PacketViewModel> SearchList { get; set; } = new List<PacketViewModel>();
        private void SearchBoxChanged(object sender, TextChangedEventArgs e)
        {
            var s = sender as System.Windows.Controls.TextBox;
            UpdateSearch(s.Text, true);
        }

        private void UpdateSearch(string q, bool bringIntoView)
        {
            SearchList.Clear();
            foreach (var packetViewModel in All)
            {
                //packetViewModel.IsSearched = true;
                packetViewModel.IsSearched = false;
            }
            if (string.IsNullOrEmpty(q))
            {
                foreach (var packetViewModel in All)
                {
                    //packetViewModel.IsSearched = true;
                    packetViewModel.IsSearched = false;
                }

                return;
            }
            try
            {
                var query = Convert.ToUInt16(q);
                //search by opcode
                foreach (var packetViewModel in All.Where(x => x.Message.OpCode == query))
                {
                    packetViewModel.IsSearched = true;
                    SearchList.Add(packetViewModel);
                }
                if (SearchList.Count != 0)
                {
                    var i = All.IndexOf(SearchList[0]);
                    if (bringIntoView)
                    {
                        var container = AllItemsControl.ItemContainerGenerator.ContainerFromItem(All[i]) as FrameworkElement;
                        container.BringIntoView();
                    }
                    foreach (var packetViewModel in All) { packetViewModel.IsSelected = packetViewModel == All[i]; }

                }
            }
            catch (Exception exception)
            {
                //search by opcodename

                OpcodeEnum opEnum;
                try
                {
                    opEnum = (OpcodeEnum)Enum.Parse(typeof(OpcodeEnum), q);
                }
                catch (Exception e1) { return; }


                foreach (var packetViewModel in All.Where(x => x.Message.OpCode == OpcodeFinder.Instance.GetOpcode(opEnum)))
                {
                    packetViewModel.IsSearched = true;
                    SearchList.Add(packetViewModel);
                }
                if (SearchList.Count != 0)
                {
                    var i = All.IndexOf(SearchList[0]);
                    if (bringIntoView)
                    {
                        var container = AllItemsControl.ItemContainerGenerator.ContainerFromItem(All[i]) as FrameworkElement;
                        container.BringIntoView();
                    }
                    foreach (var packetViewModel in All) { packetViewModel.IsSelected = packetViewModel == All[i]; }

                }
            }
            OnPropertyChanged(nameof(SearchList));

        }

        private int _currentSelectedItemIndex = 0;
        private void PreviousResult(object sender, RoutedEventArgs e)
        {
            if (SearchList.Count == 0) return;
            if (_currentSelectedItemIndex == 0) _currentSelectedItemIndex = SearchList.Count - 1;
            else _currentSelectedItemIndex--;
            var i = All.IndexOf(SearchList[_currentSelectedItemIndex]);
            var container = AllItemsControl.ItemContainerGenerator.ContainerFromItem(All[i]) as FrameworkElement;
            container.BringIntoView();
            foreach (var packetViewModel in All) { packetViewModel.IsSelected = packetViewModel == All[i]; }

        }
        private void NextResult(object sender, RoutedEventArgs e)
        {
            if (SearchList.Count == 0) return;
            if (_currentSelectedItemIndex == SearchList.Count - 1) _currentSelectedItemIndex = 0;
            else _currentSelectedItemIndex++;
            var i = All.IndexOf(SearchList[_currentSelectedItemIndex]);
            var container = AllItemsControl.ItemContainerGenerator.ContainerFromItem(All[i]) as FrameworkElement;
            container.BringIntoView();
            foreach (var packetViewModel in All) { packetViewModel.IsSelected = packetViewModel == All[i]; }

        }

        private void LoadOpcodeStrict(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog { Filter = "Supported Formats, with / without '=' separator (*.txt)|*.txt" };
            if (openFileDialog.ShowDialog() == false) return;
            NetworkController.Instance.StrictCheck = true;
            NetworkController.Instance.LoadOpcodeCheck = openFileDialog.FileName;
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
        private static OpcodeNameConv _instance;
        public static OpcodeNameConv Instance => _instance ?? (_instance = new OpcodeNameConv());
        public ObservableDictionary<ushort, OpcodeEnum> Known { get; set; } = new ObservableDictionary<ushort, OpcodeEnum>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var opn = (string)value;
            if (opn.Length == 4 && opn[1] != '_')
            {
                if (Instance.Known.TryGetValue(System.Convert.ToUInt16(opn, 16), out OpcodeEnum opc))
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
            var v = (bool)value;
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
        public int Count { get; }
        private bool _isSelected = false;
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

        public string Time => $"{Message.Time.ToString("HH:mm:ss.ffff")}";
        public List<List<string>> RowsHex => ParseDataHex(Message.Payload);
        public List<List<string>> RowsText => ParseDataText(Message.Payload);

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

        private List<ByteViewModel> _data;
        private bool _isSearched;
        public List<ByteViewModel> Data => _data ?? (_data = BuildByteView());

        public bool IsSearched
        {
            get => _isSearched;
            set
            {
                //if (_isSelected == value) return;
                _isSearched = value;
                OnPropertyChanged(nameof(IsSearched));
            }
        }

        private List<ByteViewModel> BuildByteView()
        {
            var res = new List<ByteViewModel>();
            for (int i = 0; i < Message.Payload.Count; i += 4)
            {
                var count = i + 4 > Message.Payload.Count ? Message.Payload.Count - i : 4;
                var chunk = new ArraySegment<byte>(Message.Payload.ToArray(), i, count);
                var bvm = new ByteViewModel(chunk.ToArray());
                res.Add(bvm);
            }
            return res;
        }

        public PacketViewModel(ParsedMessage message, int c)
        {
            Message = message;
            Count = c;
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

        public void RefreshData(int i)
        {
            Data[i].Refresh();

        }
    }

    public class ByteViewModel : INotifyPropertyChanged
    {
        private byte[] _value;
        public string Hex => BitConverter.ToString(_value).Replace("-", string.Empty);
        public string Text => BuildString();

        private string BuildString()
        {
            var sb = new StringBuilder();
            foreach (var b in _value)
            {
                sb.Append(b > 0x21 && b < 0x80 ? (char)b : '⋅');
            }
            return sb.ToString();
        }
        private bool _isHovered;
        public bool IsHovered
        {
            get => _isHovered;
            set
            {
                if (_isHovered == value) return;
                _isHovered = value;
                OnPropertyChanged((nameof(IsHovered)));
            }
        }

        public ByteViewModel(byte[] v)
        {
            _value = v;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Refresh()
        {
            OnPropertyChanged(nameof(IsHovered));
        }
    }
}