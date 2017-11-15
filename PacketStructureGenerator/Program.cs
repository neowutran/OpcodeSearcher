using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera;
using Tera.Game;
using Tera.PacketLog;

namespace PacketStructureGenerator
{
    class Program
    {
        private static readonly int VERSION = 321554;
        private static readonly string PATH = @"E:\Tera\PacketLogs\";
        private static readonly string IGNORE_EXTENSION = ".ignore";
        private static readonly string SPLITTED_LOGS_EXTENSION = ".opcodeLog";
        private static readonly string DEF_EXTENSION = ".def";
        private static readonly string LOGS_EXTENSION = ".TeraLog";
        private static readonly int MINIMUM_PACKET_NUMBER = 50;

        static void Main(string[] args)
        {
            var p = new Program();
            p.SplitLogsIntoSeparatorOpcodeFiles();
            p.StaticSizeCheck();
            p.DynamicSizeCheck();

        }

        public void DynamicSizeCheck()
        {
            var path = PATH + VERSION + Path.DirectorySeparatorChar;
            Parallel.ForEach(Directory.GetFiles(path, "*" + SPLITTED_LOGS_EXTENSION, SearchOption.AllDirectories), (file) =>
            {
                var filename = Path.GetFileName(file);
                if (File.Exists(path + filename + DEF_EXTENSION) || File.Exists(path + filename + IGNORE_EXTENSION))
                {
                    return;
                }
                var reader = new PacketLogFile(file);
                foreach (var message in reader.Messages)
                {
                    var structure = FindStruc(message);
                }

            });
        }

        /* 
         * This method is based on the assumption that every structure is declared at the beginning of the packet
         * / beginning of the array. 
         * 
         * TODO, the method should return a structure to describe the packet structure. Didn't thinked of a good 
         * structure yet. 
         */
        private ITeraPacketElement FindStruc(Message message, TeraMessageReader reader = null)
        {
            if (reader == null) { reader = new TeraMessageReader(message); }
            ushort offset;
            ushort count;
            long offset_position;
            long count_position;
            try
            {
                offset = reader.ReadUInt16();
                offset_position = reader.BaseStream.Position;
                count = reader.ReadUInt16();
                count_position = reader.BaseStream.Position;
            }
            // If an error happens, we have reached the end of the stream, not structure can be found. 
            catch { throw new Exception("Should not happens"); }

            if (CheckString(message, offset_position))
            {
                Debug.WriteLine("String found: offset = " + offset_position);
                // is string, check next thing
                FindStruc(message, reader);
                return;
            }
            if (CheckArray(message, offset_position, count_position))
            {
                Debug.WriteLine("String found: offset = " + count_position);
                // is array, check next thing
                FindStruc(message, reader);
                return;
            }

            // Nothing have been found, stop. 
            return;

        }


        //TODO try to parse a tera string given a string offset
        private bool CheckString(Message message, long offset)
        {
            var reader = new TeraMessageReader(message);
            reader.BaseStream.Position = offset;

            try
            {
                reader.ReadTeraString();
                return true;
            }
            catch { return false; }
        }

        //TODO try to parse an array given a array begin offset and the number of elements
        private bool CheckArray(Message message, long offset, long count)
        {
            return false;
        }

        /* 
         * Create a different log file for every opcode found. 
         */ 
        public void SplitLogsIntoSeparatorOpcodeFiles()
        {
            foreach (var file in Directory.GetFiles(PATH + Path.DirectorySeparatorChar + VERSION, "*"+ LOGS_EXTENSION, SearchOption.AllDirectories))
            {
                var reader = new PacketLogFile(file);
                foreach (var message in reader.Messages)
                {
                    var writer = GetOrInitializeWriter(message.OpCode);
                    writer.Append(message);
                }
            }
            CloseReaders();
        }

        /* 
         * Basic check to detect and tag opcode that doesn't contains any string nor array (static length)
         */
        public void StaticSizeCheck()
        {
            Parallel.ForEach(Directory.GetFiles(PATH + VERSION + Path.DirectorySeparatorChar, "*"+SPLITTED_LOGS_EXTENSION, SearchOption.AllDirectories), (file) =>
            {
                var isSameSize = true;
                var initialeSize = 0;
                var reader = new PacketLogFile(file);
                string opcode = Path.GetFileName(file);
                int numberOfPackets = 0;
                initialeSize = reader.Messages.ElementAt(0).Payload.Count;
                Parallel.ForEach(reader.Messages, (message) => {
                    numberOfPackets++;
                    if(message.Payload.Count != initialeSize)
                    {
                        isSameSize = false;
                    }
                });

                // If number of packet < 50, we considere the we don't have enought data to do anything
                if (isSameSize && numberOfPackets > MINIMUM_PACKET_NUMBER)
                {
                    //Debug.WriteLine("Opcode: " + opcode + " is static size: " +initialeSize + "; nbpackets: "+numberOfPackets);
                    File.WriteAllText(PATH + VERSION + Path.DirectorySeparatorChar + opcode + DEF_EXTENSION, initialeSize.ToString());
                }
                if(numberOfPackets < MINIMUM_PACKET_NUMBER)
                {
                    //Debug.WriteLine("Uncommon packet: Opcode: " + opcode + " ;nbpackets: " + numberOfPackets);
                    File.WriteAllText(PATH + VERSION + Path.DirectorySeparatorChar + opcode + IGNORE_EXTENSION, "Not enough data, only "+numberOfPackets + " packets");
                }

            });

        }

        private Dictionary<int, PacketLogWriter> _writers = new Dictionary<int, PacketLogWriter>();
        public PacketLogWriter GetOrInitializeWriter(int opcode)
        {
            if (_writers.ContainsKey(opcode)) { return _writers[opcode]; }
            var header = new LogHeader { Region = opcode.ToString() };
            PacketLogWriter writer = new PacketLogWriter(PATH + VERSION + Path.DirectorySeparatorChar + opcode + SPLITTED_LOGS_EXTENSION, header);
            _writers.Add(opcode, writer);
            return writer;
        }

        public void CloseReaders()
        {
            foreach(var writer in _writers)
            {
                writer.Value.Dispose();
            }
        }
    }
}
