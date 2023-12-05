using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shapes;

public class ClockArm : MonoBehaviour
{
    public enum NodeColor { Blue, Red, Off };
    public enum LineColor { Blue, Red };
    public Animator armAnim;
    public Color[] colors;
    public Disc node;
    private bool lit = false;
    public Material[] lineMats;
    public Renderer animatingLineRend;

    public LineColor currentFinalOccupancy;

    public void ToggleLightColor()
    {
        if (ClockManager.inFinal)
            return;

        lit = !lit;
        if (lit)
            node.Color = colors[ClockManager.fastTimer ? (int)NodeColor.Red : (int)NodeColor.Blue];
        else
            node.Color = colors[(int)NodeColor.Off];
    }

    public void SetLineColor(LineColor col)
    {
        animatingLineRend.material = lineMats[(int)col];
    }

    public void SetNodeColor(NodeColor col)
    {
        node.Color = colors[(int)col];
    }

    public IEnumerator FinalArmChange(NodeColor nodeColor, LineColor lineColor)
    {
        AudioManager.GetAudioManager.PlayUnique(AudioManager.OneShotClip.ClockArm);
        currentFinalOccupancy = lineColor;
        animatingLineRend.material = lineMats[(int)lineColor];
        armAnim.speed = 2f;
        armAnim.SetTrigger("toggle");
        yield return new WaitForSeconds(0.5f);
        node.Color = colors[(int)nodeColor];
        yield return new WaitForSeconds(0.1f);
        armAnim.SetTrigger("toggle");
    }
}
