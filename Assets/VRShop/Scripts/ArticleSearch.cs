using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Data;
using Mono.Data.Sqlite;

public class ArticleSearch : MonoBehaviour {

    private TMPro.TextMeshPro textMesh;
    private string currentSearchString;
    private const string DEFAULT_TEXT = "Speak to search for articles...";

	void Start () {
        textMesh = transform.GetComponent<TMPro.TextMeshPro>();
        ResetSearch();
    }

	void Update () {
        if (KeyboardSearch() || VoiceSearch()) {
            UpdateMeshText(currentSearchString);
        }
    }

    private bool KeyboardSearch() {
        if (Input.inputString.Length > 0) {
            foreach (char c in Input.inputString) {
                switch (c) {
                    // Backspace
                    case '\b':
                        if (currentSearchString.Length > 0) {
                            currentSearchString = currentSearchString.Substring(0, currentSearchString.Length - 1);
                        }
                        break;

                    // Enter/Return
                    case '\n':
                    case '\r':
                        IList<VRShopArticle> searchResultArticles = VRShopDBConnector.SearchForArticle(currentSearchString);
                        if (searchResultArticles != null && searchResultArticles.Count > 0) {
                            OfferResults(searchResultArticles);
                        }
                        ResetSearch();
                        break;

                    // Regular char input
                    default:
                        currentSearchString += c;
                        break;
                }
            }
            return true;
        }
        return false;
    }

    private bool VoiceSearch() {
        // TODO Voice Search
        return false;
    }

    private void UpdateMeshText(string s) {
        textMesh.SetText(s);
    }

    private void ResetSearch() {
        currentSearchString = "";
        UpdateMeshText(DEFAULT_TEXT);
    }

    public void OfferResults(IList<VRShopArticle> articles) {
        // Notify the ShopExplorer that new articles have been found
        SendMessageUpwards("ReceiveSearchResutls", articles);
    }
}
