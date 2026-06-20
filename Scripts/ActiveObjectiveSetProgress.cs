using System;
using System.Collections.Generic;
using System.Linq;
using RAXY.Utility;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.QuestSystem
{
    public class ObjectiveSetStatusWrapper
    {
        string Label => completionState.ToString();

        public ObjectiveSet objectiveSet;
        public CompletionState completionState;

        public ObjectiveSetStatusWrapper(ObjectiveSet objectiveSet)
        {
            this.objectiveSet = objectiveSet;
        }
    }

    public class ActiveObjectiveSetProgress
    {
        public string QuestId { get; set; }

        public ObjectiveSetStatusWrapper ObjectiveSetStatus { get; set; }

        [TitleGroup("Quest Info")]
        [ShowInInspector]
        public ObjectiveSet ObjectiveSet => ObjectiveSetStatus.objectiveSet;

        [TitleGroup("Quest Info")]
        [ShowInInspector]
        public ObjectiveSetCompletionOrderType CompletionOrderType => ObjectiveSet.completionOrderType;

        [TitleGroup("Quest Info")]
        [ShowIf("@CompletionOrderType == ObjectiveSetCompletionOrderType.Sequential")]
        [ReadOnly]
        public int activeObjectiveIndex;

        [TitleGroup("Objectives")]
        [HideReferenceObjectPicker]
        [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "ObjectiveLabel", ElementColor = "GetElementColor_Main", IsReadOnly = true)]
        public List<ObjectiveProgressBase> mainObjectives = new();

        [TitleGroup("Objectives")]
        [HideReferenceObjectPicker]
        [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "ObjectiveLabel", ElementColor = "GetElementColor_Optional", IsReadOnly = true)]
        public List<ObjectiveProgressBase> optionalObjectives = new();

#if UNITY_EDITOR
        private Color GetElementColor_Main(int index)
        {
            var selectedObjective = mainObjectives[index];
            return QuestManagerBase.GetQuestListElementColor(index, selectedObjective.completionState);
        }

        private Color GetElementColor_Optional(int index)
        {
            var selectedObjective = optionalObjectives[index];
            return QuestManagerBase.GetQuestListElementColor(index, selectedObjective.completionState);
        }
#endif

        [TitleGroup("Objectives")]
        bool _allMainObjectivesDone;
        public bool AllMainObjectivesDone
        {
            get => _allMainObjectivesDone;
            set
            {
                if (value == _allMainObjectivesDone)
                    return;

                _allMainObjectivesDone = value;

                if (_allMainObjectivesDone)
                {
                    OnActiveObjectiveSetCompleted?.Invoke();
                }
            }
        }

        [HideInInspector]
        public Action OnActiveObjectiveSetCompleted;

        [HideInInspector]
        public Action<ObjectiveProgressBase> OnObjectiveProgressed;

        public ActiveObjectiveSetProgress(ObjectiveSetStatusWrapper objectiveSetData, string questId, IQuestFactory questFactory)
        {
            ObjectiveSetStatus = objectiveSetData;
            QuestId = questId;

            #region Activate Main Objectives
            foreach (var objective in ObjectiveSet.mainObjectives)
            {
                var newProgress = questFactory.Create_ObjectiveProgress(objective, this, true);
                mainObjectives.Add(newProgress);
            }

            if (CompletionOrderType == ObjectiveSetCompletionOrderType.Sequential)
            {
                ActivateMainObjective(0);
            }
            else if (CompletionOrderType == ObjectiveSetCompletionOrderType.NonSequential)
            {
                for (int i = 0; i < mainObjectives.Count; i++)
                {
                    ActivateMainObjective(i);
                }
            }
            #endregion

            #region  Activate Optional Objectives
            foreach (var objective in ObjectiveSet.optionalObjectives)
            {
                var newProgress = questFactory.Create_ObjectiveProgress(objective, this, false);
                optionalObjectives.Add(newProgress);

                ActivateObjective(newProgress, false);
            }
            #endregion
        }

        [TitleGroup("Debug Function")]
        [Button]
        public void ActivateMainObjective(int index)
        {
            if (index < 0 || index >= mainObjectives.Count)
            {
                //CustomDebug.Log($"ActivateMainObjective early return: index {index} out of range (0..{Math.Max(0, mainObjectives.Count - 1)}) for ObjectiveSet '{ObjectiveSetId}'");
                return;
            }

            var selected = mainObjectives[index];

            if (CompletionOrderType == ObjectiveSetCompletionOrderType.Sequential)
            {
                // First objective can always be activated
                if (index == 0)
                {
                    if (selected.completionState == CompletionState.CanBeTaken)
                    {
                        activeObjectiveIndex = 0;
                        ActivateObjective(selected);
                    }

                    //CustomDebug.Log($"ActivateMainObjective returning after handling first objective index {index} for ObjectiveSet '{ObjectiveSetId}'");
                    return;
                }

                // For other objectives, previous one must be completed
                var prev = mainObjectives[index - 1];
                if (prev.completionState != CompletionState.Completed)
                {
                    //CustomDebug.Log($"ActivateMainObjective cannot activate index {index} because previous objective at index {index - 1} is not completed (state {prev.completionState}) for ObjectiveSet '{ObjectiveSetId}'");
                    return;
                }

                if (selected.completionState == CompletionState.CanBeTaken)
                {
                    activeObjectiveIndex = index;
                    ActivateObjective(selected);
                }
            }
            else // NonSequential mode
            {
                if (selected.completionState == CompletionState.CanBeTaken)
                {
                    ActivateObjective(selected);
                }
            }
        }

        void ActivateObjective(ObjectiveProgressBase objective, bool isMainObjective = true)
        {
            if (isMainObjective)
                objective.OnObjectiveDone = () => MainObjectiveDoneHandler(objective);
            
            objective.OnObjectiveProgressed = () => ObjectiveProgressedHandler(objective);
            objective.Activate();
        }

        private void ObjectiveProgressedHandler(ObjectiveProgressBase objective)
        {
            OnObjectiveProgressed?.Invoke(objective);
        }

        private void MainObjectiveDoneHandler(ObjectiveProgressBase objective)
        {
            if (CompletionOrderType == ObjectiveSetCompletionOrderType.Sequential)
            {
                // Auto-activate the next objective only if this one was the active
                if (activeObjectiveIndex + 1 < mainObjectives.Count)
                {
                    ActivateMainObjective(activeObjectiveIndex + 1);
                }
            }

            AllMainObjectivesDone = mainObjectives.All(o => o.completionState == CompletionState.Completed);
            if (AllMainObjectivesDone)
            {
                activeObjectiveIndex = -1;
            }
        }

        public void ApplySaveData(ActiveObjectiveSet_SaveData saveData)
        {
            if (saveData == null)
            {
                //CustomDebug.Log($"ApplySaveData early return: saveData is null for ObjectiveSet '{ObjectiveSetId}'");
                return;
            }

            // if (saveData.objectiveSetId != ObjectiveSetId)
            // {
            //     CustomDebug.Log($"ApplySaveData for {ObjectiveSetId} failed because incoming save data id doesn't match");
            //     return;
            // }

            // --- Restore Main Objectives ---
            for (int i = 0; i < mainObjectives.Count; i++)
            {
                var selectedObjective = mainObjectives[i];
                var savedObjective = saveData.mainObjectives[i];

                // Sequential handling
                if (CompletionOrderType == ObjectiveSetCompletionOrderType.Sequential)
                {
                    if (i < saveData.activeObjectiveIndex)
                    {
                        // Already completed
                        selectedObjective.completionState = CompletionState.Completed;
                        selectedObjective.CurrentProgress = selectedObjective.RequiredAmount;
                    }
                    else if (i == saveData.activeObjectiveIndex)
                    {
                        // In-progress
                        selectedObjective.completionState = CompletionState.InProgress;
                        selectedObjective.CurrentProgress = savedObjective.currentProgress;
                        activeObjectiveIndex = i;

                        // Re-hook event & activate
                        ActivateObjective(selectedObjective);
                    }
                    else
                    {
                        // Not started yet
                        selectedObjective.completionState = CompletionState.CanBeTaken;
                        selectedObjective.CurrentProgress = 0;
                    }
                }
                else // NonSequential handling
                {
                    bool isCompleted = savedObjective.currentProgress >= savedObjective.requiredAmount;

                    if (isCompleted)
                    {
                        selectedObjective.completionState = CompletionState.Completed;
                        selectedObjective.CurrentProgress = selectedObjective.RequiredAmount;
                    }
                    else
                    {
                        selectedObjective.completionState = CompletionState.InProgress;
                        selectedObjective.CurrentProgress = savedObjective.currentProgress;

                        // Re-hook event & activate
                        ActivateObjective(selectedObjective);
                    }
                }
            }

            // --- Restore Optional Objectives (if needed) ---
            if (saveData.optionalObjectives != null && saveData.optionalObjectives.Count == optionalObjectives.Count)
            {
                for (int i = 0; i < optionalObjectives.Count; i++)
                {
                    var selectedObjective = optionalObjectives[i];
                    var savedObjective = saveData.optionalObjectives[i];
                    bool isCompleted = savedObjective.currentProgress >= savedObjective.requiredAmount;

                    if (isCompleted)
                    {
                        selectedObjective.completionState = CompletionState.Completed;
                        selectedObjective.CurrentProgress = selectedObjective.RequiredAmount;
                    }
                    else
                    {
                        selectedObjective.completionState = CompletionState.InProgress;
                        selectedObjective.CurrentProgress = savedObjective.currentProgress;

                        ActivateObjective(selectedObjective);
                    }
                }
            }

            // --- Update overall state ---
            AllMainObjectivesDone = mainObjectives.All(o => o.completionState == CompletionState.Completed);
            if (AllMainObjectivesDone)
            {
                activeObjectiveIndex = -1;
            }
        }
    }

    [HideReferenceObjectPicker]
    public class ActiveObjectiveSet_SaveData
    {
        //public string objectiveSetId;
        public ObjectiveSetCompletionOrderType completionOrderType;
        public int activeObjectiveIndex;
        [HideReferenceObjectPicker]
        public List<Objective_SaveData> mainObjectives;
        [HideReferenceObjectPicker]
        public List<Objective_SaveData> optionalObjectives;

        public ActiveObjectiveSet_SaveData() { }
        public ActiveObjectiveSet_SaveData(ActiveObjectiveSetProgress activeObjectiveSet)
        {
            if (activeObjectiveSet == null)
                return;

            //objectiveSetId = activeObjectiveSet.ObjectiveSetId;
            completionOrderType = activeObjectiveSet.CompletionOrderType;
            activeObjectiveIndex = activeObjectiveSet.activeObjectiveIndex;

            mainObjectives = new List<Objective_SaveData>();
            foreach (var objective in activeObjectiveSet.mainObjectives)
            {
                var newObjectiveSave = new Objective_SaveData()
                {
                    currentProgress = objective.CurrentProgress,
                    requiredAmount = objective.RequiredAmount,
                };

                mainObjectives.Add(newObjectiveSave);
            }

            optionalObjectives = new List<Objective_SaveData>();
            foreach (var objective in activeObjectiveSet.optionalObjectives)
            {
                var newObjectiveSave = new Objective_SaveData()
                {
                    currentProgress = objective.CurrentProgress,
                    requiredAmount = objective.RequiredAmount,
                };

                optionalObjectives.Add(newObjectiveSave);
            }
        }
    }

    [HideReferenceObjectPicker]
    public class Objective_SaveData
    {
        public float currentProgress;
        public float requiredAmount;

        public Objective_SaveData() { }
    }
}
