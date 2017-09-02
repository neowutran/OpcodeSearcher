using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class S_SYSTEM_MESSAGE : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count < 2) return;
            var offset = Reader.ReadUInt16();
            try
            {
                var msg = Reader.ReadTeraString();
                if (msg.StartsWith("@"))
                {
                    var i = msg.IndexOf('\v');
                    string smt = "";
                    smt = i != -1 ? msg.Substring(1, i - 1) : msg.Substring(1);
                    if (!ushort.TryParse(smt, out var m)) return;
                }
                else return;
            }
            catch (Exception e) { return; }
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
