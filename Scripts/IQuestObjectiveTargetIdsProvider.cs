using System.Collections.Generic;
using UnityEngine;

namespace RAXY.QuestSystem
{
    public interface IQuestObjectiveTargetIdsProvider
    {
        public List<string> QuestObjectiveTargetIds { get; }
    }
}
