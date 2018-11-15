using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelegateColissionToParent : MonoBehaviour {
    
    void Start() {
        Collider c1 = GetComponent<Collider>();
        Transform t = transform.parent;
        while (t != null) {
            Collider c2 = t.GetComponent<Collider>();
            if (c2 != null) {
                Physics.IgnoreCollision(c1, c2);
            }
            t = t.parent;
        }

    }

    private void OnTriggerStay(Collider other) {
        if (other.transform.parent != transform.parent) {
            gameObject.SendMessageUpwards("SetTarget", other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other) {
        gameObject.SendMessageUpwards("ClearTarget");
    }
}
