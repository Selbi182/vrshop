using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ShopItemSpawner : MonoBehaviour {

    public GameObject spawnedObject;
    public bool isEnbaled = true;
    public Vector3 spawnLocation = new Vector3(0f, 5f, 0f);
    public float noGravityRotationSpeed = 1f;

    private readonly string ASSET_BUNDLE_PATH = @"Assets/AssetBundles";

    void FixedUpdate() {
        if (spawnedObject != null) {
            spawnedObject.transform.Rotate(new Vector3(0, noGravityRotationSpeed, 0));
        }
    }


    public void SpawnShopItem(VRShopArticle selectedArticle) {
        if (isEnbaled) {
            // Only spawn objects where a model is available
            string assetBundleName = selectedArticle.GetAssetBundleNameIfModelExists();
            string assetBundlePath = Path.Combine(ASSET_BUNDLE_PATH, assetBundleName);
            if (!File.Exists(assetBundlePath)) {
                return;
            }

            // Remove the previously spawned object if it hasn't been picked up yet
            DestroyHoveringObject();
            
            // Unload existing AssetBundles first that match the name of the newly loaded one
            // Don't remove spawned objects that arem't in the hovering slot, though
            foreach (AssetBundle abl in AssetBundle.GetAllLoadedAssetBundles()) {
                if (abl.name.Equals(assetBundleName)) {
                    abl.Unload(false);
                }
            }

            // Fetch the asset bundle from the path
            AssetBundle ab = AssetBundle.LoadFromFile(assetBundlePath);

            // Get all enclosed assets of the bundle and make sure the number of containing assets is EXACTLY 1
            string[] assets = ab.GetAllAssetNames();
            if (ab == null || assets.Length != 1) {
                return;
            }

            // Instantiate the object (imply that the first entry in the asset bundle is the only one)
            AssetBundleRequest abr = ab.LoadAssetAsync(assets[0]);
            foreach (GameObject g in abr.allAssets) {
                // Find the GameObject that contains the MeshRenderer
                GameObject articleSpawnObject = g.GetComponentInChildren<MeshRenderer>().gameObject;

                // Set initial locations and meta data
                Transform t = articleSpawnObject.transform;
                t.parent = transform;
                t.localPosition = spawnLocation;
                t.rotation = Quaternion.identity;

                // Scale the object
                float scaleFactor = (float)selectedArticle.ScaleFactor;
                t.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

                // Remove all other Collider components first, just in case any are loaded
                foreach (Collider c in articleSpawnObject.GetComponents<Collider>()) {
                    DestroyImmediate(c, true);
                }

                // Add a generic BoxCollider (MeshColliders would be more accurate, but they're really expensive on complex models with lots of faces)
                BoxCollider bc = articleSpawnObject.AddComponent<BoxCollider>();

                // Add a MeshCollider to this object with reduced polycount
                //MeshCollider mc = asset.AddComponent<MeshCollider>();
                //mc.cookingOptions = MeshColliderCookingOptions.InflateConvexMesh | mc.cookingOptions;
                //mc.convex = true;

                // Add a Rigidbody, but don't enable gravity yet
                Rigidbody r = articleSpawnObject.AddComponent<Rigidbody>();
                r.useGravity = false;
                r.isKinematic = !r.useGravity;

                // Spawn the object
                Instantiate(articleSpawnObject);

                // Remember the spawned object
                spawnedObject = articleSpawnObject;
            }
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
            r.isKinematic = !r.useGravity;
            spawnedObject = null;
        }
    }
}
