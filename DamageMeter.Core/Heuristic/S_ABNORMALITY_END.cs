using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class S_ABNORMALITY_END : AbstractPacketHeuristic
    {
        public static S_ABNORMALITY_END Instance => _instance ?? (_instance = new S_ABNORMALITY_END());
        private static S_ABNORMALITY_END _instance;
        private S_ABNORMALITY_END() : base(OpcodeEnum.S_ABNORMALITY_END) { }

        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }

            if (message.Payload.Count != 8 +4) return;
            var target = Reader.ReadUInt64();
            var id = Reader.ReadUInt32();
            
            //we check it on current player
            if (OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacter, out Tuple<Type, object> result))
            {
                var ch = (LoggedCharacter)result.Item2;
                if (ch.Cid != target) return;
                if(OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacterAbnormalities, out Tuple<Type, object> result2))
                {
                    var abs = (List<uint>) result2.Item2;
                    if (!abs.Contains(id)) return;
                }
            }
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            OpcodeFinder.Instance.KnowledgeDatabase.TryRemove(OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacterAbnormalities, out var garbage);
            
        }
    }
}
