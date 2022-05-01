using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(TextMeshProUGUI))]
public class GainScoreUI : MonoBehaviour {
    RectTransform rectTransform => (RectTransform)transform;
    TextMeshProUGUI text => GetComponent<TextMeshProUGUI>();
    public void Init (Vector3 targetPosition, int score, int multiplier) {
        var screenPosition = Game.Instance.camera.WorldToScreenPoint(targetPosition);
        RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform)rectTransform.parent, screenPosition, rectTransform.GetComponentInParent<Canvas>().rootCanvas.worldCamera, out Vector3 worldPoint);
        rectTransform.position = worldPoint;
        
        text.text = string.Format("{0}<size=55%>x<size=75%>{1}", score, multiplier);
        text.transform.localScale = Vector3.one * Mathf.Lerp(0.5f,2,Mathf.InverseLerp(0,250,score));
        text.CrossFadeAlpha(1f, 0f, true);
        text.CrossFadeAlpha(0f, 0.5f, true);
        Destroy(gameObject, 0.5f);
    }

    void Update () {
        rectTransform.anchoredPosition += Vector2.up * Time.deltaTime * 100;
    }
}
