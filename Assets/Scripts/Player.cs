using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour {
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
    public NetworkVariable<Color> PlayerColor = new NetworkVariable<Color>(Color.red);
    private GameManager _gameMgr;
    public float movementSpeed = 1f;



    private Vector3 CalcMovement()
    {
        Vector3 moveVect = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        moveVect*= movementSpeed;
        return moveVect;
    }

    public void ApplyPlayerColor()
    {
        GetComponent<MeshRenderer>().material.color = PlayerColor.Value;
    }

    [ServerRpc]
    void RequestPositionForMovementServerRpc(Vector3 movement)
    {
        Position.Value += movement;

        float planeSize = 5f;
        Vector3 newPosition = Position.Value + movement;
        newPosition.x = Mathf.Clamp(newPosition.x, planeSize * -1, planeSize);
        newPosition.z = Mathf.Clamp(newPosition.z, planeSize * -1, planeSize);
        Position.Value = newPosition;
    }
    public override void OnNetworkSpawn()
    {
        if(IsOwner)
        {
            _gameMgr = GameObject.Find("GameManager").GetComponent<GameManager>();
            _gameMgr.RequestNewPlayerColorServerRpc();
        }
    }


    private void Start()
    {
        ApplyPlayerColor();
        PlayerColor.OnValueChanged += OnPlayerColorChanged;
    }

    public void OnPlayerColorChanged(Color previous, Color Current)

    {
        ApplyPlayerColor();
    }

    private void Update()
    {
        //if (IsOwner)
        //{

            Vector3 move = CalcMovement();
            if (move.magnitude > 0)
            {
                RequestPositionForMovementServerRpc(move);
            }
            else
            {
                transform.position = Position.Value;
            }
        //}
    }
}
