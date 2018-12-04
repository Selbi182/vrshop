using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using AsImpL;
using System;

public class ShopItemSpawner : MonoBehaviour {

    public bool isImporting;
    public GameObject spawnedObject;
    public GameObject spawnLocation;
    public float noGravityRotationSpeed = 1f;

    private ObjectImporter objImporter;
    public ImportOptions importOptions;
    private const string OBJ_PATTERN = "*.obj";

    private Dictionary<string, GameObject> importedModelsCache;

    private VRShopArticle article;
    private string articleModelPath;

    void Awake() {
        isImporting = false;
        importedModelsCache = new Dictionary<string, GameObject>();

        objImporter = gameObject.AddComponent<ObjectImporter>();

        // Gets called when the model has been imported
        objImporter.ImportedModel += (importedGameObject, path) => {
            GameObject g;
            try {
                g = importedGameObject.GetComponentInChildren<MeshRenderer>().gameObject;
            } catch (NullReferenceException) {
                g = importedGameObject;
            }

            // Set initial locations and meta data
            Transform t = g.transform;
            t.parent = spawnLocation.transform;
            t.localPosition = Vector3.zero;
            t.rotation = Quaternion.identity;
            t.name = string.Format("({0}) {1}", article.Id.ToString(), article.Name);

            // Scale the object
            float scaleFactor = (float)article.ScaleFactor;
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

            // Cache the spawned object
            spawnedObject = g;
            GameObject cachedModel = Instantiate(g, transform);
            cachedModel.SetActive(false);
            cachedModel.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            importedModelsCache[articleModelPath] = cachedModel;

            // Destroy the container, if unequal
            if (importedGameObject != g) {
                Destroy(importedGameObject);
            }

            isImporting = false;
        };
    }

    void FixedUpdate() {
        if (spawnedObject != null) {
            spawnLocation.transform.Rotate(new Vector3(0, noGravityRotationSpeed, 0));
        }
    }

    public void ImportAndSpawnShopItem(VRShopArticle selectedArticle) {
        if (isImporting) {
            Debug.LogWarning("Cannot import another model while model importing is still in progress!");
            return;
        }

        // Only spawn objects where a model is available
        article = selectedArticle;
        articleModelPath = GetModelPath(article);
        if (articleModelPath == null) {
            return;
        }

        // Remove the previously spawned object if it hasn't been picked up yet
        DestroyHoveringObject();
        
        // Reload cached objects
        if (importedModelsCache.ContainsKey(articleModelPath) && importedModelsCache[articleModelPath] != null) {
            GameObject g = Instantiate(importedModelsCache[articleModelPath], spawnLocation.transform);
            g.SetActive(true);

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

            isImporting = false;
            return;
        }
        
        // Import the Model
        string spawnedGameObjectName = selectedArticle.Name;
        objImporter.ImportModelAsync(spawnedGameObjectName, articleModelPath, transform, importOptions);
    }

    public static void SendToTrashcan(GameObject g) {
        if (g != null) {
            Destroy(g);
        }
    }

    public void DestroyHoveringObject() {
        if (spawnedObject != null) {
            UnsetParticles();
            
            // Cache the potential previous object occupying the spawn slot
            SendToTrashcan(spawnedObject);
            spawnedObject = null;
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
        string articleModelFolder = Path.Combine(VRShopDBConnector.ARTICLE_FOLDER_PATH, id);

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
