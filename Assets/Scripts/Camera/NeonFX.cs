using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class NeonFX : MonoBehaviour {
    public CameraNeon cameraNeon;
    private Material FXMaterial;

    [SerializeField] float shiftOffset = 0.005f;
    [SerializeField] int nbPass = 3;
    [SerializeField] [Range(0, 2)] float glowCoef = 1.0f;
    [SerializeField] [Range(1, 6)] float whitePow = 3.0f;
    [SerializeField] [Range(0, 2)] float whiteCoef = 1.0f;
    public Shader FXShaderHBlur, FXShaderVBlur, FXShaderNeon;
    private Material FXMaterialHBlur, FXMaterialVBlur, FXMaterialNeon;

    void CreateMaterials()
    {
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

        if (FXMaterialNeon == null)
        {
            FXMaterialNeon = new Material(FXShaderNeon);
            FXMaterialNeon.hideFlags = HideFlags.HideAndDontSave;
        }

        FXMaterialHBlur.SetFloat("_Shift", shiftOffset);
        FXMaterialVBlur.SetFloat("_Shift", shiftOffset);

        FXMaterialNeon.SetFloat("_GlowCoef", glowCoef);
        FXMaterialNeon.SetFloat("_WhiteCoef", whiteCoef);
        FXMaterialNeon.SetFloat("_WhitePow", whitePow);
    }

	void OnRenderImage(RenderTexture source,RenderTexture destination) //Fonction appelée par unity à chaque fin de rendu. C'est maintenant qu'on fait le post-effet
	{
        CreateMaterials();
        RenderTexture rtEmission = RenderTexture.GetTemporary(Screen.width, Screen.height);
        RenderTexture rtHBlur = RenderTexture.GetTemporary(Screen.width, Screen.height);
        RenderTexture rtVBlur = RenderTexture.GetTemporary(Screen.width, Screen.height);

        Graphics.Blit(cameraNeon.rt, rtEmission);

        for (int i = 0; i < nbPass; i++)
        {
           Graphics.Blit(rtEmission, rtHBlur, FXMaterialHBlur); // Horizontal blur
           Graphics.Blit(rtHBlur, rtEmission, FXMaterialVBlur); // Vertical blur
        }

        FXMaterialNeon.SetTexture("_NeonTex", cameraNeon.rt);
        FXMaterialNeon.SetTexture("_BlurredNeonTex", rtEmission);
        Graphics.Blit(source, destination, FXMaterialNeon);

        RenderTexture.ReleaseTemporary(rtEmission);
        RenderTexture.ReleaseTemporary(rtHBlur);
        RenderTexture.ReleaseTemporary(rtVBlur);
    }
}

