using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopItemSpawner : MonoBehaviour {

    public GameObject testSpawnObject;
    public bool isEnbaled = true;

    public void SpawnShopItem(VRShopArticle selectedArticle) {
        if (isEnbaled) {
            GameObject.Instantiate(testSpawnObject, new Vector3(0f, 5f, 0f), Quaternion.identity, transform);
        }
    }
}
