using UnityEngine;

namespace RAXY.QuestSystem
{
    public interface IQuestObjectiveTarget
    {
        public string TargetId { get; }
        public string TargetName { get; }
        public string TargetSceneLocationName { get; }
        public Vector3 TargetWorldPosition { get; }
    }
}
