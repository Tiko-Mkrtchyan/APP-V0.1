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
    [SerializeField] private Button showSessionIDButton;
    [SerializeField] private Button showStateStatusButton;
    [SerializeField] private bool qrCodeBool ;
    [SerializeField] private Button hideSessionIDButton;
    [SerializeField] private Button hideStateStatusButton;
    [SerializeField] private Vector3 offset;
    [SerializeField] private Vector3 offset2;
   
    

    private void Start()
    {
        
        
        showStateStatusButton.gameObject.SetActive(false);
        showSessionIDButton.gameObject.SetActive(false);
        qrCodeButton.onClick.AddListener(ToggleLighthouse);
        showSessionIDButton.onClick.AddListener(ShowSessionID);
        showStateStatusButton.onClick.AddListener(ShowStateStatus);
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
            if (state == State.Calibrated)
            {
                showStateStatusButton.gameObject.SetActive(true);
                showSessionIDButton.gameObject.SetActive(true);
            }
        };
        
        _conjureKit.Connect();
    }

    private void Update()
    {
        hideSessionIDButton.transform.position = sessionIDText.transform.position + offset;
        hideStateStatusButton.transform.position = stateText.transform.position + offset2;

    }

    private void ToggleControlsState(bool interactable)
    {
        
        if (qrCodeButton) qrCodeButton.interactable = interactable;
        
    }
    
    public void ToggleLighthouse()
    {
        qrCodeBool = !qrCodeBool;
        _manna.SetLighthouseVisible(qrCodeBool);
    }


    public void ShowSessionID()
    {
        StartCoroutine(MoveSessionIDTextSmooth());
    }

    public void ShowStateStatus()
    {
        StartCoroutine(MoveStateStatusTextSmooth());
    }

   

   
    IEnumerator MoveSessionIDTextSmooth()
    {
        
        showSessionIDButton.gameObject.SetActive(false);
        Vector3 startPos = new Vector3(sessionIDText.transform.position.x, sessionIDText.transform.position.y,sessionIDText.transform.position.z);
        Vector3 endpos=new Vector3(230, sessionIDText.transform.position.y,
            sessionIDText.transform.position.z);
        while (sessionIDText.transform.position != endpos)
        {
            
           sessionIDText.transform.position= Vector3.Lerp(sessionIDText.transform.position, endpos,Time.deltaTime*5);
            yield return new WaitForEndOfFrame();
        }
    }
    IEnumerator MoveStateStatusTextSmooth()
    {
        showStateStatusButton.gameObject.SetActive(false);
        Vector3 startPos = new Vector3(stateText.transform.position.x, stateText.transform.position.y,stateText.transform.position.z);
        Vector3 endpos=new Vector3(230, stateText.transform.position.y,
            stateText.transform.position.z);
        while (stateText.transform.position != endpos)
        {
            
            stateText.transform.position= Vector3.Lerp(stateText.transform.position, endpos,Time.deltaTime*5);
            yield return new WaitForEndOfFrame();
        }
    }

    
    
}
