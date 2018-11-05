using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopExplorerBehavior : MonoBehaviour {

    public int numberOfArticles = 0;
    public int lowerArticleLoadIndex = 0;
    public int upperArticleLoadIndex = 0;

    // Public variables
    public int screenCount;
    public int screensPerColumn;
    public float spacingX;
    public float spacingY;
    public float initialYPos;
    public int firstColumn;
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
    public float maximumOffset = 0f;
    public float maximumScrollSpeed = 2f;
    private const float EPSILON = 0.01f;
    private const float BOUNDARY_DEGREE = 45f;

    // Used for transparency
    private Color screenColor;
    private const string TINT_COLOR = "_TintColor";
    public Color monitorActive = new Color(0.376465f, 0.731f, 0.6063015f, 0.5921569f);
    public Color monitorInactive = new Color(0.376465f, 0.6063015f, 0.731f, 0.8921569f);
    private const string SCREEN_SELECTABLE = "LaserTarget";
    private const string SCREEN_NOTSELECTABLE = "Untagged";

    // Collection of instantiated screens
    private IList<GameObject> screens;

    void Start() {
        swipeDirection = Direction.STILL;
        screenColor = prefabScreenContainer.GetComponent<Renderer>().sharedMaterial.GetColor(TINT_COLOR);

        // Spawn the prefabs
        screens = new List<GameObject>();
        for (int i = 1; i < screenCount + 1; i++) {
            GameObject newScreenObj = GameObject.Instantiate(prefabScreenContainer, transform);
            newScreenObj.name = "ArticleMonitor" + i.ToString("00");
            screens.Add(newScreenObj);
        }
    }

    void FixedUpdate() {
        // Make the screen rotation smooth
        offset = Mathf.Lerp(offset, 0f, Time.deltaTime);
        if (Mathf.Abs(offset) > maximumScrollSpeed) {
            offset = Mathf.Sign(offset) * maximumScrollSpeed;
        }

        // Immediately stop the movement when the rotation speed falls below a certain level
        if (Mathf.Abs(offset) < EPSILON) {
            offset = 0f;
        }

        // Calculate the maximum scroll offset based on the current number of articles
        maximumOffset = 180f * ((float)numberOfArticles / (float)screenCount);

        // Boundary scrolling
        actualOffset += offset;
        if (actualOffset < 0f) {
            actualOffset = 0f;
        } else if (actualOffset > maximumOffset) {
            actualOffset = maximumOffset;
        }

        // Define the current article load position
        // TODO das hier richtig berechnen
        lowerArticleLoadIndex = (int)(actualOffset / (180f / (screenCount / screensPerColumn))) * screensPerColumn;
        upperArticleLoadIndex = ((int)(actualOffset / spacingX) * screensPerColumn) + (screenCount / screensPerColumn);

        /////////////////////////////////////////////////////////////////////////////////////////

        // Iterate through all screens and update their position
        // The order goes clockwise from the column defined as "first" and every entry will be spawned below it
        int column = firstColumn;
        float y = initialYPos;
        int screensInColumnCount = 0;

        int screenID = 0;
        foreach (GameObject screen in screens) {
            screenID++;

            // Update the screen's positon using simple trigenometry scaled over the frame counter.
            // As a result, the screen will circle around the player (specifically, the position of ShopExplorer).
            Vector3 pos = new Vector3();
            float radian = (((spacingX * column) + actualOffset) % 360f) / 180f * Mathf.PI;
            pos.x = Mathf.Cos(radian) * distanceFromCenter;
            pos.z = Mathf.Sin(radian) * distanceFromCenter;

            pos.y = y;
            y -= spacingY;
            screensInColumnCount++;
            if (screensInColumnCount % screensPerColumn == 0) {
                y = initialYPos;
                column--;
            }

            // Apply updates to unslected screens
            // Skip it for a selected screen
            if (screen != selectedScreen) {
                screen.transform.position = pos;
                OrientateScreen(screen);

                // If the screen is behind the user, steadily increase the transparency
                float sin = Mathf.Sin(radian);
                if (sin < -EPSILON) {
                    screen.tag = SCREEN_NOTSELECTABLE;
                    SetMonitorInactive(screen);

                    Color transparentScreenColor = screenColor;
                    float newAlpha = Mathf.Max(0.5f - (Mathf.Abs(sin)), 0f);
                    if (newAlpha > 0f) {
                        transparentScreenColor.a = newAlpha;
                        screen.transform.GetComponent<Renderer>().material.SetColor(TINT_COLOR, transparentScreenColor);
                        screen.SetActive(true);
                    } else {
                        screen.SetActive(false);
                    }
                } else {
                    screen.tag = SCREEN_SELECTABLE;
                }
            }
        }

        // Cosmetic change to hide article monitors at the boundaries of the wall
        if (actualOffset < BOUNDARY_DEGREE) {
            for (int i = screenCount - (screensPerColumn * 2); i < screenCount; i++) {
                screens[i].SetActive(false);
            }
        }
        
        // TODO right boundary limits, damit das scrollen kein blödsinn wird
        //else if ((maximumOffset - actualOffset) < BOUNDARY_DEGREE) {
        //    for (int i = 0; i < screenCount - (screensPerColumn * 2); i++) {
        //        screens[i].SetActive(false);
        //    }
        //}

        // Move the selected monitor towards its destination
        if (selectedScreen != null) {
            selectedScreen.transform.position = Vector3.Slerp(
                selectedScreen.transform.position,
                infoScreen.transform.position,
                Time.deltaTime * selectionSpeed
            );

            selectedScreen.transform.localScale = Vector3.Slerp(
                selectedScreen.transform.localScale,
                infoScreen.transform.localScale,
                Time.deltaTime * selectionSpeed
            );

            selectedScreen.transform.rotation = Quaternion.Slerp(
                selectedScreen.transform.rotation,
                infoScreen.transform.rotation,
                Time.deltaTime * selectionSpeed
            );
        }
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

    private void SetMonitorStatus(GameObject monitor, bool status) {
        if (monitor != null) {
            monitor.transform.GetComponent<Renderer>().material.SetColor(TINT_COLOR, status ? monitorActive : monitorInactive);
        }
    }

    public void SetMonitorActive(GameObject monitor) {
        SetMonitorStatus(monitor, true);
    }

    public void SetMonitorInactive(GameObject monitor) {
        SetMonitorStatus(monitor, false);
    }

    public void SelectScreen(GameObject screen) {
        UnselectScreen();
        selectedScreen = screen;
        SetSpawnButtonActive(selectedScreen, true);
    }

    public void UnselectScreen() {
        if (selectedScreen != null) {
            SetSpawnButtonActive(selectedScreen, false);
            selectedScreen.transform.localScale = prefabScreenContainer.transform.localScale;
        }
        selectedScreen = null;
    }
}