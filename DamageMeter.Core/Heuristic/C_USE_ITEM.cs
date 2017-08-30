using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class C_USE_ITEM : AbstractPacketHeuristic 
    {
        public static C_USE_ITEM Instance => _instance ?? (_instance = new C_USE_ITEM());
        private static C_USE_ITEM _instance;

        public C_USE_ITEM() : base(OpcodeEnum.C_USE_ITEM) { }
        public uint LatestItem;
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode))
            {
                if (OpcodeFinder.Instance.GetOpcode(OPCODE) == message.OpCode)
                {
                    Reader.Skip(8);
                    LatestItem = Reader.ReadUInt32();
                }
                return;
            }

            if (message.Payload.Count != 71) return;

            var ownerId = Reader.ReadUInt64();
            LatestItem = Reader.ReadUInt32();
            /*
             * //we can check this on minor battle solution since we already use it for sAbnormalityRefresh
             * if (LatestItem != 200997) return;
            */
            var id = Reader.ReadUInt32();
            Reader.Skip(3*4);
            var unk4 = Reader.ReadUInt32();
            if (unk4 != 1) return;
            Reader.Skip(3*4);
            var pos = Reader.ReadVector3f(); //could compare this to current player position
            Reader.Skip(3*4 + 2);
            var unk11 = Reader.ReadByte();
            if (unk11 != 1) return;

            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
