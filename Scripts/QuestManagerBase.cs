using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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

        [TitleGroup("Ref")]
        [InfoBox("This object doesn't contain IQuestFactory", VisibleIf = "@ValidateQuestFactory", InfoMessageType = InfoMessageType.Error)]
        public GameObject QuestFactoryObj;

        bool ValidateQuestFactory =>
            QuestFactoryObj != null && QuestFactoryObj.GetComponent<IQuestFactory>() == null;

        [TitleGroup("Ref")]
        [field: NonSerialized]
        IQuestFactory _questFactory;

        public IQuestFactory QuestFactory
        {
            get
            {
                if (_questFactory == null && QuestFactoryObj != null &&
                    QuestFactoryObj.TryGetComponent(out IQuestFactory factory))
                {
                    _questFactory = factory;
                }

                return _questFactory;
            }
            set => _questFactory = value;
        }

        [TitleGroup("Ref")]
        [SerializeField]
        QuestDatabaseSO questDatabaseSO;
        public QuestDatabaseSO QuestDatabase => questDatabaseSO;

        [TitleGroup("All Quests")]
        [ShowInInspector]
        [DictionaryDrawerSettings(KeyLabel = "Quest Id", ValueLabel = "Quest Status")]
        [HideReferenceObjectPicker]
        public Dictionary<string, QuestStatus> AllQuestStatusDict = new();

        IReadOnlyDictionary<string, QuestStatus> IQuestManager.AllQuestStatusDict => AllQuestStatusDict;

        bool _allQuestInitialized;

        [TitleGroup("Active Quests")]
        [ShowInInspector]
        [DictionaryDrawerSettings(KeyLabel = "Quest Id", ValueLabel = "Progress")]
        [HideReferenceObjectPicker]
        public Dictionary<string, ActiveQuestProgress> ActiveQuestDict = new();

        IReadOnlyDictionary<string, ActiveQuestProgress> IQuestManager.ActiveQuestDict => ActiveQuestDict;

        [TitleGroup("Active Quests")]
        [ReadOnly]
        public string trackedQuest;

        string IQuestManager.TrackedQuest => trackedQuest;

        protected virtual void Awake()
        {
            if (QuestFactoryObj != null && QuestFactoryObj.TryGetComponent(out IQuestFactory factory))
            {
                QuestFactory = factory;
            }
        }

        [TitleGroup("All Quests")]
        [Button]
        public void InitAllQuest(bool force = false)
        {
            if (_allQuestInitialized && !force)
                return;

            foreach (var quest in QuestDatabase.Quests)
            {
                AllQuestStatusDict ??= new Dictionary<string, QuestStatus>();
                ActiveQuestDict ??= new Dictionary<string, ActiveQuestProgress>();

                if (!AllQuestStatusDict.ContainsKey(quest.QuestId))
                {
                    var newQuestStatus = new QuestStatus(quest, this);
                    AllQuestStatusDict.Add(quest.QuestId, newQuestStatus);
                }

                quest.QuestNameLoc.RefreshCacheAsync().Forget();
                quest.QuestDetailLoc.RefreshCacheAsync().Forget();
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
                QuestSO questSO = QuestDatabase.GetQuestSO(questId);
                var activeQuest = new ActiveQuestProgress(questSO, this);
                activeQuest.OnQuestCompleted = () => QuestCompletedHandler(questId);
                activeQuest.OnObjectiveProgressed = ObjectiveProgressedHandler;
                ActiveQuestDict.Add(questId, activeQuest);

                AllQuestStatusDict[questId].Set_InProgress();
                CustomDebug.Log($"Quest '{questId}' started successfully!");
                OnQuestTaken?.Invoke(questId);

                NotifyQuestObjectsRefresh();
            }

            if (string.IsNullOrEmpty(trackedQuest))
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
                trackedQuest = questId;
                OnTrackedQuestChanged?.Invoke(trackedQuest);
            }
        }

        [TitleGroup("Debug Function")]
        [Button]
        public void UntrackQuest()
        {
            trackedQuest = "";
            OnTrackedQuestChanged?.Invoke(trackedQuest);
        }

        void QuestCompletedHandler(string questId)
        {
            UntrackQuest();

            var questSO = QuestDatabase.GetQuestSO(questId);
            if (questSO.questCompleteAfterAllObjectives == false)
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

        public QuestStatus(QuestSO questSO, IQuestManager questManager)
        {
            requirements = new();

            CompletionState = CompletionState.RequirementNotMet;
            questId = questSO.QuestId;
            foreach (var requirement in questSO.requirements)
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
