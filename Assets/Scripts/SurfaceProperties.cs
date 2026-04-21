using UnityEngine;

public class SurfaceProperties : MonoBehaviour
{
    public enum Material { Grass, Rock, Sand, Wood, Water }
    public Material material;
    public JumpOn jumpOn;

    public bool landed { get; private set; }
    public Vector3 velocity { get; private set; }

    public event System.Action<Vector3> OnLanded;
    public event System.Action<Vector3> OnLeave;
    public bool CanLand ()
    {
        if (jumpOn == null) 
            return true;

        //Ignore intersection because we know the player is trying to land in the object already
        if (jumpOn.CheckJumpOn(ignoreIntersection: true)) 
            return false;

        return true;
    }
    public void Landed (Vector3 velocity)
    {
        OnLanded?.Invoke(velocity);
        this.velocity = velocity;
        landed = true;
    }
    public void Leave (Vector3 velocity)
    {
        OnLeave?.Invoke(velocity);
        this.velocity = velocity;
        landed = false;
    }
    public void Clear ()
    {
        this.velocity = Vector3.zero;
        landed = false;
    }
}
