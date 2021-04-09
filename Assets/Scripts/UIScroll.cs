using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;


public class UIScroll : MonoBehaviour, IScrollHandler
{
    public float minFactor = 1;
    public float maxFactor = 10;
    public RectTransform viewport;
    public RectTransform contain;

    private float level; // 支持的缩放等级数
    private float amount; // 滚轮的数值
    private UnityEvent scroll = new UnityEvent();


    private void Start()
    {
        level = maxFactor - minFactor + 1;
        scroll.AddListener(delegate () { ScrollEvent(); });
    }

    private void ScrollEvent()
    {
        //float delX = Input.mousePosition.x - contain.position.x;
        //float delY = Input.mousePosition.y - contain.position.y;
        
        //float scaleX = delX / GetComponent<RectTransform>().rect.width / transform.localScale.x;
        //float scaleY = delY / GetComponent<RectTransform>().rect.height / transform.localScale.y;

        //if ((Input.GetAxis("Mouse ScrollWheel") > 0) && (transform.localScale.x < 2))
        //{
        //    transform.localScale += Vector3.one * 0.1f;
        //}
        //else if ((Input.GetAxis("Mouse ScrollWheel") < 0) && (transform.localScale.x > 1f))
        //{
        //    transform.localScale += Vector3.one * -0.1f;
        //}

        //GetComponent<RectTransform>().pivot += new Vector2(scaleX, scaleY);
        //transform.position += new Vector3(delX, delY, 0);
    }

    private void Update()
    {
        //if (RectTransformUtility.RectangleContainsScreenPoint(viewport, Input.mousePosition)) // 鼠标进入viewport，才能进行缩放
        //{
        //    amount = Input.GetAxisRaw("Mouse ScrollWheel");

        //    float newScale = contain.localScale.x;
        //    if (amount > 0)
        //    {
        //        newScale = Math.Min(maxFactor, newScale + amount * level); // 新的缩放值不能超过最大缩放值
        //    }
        //    else if (amount < 0)
        //    {
        //        newScale = Math.Max(minFactor, newScale + amount * level);
        //    }
        //    contain.localScale = new Vector3(newScale, newScale, 1);
        //}
        //if (transform.localScale.x == 1f)
        //{
        //    GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        //    transform.localPosition = Vector3.zero;
        //}
    }

    public void OnScroll(PointerEventData eventData)
    {
        scroll.Invoke();
    }
}