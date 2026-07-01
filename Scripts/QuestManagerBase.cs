using System;
using System.Collections.Generic;
using RAXY.Utility;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.QuestSystem
{
    public abstract class QuestManagerBase : MonoBehaviour, IQuestManager
    {
        public event Action<string> OnTrackedQuestChanged;
        public event Action<string> OnQuestTaken;
        public event Action<string> OnQuestCompleted;
        public event Action<ObjectiveProgressBase> OnObjectiveProgressed;
        public event Action<ObjectiveProgressBase> OnObjectiveDone;

        public IQuestFactory QuestFactory { get; set; }
        public IQuestDatabase QuestDatabase { get; set; }

        [TitleGroup("All Quests")]
        [ShowInInspector]
        [DictionaryDrawerSettings(KeyLabel = "Quest Id", ValueLabel = "Quest Status")]
        [HideReferenceObjectPicker]
        public Dictionary<string, QuestStatus> AllQuestStatusDict { get; set; } = new();

        bool _allQuestInitialized;

        [TitleGroup("Active Quests")]
        [ShowInInspector]
        [DictionaryDrawerSettings(KeyLabel = "Quest Id", ValueLabel = "Progress")]
        [HideReferenceObjectPicker]
        public Dictionary<string, ActiveQuestProgress> ActiveQuestDict { get; set; } = new();

        [TitleGroup("Active Quests")]
        [ReadOnly]
        [ShowInInspector]
        public string TrackedQuest { get; set; }

        public void SetQuestDatabase(IQuestDatabase database)
        {
            QuestDatabase = database;
        }

        public void SetQuestFactory(IQuestFactory factory)
        {
            QuestFactory = factory;
        }

        [TitleGroup("All Quests")]
        [Button]
        public void InitAllQuest(bool force = false)
        {
            if (_allQuestInitialized && !force)
                return;

            foreach (var quest in QuestDatabase.AllQuests)
            {
                AllQuestStatusDict ??= new Dictionary<string, QuestStatus>();
                ActiveQuestDict ??= new Dictionary<string, ActiveQuestProgress>();

                if (!AllQuestStatusDict.ContainsKey(quest.QuestId))
                {
                    var newQuestStatus = new QuestStatus(quest, this);
                    AllQuestStatusDict.Add(quest.QuestId, newQuestStatus);
                }

                OnQuestInitialized(quest);
            }

            _allQuestInitialized = true;
        }

        [TitleGroup("Debug Function")]
        [Button]
        public void TakeQuest(string questId)
        {
            AllQuestStatusDict ??= new Dictionary<string, QuestStatus>();
            ActiveQuestDict ??= new Dictionary<string, ActiveQuestProgress>();

            if (!AllQuestStatusDict.ContainsKey(questId))
            {
                CustomDebug.Log($"QuestId '{questId}' not found in AllQuestStatusDict.");
                return;
            }

            var currentState = AllQuestStatusDict[questId].CompletionState;
            if (currentState == CompletionState.InProgress || currentState == CompletionState.Completed)
            {
                CustomDebug.Log($"QuestId '{questId}' is already {currentState}.");
                return;
            }

            if (currentState == CompletionState.RequirementNotMet)
            {
                CustomDebug.Log($"QuestId '{questId}' is not met its requirement yet.");
                return;
            }

            if (!ActiveQuestDict.ContainsKey(questId))
            {
                IQuest quest = QuestDatabase.GetQuest(questId);
                var activeQuest = new ActiveQuestProgress(quest, this);
                activeQuest.OnQuestCompleted = () => QuestCompletedHandler(questId);
                activeQuest.OnObjectiveProgressed = ObjectiveProgressedHandler;
                ActiveQuestDict.Add(questId, activeQuest);

                AllQuestStatusDict[questId].Set_InProgress();
                CustomDebug.Log($"Quest '{questId}' started successfully!");
                OnQuestTaken?.Invoke(questId);

                NotifyQuestObjectsRefresh();
            }

            if (string.IsNullOrEmpty(TrackedQuest))
            {
                SetQuestAsTracked(questId);
            }
        }

        [TitleGroup("Debug Function")]
        [Button]
        public void SetQuestAsTracked(string questId)
        {
            if (string.IsNullOrEmpty(questId))
                return;

            if (ActiveQuestDict.ContainsKey(questId))
            {
                TrackedQuest = questId;
                OnTrackedQuestChanged?.Invoke(TrackedQuest);
            }
        }

        [TitleGroup("Debug Function")]
        [Button]
        public void UntrackQuest()
        {
            TrackedQuest = "";
            OnTrackedQuestChanged?.Invoke(TrackedQuest);
        }

        void QuestCompletedHandler(string questId)
        {
            UntrackQuest();

            var quest = QuestDatabase.GetQuest(questId);
            if (quest.QuestCompleteAfterAllObjectives == false)
                return;

            CompleteQuest(questId);
        }

        [TitleGroup("Debug Function")]
        [Button]
        public void CompleteQuest(string questId)
        {
            UntrackQuest();

            AllQuestStatusDict[questId].Set_Completed();
            ActiveQuestDict.Remove(questId);

            OnQuestCompleted?.Invoke(questId);
            NotifyQuestObjectsRefresh();
            OnQuestCompletedWithRewards(questId);
        }

        void ObjectiveProgressedHandler(ObjectiveProgressBase objective)
        {
            if (objective.CurrentProgress >= objective.RequiredAmount)
            {
                OnObjectiveDone?.Invoke(objective);
            }

            OnObjectiveProgressed?.Invoke(objective);
        }

        public virtual void NotifyQuestObjectsRefresh() { }

        protected virtual void OnQuestInitialized(IQuest quest) { }

        protected virtual void OnQuestCompletedWithRewards(string questId) { }

        public static Color GetQuestListElementColor(int index, CompletionState completionState)
        {
            if (completionState == CompletionState.RequirementNotMet)
            {
                bool isEven = (index % 2 == 0);
                return isEven ? new Color(0.219f, 0.219f, 0.219f) : new Color(0.192f, 0.192f, 0.192f);
            }

            if (completionState == CompletionState.InProgress)
            {
                bool isEven = (index % 2 == 0);
                return isEven ? new Color(0.12f, 0.45f, 0.90f, 0.25f) : new Color(0.12f, 0.45f, 0.90f, 0.18f);
            }

            if (completionState == CompletionState.Completed)
            {
                bool isEven = (index % 2 == 0);
                return isEven ? new Color(0.12f, 0.90f, 0.45f, 0.25f) : new Color(0.12f, 0.90f, 0.45f, 0.18f);
            }

            bool defaultEven = (index % 2 == 0);
            return defaultEven ? new Color(0.219f, 0.219f, 0.219f) : new Color(0.192f, 0.192f, 0.192f);
        }
    }

    public enum CompletionState
    {
        None,
        RequirementNotMet,
        CanBeTaken,
        InProgress,
        Completed,
    }

    [HideReferenceObjectPicker]
    public class QuestStatus
    {
        public event Action OnRequirementMet;

        [HideInInspector]
        public string questId;

        [PropertyOrder(-1)]
        [ShowInInspector]
        public CompletionState CompletionState { get; private set; }

        [HideReferenceObjectPicker]
        public List<QuestRequirementBase> requirements;

        public bool RequirementMet { get; private set; }

        public QuestStatus() { }

        public QuestStatus(IQuest quest, IQuestManager questManager)
        {
            requirements = new();

            CompletionState = CompletionState.RequirementNotMet;
            questId = quest.QuestId;
            foreach (var requirement in quest.Requirements)
            {
                var newRequirement = questManager.QuestFactory.Create_QuestRequirement(requirement);
                newRequirement.Subscribe();

                newRequirement.OnValueChanged += (x, y) => RefreshCompletion();

                requirements.Add(newRequirement);
            }

            RefreshCompletion();
        }

        public void Set_InProgress()
        {
            CompletionState = CompletionState.InProgress;
            ClearRequirements();
        }

        public void Set_Completed()
        {
            CompletionState = CompletionState.Completed;
            ClearRequirements();
        }

        void ClearRequirements()
        {
            foreach (var requirement in requirements)
            {
                requirement.ClearAllListener();
            }

            requirements.Clear();
        }

        [Button]
        public void RefreshAllRequirements()
        {
            foreach (var requirement in requirements)
            {
                requirement.Refresh();
            }
        }

        [Button]
        private void RefreshCompletion()
        {
            RequirementMet = false;
            foreach (var requirement in requirements)
            {
                if (requirement.IsCompleted == false)
                    return;
            }

            RequirementMet = true;
            OnRequirementMet?.Invoke();
            if (CompletionState == CompletionState.RequirementNotMet)
                CompletionState = CompletionState.CanBeTaken;
        }
    }
}
