using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using System.Collections;

namespace ManyMouseUnity
{
    public class ManyMouseWrapper : MonoBehaviour
    {
        public enum ManyMouseUpdateMode
        {
            Update,
            FixedUpdate
        }

        [SerializeField] ManyMouseUpdateMode updateMode = ManyMouseUpdateMode.Update;

        [Header("Reconnect logic not currently implemented")]
        [SerializeField] bool tryToReconnect = true;
        [SerializeField] float reconnectDelay = 5;

        private static ManyMouseWrapper instance;
        public static ManyMouseWrapper Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<ManyMouseWrapper>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("ManyMouseWrapper");
                        instance = go.AddComponent<ManyMouseWrapper>();
                    }
                }
                return instance;
            }
        }

        public static Action OnInitialized;

        public static ManyMouse LastActiveMouse;
        public void UpdateLastActive(ManyMouse m) => LastActiveMouse = m;

        ManyMouse[] mice;
        List<ManyMouse> lostMice;

        #region External Methods
        [DllImport("ManyMouse")]
        private static extern int ManyMouse_Init();

        [DllImport("ManyMouse")]
        private static extern IntPtr ManyMouse_DriverName();//The string is in UTF-8 format. 

        [DllImport("ManyMouse")]
        private static extern void ManyMouse_Quit();

        [DllImport("ManyMouse", CharSet = CharSet.Ansi)]
        private static extern IntPtr ManyMouse_DeviceName(uint index);

        [DllImport("ManyMouse")]
        private static extern int ManyMouse_PollEvent(ref ManyMouseEvent mouseEvent);
        #endregion

        private void OnEnable()
        {
            Reinitialize();

            ManyMouse.OnAnyMouseUpdated += UpdateLastActive;
            ManyMouse.OnAnyMouseDisconnected += OnAnyMouseDisconnected;
        }

        private void OnDisable()
        {
            Debug.Log("ShuttingDown");
            ManyMouse_Quit();

            ManyMouse.OnAnyMouseUpdated -= UpdateLastActive;
            ManyMouse.OnAnyMouseDisconnected -= OnAnyMouseDisconnected;
        }

        public void Reinitialize()
        {
            ManyMouse_Quit();

            int initCode = ManyMouse_Init();
            if (initCode < 0)
            {
                Debug.Log("ManyMouse Init Code:" + initCode + " so there must be some error. Trying to close and open again");

                ManyMouse_Quit();
                initCode = ManyMouse_Init();
                if (initCode < 0)
                {
                    Debug.Log("ManyMouse Init Code:" + initCode + " so there must be some error. Retrying");
                    return;
                }
            }

            Debug.Log("ManyMouse Init Code:" + initCode);
            IntPtr mouseDriverNamePtr = ManyMouse_DriverName();
            string mouseDriverName = StringFromNativeUtf8(mouseDriverNamePtr);
            Debug.Log("ManyMouse Driver Name: " + mouseDriverName);

            int _numMice = initCode;

            mice = new ManyMouse[_numMice];
            for (int i = 0; i < _numMice; i++)
            {
                mice[i] = new ManyMouse(i);
                //todo: check if we already have a mouse in that id.
                //if it's not the correct id anymore, we have to check by the mouse's name. this might read very generically!
            }

            OnInitialized?.Invoke();
        }

        void Update()
        {
            if (updateMode == ManyMouseUpdateMode.Update) Poll();
        }

        void FixedUpdate()
        {
            if (updateMode == ManyMouseUpdateMode.FixedUpdate) Poll();
        }

#if UNITY_EDITOR
        private void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                Reinitialize();
            }
        }
#endif

        private void Poll()
        {
            if (MouseCount == 0)
            {
                //refresh check for mice occassionally?
            }
            else
            {
                //TODO: should sometimes check if we lost a mouse and then send out a "lost mouse" signal in that mouse object

                // Signal to refresh old deltas
                for (int i = 0; i < MouseCount; i++)
                {
                    mice[i].PollingReset();
                }

                //poll until empty
                ManyMouseEvent mouseEvent = new ManyMouseEvent();
                int eventsLeft = ManyMouse_PollEvent(ref mouseEvent);
                while (eventsLeft > 0)
                {
                    ProcessEvent(mouseEvent);
                    eventsLeft = ManyMouse_PollEvent(ref mouseEvent);
                }
            }
        }

        //note you'll be receiving this very rapidly!
        private void ProcessEvent(ManyMouseEvent mouseEvent)
        {
            mice[(int)mouseEvent.device].ProcessEvent(mouseEvent);
        }

        public static int MouseCount { get { return Instance.mice == null ? 0 : Instance.mice.Length; } }

        public static string MouseDeviceName(int id)
        {
            if (id > MouseCount)
            {
                Debug.Log("Mouse ID Not found: " + id + ". There are only " + MouseCount + " devices found");
                return "";
            }
            IntPtr mouseNamePtr = ManyMouse_DeviceName((uint)id);
            return Marshal.PtrToStringAnsi(mouseNamePtr);
        }

        //TODO: GetMouseBy by device name? but these are not unique?
        public static ManyMouse GetMouseByID(int id)
        {
            return Instance.mice[id];
        }

        public static string StringFromNativeUtf8(IntPtr nativeUtf8)
        {
            int len = 0;
            while (Marshal.ReadByte(nativeUtf8, len) != 0) ++len;
            if (len == 0) return string.Empty;
            byte[] buffer = new byte[len - 1];
            Marshal.Copy(nativeUtf8, buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer);
        }

        Coroutine reconnectRoutine;
        private void OnAnyMouseDisconnected(ManyMouse obj)
        {
            Debug.Log("Mouse " + obj.DeviceName + " disconnected!");

            //if (reconnectRoutine != null)
            //{
            //    reconnectRoutine = StartCoroutine(ReconnectRoutine());
            //}
        }

        IEnumerator ReconnectRoutine()
        {
            while (lostMice.Count > 0)
            {
                for (int i = 0; i < lostMice.Count; i++)
                {
                    // TODO: Attempt to relocate mouse... somehow...
                }

                yield return new WaitForSeconds(reconnectDelay);
            }
            reconnectRoutine = null;
        }
    }
}