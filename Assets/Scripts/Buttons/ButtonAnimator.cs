using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private bool isColorAnim = true;
    [SerializeField] private bool isMoveAnim = true;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(0f, 1f, 0f); 
    [SerializeField] private float durationColor = 0.3f;
    [SerializeField] private float moveDuration = 1f;

    private Image targetImage;
    void Start()
    {
        targetImage = GetComponent<Image>();
        if (targetImage != null)
        {
            targetImage.color = normalColor;
        }

        if (isMoveAnim)
        GetComponent<RectTransform>()
                    .DOAnchorPosY(-400f, 3f)
                    .From(true) 
                    .SetDelay(moveDuration)
                    .SetEase(Ease.InOutSine);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetImage != null)
        {
            targetImage.DOKill();
            targetImage.DOColor(hoverColor, durationColor);
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetImage != null)
        {
            targetImage.DOKill();
            targetImage.DOColor(normalColor, durationColor);
        }
    }
}
