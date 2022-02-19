using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ManyMouseUnity;

namespace ManyMouseExample
{
    public class ManyMouseCrosshair : MonoBehaviour
    {
        [SerializeField] float cursorSpeed = 10;
        [SerializeField] UnityEngine.UI.Image selectionImage = null;

        RectTransform rectTransform { get { return transform as RectTransform; } }

        public ManyMouse Mouse { get { return mouse; } }
        ManyMouse mouse;

        /// <summary>
        /// A more savvy way to initialize may be to use a ManyMouse reference rather than 
        /// an ID, but this is to showcase the API
        /// </summary>
        /// <param name="id"></param>
        public void Initialize(int id)
        {
            if (ManyMouseWrapper.MouseCount > id)
            {
                // If re-initializing, unsubscribe first
                if (mouse != null)
                {
                    mouse.OnMouseDeltaChanged -= UpdateDelta;
                    mouse.OnMousePositionChanged -= UpdatePosition;
                }

                mouse = ManyMouseWrapper.GetMouseByID(id);
                Debug.Log(gameObject.name + " connected to mouse: " + mouse.DeviceName);

                mouse.OnMouseDeltaChanged += UpdateDelta;
                mouse.OnMousePositionChanged += UpdatePosition;
            }
            else
            {
                Debug.Log("Mouse ID " + id + " not found. Plug in an extra mouse?");
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            ManyMouse.OnAnyMouseUpdated += HighlightLastUpdated;
        }

        private void OnDisable()
        {
            ManyMouse.OnAnyMouseUpdated -= HighlightLastUpdated;
            if (mouse != null)
            {
                mouse.OnMouseDeltaChanged -= UpdateDelta;
                mouse.OnMousePositionChanged -= UpdatePosition;
            }
        }

        private void UpdatePosition(Vector2 Pos)
        {
            float x = Mathf.Clamp(mouse.Position.x * Screen.currentResolution.width, 0, Screen.currentResolution.width);
            float y = Mathf.Clamp(mouse.Position.y * Screen.currentResolution.height, -Screen.currentResolution.height, 0);
            rectTransform.anchoredPosition = new Vector2(x, y);
        }

        /// <summary>
        /// There's many ways you can extend this code to get the "true mouse delta" based 
        /// on current screen dimensions and window size, but for the purpose of demonstration,
        /// this is enough. Especially considering that we hide the mouse cursor and assume the
        /// game is running in full-screen
        /// </summary>
        /// <param name="Delta"></param>
        private void UpdateDelta(Vector2 Delta)
        {
            Vector2 delta = mouse.Delta * cursorSpeed * Time.deltaTime;
            rectTransform.anchoredPosition += delta;
            rectTransform.anchoredPosition = new Vector2
                (
                Mathf.Clamp(rectTransform.anchoredPosition.x, 0, Screen.currentResolution.width),
                Mathf.Clamp(rectTransform.anchoredPosition.y, -Screen.currentResolution.height, 0)
                );
        }

        private void HighlightLastUpdated(ManyMouse obj)
        {
            selectionImage.enabled = obj == mouse;
        }
    }
}