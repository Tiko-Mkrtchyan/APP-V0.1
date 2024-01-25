using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Auki.ConjureKit;
using Auki.ConjureKit.Manna;
using DG.Tweening;
using UnityEngine.Networking;
using Button = UnityEngine.UI.Button;
using State = Auki.ConjureKit.State;

public class ConjureKitManager : MonoBehaviour
{
    private readonly string key = "af2397af-d18b-47e3-8877-e01c855cb626";
    private readonly string secret = "15ecaba0-5337-488e-85f1-1caf2c45ea908a9b8b9f-8ea8-46e0-b8dd-a19f2f34c4de";
    private Manna _manna;
    private IConjureKit  _conjureKit;
<<<<<<< Updated upstream
    [SerializeField] private TextMeshProUGUI sessionIDText;
    [SerializeField] private TextMeshProUGUI stateText;
    [SerializeField] private Transform arCamera;
    [SerializeField] private Button qrCodeButton;
    [SerializeField] private Button spawnButton;
    [SerializeField] private bool qrCodeBool ;

    private void Start()
    {
        qrCodeButton.onClick.AddListener(ToggleLighthouse);
        
=======
    private bool movedPanel;
    [SerializeField] private Transform arCamera;
    [SerializeField] private Button qrCodeButton;
    [SerializeField] private bool qrCodeBool ;
    [SerializeField] private Button movePanelButton;
    [SerializeField] private RectTransform layersPanel;
    
   private Vector3 layersPanelStartPos;
   private string pocketBaseUrl = "https://17f7-46-70-216-9.ngrok-free.app/api/collections/christmas_trees/records";
  
   

    private void Start()
    {
        
        
        layersPanelStartPos = layersPanel.transform.localPosition;
        qrCodeButton.onClick.AddListener(ToggleLighthouse);
       movePanelButton.onClick.AddListener(MoveLayersPanel);
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
=======
            
            
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
    
=======

    void MoveLayersPanel()
    {
        movedPanel=! movedPanel;
        
        if (movedPanel==true)
        {
            layersPanel.DOLocalMoveX(3 , 1f);
        }
        else
        {
            layersPanel.DOLocalMoveX(layersPanelStartPos.x, 1f);
        }
    }

    public void UploadToDB(string domainId, string id, Pose pose)
    {
        string url = pocketBaseUrl;
        UnityWebRequest request = new UnityWebRequest(url);
        request.method = "POST";
        var json = JsonUtility.ToJson(new MyData
        {
            domainId = domainId,
            pose = SerializablePose.FromPose(pose)

        });
        Debug.Log(json);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        request.SendWebRequest().completed += operation =>
        {
            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.error);
            }
            else
            {
                Debug.Log("Success");
               
            }
        };
        
    }

>>>>>>> Stashed changes
}
