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
    public float speedFactor;
    public GameObject prefabScreenContainer;

    // Collection of instantiated screens
    private IList<GameObject> screens;

    void Start() {
        screens = new List<GameObject>();
        // Spawn the prefabs
        for (int i = 1; i < screenCount + 1; i++) {
            GameObject newScreenObj = GameObject.Instantiate(prefabScreenContainer, transform);
            screens.Add(newScreenObj);
        }
    }

    void Update() {
        // Iterate through all screens and update their position
        int num = 0;
        float y = spacingY;
        foreach (GameObject screen in screens) {
            // Update the screen's positon using simple trigenometry scaled over the frame counter.
            // As a result, the screen will circle around the player (specifically, the position of ShopExplorer).
            Vector3 pos = new Vector3();
            float radian = (((Time.frameCount - (spacingX * num)) * speedFactor) % 360f) / 180f * Mathf.PI;
            pos.x = Mathf.Cos(radian) * distanceFromCenter;
            num++;
            pos.y = y;
            if (num % screensPerRow == 0) {
                y += spacingY;
                num = 0;
            }

            
            pos.z = Mathf.Sin(radian) * distanceFromCenter;
            screen.transform.position = pos;

            // Update the rotation to keep looking at the player.
            screen.transform.rotation = Quaternion.LookRotation(transform.parent.position - screen.transform.position);
            Vector3 tmpAngles = screen.transform.localEulerAngles;
            tmpAngles.x = 0;
            screen.transform.localEulerAngles = tmpAngles;
        }
    }
}