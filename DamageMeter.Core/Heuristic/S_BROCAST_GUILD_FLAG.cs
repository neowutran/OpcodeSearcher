using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{

    public class S_BROCAST_GUILD_FLAG : AbstractPacketHeuristic
    {

        public static S_BROCAST_GUILD_FLAG Instance => _instance ?? (_instance = new S_BROCAST_GUILD_FLAG());
        private static S_BROCAST_GUILD_FLAG _instance;

        private S_BROCAST_GUILD_FLAG() : base(OpcodeEnum.S_BROCAST_GUILD_FLAG) { }
        public new void Process(ParsedMessage message)
        {
            base.Process(message);

            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }
            if(message.Payload.Count != 4) { return; }
            var updateContent = OpcodeFinder.Instance.GetOpcode(OpcodeEnum.S_UPDATE_CONTENTS_ON_OFF);
            if (updateContent == null || !updateContent.HasValue) { return; }
            if (Reader.ReadUInt32() == 0)
            {
                OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            }

        }
    }
}
