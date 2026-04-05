using UnityEngine;

public class GenericDestroyable : MonoBehaviour
{
    public string[] vfxOnDestroy;
    public GameObject childVFXOnDestroy;

    ITrigger trigger;

    private void Awake()
    {
        trigger = GetComponent<ITrigger>();
        trigger.OnTriggered += _ => DestroyObject();
    }

    public void DestroyObject ()
    {
        vfxOnDestroy.ForEach(vfx => PoolManager.Instance.GetPool<VFXPool>(vfx).GetObject(transform.position));

        if (childVFXOnDestroy != null)
        {
            childVFXOnDestroy.transform.SetParent(null);
            childVFXOnDestroy.transform.gameObject.SetActive(true);
        }

        gameObject.SetActive(false);
    }
}
