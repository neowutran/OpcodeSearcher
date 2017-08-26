using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using Tera.Game;

namespace Data
{
    public class BasicTeraData
    {
        private static BasicTeraData _instance;
        private static readonly ILog Log = LogManager.GetLogger("ShinraMeter");
        private static int _errorCount = 10; //limit number of debug messages in one session

        private BasicTeraData() : this(FindResourceDirectory()) { }

        private BasicTeraData(string resourceDirectory)
        {
            ResourceDirectory = resourceDirectory;
            Directory.CreateDirectory(Path.Combine(resourceDirectory, "config")); //ensure config dir is created
            XmlConfigurator.Configure(new Uri(Path.Combine(ResourceDirectory, "log4net.xml")));
            Servers = new ServerDatabase(Path.Combine(ResourceDirectory, "data"));
            Icons = new IconsDatabase(Path.Combine(ResourceDirectory, "data/"));
        }


        //public QuestInfoDatabase QuestInfoDatabase { get; set; }
        public HotDotDatabase HotDotDatabase { get; set; }
        public static BasicTeraData Instance => _instance ?? (_instance = new BasicTeraData());
        public PetSkillDatabase PetSkillDatabase { get; set; }
        public SkillDatabase SkillDatabase { get; set; }
        public NpcDatabase MonsterDatabase { get; set; }
        public string ResourceDirectory { get; }
        public ServerDatabase Servers { get; }
        public IconsDatabase Icons { get; set; }

        private static IEnumerable<Server> GetServers(string filename)
        {
            return File.ReadAllLines(filename).Where(s => !s.StartsWith("#") && !string.IsNullOrWhiteSpace(s)).Select(s => s.Split(new[] {' '}, 3))
                .Select(parts => new Server(parts[2], parts[1], parts[0]));
        }

        private static string FindResourceDirectory()
        {
            var directory = Path.GetDirectoryName(typeof(BasicTeraData).Assembly.Location);
            while (directory != null)
            {
                var resourceDirectory = Path.Combine(directory, @"resources\");
                if (Directory.Exists(resourceDirectory)) { return resourceDirectory; }
                directory = Path.GetDirectoryName(directory);
            }
            throw new InvalidOperationException("Could not find the resource directory");
        }
    }
}