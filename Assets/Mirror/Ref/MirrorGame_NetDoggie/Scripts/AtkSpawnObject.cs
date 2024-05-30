using UnityEngine;
using Mirror;

public class AtkSpawnObject : NetworkBehaviour
{
    public float _destoryAfter = 10.0f;
    public float _force = 1000;

    public Rigidbody RigidBody_AtkObject;

    public override void OnStartServer()
    {
        Invoke(nameof(DestorySelf), _destoryAfter);
    }

    private void Start()
    {
        RigidBody_AtkObject.AddForce(this.transform.forward * _force);
    }

    [Server]
    private void DestorySelf()
    {
        NetworkServer.Destroy(this.gameObject);
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        DestorySelf();
    }


}
