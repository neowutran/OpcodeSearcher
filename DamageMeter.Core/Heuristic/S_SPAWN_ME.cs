using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    internal class S_SPAWN_ME : AbstractPacketHeuristic
    {
        public static S_SPAWN_ME Instance => _instance ?? (_instance = new S_SPAWN_ME());
        private static S_SPAWN_ME _instance;

        private S_SPAWN_ME() : base(OpcodeEnum.S_SPAWN_ME)
        {
        }

        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }

            if (message.Payload.Count != 26) return;
            var target = Reader.ReadUInt64();
            var pos = Reader.ReadVector3f(); //maybe add more checks based on pos?
            var w = Reader.ReadUInt16();
            var alive = Reader.ReadBoolean();
            var unk = Reader.ReadByte();
            if (unk != 0) return;
            if (!OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacter, out Tuple<Type, object> currChar)) {
                throw new Exception("Logger character should be know at this point.");
            }
            var ch = (LoggedCharacter)currChar.Item2;
            if (target != ch.Cid) return;

            OpcodeFinder.Instance.SetOpcode(message.OpCode, OpcodeEnum.S_SPAWN_ME);
        }
    }
}