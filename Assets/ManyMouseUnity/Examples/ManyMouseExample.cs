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

        List<ExampleCrosshair> crosshairs;

        public List<ExampleCrosshair> Crosshairs { get { return crosshairs; } }

        private void Start()
        {
            InitializeCrosshairs();
        }

        private void OnEnable()
        {
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
            crosshairs = new List<ExampleCrosshair>();

            for (int i = 0; i < ManyMouseWrapper.MouseCount; i++)
            {
                if (crosshairs.Count == i)
                {
                    ExampleCrosshair newCrosshair = Instantiate(sceneMousePrefab, canvas.transform).GetComponent<ExampleCrosshair>();
                    crosshairs.Add(newCrosshair);
                }
                crosshairs[i].Initialize(i);
            }
        }
    }
}