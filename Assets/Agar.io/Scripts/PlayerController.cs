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
    public float maxSpeed = 1000f;
    public float minSpeed = 1f;

    public Vector2 nextScale;
    public Rigidbody2D rb;
    public GameHUD gameHUD;
    MapLimits map;

    public Vector3 LocalPostion;

    [Header("Movement Speed")]
    public float Speed = 100f;
    public float ScaleFactor = 1f;
    public bool IsMove;
    public float AdjustmentSpeed;

    [Header("Camera Properties")]
    private CinemachineVirtualCamera virtualCamera;
    public float checkorthoSizeTime = 0.05f;
    public Transform playerTransform;
    [SerializeField] private Transform cameraLock;
    public GameObject VC;
    public float nextOrthographicSize = 20;

    #region FetchData
    public override void OnStartClient()
    {
        if (!string.IsNullOrWhiteSpace(MyPlayerID))
        {
            Debug.LogError($"2 Is game manager null? {GameManager.instance == null} && is player manager there? {GameManager.instance.playerManagerList.Exists(x => x.myPlayerState.playerData.PlayerId.Equals(MyPlayerID))}");

            if (GameManager.instance != null && GameManager.instance.playerManagerList.Exists(x => x.myPlayerState.playerData.PlayerId.Equals(MyPlayerID)))
            {
                PlayerManager pm = GameManager.instance.playerManagerList.Find(x => x.myPlayerState.playerData.PlayerId.Equals(MyPlayerID));
                Debug.LogError($"3 Is game manager null? {GameManager.instance == null} && is player manager there? {GameManager.instance.playerManagerList.Exists(x => x.myPlayerState.playerData.PlayerId.Equals(MyPlayerID))}");

                pm.myPlayerState.speed = Speed;
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

    public IEnumerator SetScalePlayer()
    {
        while (!IsMove)
        {
            if (GameManager.instance != null && GameManager.instance.playerManagerList.Exists(x => x.myPlayerState.playerData.PlayerId.Equals(MyPlayerID)))
            {
                PlayerManager pm = GameManager.instance.playerManagerList.Find(x => x.myPlayerState.playerData.PlayerId.Equals(MyPlayerID));

                Vector3 scale = transform.localScale;
                if (scale != pm.SyncScale)
                {

                    transform.localScale = Vector2.Lerp(scale, pm.SyncScale, 1f * Time.fixedDeltaTime);
                    UpdateOrthoGraphicSize();
                }
            }
            yield return null;
        }

    }
    [Client]
    private void Start()
    {
        if (isLocal)
        {

            rb = GetComponent<Rigidbody2D>();
            StartCoroutine(MoveTowardsMouseCoroutine());
            StartCoroutine(SetScalePlayer());

            //Camera  properties
            VC = gameHUD.VirtualCamera;
            virtualCamera = VC.GetComponent<CinemachineVirtualCamera>();

            if (isLocalPlayer)
            {
                virtualCamera.Follow = transform;

            }
            virtualCamera.m_Lens.OrthographicSize = 600;
            UpdateOrthoGraphicSize();
        }
    }
    private void LateUpdate()
    {
        if (isLocal)
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
        while (!IsMove && isLocal/* && isLocalPlayer*/)
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
        AdjustmentSpeed = adjustedSpeed;

        Vector3 direction = (targetPosition - transform.position).normalized;
        Vector3 velocity = adjustedSpeed * Time.fixedDeltaTime * direction;

        GrowPlayer();
        if (Vector3.Distance(velocity, PlayerManager.instance.PreviousPos) > 0.01f)
        {
            if (GameManager.instance != null && GameManager.instance.playerManagerList.Exists(x => x.myPlayerState.playerData.PlayerId.Equals(MyPlayerID)))
            {
                PlayerManager pm = GameManager.instance.playerManagerList.Find(x => x.myPlayerState.playerData.PlayerId.Equals(MyPlayerID));
                transform.SetParent(pm.transform);
                pm.CmdSendMovement(transform.position, velocity);

                if (transform.localScale.x != pm.SyncScale.x)
                {
                    pm.CmdSendPlayerScale(transform.localScale);
                }

            }
        }
        rb.velocity = velocity;

    }
    public Vector3 GrowPlayer()
    {

        Debug.Log("Before Rb Values arre --> " + rb.mass);

        float c = Mathf.Sqrt((rb.mass * 1.15f));

        Debug.Log("After Rb Values arre --> " + rb.mass);

        c = Mathf.Clamp(c, 2, 2000);

        Debug.Log("Values are Updating  SClaing--> " + c);
        return transform.localScale = new Vector3(c, c, 0);
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

            if (previousSize != nextOrthographicSize)
            {
                virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(previousSize, nextOrthographicSize, Time.deltaTime * 5f);
            }
        }
    }
    public void UpdateOrthoGraphicSize()
    {
        float scale = transform.localScale.x;
        float m = Mathf.Sqrt(scale + 26);
        float calc = (m * m) / 1.4f;
        nextOrthographicSize = Mathf.Max(calc, 20);
    }
    #endregion
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Mass"))
        {
            if (isLocalPlayer)
            {
                foreach (var coinData in GameManager.instance.gameState.collectableDatas)
                {
                    if (coinData.MyCode.Equals(collision.gameObject.name))
                    {
                        PlayerManager.instance.SendCoinCollectedData(coinData.MyCode, NetworkTime.time, MyPlayerID);
                    }
                }
            }
            collision.gameObject.SetActive(false);
            collision.gameObject.transform.SetAsFirstSibling();
        }
    }
}
