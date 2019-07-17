using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraGlitchFX : MonoBehaviour {

    [SerializeField] Shader FXShader;
    private Material FXMaterial;
    [SerializeField] float amplitude;
    [SerializeField] float position;
    [SerializeField] float width;

    private void Start()
    {
        StartCoroutine(RandomCoroutine());
    }

    void CreateMaterial () {
		if(FXMaterial == null)
        {
            FXMaterial = new Material(FXShader);
            FXMaterial.hideFlags = HideFlags.HideAndDontSave;
        }
        //FXMaterial.SetFloat("_Amplitude", amplitude); // set amplitude
        //FXMaterial.SetFloat("_Position", position); // set position
        //FXMaterial.SetFloat("_Width", width); // set width
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        CreateMaterial();
        Graphics.Blit(source, destination, FXMaterial);
    }


    IEnumerator RandomCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.2f + Random.value * (2.0f - 0.2f)); // wait between 0.2 and 2 seconds
            FXMaterial.SetFloat("_Amplitude", (2.0f * Random.value - 1.0f) * amplitude); // set amplitude
            FXMaterial.SetFloat("_Position", Random.value); // set position
            FXMaterial.SetFloat("_Width", Random.value * width); // set width
            yield return new WaitForSeconds(Random.value * 0.1f); // wait between 0.1 and 0.2 seconds
            FXMaterial.SetFloat("_Amplitude", 0.0f); // reset amplitude
            FXMaterial.SetFloat("_Position", 0.0f); // set position
            FXMaterial.SetFloat("_Width", 0.0f); // set width
        }
    }
}
