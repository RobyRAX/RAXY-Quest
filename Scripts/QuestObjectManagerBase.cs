using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.QuestSystem
{
    public abstract class QuestObjectManagerBase : MonoBehaviour, IQuestObjectManager
    {
        protected IQuestManager QuestManagerRef { get; private set; }

        protected void BindQuestManager(IQuestManager questManager)
        {
            QuestManagerRef = questManager;
        }

        IReadOnlyDictionary<string, ActiveQuestProgress> ActiveQuestDict =>
            QuestManagerRef?.ActiveQuestDict;

        IReadOnlyDictionary<string, QuestStatus> AllQuestStatusDict =>
            QuestManagerRef?.AllQuestStatusDict;

#if UNITY_EDITOR
        [TitleGroup("Quest Objects")]
        [ShowInInspector]
        [TableList(AlwaysExpanded = true)]
        List<IQuestObjectDrawer> _allQuestObjectsDrawer
        {
            get
            {
                if (QuestObjects == null)
                    return null;

                var temp = new List<IQuestObjectDrawer>();
                foreach (var questObject in QuestObjects)
                {
                    temp.Add(new IQuestObjectDrawer { QuestObject = questObject });
                }

                return temp;
            }
        }
#endif

        public List<IQuestObject> QuestObjects = new();

        IReadOnlyList<IQuestObject> IQuestObjectManager.QuestObjects => QuestObjects;

        [Button]
        public void RefreshQuestObjectState()
        {
            if (ActiveQuestDict == null)
            {
                Debug.LogError("[QuestObjectManager] ActiveQuestDict is NULL");
                return;
            }

            if (AllQuestStatusDict == null)
            {
                Debug.LogError("[QuestObjectManager] AllQuestStatusDict is NULL");
                return;
            }

            QuestObjects = FindAllQuestObjects();

            List<string> questIds = AllQuestStatusDict.Keys.ToList();

            foreach (var questObj in QuestObjects)
            {
                if (questObj == null)
                    continue;

                bool shouldFlip = EvaluateFlipConditions(questObj, questIds);
                ApplyState(questObj, shouldFlip);
            }
        }

        protected List<IQuestObject> FindAllQuestObjects()
        {
            return GameObject.FindObjectsByType<MonoBehaviour>(
                    findObjectsInactive: FindObjectsInactive.Include,
                    sortMode: FindObjectsSortMode.None)
                .OfType<IQuestObject>()
                .ToList();
        }

        bool EvaluateFlipConditions(IQuestObject questObj, List<string> questIds)
        {
            foreach (var cond in questObj.FlipStateConditions)
            {
                if (cond == null)
                    continue;

                if (EvaluateSingleCondition(cond, questIds))
                    return true;
            }

            return false;
        }

        bool EvaluateSingleCondition(QuestObjectVisibilityCondition cond, List<string> allQuestIds)
        {
            foreach (var questId in cond.questIds)
            {
                if (allQuestIds.Contains(questId) == false)
                    return false;
            }

            if (cond.hierarchyType == QuestHierarchyType.Quest &&
                cond.completionStates != null &&
                cond.completionStates.Count > 0)
            {
                bool stateMatch = false;

                foreach (var id in cond.questIds)
                {
                    if (!AllQuestStatusDict.TryGetValue(id, out QuestStatus st))
                        continue;
                    if (!cond.completionStates.Contains(st.CompletionState))
                        continue;

                    stateMatch = true;
                    break;
                }

                return stateMatch;
            }

            if (cond.hierarchyType == QuestHierarchyType.ObjectiveSet &&
                cond.objectiveSetIndices != null &&
                cond.objectiveSetIndices.Count > 0)
            {
                bool stateMatch = false;

                foreach (var questId in cond.questIds)
                {
                    if (ActiveQuestDict.TryGetValue(questId, out var activeQuest))
                    {
                        foreach (var index in cond.objectiveSetIndices)
                        {
                            if (activeQuest.objectiveSets[index].completionState != CompletionState.InProgress)
                                continue;

                            stateMatch = true;
                            break;
                        }
                    }
                }

                return stateMatch;
            }

            return false;
        }

        void ApplyState(IQuestObject questObj, bool flipped)
        {
            bool finalActive = (questObj.DefaultState == QuestObjectDefaultState.Enabled)
                ? !flipped
                : flipped;

            if (flipped)
                questObj.OnStateFlipped?.Invoke();
            else
                questObj.OnStateReverted?.Invoke();

            questObj.ShouldActive = finalActive;

            bool hasNonEventOnly = questObj.FlipStateConditions.Any(c => !c.triggerEventOnly);

            if (hasNonEventOnly)
            {
                questObj.GetGameObject.SetActive(finalActive);
            }
        }
    }
}
