﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class PickupAndMoveObjects : MonoBehaviour {

    // CONSTANTS
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

    public float throwVelocity = 1f;

    [HideInInspector]
    public Boolean isGrabbing = false;

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
                isGrabbing = true;
            }
        } else if (Controller.GetPressUp(TRIGGER_BUTTON)) {
            if (fixedJoint.connectedBody != null) {
                fixedJoint.connectedBody = null;
                pickupObj.transform.GetComponent<Rigidbody>().velocity = Controller.velocity * throwVelocity;
                pickupObj = null;
                isGrabbing = false;
            }
        }
    }

    // Identify if we are in range of an object
    void OnTriggerStay(Collider other) {
        if (other.transform.parent.transform.Equals(pickupObjectsParent.transform)) {
            if (pickupObj == null) {
                SendMessage("HapticPulseDo", 1.0f);
            }
            pickupObj = other.gameObject;
        }
    }

    // Reset trigger
    void OnTriggerExit(Collider other) {
        if (!isGrabbing) {
            pickupObj = null;
        }
    }
}