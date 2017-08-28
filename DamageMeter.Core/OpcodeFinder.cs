using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Tera;
using Tera.Game.Messages;
using OpcodeId = System.UInt16;
using Grade = System.UInt32;

namespace DamageMeter
{
    
    public class OpcodeFinder
    {
        public static OpcodeFinder Instance => _instance ?? (_instance = new OpcodeFinder());
        private static OpcodeFinder _instance;
        private OpcodeFinder(){}

        // Use that to set value like CID etc ...
        public Dictionary<string, Tuple<Type, object>> KnowledgeDatabase = new Dictionary<string, Tuple<Type, object>>();

        public Dictionary<OpcodeId, OpcodeEnum> KnownOpcode = new Dictionary<OpcodeId, OpcodeEnum>()
        {
            {19900, OpcodeEnum.C_CHECK_VERSION }, //Those 2 opcode never change
            {19901, OpcodeEnum.S_CHECK_VERSION }


            // Add here if you already know some opcode
        };

        public event Action<ushort> OpcodeFound;
        public event Action<ParsedMessage> NewMessage;
        // Once your module found a new opcode
        public void SetOpcode(OpcodeId opcode, OpcodeEnum opcodeName)
        {
            if (KnownOpcode.ContainsKey(opcode))
            {
                KnownOpcode.TryGetValue(opcode, out var value);
                throw new Exception("opcode: " + opcode + " is already know = " + value + " . You try to add instead = " + Enum.GetName(typeof(OpcodeEnum), opcodeName));
            }
            if (KnownOpcode.ContainsValue(opcodeName))
            {
                throw new Exception("opcodename: " + Enum.GetName(typeof(OpcodeEnum), opcodeName) + " is already know = " + opcode);
            }
            KnownOpcode.Add(opcode, opcodeName);

            OpcodeFound?.Invoke(opcode);
            Console.WriteLine(Enum.GetName(typeof(OpcodeEnum), opcodeName) + " = " + opcode);
            Debug.WriteLine(Enum.GetName(typeof(OpcodeEnum), opcodeName) + " = " + opcode);
        }

        public bool IsKnown(OpcodeId opcode) => KnownOpcode.ContainsKey(opcode);
        public bool IsKnown(OpcodeEnum opcode) => KnownOpcode.ContainsValue(opcode);
        public ushort? GetOpcode(OpcodeEnum opcode) {
            if (IsKnown(opcode)){
                return KnownOpcode.Where(x => x.Value == opcode).Select(x => x.Key).First();
            }
            return null;
        }

        //For the kind of heuristic "only appear in the first X packet" 
        public long PacketCount = 0;

        public void Find(ParsedMessage message)
        {

            message.PrintRaw();

            PacketCount++;
            AllPackets.Add(PacketCount, message);
            NewMessage?.Invoke(message);
            if (message.Direction == Tera.MessageDirection.ClientToServer)
            {
                ClientOpcode.ForEach(x => x.DynamicInvoke(message));
                return;
            }
            ServerOpcode.ForEach(x => x.DynamicInvoke(message));

        }

        // For the kind of heuristic "this opcode only appear less than 1 second after this other opcode"  
        private Dictionary<long, ParsedMessage> AllPackets = new Dictionary<long, ParsedMessage>();

        public long TotalOccurrenceOpcode(OpcodeId opcode) => AllPackets.Where(x => x.Value.OpCode == opcode).Count();
        public ParsedMessage GetMessage(long messageNumber) => AllPackets[messageNumber];

        private static readonly List<Delegate> ClientOpcode = new List<Delegate>
        {
            {new Action<ParsedMessage>(x => Heuristic.C_CHECK_VERSION.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.C_LOGIN_ARBITER.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.C_SET_VISIBLE_RANGE.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.C_GET_USER_LIST.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.C_PLAYER_LOCATION.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.C_WHISPER.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.C_CHAT.Instance.Process(x))},
        };

        private static readonly List<Delegate> ServerOpcode = new List<Delegate>
        {
            {new Action<ParsedMessage>(x => Heuristic.S_LOGIN.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_LOADING_SCREEN_CONTROL_INFO.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_REMAIN_PLAY_TIME.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_LOGIN_ARBITER.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_LOGIN_ACCOUNT_INFO.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_LOAD_CLIENT_ACCOUNT_SETTING.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_GET_USER_LIST.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_ACCOUNT_PACKAGE_LIST.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_CONFIRM_INVITE_CODE_BUTTON.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_UPDATE_CONTENTS_ON_OFF.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_EACH_SKILL_RESULT.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_CHAT.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_WHISPER.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_LOGIN.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_SPAWN_ME.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_SPAWN_NPC.Instance.Process(x))},

        };
    }
}
