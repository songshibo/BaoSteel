using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;


public class UIScroll : MonoBehaviour, IScrollHandler, IPointerEnterHandler, IPointerExitHandler
{
    public float minFactor = 1;
    public float maxFactor = 10;
    public RectTransform viewport;
    public RectTransform contain;

    private float level; // 支持的缩放等级数
    private float amount; // 滚轮的数值
    private UnityEvent scroll = new UnityEvent();
    private bool isShowPosition;


    private void Start()
    {
        isShowPosition = false;
        level = maxFactor - minFactor + 1;
        scroll.AddListener(delegate () { ScrollEvent(); });
    }

    private void ScrollEvent()
    {
        float delX = Input.mousePosition.x - contain.position.x;
        float delY = Input.mousePosition.y - contain.position.y;

        float scaleX = delX / contain.rect.width / contain.localScale.x;
        float scaleY = delY / contain.rect.height / contain.localScale.y;

        amount = Input.GetAxisRaw("Mouse ScrollWheel");

        float newScale = contain.localScale.x;
        if (amount > 0)
        {
            newScale = Math.Min(maxFactor, newScale + amount * level); // 新的缩放值不能超过最大缩放值
        }
        else if (amount < 0)
        {
            newScale = Math.Max(minFactor, newScale + amount * level);
        }
        contain.localScale = new Vector3(newScale, newScale, 1);

        contain.pivot += new Vector2(scaleX, scaleY);
        contain.position += new Vector3(delX, delY, 0);
    }

    public void OnScroll(PointerEventData eventData)
    {
        scroll.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isShowPosition = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isShowPosition = false;
    }

    private void OnGUI()
    {
        if (isShowPosition)
        {
            
            float dx = Input.mousePosition.x - contain.position.x;
            float dy = Input.mousePosition.y - contain.position.y;

            float pos_x = dx + contain.rect.x * contain.localScale.x * -1;
            float pos_y = dy + contain.rect.y * contain.localScale.y * -1;

            string angle = Math.Round(Math.Min(360, 360f * pos_x / (contain.rect.width * contain.localScale.x)), 0).ToString();
            string height = Math.Round(55 * pos_y / (contain.rect.height * contain.localScale.y), 0).ToString();

            string content = "角度：" + angle + "°\n高度：" + height + "m";

            GUI.skin.box.fontSize = 23;
            GUI.Box(new Rect(Input.mousePosition.x - 150, Screen.height - Input.mousePosition.y + 20, 150, 70), content);
        }
    }

}