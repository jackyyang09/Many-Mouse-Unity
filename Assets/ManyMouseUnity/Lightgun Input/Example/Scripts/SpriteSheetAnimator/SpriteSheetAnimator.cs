using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteSheetAnimator : MonoBehaviour {
    public Sprite[] sprites;
    public SpriteRenderer spriteRenderer;
    public Image image;
    public bool playing = true;
    public float FPS = 30;
    [SerializeField]
    float currentFrame;
    int currentFrameIndex => Mathf.FloorToInt(currentFrame);
    int loopedCurrentFrameIndex => currentFrameIndex % sprites.Length;
    public LoopBehaviour loopBehaviour;
    public enum LoopBehaviour {
        Repeat,
        Stop,
        Destroy
    }

    public void Stop () {
        currentFrame = 0;
        playing = false;
    }

    void OnEnable () {
        Render();
    }

    void Update() {
        if(playing) {
            currentFrame += Time.deltaTime * FPS;
            if(currentFrameIndex > sprites.Length - 1) {
                if(loopBehaviour == LoopBehaviour.Stop) {
                    Stop();
                } else if (loopBehaviour == LoopBehaviour.Repeat) {
                    // currentFrameIndex = 0;
                    // currentFrame = 0;
                } else if (loopBehaviour == LoopBehaviour.Destroy) {
                    Destroy(gameObject);
                }
            }
        }
        Render();
    }

    void Render () {
        if(image != null) image.sprite = sprites[loopedCurrentFrameIndex];
        if(spriteRenderer != null) spriteRenderer.sprite = sprites[loopedCurrentFrameIndex];
    }
}