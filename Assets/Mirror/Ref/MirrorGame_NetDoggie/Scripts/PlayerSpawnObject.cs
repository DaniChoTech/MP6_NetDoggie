using UnityEngine;
using Mirror;
using UnityEngine.AI;

public class PlayerSpawnObject : NetworkBehaviour
{
    [Header("Componets")]
    public NavMeshAgent NavAgent_Player;
    public Animator Animator_Player;
    public TextMesh TextMesh_HealthBar;
    public TextMesh TextMesh_NetType;
    public Transform Transform_Player;

    [Header("Movement")]
    public float _ratationSpeed = 100.0f;

    [Header("Attack")]
    public KeyCode _atkKey = KeyCode.Space;
    public GameObject Prefab_AtkObject;
    public Transform Tranform_AtkSpawnPos;

    [Header("Stats Server")]
    [SyncVar] public int _health = 4;

    public void Update()
    {
        SetHealthBarOnUpdate(_health);

        if (CheckIsFocusedOnUpdate() == false)
            return;

        CheckIsLocalPlayerOnUpdate();
    }

    private void SetHealthBarOnUpdate(int health)
    {
        TextMesh_HealthBar.text = new string('-', health);
    }

    private bool CheckIsFocusedOnUpdate()
    {
        return Application.isFocused;
    }

    private void CheckIsLocalPlayerOnUpdate()
    {
        if (this.isLocalPlayer == false)
            return;

        // 로컬플레이어의 회전
        float horizontal = Input.GetAxis("Horizontal");
        transform.Rotate(0, horizontal * _ratationSpeed * Time.deltaTime, 0);

        // 로컬플레이어의 이동
        float vetrial = Input.GetAxis("Vertical");
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        NavAgent_Player.velocity = forward * Mathf.Max(vetrial, 0) * NavAgent_Player.speed;
        Animator_Player.SetBool("Moving", NavAgent_Player.velocity != Vector3.zero);

        // 공격
        if (Input.GetKeyDown(_atkKey))
        {
            CommandAtk();
        }

        RotateLocalPlayer();
    }

    private void RotateLocalPlayer()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out RaycastHit hit, 100))
        {
            Debug.DrawLine(ray.origin, hit.point);
            Vector3 lookRotate = new Vector3(hit.point.x, Transform_Player.position.y, hit.point.z);
            Transform_Player.LookAt(lookRotate);
        }
    }

    // 클라에서 서버로 호출은 하지만 로직의 동작은 서버사이드 온리
    [Command]
    private void CommandAtk()
    {
        GameObject atkObjectForSpawn = Instantiate(Prefab_AtkObject, Tranform_AtkSpawnPos.transform.position, Tranform_AtkSpawnPos.transform.rotation);
        NetworkServer.Spawn(atkObjectForSpawn);

        RpcOnAtk();
    }

    [ClientRpc]
    private void RpcOnAtk()
    {
        Debug.LogWarning($"{this.netId} 가 RPC 옴");
        Animator_Player.SetTrigger("Atk");
    }

    // 클라에서 다음 함수가 실행되지 않도록 ServerCallback을 달아줌
    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        var atkGenObject = other.GetComponent<AtkSpawnObject>();
        if (atkGenObject == null)
            return;

        _health--;

        if(_health == 0)
        {
            NetworkServer.Destroy(this.gameObject);
        }
    }

}
