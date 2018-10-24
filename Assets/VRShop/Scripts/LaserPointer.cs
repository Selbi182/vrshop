using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserPointer : MonoBehaviour {

    public GameObject laserVisuals;
    public GameObject laserCollider;

    public static string TINT_COLOR = "_TintColor";
    public static Color monitorActive   = new Color(0.376465f, 0.731f, 0.6063015f, 0.5921569f);
    public static Color MONITOR_INACTIVE = new Color(0.376465f, 0.6063015f, 0.731f, 0.8921569f);

    public GameObject shopExplorer;
    private GameObject targetObject;
    private GameObject targetObjectThisFrame = null;
    private float closestTargetObjectThisFrameDistance = float.MaxValue;
    

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

        if (targetObject != null
            && !isGrabbing
            && Controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger)) {
            
            // Send the information that a screen has been selected for preview
            shopExplorer.SendMessage("SelectScreen", targetObject);

            // Spawn shop item (just delegate the selected monitor to the method and let it do the actual logic)
            //shopItemSpawner.SendMessage("SpawnShopItem", targetObject);
            SendMessage("HapticPulseDoLerp", 1f/30f);
        }    
    }

    public void ClearTarget() {
        SetLaserColor(Color.red);
        SetLaserLength(laserLength);
        ResetMonitorColor(targetObject);
        targetObject = null;
    }

    public void SetTarget(GameObject target) {
        // Find the closest collision this frame
        float distance = Vector3.Distance(laserVisuals.transform.position, target.transform.position);
        if (distance < closestTargetObjectThisFrameDistance) {
            closestTargetObjectThisFrameDistance = distance;
            targetObjectThisFrame = target;
        }

    }

    private void LateUpdate() {
        if (targetObjectThisFrame != null) {
            // Block double set
            if (targetObjectThisFrame == targetObject) {
                return;
            }

            // To prevent race conditions
            if (targetObject != null) {
                ClearTarget();
            }
            targetObject = targetObjectThisFrame;

            SetLaserColor(Color.green);
            SetLaserLength(closestTargetObjectThisFrameDistance);
            SetMonitorColor(targetObject, monitorActive);
            SendMessage("HapticPulseDo", 0.5f);

            closestTargetObjectThisFrameDistance = float.MaxValue;
            targetObjectThisFrame = null;
        }
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

    private static void SetMonitorColor(GameObject monitor, Color color) {
        if (monitor != null) {
            monitor.transform.GetComponent<Renderer>().material.SetColor(LaserPointer.TINT_COLOR, color);
        }
    }

    public static void ResetMonitorColor(GameObject monitor) {
        if (monitor != null) {
            monitor.transform.GetComponent<Renderer>().material.SetColor(LaserPointer.TINT_COLOR, LaserPointer.MONITOR_INACTIVE);
        }
    }
}



