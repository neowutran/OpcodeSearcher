using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    public class C_GET_USER_GUILD_LOGO : AbstractPacketHeuristic
    {
        public static C_GET_USER_GUILD_LOGO Instance => _instance ?? (_instance = new C_GET_USER_GUILD_LOGO());
        private static C_GET_USER_GUILD_LOGO _instance;

        public bool Initialized = false;
        private C_GET_USER_GUILD_LOGO() : base(OpcodeEnum.C_GET_USER_GUILD_LOGO) { }

        public new void Process(ParsedMessage message)
        {
            base.Process(message);

            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }
            if (message.Payload.Count < 8) { return; }

            var playerId = Reader.ReadUInt32();
            var guildId = Reader.ReadUInt32();
            
            if (!OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.Characters, out Tuple<Type, object> currChar))
            {
                throw new Exception("Logger character is not in database.");
            }
            var chList = (Dictionary<uint, Character>)currChar.Item2;
            if (!chList.TryGetValue(playerId, out var character)) return;
            if (character.GuildId != guildId) return;

            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
