using UnityEngine;
using System.Collections;

public class BloodEffect : MonoBehaviour {
    public float fadeTime = 2f;

    SpriteRenderer renderer;

	void Start () {
        renderer = GetComponent<SpriteRenderer>();
        Destroy(gameObject, fadeTime);
	}
	
	void Update () {
        renderer.color = Color.Lerp(renderer.color, Color.clear, (1 / fadeTime) * Time.deltaTime);
	}
}
