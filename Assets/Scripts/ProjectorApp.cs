using UnityEngine;
using UnityEngine.UI;

public class ProjectorApp : MonoBehaviour
{
    [SerializeField] private int _webSocketServerPort = 9999;
    [SerializeField] private Transform _portalCameraRig;
    [SerializeField] private GameObject _portalDisplay;
    [SerializeField] private GameObject _markerDisplay;
    [SerializeField] private int _width = 1920;
    [SerializeField] private int _height = 1080;

    [SerializeField] private Transform _stage;

    private bool _isImageTracking;
    private Pose _diffPose;
    private WebSocketServerManager _wssManager;

    private StereoscopicCapture _stereoscopicCapture;

    [Space]
    [Header("Preview")]
    [SerializeField] private Texture _tex;

    // Projector > 350x200 MarkerSize:
    // LCD > 60x33 MarkerSize: 33x33

    private void Start()
    {
        var lCam = _portalCameraRig.Find("LeftCamera").GetComponent<Camera>();
        var rCam = _portalCameraRig.Find("RightCamera").GetComponent<Camera>();

        _stereoscopicCapture = new StereoscopicCapture(lCam, rCam, _width, _height);
        _portalDisplay.GetComponent<RawImage>().texture = _tex = _stereoscopicCapture.Texture;

        _wssManager = new WebSocketServerManager(_webSocketServerPort);
        _wssManager.OnClientConnected += _wssManager_OnClientConnected;
        _wssManager.OnClientReset += _wssManager_OnClientReset;
        _wssManager.OnMarkerPose += _wssManager_OnMarkerPose;
        _wssManager.OnClientPose += _wssManager_OnClientPose;
        _wssManager.ServerStart();
        _markerDisplay.SetActive(true);
        _portalDisplay.SetActive(false);
    }

    private void _wssManager_OnClientConnected(string id)
    {
        Debug.Log($"Client[{id}] Connected");
        _markerDisplay.SetActive(true);
        _portalDisplay.SetActive(false);
    }

    private void _wssManager_OnClientReset(string id)
    {
        Debug.Log($"Client[{id}] _wssManager_OnClientReset");
        _markerDisplay.SetActive(true);
        _portalDisplay.SetActive(false);
    }

    private void _wssManager_OnMarkerPose(string id, Pose pose)
    {
        Debug.Log($"Client[{id}] _wssManager_OnMarkerPose");
        _stage.SetPositionAndRotation(pose.position, pose.rotation);
        _markerDisplay.SetActive(false);
        _portalDisplay.SetActive(true);
    }

    private void _wssManager_OnClientPose(string id, Pose pose)
    {
        //Debug.Log($"Client[{id}] _wssManager_OnClientPose");
        _portalCameraRig.SetPositionAndRotation(pose.position, pose.rotation);
    }

    private void Update()
    {
        _stereoscopicCapture.Update();
    }
}
