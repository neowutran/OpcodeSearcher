using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class S_DESPAWN_NPC : AbstractPacketHeuristic
    {
        public static S_DESPAWN_NPC Instance => _instance ?? (_instance = new S_DESPAWN_NPC());
        private static S_DESPAWN_NPC _instance;

        public S_DESPAWN_NPC() : base(OpcodeEnum.S_DESPAWN_NPC) { }

        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode))
            {
                if (OpcodeFinder.Instance.GetOpcode(OPCODE) == message.OpCode)
                {
                    var id = Reader.ReadUInt64();
                    RemoveNpcFromDatabase(id);
                }
                return;
            }
            if (message.Payload.Count != 8 + 4 + 4 + 4 + 4 + 4) return;

            var cid = Reader.ReadUInt64();
            var pos = Reader.ReadVector3f();
            var type = Reader.ReadUInt32();
            if (type != 1 && type != 5) return;
            var unk = Reader.ReadUInt32();
            if (unk != 0) return;
            if (!IsNpcSpanwed(cid)) return;
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            RemoveNpcFromDatabase(cid);
        }

        private bool IsNpcSpanwed(ulong id)
        {
            if (OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.SpawnedNpcs, out Tuple<Type, object> result))
            {
                var list = (List<ulong>)result.Item2;
                if (list.Contains(id)) return true;
            }
            return false;
        }
        private void RemoveNpcFromDatabase(ulong id)
        {
            if (OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.SpawnedNpcs, out Tuple<Type, object> result))
            {
                OpcodeFinder.Instance.KnowledgeDatabase.Remove(OpcodeFinder.KnowledgeDatabaseItem.SpawnedNpcs);
            }
            var list = (List<ulong>)result.Item2;
            if (list.Contains(id)) list.Remove(id);
            OpcodeFinder.Instance.KnowledgeDatabase.Add(OpcodeFinder.KnowledgeDatabaseItem.SpawnedNpcs, new Tuple<Type, object>(typeof(List<ulong>), list));
        }

    }
}
