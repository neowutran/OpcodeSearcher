using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class S_WORLD_QUEST_VILLAGER_INFO : AbstractPacketHeuristic
    {
        public static S_WORLD_QUEST_VILLAGER_INFO Instance => _instance ?? (_instance = new S_WORLD_QUEST_VILLAGER_INFO());
        private static S_WORLD_QUEST_VILLAGER_INFO _instance;

        public S_WORLD_QUEST_VILLAGER_INFO() : base(OpcodeEnum.S_WORLD_QUEST_VILLAGER_INFO) { }

        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_LOGIN)) return;
            if (!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_CLEAR_WORLD_QUEST_VILLAGER_INFO)) return;
            //if (message.Payload.Count < 300) return; //should be a big packet, but not sure
            if (OpcodeFinder.Instance.PacketCount > 300) return; //should be received after login 1st time
            //check the last 5 packets for sClearWorldQuestVillagerInfo
            bool found = false;
            for (int i = 0; i < 5; i++)
            {
                var msg = OpcodeFinder.Instance.GetMessage(OpcodeFinder.Instance.PacketCount - 1 - i);
                if (msg.OpCode == OpcodeFinder.Instance.GetOpcode(OpcodeEnum.S_CLEAR_WORLD_QUEST_VILLAGER_INFO)) found = true;
            }
            if (!found) return;

            //TODO: check that it's actually a list or find useful info in this packet

            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);


        }
    }
}
