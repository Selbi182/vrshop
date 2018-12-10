using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserPointer : MonoBehaviour {

    public GameObject laserVisuals;
    public GameObject laserCollider;

    public GameObject shopExplorer;
    private ShopExplorerBehavior explorer;
    public GameObject leftScrollButton;
    public GameObject rightScrollButton;
    public GameObject searchButton;
    private GameObject targetObject;
    private GameObject targetObjectThisFrame = null;
    private float closestTargetObjectThisFrameDistance = float.MaxValue;
    private Outline outlineComponent;

    private const string COMPARE_TAG = "LaserTarget";
    

    private readonly float laserLength = 50f;
    
    // Steam VR Stuff
    private SteamVR_Controller.Device Controller { get { return SteamVR_Controller.Input((int)trackedObj.index); } }
    private SteamVR_TrackedObject trackedObj;

    void Start () {
        trackedObj = GetComponent<SteamVR_TrackedObject>();

        // Replace the prefabs with clones at startup
        laserVisuals = GameObject.Instantiate(laserVisuals, transform);
        laserCollider = GameObject.Instantiate(laserCollider, transform);

        // Get the template outline object
        outlineComponent = GetComponent<Outline>();

        // Get the shop explorer
        if (shopExplorer != null) {
            explorer = shopExplorer.GetComponent<ShopExplorerBehavior>();
        }
    }

    void Update() {
        // Check if the controller is hovering over an object
        bool isHovering = GetComponent<PickupAndMoveObjects>().pickupObj != null;
        laserVisuals.gameObject.SetActive(!isHovering);
        laserCollider.gameObject.SetActive(!isHovering);
        if (isHovering) {
            ClearTarget();
        }

        // Events when pressing the trigger button without grabbing an object
        if (Controller.GetPress(SteamVR_Controller.ButtonMask.Trigger) && !isHovering) {
            // Differenet behavior for different buttons
            if (targetObject == leftScrollButton) {
                SendMessage("NotifyButtonScroll", -1);
            } else if (targetObject == rightScrollButton) {
                SendMessage("NotifyButtonScroll", 1);
            } else if (Controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger)) {
                // Every handle for single-frame button presses
                if (targetObject != null) {
                    if (targetObject == searchButton) {
                        StartSearch();
                    } else if (targetObject.transform.parent.name.Equals("Cart")) {
                        // Handle cart presses
                        targetObject.SendMessageUpwards("HandleCartSelection", targetObject);
                        SendMessage("HapticPulseDo", 0.5f);
                    } else {
                        // Send the command that an article screen has been selected for preview
                        targetObject.GetComponent<ArticleMonitorWrapper>().Select();
                        SendMessage("HapticPulseDoLerp", 1f / 30f);
                    }
                }
            }
        } else if (Controller.GetPressDown(SteamVR_Controller.ButtonMask.Grip)) {
            // When pressing the grip button, reset selected target
            ClearTarget();
            explorer.UnselectScreen();
            SendMessage("HapticPulseDo", 0.5f);
        } else if (Controller.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu)) {
            StartSearch();
        }
    }

    private void StartSearch() {
        searchButton.transform.parent.GetComponent<SearchStart>().StartSearch();
        SendMessage("HapticPulseDoLerp", 1f / 30f);
    }

    public void ClearTarget() {
        SetLaserColor(Color.red);
        SetLaserLength(laserLength);
        if (targetObject != null) {
            targetObject.GetComponent<Outline>().enabled = false;
        }
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
            // To prevent race conditions
            if (targetObject != null) {
                ClearTarget();
            }

            SetLaserLength(closestTargetObjectThisFrameDistance);

            if (targetObjectThisFrame.CompareTag(COMPARE_TAG)) {
                // Prevent haptic pulse spam
                if (targetObjectThisFrame != targetObject) {
                    SendMessage("HapticPulseDo", 0.5f);
                }

                SetLaserColor(Color.green);

                targetObject = targetObjectThisFrame;

                Outline o = targetObject.GetComponent<Outline>();
                if (o == null) {
                    o = targetObject.AddComponent<Outline>();
                    o.OutlineColor = outlineComponent.OutlineColor;
                    o.OutlineWidth = outlineComponent.OutlineWidth;
                }
                o.enabled = true;

            }
        }
        closestTargetObjectThisFrameDistance = float.MaxValue;
        targetObjectThisFrame = null;
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

}



