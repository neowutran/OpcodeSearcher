using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    public class S_GET_USER_LIST : AbstractPacketHeuristic
    {
        public static S_GET_USER_LIST Instance => _instance ?? (_instance = new S_GET_USER_LIST());
        private static S_GET_USER_LIST _instance;

        public bool Initialized = false;
        private S_GET_USER_LIST() : base(OpcodeEnum.S_GET_USER_LIST) { }
        public new void Process(ParsedMessage message)
        {
            base.Process(message);

            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }

            if (OpcodeFinder.Instance.PacketCount == 10 && message.Payload.Count > 100)
            {
                OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);

                //create a <PlayerId, Name> dictionary and add to KnowledgeDB
                var chars = new Dictionary<uint, string>();
                var count = Reader.ReadUInt16();
                var offset = Reader.ReadUInt16();

                for (int i = 0; i < count; i++)
                {
                    Reader.BaseStream.Position = offset - 4;
                    Reader.Skip(2);
                    offset = Reader.ReadUInt16();
                    Reader.Skip(4);
                    var nameOffset = Reader.ReadUInt16();
                    Reader.Skip(10);
                    var id = Reader.ReadUInt32();
                    Reader.BaseStream.Position = nameOffset - 4;
                    var name = Reader.ReadTeraString();
                    chars.Add(id, name);
                }
                OpcodeFinder.Instance.KnowledgeDatabase.Add("Characters", new Tuple<Type, object>(typeof(Dictionary<uint, string>), chars));
            }

        }
    }
}
