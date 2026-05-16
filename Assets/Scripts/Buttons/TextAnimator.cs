using DG.Tweening;
using System;
using UnityEngine;

public class TextAnimator : MonoBehaviour
{
    public float delay = 1f;
    void Start()
    {
        GetComponent<RectTransform>()
            .DOAnchorPosX(-800f, 3f)
            .From(true).SetDelay(delay)
            .SetEase(Ease.InOutSine);
    }
    void Update()
    {
        
    }
}
