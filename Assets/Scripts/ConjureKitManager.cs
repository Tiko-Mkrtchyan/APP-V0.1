using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Auki.ConjureKit;
using Auki.ConjureKit.Manna;
using Auki.Integration.ARFoundation.Manna;
using Datas;
using DG.Tweening;
using TMPro;
using Button = UnityEngine.UI.Button;
using State = Auki.ConjureKit.State;

public class ConjureKitManager : MonoBehaviour
{
    private readonly string key = "af2397af-d18b-47e3-8877-e01c855cb626";
    private readonly string secret = "15ecaba0-5337-488e-85f1-1caf2c45ea908a9b8b9f-8ea8-46e0-b8dd-a19f2f34c4de";
    private Manna _manna;
    private IConjureKit  _conjureKit;
   public string _currentDomainId = "WPHVS4OIGE2";

    private TMP_Text buttonText;
    private bool movedPanel;
   
    [SerializeField] private Transform arCamera;
    [SerializeField] private Button movePanelButton;
    [SerializeField] private RectTransform layersPanel;
    
    
    [Tooltip("End Value For Moving Layers Panel Through X axes")]
    public int xEndValue;

    private void Start()
    {
        
        
       movePanelButton.onClick.AddListener(MoveLayersPanel);
       

        
        _conjureKit = new ConjureKit
        (
            arCamera,
            key,
            secret
        );
        _manna = new Manna(_conjureKit);
#if UNITY_EDITOR
        arCamera.gameObject.AddComponent<FrameFeederEditor>().AttachMannaInstance(_manna);
#else
        _manna.GetOrCreateFrameFeederComponent().AttachMannaInstance(_manna);
#endif
        
        _manna.OnLighthouseTracked += OnQRCodeDetected;

        _manna.GetOrCreateFrameFeederComponent().AttachMannaInstance(_manna);
        _conjureKit.OnStateChanged += state =>
        {

        };
        

        
        _conjureKit.Connect();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log(PlayerPrefs.GetString("RecordId")+"sdsdsadsadsdssdsd");
        }
    }


    void MoveLayersPanel()
    {
        movedPanel=! movedPanel;
        
        if (movedPanel==true)
        {
            layersPanel.DOAnchorPosX(xEndValue , 1f);
        }
        else
        {
            layersPanel.DOAnchorPosX(304, 1f);
        }
    }

   
    private void OnQRCodeDetected(Lighthouse lighthouse, Pose pose, bool isClose)
    {
        layersPanel.gameObject.SetActive(true);
            Debug.Log("/////////"+lighthouse.Id);
            
    }

    



    
   
}
