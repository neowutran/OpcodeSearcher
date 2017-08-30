using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DamageMeter.Heuristic;

namespace DamageMeter
{
    public static class DbUtils
    {
        public static bool IsNpcSpawned(ulong id)
        {
            if (!OpcodeFinder.Instance.KnowledgeDatabase.ContainsKey(OpcodeFinder.KnowledgeDatabaseItem.SpawnedNpcs)) { return false; }
            var res = OpcodeFinder.Instance.KnowledgeDatabase[OpcodeFinder.KnowledgeDatabaseItem.SpawnedNpcs].Item2;
            var list = (List<Npc>)res;

            return list.Any(x => x.Cid == id);
        }
        public static bool IsNpcSpawned(ulong id, uint zoneId, uint templateId)
        {
            if (!OpcodeFinder.Instance.KnowledgeDatabase.ContainsKey(OpcodeFinder.KnowledgeDatabaseItem.SpawnedNpcs)) { return false; }
            var res = OpcodeFinder.Instance.KnowledgeDatabase[OpcodeFinder.KnowledgeDatabaseItem.SpawnedNpcs].Item2;
            var list = (List<Npc>)res;

            return list.Any(x => x.Cid == id && x.ZoneId == zoneId && x.TemplateId == templateId);
        }
    }
}
