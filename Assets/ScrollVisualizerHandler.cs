using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollVisualizerHandler : MonoBehaviour {

    public GameObject leftButton;
    public GameObject rightButton;
    public GameObject positionMarker;
    public GameObject positionMarkerBox;

    public Color disabledColor;
    public Color enabledColor;
    private const string TINT_COLOR = "_TintColor";
    private const string SCREEN_SELECTABLE = "LaserTarget";
    private const string SCREEN_NOTSELECTABLE = "Untagged";

    private GameObject shopExplorer;
    private float markerBoxScale;

    void Start () {
        shopExplorer = transform.parent.gameObject;
        markerBoxScale = positionMarkerBox.transform.localScale.x;
	}
	
	void FixedUpdate () {
        // Change the position of the marker according to the range 0..maximumOffset where actualOffset is the defining position at this frame's moment
        float actualOffset = shopExplorer.GetComponent<ShopExplorerBehavior>().actualOffset;
        float maximumOffset = shopExplorer.GetComponent<ShopExplorerBehavior>().maximumOffset;
        float translatedMarkerPosition = ((actualOffset / maximumOffset) * markerBoxScale) - (markerBoxScale / 2f);

        // Apply the new position
        Vector3 newPos = positionMarker.transform.position;
        newPos.x = translatedMarkerPosition;
        positionMarker.transform.position = newPos;

        // Disable the buttons according to the position
        leftButton.tag = SCREEN_SELECTABLE;
        rightButton.tag = SCREEN_SELECTABLE;
        SetColor(leftButton, enabledColor);
        SetColor(rightButton, enabledColor);
        if (actualOffset == 0f) {
            leftButton.tag = SCREEN_NOTSELECTABLE;
            SetColor(leftButton, disabledColor);
        } else if (actualOffset == maximumOffset) {
            rightButton.tag = SCREEN_NOTSELECTABLE;
            SetColor(rightButton, disabledColor);
        }
	}

    private void SetColor(GameObject button, Color color) {
        button.transform.GetComponent<Renderer>().material.SetColor(TINT_COLOR, color);
    }
}
