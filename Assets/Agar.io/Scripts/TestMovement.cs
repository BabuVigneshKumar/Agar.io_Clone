using Cinemachine;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public class TestMovement : NetworkBehaviour
{
    public float speed = 10f;
    public GameObject VC;
    public CinemachineVirtualCamera virtualCamera;
   
    [SyncVar(hook = nameof(OnPositionChanged))] public Vector3 myPosition;
    [SyncVar(hook = nameof(OnDirectionChanged))] public Vector3 myDirection;


    private void Awake()
    {
        VC = GameHUD.instance.VirtualCamera;
        virtualCamera = VC.GetComponent<CinemachineVirtualCamera>();
    }

    public Vector3 PreviousPos;

    private void Update()
    {
        if (!isLocalPlayer) return;


        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");


        Vector3 movement = new Vector3(horizontalInput, verticalInput, 0f) * speed * Time.deltaTime;


        Vector3 currentPos = transform.position + movement;

        if (Vector3.Distance(currentPos, PreviousPos) > 0.01f)
        {
            CmdSendMovement(currentPos, movement);
            PreviousPos = currentPos;
        }
    }

    [Command]
    private void CmdSendMovement(Vector3 position, Vector3 _directions)
    {
        myPosition = position;
        myDirection = _directions;
        Debug.Log("Server Position Transfer --> " + position + " Directions" + _directions);
    }

    private void OnPositionChanged(Vector3 oldPositions, Vector3 newPosition)
    {
        if (isLocalPlayer)
        {
            transform.position = newPosition;
        }
    }

    private void OnDirectionChanged(Vector3 oldDirections, Vector3 newDirection)
    {
        if (isLocalPlayer)
        {
            transform.Translate(newDirection);
        }
    }


    public override void OnStartLocalPlayer()
    {

        virtualCamera.Follow = transform;
    }
}
