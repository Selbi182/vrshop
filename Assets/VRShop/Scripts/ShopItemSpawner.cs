using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using AsImpL;
using System;

public class ShopItemSpawner : MonoBehaviour {

    public GameObject spawnedObject;
    public GameObject spawnLocation;
    public float noGravityRotationSpeed = 1f;

    private ObjectImporter objImporter;
    public ImportOptions importOptions;
    private const string OBJ_PATTERN = "*.obj";

    private Dictionary<string, GameObject> importedModels;

    void Awake() {
        objImporter = gameObject.AddComponent<ObjectImporter>();
        importedModels = new Dictionary<string, GameObject>();
    }

    void FixedUpdate() {
        if (spawnedObject != null) {
            spawnLocation.transform.Rotate(new Vector3(0, noGravityRotationSpeed, 0));
        }
    }

    public void ImportAndSpawnShopItem(VRShopArticle selectedArticle) {
        // Only spawn objects where a model is available
        string articleModelPath = GetModelPath(selectedArticle);
        if (articleModelPath == null) {
            return;
        }

        // Remove the previously spawned object if it hasn't been picked up yet
        DestroyHoveringObject();

        // Reload cached objects
        if (importedModels.ContainsKey(articleModelPath)) {
            GameObject g = Instantiate(importedModels[articleModelPath], spawnLocation.transform);

            // Set initial locations and meta data
            Transform t = g.transform;
            t.parent = spawnLocation.transform;
            t.localPosition = Vector3.zero;
            t.rotation = Quaternion.identity;

            Rigidbody r = g.GetComponent<Rigidbody>();
            r.useGravity = false;
            r.isKinematic = !r.useGravity;

            spawnLocation.SetActive(true);
            spawnedObject = g;
            return;
        }
        
        // Import the Model
        string spawnedGameObjectName = selectedArticle.Name;
        objImporter.ImportModelAsync(spawnedGameObjectName, articleModelPath, transform, importOptions);

        // Gets called when the model has been imported
        objImporter.ImportedModel += (importedGameObject, path) => {
            GameObject g;
            try {
                g = importedGameObject.GetComponentInChildren<MeshRenderer>().gameObject;
            } catch(NullReferenceException) {
                g = importedGameObject;
            }

            // Set initial locations and meta data
            Transform t = g.transform;
            t.parent = spawnLocation.transform;
            t.localPosition = Vector3.zero;
            t.rotation = Quaternion.identity;
            t.name = selectedArticle.Id.ToString();

            // Scale the object
            float scaleFactor = (float)selectedArticle.ScaleFactor;
            t.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

            // Remove all other Collider components first, just in case any are loaded
            foreach (Collider c in g.GetComponentsInChildren<Collider>()) {
                DestroyImmediate(c, true);
            }

            // Add a generic BoxCollider (MeshColliders would be more accurate, but they're really expensive on complex models with lots of faces)
            g.AddComponent<BoxCollider>();
            
            // Add a Rigidbody, but don't enable gravity yet
            Rigidbody r = g.AddComponent<Rigidbody>();
            r.useGravity = false;
            r.isKinematic = !r.useGravity;

            // Draw particles
            spawnLocation.SetActive(true);

            // Remember the spawned object
            spawnedObject = g;
            importedModels[articleModelPath] = g;

            // Destroy the container, if unequal
            if (importedGameObject != g) {
                Destroy(importedGameObject);
            }
        };
    }

    [Obsolete("Use ImportAndSpawnShopItem() instead")]
    public void SpawnShopItem(VRShopArticle selectedArticle) {
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
            g.AddComponent<BoxCollider>();

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

            // Set parent
            spawnedObject.transform.parent = transform;

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
        string articleModelFolder = Path.Combine(Path.Combine(Application.dataPath, VRShopDBConnector.ARTICLE_PATH), id);

        // Find the .obj file in this folder corresponding to the model
        // For consistency's sake, there should always only be one result in the above search
        string[] files = Directory.GetFiles(articleModelFolder, OBJ_PATTERN, SearchOption.TopDirectoryOnly);
        if (files.Length > 1) {
            Debug.LogWarningFormat("Multiple article models found for article '{0}'. Loading first file system instance only.", a.Name);
        }

        // Return the first valid path (which would be the only one, too)
        foreach (string f in files) {
            if (File.Exists(f)) {
                return f;
            }
        }

        // Otherwise, a scale factor is given but no model (which should be avoided for consistency's sake)
        return null;
    }
}
