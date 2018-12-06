using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchStart : MonoBehaviour {

    public GameObject searchTextBox;
    private ShopExplorerBehavior shopExplorer;
    public bool start;

    void Start() {
        shopExplorer = transform.parent.GetComponent<ShopExplorerBehavior>();    
    }

    void Update() {
        if (start) {
            start = false;
            StartSearch();
        }    
    }

    public void StartSearch() {
        // Handle the search button
        searchTextBox.transform.localPosition = transform.localPosition;
        searchTextBox.SetActive(true);
        searchTextBox.GetComponent<ArticleSearch>().EnableListener();
        shopExplorer.SelectScreen(searchTextBox);
    }
}
