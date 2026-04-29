using UnityEngine;

public class UIInteractionMarker : MonoBehaviour
{
    public static UIInteractionMarker Instance { get; private set; }

    public RectTransform marker;
    public RectTransform markerAnimation;
    public RectTransform viewport;

    protected int showRequests;
    protected Vector3 worldTarget;
    protected bool showMarker;

    public static void SetInstance(UIInteractionMarker instance) =>
    Instance = instance;

    private void Awake()
    {
        HideMarker();
    }
    public void ShowMarker (Vector3 worldPos)
    {
        worldTarget = worldPos;
        showMarker = true;
        showRequests++;
    }
    public void HideMarker ()
    {
        showRequests--;
        if (showRequests < 0) showRequests = 0;
        showMarker = showRequests > 0;
    }

    private void LateUpdate()
    {
        if (marker.gameObject.activeSelf != showMarker)
            marker.gameObject.SetActive(showMarker);

        if (!showMarker)
            return;

        var viewportTarget = Camera.main.WorldToViewportPoint(worldTarget);
        var screenTarget = viewport.rect.size * viewportTarget;
        screenTarget += Mathf.Max(Mathf.Sign(Mathf.Sin(Time.time * 5f)), 0) * Vector2.up * 1f;
        marker.anchoredPosition = screenTarget;
    }
}
