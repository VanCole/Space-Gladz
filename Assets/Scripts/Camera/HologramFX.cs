using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class HologramFX : MonoBehaviour {
    public CameraHologram cameraHologram;
    public Shader FXShaderLayer;
    private Material FXMaterialLayer;

    void CreateMaterials()
    {
        if (FXMaterialLayer == null)
        {
            FXMaterialLayer = new Material(FXShaderLayer);
            FXMaterialLayer.hideFlags = HideFlags.HideAndDontSave;
        }

        FXMaterialLayer.SetTexture("_LayerTex", cameraHologram.rt);
        FXMaterialLayer.SetFloat("_Coef", 0.95f + 0.05f * Random.value);
    }

	void OnRenderImage(RenderTexture source,RenderTexture destination)
	{
        CreateMaterials();
        Graphics.Blit(source, destination, FXMaterialLayer);
    }
}

