using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Interactible : NetworkBehaviour {
    
    protected Vector3 offset = new Vector3(0, 0.6f, 0);
    protected Vector3 targetPosition = Vector3.zero;
    protected Vector3 zero = Vector3.zero;

    bool positionChanged = false;
    
    protected Tile tile;

    public AudioSource source;

    private Coroutine activationCoroutine = null;

    // Use this for initialization
    protected virtual void Start () {
        tile = transform.parent.gameObject.GetComponent<Tile>();
	}
	
	// Update is called once per frame
	protected virtual void Update () {
        if (positionChanged)
        {
            if ((transform.localPosition - (targetPosition + offset)).sqrMagnitude > 0.001f)
            {
                transform.localPosition = Vector3.SmoothDamp(transform.localPosition, targetPosition + offset, ref zero, 0.2f);
            }
            else
            {
                transform.localPosition = targetPosition + offset;
                positionChanged = false;
            }
        }
    }

    public void SetPosition(Vector3 _position)
    {
        positionChanged = true;
        targetPosition = _position;
    }

    public void Activate(float _delay = 0.0f)
    {
        if(DataManager.instance.isMulti)
        {
            CmdActivate(_delay);
        }
        else
        {
            LocalActivate(_delay);
        }
    }

    IEnumerator DelayedActivation(float _delay)
    {
        yield return new WaitForSeconds(_delay);
        if (!tile.isActive)
        {
            tile.isActive = true;
            Behaviour();
        }
    }

    public void CancelActivation()
    {
        if(activationCoroutine != null)
        {
            StopCoroutine(activationCoroutine);
            Debug.Log("Activation canceled");
        }
    }

    protected virtual void Behaviour()
    {
        // Do nothing but inheritance do something
    }

    public void SetColor(Color _c)
    {
        foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>())
        {
            foreach (Material m in mr.materials)
            {
                m.SetColor("_EmissionColor", _c);
            }
        }
    }



    ///////////// PRIVATE LOCAL FUNCTIONS //////////////
    private void LocalActivate(float _delay)
    {
        if (_delay > 0.0f)
        {
            activationCoroutine = StartCoroutine(DelayedActivation(_delay));
        }
        else
        {
            if (!tile.isActive)
            {
                tile.isActive = true;
                Behaviour();
            }
        }
    }


    ///////////// PRIVATE NETWORK FUNCTION ///////////////
    [Command]
    private void CmdActivate(float _delay)
    {
        RpcActivate(_delay);
    }

    [ClientRpc]
    private void RpcActivate(float _delay)
    {
        LocalActivate(_delay);
    }
}
