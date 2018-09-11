﻿using System.Collections;
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
    private float actualOffset = 0f;


    // Collection of instantiated screens
    private IList<GameObject> screens;

    void Start() {
        swipeDirection = Direction.STILL;
        screens = new List<GameObject>();
        // Spawn the prefabs
        for (int i = 1; i < screenCount + 1; i++) {
            GameObject newScreenObj = GameObject.Instantiate(prefabScreenContainer, transform);
            screens.Add(newScreenObj);
        }
    }

    void FixedUpdate() {
        // Make it smooth af
        offset = Mathf.Lerp(offset, 0f, Time.deltaTime);
        actualOffset += offset;

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

            // Skip it for a selected screen
            if (screen != selectedScreen) {
                screen.transform.position = pos;
                OrientateScreen(screen);
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