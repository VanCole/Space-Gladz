using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraNeon : MonoBehaviour {

    enum Resolution { Full, DividedBy2, DividedBy4, DividedBy8 }

    Camera cam;
    [SerializeField] Shader neonShader;
    [SerializeField] Resolution renderResolution;
    [HideInInspector] public RenderTexture rt;

	// Use this for initialization
	void Start ()
    {
        cam = GetComponent<Camera>();
        //neonShader = Shader.Find("Custom/NeonShader");
    }

    private void OnPreRender()
    {
        if (cam)
        {
            cam.SetReplacementShader(neonShader, "RenderType");
            if (cam.targetTexture != null)
            {
                cam.targetTexture = null;
                RenderTexture.ReleaseTemporary(rt);
            }
            float resolution = GetResolution();
            rt = RenderTexture.GetTemporary((int)(Screen.width * resolution), (int)(Screen.height * resolution), 24);
            cam.targetTexture = rt;
        }
        else
        {
            rt = null;
        }
    }


    float GetResolution()
    {
        float f = 0.0f;
        switch(renderResolution)
        {
            case Resolution.Full:
                f = 1.0f;
                break;
            case Resolution.DividedBy2:
                f = 0.5f;
                break;
            case Resolution.DividedBy4:
                f = 0.25f;
                break;
            case Resolution.DividedBy8:
                f = 0.125f;
                break;
        }
        return f;
    }
}
