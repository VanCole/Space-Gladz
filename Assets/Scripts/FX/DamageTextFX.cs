using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TextMesh))]
public class DamageTextFX : MonoBehaviour {
    [SerializeField] float duration = 1.0f;
    [SerializeField] float moveDistance = 1.0f;
    TextMesh mesh;
    [SerializeField] float spreading = 1.0f;
    float randomSpeed;

	// Use this for initialization
	void Start () {
        mesh = GetComponent<TextMesh>();
        randomSpeed = Random.Range(-spreading, spreading);

    }
	
	// Update is called once per frame
	void Update () {

        float speed = (duration > 0) ? Time.deltaTime / duration : 0.0f;

        Color color = mesh.color;
        color.a -= speed;
        mesh.color = color;

        transform.position += Vector3.up * moveDistance * speed + Vector3.right * randomSpeed * speed;

        if(color.a <= 0.0f)
        {
            Destroy(gameObject);
        }
	}
}
