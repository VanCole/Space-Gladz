using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cinemachine;

[DefaultExecutionOrder(5)]
public class ShowDirector : MonoBehaviour
{
    public static ShowDirector instance;

    public GameObject showDirector;

    [SerializeField]
    public Vector3 lookAtTarget;

    [SerializeField]
    Transform center;

    public GameObject focus;

    public Transform[] standbyPosition;
    public Transform[] posSpawner;
    public Transform[] posMenu;


    public CinemachinePath[] tracksEvent;

    float speed = 0.5f;

    public bool hasSwitchPos = false;
    bool isSetup = true;

    int rngPlayer;
    Vector3 zero = Vector3.zero;
    float angle = 0.0f;


    public Coroutine currentCoroutine;
    int currentPosIndex;

    public bool lookAtGift;

    // Use this for initialization
    void Start()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }

        currentPosIndex = 0;

        showDirector.GetComponent<CinemachineDollyCart>().m_Path = tracksEvent[1];

        if (DataManager.instance.gameState == DataManager.GameState.inArena)
        {
            transform.position = standbyPosition[0].position;

            focus = GameObject.Find("Arena").gameObject;

            currentCoroutine = StartCoroutine(DelayUntilNextPosition(8.0f));
        }

        if (DataManager.instance.gameState == DataManager.GameState.mainMenu || DataManager.instance.gameState == DataManager.GameState.lobbyMenu)
        {
            transform.position = posMenu[0].position;

            focus = GameObject.Find("Main Camera").gameObject;

        }


       

    }

    void CheckAngle()
    {
        float posz = transform.position.z - Vector3.zero.z;
        float posx = transform.position.x - Vector3.zero.x;

        angle = (Mathf.Atan2(posx, posz) * Mathf.Rad2Deg) + 180.0f;

    }

    void SmoothLook(Vector3 newDirection)
    {
        Quaternion targetRotation = Quaternion.LookRotation(newDirection - showDirector.transform.position);
        showDirector.transform.rotation = Quaternion.Slerp(showDirector.transform.rotation, targetRotation, Time.deltaTime * 5.0f);
    }

    void SpeedManager()
    {
        showDirector.GetComponent<CinemachineDollyCart>().m_Speed = speed;
    }


    // Update is called once per frame
    void Update()
    {
        SpeedManager();

        if (focus)
        {
            if (DataManager.instance.gameState == DataManager.GameState.inArena)
            {
                lookAtTarget = focus.transform.position + Vector3.up;  //Look slightly above target (useful for player model)
            }
            else
            {
                focus = GameObject.Find("Main Camera").gameObject;

                lookAtTarget = focus.transform.position; //hard look to target
            }

            if (isSetup)
            {
                showDirector.transform.LookAt(lookAtTarget);
            }
            else
            {
                SmoothLook(lookAtTarget);
            }
        }

        //Position in menu scene
        if (DataManager.instance.gameState == DataManager.GameState.mainMenu && hasSwitchPos == false)
        {
            StartCoroutineSwitchToMenuPosition(posMenu[0].position, 15.0f);
        }
        if (DataManager.instance.gameState == DataManager.GameState.lobbyMenu && hasSwitchPos == false)
        {
            StartCoroutineSwitchToMenuPosition(posMenu[1].position, 15.0f);
        }

#if __DEBUG
        if ( DataManager.instance.gameState == DataManager.GameState.inArena && GameManager.instance.gameInitialized)
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                StartCoroutineSwitchToSpawnerPosition(15.0f, GameObject.Find("Arena").gameObject, 4.0f);
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                float rng = RandomPositionGenerator(0, 11);
                rngPlayer = Random.Range(0, DataManager.instance.currentNbrPlayer);

                StartCoroutineSwitchPosition(standbyPosition[(int)rng].position, tracksEvent[1], 0, 15.0f, DataManager.instance.player[rngPlayer].gameObject, true, 7.0f);
                currentPosIndex = (int)rng;
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                focus = DataManager.instance.player[0].gameObject;
                StartCoroutineSwitchToDeadPlayer(focus.transform.position, tracksEvent[2], 30.0f, focus, 4.0f, false);

            }
            if (Input.GetKeyDown(KeyCode.M))
            {
                focus = DataManager.instance.player[0].gameObject;
                StartCoroutineSwitchToDeadPlayer(focus.transform.position, tracksEvent[2], 30.0f, focus, 4.0f, true);
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                StartCoroutineSwitchToMenuPosition(posMenu[0].position, 15.0f);
            }
            if (Input.GetKeyDown(KeyCode.G))
            {
                StartCoroutineSwitchToMenuPosition(posMenu[1].position, 15.0f);
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                StartCoroutineSwitchToVictoriousPlayer(DataManager.instance.player[0].transform.position, 15.0f, DataManager.instance.player[0].gameObject);
            }
            if (Input.GetKeyDown(KeyCode.V))
            {
                StartCoroutineSwitchToVictoriousPlayer(DataManager.instance.player[1].transform.position, 15.0f, DataManager.instance.player[1].gameObject);
            }
        }
#endif
    }

    public void StartCoroutineSwitchPosition(Vector3 newPosition, CinemachinePath newTrack, int waypointIndex, float transitionSpeed, GameObject target, bool isDelayRng, float Maxdelay)
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(SwitchPosition(newPosition, newTrack, waypointIndex, transitionSpeed, target, isDelayRng, Maxdelay));
    }

    public void StartCoroutineSwitchToDeadPlayer(Vector3 newPosition, CinemachinePath newTrack, float transitionSpeed, GameObject target, float Maxdelay, bool deadByFall)
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(SwitchToDeadPlayer(newPosition, newTrack, transitionSpeed, target, Maxdelay, deadByFall));
    }

    public void StartCoroutineSwitchToSpawnerPosition(float transitionSpeed, GameObject target, float Maxdelay)
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(SwitchToSpawnerPosition(transitionSpeed, target, Maxdelay));
    }

    public void StartCoroutineSwitchToMenuPosition(Vector3 newPosition, float transitionSpeed)
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(SwitchPositionInMenu(newPosition, transitionSpeed));
    }

    public void StartCoroutineSwitchToVictoriousPlayer(Vector3 newPosition, float transitionSpeed, GameObject target)
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(SwitchToVictoriousPlayer(newPosition, transitionSpeed, target));
    }


    public IEnumerator SwitchPosition(Vector3 newPosition, CinemachinePath newTrack, int waypointIndex, float transitionSpeed, GameObject target, bool isDelayRng, float Maxdelay)
    {
        showDirector.GetComponent<CinemachineDollyCart>().m_Path = null;
        isSetup = false;

        focus = target;

        yield return new WaitUntil(() =>
        {
            transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref zero, 0.5f, transitionSpeed);
            showDirector.transform.position = Vector3.Slerp(showDirector.transform.position, transform.position, 2.0f * Time.deltaTime);

            return Vector3.Distance(transform.position, newPosition) <= 0.1f;
        });

        isSetup = true;
        speed = 0.5f;
        showDirector.GetComponent<CinemachineDollyCart>().m_Path = newTrack;
        showDirector.GetComponent<CinemachineDollyCart>().m_Position = waypointIndex;

        if (isDelayRng)
        {
            float rngDelay = Random.Range(4.0f, Maxdelay);
            currentCoroutine = StartCoroutine(DelayUntilNextPosition(rngDelay));
        }
        else
        {
            currentCoroutine = StartCoroutine(DelayUntilNextPosition(Maxdelay));
        }
    }

    public IEnumerator SwitchPositionInMenu(Vector3 newPosition, float transitionSpeed)
    {
        showDirector.GetComponent<CinemachineDollyCart>().m_Path = null;
        isSetup = false;
        hasSwitchPos = true;



        yield return new WaitUntil(() =>
        {
            transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref zero, 0.6f, transitionSpeed);
            showDirector.transform.position = Vector3.Slerp(showDirector.transform.position, transform.position, 3.0f * Time.deltaTime);

            return Vector3.Distance(transform.position, newPosition) <= 0.1f;
        });

        showDirector.GetComponent<CinemachineDollyCart>().m_Position = 0.0f;

        showDirector.GetComponent<CinemachineDollyCart>().m_Path = tracksEvent[1];


        isSetup = true;
        speed = 0.5f;
    }

    public IEnumerator SwitchToDeadPlayer(Vector3 newPosition, CinemachinePath newTrack, float transitionSpeed, GameObject target, float Maxdelay, bool deadByFall)
    {
        showDirector.GetComponent<CinemachineDollyCart>().m_Path = null;
        isSetup = false;

        focus = target;

        yield return new WaitUntil(() =>
        {
            if (!deadByFall)
            {
                if (Vector3.Distance(transform.position, newPosition) <= 5.0f)
                {
                    newPosition = transform.position;
                }

                //Director translation to position itself
                if (transform.position.y <= 4.5f)
                {
                    transform.position = new Vector3(transform.position.x, 4.5f, transform.position.z);
                }
            }
            else
            {
                Vector3 positionAbove = new Vector3(target.transform.position.x, 10.0f, target.transform.position.z);

                newPosition = positionAbove;

                if (Vector3.Distance(transform.position, positionAbove) <= 5.0f)
                {
                    newPosition = transform.position;
                }
            }


            transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref zero, 0.5f, transitionSpeed);
            showDirector.transform.position = Vector3.Slerp(showDirector.transform.position, transform.position, 2.0f * Time.deltaTime);

            //Death Track rotation to face player dead body
            Vector3 lookPos = target.transform.position - tracksEvent[2].transform.position;
            lookPos.y = 0;
            Quaternion rotation = Quaternion.LookRotation(lookPos);
            tracksEvent[2].transform.rotation = Quaternion.Slerp(tracksEvent[2].transform.rotation, rotation, 1.0f);

            return Vector3.Distance(transform.position, newPosition) <= 0.1f;
        });

        isSetup = true;
        speed = 1.0f;

        showDirector.GetComponent<CinemachineDollyCart>().m_Path = newTrack;
        showDirector.GetComponent<CinemachineDollyCart>().m_Position = 0.0f;

        currentCoroutine = StartCoroutine(DelayUntilNextPosition(Maxdelay));
    }

    public IEnumerator SwitchToSpawnerPosition(float transitionSpeed, GameObject target, float Maxdelay)
    {
        lookAtGift = true;
        Vector3 newPosition = Vector3.zero;
        showDirector.GetComponent<CinemachineDollyCart>().m_Path = null;
        isSetup = false;

        focus = target;
        CheckAngle();

        //DOWN
        if (angle >= 0.0f && angle < 45.0f || angle >= 315.0f && angle <= 360.0f)
        {
            newPosition = posSpawner[2].position;

            if (transform.position.x > 0.0f)
            {
                speed = -0.5f;
            }
            else
            {
                speed = 0.5f;
            }

        }
        //LEFT
        if (angle >= 45.0f && angle < 135.0f)
        {
            newPosition = posSpawner[3].position;

            if (transform.position.z > 0.0f)
            {
                speed = 0.5f;
            }
            else
            {
                speed = -0.5f;
            }
        }
        //UP
        if (angle >= 135.0f && angle < 225.0f)
        {
            newPosition = posSpawner[0].position;

            if (transform.position.x > 0.0f)
            {
                speed = 0.5f;
            }
            else
            {
                speed = -0.5f;
            }
        }
        //RIGHT
        if (angle >= 225.0f && angle < 315.0f)
        {
            newPosition = posSpawner[1].position;

            if (transform.position.z > 0.0f)
            {
                speed = -0.5f;
            }
            else
            {
                speed = 0.5f;
            }
        }

        yield return new WaitUntil(() =>
        {
            transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref zero, 0.5f, transitionSpeed);

            showDirector.transform.position = Vector3.Slerp(showDirector.transform.position, transform.position, 2.0f * Time.deltaTime);

            //Track rotation to face spawner
            Vector3 lookPos = target.transform.position - tracksEvent[0].transform.position;
            lookPos.y = 0;
            Quaternion rotation = Quaternion.LookRotation(lookPos);
            tracksEvent[0].transform.rotation = Quaternion.Slerp(tracksEvent[0].transform.rotation, rotation, 1.0f);

            return Vector3.Distance(transform.position, newPosition) <= 0.1f;
        });

        isSetup = true;

        showDirector.GetComponent<CinemachineDollyCart>().m_Path = tracksEvent[0];
        showDirector.GetComponent<CinemachineDollyCart>().m_Position = 0.0f;

        currentCoroutine = StartCoroutine(DelayUntilNextPosition(Maxdelay));
    }

    public IEnumerator SwitchToVictoriousPlayer(Vector3 newPosition, float transitionSpeed, GameObject target)
    {
        showDirector.GetComponent<CinemachineDollyCart>().m_Path = null;
        isSetup = false;

        focus = target;

        yield return new WaitUntil(() =>
        {
           
            Vector3 newPos = new Vector3(newPosition.x, newPosition.y + 3.0f, newPosition.z);
            transform.position = Vector3.SmoothDamp(transform.position, newPos, ref zero, 0.5f, transitionSpeed);

            showDirector.transform.position = Vector3.Slerp(showDirector.transform.position, transform.position, 2.0f * Time.deltaTime);

            //Track rotation to face player
            Vector3 lookPos = target.transform.position - tracksEvent[0].transform.position;
            lookPos.y = 0;
            Quaternion rotation = Quaternion.LookRotation(lookPos);
            tracksEvent[0].transform.rotation = Quaternion.Slerp(tracksEvent[0].transform.rotation, rotation, 1.0f);

            return Vector3.Distance(new Vector3(center.position.x, 0.0f, center.position.z), new Vector3(newPosition.x, 0.0f, newPosition.z)) <= 1.0f;
        });


        yield return new WaitUntil(() =>
        {

            Vector3 newPos = new Vector3(transform.position.x, newPosition.y + 3.0f, transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, newPos, ref zero, 0.5f, transitionSpeed);

            return Vector3.Distance(new Vector3(0.0f, transform.position.y, 0.0f), new Vector3(0.0f, newPos.y, 0.0f)) <= 0.1f;
        });


        isSetup = true;
        speed = 0.2f;

        showDirector.GetComponent<CinemachineDollyCart>().m_Path = tracksEvent[0];
        showDirector.GetComponent<CinemachineDollyCart>().m_Position = 0.0f;
    }

    IEnumerator DelayUntilNextPosition(float delay)
    {
        yield return new WaitForSeconds(delay);
        lookAtGift = false;

        float rng = RandomPositionGenerator(0, 11);
        rngPlayer = Random.Range(0, DataManager.instance.currentNbrPlayer);

        StartCoroutineSwitchPosition(standbyPosition[(int)rng].position, tracksEvent[1], 0, 15.0f, DataManager.instance.player[rngPlayer].gameObject, true, 8.0f);
    }


    float RandomPositionGenerator(int min, int max)
    {
        float rng = Random.Range(min, max);

        if (currentPosIndex == rng)
        {
            rng = Random.Range(min, max);
        }
        currentPosIndex = (int)rng;

        return rng;
    }


}
