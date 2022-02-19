using UnityEngine;

namespace ManyMouseUnity
{
    public enum ManyMouseEventType
    {
        MANYMOUSE_EVENT_ABSMOTION = 0,
        MANYMOUSE_EVENT_RELMOTION,
        MANYMOUSE_EVENT_BUTTON,
        MANYMOUSE_EVENT_SCROLL,
        MANYMOUSE_EVENT_DISCONNECT,
        MANYMOUSE_EVENT_MAX
    }

    public struct ManyMouseEvent
    {
        public ManyMouseEventType type;
        public uint device;
        public uint item;
        public int value;
        public int minval;
        public int maxval;
    }

    [System.Serializable]
    public class ManyMouse
    {
        public int ID { get; private set; }
        public string DeviceName { get; private set; }

        /// <summary>
        /// The change in mouse position since the last update. May not 100% match up with what the system 
        /// thinks, so be sure to hide the mouse cursor. 
        /// Only works when polling from Mice, may not work with non-mouse devices.
        /// </summary>
        public Vector2 Delta;
        /// <summary>
        /// The mouse cursor's current Position as a normalized screen coordinate from (0, 0) to (1f, 1f)
        /// Unless using a special HID (ex. the Sinden), poll for input from the Delta instead
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// ManyMouse driver only (only!) supports 5 mouse buttons
        /// </summary>
        private const int NUM_MOUSE_BUTTONS = 5;
        public bool[] Buttons { get; private set; }
        public bool Button1 { get { return Buttons[0]; } }
        public bool Button2 { get { return Buttons[1]; } }
        public bool Button3 { get { return Buttons[2]; } }
        public bool Button4 { get { return Buttons[3]; } }
        public bool Button5 { get { return Buttons[4]; } }

        /// <summary>
        /// Currently no support for horizontal wheel movements
        /// this could cancel out without someone knowing if the wheel went up then down between polling.
        /// But the event should still fire for both.
        /// </summary>
        public int ScrollWheel { get; private set; }

        bool[] prevMouseButtons;
        int prevScrollWheel;
        Vector2 prevPos, prevDelta;

        #region Events
        /// <summary>
        /// Invoked when any connected mouse is moved.
        /// Passes the mouses's move delta
        /// </summary>
        public System.Action<Vector2> OnMouseDeltaChanged;
        /// <summary>
        /// Invoked when any connected mouse is moved.
        /// Passes the mouses's absolute position
        /// </summary>
        public System.Action<Vector2> OnMousePositionChanged;
        /// <summary>
        /// Invoked when a button of the mouse is pressed.
        /// Passes the button that was pressed
        /// </summary>
        public System.Action<int> OnMouseButtonDown;
        /// <summary>
        /// Invoked when a button of any connected mouse is released.
        /// Passes the button that was released
        /// </summary>
        public System.Action<int> OnMouseButtonUp;
        /// <summary>
        /// Invoked when the scroll wheel of the mouse is moved
        /// Passes the scroll wheel delta
        /// </summary>
        public System.Action<int> OnMouseScrolled;
        /// <summary>
        /// Invoked when the mouse is disconnected
        /// </summary>
        public System.Action OnMouseDisconnected;

        /// <summary>
        /// Invoked when any connected mouse is moved.
        /// Passes the mouse that was updated, mouse's move delta
        /// </summary>
        public static System.Action<ManyMouse, Vector2> OnAnyMouseDeltaChanged;
        /// <summary>
        /// Invoked when any connected mouse is moved.
        /// Passes the mouse that was updated, the mouses's position
        /// </summary>
        public static System.Action<ManyMouse, Vector2> OnAnyMousePositionChanged;
        /// <summary>
        /// Invoked when a button of any connected mouse is pressed.
        /// Passes the mouse that was updated, button that was pressed
        /// </summary>
        public static System.Action<ManyMouse, int> OnAnyMouseButtonDown;
        /// <summary>
        /// Invoked when a button of any connected mouse is released.
        /// Passes the mouse that was updated, button that was released
        /// </summary>
        public static System.Action<ManyMouse, int> OnAnyMouseButtonUp;
        /// <summary>
        /// Invoked when the properties of any connected mouse is changed.
        /// Passes the mouse that was updated
        /// </summary>
        public static System.Action<ManyMouse> OnAnyMouseUpdated;
        /// <summary>
        /// Invoked when the scroll wheel of any mouse is moved
        /// Passes the mouse that was updated, scroll wheel delta
        /// </summary>
        public static System.Action<ManyMouse, int> OnAnyMouseScrolled;
        /// <summary>
        /// Invoked when any mouse is disconnected
        /// Passes the mouse that was disconnected
        /// </summary>
        public static System.Action<ManyMouse> OnAnyMouseDisconnected;
        #endregion

        public ManyMouse(int _id)
        {
            ID = _id;
            Buttons = new bool[NUM_MOUSE_BUTTONS];
            prevMouseButtons = new bool[NUM_MOUSE_BUTTONS];
            DeviceName = ManyMouseWrapper.MouseDeviceName(_id);
        }

        internal void PollingReset()
        {
            Position += Delta;
            Delta = Vector2.zero;
            ScrollWheel = 0;

            for (int i = 0; i < NUM_MOUSE_BUTTONS; i++)
            {
                prevMouseButtons[i] = Buttons[i];
            }
        }

        internal void ProcessEvent(ManyMouseEvent mouseEvent)
        {
            switch (mouseEvent.type)
            {
                // Sinden Lightgun will use Absolute Motion rather than Relative Motion
                // For some reason, it returns absolute mouse coordinates from (0, 0) to (65535, 65535)
                // rather than using the actual screen resolution.
                // Therefore the value is a normalized float from 0 to 1;
                case ManyMouseEventType.MANYMOUSE_EVENT_ABSMOTION:
                    switch (mouseEvent.item)
                    {
                        case 0:
                            Position.x = (float)mouseEvent.value / (float)ushort.MaxValue;
                            break;
                        case 1:
                            Position.y = (float)-mouseEvent.value / (float)ushort.MaxValue;
                            break;
                    }

                    if (prevPos != Position)
                    {
                        OnMousePositionChanged?.Invoke(Position);
                        OnAnyMousePositionChanged?.Invoke(this, Position);
                        OnAnyMouseUpdated?.Invoke(this);
                        prevPos = Position;
                    }
                    break;
                // For some reason, ManyMouse may provide a whack series of Delta events
                // every few seconds. This may be something inherent in either the original
                // ManyMouse implementation or the C# port in the .dll
                // Seems to differ between projects/Unity versions, no clue why this happens
                case ManyMouseEventType.MANYMOUSE_EVENT_RELMOTION:
                    Delta.x += mouseEvent.item == 0 ? mouseEvent.value : 0;
                    Delta.y -= mouseEvent.item == 1 ? mouseEvent.value : 0;

                    if (prevDelta != Delta)
                    {
                        OnMouseDeltaChanged?.Invoke(Delta);
                        OnAnyMouseDeltaChanged?.Invoke(this, Delta);
                        OnAnyMouseUpdated?.Invoke(this);
                        prevDelta = Delta;
                    }
                    break;
                case ManyMouseEventType.MANYMOUSE_EVENT_BUTTON:
                    Buttons[mouseEvent.item] = mouseEvent.value == 1;
                    if (mouseEvent.value == 1)
                    {
                        OnMouseButtonDown?.Invoke((int)mouseEvent.item);
                        OnAnyMouseButtonDown?.Invoke(this, (int)mouseEvent.item);
                    }
                    else
                    {
                        OnMouseButtonUp?.Invoke((int)mouseEvent.item);
                        OnAnyMouseButtonUp?.Invoke(this, (int)mouseEvent.item);
                    }
                    OnAnyMouseUpdated?.Invoke(this);
                    break;
                case ManyMouseEventType.MANYMOUSE_EVENT_SCROLL:
                    ScrollWheel = mouseEvent.value;
                    if (prevScrollWheel != ScrollWheel)
                    {
                        OnMouseScrolled?.Invoke(mouseEvent.value);
                        OnAnyMouseScrolled?.Invoke(this, mouseEvent.value);
                        OnAnyMouseUpdated?.Invoke(this);
                        prevScrollWheel = ScrollWheel;
                    }
                    break;
                case ManyMouseEventType.MANYMOUSE_EVENT_DISCONNECT:
                    OnMouseDisconnected?.Invoke();
                    OnAnyMouseDisconnected?.Invoke(this);
                    break;
            }
        }
    }
}