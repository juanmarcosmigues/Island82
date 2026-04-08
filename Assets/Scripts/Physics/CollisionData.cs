using UnityEngine;

public struct CollisionData
{
    public CollisionData (Vector3 point, Vector3 normal, Vector3 velocity, Collider coll)
    {
        this.point = point;
        this.normal = normal;
        this.velocity = velocity;   
        this.coll = coll;
    }

    public Vector3 point;
    public Vector3 normal;
    public Vector3 velocity;
    public Collider coll;
}
