using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.QuestSystem
{
    public abstract class QuestRequirementBase
    {
        public event Action<float, float> OnValueChanged;

        [TitleGroup("Progress")]
        [ShowInInspector]
        public virtual float RequiredValue { get; }
        
        float _currentValue;
        [TitleGroup("Progress")]
        [ShowInInspector]
        public float CurrentValue
        {
            get => _currentValue;
            set
            {
                _currentValue = value;
                Fire_ValueChanged();
            }
        }

        [TitleGroup("Progress")]
        [ShowInInspector]
        public bool IsCompleted => CurrentValue >= RequiredValue;

        public void Fire_ValueChanged() => OnValueChanged?.Invoke(CurrentValue, RequiredValue);

        public abstract void Subscribe();
        public abstract void Unsubscribe();

        [Button]
        public virtual void Refresh() { }

        public void ClearAllListener()
        {
            OnValueChanged = null;
        }
    }
}
