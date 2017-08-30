using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class S_START_COOLTIME_SKILL : AbstractPacketHeuristic
    {
        public static S_START_COOLTIME_SKILL Instance => _instance ?? (_instance = new S_START_COOLTIME_SKILL());
        private static S_START_COOLTIME_SKILL _instance;

        public S_START_COOLTIME_SKILL() : base(OpcodeEnum.S_START_COOLTIME_SKILL) { }

        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if(message.Payload.Count != 8) return;
            if(!OpcodeFinder.Instance.IsKnown(OpcodeEnum.C_START_SKILL)) return;

            var skill = Reader.ReadUInt32();
            if (C_START_SKILL.Instance.LatestSkill != skill) return;
            var cd = Reader.ReadUInt32();

            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
