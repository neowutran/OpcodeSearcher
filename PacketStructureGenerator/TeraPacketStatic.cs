using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game;

namespace PacketStructureGenerator
{
    public class TeraPacketStatic : ITeraPacketElement
    {

        private int _size;
        public TeraPacketStatic(int size) { _size = size; }

        public bool Check(TeraMessageReader reader)
        {
            try
            {
                reader.Skip(_size);
            }
            catch { return false; }
            return true;
        }
    }
}
