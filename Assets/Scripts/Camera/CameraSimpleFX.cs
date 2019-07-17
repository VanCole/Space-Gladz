using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraSimpleFX : MonoBehaviour {

    [SerializeField] Shader FXShader;
    private Material FXMaterial;
    [SerializeField] float amplitude;


	void CreateMaterial () {
		if(FXMaterial == null)
        {
            FXMaterial = new Material(FXShader);
            FXMaterial.hideFlags = HideFlags.HideAndDontSave;
        }
        FXMaterial.SetFloat("_Amplitude", amplitude);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        CreateMaterial();
        Graphics.Blit(source, destination, FXMaterial);
    }
}
