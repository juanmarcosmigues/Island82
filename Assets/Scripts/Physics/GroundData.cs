using UnityEngine;

public struct GroundData
{
    public GroundData(Collider coll, Vector3 point, Vector3 normal)
    {
        this.point = point;
        this.normal = normal;
        this.coll = coll;
    }

    public Vector3 point;
    public Vector3 normal;
    public Collider coll;
}
