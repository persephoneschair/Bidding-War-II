using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LineSegment : MonoBehaviour
{
    private RawImage line;
    public enum ColorOption { Blue, Red };
    public Color[] colors;

    private void Awake()
    {
        line = GetComponent<RawImage>();
    }

    public void SetToColor(ColorOption col)
    {
        line.color = colors[(int)col];
    }
}
