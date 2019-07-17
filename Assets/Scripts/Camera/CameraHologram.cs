using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraHologram : MonoBehaviour {
    
    private Material FXMaterial;
    public Shader FXShaderScanline;
    private Material FXMaterialScanline;
    [SerializeField] Texture lineTexture;
    [SerializeField] int nbLine = 1;
    [SerializeField] [Range(0, 1)] float opacity = 1;
    [HideInInspector] public RenderTexture rt;
    Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    void CreateMaterials()
    {
        if (FXMaterialScanline == null)
        {
            FXMaterialScanline = new Material(FXShaderScanline);
            FXMaterialScanline.hideFlags = HideFlags.HideAndDontSave;
        }
        FXMaterialScanline.SetTexture("_LineTexture", lineTexture);
        FXMaterialScanline.SetInt("_NbLine", nbLine);
        FXMaterialScanline.SetFloat("_Opacity", opacity);
    }

    void CreateRenderTexture()
    {
        if (cam)
        {
            if (cam.targetTexture != null)
            {
                cam.targetTexture = null;
                RenderTexture.ReleaseTemporary(rt);
            }
            rt = RenderTexture.GetTemporary(Screen.width, Screen.height, 24);
            cam.targetTexture = rt;
        }
        else
        {
            rt = null;
        }
    }

    private void OnPreRender()
    {
        CreateRenderTexture();
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination) //Fonction appelée par unity à chaque fin de rendu. C'est maintenant qu'on fait le post-effet
    {
        CreateMaterials();
        //RenderTexture rtEmission = RenderTexture.GetTemporary(Screen.width, Screen.height);
        Graphics.Blit(source, destination, FXMaterialScanline);

        //RenderTexture.ReleaseTemporary(rtEmission);
        //RenderTexture.ReleaseTemporary(rtHBlur);
        //RenderTexture.ReleaseTemporary(rtVBlur);
    }
}
