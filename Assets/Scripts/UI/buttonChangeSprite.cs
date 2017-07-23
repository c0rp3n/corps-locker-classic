using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class buttonChangeSprite : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Sprite primaryImage;
    public Sprite secondaryImage;

    public void OnPointerEnter(PointerEventData eventData)
    {
        gameObject.GetComponent<Image>().sprite = secondaryImage;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        gameObject.GetComponent<Image>().sprite = primaryImage;
    }
}