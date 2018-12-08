using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SineLight : MonoBehaviour {

    private Light lightComponent;
    private float baseRange;
    public float speed;
    
	void Start () {
        lightComponent = GetComponent<Light>();
        baseRange = lightComponent.range;
	}
	
	void FixedUpdate () {
        lightComponent.range = (Mathf.Sin(Time.frameCount * speed) + 1f) * baseRange;
	}
}
