using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraBlurFX : MonoBehaviour {

    [SerializeField] float shiftOffset = 0.005f;
    [SerializeField] int nbPass = 3;
    [SerializeField] [Range(0, 2)] float opacity = 1.0f;
    [SerializeField] Shader FXShaderHBlur, FXShaderVBlur, FXShaderAdd;
    private Material FXMaterialHBlur, FXMaterialVBlur, FXMaterialAdd;
    [SerializeField] float amplitude;


	void CreateMaterial () {
        if (FXMaterialHBlur == null)
        {
            FXMaterialHBlur = new Material(FXShaderHBlur);
            FXMaterialHBlur.hideFlags = HideFlags.HideAndDontSave;
        }
        if (FXMaterialVBlur == null)
        {
            FXMaterialVBlur = new Material(FXShaderVBlur);
            FXMaterialVBlur.hideFlags = HideFlags.HideAndDontSave;
        }

        if (FXMaterialAdd == null)
        {
            FXMaterialAdd = new Material(FXShaderAdd);
            FXMaterialAdd.hideFlags = HideFlags.HideAndDontSave;
        }

        FXMaterialHBlur.SetFloat("_Shift", shiftOffset);
        FXMaterialVBlur.SetFloat("_Shift", shiftOffset);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        CreateMaterial();
        RenderTexture rtHBlur = RenderTexture.GetTemporary(Screen.width, Screen.height);
        RenderTexture rtVBlur = RenderTexture.GetTemporary(Screen.width, Screen.height);

        Graphics.Blit(source, rtVBlur);

        for (int i = 0; i < nbPass; i++)
        {
            Graphics.Blit(rtVBlur, rtHBlur, FXMaterialHBlur); // Horizontal blur
            Graphics.Blit(rtHBlur, rtVBlur, FXMaterialVBlur); // Vertical blur
        }
        FXMaterialAdd.SetTexture("_MainTex2", rtVBlur);
        FXMaterialAdd.SetFloat("_Coef", opacity);
        Graphics.Blit(source, destination, FXMaterialAdd);
        
        RenderTexture.ReleaseTemporary(rtHBlur);
        RenderTexture.ReleaseTemporary(rtVBlur);
    }
}
