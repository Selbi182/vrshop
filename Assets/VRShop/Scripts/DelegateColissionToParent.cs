using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelegateColissionToParent : MonoBehaviour {

    public string tagToCompare;
    
    private void OnTriggerStay(Collider other) {
        if (other.tag.Equals(tagToCompare)) {
            gameObject.SendMessageUpwards("SetTarget", other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other) {
        gameObject.SendMessageUpwards("ClearTarget");
    }
}
