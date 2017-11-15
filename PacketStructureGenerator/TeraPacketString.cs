using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game;

namespace PacketStructureGenerator
{
    public class TeraPacketString : ITeraPacketElement
    {
        public bool Check(TeraMessageReader reader)
        {
            try
            {
                reader.ReadTeraString();
                return true;
            }
            catch { return false; }
        }
    }
}
