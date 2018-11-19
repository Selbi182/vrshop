using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ShopItemSpawner : MonoBehaviour {

    public GameObject spawnedObject;
    public bool isEnbaled = true;
    public GameObject spawnLocation;
    public float noGravityRotationSpeed = 1f;

    private readonly string ASSET_BUNDLE_PATH = "Assets" + Path.DirectorySeparatorChar + "AssetBundles";

    void FixedUpdate() {
        if (spawnedObject != null) {
            spawnedObject.transform.Rotate(new Vector3(0, noGravityRotationSpeed, 0));
        }
    }


    public void SpawnShopItem(VRShopArticle selectedArticle) {
        if (isEnbaled) {
            // Only spawn objects where a model is available
            string assetBundlePath = GetModelPath(selectedArticle);
            if (assetBundlePath == null) {
                return;
            }

            // Remove the previously spawned object if it hasn't been picked up yet
            DestroyHoveringObject();
            
            // Fetch the asset bundle from the path
            AssetBundle ab = AssetBundle.LoadFromFile(assetBundlePath);

            // Get all enclosed assets of the bundle and make sure the number of containing assets is EXACTLY 1
            string[] assets = ab.GetAllAssetNames();
            if (ab == null || assets.Length != 1) {
                return;
            }

            // Instantiate the object (imply that the first entry in the asset bundle is the only one)
            foreach (string s in assets) {
                // Load the asset
                GameObject g = ab.LoadAsset(s) as GameObject;

                // Find the GameObject that contains the MeshRenderer and spawn that
                GameObject articleSpawnObject = g.GetComponentInChildren<MeshRenderer>().gameObject;
                g = Instantiate(articleSpawnObject);

                // Set initial locations and meta data
                Transform t = g.transform;
                t.parent = transform;
                t.localPosition = spawnLocation.transform.position;
                t.rotation = Quaternion.identity;
                t.name = selectedArticle.Name;

                // Scale the object
                float scaleFactor = (float)selectedArticle.ScaleFactor;
                t.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

                // Remove all other Collider components first, just in case any are loaded
                foreach (Collider c in g.GetComponentsInChildren<Collider>()) {
                    DestroyImmediate(c, true);
                }

                // Add a generic BoxCollider (MeshColliders would be more accurate, but they're really expensive on complex models with lots of faces)
                BoxCollider bc = g.AddComponent<BoxCollider>();

                // Add a MeshCollider to this object with reduced polycount
                //MeshCollider mc = asset.AddComponent<MeshCollider>();
                //mc.cookingOptions = MeshColliderCookingOptions.InflateConvexMesh | mc.cookingOptions;
                //mc.convex = true;

                // Add a Rigidbody, but don't enable gravity yet
                Rigidbody r = g.AddComponent<Rigidbody>();
                r.useGravity = false;
                r.isKinematic = !r.useGravity;

                // Draw particles
                spawnLocation.SetActive(true);

                // Remember the spawned object
                spawnedObject = g;
            }

            // Unload AssetBundle after everything is done to free memory
            ab.Unload(false);
        }
    }

    public void DestroyHoveringObject() {
        if (spawnedObject != null) {
            UnsetParticles();

            // Delete the potential previous object occupying the spawn slot
            Destroy(spawnedObject);
        }
    }

    public void DetachHoveringObject() {
        if (spawnedObject != null) {
            UnsetParticles();

            // Detach the spawned object from the pickup region
            Rigidbody r = spawnedObject.GetComponent<Rigidbody>();
            r.useGravity = true;
            r.isKinematic = !r.useGravity;
            spawnedObject = null;
        }
    }

    private void UnsetParticles() {
        spawnLocation.SetActive(false);
    }

    private string GetModelPath(VRShopArticle a) {
        // Having no scaling factor implies the abscence of a model.
        if (a.ScaleFactor == null) {
            return null;
        }

        // Otherwise, check for the actual existance of a model based on the ID and return the path
        string id = a.Id.ToString();
        string assetBundlePath = Path.Combine(ASSET_BUNDLE_PATH, id);
        if (File.Exists(assetBundlePath)) {
            return assetBundlePath;
        }
        return null;
    }
}
