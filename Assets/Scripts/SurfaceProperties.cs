using UnityEngine;

public class SurfaceProperties : MonoBehaviour
{
    public enum Material { Grass, Rock, Sand, Wood, Water }
    public Material material;
    public JumpOn jumpOn;

    public bool CanLand ()
    {
        if (jumpOn == null) 
            return true;

        //Ignore intersection because we know the player is trying to land in the object already
        if (jumpOn.CheckJumpOn(ignoreIntersection: true)) 
            return false;

        return true;
    }
}
