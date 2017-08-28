using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    public struct Character
    {
        public uint Id;
        public uint Gender;
        public uint Race;
        public uint Class;
        public uint Level;
        public string Name;

        public Character(uint id, uint gender, uint race, uint c, uint level, string name)
        {
            Id = id;
            Gender = gender;
            Race = race;
            Class = c;
            Level = level;
            Name = name;
        }
    }
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
                var chars = new Dictionary<uint, Character>();
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
                    var gender = Reader.ReadUInt32();
                    var race = Reader.ReadUInt32();
                    var cl = Reader.ReadUInt32();
                    var level = Reader.ReadUInt32();
                    Reader.BaseStream.Position = nameOffset - 4;
                    var name = Reader.ReadTeraString();
                    var ch = new Character(id, gender, race, cl, level, name);
                    chars.Add(id, ch);
                }
                OpcodeFinder.Instance.KnowledgeDatabase.Add(OpcodeFinder.KnowledgeDatabaseItem.Characters, new Tuple<Type, object>(typeof(Dictionary<uint, Character>), chars));
            }

        }
    }
}
