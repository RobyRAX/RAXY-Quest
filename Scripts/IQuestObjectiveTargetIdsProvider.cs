using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RAXY.QuestSystem
{
    public interface IQuestObjectiveTargetDatabase
    {
        public List<IQuestObjectiveTarget> QuestObjectiveTargets { get; }

#if UNITY_EDITOR
        public void RefreshTargetList()
        {
            ObjectiveData.TargetIds = QuestObjectiveTargets.Select(x => x.TargetId).ToList();
        }
#endif
    }
}
