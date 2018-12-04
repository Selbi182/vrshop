using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDespawnBehavior : MonoBehaviour {


    void OnTriggerEnter(Collider other) {
        GameObject g = other.gameObject;
        if (g.transform.parent == transform.parent) {
            ShopItemSpawner.SendToTrashcan(other.gameObject);
        }
    }
}
