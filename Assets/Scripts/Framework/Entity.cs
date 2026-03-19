using UnityEngine;

public class Entity : MonoBehaviour
{
    public enum Type
    {
        Other,
        Player,
        Object,
        Enemy,
        Projectile,
        Hazard
    }
    public Type type;
    public string customTag;
}
