using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerMovement : NetworkBehaviour
{
    [SyncVar] public string MyPlayerID;
    public float speed_ = 5f;
    public Actions actions;
    public bool LockAction;
    public float MaxZoom = 6f;
    public float MinZoom = 3f;
    public float ZoomController = 2f;
    public float ZoomSpeed = 1f;
    public float currentSpeed;
    public static PlayerMovement Instance;
    private Rigidbody2D rb;
    MapLimits map;
    public float ReducedSpeed = 1f;
    public float speedReductionRate = 0.5f;
    public string playerID;
    public int Points = 0;
    public bool isLocal;


    public override void OnStartClient()
    {
        Debug.LogError($" 0 My PLayer ID --> {MyPlayerID}  *** {PlayerManager.instance.PlayerID}");
        if (!string.IsNullOrWhiteSpace(MyPlayerID))
        {
            Debug.LogError($"2 Is game manager null? {GameManager.instance == null} && is player manager there? {GameManager.instance.playerManagerList.Exists(x => x.myPlayerState.playerData.PlayerId.Equals(MyPlayerID))}");

            if (GameManager.instance != null && GameManager.instance.playerManagerList.Exists(x => x.myPlayerState.playerData.PlayerId.Equals(MyPlayerID)))
            {
                PlayerManager pm = GameManager.instance.playerManagerList.Find(x => x.myPlayerState.playerData.PlayerId.Equals(MyPlayerID));

                transform.SetParent(pm.transform);
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

    private void Start()
    {
        GameHUD.instance.ScoreTxt.text = Points.ToString();
        //playerID = PlayerManager.instance.PlayerID;
        //Debug.Log("MY Player ID ++++ " + PlayerManager.instance.PlayerID);
        actions = GetComponent<Actions>();
        rb = GetComponent<Rigidbody2D>();
        StartCoroutine(MoveTowardsMouseCoroutine());


        if (MassSpawnner.Instance == null)
        {
            Debug.LogError("MassSpawnner instance is null. Ensure that it is properly initialized.");
            return;
        }

        if (MassSpawnner.Instance.Players.Count >= MassSpawnner.Instance.MaxPlayers)
        {
            Destroy(gameObject);
            return;
        }

        MassSpawnner.Instance.AddPlayer(gameObject);
    }


    private void Awake()
    {
        map = MapLimits.Instance;
        if (Instance == null)
        {
            Instance = this;
        }
    }

    float _speed;
    public IEnumerator MoveTowardsMouseCoroutine()
    {

        while (true && UIController.instance.IsFocus && isLocal)
        {
            float speed = speed_ / transform.localScale.x;
            speed_ = Mathf.Min(speed_, 0.5f);
            currentSpeed = speed;
            Vector2 direction = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            direction.x = Mathf.Clamp(direction.x, map.Maplimits.x * -1 / 2, map.Maplimits.x / 2);
            direction.y = Mathf.Clamp(direction.y, map.Maplimits.y * -1 / 2, map.Maplimits.y / 2);



            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            //SendDataMovement(direction.x, direction.y , playerID);

            transform.position = Vector2.MoveTowards(transform.position, direction, speed * Time.deltaTime);

            if (GetComponent<Collider2D>().OverlapPoint(mousePosition))
            {
                ReducedSpeed = 1f;
            }
            else
            {
                speed_ = 5f;
            }

            if (LockAction)
            {
                yield return null;
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                actions.ThorwMass();
            }

            //if (Input.GetKeyDown(KeyCode.Space))
            //{
            //    if (MassSpawnner.Instance.Players.Count >= MassSpawnner.Instance.MaxPlayers)
            //    {
            //        yield return null;
            //    }

            //    //actions.Split();
            //}

            yield return null;
        }


    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Mass"))
        {
            if (isLocalPlayer)
            {
                Points += 1;
                GameHUD.instance.ScoreTxt.text = Points.ToString();

            }
            Destroy(collision.gameObject);


        }
    }





}
