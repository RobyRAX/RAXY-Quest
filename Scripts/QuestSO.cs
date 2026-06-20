using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RAXY.InventorySystem;
using RAXY.Utility.Localization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.QuestSystem
{
    [CreateAssetMenu(fileName = "New Quest", menuName = "RAXY/Quest/Quest SO")]
    public class QuestSO : ScriptableObject
    {
        [ShowInInspector]
        [ReadOnly]
        [PropertyOrder(-1)]
        [TitleGroup("Quest Information")]
        public string QuestId => this.name;

        [TitleGroup("Quest Information")]
        public LocalizationCacher QuestNameLoc;
        [TitleGroup("Quest Information")]
        public LocalizationCacher QuestDetailLoc;
        [TitleGroup("Quest Information")]
        [EnumToggleButtons]
        public QuestType questType;

        [TitleGroup("Quest Requirement Data")]
        public List<QuestRequirementData> requirements;

#if UNITY_EDITOR
        [TitleGroup("Quest Information")]
        [SerializeField]
        string note = "Write notes here...";
#endif
        [TitleGroup("Objective Set")]
        [ToggleLeft]
        public bool questCompleteAfterAllObjectives = true;

        [TitleGroup("Objective Set")]
        [ListDrawerSettings(ShowIndexLabels = true)]
        public List<ObjectiveSet> objectiveSets;

        [TitleGroup("Quest Reward")]
        [ToggleLeft]
        public bool giveRewardOnQuestComplete;

        [TitleGroup("Quest Reward")]
        public List<ItemAmountContainer> questRewards;
    }

    [System.Serializable]
    public class QuestRequirementData
    {
        public QuestRequirementType type;

        [TitleGroup("Data")]
        [ShowIf("@type == QuestRequirementType.PlayerLevel")]
        public int requiredPlayerLevel;

        [TitleGroup("Data")]
        [ShowIf("@type == QuestRequirementType.HasItem")]
        public bool subtractItemOnQuestStart;

        [TitleGroup("Data")]
        [ShowIf("@type == QuestRequirementType.HasItem")]
        [ListDrawerSettings(ListElementLabelName = "Label")]
        public ItemAmountContainer requiredItem;

        [TitleGroup("Data")]
        [ShowIf("@type == QuestRequirementType.CompletedQuest")]
#if UNITY_EDITOR
        [ValueDropdown(nameof(EditorQuestChainIds))]
#endif
        public string requiredQuest;

#if UNITY_EDITOR
        public static List<string> EditorQuestChainIds()
        {
            if (!PlayerPrefs.HasKey(QuestDatabaseSO.QUESTS_PLAYER_PREF))
                return new List<string>();

            string json = PlayerPrefs.GetString(QuestDatabaseSO.QUESTS_PLAYER_PREF);
            return JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
        }
#endif
    }

    public enum QuestRequirementType
    {
        PlayerLevel, HasItem, CompletedQuest
    }

    public enum QuestType
    { 
        Main, Side
    }
}
