using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations.Design;
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
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode))
            {
                if (OpcodeFinder.Instance.GetOpcode(OPCODE) == message.OpCode) { Parse(); }
                return;
            }
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

        private void Parse()
        {
            Reader.Skip(2);
            var msg = Reader.ReadTeraString();
            if (msg.StartsWith("@970") && msg.Contains("ChannelName")) S_JOIN_PRIVATE_CHANNEL.Confirm();

        }
    }
}
