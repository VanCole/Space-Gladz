using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class VerticalLaserBeam : MonoBehaviour
{
    private LineRenderer line;

    VerticalBeamGenerator beamGenerator;

    [SerializeField]
    GameObject target;

    Vector3 playerPos;
    Vector3 startPos;
    Vector3 endPos;

    AudioSource source;

    [SerializeField]
    int indexPlayer;

    public bool isStarting;
    bool isActive;

    Vector3 velocity = Vector3.zero;
    float rayLength = 100.0f;

    bool canActivateItem;

    float maxBeamWidth = 0.7f;
    float heatingBeamWidth = 0.2f;

    Vector3 hitPos;

    ArenaEventManager arenaEventManager;

    void Start()
    {
        arenaEventManager = transform.parent.GetComponentInParent<ArenaEventManager>();

        line = GetComponent<LineRenderer>();
        beamGenerator = GetComponentInParent<VerticalBeamGenerator>();

        isStarting = false;
        line.enabled = false;
        isActive = false;
        canActivateItem = true;
        target.GetComponent<ParticleSystem>().Stop();

        StartCoroutine(CheckTile());
    }

    void Update()
    {
        if (GameManager.instance.gameInitialized)
        {
            if (indexPlayer < DataManager.instance.currentNbrPlayer)
            {
                playerPos = DataManager.instance.player[indexPlayer].transform.position;
            }

            if (isStarting)
            {
                SoundManager.instance.PlaySound(65, 0.2f, AudioType.SFX);
                arenaEventManager.PlayRandomEventSound();
                StartCoroutine(StartLaser(beamGenerator.delay, beamGenerator.duration));
                //StartCoroutine(IncreaseLaser());
                isStarting = false;
            }

            //RaycastManager();
            PositionUpdate();
        }
    }

    void PositionUpdate()
    {
        startPos = new Vector3(target.transform.position.x, target.transform.position.y + 100.0f, target.transform.position.z);
        endPos = new Vector3(playerPos.x, playerPos.y - 0.5f, playerPos.z);

        line.SetPosition(0, startPos);
        line.SetPosition(1, target.transform.position);

        target.transform.position = new Vector3(Mathf.SmoothDamp(target.transform.position.x, endPos.x, ref velocity.x, beamGenerator.smoothTime),
                                                hitPos.y, //playerPos.y - 0.5f,
                                                Mathf.SmoothDamp(target.transform.position.z, endPos.z, ref velocity.z, beamGenerator.smoothTime));

        //target.transform.position = Vector3.SmoothDamp(target.transform.position, endPos, ref velocity, beamGenerator.smoothTime);
    }

    public IEnumerator StartLaser(float activationDelay, float Duration)
    {
        target.transform.position = endPos;
        
        // Wait for the end of the coroutine
        yield return StartCoroutine(IncreaseLaser(activationDelay));
        line.startWidth = maxBeamWidth;

        isActive = true;
        target.GetComponent<ParticleSystem>().Play();
        source = SoundManager.instance.PlaySound(66, 0.2f, true, AudioType.SFX);


        yield return new WaitForSeconds(Duration);
        isActive = false;
        target.GetComponent<ParticleSystem>().Stop();
        SoundManager.instance.StopSound(source);

        source = SoundManager.instance.PlaySound(67, 0.2f, AudioType.SFX);
        // Wait for the end of the coroutine
        yield return StartCoroutine(DecreaseLaser(activationDelay));

        //SoundManager.instance.StopSound(source);
        arenaEventManager.isEventActive = false;
    }

    public IEnumerator IncreaseLaser(float _duration)
    {
        line.startWidth = 0.0f;
        line.enabled = true;
        while (line.startWidth < heatingBeamWidth)
        {
            line.startWidth += heatingBeamWidth * Time.deltaTime / _duration;
            yield return 0;
        }
    }

    public IEnumerator DecreaseLaser(float _duration)
    {
        while (line.startWidth > 0.0f)
        {
            line.startWidth -= maxBeamWidth * Time.deltaTime / _duration;
            yield return 0;
        }
        line.enabled = false;
    }

    // Check raycast every 0.1 seconds
    // Active liguid or baril and damage players
    public IEnumerator CheckTile()
    {
        while (true)
        {
            RaycastManager();

        //if (canActivateItem)
        //    {
        //        Tile tile = hit.collider.transform.parent.gameObject.GetComponent<Tile>();
        //        if (tile)
        //        {
        //            if (!tile.isActive && tile.Enable)
        //            {
        //                if (tile.Type != Tile.TileType.Turret && tile.Type != Tile.TileType.Hole)
        //                {
        //                    tile.Activate();
        //                }
        //            }
        //        }
        //        canActivateItem = false;
        //    }
            yield return new WaitForSeconds(0.1f);

            //canActivateItem = true;
        }

    }


    void RaycastManager()
    {
        RaycastHit hit;


        if (isActive)
        {
            if (Physics.SphereCast(startPos, 0.7f, -transform.up, out hit, rayLength, LayerMask.GetMask("Character")))
            {
                if (hit.transform.tag == "Player")
                {
                    //hit.transform.GetComponent<Player>().currentHealth -= 5.0f;
                    hit.transform.GetComponent<Player>().GetDamage(10.0f);
                    //Debug.Log(hit.transform.name);
                }
            }

            if (Physics.SphereCast(startPos, 0.7f, -transform.up, out hit, rayLength, LayerMask.GetMask("Hexagon")))
            {
                hitPos = hit.point;
                //if (isActive && hit.transform.gameObject.layer == LayerMask.NameToLayer("Hexagon"))
                //{
                //StartCoroutine(CheckTile(hit));
                //}

                Tile tile = hit.collider.transform.parent.gameObject.GetComponent<Tile>();
                if (tile)
                {
                    if (!tile.isActive && tile.Enable)
                    {
                        if (tile.Type == Tile.TileType.Baril || tile.Type == Tile.TileType.Liquid)
                        {
                            tile.Activate();
                        }
                    }
                }
            }
        }
    }


    public void StopLaser(float delayStop)
    {
        StopAllCoroutines();

        isActive = false;

        target.GetComponent<ParticleSystem>().Stop();
        SoundManager.instance.StopSound(source);
        //source = SoundManager.instance.PlaySound(67, 0.2f, AudioType.SFX);

        StartCoroutine(DecreaseLaser(delayStop));
    }
}