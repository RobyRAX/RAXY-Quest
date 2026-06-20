using System.Collections.Generic;

namespace RAXY.QuestSystem
{
    public interface IQuestObjectManager
    {
        IReadOnlyList<IQuestObject> QuestObjects { get; }
        void RefreshQuestObjectState();
    }
}
