using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RAXY.QuestSystem
{
    [CreateAssetMenu(fileName = "Quest DB", menuName = "RAXY/Quest/Quest Database")]
    public class QuestDatabaseSO : ScriptableObject
    {
        [TitleGroup("All Quest")]
        public List<QuestSO> Quests;

#if UNITY_EDITOR
        [TitleGroup("Sorting")]
        [ShowInInspector]
        public List<QuestSO> MainQuestChain
        {
            get
            {
                return Quests.Where(q => q.questType == QuestType.Main).ToList();
            }
        }

        [TitleGroup("Sorting")]
        [ShowInInspector]
        public List<QuestSO> SideQuestChain
        {
            get
            {
                return Quests.Where(q => q.questType == QuestType.Side).ToList();
            }
        }

        public const string QUESTS_PLAYER_PREF = "Editor_Quests";
        public const string OBJECTIVE_SETS_PLAYER_PREF = "Editor_ObjectiveSets";

        [HorizontalGroup("All Quest/Op")]
        [Button]
        public void SaveQuestIdsToPrefs()
        {
            var questIds = Quests
                            .Where(q => q != null)
                            .Select(q => q.QuestId) // Replace `Id` with the actual field/property name
                            .ToList();
            string json = JsonConvert.SerializeObject(questIds, Formatting.None);
            PlayerPrefs.SetString(QUESTS_PLAYER_PREF, json);
            Debug.Log($"Saved - {json}");

            PlayerPrefs.Save();
        }

        [HorizontalGroup("All Quest/Op")]
        [Button]
        public void ScanQuestSO()
        {
            List<QuestSO> result = new List<QuestSO>();

            // Find all assets of type QuestSO
            string[] guids = AssetDatabase.FindAssets("t:QuestSO");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                QuestSO quest = AssetDatabase.LoadAssetAtPath<QuestSO>(path);

                if (quest == null)
                    continue;

                result.Add(quest);
            }

            Quests = result;
        }
#endif

        public QuestSO GetQuestSO(string questId)
        {
            return Quests.Find(x => x.QuestId == questId);
        }
    }
}

