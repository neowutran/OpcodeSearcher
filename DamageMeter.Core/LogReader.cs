using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera;
using Tera.Game;
using Tera.Game.Messages;
using Tera.PacketLog;

namespace DamageMeter
{
    public static class LogReader
    {
        public static List<Message> LoadLogFromFile(string filename)
        {
            var plf = new PacketLogFile(filename);
            var messageList = plf.Messages.ToList();
            return messageList;
        }
    }
}
