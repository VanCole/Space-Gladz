using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraChroAberFX : MonoBehaviour {

    [SerializeField] Shader FXShader;
    private Material FXMaterial;
    [SerializeField] float amplitude;

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
            yield return new WaitForSeconds(0.1f + Random.value * (1.0f - 0.1f)); // wait between 0.2 and 2 seconds
            FXMaterial.SetFloat("_Amplitude", Random.value * amplitude); // set amplitude
            yield return new WaitForSeconds(0.1f + Random.value * 0.1f); // wait between 0.1 and 0.2 seconds
            FXMaterial.SetFloat("_Amplitude", 0.0f); // reset amplitude
        }
    }
}
