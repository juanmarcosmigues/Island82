using System.Collections;
using UnityEngine;

public class SpiritPainting : MonoBehaviour
{
    public Trigger trigger;

    private void Awake()
    {
        trigger.onTriggerEnter += InsidePainting;
    }

    private void InsidePainting ()
    {
        StartCoroutine(EnterSpiritMode());
    }

    IEnumerator EnterSpiritMode ()
    {
        trigger.gameObject.SetActive(false);
        Player.Instance.gameObject.SetActive(false);

        yield return new WaitForSeconds(1);

        Player.Instance.transform.position = trigger.transform.position;
        Player.Instance.gameObject.SetActive(true);
        Player.Instance.EnterSpiritMode();
        Player.Instance.SetHorizontalForce(transform.forward * 5f);

        yield return new WaitForSeconds(1);

        trigger.gameObject.SetActive(true);
    }
}
