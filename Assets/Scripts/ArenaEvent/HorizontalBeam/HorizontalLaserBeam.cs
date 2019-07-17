using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class HorizontalLaserBeam : MonoBehaviour
{
    private LineRenderer line;

    [SerializeField]
    HorizontalAimBeam aimBeam;

    HorizontalLaserGenerator beamGenerator;

    GameObject particle;

    Vector3 startPos;
    Vector3 endPos;

    AudioSource source;

    public bool isStarting;
    public bool isHeating;
    bool isActive;

    Vector3 velocity = Vector3.zero;
    float rayLength = 100.0f;
    
    float maxBeamWidth = 0.7f;
    float heatingBeamWidth = 0.2f;

    ArenaEventManager arenaEventManager;

    void Start()
    {
        arenaEventManager = transform.parent.GetComponentInParent<ArenaEventManager>();

        line = GetComponent<LineRenderer>();
        beamGenerator = GetComponentInParent<HorizontalLaserGenerator>();
        particle = transform.GetChild(2).gameObject;

        isStarting = false;
        line.enabled = false;
        isActive = false;
        isHeating = false;

        particle.GetComponent<ParticleSystem>().Stop();

        StartCoroutine(CheckTile());
    }

    void Update()
    {

        startPos = transform.GetChild(0).GetComponent<Transform>().position;
        endPos = transform.GetChild(1).GetComponent<Transform>().position;


        if (isStarting)
        {
            SoundManager.instance.PlaySound(65, 0.2f, AudioType.SFX);
            StartCoroutine(StartLaser(beamGenerator.delay, beamGenerator.duration));
            isStarting = false;
        }

        //RaycastManager();
        UpdateLength();
        PositionUpdate();
    }

    void PositionUpdate()
    {
        line.SetPosition(0, startPos);
        line.SetPosition(1, endPos);

        particle.transform.position = endPos;
    }

    public IEnumerator StartLaser(float activationDelay, float Duration)
    {
        aimBeam.isActive = true;
        // Wait for the end of the coroutine
        yield return StartCoroutine(IncreaseLaser(activationDelay));
        line.startWidth = maxBeamWidth;
        
        isActive = true;
        particle.GetComponent<ParticleSystem>().Play();
        source = SoundManager.instance.PlaySound(66, 0.2f, true, AudioType.SFX);
        yield return new WaitForSeconds(Duration);

        isActive = false;
        particle.GetComponent<ParticleSystem>().Stop();
        SoundManager.instance.StopSound(source);

        source = SoundManager.instance.PlaySound(67, 0.2f, AudioType.SFX);
        aimBeam.isActive = false;
        // Wait for the end of the coroutine
        StartCoroutine(DecreaseLaser(activationDelay));

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


    void RaycastManager()
    {       
        if (isActive)
        {
            RaycastHit[] hitsPlayer = Physics.SphereCastAll(startPos, 0.7f, Vector3.Normalize(endPos - startPos), (endPos - startPos).magnitude, LayerMask.GetMask("Character"));

            foreach (RaycastHit h in hitsPlayer)
            {
                if (isActive && h.transform.tag == "Player")
                {
                    //h.transform.GetComponent<Player>().currentHealth -= 5.0f;
                    h.transform.GetComponent<Player>().GetDamage(10.0f);
                }
            }
        }
    }

    void UpdateLength()
    {
        RaycastHit hitsHexagon;

        if (Physics.Raycast(startPos, Vector3.Normalize(endPos - startPos), out hitsHexagon, rayLength, LayerMask.GetMask("Hexagon")))
        {
            if (hitsHexagon.transform.gameObject.layer == LayerMask.NameToLayer("Hexagon"))
            {
                endPos = hitsHexagon.point;
            }
            else
            {
                endPos = aimBeam.endPos;
            }
        }
    }

    // Check raycast every 0.1 seconds
    // Active liguid or baril and damage players
    public IEnumerator CheckTile()
    {
        while (true)
        {
            RaycastManager();
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void StopLaser(float delayStop)
    {
        StopAllCoroutines();

        isActive = false;
        particle.GetComponent<ParticleSystem>().Stop();
        SoundManager.instance.StopSound(source);

        //source = SoundManager.instance.PlaySound(67, 0.2f, AudioType.SFX);
        aimBeam.isActive = false;
        StartCoroutine(DecreaseLaser(delayStop));
    }
}