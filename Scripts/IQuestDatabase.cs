using System.Collections.Generic;

namespace RAXY.QuestSystem
{
    public interface IQuestDatabase
    {
        public List<IQuest> AllQuests { get; }
        public IQuest GetQuest(string questId);
    }
}
