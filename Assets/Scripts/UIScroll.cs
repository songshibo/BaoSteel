using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;


public class UIScroll : MonoBehaviour
{
    public float minFactor = 1;
    public float maxFactor = 10;
    public RectTransform viewport;
    public RectTransform contain;

    private float level; // 支持的缩放等级数
    private float amount; // 滚轮的数值


    private void Start()
    {
        level = maxFactor - minFactor + 1;
    }

    private void Update()
    {
        if (RectTransformUtility.RectangleContainsScreenPoint(viewport, Input.mousePosition)) // 鼠标进入viewport，才能进行缩放
        {
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
        }
        
    }
}