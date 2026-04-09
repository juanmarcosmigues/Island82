using UnityEngine;

public class GenericDestroyable : MonoBehaviour
{
    public string[] vfxOnDestroy;
    public GameObject childVFXOnDestroy;
    public ObjectSounds sound;
    [Range(0f, 1f)]
    public float dropChance;
    public string drop;

    ITrigger trigger;

    private void Awake()
    {
        trigger = GetComponent<ITrigger>();
        if (trigger ==null)
            trigger = GetComponentInChildren<ITrigger>();
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

        if (Random.value <= dropChance && drop.Length > 0)
        {
            PoolManager.Instance.GetPool<ObjectPool>(drop).GetObject().transform.position = transform.position;
        }

        gameObject.SetActive(false);
    }
}
