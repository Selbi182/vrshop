using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopSwipe : MonoBehaviour {

    public float scrollSpeed;
    public float scrollThreshold;
    public GameObject shopExplorer;

    private float lastX = 0f;
    private int lastTimestamp;

    // Steam VR Stuff
    private SteamVR_Controller.Device Controller { get { return SteamVR_Controller.Input((int)trackedObj.index); } }
    private SteamVR_TrackedObject trackedObj;

    // Use this for initialization
    void Start () {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        lastTimestamp = Time.frameCount;
    }

    // Update is called once per frame
    void FixedUpdate() {
        float currentX = Controller.GetAxis().x;
        int currentTimestamp = Time.frameCount;
        if (currentX != 0f) {
            if (lastX == 0f) {
                lastX = currentX;
            }
            if (Mathf.Abs(currentX - lastX) > scrollThreshold) {
                if (shopExplorer != null) {
                    
                    float newOffset = ((float)lastTimestamp / Mathf.Pow((float)currentTimestamp,2f)) * scrollSpeed * Mathf.Sign(lastX- currentX);
                    shopExplorer.SendMessage("UpdateOffset", newOffset);
                }
                lastX = currentX;
                SendMessage("HapticPulseDo", 0.25f);
            }
        } else {
            lastX = 0f;
        }
        lastTimestamp = currentTimestamp;
    }
}
