using UnityEngine;
using ManyMouseUnity;

namespace LightgunEngine
{
    [System.Serializable]
    public class Lightgun
    {
        // The raw ManyMouse input used to determine the position of the lightgun. ManyMouse is not a very good tool, so this is private to prevent accessing properties that don't work as expected.
        ManyMouse manyMouseInput;

        /// <summary>
        /// This is the viewport position of the lightgun's "mouse" inside the game window. Works in game and in editor.
        /// </summary>
        public Vector2 ViewportPosition
        {
            get
            {
                var normalizedContainerRect = GetNormalizedUISpaceContainerRect();
                var x = Mathf.InverseLerp(normalizedContainerRect.xMin, normalizedContainerRect.xMax, manyMouseInput.Position.x);
                var y = Mathf.InverseLerp(normalizedContainerRect.yMin, normalizedContainerRect.yMax, manyMouseInput.Position.y);
                return new Vector2(x, 1 - y);
            }
        }

        /// <summary>
        /// The screen position of the lightgun's "mouse" in the game window. Bottom-left is (0,0), top-right is (screen.width, screen.height).
        /// </summary>
        public Vector2 Position
        {
            get
            {
                return new Vector2(ViewportPosition.x * Screen.width, ViewportPosition.y * Screen.height);
            }
        }
        public int index;

        public class ButtonState
        {
            public int buttonIndex;
            public bool down;
            public bool held;
            public bool resetHeldAtEndOfFrame;
        }
        public ButtonState[] buttons = new ButtonState[8];

        public Lightgun(ManyMouse manyMouseInput)
        {
            this.manyMouseInput = manyMouseInput;
            manyMouseInput.OnMouseButtonDown += OnMouseButtonDown;
            manyMouseInput.OnMouseButtonUp += OnMouseButtonUp;
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i] = new ButtonState() { buttonIndex = i };
            }
        }

        internal void EndOfFrameUpdate()
        {
            foreach (var buttonState in buttons)
            {
                buttonState.down = false;
                if (buttonState.resetHeldAtEndOfFrame)
                {
                    buttonState.resetHeldAtEndOfFrame = true;
                    buttonState.held = false;
                }
            }
        }

        public bool GetButton(int button)
        {
            if (button < 0 || button >= buttons.Length) return false;
            return buttons[button].held;
        }

        public bool GetButtonDown(int button)
        {
            if (button < 0 || button >= buttons.Length) return false;
            return buttons[button].down;
        }

        void OnMouseButtonDown(int button)
        {
            if (button < 0 || button >= buttons.Length) return;
            buttons[button].held = true;
            buttons[button].down = true;
        }

        void OnMouseButtonUp(int button)
        {
            if (button < 0 || button >= buttons.Length) return;
            buttons[button].resetHeldAtEndOfFrame = true;
        }

        public bool CompareManyMouse(ManyMouse other)
        {
            return other == manyMouseInput;
        }

        // Uses top left as 0,0 and bottom right as 1,1; as standard mouse input/unity editor ui does.
        static Rect GetNormalizedUISpaceContainerRect()
        {
            var containerRect = new Rect(0, 0, Screen.width, Screen.height);
#if UNITY_EDITOR
            var gameView = UnityEditor.EditorWindow.GetWindow(typeof(UnityEditor.EditorWindow).Assembly.GetType("UnityEditor.GameView"));
            int tabHeight = 22;
            containerRect = new Rect(gameView.position.x, gameView.position.y + tabHeight, gameView.position.width, gameView.position.height - tabHeight);
#elif UNITY_STANDALONE_WIN
            // Screen.mainWindowPosition and Screen.mainWindowDisplayInfo seem to return the values for the actual screen, rather than the window.
            // Debug.Log(containerRect+" "+Screen.mainWindowPosition+" "+Screen.mainWindowDisplayInfo.width+" "+Screen.mainWindowDisplayInfo.height+" "+ Screen.mainWindowDisplayInfo.workArea);
            containerRect = WindowsUtil.GetWindowPosition();
#endif
            Debug.Log(containerRect);
#if UNITY_2021_3_OR_NEWER
            var displayInfo = Screen.mainWindowDisplayInfo;
            return new Rect(containerRect.x / displayInfo.width, containerRect.y / displayInfo.height, containerRect.width / displayInfo.width, containerRect.height / displayInfo.height);
#else
            return new Rect(containerRect.x / (float)Screen.currentResolution.width, containerRect.y / (float)Screen.currentResolution.height, containerRect.width / Screen.currentResolution.width, containerRect.height / (float)Screen.currentResolution.height);
#endif
        }
    }
}