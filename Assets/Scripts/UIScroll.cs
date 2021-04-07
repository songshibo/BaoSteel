using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;

public class ResizeScrollEvent : UnityEvent<float> { }

public class UIScroll : MonoBehaviour, IScrollHandler
{
    public float minFactor = 1;
    public float maxFactor = 3;
    public bool wholeSizeFactor = true;

    private ResizeScrollEvent _onResize = new ResizeScrollEvent();
    private float _sizeFactor = 1f;

    public ResizeScrollEvent OnResize { get { return _onResize; } }
    public float SizeFactor
    {
        get
        {
            if (wholeSizeFactor)
                return Mathf.Round(_sizeFactor);
            return _sizeFactor;
        }
        set
        {
            SetFactor(value);
        }
    }

    [SerializeField]
    RectTransform content;
    [SerializeField]
    RectTransform viewport;

    Rect _rect;
    Vector2 _focusPos;

    void Start()
    {
        _rect = GetWorldRect(viewport);
        _focusPos = _rect.center;
    }

    public static Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] cors_ = new Vector3[4];
        rt.GetWorldCorners(cors_);
        Vector2 center = cors_[0];
        float width = Mathf.Abs(cors_[0].x - cors_[2].x);
        float height = Mathf.Abs(cors_[0].y - cors_[2].y);
        Rect rect_ = new Rect(center.x, center.y, width, height);
        return rect_;
    }

    private void SetFactor(float value)
    {
        value = ClampFactorValue(value);

        if (value != _sizeFactor)
        {
            _sizeFactor = value;
            ChangeSizeFactor(_sizeFactor);
            _onResize.Invoke(_sizeFactor);
        }
    }

    private void ChangeSizeFactor(float v)
    {
        Vector2 viewportSize_ = _rect.size;//此处有点问题，解决后更新
        //Vector2 viewportSize_ = viewport.rect.size;

        //缩放过程中的焦点位置
        //_focusPos = _rect.center;

        Rect contentRect_ = GetWorldRect(content);
        Vector2 contentSize_ = contentRect_.size;
        Vector2 contentCenter_ = contentRect_.center;

        Vector2 contentCenter2ViewportCenter_ = _focusPos - contentCenter_;
        float centerPosPercentX_ = contentCenter2ViewportCenter_.x / contentSize_.x;
        float centerPosPercentY_ = contentCenter2ViewportCenter_.y / contentSize_.y;

        Vector2 scorllSize_ = viewportSize_ + (v - 1) * viewportSize_ / 5;
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, scorllSize_.x);
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, scorllSize_.y);

        Vector2 sizeDelta_ = scorllSize_ - contentSize_;
        Vector2 posOffset_ = new Vector2(sizeDelta_.x * -centerPosPercentX_, sizeDelta_.y * -centerPosPercentY_);
        content.anchoredPosition += posOffset_;

        Vector3[] viewCorner_ = new Vector3[4];
        Vector3[] contentCorner_ = new Vector3[4];
        viewport.GetWorldCorners(viewCorner_);
        content.GetWorldCorners(contentCorner_);

        float xFixDelta_ = 0;
        float yFixDelta_ = 0;

        if (viewCorner_[0].x < contentCorner_[0].x) xFixDelta_ = viewCorner_[0].x - contentCorner_[0].x;
        if (viewCorner_[0].y < contentCorner_[0].y) yFixDelta_ = viewCorner_[0].y - contentCorner_[0].y;
        if (viewCorner_[2].x > contentCorner_[2].x) xFixDelta_ = viewCorner_[2].x - contentCorner_[2].x;
        if (viewCorner_[2].y > contentCorner_[2].y) yFixDelta_ = viewCorner_[2].y - contentCorner_[2].y;

        content.anchoredPosition += new Vector2(xFixDelta_, yFixDelta_);
    }

    private float ClampFactorValue(float value)
    {
        float factor_ = Mathf.Clamp(value, minFactor, maxFactor);

        if (wholeSizeFactor) factor_ = Mathf.Round(factor_);

        return factor_;
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (!isActiveAndEnabled) return;

        _focusPos = eventData.position;

        float delta_ = 0;
        if (Mathf.Abs(eventData.scrollDelta.x) > Mathf.Abs(eventData.scrollDelta.y))
            delta_ = eventData.scrollDelta.x;
        else delta_ = eventData.scrollDelta.y;

        SetFactor(_sizeFactor + delta_);
    }
}