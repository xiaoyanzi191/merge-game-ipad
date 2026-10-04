using System;
using System.Collections.Generic;
using Core.GridPawns.Data;
using Core.GridPawns.Effect;
using Core.GridPawns.Enum;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core.GridPawns
{
    public class Producer : GridPawn
    {
        [field: SerializeField] public SpriteRenderer CapacitySprite { get; private set; }

        [field: SerializeField] public ProducerType ProducerType { get; set; }
        // Old saves may contain zero capacity. Unlimited production ignores that value.
        public int Capacity { get => int.MaxValue; set { } }
        [field: SerializeField] public ApplianceType GeneratedApplianceType { get; set; }

        // Override PawnEffect to return ProducerEffect
        public new ProducerEffect PawnEffect => (ProducerEffect)base.PawnEffect;

        private Dictionary<int, float> _generatingRatioDict { get; set; }

        public override System.Enum Type
        {
            get => ProducerType;
            protected set => ProducerType = (ProducerType)value;
        }

        public override void ApplyData(GridPawnLevelDataSO levelData)
        {
            base.ApplyData(levelData);
            var producerData = levelData as ProducerLevelDataSO;
            if (producerData is null)
            {
                throw new InvalidOperationException("Invalid data type provided!");
            }

            SpriteRenderer.sprite = producerData.ProducerSprite;
            if (CapacitySprite != null) CapacitySprite.enabled = false;
            GeneratedApplianceType = producerData.GeneratedApplianceType;
            _generatingRatioDict = producerData.GeneratingRatioDict;
        }

        // Kept for existing callers and save compatibility; no timer or decrement.
        public void ReduceCapacity() { }

        public override string ToString()
        {
            return $"Column{Coordinate.x},Row{Coordinate.y}, Level:{Level}, Type:{ProducerType}";
        }

        /// <summary> Selects an appliance level to produce based on the probability ratios. </summary>
        public int GetApplianceLevelToProduce()
        {
            if (_generatingRatioDict == null || _generatingRatioDict.Count == 0)
            {
                Debug.LogError("GeneratingRatioDict is empty! Returning default level 1.");
                return 1; // Default level
            }

            float randomValue = Random.value; // Random float between 0.0 - 1.0
            float cumulativeProbability = 0f;

            foreach (var kvp in _generatingRatioDict)
            {
                cumulativeProbability += kvp.Value;

                if (randomValue <= cumulativeProbability)
                {
                    return kvp.Key; // Return selected appliance level
                }
            }

            Debug.LogError("No valid appliance level found in GeneratingRatioDict. Returning default.");
            return 1; // Fallback default level
        }
    }
}