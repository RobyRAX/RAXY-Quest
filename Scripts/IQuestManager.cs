using System;
using System.Collections.Generic;

namespace RAXY.QuestSystem
{
    public interface IQuestManager
    {
        public IQuestFactory QuestFactory { get; }
        public IQuestDatabase QuestDatabase { get; }

        public Dictionary<string, QuestStatus> AllQuestStatusDict { get; set; }
        public Dictionary<string, ActiveQuestProgress> ActiveQuestDict { get; set; }
        public string TrackedQuest { get; set; }

        public event Action<string> OnTrackedQuestChanged;
        public event Action<string> OnQuestTaken;
        public event Action<string> OnQuestCompleted;
        public event Action<ObjectiveProgressBase> OnObjectiveProgressed;
        public event Action<ObjectiveProgressBase> OnObjectiveDone;

        public void InitAllQuest(bool force = false);
        public void TakeQuest(string questId);
        public void CompleteQuest(string questId);
        public void SetQuestAsTracked(string questId);
        public void UntrackQuest();
        public void NotifyQuestObjectsRefresh();
    }
}
