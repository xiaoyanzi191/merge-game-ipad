using System;
using DG.Tweening;
using UnityEngine;

namespace Core.GridPawns.Effect
{
    public class GridPawnEffect : MonoBehaviour
    {
        [field: SerializeField] public SpriteRenderer FocusSprite { get; private set; }
        [field: SerializeField] public SpriteRenderer LastLevelSprite { get; private set; }
        private Tween _shiftTween;

        protected virtual void OnDisable()
        {
            _shiftTween?.Kill();
            _shiftTween = null;
            transform.DOKill();
            if (FocusSprite != null)
            {
                FocusSprite.transform.DOKill();
                FocusSprite.transform.localScale = Vector3.zero;
            }
            if (LastLevelSprite != null) LastLevelSprite.transform.DOKill();
            transform.localScale = Vector3.one;
        }

        public void SetLastLevel(bool isLast)
        {
            LastLevelSprite.transform.DOKill();

            if (isLast)
            {
                LastLevelSprite.transform.DOScale(1f, 0.15f);
            }
            else
            {
                LastLevelSprite.transform.localScale = Vector3.zero;
            }
        }

        public void SetFocus(bool isFocusing)
        {
            transform.DOKill();
            FocusSprite.transform.DOKill();

            if (isFocusing)
            {
                transform.localScale = Vector3.one;

                FocusSprite.transform.DOScale(1f, 0.15f);
                transform.DOScale(0.9f, 0.15f).SetLoops(2, LoopType.Yoyo);
            }
            else
            {
                transform.localScale = Vector3.one;
                FocusSprite.transform.localScale = Vector3.zero;
            }
        }

        public void Shift(Vector3 position, Action a, float animTime = 0.3f)
        {
            _shiftTween?.Kill(); // Ensure only one tween sequence is active
            _shiftTween = transform.DOMove(position, animTime).SetEase(Ease.OutQuad)
                .OnComplete(a.Invoke)
                .OnKill(a.Invoke);
        }
    }
}