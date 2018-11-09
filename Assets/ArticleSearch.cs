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
                        IList<VRShopArticle> articles = VRShopDBConnector.SearchForArticle(currentSearchString);
                        foreach (VRShopArticle article in articles) {
                            Debug.Log(article);
                        }
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
        // TODO
        return false;
    }

    private void UpdateMeshText(string s) {
        textMesh.SetText(s);
    }

    private void ResetSearch() {
        currentSearchString = "";
        UpdateMeshText(DEFAULT_TEXT);
    }
}
