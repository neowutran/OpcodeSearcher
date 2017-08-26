using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    public class C_CHECK_VERSION : AbstractPacketHeuristic
    {
        public static C_CHECK_VERSION Instance => _instance ?? (_instance = new C_CHECK_VERSION());
        private static C_CHECK_VERSION _instance;

        public bool Initialized = false;
        private C_CHECK_VERSION(): base(OpcodeEnum.C_CHECK_VERSION) {}        
        public new void Process(ParsedMessage message)
        {
            base.Process(message);

            var samePacket = IsSamePacket(message.OpCode);
            // We are not supposed to find this packet more than 1 time
            if (Initialized && samePacket)
            {
                throw new Exception("Relogin is not supported yet.");
            }

            Initialized = samePacket;
        }
    }
}
