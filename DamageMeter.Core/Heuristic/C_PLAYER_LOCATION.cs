using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    public class C_PLAYER_LOCATION : AbstractPacketHeuristic
    {

        public static C_PLAYER_LOCATION Instance => _instance ?? (_instance = new C_PLAYER_LOCATION());
        private static C_PLAYER_LOCATION _instance;

        public bool Initialized = false;
        private C_PLAYER_LOCATION() : base(OpcodeEnum.C_PLAYER_LOCATION) { }
        public new void Process(ParsedMessage message)
        {
            base.Process(message);

            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }
            if(message.Payload.Count != 41) { return; }
            var origin = Reader.ReadVector3f();
            var w = Reader.ReadUInt16();
            var unknown1 = Reader.ReadUInt16();
            var destination = Reader.ReadVector3f();
            var type = Reader.ReadUInt32();
            var speed = Reader.ReadUInt16();
            var unknown2 = Reader.ReadByte();
            var distance = origin.DistanceTo(destination);         

            if (unknown1 == 0 && AcceptedTypeValue.Contains(type) && distance < 200 && distance >= 0  && speed == 0 && unknown2 == 0)
            {
                OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            }

        }

        public List<uint> AcceptedTypeValue = new List<uint>()
        {
            0, // run
            1, // walk
            2, // fall
            5, // jump
            6, // jump on steep terrain?
            7, // stop moving
            10 // fall w/ recovery
        };
    }
}
