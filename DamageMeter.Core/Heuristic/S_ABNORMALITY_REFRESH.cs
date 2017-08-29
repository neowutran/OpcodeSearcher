using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class S_ABNORMALITY_REFRESH : AbstractPacketHeuristic
    {
        public static S_ABNORMALITY_REFRESH Instance => _instance ?? (_instance = new S_ABNORMALITY_REFRESH());
        private static S_ABNORMALITY_REFRESH _instance;
        private S_ABNORMALITY_REFRESH() : base(OpcodeEnum.S_ABNORMALITY_REFRESH) { }

        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }

            if (message.Payload.Count != 8 + 4 + 4 + 4 + 4) return;
            var target = Reader.ReadUInt64();
            var id = Reader.ReadUInt32();
            var duration = Reader.ReadUInt32();
            Reader.Skip(4);
            var stacks = Reader.ReadUInt32();

            //we could check this on minor battle solutions

            if (id != 4000) return;
            if (stacks != 1) return;
            if (duration != 30 * 60 * 1000) return;
            if (OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacter, out Tuple<Type, object> result))
            {
                var ch = (LoggedCharacter)result.Item2;
                if (ch.Cid != target) return;
            }
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }

}
