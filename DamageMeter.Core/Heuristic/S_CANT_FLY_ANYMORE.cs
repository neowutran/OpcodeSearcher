using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class S_CANT_FLY_ANYMORE : AbstractPacketHeuristic
    {
        public static S_CANT_FLY_ANYMORE Instance => _instance ?? (_instance = new S_CANT_FLY_ANYMORE());
        private static S_CANT_FLY_ANYMORE _instance;

        public S_CANT_FLY_ANYMORE() : base(OpcodeEnum.S_CANT_FLY_ANYMORE) { }

        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) return;
            if (message.Payload.Count != 0) return;

            if(!OpcodeFinder.Instance.IsKnown(OpcodeEnum.C_PLAYER_FLYING_LOCATION)) return;
            if(!OpcodeFinder.Instance.IsKnown(OpcodeEnum.S_PLAYER_CHANGE_FLIGHT_ENERGY)) return;
            if(S_PLAYER_CHANGE_FLIGHT_ENERGY.Instance.LastEnergy != 0) return; //we should be out of energy if we can't fly anymore
            if(C_PLAYER_FLYING_LOCATION.Instance.LastType != 7) return; //we should be in descending if we can't fly anymore
            OpcodeFinder.Instance.SetOpcode(message.OpCode, OPCODE);
        }
    }
}
