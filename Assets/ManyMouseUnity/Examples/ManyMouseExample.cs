using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ManyMouseUnity;

namespace ManyMouseExample
{
    public class ManyMouseExample : MonoBehaviour
    {
        [SerializeField] Canvas canvas;
        [SerializeField] Canvas middleCanvas;
        [SerializeField] GameObject sceneMousePrefab;

        List<ManyMouseCrosshair> crosshairs;

        public List<ManyMouseCrosshair> Crosshairs { get { return crosshairs; } }

        private void OnEnable()
        {
            int numMice = ManyMouseWrapper.MouseCount;
            crosshairs = new List<ManyMouseCrosshair>();

            InitializeCrosshairs();

            ManyMouseWrapper.OnInitialized += InitializeCrosshairs;
        }

        private void OnDisable()
        {
            ManyMouseWrapper.OnInitialized -= InitializeCrosshairs;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                ManyMouseWrapper.Instance.Reinitialize();
            }
        }

#if UNITY_EDITOR
        private void OnApplicationFocus(bool focus)
        {
            middleCanvas.enabled = !focus;
        }
#endif

        private void InitializeCrosshairs()
        {
            for (int i = 0; i < ManyMouseWrapper.MouseCount; i++)
            {
                if (crosshairs.Count == i)
                {
                    ManyMouseCrosshair newCrosshair = Instantiate(sceneMousePrefab, canvas.transform).GetComponent<ManyMouseCrosshair>();
                    crosshairs.Add(newCrosshair);
                }
                crosshairs[i].Initialize(i);
            }
        }
    }
}