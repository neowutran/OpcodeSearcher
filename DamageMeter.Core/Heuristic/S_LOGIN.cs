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
        private S_LOGIN() : base(OpcodeEnum.C_CHECK_VERSION) { }
        public new void Process(ParsedMessage message)
        {
            base.Process(message);
            
            
            //TODO


        }
    }
}
