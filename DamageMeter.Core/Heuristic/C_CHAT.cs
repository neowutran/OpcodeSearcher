using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    public class C_CHAT : AbstractPacketHeuristic
    {

        public static C_CHAT Instance => _instance ?? (_instance = new C_CHAT());
        private static C_CHAT _instance;

        public bool Initialized = false;
        private C_CHAT() : base(OpcodeEnum.C_CHAT) { }
        public new void Process(ParsedMessage message)
        {
            base.Process(message);

            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }
            if (message.Payload.Count < 16) { return; }
            var offsetMessage = Reader.ReadUInt16();
            var channel = Reader.ReadUInt32();

            var messageTxt = "";
            try
            {
                messageTxt = Reader.ReadTeraString();
            }
            catch
            {
                //Not a string
                return;
            }
            if (Reader.BaseStream.Position + 2 != Reader.BaseStream.Length) //at this point, we must have reached the end of the stream
            {
                return;
            }
            if (!messageTxt.StartsWith("<FONT"))
            {
                return;
            }

            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }

    }
}
