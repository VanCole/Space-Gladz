using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsScreen : MonoBehaviour
{

    [SerializeField]
    GameObject[] playerStatPanel;


    // Use this for initialization
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        DisplayPanel();
    }

    void DisplayPanel()
    {
        if (DataManager.instance.currentNbrPlayer == 2)
        {
            playerStatPanel[0].gameObject.SetActive(true);
            playerStatPanel[1].gameObject.SetActive(true);
            playerStatPanel[2].gameObject.SetActive(false);
            playerStatPanel[3].gameObject.SetActive(false);


            playerStatPanel[0].GetComponent<RectTransform>().anchorMin = new Vector2(0.25f, 0.0f);
            playerStatPanel[0].GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1.0f);

            playerStatPanel[1].GetComponent<RectTransform>().anchorMin = new Vector2(0.50f, 0.0f);
            playerStatPanel[1].GetComponent<RectTransform>().anchorMax = new Vector2(0.75f, 1.0f);

           
        }
        if (DataManager.instance.currentNbrPlayer == 3)
        {
            playerStatPanel[0].gameObject.SetActive(true);
            playerStatPanel[1].gameObject.SetActive(true);
            playerStatPanel[2].gameObject.SetActive(true);
            playerStatPanel[3].gameObject.SetActive(false);


            playerStatPanel[0].GetComponent<RectTransform>().anchorMin = new Vector2(0.125f, 0.0f);
            playerStatPanel[0].GetComponent<RectTransform>().anchorMax = new Vector2(0.375f, 1.0f);

            playerStatPanel[1].GetComponent<RectTransform>().anchorMin = new Vector2(0.375f, 0.0f);
            playerStatPanel[1].GetComponent<RectTransform>().anchorMax = new Vector2(0.625f, 1.0f);

            playerStatPanel[2].GetComponent<RectTransform>().anchorMin = new Vector2(0.625f, 0.0f);
            playerStatPanel[2].GetComponent<RectTransform>().anchorMax = new Vector2(0.875f, 1.0f);

        }
        if (DataManager.instance.currentNbrPlayer == 4)
        {
            playerStatPanel[0].gameObject.SetActive(true);
            playerStatPanel[1].gameObject.SetActive(true);
            playerStatPanel[2].gameObject.SetActive(true);
            playerStatPanel[3].gameObject.SetActive(true);

            playerStatPanel[0].GetComponent<RectTransform>().anchorMin = new Vector2(0.0f, 0.0f);
            playerStatPanel[0].GetComponent<RectTransform>().anchorMax = new Vector2(0.25f, 1.0f);

            playerStatPanel[1].GetComponent<RectTransform>().anchorMin = new Vector2(0.25f, 0.0f);
            playerStatPanel[1].GetComponent<RectTransform>().anchorMax = new Vector2(0.50f, 1.0f);

            playerStatPanel[2].GetComponent<RectTransform>().anchorMin = new Vector2(0.50f, 0.0f);
            playerStatPanel[2].GetComponent<RectTransform>().anchorMax = new Vector2(0.75f, 1.0f);

            playerStatPanel[3].GetComponent<RectTransform>().anchorMin = new Vector2(0.75f, 0.0f);
            playerStatPanel[3].GetComponent<RectTransform>().anchorMax = new Vector2(1.0f, 1.0f);
        
        }
    }


    
}
