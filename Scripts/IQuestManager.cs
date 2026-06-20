using System;
using System.Collections.Generic;

namespace RAXY.QuestSystem
{
    public interface IQuestManager
    {
        IQuestFactory QuestFactory { get; }
        QuestDatabaseSO QuestDatabase { get; }

        IReadOnlyDictionary<string, QuestStatus> AllQuestStatusDict { get; }
        IReadOnlyDictionary<string, ActiveQuestProgress> ActiveQuestDict { get; }
        string TrackedQuest { get; }

        event Action<string> OnTrackedQuestChanged;
        event Action<string> OnQuestTaken;
        event Action<string> OnQuestCompleted;
        event Action<ObjectiveProgressBase> OnObjectiveProgressed;
        event Action<ObjectiveProgressBase> OnObjectiveDone;

        void InitAllQuest(bool force = false);
        void TakeQuest(string questId);
        void CompleteQuest(string questId);
        void SetQuestAsTracked(string questId);
        void UntrackQuest();
        void NotifyQuestObjectsRefresh();
    }
}
