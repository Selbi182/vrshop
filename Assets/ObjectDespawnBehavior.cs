using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDespawnBehavior : MonoBehaviour {


    void OnTriggerEnter(Collider other) {
        GameObject g = other.gameObject;
        if (g.transform.parent == transform.parent) {
            Destroy(other.gameObject);
        }
    }
}
