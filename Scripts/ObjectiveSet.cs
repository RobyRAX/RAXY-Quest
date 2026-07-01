using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RAXY.InventorySystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.QuestSystem
{
    [Serializable]
    public class ObjectiveSet
    {
        public ObjectiveSetCompletionOrderType completionOrderType;

        [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "ObjectiveLabel")]
        public List<ObjectiveData> mainObjectives;

        [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "ObjectiveLabel")]
        public List<ObjectiveData> optionalObjectives;
    }

    [Serializable]
    public class ObjectiveData
    {
        [OnValueChanged("RefreshTargetIds")]
        public ObjectiveType type;       // Kill, Collect, Talk, Explore, dll

        [LabelText("@TargetIdLabel")]
        //[InlineButton("RefreshTargetIds", SdfIconType.ArrowClockwise, "Refresh")]
        [ValueDropdown("@TargetIds")]
        public string targetId;          // EnemyID / ItemID / NPCID / AreaID
        public int requiredAmount = 1;   // Default 1

#if UNITY_EDITOR
        void RefreshTargetIds()
        {
            switch (type)
            {
                case ObjectiveType.KillEnemy:
                    break;
                case ObjectiveType.Collect:
                    break;
                case ObjectiveType.TalkTo:
                    break;
            }
        }

        public static List<string> TargetIds { get; set; }

        public string ObjectiveLabel
        {
            get
            {
                string typeLabel = GetTypeDisplay(type);

                if (ShowRequiredAmount)
                    return $"{typeLabel} → {targetId} (x{requiredAmount})";
                else
                    return $"{typeLabel} → {targetId}";
            }
        }
        string GetTypeDisplay(ObjectiveType t)
        {
            switch (t)
            {
                case ObjectiveType.KillEnemy: return "Kill";
                case ObjectiveType.KillEnemyRace: return "Kill (Race)";
                case ObjectiveType.Collect: return "Collect";
                case ObjectiveType.TalkTo: return "Talk";
                case ObjectiveType.GoTo: return "Go";
                default: return t.ToString();
            }
        }

        string TargetIdLabel
        {
            get
            {
                switch (type)
                {
                    case ObjectiveType.KillEnemy:
                        return "Enemy Id";
                    case ObjectiveType.KillEnemyRace:
                        return "Enemy Race Id";
                    case ObjectiveType.Collect:
                        return "Item Id";
                    case ObjectiveType.TalkTo:
                        return "Talk Target Id";
                    case ObjectiveType.GoTo:
                        return "Area Id";
                    default:
                        return "Target Id";
                }
            }
        }

        bool ShowRequiredAmount
        {
            get
            {
                return type == ObjectiveType.KillEnemy || type == ObjectiveType.KillEnemyRace || type == ObjectiveType.Collect;
            }
        }
#endif
    }

    public enum ObjectiveType
    {
        KillEnemy, KillEnemyRace, Collect, TalkTo, GoTo
    }

    public enum ObjectiveSetCompletionOrderType
    {
        Sequential, NonSequential
    }
}

