using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopSwipe : MonoBehaviour {

    public float scrollSpeed;
    public float scrollThreshold;
    public GameObject shopExplorer;

    private float lastX = 0f;
    private int lastTimestamp;
    public float pressButtonScrollSpeed = 1f;
    public float pressButtonMinimumAxis = 0.75f;
    private readonly float hapticPulseIntensity = 0.25f;
    private int buttonPressSignum = 0;

    // Steam VR Stuff
    private SteamVR_Controller.Device Controller { get { return SteamVR_Controller.Input((int)trackedObj.index); } }
    private SteamVR_TrackedObject trackedObj;
    private const Valve.VR.EVRButtonId DPAD_RIGHT = Valve.VR.EVRButtonId.k_EButton_DPad_Right;

    // Use this for initialization
    void Start () {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        lastTimestamp = Time.frameCount;
        buttonPressSignum = 0;
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (Controller == null) {
            return;
        }

        int currentTimestamp = Time.frameCount;

        // Button press check
        if (buttonPressSignum != 0 || Controller.GetPress(SteamVR_Controller.ButtonMask.Touchpad)) {
            if (buttonPressSignum  > 0 || Controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).x > pressButtonMinimumAxis) {
                shopExplorer.SendMessage("UpdateOffset", pressButtonScrollSpeed);
                SendMessage("HapticPulseDo", hapticPulseIntensity);
            } else if (buttonPressSignum < 0 || Controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).x < -pressButtonMinimumAxis)  {
                shopExplorer.SendMessage("UpdateOffset", -pressButtonScrollSpeed);
                SendMessage("HapticPulseDo", hapticPulseIntensity);
            }
        } else {
            // Swipe check
            float currentX = Controller.GetAxis().x;
            if (currentX != 0f) {
                if (lastX == 0f) {
                    lastX = currentX;
                }
                if (Mathf.Abs(currentX - lastX) > scrollThreshold) {
                    if (shopExplorer != null) {
                        float newOffset = ((float)lastTimestamp / Mathf.Pow((float)currentTimestamp, 2f)) * scrollSpeed * Mathf.Sign(lastX - currentX);
                        shopExplorer.SendMessage("UpdateOffset", -newOffset);
                    }
                    lastX = currentX;
                    SendMessage("HapticPulseDo", hapticPulseIntensity);
                }
            } else {
                lastX = 0f;
            }
        }
        lastTimestamp = currentTimestamp;
        buttonPressSignum = 0;
    }

    public void NotifyButtonScroll(int direction) {
        buttonPressSignum = (int)Mathf.Sign(direction);
    }
}
