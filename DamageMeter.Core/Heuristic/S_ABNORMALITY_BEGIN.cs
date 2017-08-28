using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game.Messages;

namespace DamageMeter.Heuristic
{
    class S_ABNORMALITY_BEGIN : AbstractPacketHeuristic
    {
        public static S_ABNORMALITY_BEGIN Instance => _instance ?? (_instance = new S_ABNORMALITY_BEGIN());
        private static S_ABNORMALITY_BEGIN _instance;
        private S_ABNORMALITY_BEGIN() : base(OpcodeEnum.S_ABNORMALITY_BEGIN) { }

        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            if (IsKnown || OpcodeFinder.Instance.IsKnown(message.OpCode)) { return; }
            var matchesLength = message.Payload.Count == 36;
            if (!matchesLength) return;


        }
    }
}
