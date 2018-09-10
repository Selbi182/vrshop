using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopItemSpawner : MonoBehaviour {

    public GameObject testSpawnObject;

    public void SpawnShopItem(GameObject selectedScreenMonitor) {
        GameObject.Instantiate(testSpawnObject, new Vector3(0f, 5f, 0f), Quaternion.identity, transform);
    }
}
