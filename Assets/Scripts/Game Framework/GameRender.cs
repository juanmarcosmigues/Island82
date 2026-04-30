using UnityEngine;
using UnityEngine.Rendering.Universal;

[DefaultExecutionOrder(GameDefinitions.EXECUTION_ORDER_SYSTEM)]
public class GameRender : MonoBehaviour
{
    public static GameRender Instance {  get; private set; }

    public Camera renderCamera;

    private void Awake()
    {
        SingletonGameObject parent = GetComponentInParent<SingletonGameObject>();
        if (parent != null)
        {
            if (parent.queuedToBeDestroyed)
                return;
        }

        Instance = this;
    }

    public static void AddOverlayCamera (Camera camera)
    {
        var overlayData = camera.GetUniversalAdditionalCameraData();
        overlayData.renderType = CameraRenderType.Overlay;

        var baseCameraData = Instance.renderCamera.GetUniversalAdditionalCameraData();
        baseCameraData.cameraStack.Add(camera);
    }
    public static void RemoveOverlayCamera(Camera camera)
    {
        var baseCameraData = Instance.renderCamera.GetUniversalAdditionalCameraData();
        baseCameraData.cameraStack.Remove(camera);
    }
}
