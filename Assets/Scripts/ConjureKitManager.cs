using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Auki.ConjureKit;
using Auki.ConjureKit.Manna;
using TMPro;
using UnityEngine.UI;

public class ConjureKitManager : MonoBehaviour
{
    
    private readonly string key = "af2397af-d18b-47e3-8877-e01c855cb626";
    private readonly string secret = "15ecaba0-5337-488e-85f1-1caf2c45ea908a9b8b9f-8ea8-46e0-b8dd-a19f2f34c4de";
    private Manna _manna;
    private IConjureKit  _conjureKit;
    [SerializeField] private TextMeshProUGUI sessionIDText;
    [SerializeField] private TextMeshProUGUI stateText;
    [SerializeField] private Transform arCamera;
    [SerializeField] private Button qrCodeButton;
    [SerializeField] private Button spawnButton;
    [SerializeField] private bool qrCodeBool ;

    private void Start()
    {
        qrCodeButton.onClick.AddListener(ToggleLighthouse);
        
        _manna.GetOrCreateFrameFeederComponent();
        _conjureKit = new ConjureKit
        (
            arCamera,
            key,
            secret
        );
        _manna = new Manna(_conjureKit);

        _conjureKit.OnJoined += session =>
        {
            sessionIDText.text = session.Id;
        };
        _conjureKit.OnStateChanged += state =>
        {
            stateText.text = state.ToString();
            ToggleControlsState(state == State.Calibrated);
        };
        
        _conjureKit.Connect();
    }

    private void ToggleControlsState(bool interactable)
    {
        if (spawnButton) spawnButton.interactable = interactable;
        if (qrCodeButton) qrCodeButton.interactable = interactable;
    }
    
    public void ToggleLighthouse()
    {
        qrCodeBool = !qrCodeBool;
        _manna.SetLighthouseVisible(qrCodeBool);
    }
    
}
