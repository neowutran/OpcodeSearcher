using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Tera.Game;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    internal class S_SPAWN_ME : AbstractPacketHeuristic
    {
        public static S_SPAWN_ME Instance => _instance ?? (_instance = new S_SPAWN_ME());
        private static S_SPAWN_ME _instance;

        public Vector3f LatestPos;


        private S_SPAWN_ME() : base(OpcodeEnum.S_SPAWN_ME) { }

        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode))
            {
                if (OpcodeFinder.Instance.GetOpcode(OPCODE) == message.OpCode)
                {
                    Reader.Skip(8);
                    LatestPos = Reader.ReadVector3f();
                    if (!S_LOAD_TOPO.Instance.IsKnown)
                    {
                        S_LOAD_TOPO.Instance.Confirm(LatestPos);
                    }
                }
                return;
            }

            if (message.Payload.Count != 24) return;
            var target = Reader.ReadUInt64();
            LatestPos = Reader.ReadVector3f(); //maybe add more checks based on pos?
            var w = Reader.ReadUInt16();
            var alive = Reader.ReadBoolean();
            var unk = Reader.ReadByte();
            if (unk != 0) return;
            if (!OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacter, out Tuple<Type, object> currChar))
            {
                UpdateLocationInDictionary(OpcodeFinder.KnowledgeDatabaseItem.CharacterSpawnedSuccesfully, false);
                return;
            }
            var ch = (LoggedCharacter)currChar.Item2;
            if (target != ch.Cid) return;

            OpcodeFinder.Instance.SetOpcode(message.OpCode, OpcodeEnum.S_SPAWN_ME);
            if (!S_LOAD_TOPO.Instance.IsKnown)
            {

                S_LOAD_TOPO.Instance.Confirm(LatestPos);

            }
            UpdateLocationInDictionary(OpcodeFinder.KnowledgeDatabaseItem.CharacterSpawnedSuccesfully, true);
        }

        private static void UpdateLocationInDictionary(OpcodeFinder.KnowledgeDatabaseItem knowledgeDatabaseKey, bool state)
        {
            if (OpcodeFinder.Instance.KnowledgeDatabase.ContainsKey(knowledgeDatabaseKey))
            {
                OpcodeFinder.Instance.KnowledgeDatabase.TryRemove(knowledgeDatabaseKey, out var garbage);
            }
            OpcodeFinder.Instance.KnowledgeDatabase.TryAdd(knowledgeDatabaseKey, new Tuple<Type, object>(typeof(Boolean), state));
        }
    }
}