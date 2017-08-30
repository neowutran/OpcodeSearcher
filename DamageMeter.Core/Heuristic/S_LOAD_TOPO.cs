using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml.Drawing.Chart;
using Tera.Game;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class S_LOAD_TOPO : AbstractPacketHeuristic
    {
        public static S_LOAD_TOPO Instance => _instance ?? (_instance = new S_LOAD_TOPO());
        private static S_LOAD_TOPO _instance;

        public S_LOAD_TOPO() : base(OpcodeEnum.S_LOAD_TOPO) { }
        private Dictionary<ushort, Vector3f> PossibleMessages = new Dictionary<ushort, Vector3f>();

        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;

            if (message.Payload.Count != 4 + 4 + 4 + 4 + 1) return;
            var zone = Reader.ReadUInt32();
            var pos = Reader.ReadVector3f();
            try
            {
                var quick = Reader.ReadBoolean();
            }
            catch (Exception e) { return; }
            if(!PossibleMessages.ContainsKey(message.OpCode)) PossibleMessages.Add(message.OpCode, pos);
        }

        public void Confirm(Vector3f pos)
        {
            if (PossibleMessages.ContainsValue(pos))
            {
                var opc = PossibleMessages.FirstOrDefault(x => x.Value.Equals(pos)).Key;
                OpcodeFinder.Instance.SetOpcode(opc, OPCODE);
            }
        }
    }
}
