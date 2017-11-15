using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game;

namespace PacketStructureGenerator
{
    interface ITeraPacketElement
    {

        bool Check(TeraMessageReader reader);
    }
}
