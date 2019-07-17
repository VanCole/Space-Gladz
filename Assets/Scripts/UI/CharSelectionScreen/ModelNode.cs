using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelNode : MonoBehaviour
{
    RectTransform tf;

    public enum TypeModel
    {
        male,
        female
    }
    public TypeModel chooseModel;

    public bool isSelected;

    // Use this for initialization
    void Start()
    {
        isSelected = false;
        tf = GetComponent<RectTransform>();

        if (chooseModel == (int)TypeModel.male)
        {
            isSelected = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isSelected)
        {
            Vector3 tempEndSize = new Vector2(100.0f, 100.0f);
            tf.sizeDelta = Vector2.Lerp(tf.sizeDelta, tempEndSize, 0.2f);
        }
        else
        {
            Vector3 tempEndSize = new Vector2(80.0f, 80.0f);
            tf.sizeDelta = Vector2.Lerp(tf.sizeDelta, tempEndSize, 0.2f);
        }
    }
}
