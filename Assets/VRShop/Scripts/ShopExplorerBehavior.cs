using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopExplorerBehavior : MonoBehaviour {

    // Public variables
    public int screenCount;
    public int screensPerRow;
    public float spacingX;
    public float spacingY;
    public float distanceFromCenter;
    public GameObject prefabScreenContainer;

    public GameObject shopItemSpawner;
    public GameObject infoScreen;
    private GameObject selectedScreen;
    public float selectionSpeed;

    // Used for swiping
    private enum Direction {
        LEFT  = -1,
        STILL = 0,
        RIGHT = +1
    };
    private Direction swipeDirection;
    public float offset = 0f;
    public float actualOffset = 0f;

    // Used for transparency
    private Color screenColor;
    private readonly string TINT_COLOR = "_TintColor";
    private readonly string SCREEN_SELECTABLE = "LaserTarget";
    private readonly string SCREEN_NOTSELECTABLE = "Untagged";

    // Collection of instantiated screens
    private IList<GameObject> screens;

    void Start() {
        swipeDirection = Direction.STILL;
        screenColor = prefabScreenContainer.GetComponent<Renderer>().sharedMaterial.GetColor(TINT_COLOR);

        // Spawn the prefabs
        screens = new List<GameObject>();
        for (int i = 1; i < screenCount + 1; i++) {
            GameObject newScreenObj = GameObject.Instantiate(prefabScreenContainer, transform);
            screens.Add(newScreenObj);
        }
    }

    void FixedUpdate() {
        // Make it smooth af
        
        offset = Mathf.Lerp(offset, 0f, Time.deltaTime);
        if (Mathf.Abs(offset) > 2f) {
            offset = Mathf.Sign(offset) * 2f;
        }

        if (Mathf.Abs(offset) < 0.01f) {
            offset = 0f;
        }

        actualOffset += offset;
        if (actualOffset > 0f) {
            // Left boundary scrolling
            actualOffset = 0f;
        }

        // Iterate through all screens and update their position
        int num = 0;
        float y = 0;
        foreach (GameObject screen in screens) {
            // Update the screen's positon using simple trigenometry scaled over the frame counter.
            // As a result, the screen will circle around the player (specifically, the position of ShopExplorer).
            Vector3 pos = new Vector3();
            float radian = (((spacingX * num++) + actualOffset) % 360f) / 180f * Mathf.PI;
            pos.x = Mathf.Cos(radian) * distanceFromCenter;
            pos.z = Mathf.Sin(radian) * distanceFromCenter;

            pos.y = y;
            if (num % screensPerRow == 0) {
                y += spacingY;
                num = 0;
            }

            // Apply updates to unslected screens
            // Skip it for a selected screen
            if (screen != selectedScreen) {
                screen.transform.position = pos;
                OrientateScreen(screen);

                // If the screen is behind the user, steadily increase the transparency
                float sin = Mathf.Sin(radian);
                if (sin < 0f) {
                    screen.tag = SCREEN_NOTSELECTABLE;
                    LaserPointer.ResetMonitorColor(screen);

                    Color transparentScreenColor = screenColor;
                    transparentScreenColor.a = Mathf.Max(0.5f - (Mathf.Abs(sin)), 0f);
                    screen.transform.GetComponent<Renderer>().material.SetColor(TINT_COLOR, transparentScreenColor);
                } else {
                    screen.tag = SCREEN_SELECTABLE;
                }
            }
        }

        // Move the selected monitor towards its destination
        if (selectedScreen != null) {
            selectedScreen.transform.position = Vector3.Slerp(
                selectedScreen.transform.position,
                infoScreen.transform.position,
                Time.deltaTime * selectionSpeed
            );

            selectedScreen.transform.rotation = Quaternion.Slerp(
                selectedScreen.transform.rotation,
                infoScreen.transform.rotation,
                Time.deltaTime * selectionSpeed
            );
        }
        //OrientateScreen(infoScreen);
    }

    public void SelectScreen(GameObject screen) {
        if (selectedScreen != null) {
            SetSpawnButtonActive(selectedScreen, false);
        }
        selectedScreen = screen;
        SetSpawnButtonActive(selectedScreen, true);
    }

    public void UpdateOffset(float newOffset) {
        Direction newSwipeDirection = Direction.STILL;
        if (newOffset == 0f) {
            swipeDirection = newSwipeDirection;
            return;
        } else if (newOffset < 0) {
            newSwipeDirection = Direction.LEFT;
        } else if (newOffset > 0) {
            newSwipeDirection = Direction.RIGHT;
        }

        if (newSwipeDirection == swipeDirection) {
            offset += newOffset;
            return;
        }
        offset = newOffset;
        swipeDirection = newSwipeDirection;
    }

    private void OrientateScreen(GameObject screen) {
        // Update the rotation to keep looking at the player.
        screen.transform.rotation = Quaternion.LookRotation(transform.parent.position - screen.transform.position);
        Vector3 tmpAngles = screen.transform.localEulerAngles;
        tmpAngles.x = 0;
        screen.transform.localEulerAngles = tmpAngles;
    }

    private void SetSpawnButtonActive(GameObject screen, bool active) {
        for (int i = 0; i < screen.transform.childCount; i++) {
            screen.transform.GetChild(i).gameObject.SetActive(active);
        }
    }
}