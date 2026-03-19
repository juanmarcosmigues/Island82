using UnityEngine;

public class CombatHandler : MonoBehaviour
{
    public enum Weight { Light, Medium, Heavy  };
    public event System.Action<GameObject, int, Weight, string> OnGetHit;
    public void GetHit (GameObject source, int damage, Weight weight, string tag) =>
        OnGetHit?.Invoke (source, damage, weight, tag);

    public void DealHit ()
    {

    }
}
