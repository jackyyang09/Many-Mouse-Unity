using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ManyMouseUnity;
using LightgunEngine;

public class LightgunInputExample : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] LightgunInputCrosshair lightgunCrosshairUIPrefab;

    List<LightgunInputCrosshair> crosshairs = new List<LightgunInputCrosshair>();

    public List<LightgunInputCrosshair> Crosshairs { get { return crosshairs; } }

    private void Start()
    {
        InitializeCrosshairs();
    }

    private void OnEnable()
    {
        InitializeCrosshairs();
        LightgunInput.OnAddLightgun += OnAddLightgun;
        LightgunInput.OnRemoveLightgun += OnRemoveLightgun;
    }

    private void OnDisable()
    {
        LightgunInput.OnAddLightgun -= OnAddLightgun;
        LightgunInput.OnRemoveLightgun -= OnRemoveLightgun;

        Clear();
    }

    void Clear()
    {
        foreach (var crosshair in crosshairs)
        {
            if (crosshair != null)
                Destroy(crosshair.gameObject);
        }
        crosshairs.Clear();
    }

    private void Update()
    {
        if (Game.Instance.playing)
        {
            if (LightgunInput.lightguns.Count > 0)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            ManyMouseWrapper.Instance.Reinitialize();
        }
    }

    private void InitializeCrosshairs()
    {
        Clear();
        if (LightgunInput.lightguns.Count == 0)
        {
            LightgunInputCrosshair mouseCrosshair = Instantiate<LightgunInputCrosshair>(lightgunCrosshairUIPrefab, canvas.transform);
            mouseCrosshair.Initialize(null);
            crosshairs.Add(mouseCrosshair);
        }
        else
        {
            foreach (var lightgun in LightgunInput.lightguns)
            {
                OnAddLightgun(lightgun);
            }
        }
    }

    void DestroyMouseLightguns()
    {
        for (int i = crosshairs.Count - 1; i >= 0; i--)
        {
            LightgunInputCrosshair crosshair = crosshairs[i];
            if (crosshair.usingMouseInput)
            {
                crosshairs.RemoveAt(i);
                Destroy(crosshair.gameObject);
            }
        }
    }

    void OnAddLightgun(Lightgun lightgun)
    {
        DestroyMouseLightguns();
        LightgunInputCrosshair newCrosshair = Instantiate<LightgunInputCrosshair>(lightgunCrosshairUIPrefab, canvas.transform);
        newCrosshair.Initialize(lightgun);
        crosshairs.Add(newCrosshair);
    }

    void OnRemoveLightgun(Lightgun lightgun)
    {
        LightgunInputCrosshair crosshair = crosshairs.Find(x => x.lightgun == lightgun);
        if (crosshair != null)
        {
            crosshairs.Remove(crosshair);
            Destroy(crosshair.gameObject);
        }
    }
}