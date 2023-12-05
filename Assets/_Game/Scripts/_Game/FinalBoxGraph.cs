using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FinalBoxGraph : MonoBehaviour
{
    #region Init

    public static FinalBoxGraph GetGraph { get; private set; }
    private void Awake()
    {
        if (GetGraph != null && GetGraph != this)
            Destroy(this);
        else
            GetGraph = this;
    }

    #endregion

    private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public LineSegment[] lineSegments;
    public GameObject[] topArrows;
    public TextMeshProUGUI[] topMeshes;
    public GameObject[] bottomArrows;
    public TextMeshProUGUI[] bottomMeshes;

    public void ShowGraph(int blueScore, int[] bids)
    {
        anim.SetTrigger("toggle");

        for (int i = 0; i < blueScore; i++)
            lineSegments[i].SetToColor(LineSegment.ColorOption.Blue);
        for(int i = blueScore; i < lineSegments.Length; i++)
            lineSegments[i].SetToColor(LineSegment.ColorOption.Red);

        for(int i = 0; i < topArrows.Length; i++)
        {
            topArrows[i].SetActive(false);
            topMeshes[i].text = "";
            bottomArrows[i].SetActive(false);
            bottomMeshes[i].text = "";
        }

        int wrongWrong = Limiter(blueScore - bids[0] + bids[1]);
        int wrongRight = Limiter(blueScore - bids[0] - bids[1]);
        int rightWrong = Limiter(blueScore + bids[0] + bids[1]);
        int rightRight = Limiter(blueScore + bids[0] - bids[1]);

        topArrows[wrongWrong].SetActive(true);
        topMeshes[wrongWrong].text = LabelGenerator("[BL]WRONG[/]/[RD]WRONG");

        bottomArrows[wrongRight].SetActive(true);
        bottomMeshes[wrongRight].text = LabelGenerator("[BL]WRONG[/]/[RD]RIGHT");

        topArrows[rightWrong].SetActive(true);
        topMeshes[rightWrong].text = LabelGenerator("[BL]RIGHT[/]/[RD]WRONG");

        bottomArrows[rightRight].SetActive(true);
        bottomMeshes[rightRight].text = LabelGenerator("[BL]RIGHT[/]/[RD]RIGHT");
    }

    public void HideGraph()
    {
        anim.SetTrigger("toggle");
    }

    int Limiter(int baseValue)
    {
        return baseValue < 0 ? 0 : baseValue > 15 ? 15 : baseValue;
    }

    string LabelGenerator(string baseLabel)
    {
        return baseLabel.Replace("[BL]", "<color=#00FFF3>").Replace("[/]", "</color>").Replace("[RD]", "<color=#F50000>");
    }
}
