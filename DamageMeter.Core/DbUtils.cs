using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DamageMeter.Heuristic;
using Tera.Game;

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

        public static bool IsUserSpawned(ulong id)
        {
            if (!OpcodeFinder.Instance.KnowledgeDatabase.ContainsKey(OpcodeFinder.KnowledgeDatabaseItem.SpawnedUsers)) { return false; }
            var res = OpcodeFinder.Instance.KnowledgeDatabase[OpcodeFinder.KnowledgeDatabaseItem.SpawnedUsers].Item2;
            var list = (List<ulong>)res;
            return list.Any(x => x == id);
        }

        public static Vector3f GetPlayerLocation()
        {
            if (!OpcodeFinder.Instance.KnowledgeDatabase.ContainsKey(OpcodeFinder.KnowledgeDatabaseItem.PlayerLocation)) { return new Vector3f(); }
            var res = OpcodeFinder.Instance.KnowledgeDatabase[OpcodeFinder.KnowledgeDatabaseItem.PlayerLocation].Item2;
            return (Vector3f)res;
        }

        public static ulong GetPlayercId()
        {
            if (!OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.LoggedCharacter, out var res)) { return 0; }
            var character = (LoggedCharacter)res.Item2;
            return character.Cid;
        }

        public static bool IsPartyMember(uint playerId)
        {
            if (!OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList, out var res)) { return false; }
            var list = (List<PartyMember>)res.Item2;
            return list.Any(x => x.PlayerId == playerId);
        }
        public static bool IsPartyMember(uint playerId, uint serverId)
        {
            if (!OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList, out var res)) { return false; }
            var list = (List<PartyMember>)res.Item2;
            return list.Any(x => x.PlayerId == playerId && x.ServerId == serverId);
        }

        public static void AddPartyMemberAbnormal(uint playerId, uint serverId, uint abnormId)
        {
            if (!OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList, out var res)) { return; }
            var list = (List<PartyMember>)res.Item2;
            if (!list.Any(x => x.ServerId == serverId && x.PlayerId == playerId)) { return; }

            var member = list.FirstOrDefault(x => x.ServerId == serverId && x.PlayerId == playerId);
            if (member.Abnormals.Contains(abnormId)) return;
            list.Remove(member);
            member.Abnormals.Add(abnormId);
            list.Add(member);
            OpcodeFinder.Instance.KnowledgeDatabase.Remove(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList);
            OpcodeFinder.Instance.KnowledgeDatabase.Add(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList, new Tuple<Type, object>(typeof(List<PartyMember>), list));

        }
        public static void RemovePartyMemberAbnormal(uint playerId, uint serverId, uint abnormId)
        {
            if (!OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList, out var res)) { return; }
            var list = (List<PartyMember>)res.Item2;
            if (!list.Any(x => x.ServerId == serverId && x.PlayerId == playerId)) { return; }

            var member = list.FirstOrDefault(x => x.ServerId == serverId && x.PlayerId == playerId);
            if (!member.Abnormals.Contains(abnormId)) return;
            list.Remove(member);
            member.Abnormals.Remove(abnormId);
            list.Add(member);
            OpcodeFinder.Instance.KnowledgeDatabase.Remove(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList);
            OpcodeFinder.Instance.KnowledgeDatabase.Add(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList, new Tuple<Type, object>(typeof(List<PartyMember>), list));

        }

        public static bool PartyMemberHasAbnorm(uint playerId, uint serverId, uint abnormId)
        {
            if (!OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList, out var res)) { return false; }
            var list = (List<PartyMember>)res.Item2;
            if (!list.Any(x => x.ServerId == serverId && x.PlayerId == playerId)) { return false; }

            var member = list.FirstOrDefault(x => x.ServerId == serverId && x.PlayerId == playerId);
            return member.Abnormals.Contains(abnormId);

        }

        public static void UpdatePartyMemberMaxHp(uint playerId, uint serverId, uint maxHp)
        {
            if (!OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList, out var res)) { return; }
            var list = (List<PartyMember>)res.Item2;
            if (!list.Any(x => x.ServerId == serverId && x.PlayerId == playerId)) { return; }

            var member = list.FirstOrDefault(x => x.ServerId == serverId && x.PlayerId == playerId);
            list.Remove(member);
            member.MaxHp = maxHp;
            list.Add(member);
            OpcodeFinder.Instance.KnowledgeDatabase.Remove(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList);
            OpcodeFinder.Instance.KnowledgeDatabase.Add(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList, new Tuple<Type, object>(typeof(List<PartyMember>), list));
        }
        public static void UpdatePartyMemberMaxMp(uint playerId, uint serverId, uint maxMp)
        {
            if (!OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList, out var res)) { return; }
            var list = (List<PartyMember>)res.Item2;
            if (!list.Any(x => x.ServerId == serverId && x.PlayerId == playerId)) { return; }

            var member = list.FirstOrDefault(x => x.ServerId == serverId && x.PlayerId == playerId);
            list.Remove(member);
            member.MaxMp = maxMp;
            list.Add(member);
            OpcodeFinder.Instance.KnowledgeDatabase.Remove(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList);
            OpcodeFinder.Instance.KnowledgeDatabase.Add(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList, new Tuple<Type, object>(typeof(List<PartyMember>), list));
        }
        public static void UpdatePartyMemberMaxRe(uint playerId, uint serverId, uint maxRe)
        {
            if (!OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList, out var res)) { return; }
            var list = (List<PartyMember>)res.Item2;
            if (!list.Any(x => x.ServerId == serverId && x.PlayerId == playerId)) { return; }

            var member = list.FirstOrDefault(x => x.ServerId == serverId && x.PlayerId == playerId);
            list.Remove(member);
            member.MaxRe = maxRe;
            list.Add(member);
            OpcodeFinder.Instance.KnowledgeDatabase.Remove(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList);
            OpcodeFinder.Instance.KnowledgeDatabase.Add(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList, new Tuple<Type, object>(typeof(List<PartyMember>), list));
        }

        public static PartyMember GetPartyMember(uint playerId, uint serverId)
        {
            if (!OpcodeFinder.Instance.KnowledgeDatabase.TryGetValue(OpcodeFinder.KnowledgeDatabaseItem.PartyMemberList, out var res)) { return new PartyMember(0,0,""); }
            var list = (List<PartyMember>)res.Item2;
            if (!list.Any(x => x.ServerId == serverId && x.PlayerId == playerId)) { return new PartyMember(0,0,""); }
            return  list.FirstOrDefault(x => x.ServerId == serverId && x.PlayerId == playerId);
        }
    }

}
