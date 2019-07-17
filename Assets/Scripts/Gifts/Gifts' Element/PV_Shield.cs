using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PV_Shield : MonoBehaviour {

    public bool newShield;

    Coroutine shield = null;

    private void Start()
    {
        shield = StartCoroutine(ShieldCoroutine());
    }

    private void Update()
    {
        if(newShield == true)
        {
            StopCoroutine(shield);
            shield = StartCoroutine(ShieldCoroutine());
            newShield = false;
        }

        if (transform.parent.GetComponent<Player>().isPV_Shielded == false)
        {
            Destroy(gameObject);
        }
    }

    public IEnumerator ShieldCoroutine()
    {
        yield return new WaitForSeconds(15.0f);

        transform.parent.GetComponent<Player>().isPV_Shielded = false;
        transform.parent.GetComponent<Player>().currentShieldPower = 0.0f;
    }
}
