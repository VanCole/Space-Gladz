using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour {

    Collider collider;
    [SerializeField] float duration;

    //public bool isExploding;
    
	IEnumerator Start () {
        collider = GetComponent<Collider>();
        //isExploding = true;
        yield return new WaitForSeconds(duration);
        collider.enabled = false;
        //isExploding = false;
    }
}
