using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class HapticFeedback : MonoBehaviour {

    private const int MAX_PULSE = 3999;
    private const float EPSILON = 0.01f;

    private SteamVR_Controller.Device Controller {
        get {
            if (trackedObj != null) {
                return SteamVR_Controller.Input((int)trackedObj.index);
            }
            return null;
        }
    }
    private SteamVR_TrackedObject trackedObj;

    private float lerp;
    private float lerpSpeed;

    // Use this for initialization
    void Start () {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        lerp = 0f;
    }

    void FixedUpdate() {
        if (lerp > EPSILON) {
            lerp = Mathf.Lerp(lerp, 0f, lerpSpeed);
            HapticPulseDo(lerp);
        } else {
            lerp = 0f;
        }
    }

    // Utility method for haptic pulse for range float of 0.0..1.0
    public void HapticPulseDo(float factor) {
        float inBounds = Mathf.Max(0f, Mathf.Min(1f, factor));

        // Calculate power based on factor 0.0..1.0
        ushort pulseForce = Convert.ToUInt16(Mathf.Min(MAX_PULSE * inBounds, MAX_PULSE) * 0.2f);

        if (Controller != null) {
            // Send calculated pulse force to controller for this frame
            Controller.TriggerHapticPulse(pulseForce);
        }
    }

    // For a steadily decaying haptic pulse over the next few frames
    public void HapticPulseDoLerp(float setLerpSpeed) {
        lerp = 1f;
        lerpSpeed = setLerpSpeed;
        HapticPulseDo(lerp);
    }
}