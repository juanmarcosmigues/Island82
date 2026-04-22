using UnityEngine;

public class SteelChest : MonoBehaviour
{
    public Transform breakParts;

    private JumpOn jumpOn;
    private void Awake()
    {
        jumpOn = GetComponent<JumpOn>();
        jumpOn.OnJumpedOn += _ => Break();
    }

    public void Break ()
    {
        breakParts.SetParent(null);
        breakParts.gameObject.SetActive(true);

        gameObject.SetActive(false);
    }
}
