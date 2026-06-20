using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.QuestSystem
{
    [HideReferenceObjectPicker]
    [System.Serializable]
    public class QuestObjective
    {
        [Header("Objective Info")]
        public string objectiveName;
        public string objectiveDescription;
        public int maxValue;
        
        [Header("Status")]
        [ReadOnly] public int currentValue;
        [ReadOnly] public bool isCompleted;
        
        public QuestObjective(string name, string description, int max)
        {
            objectiveName = name;
            objectiveDescription = description;
            maxValue = max;
            currentValue = 0;
            isCompleted = false;
        }
        
        public void UpdateProgress(int amount = 1)
        {
            currentValue = Mathf.Min(currentValue + amount, maxValue);
            if (currentValue >= maxValue && !isCompleted)
            {
                isCompleted = true;
            }
        }
        
        public void SetProgress(int value)
        {
            currentValue = Mathf.Clamp(value, 0, maxValue);
            if (currentValue >= maxValue && !isCompleted)
            {
                isCompleted = true;
            }
        }
        
        public string GetProgressText()
        {
            return $"{currentValue}/{maxValue}";
        }
        
        public float GetProgressPercentage()
        {
            return maxValue > 0 ? (float)currentValue / maxValue : 0f;
        }
    }    
}
