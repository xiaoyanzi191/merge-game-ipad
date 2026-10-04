using UnityEngine;

namespace Core.GridPawns.Effect
{
    public class ProducerEffect : GridPawnEffect
    {
        [field: SerializeField] public SpriteRenderer CapacitySprite { get; private set; }
        private void OnEnable() { if (CapacitySprite != null) CapacitySprite.enabled = false; }
    }
}
