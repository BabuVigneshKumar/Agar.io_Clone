using Cinemachine;
using DG.Tweening;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerController : NetworkBehaviour
{
    public static PlayerController Instance;

    [Header("Player Data Update")]

    [SyncVar] public string MyPlayerID;
    public bool isLocal;
    public Rigidbody2D rb;
    public GameHUD gameHUD;
    MapLimits map;


    public Vector3 LocalPostion;

    [Header("Movement Speed")]
     public float Speed = 100f;
    public float ScaleFactor = 1f;
    public bool IsMove;

    public int Points = 0;

    [Header("Camera Properties")]
    private CinemachineVirtualCamera virtualCamera;
    public float checkorthoSizeTime = 0.05f;
    public Transform playerTransform;
    public float offset = 2f;
    public GameObject VC;

    public float maxSpeed = 1000f;
    public float minSpeed = 1f;



    #region FetchData
    public override void OnStartClient()
    {
        if (!string.IsNullOrWhiteSpace(MyPlayerID))
        {
            Debug.LogError($"2 Is game manager null? {GameManager.instance == null} && is player manager there? {GameManager.instance.playerManagerList.Exists(x => x.myPlayerState.playerData.PlayerId.Equals(MyPlayerID))}");

            if (GameManager.instance != null && GameManager.instance.playerManagerList.Exists(x => x.myPlayerState.playerData.PlayerId.Equals(MyPlayerID)))
            {
                PlayerManager pm = GameManager.instance.playerManagerList.Find(x => x.myPlayerState.playerData.PlayerId.Equals(MyPlayerID));

                transform.SetParent(pm.transform);
                pm.playerUI.Add(this.GetComponent<PlayerUI>());
            }
        }

        if (!string.IsNullOrWhiteSpace(MyPlayerID))
        {
            if (PlayerManager.instance.PlayerID == MyPlayerID)
            {
                isLocal = true;
            }
        }
        else
        {
            isLocal = isLocalPlayer;
        }

    }
    #endregion

    private void Awake()
    {


        rb = GetComponent<Rigidbody2D>();
        map = MapLimits.Instance;
        if (Instance == null)
        {
            Instance = this;
        }
        gameHUD = UIController.instance.gameHUD;
    }
    [Client]
    private void Start()
    {

        if (isLocalPlayer)
        {
            GameHUD.instance.ScoreTxt.text = Points.ToString();
            rb = GetComponent<Rigidbody2D>();
            StartCoroutine(MoveTowardsMouseCoroutine());


            //Camera  properties
            VC = gameHUD.VirtualCamera;
            virtualCamera = VC.GetComponent<CinemachineVirtualCamera>();


            virtualCamera.Follow = transform;
            virtualCamera.m_Lens.OrthographicSize = 150;
        }
    }


    private void LateUpdate()
    {
        if (isLocalPlayer && isActiveAndEnabled)
        {
            AdjustCamera();
        }
    }

    #region Movement
    Vector3 GetMousePosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -Camera.main.transform.position.z;

        return Camera.main.ScreenToWorldPoint(mousePos);
    }

    [Client]
    public IEnumerator MoveTowardsMouseCoroutine()
    {

        Debug.Log("MOvement &&& ");
        while (!IsMove /*&& UIController.instance.IsFocus*/ && isLocalPlayer && isLocal)
        {
            Vector3 targetPos = GetMousePosition();
            Move(targetPos);
            LocalPostion = targetPos;


            yield return new WaitForFixedUpdate();
        }
        if (IsMove)
        {
            yield break;
        }
    }
    void Move(Vector3 targetPosition)
    {
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        float adjustedSpeed = Speed / (transform.localScale.x * ScaleFactor * rb.mass) * distanceToTarget;

        adjustedSpeed = Mathf.Clamp(adjustedSpeed, minSpeed, maxSpeed);

        Vector3 direction = (targetPosition - transform.position).normalized;
        Vector3 velocity = adjustedSpeed * Time.fixedDeltaTime * direction;


        rb.velocity = velocity;


        if (Vector3.Distance(velocity, PlayerManager.instance.PreviousPos) > 0.01f)
        {
            
            if (GameManager.instance != null && GameManager.instance.playerManagerList.Exists(x => x.myPlayerState.playerData.PlayerId.Equals(MyPlayerID)))
            {
                PlayerManager pm = GameManager.instance.playerManagerList.Find(x => x.myPlayerState.playerData.PlayerId.Equals(MyPlayerID));

                transform.SetParent(pm.transform);
                pm.CmdSendMovement(transform.position, velocity, MyPlayerID, NetworkTime.time);

            }
        }
    }



    #endregion

    #region Camera Mechanism


    public void AdjustCamera()
    {
        StartCoroutine(AdjustOrthoSize());
    }


    public IEnumerator AdjustOrthoSize()
    {
        yield return new WaitForSecondsRealtime(checkorthoSizeTime);

        if (playerTransform != null || virtualCamera != null)
        {
            float previousSize = virtualCamera.m_Lens.OrthographicSize;
            float targetOrthoSize = playerTransform.localScale.x + offset;

            virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(previousSize, targetOrthoSize, Time.deltaTime * 5f);
        }

    }
    #endregion

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Mass"))
        {
            if (isLocalPlayer)
            {
                //PlayerManager.instance.myPlayerState.Points++;


                foreach (var coinData in GameManager.instance.gameState.collectableDatas)
                {
                    if (coinData.MyCode.Equals(collision.gameObject.name))
                    {
                        Debug.Log($" Collsion Oobj Name {collision.gameObject.name}  ^^ coinData Name {coinData.MyCode}");
                        Debug.Log($"Total data size is {coinData.MyCode.Length * 8}");
                        PlayerManager.instance.SendCoinCollectedData(coinData.MyCode, NetworkTime.time);
                        Debug.Log($"Point_System 1 ****** {PlayerManager.instance.gameManager != null}");
                    }


                }

                //GameHUD.instance.ScoreTxt.text = PlayerManager.instance.myPlayerState.Points.ToString();

            }
            Destroy(collision.gameObject);


        }
    }

}
