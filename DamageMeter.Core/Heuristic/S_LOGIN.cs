using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    public class S_LOGIN : AbstractPacketHeuristic
    {
        public static S_LOGIN Instance => _instance ?? (_instance = new S_LOGIN());
        private static S_LOGIN _instance;
        private S_LOGIN() : base(OpcodeEnum.S_LOGIN) { }
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }
            if (message.Payload.Count < 268) return;
            var nameOffset = Reader.ReadUInt16();
            Reader.Skip(8);
            var model = Reader.ReadUInt32();
            var cid = Reader.ReadUInt64();
            var serverId = Reader.ReadUInt32();
            if (NetworkController.Instance.Server.ServerId != serverId) return;
            var playerId = Reader.ReadUInt32();
            Reader.Skip(4+1+4+4+4+8+2);
            var level = Reader.ReadUInt16();
            Reader.BaseStream.Position = nameOffset - 4;
            var name = "";
            try
            {
                name = Reader.ReadTeraString();
            }
            catch (Exception) { return; }
            if (OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.Characters, out Tuple<Type, object> chars))
            {
                var list = chars.Item2 as Dictionary<uint, Character>;
                if (!list.TryGetValue(playerId, out Character c)) { return; }
                if (model != c.RaceGenderClass.Raw) return;
                if (c.Name != name) return;
            }
            else
            {
                throw new Exception("At this point, characters must be known");
            }

            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
            var ch = new LoggedCharacter(cid, model, name, playerId, level);
            OpcodeFinder.Instance.KnowledgeDatabase.Add(OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacter, new Tuple<Type, object>(typeof(LoggedCharacter), ch));
            //TODO


        }

    }

    public struct LoggedCharacter
    {
        public ulong Cid;
        public uint Model;
        public string Name;
        public uint PlayerId;
        public uint Level;

        public LoggedCharacter(ulong cid, uint model, string name, uint pId, uint lvl)
        {
            Cid = cid;
            Model = model;
            Name = name;
            PlayerId = pId;
            Level = lvl;
        }
    }
}
