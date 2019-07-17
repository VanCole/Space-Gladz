using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelDisplay : MonoBehaviour
{
    [SerializeField]
    GameObject[] modelScreen = new GameObject[4];

    bool mustHideModel;

    // Use this for initialization
    void Start()
    {
        for (int i = 0; i < modelScreen.Length; i++)
        {
            modelScreen[i].SetActive(false);
        }
        mustHideModel = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (DataManager.instance.gameState == DataManager.GameState.charSelection)
        {
            mustHideModel = false;
            for (int i = 0; i < DataManager.instance.currentNbrPlayer; i++)
            {
                modelScreen[i].SetActive(true);

                Vector3 direction = Camera.main.transform.position - modelScreen[i].GetComponent<Transform>().position;
                direction.y = 0.0f;
                Quaternion lookAtCenter = Quaternion.LookRotation(direction);

                modelScreen[i].GetComponent<Transform>().rotation = lookAtCenter;
            }

            if (DataManager.instance.currentNbrPlayer < DataManager.instance.player.Length)
            {
                for (int i = DataManager.instance.player.Length; i > DataManager.instance.currentNbrPlayer; --i)
                {
                    modelScreen[i - 1].SetActive(false);
                }
            }
        }
        else
        {
            StartCoroutine(HideModel());         
        }
    }




    IEnumerator HideModel()
    {
        if (!mustHideModel)
        {
            yield return new WaitForSeconds(0.5f);
            for (int i = 0; i < modelScreen.Length; i++)
            {
                modelScreen[i].SetActive(false);
            }
            mustHideModel = true;
        }    
    }
}
