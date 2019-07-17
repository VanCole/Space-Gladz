using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuBlackScreen : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        StartCoroutine(FadeOutCanvas(gameObject));
    }

    // Update is called once per frame
    void Update()
    {

    }

    public IEnumerator FadeOutCanvas(GameObject canvas)
    {
        while (canvas.GetComponent<CanvasGroup>().alpha > 0.0f)
        {
            canvas.GetComponent<CanvasGroup>().alpha -= 0.1f;
            yield return new WaitForSeconds(0.03f);
        }
        canvas.SetActive(false);
    }
}
