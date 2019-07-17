using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class AboveLight : MonoBehaviour
{
    //MENU
    float delayMenu = 0.5f;
    float maxIntensityMenu = 2.0f;
    float minIntensityMenu = 0.0f;
    float incrementMenu = 0.1f;
    float speedMenu = 0.05f;

    [SerializeField] float angleMenu = 6.0f;
    [SerializeField] float RangeMenu = 200.0f;


    //Arena
    float delayArena = 3.5f;
    float maxIntensityArena = 3.0f;
    float minIntensityArena = 0.0f;
    float incrementArena = 0.1f;
    float speedArena = 0.05f;

    [SerializeField] float angleArena = 50.0f;
    [SerializeField] float RangeArena = 200.0f;




    bool isIncreasing;

    float maxIntensity = 2.0f;
    float minIntensity = 0.0f;

    float increment = 0.1f;
    float speedVariation = 0.05f;


    DataManager.GameState gamestate;


    IEnumerator TurnOnSpotlight(float Delay, float maxIntensity, float speed, float increment)
    {
        GetComponent<Light>().intensity = 0.0f;

        yield return new WaitForSeconds(Delay);
        while (GetComponent<Light>().intensity <= maxIntensity)
        {
            yield return new WaitForSeconds(speedVariation);
            GetComponent<Light>().intensity += increment;
        }
    }

    IEnumerator TurnOffSpotlight()
    {
        while (GetComponent<Light>().intensity > minIntensity)
        {
            yield return new WaitForSeconds(speedVariation);
            GetComponent<Light>().intensity -= increment * 3.0f;
        }
    }

    // Use this for initialization
    void Start()
    {
        isIncreasing = true;
        GetComponent<Light>().intensity = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (DataManager.instance.gameState != DataManager.GameState.inArena)
        {
            GetComponent<Light>().spotAngle = angleMenu;
            GetComponent<Light>().range = RangeMenu;

        }
        else
        {
            GetComponent<Light>().spotAngle = angleArena;
            GetComponent<Light>().range = RangeArena;

        }

        Vector3 direction = Vector3.zero - GetComponent<Transform>().position;
        Quaternion lookAtCenter = Quaternion.LookRotation(direction);
        transform.rotation = lookAtCenter;

        if (DataManager.instance.gameState == DataManager.GameState.charSelection && DataManager.instance.gameState != gamestate)
        {
            StartCoroutine(TurnOnSpotlight(delayMenu, maxIntensityMenu, speedMenu, incrementMenu));
        }
        else if (DataManager.instance.gameState == DataManager.GameState.mainMenu && DataManager.instance.gameState != gamestate)
        {
            StartCoroutine(TurnOffSpotlight());
        }
        else if (DataManager.instance.gameState == DataManager.GameState.inArena && DataManager.instance.gameState != gamestate)
        {
            StartCoroutine(TurnOnSpotlight(delayArena, maxIntensityArena, speedArena, incrementArena));
        }


        gamestate = DataManager.instance.gameState;
    }
}
