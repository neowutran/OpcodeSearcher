using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
  
    public class C_WHISPER : AbstractPacketHeuristic
    {

        public static C_WHISPER Instance => _instance ?? (_instance = new C_WHISPER());
        private static C_WHISPER _instance;

        public bool Initialized = false;
        private C_WHISPER() : base(OpcodeEnum.C_WHISPER) { }
        public new void Process(ParsedMessage message)
        {
            base.Process(message);

            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }
            if (message.Payload.Count < 10) { return; }
            var offsetTarget = Reader.ReadUInt16();
            var offsetMessage = Reader.ReadUInt16();
          
            if (Reader.BaseStream.Position + 4 != offsetTarget)
            {
                return;
            }

            var authorName = "";
            try
            {
                authorName = Reader.ReadTeraString();
            }
            catch
            {
                //Not a string
                return;
            }
            if (Reader.BaseStream.Position + 4 != offsetMessage)
            {
                return;
            }

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
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }

    }
}
