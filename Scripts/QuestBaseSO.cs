using System.Collections.Generic;
using RAXY.InventorySystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.QuestSystem
{
    public abstract class QuestBaseSO : ScriptableObject, IQuest
    {
        public virtual string QuestId => name;

        public virtual string QuestName => name;
        public virtual string QuestDetail => name;
        public virtual QuestType QuestType => QuestType.Main;
        public abstract List<QuestRequirementData> Requirements { get; }
        public abstract bool QuestCompleteAfterAllObjectives { get; }
        public abstract List<ObjectiveSet> ObjectiveSets { get; }
        public virtual bool GiveRewardOnQuestComplete => false;
        public virtual List<ItemAmountContainer> QuestRewards => null;

        [TitleGroup("Editor")]
        [SerializeField]
        [PropertyOrder(2)]
        Object questObjectiveTargetDatabaseObj;

        [TitleGroup("Editor")]
        [Button]
        [PropertyOrder(2)]
        void Refresh()
        {
            QuestObjectiveTargetDatabase?.RefreshTargetList();
        }

        public IQuestObjectiveTargetDatabase QuestObjectiveTargetDatabase
        {
            get
            {
                if (questObjectiveTargetDatabaseObj is IQuestObjectiveTargetDatabase db)
                    return db;
                else
                    return null;
            }
        }
    }
}
