using System;
using UnityEngine;

public class Bubble : BaseItem
{
    private Vector3 originSize;

    private void Awake()
    {
        originSize = transform.localScale;
    }

    public void SetSize(float size)
    {
        transform.localScale = originSize * size;
    }
}
