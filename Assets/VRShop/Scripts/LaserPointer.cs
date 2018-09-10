using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserPointer : MonoBehaviour {

    public GameObject laserVisuals;
    public GameObject laserCollider;

    public Material monitorActive;
    public Material monitorInactive;

    private GameObject targetObject;
    public GameObject shopItemSpawner;

    private readonly float laserLength = 50f;
    
    // Steam VR Stuff
    private SteamVR_Controller.Device Controller { get { return SteamVR_Controller.Input((int)trackedObj.index); } }
    private SteamVR_TrackedObject trackedObj;

    void Start () {
        trackedObj = GetComponent<SteamVR_TrackedObject>();

        // Replace the prefabs with clones at startup
        laserVisuals = GameObject.Instantiate(laserVisuals, transform);
        laserCollider = GameObject.Instantiate(laserCollider, transform);
	}

    void Update() {
        bool isGrabbing = GetComponent<PickupAndMoveObjects>().isGrabbing;
        laserVisuals.gameObject.SetActive(!isGrabbing);
        laserCollider.gameObject.SetActive(!isGrabbing);
        if (isGrabbing) {
            ClearTarget();
        }

        if (shopItemSpawner != null
            && targetObject != null
            && !isGrabbing
            && Controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger)) {

            // Spawn shop item (just delegate the selected monitor to the method and let it do the actual logic)
            shopItemSpawner.SendMessage("SpawnShopItem", targetObject);
            SendMessage("HapticPulseDoLerp", 1f/30f);
        }    
    }

    public void ClearTarget() {
        SetLaserColor(Color.red);
        SetLaserLength(laserLength);
        SetMonitorMaterial(monitorInactive);
        targetObject = null;
    }

    public void SetTarget(GameObject target) {
        // Block double set
        if (target == targetObject) {
            return;
        }

        // To prevent race conditions
        if (targetObject != null) {
            ClearTarget();
        }
        targetObject = target;

        SetLaserColor(Color.green);
        SetLaserLength(Vector3.Distance(laserVisuals.transform.position, targetObject.transform.position));
        SetMonitorMaterial(monitorActive);
        SendMessage("HapticPulseDo", 0.5f);
    }

    private void SetLaserColor(Color color) {
        if (laserVisuals != null) {
            laserVisuals.GetComponent<LineRenderer>().startColor = color;
            laserVisuals.GetComponent<LineRenderer>().endColor = color;
        }
    }

    private void SetLaserLength(float length) {
        if (laserVisuals != null && laserVisuals.GetComponent<LineRenderer>().positionCount > 1) {
            laserVisuals.GetComponent<LineRenderer>().SetPosition(1, new Vector3(0f, 0f, length));
        }
    }

    private void SetMonitorMaterial(Material material) {
        if (targetObject != null) {
            targetObject.GetComponent<MeshRenderer>().material = material;
        }
    }
}



