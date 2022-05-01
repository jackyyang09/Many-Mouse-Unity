using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(TextMeshProUGUI))]
public class GainTimeUI : MonoBehaviour {
    RectTransform rectTransform => (RectTransform)transform;
    TextMeshProUGUI text => GetComponent<TextMeshProUGUI>();
    Blinker blinker = new Blinker();

    public void Init (float timeGained) {
        text.text = string.Format("+{0:0.0}<size=65%>s", timeGained);
        blinker.cycles = 6;
        blinker.Start();
    }

    void Update () {
        blinker.Update();
        if(!blinker.playing) {
            // Before doing this, I'd like it to move towards the timer!
            Destroy(gameObject);
            // rectTransform.anchoredPosition += Vector2.up * Time.deltaTime * 100;
        } else {
            text.alpha = blinker.isOn ? 1f : 0f;
        }
    }
}
