using UnityEngine;

/// <summary>
/// Plays a sequence of meshes on a MeshFilter like frames of a flipbook animation.
/// Supports a user-defined framerate and looping or play-once playback.
/// </summary>
[RequireComponent(typeof(MeshFilter))]
public class MeshFrameAnimator : MonoBehaviour
{
    [Header("Frames")]
    [Tooltip("Meshes played in order, one per frame.")]
    [SerializeField] private Mesh[] frames;

    [Header("Playback")]
    [Tooltip("Frames per second.")]
    [Min(0.0001f)]
    [SerializeField] private float frameRate = 12f;

    [Tooltip("If true, the animation restarts after the last frame. If false, it stops on the last frame.")]
    [SerializeField] private bool loop = true;

    [Tooltip("Start playing automatically on Start().")]
    [SerializeField] private bool playOnStart = true;

    // --- Runtime state ---
    private MeshFilter meshFilter;
    private int currentFrame;
    private float timer;
    private bool isPlaying;

    /// <summary>True while the animation is actively advancing frames.</summary>
    public bool IsPlaying => isPlaying;

    /// <summary>Index of the mesh currently shown.</summary>
    public int CurrentFrame => currentFrame;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
    }

    private void Start()
    {
        if (playOnStart)
            Play();
    }

    private void Update()
    {
        if (!isPlaying || frames == null || frames.Length == 0)
            return;

        timer += Time.deltaTime;
        float frameDuration = 1f / frameRate;

        // Use while-loop so we can catch up if a frame takes longer than frameDuration
        // (e.g. after a hitch or very high framerate settings).
        while (timer >= frameDuration)
        {
            timer -= frameDuration;
            AdvanceFrame();
            if (!isPlaying) break; // non-looping playback finished
        }
    }

    private void AdvanceFrame()
    {
        int next = currentFrame + 1;

        if (next >= frames.Length)
        {
            if (loop)
            {
                next = 0;
            }
            else
            {
                // Stay on the last frame and stop.
                currentFrame = frames.Length - 1;
                ApplyCurrentFrame();
                Stop();
                return;
            }
        }

        currentFrame = next;
        ApplyCurrentFrame();
    }

    private void ApplyCurrentFrame()
    {
        if (frames == null || frames.Length == 0) return;
        meshFilter.sharedMesh = frames[currentFrame];
    }

    // --- Public API ---

    /// <summary>Start playing from the beginning.</summary>
    public void Play()
    {
        if (frames == null || frames.Length == 0)
        {
            Debug.LogWarning($"{nameof(MeshFrameAnimator)} on '{name}' has no frames assigned.", this);
            return;
        }

        currentFrame = 0;
        timer = 0f;
        isPlaying = true;
        ApplyCurrentFrame();
    }

    /// <summary>Resume playing from the current frame.</summary>
    public void Resume() => isPlaying = true;

    /// <summary>Pause playback without resetting the frame.</summary>
    public void Pause() => isPlaying = false;

    /// <summary>Stop playback and reset to the first frame.</summary>
    public void Stop()
    {
        isPlaying = false;
        timer = 0f;
    }

    /// <summary>Jump to a specific frame index (clamped to valid range).</summary>
    public void GoToFrame(int index)
    {
        if (frames == null || frames.Length == 0) return;
        currentFrame = Mathf.Clamp(index, 0, frames.Length - 1);
        timer = 0f;
        ApplyCurrentFrame();
    }

    /// <summary>Change the framerate at runtime.</summary>
    public void SetFrameRate(float fps)
    {
        frameRate = Mathf.Max(0.0001f, fps);
    }

    /// <summary>Replace the frame sequence at runtime.</summary>
    public void SetFrames(Mesh[] newFrames, bool restart = true)
    {
        frames = newFrames;
        if (restart) Play();
    }

    /// <summary>Enable or disable looping at runtime.</summary>
    public void SetLoop(bool shouldLoop) => loop = shouldLoop;
}
