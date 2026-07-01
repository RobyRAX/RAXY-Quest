using System.Collections.Generic;
using RAXY.InventorySystem;

namespace RAXY.QuestSystem
{
    public interface IQuest
    {
        public string QuestId { get; }
        public string QuestName { get; }
        public string QuestDetail { get; }
        public QuestType QuestType { get; }
        public List<QuestRequirementData> Requirements { get; }
        public bool QuestCompleteAfterAllObjectives { get; }
        public List<ObjectiveSet> ObjectiveSets { get; }
        public bool GiveRewardOnQuestComplete { get; }
        public List<ItemAmountContainer> QuestRewards { get; }
    }
}
