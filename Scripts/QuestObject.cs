using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace RAXY.QuestSystem
{
    public class QuestObject : MonoBehaviour, IQuestObject
    {
        #region IQuestObject
        [TitleGroup("Quest Object")]
        [HideLabel]
        [SerializeField] IQuestObjectInputData inputData;

        public GameObject GetGameObject => gameObject;
        public bool ShouldActive { get; set; }

        public QuestObjectDefaultState DefaultState => inputData.DefaultState;
        public List<QuestObjectVisibilityCondition> FlipStateConditions => inputData.FlipStateConditions;
        public UnityEvent OnStateFlipped => inputData.OnStateFlipped;
        public UnityEvent OnStateReverted => inputData.OnStateReverted;
        #endregion
    }
}
