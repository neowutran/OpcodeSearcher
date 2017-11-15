using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera;
using Tera.Game;

namespace PacketStructureGenerator
{
    public class TeraPacketArray : ITeraPacketElement
    {
        private LinkedList<ITeraPacketElement> _elements = new LinkedList<ITeraPacketElement>();
        private int _count;
        private bool _isRoot;
        public TeraPacketArray(int count, bool isRoot)
        {
            _count = count;
            _isRoot = isRoot;
        }

        public bool Check(TeraMessageReader reader)
        {
            ushort nextOffset = 0;
            for (int i = 0; i < _count; i++)
            {
                // If it s not the root array, we must check the current offset & next offset value
                if (!_isRoot)
                {
                    ushort currentOffset = reader.ReadUInt16();
                    if(nextOffset != currentOffset) { return false; }
                    nextOffset = reader.ReadUInt16();

                    // Array too small
                    if(nextOffset == 0 && i == _count - 1) { return false; }
                    // Array too big
                    if(nextOffset != 0 && i == _count) { return false; }
                }

                foreach (var element in _elements)
                {
                    if (!element.Check(reader)) { return false; }
                }
             
            }

            // If it's the root structure, need to check that we have reached the end of the stream
            if (_isRoot && reader.BaseStream.Position != reader.BaseStream.Length) { return false; }
            return true;

        }

    }
}
