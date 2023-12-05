using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class TextAppear : MonoBehaviour
{
    public float AppearSpeed = 1.0f;
    public float CharacterSpeed = 1.0f;
    public float XOffset = 10.0f;
    public float YOffset = 0.0f;

    private TMP_Text _text = null;
    private Coroutine _animation = null;
    private string _previousText = null;

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        DoAnimation();
    }

    private void LateUpdate()
    {
        if (_text.text != _previousText)
        {
            DoAnimation();
        }
    }

    private void DoAnimation()
    {
        if (_animation != null)
        {
            StopCoroutine(_animation);
        }

        _animation = StartCoroutine(AnimationCoroutine());
    }

    private IEnumerator AnimationCoroutine()
    {
        _previousText = _text.text;
        _text.ForceMeshUpdate();

        TMP_TextInfo textInfo = _text.textInfo;
        int charCount = textInfo.characterCount;

        float totalTime = (charCount / AppearSpeed) + CharacterSpeed;

        for (float time = 0; time < totalTime; time += Time.deltaTime)
        {
            DoAnimationForTime(time, charCount, textInfo);
            _text.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices | TMP_VertexDataUpdateFlags.Colors32);
            yield return null;
        }

        DoAnimationForTime(totalTime, charCount, textInfo);
        _text.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices | TMP_VertexDataUpdateFlags.Colors32);
    }

    private void DoAnimationForTime(float time, int charCount, TMP_TextInfo textInfo)
    {
        for (int charIndex = 0; charIndex < charCount; ++charIndex)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[charIndex];
            if (!charInfo.isVisible)
            {
                continue;
            }

            int materialIndex = charInfo.materialReferenceIndex;
            Vector3[] verts = textInfo.meshInfo[materialIndex].vertices;
            Color32[] colors = textInfo.meshInfo[materialIndex].colors32;

            float delta = Mathf.InverseLerp(0, CharacterSpeed, time - (charIndex / AppearSpeed));
            int vertIndex = charInfo.vertexIndex;

            Vector3 offset = Vector3.right * Mathf.SmoothStep(XOffset, 0, delta) + Vector3.up * Mathf.SmoothStep(YOffset, 0, delta);

            verts[vertIndex] = charInfo.bottomLeft + offset;
            verts[vertIndex + 1] = charInfo.topLeft + offset;
            verts[vertIndex + 2] = charInfo.topRight + offset;
            verts[vertIndex + 3] = charInfo.bottomRight + offset;

            byte alpha = (byte)(delta * 255);
            colors[vertIndex].a = alpha;
            colors[vertIndex + 1].a = alpha;
            colors[vertIndex + 2].a = alpha;
            colors[vertIndex + 3].a = alpha;
        }
    }
}
