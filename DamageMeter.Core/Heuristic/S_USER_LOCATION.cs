using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    public class S_USER_LOCATION : AbstractPacketHeuristic
    {
        public static S_USER_LOCATION Instance => _instance ?? (_instance = new S_USER_LOCATION());
        private static S_USER_LOCATION _instance;

        public bool Initialized = false;

        private S_USER_LOCATION() : base(OpcodeEnum.S_USER_LOCATION)
        {
        }

        public new void Process(ParsedMessage message)
        {
            base.Process(message);

            if (message.Payload.Count != 43) { return; }
            var target = Reader.ReadUInt64();
            var origin = Reader.ReadVector3f();
            var w = Reader.ReadUInt16();
            var unknown1 = Reader.ReadUInt16();
            var speed = Reader.ReadUInt16();
            var destination = Reader.ReadVector3f();

            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode))
            {
                // For the moment, only update our own location. If later it will become required, add other users location
                var self = (LoggedCharacter)OpcodeFinder.Instance.KnowledgeDatabase[OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacter].Item2;
                if (self.Cid == target)
                {
                    UpdateLocationInDictionary(OpcodeFinder.KnowledgeDatabaseItem.PlayerLocation, destination);
                }
                return;
            }
            var type = Reader.ReadUInt32();
            var unknown2 = Reader.ReadByte();
            var distance = origin.DistanceTo(destination);
            //added check for SpawnedUser, sUserLocation detect will work after sSpawnUser (you should tp into town as example)
            if (!OpcodeFinder.Instance.KnowledgeDatabase.ContainsKey(OpcodeFinder.KnowledgeDatabaseItem.SpawnedUsers))
            {
                return;
            }
            var users = (List<ulong>)OpcodeFinder.Instance.KnowledgeDatabase[OpcodeFinder.KnowledgeDatabaseItem.SpawnedUsers].Item2;
            if (!users.Contains(target)) { return; }
            if (unknown1 == 0 && AcceptedTypeValue.Contains(type) && distance < 200 && distance >= 0 && unknown2 == 0)
            {
                var self = (LoggedCharacter)OpcodeFinder.Instance.KnowledgeDatabase[OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacter].Item2;
                if (self.Cid == target)
                {
                    UpdateLocationInDictionary(OpcodeFinder.KnowledgeDatabaseItem.PlayerLocation, destination);
                }
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

        public struct PlayerLocation
        {
            public Tera.Game.Vector3f Location;

            public PlayerLocation(Tera.Game.Vector3f loc)
            {
                Location = loc;
            }
        }

        private static void UpdateLocationInDictionary(OpcodeFinder.KnowledgeDatabaseItem knowledgeDatabaseKey, Tera.Game.Vector3f destination)
        {
            OpcodeFinder.Instance.KnowledgeDatabase[knowledgeDatabaseKey] = new Tuple<Type, object>(typeof(PlayerLocation), destination);
        }
    }
}