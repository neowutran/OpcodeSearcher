using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class S_START_COOLTIME_ITEM : AbstractPacketHeuristic
    {
        public static S_START_COOLTIME_ITEM Instance => _instance ?? (_instance = new S_START_COOLTIME_ITEM());
        private static S_START_COOLTIME_ITEM _instance;

        public S_START_COOLTIME_ITEM() : base(OpcodeEnum.S_START_COOLTIME_ITEM) { }


        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;

            if (message.Payload.Count != 8) return;
            var item = Reader.ReadUInt32();
            var cd = Reader.ReadUInt32();
            /*
             * // we could also check on minor battle solution, since we use it for sAbnormalityRefresh already
             * if (item != 200997 && cd != 5) return; 
             */
            if (C_USE_ITEM.Instance.IsKnown)
            {
                if (C_USE_ITEM.Instance.LatestItem == item)
                {
                    OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
                }
            }

        }
    }
}
