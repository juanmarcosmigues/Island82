using UnityEngine;

public class GenericDestroyable : MonoBehaviour
{
    public string[] vfxOnDestroy;
    public GameObject childVFXOnDestroy;
    public ObjectSounds sound;

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
        if (sound != null)
            sound.PlaySound("Destroy");

        gameObject.SetActive(false);
    }
}
