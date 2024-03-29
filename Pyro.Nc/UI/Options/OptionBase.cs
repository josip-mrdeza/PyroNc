using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Pyro.Nc.UI.Options;

public class OptionBase : MonoBehaviour
{
    private void Awake()
    {
        List<RectTransform> children = new List<RectTransform>();
        var cc = transform.childCount;
        for (int i = 0; i < cc; i++)
        {
            children.Add((RectTransform) transform.GetChild(i));
        }

        _children = children.ToArray();
    }

    public float Width
    {
        get => _width;
        set
        {
            _width = value;
            SetWidth(value);
        }
    }
    public float Height
    {
        get => _height;
        set
        {
            _height = value;
            SetHeight(value);
        }
    }
    internal RectTransform _uiTransform;
    internal RectTransform[] _children;
    internal float _height;
    internal float _width;
    internal Vector2 Position
    {
        get => _uiTransform.position;
        set => _uiTransform.position = value;
    }
    public void SetWidth(float width)
    {
        var size = new Vector2(width, Height);
        _uiTransform.sizeDelta = size;
    }
    public void SetHeight(float height)
    {
        var size = new Vector2(Width, height);
        _uiTransform.sizeDelta = size;
    }
    public void SetSize(Vector2 sizeDelta)
    {
        _uiTransform.sizeDelta = sizeDelta;
        _width = sizeDelta.x;
        _height = sizeDelta.y;
        foreach (var child in _children)
        {
            child.sizeDelta = sizeDelta;
        }
    }
    public void Move(Vector2 position)
    {
        _uiTransform.localPosition = position;
    }
    public virtual void Init(){}
    public enum Side
    {
        Left, Middle, Right
    }
}