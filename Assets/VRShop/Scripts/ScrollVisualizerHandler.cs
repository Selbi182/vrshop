using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollVisualizerHandler : MonoBehaviour {

    public GameObject leftButton;
    public GameObject rightButton;
    public GameObject positionMarker;
    public GameObject positionMarkerBox;

    private const string TINT_COLOR = "_TintColor";
    private const string SCREEN_SELECTABLE = "LaserTarget";
    private const string SCREEN_NOTSELECTABLE = "Untagged";

    private ShopExplorerBehavior shopExplorer;
    private float markerBoxScale;

    void Awake() {
        shopExplorer = transform.parent.GetComponent<ShopExplorerBehavior>();
        markerBoxScale = positionMarkerBox.transform.localScale.x;
	}
	
	void FixedUpdate () {
        if (shopExplorer.numberOfArticles > 0) {
            // Change the position of the marker according to the range 0..maximumOffset where actualOffset is the defining position at this frame's moment
            float actualOffset = shopExplorer.actualOffset;
            float maximumOffset = Mathf.Max(shopExplorer.maximumOffset, float.Epsilon);
            float translatedMarkerPosition = ((actualOffset / maximumOffset) * markerBoxScale) - (markerBoxScale / 2f);

            // Apply the new position
            Vector3 newPos = positionMarker.transform.localPosition;
            newPos.x = translatedMarkerPosition;
            positionMarker.transform.localPosition = newPos;

            // Disable the buttons according to the position
            leftButton.tag = SCREEN_SELECTABLE;
            rightButton.tag = SCREEN_SELECTABLE;
            SetColor(leftButton, shopExplorer.colorActive);
            SetColor(rightButton, shopExplorer.colorActive);
            if (actualOffset == 0f) {
                leftButton.tag = SCREEN_NOTSELECTABLE;
                SetColor(leftButton, shopExplorer.colorInactive);
            } else if (actualOffset == maximumOffset) {
                rightButton.tag = SCREEN_NOTSELECTABLE;
                SetColor(rightButton, shopExplorer.colorInactive);
            }
        } else {
            Vector3 newPos = positionMarker.transform.localPosition;
            newPos.x = 0f;
            positionMarker.transform.localPosition = newPos;

            leftButton.tag = SCREEN_NOTSELECTABLE;
            rightButton.tag = SCREEN_NOTSELECTABLE;
            SetColor(leftButton, shopExplorer.colorInactive);
            SetColor(rightButton, shopExplorer.colorInactive);
        }
    }

    private void SetColor(GameObject button, Color color) {
        button.transform.GetComponent<Renderer>().material.SetColor(TINT_COLOR, color);
    }
}
