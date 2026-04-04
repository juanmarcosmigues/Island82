using UnityEngine;

public class VoidTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        other.gameObject.GetComponent<IDynamicObject>()?.EnteredVoid();
    }
}
