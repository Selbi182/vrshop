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

    public float selectionSpeed;
    private GameObject selectedScreen;
    private GameObject expandedScreen;
    private VRShopArticle selectedArticle;

    // Used for swiping
    private enum Direction {
        LEFT  = -1,
        STILL = 0,
        RIGHT = +1
    };
    private Direction swipeDirection;
    public float offsetChangeThisFrame = 0f;
    public float actualOffset = 0f;
    public float maximumOffset = 0f;
    public float maximumScrollSpeed = 2f;
    private const float EPSILON = 0.01f;
    private const float BOUNDARY_DEGREE = 45f;

    // Used for transparency
    private Color screenColor;
    public Color monitorActive;
    public Color monitorInactive;
    private const string SCREEN_SELECTABLE = "LaserTarget";
    private const string SCREEN_NOTSELECTABLE = "Untagged";
    private bool isArticleMonitor = false;

    // Collection of instantiated screens
    private IList<GameObject> screens;

    // Collection of active articles
    private IList<VRShopArticle> articles;

    void Start() {
        swipeDirection = Direction.STILL;
        screenColor = prefabScreenContainer.GetComponent<ArticleMonitorWrapper>().GetMonitorColor();

        articles = new List<VRShopArticle>();

        // Spawn the prefabs
        screens = new List<GameObject>();
        for (int i = 1; i < screenCount + 1; i++) {
            GameObject newScreenObj = GameObject.Instantiate(prefabScreenContainer, transform);
            newScreenObj.name = "ArticleMonitor" + i.ToString("00");
            newScreenObj.GetComponent<ArticleMonitorWrapper>().wallPositionId = i;
            screens.Add(newScreenObj);
        }
    }

    /// <summary>
    /// /////////////////////////////////////////////////
    /// </summary>
    void FixedUpdate() {
        // Make the screen rotation smooth
        offsetChangeThisFrame = Mathf.Lerp(offsetChangeThisFrame, 0f, Time.deltaTime);
        if (Mathf.Abs(offsetChangeThisFrame) > maximumScrollSpeed) {
            offsetChangeThisFrame = Mathf.Sign(offsetChangeThisFrame) * maximumScrollSpeed;
        }

        // Immediately stop the movement when the rotation speed falls below a certain level
        if (Mathf.Abs(offsetChangeThisFrame) < EPSILON) {
            offsetChangeThisFrame = 0f;
        }

        // Calculate the maximum scroll offset based on the current number of articles
        maximumOffset = 180f * ((float)numberOfArticles / (float)screenCount);

        // Boundary scrolling
        actualOffset += offsetChangeThisFrame;
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

        for (int i = 0; i < screens.Count; i++) {
            GameObject screen = screens[i];
            ArticleMonitorWrapper wrapper = screen.GetComponent<ArticleMonitorWrapper>();

            if (i >= articles.Count) {
                screen.SetActive(false);
                continue;
            }

            // Update the screen content if there is any
            wrapper.SetArticle(articles[i]);

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
            screen.transform.position = pos;
            OrientateScreen(screen);

            // If the article ordinal in the wall of the screen is greater than the number of available articles, hide it
            // If the screen is behind the user, steadily increase the transparency
            float sin = Mathf.Sin(radian);
            if (sin < -EPSILON) {
                screen.tag = SCREEN_NOTSELECTABLE;
                SetMonitorInactive(screen);

                float newAlpha = Mathf.Max(0.5f - (Mathf.Abs(sin)), 0f);
                if (newAlpha > 0f) {
                    screen.SetActive(true);
                    SetMonitorTransparency(screen, newAlpha);
                } else {
                    screen.SetActive(false);
                }
            } else {
                screen.tag = SCREEN_SELECTABLE;
            }
        }

        // Cosmetic change to hide article monitors at the boundaries of the wall
        if (actualOffset < BOUNDARY_DEGREE) {
            for (int i = screenCount - (screensPerColumn * 2); i < screenCount; i++) {
                screens[i].SetActive(false);
            }
        }

        // Move the selected monitor towards its destination
        if (expandedScreen != null) {
            expandedScreen.transform.position = Vector3.Slerp(
                expandedScreen.transform.position,
                infoScreen.transform.position,
                Time.deltaTime * selectionSpeed
            );

            if (isArticleMonitor) {
                expandedScreen.transform.localScale = Vector3.Slerp(
                    expandedScreen.transform.localScale,
                    infoScreen.transform.localScale,
                    Time.deltaTime * selectionSpeed
                );
            }

            expandedScreen.transform.rotation = Quaternion.Slerp(
                expandedScreen.transform.rotation,
                infoScreen.transform.rotation,
                Time.deltaTime * selectionSpeed
            );
        }
    }
    /////////////////////////////////////////////////

    public void SelectScreen(GameObject screen) {
        // Mark the previously selected screen as unselected to make it move back or hide it
        UnselectScreen();

        // Create a clone of the screen with visible backside (pretend that it's the same gameobject as the selected one)
        isArticleMonitor = screen.transform.parent.transform == transform;
        expandedScreen = GameObject.Instantiate(screen, transform);
        if (isArticleMonitor) {
            selectedArticle = screen.GetComponent<ArticleMonitorWrapper>().GetArticle();
            expandedScreen.GetComponent<ArticleMonitorWrapper>().SetArticle(selectedArticle);
        } else { 
            selectedArticle = null;
        }
        expandedScreen.tag = SCREEN_NOTSELECTABLE;
        SetBacksideActive(expandedScreen, true);

        // Rememeber the selected screen
        selectedScreen = screen;
        screen.SetActive(false);
    }

    public void UnselectScreen() {
        if (expandedScreen != null) {
            Destroy(expandedScreen);
            if (isArticleMonitor) {
                selectedScreen.SetActive(true);
                selectedArticle = null;
            }
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
            offsetChangeThisFrame += newOffset;
            return;
        }
        offsetChangeThisFrame = newOffset;
        swipeDirection = newSwipeDirection;
    }

    private void OrientateScreen(GameObject screen) {
        // Update the rotation to keep looking at the player.
        screen.transform.rotation = Quaternion.LookRotation(transform.parent.position - screen.transform.position);
        Vector3 tmpAngles = screen.transform.localEulerAngles;
        tmpAngles.x = 0;
        screen.transform.localEulerAngles = tmpAngles;
    }

    private void SetBacksideActive(GameObject screen, bool active) {
        if (screen != null && isArticleMonitor) {
            screen.transform.GetComponent<ArticleMonitorWrapper>().SetBacksideActive(active);
        }
    }

    private void SetMonitorStatus(GameObject monitor, bool status) {
        if (monitor != null) {
            // TODO fix für nicht artikelmonitore
            //monitor.transform.GetComponent<ArticleMonitorWrapper>().SetMonitorColor(status ? monitorActive : monitorInactive);
        }
    }

    public void SetMonitorActive(GameObject monitor) {
        SetMonitorStatus(monitor, true);
    }

    public void SetMonitorInactive(GameObject monitor) {
        SetMonitorStatus(monitor, false);
    }




    public void SpawnShopItem(GameObject targetObject) {
        if (selectedArticle != null) {
            shopItemSpawner.SendMessage("SpawnShopItem", selectedArticle);
        }
    }

    private void SetMonitorTransparency(GameObject monitor, float alpha) {
        monitor.GetComponent<ArticleMonitorWrapper>().SetMonitorAlpha(alpha);
    }
    
    private void SetMonitorColor(GameObject monitor, Color color) {
        monitor.GetComponent<ArticleMonitorWrapper>().SetMonitorColor(color);
    }


    public void ReceiveSearchResutls(IList<VRShopArticle> searchResultArticles) {
        articles = searchResultArticles;

        // Hide search textbox
        UnselectScreen();

        // Reset position
        actualOffset = 0f;
        offsetChangeThisFrame = 0f;

        // Update the number of articles
        numberOfArticles = articles.Count;

        // Update number of shown articles
        for (int i = 0; i < numberOfArticles; i++) {
            screens[i].SetActive(true);
        }
    }
}