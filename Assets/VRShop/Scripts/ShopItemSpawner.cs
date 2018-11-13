using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopItemSpawner : MonoBehaviour {

    public GameObject spawnedObject;
    public bool isEnbaled = true;
    public Vector3 spawnLocation = new Vector3(0f, 5f, 0f);
    public float noGravityRotationSpeed = 1f;

    void FixedUpdate() {
        if (spawnedObject != null) {
            spawnedObject.transform.Rotate(new Vector3(0, noGravityRotationSpeed, 0));
        }
    }


    public void SpawnShopItem(VRShopArticle selectedArticle) {
        if (isEnbaled) {
            // Only spawn objects where a model is available
            if (!selectedArticle.HasModel()) {
                return;
            }

            // Remove the previously spawned object if it hasn't been picked up yet
            DestroyHoveringObject();

            // Instantiate the object in world space
            // TODO actual import
            GameObject articleSpawnObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Transform t = articleSpawnObject.transform;

            // Set initial locations and meta data
            t.parent = transform;
            t.localPosition = spawnLocation;
            t.rotation = Quaternion.identity;

            // Scale the object
            float scaleSize = (float)selectedArticle.Size;
            t.localScale = new Vector3(scaleSize, scaleSize, scaleSize);

            // Add physics to the object and freeze it in place on spawn
            Rigidbody r = articleSpawnObject.AddComponent<Rigidbody>();
            r.useGravity = false;
            r.isKinematic = true;

            // Remember the spawned object
            spawnedObject = articleSpawnObject;
        }
    }

    public void DestroyHoveringObject() {
        if (spawnedObject != null) {
            // Delete the potential previous object occupying the spawn slot
            Destroy(spawnedObject);
        }
    }

    public void DetachHoveringObject() {
        if (spawnedObject != null) {
            // Detach the spawned object from the pickup region
            Rigidbody r = spawnedObject.GetComponent<Rigidbody>();
            r.useGravity = true;
            r.isKinematic = false;
            spawnedObject = null;
        }
    }
}
