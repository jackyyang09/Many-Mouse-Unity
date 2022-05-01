using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using ManyMouseUnity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LightgunEngine
{
    public static class LightgunInput
    {
        static List<Lightgun> _lightguns = new List<Lightgun>();
        public static ReadOnlyCollection<Lightgun> lightguns => _lightguns.AsReadOnly();
        public static Action<Lightgun> OnAddLightgun;
        public static Action<Lightgun> OnRemoveLightgun;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnRuntimeMethodLoad()
        {
            Debug.Log("After Scene is loaded and game is running");
            ManyMouseWrapper.OnAddMouse += OnAddMouse;
            ManyMouseWrapper.OnRemoveMouse += OnRemoveMouse;
            for (int i = 0; i < ManyMouseWrapper.MouseCount; i++)
            {
                OnAddMouse(ManyMouseWrapper.GetMouseByID(i));
            }

            PlayerLoopSystem playerLoop = PlayerLoop.GetDefaultPlayerLoop();
            // Debug.Assert(PlayerLoopUtils.AddToPlayerLoop(EndOfFrameUpdate, typeof(LightgunInput), ref playerLoop, typeof(PreUpdate.NewInputUpdate), PlayerLoopUtils.AddMode.End));
            Debug.Assert(PlayerLoopUtils.AddToPlayerLoop(EndOfFrameUpdate, typeof(LightgunInput), ref playerLoop, typeof(PostLateUpdate.ResetInputAxis), PlayerLoopUtils.AddMode.End));
            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        private static void EndOfFrameUpdate()
        {
            foreach (var lightgun in _lightguns)
            {
                lightgun.EndOfFrameUpdate();
            }
        }

        static void OnAddMouse(ManyMouse mouse)
        {
            if (mouse.DeviceType == ManyMouseDeviceType.Lightgun) AddLightgun(mouse);
            mouse.OnDeviceTypeChanged += OnMouseDeviceTypeChanged;
        }

        static void OnRemoveMouse(ManyMouse mouse)
        {
            mouse.OnDeviceTypeChanged -= OnMouseDeviceTypeChanged;
            RemoveLightgun(_lightguns.Find(x => x.CompareManyMouse(mouse)));
        }

        static void OnMouseDeviceTypeChanged(ManyMouse mouse, ManyMouseDeviceType deviceType)
        {
            if (deviceType == ManyMouseDeviceType.Lightgun) AddLightgun(mouse);
            else RemoveLightgun(_lightguns.Find(x => x.CompareManyMouse(mouse)));
        }

        static void AddLightgun(ManyMouse mouse)
        {
            var lightgunInput = new Lightgun(mouse);
            lightgunInput.index = _lightguns.Count;
            _lightguns.Add(lightgunInput);
            if (OnAddLightgun != null) OnAddLightgun(lightgunInput);
        }

        static void RemoveLightgun(Lightgun lightgunInput)
        {
            if (lightgunInput == null) return;
            _lightguns.Remove(lightgunInput);
            if (OnRemoveLightgun != null) OnRemoveLightgun(lightgunInput);
        }
    }
}