using UnityEngine;

public class CombatHandler : MonoBehaviour
{
    public enum Weight { Light, Medium, Heavy  };
    public event System.Action<GameObject, int, Weight, string, bool> OnGetHit;
    public void GetHit (GameObject source, int damage, Weight weight, string tag, bool knockback = true) =>
        OnGetHit?.Invoke (source, damage, weight, tag, knockback);

    public void DealHit ()
    {

    }
}
