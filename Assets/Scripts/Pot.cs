using UnityEngine;

public class Pot : MonoBehaviour
{
    public Mesh[] meshes;
    public MeshFilter mesh;
    public ParticleSystem breakParticles;
    public Transform vfxParts;
    public BoxCollider trigger;
    public BoxCollider coll;
    public Animation anim;
    public Vector3 parentOffset;

    int life;
    Timestamp timer;

    private void Awake()
    {
        trigger.isTrigger = true;
        life = meshes.Length;
    }
    private void FixedUpdate()
    {
        if ((coll.bounds.max.y - Player.Instance.coll.bounds.min.y) <= 0.01f  && 
            Player.Instance.VerticalVelocity < 1f && 
            trigger.bounds.Intersects(Player.Instance.coll.bounds))
        {
            PlayerIn();
        }
    }
    void PlayerIn()
    {
        coll.enabled = false;
        Player.Instance.InsideObject(true);
        transform.SetParent(Player.Instance.transform);
        transform.localPosition = parentOffset;
        anim.Play();
        timer.Set();

        Player.Instance.OnGroundedStart += Land;

        enabled = false;
    }

    void Land ()
    {
        if (timer.elapsed < 0.2f) return;

        life--;
        if (life <= 0 )
        {
            Player.Instance.InsideObject(false);

            vfxParts.transform.SetParent(null);
            vfxParts.gameObject.SetActive(true);
            gameObject.SetActive(false);
            transform.SetParent(null);

            Player.Instance.OnGroundedStart -= Land;
        }
        else
        {
            mesh.mesh = meshes[meshes.Length-life];
            breakParticles.Play();
            anim.Play();
        }
    }
}
