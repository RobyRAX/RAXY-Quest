using System;
using System.Collections.Generic;
using System.Linq;
using RAXY.Utility;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.QuestSystem
{
    [Serializable]
    [HideReferenceObjectPicker]
    public class ActiveQuestProgress
    {
        public const int ALL_OBJECTIVE_DONE = 200;

        [HideInInspector]
        public Action OnObjectiveSetCompleted; 
        [HideInInspector]
        public Action<ObjectiveProgressBase> OnObjectiveProgressed; 

        [HideInInspector]
        public Action OnQuestCompleted;
        public string QuestId => quest.QuestId;

        [TitleGroup("Quest")]
        [PropertyOrder(-2)]
        public IQuest quest;

        [TitleGroup("Quest")]
        [ShowInInspector]
        [PropertyOrder(-1)]
        public bool QuestCompleteAfterAllObjectives => quest.QuestCompleteAfterAllObjectives;

        [TitleGroup("Quest")]
        [ReadOnly]
        public int activeObjectiveSetIndex;

        [TitleGroup("Quest")]
        [ShowInInspector]
        [ListDrawerSettings(ShowIndexLabels = true, ElementColor = "GetElementColor", IsReadOnly = true, ListElementLabelName = "Label")]
        [HideReferenceObjectPicker]
        public List<ObjectiveSetStatusWrapper> objectiveSets = new();

        [TitleGroup("Quest")]
        bool _allObjectiveSetDone;
        public bool AllObjectiveSetDone
        {
            get => _allObjectiveSetDone;
            set
            {
                if (value == _allObjectiveSetDone)
                    return;

                _allObjectiveSetDone = value;

                if (_allObjectiveSetDone)
                {
                    OnQuestCompleted?.Invoke();
                }
            }
        }

        private Color GetElementColor(int index)
        {
            var selectedQuest = objectiveSets[index];
            return QuestManagerBase.GetQuestListElementColor(index, selectedQuest.completionState);
        }

        [TitleGroup("Active Objectives")]
        [BoxGroup("Active Objectives/Active Objectives")]
        [HideReferenceObjectPicker]
        [HideLabel]
        public ActiveObjectiveSetProgress activeObjectiveSet;

        IQuestManager _questManager;

        public ActiveQuestProgress(IQuest quest, IQuestManager questManager)
        {
            this.quest = quest;
            _questManager = questManager;

            foreach (var questObjectiveSet in quest.ObjectiveSets)
            {
                objectiveSets.Add(new ObjectiveSetStatusWrapper(questObjectiveSet));
            }

            TakeObjectiveSet(0);
        }

        [TitleGroup("Debug Function")]
        [Button]
        public void TakeObjectiveSet(int index)
        {
            // Out of bounds check
            if (index < 0 || index >= objectiveSets.Count)
            {
                Debug.LogWarning($"[ActiveQuestProgress] Invalid quest index {index}");
                return;
            }

            // If it's not the first quest, check if the previous one is completed
            if (index > 0)
            {
                var previousQuest = objectiveSets[index - 1];
                if (previousQuest.completionState != CompletionState.Completed)
                {
                    Debug.LogWarning($"[ActiveQuestProgress] Cannot take objective set {index} " +
                                     $"because previous objective set {index - 1} is not completed.");
                    return;
                }
            }

            var objectiveData = objectiveSets[index];
            objectiveData.completionState = CompletionState.InProgress;
            activeObjectiveSetIndex = index;
            activeObjectiveSet = new ActiveObjectiveSetProgress(objectiveData, QuestId, _questManager.QuestFactory);
            activeObjectiveSet.OnActiveObjectiveSetCompleted = ActiveObjectiveSetCompletedHandler;
            activeObjectiveSet.OnObjectiveProgressed = ObjectiveProgressedHandler;

            _questManager?.NotifyQuestObjectsRefresh();

            //Debug.Log($"[ActiveQuestProgress] Now taking objective set {index}: {objectiveData.objectiveSetSO.ObjectiveSetId}");
        }

        private void ObjectiveProgressedHandler(ObjectiveProgressBase objective)
        {
            OnObjectiveProgressed?.Invoke(objective);
        }

        void ActiveObjectiveSetCompletedHandler()
        {
            CompleteActiveObjectiveSet();
            OnObjectiveSetCompleted?.Invoke();
        }

        [TitleGroup("Debug Function")]
        [Button]
        public void CompleteActiveObjectiveSet()
        {
            activeObjectiveSet.ObjectiveSetStatus.completionState = CompletionState.Completed;

            AllObjectiveSetDone = objectiveSets.All(x => x.completionState == CompletionState.Completed);

            if (AllObjectiveSetDone)
            {
                activeObjectiveSetIndex = ALL_OBJECTIVE_DONE;
            }
            else
            {
                TakeObjectiveSet(activeObjectiveSetIndex + 1);
            }
        }

        public void ApplySaveData(ActiveQuest_SaveData saveData)
        {
            if (saveData.questId != QuestId)
            {
                CustomDebug.Log($"ApplySaveData for {QuestId} failed because incoming save data id doesn't match");
                return;
            }

            activeObjectiveSetIndex = saveData.activeObjectiveSetIndex;
            if (activeObjectiveSetIndex != ALL_OBJECTIVE_DONE)
            {
                for (int i = activeObjectiveSetIndex - 1; i >= 0; i--)
                {
                    objectiveSets[i].completionState = CompletionState.Completed;
                }
                TakeObjectiveSet(activeObjectiveSetIndex);
            }
            else
            {
                for (int i = objectiveSets.Count - 1; i >= 0; i--)
                {
                    objectiveSets[i].completionState = CompletionState.Completed;
                }
                TakeObjectiveSet(objectiveSets.Count - 1);
                foreach (var obj in activeObjectiveSet.mainObjectives)
                {
                    obj.CurrentProgress = float.MaxValue;
                }
            }
            activeObjectiveSet.ApplySaveData(saveData.activeObjectiveSet);
        }
    }

    [HideReferenceObjectPicker]
    public class ActiveQuest_SaveData
    {
        public string questId;
        public int activeObjectiveSetIndex;
        [HideReferenceObjectPicker]
        public ActiveObjectiveSet_SaveData activeObjectiveSet;

        public ActiveQuest_SaveData() { }
        public ActiveQuest_SaveData(ActiveQuestProgress questProgress)
        {
            if (questProgress == null)
                return;

            questId = questProgress.QuestId;
            activeObjectiveSetIndex = questProgress.activeObjectiveSetIndex;
            activeObjectiveSet = new ActiveObjectiveSet_SaveData(questProgress.activeObjectiveSet);
        }
    }
}
