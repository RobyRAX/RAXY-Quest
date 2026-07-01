using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace RAXY.QuestSystem
{
    public interface IQuestObject
    {
        public GameObject GetGameObject { get; }
        public bool ShouldActive { get; set; }

        public QuestObjectDefaultState DefaultState { get; }
        public List<QuestObjectVisibilityCondition> FlipStateConditions { get; }

        public UnityEvent OnStateFlipped { get; }
        public UnityEvent OnStateReverted { get; }
    }

    [Serializable]
    public class IQuestObjectInputData
    {
        [EnumToggleButtons]
        public QuestObjectDefaultState DefaultState;
        [FormerlySerializedAs("VisibilityConditions")]
        public List<QuestObjectVisibilityCondition> FlipStateConditions;

        [FoldoutGroup("Events")]
        public UnityEvent OnStateFlipped;
        [FoldoutGroup("Events")]
        public UnityEvent OnStateReverted;
    }

    [Serializable]
    public class QuestObjectVisibilityCondition
    {
        public bool triggerEventOnly;
        public QuestHierarchyType hierarchyType;

        [ValueDropdown("@QuestIdList")]
        [FormerlySerializedAs("targetIds")]
        public List<string> questIds;

        [ShowIf("@hierarchyType == QuestHierarchyType.Quest")]
        public List<CompletionState> completionStates;

        [ShowIf("@hierarchyType == QuestHierarchyType.ObjectiveSet")]
        public List<int> objectiveSetIndices;

#if UNITY_EDITOR
        List<string> QuestIdList
        {
            get
            {
                string fromJson = PlayerPrefs.GetString(QuestEditorConstants.QUESTS_PLAYER_PREF);
                return JsonConvert.DeserializeObject<List<string>>(fromJson);
            }
        }
#endif
    }

    public enum QuestObjectDefaultState
    {
        Disabled, Enabled
    }

    public enum QuestHierarchyType
    {
        Quest, ObjectiveSet
    }

#if UNITY_EDITOR
    [Serializable]
    public class IQuestObjectDrawer
    {
        public IQuestObject QuestObject;

        [ShowInInspector]
        [TableColumnWidth(75, false)]
        public bool ShouldActive => QuestObject != null ? QuestObject.ShouldActive : false;
    }
#endif
}
