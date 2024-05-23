using Auki.ConjureKit;
using Auki.ConjureKit.Manna;
using Auki.Integration.ARFoundation.Manna;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Button = UnityEngine.UI.Button;

namespace ConjureKit
{
    public class ConjureKitManager : MonoBehaviour
    {
        private const string Key = "af2397af-d18b-47e3-8877-e01c855cb626";
        private const string Secret = "15ecaba0-5337-488e-85f1-1caf2c45ea908a9b8b9f-8ea8-46e0-b8dd-a19f2f34c4de";
        public  string currentDomainId = "WPHVS4OIGE2";

        private Manna _manna;
        private IConjureKit _conjureKit;

        [SerializeField] private Transform arCamera;
        [SerializeField] private Button movePanelButton;
        [SerializeField] private RectTransform layersPanel;
        [Tooltip("End value for moving layers panel through X axis")]
        public int xEndValue;
        [SerializeField] private GameObject furniturePanel;
        public TMP_Text informationText;

        private bool _movedPanel;
        public bool LayersActive { get; private set; }

        private void Start()
        {
            InitializeConjureKit();
            movePanelButton.onClick.AddListener(MoveLayersPanel);
            informationText.gameObject.SetActive(true);
            layersPanel.gameObject.SetActive(false);
        }

        private void InitializeConjureKit()
        {
            _conjureKit = new Auki.ConjureKit.ConjureKit(arCamera, Key, Secret);
            _manna = new Manna(_conjureKit);

#if UNITY_EDITOR
            arCamera.gameObject.AddComponent<FrameFeederEditor>().AttachMannaInstance(_manna);
#else
        _manna.GetOrCreateFrameFeederComponent().AttachMannaInstance(_manna);
#endif

            _manna.OnLighthouseTracked += OnQRCodeDetected;
            _manna.GetOrCreateFrameFeederComponent().AttachMannaInstance(_manna);

            _conjureKit.Connect();
        }

        private void Update()
        {
            LayersActive = layersPanel.gameObject.activeSelf;
        }

        private void MoveLayersPanel()
        {
            _movedPanel = !_movedPanel;
            float targetPositionX = _movedPanel ? xEndValue : 304;
            layersPanel.DOAnchorPosX(targetPositionX, 1f);
        }

        private void OnQRCodeDetected(Lighthouse lighthouse, Pose pose, bool isClose)
        {
            furniturePanel.SetActive(true);
            informationText.text = "Choose or Create Layer";
        }
    }
}
