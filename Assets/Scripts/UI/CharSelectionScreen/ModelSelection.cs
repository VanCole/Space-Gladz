using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;


public class ModelSelection : MonoBehaviour
{
    [SerializeField]
    GameObject[] nodeModel = new GameObject[2];

    EventSystem EventSystem;


    public void SelectModel()
    {
        int selectedModel = (int)EventSystem.currentSelectedGameObject.GetComponent<ModelNode>().chooseModel;
        //Debug.Log(selectedModel);

        for (int i = 0; i < 2; i++)
        {
            if (selectedModel == (int)nodeModel[i].GetComponent<ModelNode>().chooseModel)
            {
                nodeModel[i].GetComponent<ModelNode>().isSelected = true;
            }
            else
            {
                nodeModel[i].GetComponent<ModelNode>().isSelected = false;
            }
        }
    }

    private void Start()
    {
        EventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();     
    }


    private void Update()
    {

    }
}
