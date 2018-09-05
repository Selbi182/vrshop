using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class PickupAndMoveObjects : MonoBehaviour {

    // CONSTANTS
    private const ushort MAX_PULSE = 3999;
    private const string PICKUP = "Pickup";
    private const Valve.VR.EVRButtonId TRIGGER_BUTTON = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;
    
    // Steam VR Stuff
    private SteamVR_Controller.Device Controller { get { return SteamVR_Controller.Input((int)trackedObj.index); } }
    private SteamVR_TrackedObject trackedObj;

    // Container for the objects
    public GameObject pickupObjectsParent;

    // Connected Objects
    private GameObject pickupObj;
    private FixedJoint fixedJoint;

    // Initialize 
    void Start() {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        fixedJoint = GetComponent<FixedJoint>();
    }

    // Handles the pickup and release of objects
    void Update() {
        // Precondition to avoid NullPointerExceptions
        if (Controller == null) {
            return;
        }

        // Pickup object if trigger button is pressed
        // Release object if trigger button is released (with velocity)
        if (Controller.GetPressDown(TRIGGER_BUTTON)) {
            if (pickupObj != null) {
                fixedJoint.connectedBody = pickupObj.GetComponent<Rigidbody>();
            }
        } else if (Controller.GetPressUp(TRIGGER_BUTTON)) {
            if (fixedJoint.connectedBody != null) {
                fixedJoint.connectedBody = null;
                pickupObj.transform.GetComponent<Rigidbody>().velocity = Controller.velocity;
            }
        }
    }

    // Identify if we are in range of an object
    void OnTriggerStay(Collider other) {
        if (other.transform.parent.transform.Equals(pickupObjectsParent.transform)) {
            pickupObj = other.gameObject;
            HapticPulseDo(0.5f);
        }
    }

    // Reset trigger
    void OnTriggerExit(Collider other) {
        pickupObj = null;
    }

    // Utility method for haptic pulse for range float of 0.0..1.0
    void HapticPulseDo(float factor) {
        float inBounds = Mathf.Max(0f, Mathf.Min(1f, factor));

        // Calculate power based on factor 0.0..1.0
        ushort pulseForce = Convert.ToUInt16(Mathf.Min(MAX_PULSE * inBounds, MAX_PULSE) * 0.2f);

        // Send calculated pulse force to controller for this frame
        Controller.TriggerHapticPulse(pulseForce);
    }
}
