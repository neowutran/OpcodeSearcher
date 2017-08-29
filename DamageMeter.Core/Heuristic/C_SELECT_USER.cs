using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
 
    public class C_SELECT_USER : AbstractPacketHeuristic
    {
        public static C_SELECT_USER Instance => _instance ?? (_instance = new C_SELECT_USER());
        private static C_SELECT_USER _instance;
        private C_SELECT_USER() : base(OpcodeEnum.C_SELECT_USER) { }
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }
            if (message.Payload.Count != 5) return;
            var id = Reader.ReadUInt32();
            if (OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.Characters, out Tuple<Type, object> chars))
            {
                var list = chars.Item2 as Dictionary<uint, Character>;
                if (!list.ContainsKey(id)) { return; }
            }
            else
            {
                throw new Exception("At this point, characters must be known");
            }
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }

    }

}
