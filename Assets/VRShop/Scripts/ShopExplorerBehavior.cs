using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ShopExplorerBehavior : MonoBehaviour {

    private enum ArticleLoadBehavior {
        Visible,
        Hidden,
        TurnVisibleForward,
        TurnVisibleBackward
    }

    public int numberOfArticles = 0;
    public int articleLoadOffset = 0;

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
    private ShopItemSpawner spawner;
    public GameObject infoScreen;
    public GameObject forwardLoadTrigger;
    public GameObject backwardLoadTrigger;

    public float selectionSpeed;
    private GameObject selectedScreen;
    private GameObject expandedScreen;
    private VRShopArticle selectedArticle;

    // Used for swiping
    public enum Direction {
        LEFT  = -1,
        STILL = 0,
        RIGHT = +1
    };
    public Direction swipeDirection = Direction.STILL;
    public float offsetChangeThisFrame = 0f;
    public float actualOffset = 0f;
    public float maximumOffset = 0f;
    public float maximumScrollSpeed = 2f;
    private const float EPSILON = 0.01f;
    private const float BOUNDARY_DEGREE = 45f;

    public Color colorActive;
    public Color colorInactive;

    // Used for transparency
    private const string SCREEN_SELECTABLE = "LaserTarget";
    private const string SCREEN_NOTSELECTABLE = "Untagged";
    private bool isArticleMonitor = false;

    // Collection of instantiated screens
    private IList<GameObject> screens;

    // Collection of active articles
    private IList<VRShopArticle> articles;

    // Delegation
    private CartItemsHandler cart;

    void Start() {
        swipeDirection = Direction.STILL;

        articles = new List<VRShopArticle>();

        // Spawn the prefabs
        screens = new List<GameObject>();
        for (int i = 1; i < screenCount + 1; i++) {
            GameObject newScreenObj = GameObject.Instantiate(prefabScreenContainer, transform);
            newScreenObj.name = "ArticleMonitor" + i.ToString("00");
            newScreenObj.GetComponent<ArticleMonitorWrapper>().wallPositionId = i - 1;
            newScreenObj.GetComponent<ArticleMonitorWrapper>().articleLoadIndexId = i - 1;
            screens.Add(newScreenObj);
        }
        forwardLoadTrigger = screens[0];
        backwardLoadTrigger = null;

        cart = transform.Find("Cart").GetComponent<CartItemsHandler>();
        spawner = shopItemSpawner.GetComponent<ShopItemSpawner>();
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
        if (Mathf.Abs(offsetChangeThisFrame) < EPSILON
            || (actualOffset < EPSILON && swipeDirection == Direction.LEFT)
            || (actualOffset > maximumOffset - EPSILON && swipeDirection == Direction.RIGHT)) {
            offsetChangeThisFrame = 0f;
        }

        // Keep swipe direction updated
        swipeDirection = CurrentSwipeDirection();

        // Calculate the maximum scroll offset based on the current number of articles
        maximumOffset = Math.Max(0f, 360f * ((float)numberOfArticles / (float)screenCount) - spacingX);

        // Boundary scrolling
        actualOffset += offsetChangeThisFrame;
        if (actualOffset < 0f) {
            actualOffset = 0f;
        } else if (actualOffset > maximumOffset) {
            actualOffset = maximumOffset;
        }

        /////////////////////////////////////////////////////////////////////////////////////////

        // Iterate through all screens and update their position
        // The order goes clockwise from the column defined as "first" and every entry will be spawned below it
        int column = firstColumn;
        float y = initialYPos;
        int screensInColumnCount = 0;

        for (int i = 0; i < screens.Count; i++) {
            // Fetch the mandatory information about the current monitor
            GameObject screen = screens[i];
            ArticleMonitorWrapper wrapper = screen.GetComponent<ArticleMonitorWrapper>();

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
            float previousX = screen.transform.localPosition.x;
            screen.transform.localPosition = pos;
            OrientateScreen(screen);

            // If the article ordinal in the wall of the screen is greater than the number of available articles, hide it
            // If the screen is behind the user, steadily increase the transparency
            float sin = Mathf.Sin(radian);
            if (sin > -EPSILON) {
                screen.SetActive(true);
                screen.tag = SCREEN_SELECTABLE;
            } else {
                screen.SetActive(false);
                screen.tag = SCREEN_NOTSELECTABLE;
            }

            // Invisible loading zone behind the user (which is approaching sin 1.00)
            if (sin - EPSILON < -1f && (pos.x * previousX) < 0f) {
                // Get the base index
                switch (swipeDirection) {
                    case Direction.RIGHT:
                        // Monitor just turned invisible by rotating right, load next article wall
                        if (screen == forwardLoadTrigger) {
                            wrapper.articleLoadIndexId += screenCount;
                            screens[i + 1].GetComponent<ArticleMonitorWrapper>().articleLoadIndexId += screenCount;

                            forwardLoadTrigger = screens[(wrapper.wallPositionId + screensPerColumn) % screenCount];
                            backwardLoadTrigger = screen;
                        }
                        break;
                    case Direction.LEFT:
                        // Monitor just turned invisible by rotating left, load previous article wall
                        if (screen == backwardLoadTrigger) {
                            wrapper.articleLoadIndexId -= screenCount;
                            screens[i + 1].GetComponent<ArticleMonitorWrapper>().articleLoadIndexId -= screenCount;

                            forwardLoadTrigger = screen;
                            if (wrapper.wallPositionId - screensPerColumn >= 0) {
                                backwardLoadTrigger = screens[(wrapper.wallPositionId - screensPerColumn) % screenCount];
                            } else {
                                backwardLoadTrigger = screens[screenCount - screensPerColumn];
                            }
                        }
                        break;
                }
            }

            // Hide this monitor if it exceeds the maximum number of articles and all following ones
            if (wrapper.articleLoadIndexId + 1 > articles.Count || wrapper.articleLoadIndexId < 0) {
                screen.SetActive(false);
                continue;
            }

            // Update the content, if needs to be
            wrapper.SetArticle(articles[wrapper.articleLoadIndexId]);

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
            spawner.DestroyHoveringObject();
            Destroy(expandedScreen);
            if (isArticleMonitor) {
                selectedScreen.SetActive(true);
                selectedArticle = null;
            }
        }
    }
    
    private Direction CurrentSwipeDirection() {
        if (offsetChangeThisFrame < 0f) {
            return Direction.LEFT;
        } else if (offsetChangeThisFrame > 0f) {
            return Direction.RIGHT;
        }
        return Direction.STILL;
    }

    public void UpdateOffset(float newOffset) {
        Direction currentSwipeDirection = CurrentSwipeDirection();
        
        // If direction didn't change, increase swipe speed
        if (currentSwipeDirection == swipeDirection) {
            if (Mathf.Abs(offsetChangeThisFrame + newOffset) < EPSILON) {
                offsetChangeThisFrame = 0f;
                return;
            }
            offsetChangeThisFrame += newOffset;
            return;
        }

        // If direction changed, immediately stop and turn around
        offsetChangeThisFrame = newOffset;
        swipeDirection = currentSwipeDirection;
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

    public void SpawnShopItem(GameObject targetObject) {
        if (selectedArticle != null) {
            spawner.ImportAndSpawnShopItem(selectedArticle);
        }
    }
    
    public void ReceiveSearchResutls(IList<VRShopArticle> searchResultArticles) {
        articles = searchResultArticles;

        // Reset position
        actualOffset = 0f;
        offsetChangeThisFrame = 0f;
        articleLoadOffset = 0;
        forwardLoadTrigger = screens[0];
        backwardLoadTrigger = null;

        // Update the number of articles
        numberOfArticles = articles.Count;

        // Update number of shown articles (cap it at the max number of possible screens)
        for (int i = 0; i < Math.Min(numberOfArticles, screenCount); i++) {
            //screens[i].SetActive(true);
            screens[i].GetComponent<ArticleMonitorWrapper>().articleLoadIndexId = i;
        }
    }

    public void AddToCart(int cartQuantity) {
        cart.AddToCart(selectedArticle, cartQuantity);
        UnselectScreen();
    }
}