using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class QuadTextureAnimation : MonoBehaviour {

    [SerializeField] bool displacement = false;
    [SerializeField] Vector2 startPosition = Vector2.zero;
    [SerializeField] Vector2 endPosition = Vector2.zero;
    [SerializeField] Vector2 sizeUV = Vector2.one;

    [SerializeField] int nbColumn = 1;
    [SerializeField] int nbRow = 1;
    [SerializeField] float duration = 1.0f;

    [SerializeField] public bool playOnAwake = false;
    [SerializeField] public bool loop = false;
    [SerializeField] public bool destroyAfterPlaying = false;
    [SerializeField] public int startFrame = 0;
    [SerializeField] public float speed = 1.0f;

    Mesh mesh;
    List<Vector2> startUV = null;

    [HideInInspector] public bool playing = false;

    private Coroutine animationCoroutine = null;

    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        startUV = new List<Vector2>();
        mesh.GetUVs(0, startUV);
    }

    // Use this for initialization
    void Start () {
        if (playOnAwake)
        {
            Play();
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    public void Play()
    {
        if(animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(PlayAnimation());
    }

    IEnumerator PlayAnimation()
    {
        float step = 1.0f / (nbRow * nbColumn);
        playing = true;
        
        float timer = 0.0f;
        int totalFrame = nbRow * nbColumn;
        yield return new WaitUntil(() =>
        {
            // timer is the normailzed time in the animation
            timer += Time.deltaTime / duration;

            // return to start if looping (also prevent from exiting the coroutine)
            if (timer >= 1.0f)
            {
                timer = (loop) ? timer - 1.0f : 1.0f;
            }

            else
            {
                if (displacement)
                {
                    SetOffset(Vector2.Lerp(startPosition, endPosition, timer));
                }
                else
                {
                    int currentFrame = startFrame + (int)(totalFrame * timer * speed);
                    currentFrame %= totalFrame; // keep frame in total frame
                    ChangeFrame(currentFrame / nbColumn, currentFrame % nbColumn);
                    //Debug.Log(gameObject.name + " | Timer : " + timer + " | Frame : " + currentFrame + " | Position : " + (currentFrame / nbColumn) + ":" + (currentFrame % nbColumn));
                }
            }
            return timer >= 1.0f;
        });

        playing = false;
        if(destroyAfterPlaying)
        {
            Destroy(gameObject);
        }
    }

    void ChangeFrame(int row, int column)
    {
        List<Vector2> uvs = new List<Vector2>();
        for(int i = 0; startUV != null && i < startUV.Count; i++)
        {
            uvs.Add(new Vector2(
                (column + startUV[i].x) / nbColumn,
                (row + startUV[i].y) / nbRow
                ));
        }
        mesh.SetUVs(0, uvs);
    }

    void SetOffset(Vector2 offset)
    {
        List<Vector2> uvs = new List<Vector2>();
        for (int i = 0; startUV != null && i < startUV.Count; i++)
        {
            uvs.Add(Vector2.Scale(startUV[i], sizeUV) + offset);
        }
        mesh.SetUVs(0, uvs);
    }

}
