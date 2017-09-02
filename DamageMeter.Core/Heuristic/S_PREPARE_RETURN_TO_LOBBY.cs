using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    public class S_PREPARE_RETURN_TO_LOBBY : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);

            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }
            if (message.Payload.Count != 4) { return; }
            var previousPacket = OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 1);
            if (previousPacket.Direction == Tera.MessageDirection.ClientToServer && previousPacket.Payload.Count == 0)
            {
                var time = Reader.ReadInt32();
                if (time != 0 && OpcodeFinder.Instance.KnowledgeDatabase.ContainsKey(OpcodeFinder.KnowledgeDatabaseItem.CharacterSpawnedSuccesfully))
                {
                    //Return To lobby detection
                    OpcodeFinder.Instance.SetOpcode(previousPacket.OpCode, OpcodeEnum.C_RETURN_TO_LOBBY);
                    //S_PREPARE_RETURN_TO_LOBBY
                    OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
                }
            }
        }
    }
}