using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DamageMeter.Sniffing;
using Tera.Game;
using Tera.Game.Abnormality;
using Tera.Game.Messages;
using Message = Tera.Message;

namespace DamageMeter
{
    public class NetworkController
    {
        public delegate void ConnectedHandler(string serverName);

        public delegate void GuildIconEvent(Bitmap icon);

        public delegate void UpdateUiHandler(UiUpdateMessage message);

        private static NetworkController _instance;

        private bool _keepAlive = true;
        private long _lastTick;
        internal MessageFactory MessageFactory = new MessageFactory();
        internal bool NeedInit = true;
        public Server Server;
        internal UserLogoTracker UserLogoTracker = new UserLogoTracker();

        private NetworkController()
        {
            TeraSniffer.Instance.NewConnection += HandleNewConnection;
            TeraSniffer.Instance.EndConnection += HandleEndConnection;
            var packetAnalysis = new Thread(PacketAnalysisLoop);
            packetAnalysis.Start();
        }

        public PlayerTracker PlayerTracker { get; internal set; }

        public NpcEntity Encounter { get; private set; }
        public NpcEntity NewEncounter { get; set; }

        public bool TimedEncounter { get; set; }

        public string FileName { get; set; }

        public static NetworkController Instance => _instance ?? (_instance = new NetworkController());

        public EntityTracker EntityTracker { get; internal set; }
        public bool SendFullDetails { get; set; }

        public event GuildIconEvent GuildIconAction;

        public void Exit()
        {
            _keepAlive = false;
            TeraSniffer.Instance.Enabled = false;
            Thread.Sleep(500);
            Application.Exit();
        }

        internal void RaiseConnected(string message)
        {
            Connected?.Invoke(message);
        }

    
        public event ConnectedHandler Connected;
        public event UpdateUiHandler TickUpdated;

        protected virtual void HandleEndConnection()
        {
            NeedInit = true;
            MessageFactory = new MessageFactory();
            Connected?.Invoke("no server");
            OnGuildIconAction(null);
        }

        protected virtual void HandleNewConnection(Server server)
        {
            Server = server;
            NeedInit = true;
            MessageFactory = new MessageFactory();
            Connected?.Invoke(server.Name);
        }

        private void UpdateUi(int packetsWaiting = 0)
        {
            /*
            var uiMessage = new UiUpdateMessage(statsSummary, skills, filteredEntities, timedEncounter, abnormals, teradpsHistory, chatbox, flash);
            handler?.Invoke(uiMessage);
            */
        }
        private void PacketAnalysisLoop()
        {
     
            while (_keepAlive)
            {

                if (FileName != null)
                {
                    LoadFile();
                    FileName = null;
                }

                Encounter = NewEncounter;

                var packetsWaiting = TeraSniffer.Instance.Packets.Count;
                CheckUpdateUi(packetsWaiting);
                var successDequeue = TeraSniffer.Instance.Packets.TryDequeue(out var obj);
                if (!successDequeue)
                {
                    Thread.Sleep(1);
                    continue;
                }

                var message = MessageFactory.Create(obj);
                OpcodeFinder.Instance.Find(message);
            }
        }

        public void CheckUpdateUi(int packetsWaiting)
        {
            var second = DateTime.UtcNow.Ticks;
            if (second - _lastTick < TimeSpan.TicksPerSecond) { return; }
            UpdateUi(packetsWaiting);
        }

        internal virtual void OnGuildIconAction(Bitmap icon)
        {
            GuildIconAction?.Invoke(icon);
        }

        void LoadFile()
        {
            if (FileName != null)
            {
                List<Message> nonparsedList = LogReader.LoadLogFromFile(FileName);
                foreach (Message message in nonparsedList)
                {
                    TeraSniffer.Instance.Packets.Enqueue(message);
                }
            }
        }
    }
}