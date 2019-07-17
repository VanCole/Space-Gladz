using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenseDome : MonoBehaviour
{
    float lifeSpan = 10.0f;

    float size = 30.0f;
    float checkTreshold = 13.0f;

    AudioSource source;

    ArenaEventManager arenaEventManager;

    // Use this for initialization
    void Start()
    {
        arenaEventManager = GameObject.Find("ArenaEventGenerator").GetComponent<ArenaEventManager>();
        transform.localScale = new Vector3(size, 15.0f, size);
        arenaEventManager.activeDome = gameObject;


        source = SoundManager.instance.PlaySound(64, 0.5f, true, AudioType.SFX);
        StartCoroutine(DestroyShield());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator DestroyShield()
    {
        yield return new WaitForSeconds(lifeSpan);
        Destroy(gameObject);
        arenaEventManager.activeDome = null;
        arenaEventManager.isEventActive = false;
        SoundManager.instance.StopSound(source);
    }


    //IEnumerator CheckProjectile(Collider other)
    //{
    //    float firstDistance = Vector3.Distance(other.transform.position, transform.position);
    //    yield return new WaitForSeconds(0.05f);
    //    float secondDistance = Vector3.Distance(other.transform.position, transform.position);

    //    if (firstDistance - secondDistance > 0.0f)
    //    {
    //        Destroy(other.gameObject);
    //    }
    //}
}
