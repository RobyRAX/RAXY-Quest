using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RAXY.InventorySystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.QuestSystem
{
    [Serializable]
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
            if (!PlayerPrefs.HasKey(QuestEditorConstants.QUESTS_PLAYER_PREF))
                return new List<string>();

            string json = PlayerPrefs.GetString(QuestEditorConstants.QUESTS_PLAYER_PREF);
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
