using UnityEngine;

namespace RAXY.QuestSystem
{
    public interface IQuestFactory
    {
        public ObjectiveProgressBase Create_ObjectiveProgress(ObjectiveData objective, ActiveObjectiveSetProgress objectiveSet, bool isMainObjective);
        public QuestRequirementBase Create_QuestRequirement(QuestRequirementData requirement);
    }
}
