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
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace DamageMeter
{
    public class OpcodeFinder
    {
        public static OpcodeFinder Instance => _instance ?? (_instance = new OpcodeFinder());
        private static OpcodeFinder _instance;

        private OpcodeFinder() {
            NetworkController.Instance.UiUpdateKnownOpcode.Add(19900, OpcodeEnum.C_CHECK_VERSION);
            NetworkController.Instance.UiUpdateKnownOpcode.Add(19901, OpcodeEnum.S_CHECK_VERSION);
        }

        public enum KnowledgeDatabaseItem
        {
            LoggedCharacter = 0,
            PlayerLocation = 1,
            Characters = 2,
            SpawnedUsers = 3,
            SpawnedNpcs = 4,
            LoggedCharacterAbnormalities = 5,
            CharacterSpawnedSuccesfully = 6,
            PartyMemberList
        }

        public bool OpcodePartialMatch()
        {
            var opcodeFile = NetworkController.Instance.LoadOpcodeCheck;
            NetworkController.Instance.LoadOpcodeCheck = null;
            var file = new System.IO.StreamReader(opcodeFile);
            string line;
            bool matched = true;
            while ((line = file.ReadLine()) != null)
            {
                line = line.Replace("=", "");
                var match = Regex.Match(line, @"(?i)^\s*([a-z_0-9]+)\s+(\d+)\s*$");
                Enum.TryParse(match.Groups[1].Value, out OpcodeEnum opcodeName);
                OpcodeId opcodeId = OpcodeId.Parse(match.Groups[2].Value);

                if(KnownOpcode.ContainsKey(opcodeId) && KnownOpcode[opcodeId] != opcodeName)
                {
                    Console.WriteLine("Incorrect match type 1 for " + KnownOpcode[opcodeId] + " : " + opcodeId);
                    matched = false;
                }else if(ReverseKnownOpcode.ContainsKey(opcodeName) && ReverseKnownOpcode[opcodeName] != opcodeId)
                {
                    Console.WriteLine("Incorrect match type 2 for " + opcodeName + " : " + opcodeId);
                    matched = false;
                }else if(!ReverseKnownOpcode.ContainsKey(opcodeName) && !KnownOpcode.ContainsKey(opcodeId))
                {
                    // Stay silent if the parser didn't found every opcode. 
                    // TODO: add option for strict match: aka -> this case generate error 
                }
                else
                {
                    Console.WriteLine("Correct match for " + KnownOpcode[opcodeId] + " : " + opcodeId);
                }
            }

            file.Close();
            return matched;
        }

        // Use that to set value like CID etc ...
        public ConcurrentDictionary<KnowledgeDatabaseItem, Tuple<Type, object>> KnowledgeDatabase = new ConcurrentDictionary<KnowledgeDatabaseItem, Tuple<Type, object>>();
        private Dictionary<OpcodeId, OpcodeEnum> KnownOpcode = new Dictionary<OpcodeId, OpcodeEnum>()
        {
            { 19900, OpcodeEnum.C_CHECK_VERSION },
            { 19901, OpcodeEnum.S_CHECK_VERSION },
        };
        private Dictionary<OpcodeEnum, OpcodeId> ReverseKnownOpcode = new Dictionary<OpcodeEnum, OpcodeId>()
        {
            { OpcodeEnum.C_CHECK_VERSION, 19900 },
            { OpcodeEnum.S_CHECK_VERSION, 19901 },
        };

        // Once your module found a new opcode
        public void SetOpcode(OpcodeId opcode, OpcodeEnum opcodeName)
        {
            if (KnownOpcode.ContainsKey(opcode))
            {
                KnownOpcode.TryGetValue(opcode, out var value);
                throw new Exception("opcode: " + opcode + " is already know = " + value + " . You try to add instead = " + Enum.GetName(typeof(OpcodeEnum), opcodeName));
            }
            if (KnownOpcode.Values.Contains(opcodeName))
            {
                throw new Exception("opcodename: " + Enum.GetName(typeof(OpcodeEnum), opcodeName) + " is already know = " + opcode);
            }
            Console.WriteLine(opcode +" => "+opcodeName);
            ReverseKnownOpcode.Add(opcodeName, opcode);
            KnownOpcode.Add(opcode, opcodeName);
            NetworkController.Instance.UiUpdateKnownOpcode.Add(opcode, opcodeName);
        }

        public bool IsKnown(OpcodeId opcode) => KnownOpcode.ContainsKey(opcode);

        public bool IsKnown(OpcodeEnum opcode) => ReverseKnownOpcode.ContainsKey(opcode);

        public ushort? GetOpcode(OpcodeEnum opcode)
        {
            if (!ReverseKnownOpcode.ContainsKey(opcode)) { return null; }
            return ReverseKnownOpcode[opcode];
        }

        //For the kind of heuristic "only appear in the first X packet"
        public long PacketCount = 0;

        public void Find(ParsedMessage message)
        {
            PacketCount++;
            AllPackets.Add(PacketCount, message);
            NetworkController.Instance.UiUpdateData.Add(message);
            if (message.Direction == MessageDirection.ClientToServer) {         
                Parallel.ForEach(ClientOpcode, x => x.DynamicInvoke(message));
            }
            else
            {                
                Parallel.ForEach(ServerOpcode, x => x.DynamicInvoke(message));
            }
        }

        // For the kind of heuristic "this opcode only appear less than 1 second after this other opcode"
        public Dictionary<long, ParsedMessage> AllPackets = new Dictionary<long, ParsedMessage>();

        public long TotalOccurrenceOpcode(OpcodeId opcode) => AllPackets.Where(x => x.Value.OpCode == opcode).Count();

        public ParsedMessage GetMessage(long messageNumber) => AllPackets[messageNumber];

        private static readonly List<Delegate> ClientOpcode = new List<Delegate>
        {
            {new Action<ParsedMessage>(x => Heuristic.C_SECOND_PASSWORD_AUTH.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.C_CHECK_VERSION.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.C_LOGIN_ARBITER.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.C_SET_VISIBLE_RANGE.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.C_GET_USER_LIST.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.C_PLAYER_LOCATION.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.C_WHISPER.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.C_CHAT.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.C_SELECT_USER.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.C_GET_USER_GUILD_LOGO.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.C_PONG.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.C_USE_ITEM.Instance.Process(x))}, //needed only for S_START_COOLTIME_ITEM
            {new Action<ParsedMessage>(x => Heuristic.C_DUNGEON_COOL_TIME_LIST.Instance.Process(x))}, 
            {new Action<ParsedMessage>(x => Heuristic.C_NPCGUILD_LIST.Instance.Process(x))}, 
            {new Action<ParsedMessage>(x => Heuristic.C_DUNGEON_CLEAR_COUNT_LIST.Instance.Process(x))}, 
            {new Action<ParsedMessage>(x => Heuristic.C_REQUEST_USER_ITEMLEVEL_INFO.Instance.Process(x))}, 
            {new Action<ParsedMessage>(x => Heuristic.C_REQUEST_USER_PAPERDOLL_INFO.Instance.Process(x))}, 
            {new Action<ParsedMessage>(x => Heuristic.C_CHANGE_PARTY_MEMBER_AUTHORITY.Instance.Process(x))}, 
            {new Action<ParsedMessage>(x => Heuristic.C_CHANGE_PARTY_MANAGER.Instance.Process(x))}, 
            {new Action<ParsedMessage>(x => Heuristic.C_START_SKILL.Instance.Process(x))}, 
            {new Action<ParsedMessage>(x => Heuristic.C_PARTY_APPLICATION_DENIED.Instance.Process(x))}, 
        };

        private static readonly List<Delegate> ServerOpcode = new List<Delegate>
        {
            {new Action<ParsedMessage>(x => Heuristic.S_SECOND_PASSWORD_AUTH_RESULT.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_REQUEST_SECOND_PASSWORD_AUTH.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_SELECT_USER.Instance.Process(x))},
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
            {new Action<ParsedMessage>(x => Heuristic.S_SPAWN_USER.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_PARTY_MEMBER_LIST.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_PLAYER_STAT_UPDATE.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_CREATURE_CHANGE_HP.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_PLAYER_CHANGE_MP.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_PLAYER_CHANGE_STAMINA.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_DESPAWN_NPC.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_DESPAWN_USER.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_ABNORMALITY_BEGIN.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_ABNORMALITY_REFRESH.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_ABNORMALITY_END.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_BROCAST_GUILD_FLAG.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_USER_LOCATION.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_NPC_LOCATION.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_SPAWN_PROJECTILE.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_BOSS_GAGE_INFO.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_USER_STATUS.Instance.Process(x))},
  			{new Action<ParsedMessage>(x => Heuristic.S_PREPARE_RETURN_TO_LOBBY.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_NPC_STATUS.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_LOAD_TOPO.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_START_COOLTIME_ITEM.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_USER_PAPERDOLL_INFO.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_TRADE_BROKER_DEAL_SUGGESTED.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_ANSWER_INTERACTIVE.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_UPDATE_FRIEND_INFO.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_LEAVE_PARTY_MEMBER.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_GUILD_TOWER_INFO.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_START_COOLTIME_SKILL.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_DECREASE_COOLTIME_SKILL.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_WEAK_POINT.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_USER_EFFECT.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_DUNGEON_EVENT_MESSAGE.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_PARTY_MEMBER_ABNORMAL_ADD.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_PARTY_MEMBER_ABNORMAL_DEL.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_PARTY_MEMBER_ABNORMAL_REFRESH.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_PARTY_MEMBER_ABNORMAL_CLEAR.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_PARTY_MEMBER_STAT_UPDATE.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_PARTY_MEMBER_CHANGE_HP.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_PARTY_MEMBER_CHANGE_MP.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_PARTY_MEMBER_CHANGE_STAMINA.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_LOGOUT_PARTY_MEMBER.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_CHANGE_PARTY_MANAGER.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_FIN_INTER_PARTY_MATCH.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_OTHER_USER_APPLY_PARTY.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_RESULT_BIDDING_DICE_THROW.Instance.Process(x))},
            {new Action<ParsedMessage>(x => Heuristic.S_RESULT_ITEM_BIDDING.Instance.Process(x))},
        };
    }
}