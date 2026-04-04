using UnityEngine;

[RequireComponent (typeof(JumpOn))]
public class GenericDestroyable : MonoBehaviour
{
    public string[] vfxOnDestroy;
    public GameObject childVFXOnDestroy;

    JumpOn jumpOn;

    private void Awake()
    {
        jumpOn = GetComponent<JumpOn>();
        jumpOn.OnJumpedOn += _ => DestroyObject();
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
