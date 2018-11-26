using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Data;
using Mono.Data.Sqlite;
using TMPro;

public class ArticleSearch : MonoBehaviour {

    public bool isWaitingForInput = true;
    public GameObject headsetMicrophone;
    private MicrophoneRecorder microphoneRecorder;

    private TextMeshPro textMesh;

    private string searchString;

    private string LiveSearchString {
        get {
            return _tmpSearchString;
        }
        set {
            _tmpSearchString = value;
            UpdateMeshText(_tmpSearchString);
        }
    }
    private string _tmpSearchString;

    private const string DEFAULT_TEXT = "Speak to search for articles...";

	void Start () {
        // Initialize the components
        textMesh = transform.GetComponent<TextMeshPro>();
        if (headsetMicrophone != null) {
            microphoneRecorder = headsetMicrophone.GetComponent<MicrophoneRecorder>();
        }

        // Initialize the search
        ResetSearch();
        isWaitingForInput = true;
    }

	void Update () {
        if (isWaitingForInput) {
            // Wait for any input and start the search on a successful entry
            KeyboardSearch();
            VoiceSearch();
        }

        if (searchString.Length > 0) {
            isWaitingForInput = false;

            IList<VRShopArticle> searchResultArticles = VRShopDBConnector.SearchForArticle(searchString);
            OfferResults(searchResultArticles);


            int resultsCount = searchResultArticles.Count;
            string resultText = string.Format("{0} Suchergebnisse", resultsCount);

            if (resultsCount == 1) {
                resultText = string.Format("{0} Suchergebniss", resultsCount);
            }

            string formatted = string.Format("({0}) {1}", resultText, searchString);

            ResetSearch();
            UpdateMeshText(formatted);
        }
    }

    private void KeyboardSearch() {
        if (Input.inputString.Length > 0) {
            foreach (char c in Input.inputString) {
                switch (c) {
                    // Backspace
                    case '\b':
                        if (LiveSearchString.Length > 0) {
                            LiveSearchString = LiveSearchString.Substring(0, LiveSearchString.Length - 1);
                        }
                        break;

                    // Enter/Return
                    case '\n':
                    case '\r':
                        searchString = _tmpSearchString;
                        break;

                    // Regular char input
                    default:
                        LiveSearchString += c;
                        break;
                }
            }
        }
    }

    private void VoiceSearch() {
        if (microphoneRecorder != null) {
            // Keep the process alive
            microphoneRecorder.StartSpeechToText();

            // Poll for any dictation results and assign the search string on success
            string dictationResult = microphoneRecorder.DictationResult();
            if (dictationResult != null) {
                searchString = dictationResult;
            } else {
                // If no text is available, poll for hypotheses results and update the text
                string hypothesisResult = microphoneRecorder.HypothesisResult();
                if (hypothesisResult != null) {
                    LiveSearchString = hypothesisResult;
                }
            }
        }
        return;
    }

    private void PerformSearch(string searchString) {
        IList<VRShopArticle> searchResultArticles = VRShopDBConnector.SearchForArticle(searchString);
        if (searchResultArticles != null && searchResultArticles.Count > 0) {
            OfferResults(searchResultArticles);
        }
        ResetSearch();
    }

    private void UpdateMeshText(string s) {
        textMesh.SetText(s);
    }

    private void ResetSearch() {
        searchString = "";
        LiveSearchString = "";
        UpdateMeshText(DEFAULT_TEXT);
        microphoneRecorder.FinishSearch();
    }

    public void OfferResults(IList<VRShopArticle> articles) {
        // Notify the ShopExplorer that new articles have been found
        SendMessageUpwards("ReceiveSearchResutls", articles);
    }
}
