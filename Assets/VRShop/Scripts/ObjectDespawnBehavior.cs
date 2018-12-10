using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDespawnBehavior : MonoBehaviour {

    public GameObject despawnParticles;

    void OnTriggerEnter(Collider other) {
        GameObject g = other.gameObject;
        if (g.transform.parent == transform.parent) {
            ShopItemSpawner.SendToTrashcan(other.gameObject);
            if (despawnParticles != null) {
                Instantiate(despawnParticles, g.transform.position, transform.rotation, transform);
            }
        }
    }
}
