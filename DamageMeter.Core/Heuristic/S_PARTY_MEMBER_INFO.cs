using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class S_PARTY_MEMBER_INFO : AbstractPacketHeuristic
    {
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count < 2 + 2 + 4 + 2 + 4 + 4 + 4 + 4 + 1 + 8 + 4 + 1 + 4 + 4) return;
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_LOGIN)) return;
            var count = Reader.ReadUInt16();
            var offset = Reader.ReadUInt16();
            try
            {
                //var msg = Reader.ReadTeraString();
                Reader.BaseStream.Position = offset - 4;

                for (int i = 0; i < count; i++)
                {
                    Reader.Skip(4);
                    var nameOffset = Reader.ReadUInt16();
                    var playerId = Reader.ReadUInt32();
                    var clas = Reader.ReadUInt16();
                    if (clas > 12) return;
                    var race = Reader.ReadUInt16();
                    var level = Reader.ReadUInt16();
                    if (level > 65) return;
                    Reader.Skip(4);
                    var cId = Reader.ReadUInt64();
                    Reader.Skip(4);
                    try { var name = Reader.ReadTeraString(); }
                    catch (Exception e) { return; }
                    if (playerId == C_REQUEST_PARTY_INFO.LastId)
                    {
                        OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
                        C_REQUEST_PARTY_INFO.Confirm();
                        return;
                    }
                }
            }
            catch (Exception e) { return; }

        }
    }
}
