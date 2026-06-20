
using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.QuestSystem
{
    public abstract class ObjectiveProgressBase
    {
        [HideInInspector]
        public Action OnObjectiveProgressed;

        [HideInInspector]
        public Action OnObjectiveDone;

        [HideInInspector]
        public ObjectiveData objectiveData;

        public bool IsMainObjective { get; set; }
        public ActiveObjectiveSetProgress ObjectiveSet { get; set; }

#if UNITY_EDITOR
        public string ObjectiveLabel => $"{objectiveData.ObjectiveLabel} → {_currentProgress}/{RequiredAmount} → {completionState}";
#endif

        [ShowInInspector]
        private float _currentProgress;
        public float CurrentProgress
        {
            get => _currentProgress;
            set
            {
                // clamp progress between 0 and requiredAmount
                float clamped = Mathf.Clamp(value, 0, RequiredAmount);

                // only assign if value changed
                if (Math.Abs(clamped - _currentProgress) < float.Epsilon)
                    return;

                _currentProgress = clamped;

                // Only set isDone once, when requirement is first reached
                if (completionState != CompletionState.Completed && _currentProgress >= RequiredAmount)
                {
                    OnCompleted(); // hook for derived classes if needed
                }
            }
        }

        public float RequiredAmount => objectiveData.requiredAmount;
        public CompletionState completionState;

        public virtual void Activate()
        {
            completionState = CompletionState.InProgress;
            Subscribe();
        }

        public abstract void Subscribe();
        public abstract void Unsubscribe();

        [Button]
        public virtual void AddProgress(float progress = 1)
        {
            if (completionState != CompletionState.InProgress)
                return;

            CurrentProgress += progress;
            OnObjectiveProgressed?.Invoke();
        }

        // optional virtual callback when completed
        protected virtual void OnCompleted()
        {
            completionState = CompletionState.Completed;
            // derived classes can react here
            OnObjectiveDone?.Invoke();
        }
    }
}
