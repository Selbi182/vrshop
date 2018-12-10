using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Data;
using Mono.Data.Sqlite;
using TMPro;

public class ArticleSearch : MonoBehaviour {

    private const string DEFAULT_TEXT = "Zur Suche ins Mikrofon sprechen...";
    private const string MICROPHONE_ERROR_TEXT = "Fehler bei der Spracherkennung! Bitte Tastatur verwenden...";

    public string debugTriggerWord = "alles";
    public bool isWaitingForInput;
    public GameObject headsetMicrophone;
    private MicrophoneRecorder microphoneRecorder;

    private TextMeshPro textMesh;

    public Color colorVoiceSearchActive;
    public Color colorVoiceSearchInactive;
    public Color colorSearchComplete;
    public Color colorSearchError;
    private Material textBoxMaterial;
    private Color _setColor;
    private readonly static string TINT_COLOR = "_TintColor";

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


    public void EnableListener() {
        isWaitingForInput = true;
        microphoneRecorder.StartSpeechToText();
        SetColor(colorVoiceSearchActive);
    }

    void Awake () {
        // Initialize the components
        textMesh = transform.GetComponent<TextMeshPro>();
        if (headsetMicrophone != null) {
            microphoneRecorder = headsetMicrophone.GetComponent<MicrophoneRecorder>();
        }
        textBoxMaterial = transform.Find("SearchBox").GetComponent<Renderer>().material;

        // Initialize the search
        ResetSearch();
    }

	void Update () {
        if (isWaitingForInput) {
            // Wait for any input and start the search on a successful entry
            KeyboardSearch();
            VoiceSearch();
        }

        // Gets called when the search string has been set, meaning that the search can be initiated
        if (searchString.Length > 0) {
            SetColor(colorSearchComplete);
            isWaitingForInput = false;
            PerformSearch(searchString);
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
        if (microphoneRecorder != null && microphoneRecorder.isRunning) {
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
            if (SetColor(colorVoiceSearchActive) && LiveSearchString.Length == 0) {
                UpdateMeshText(DEFAULT_TEXT);
            }
        } else {
            if (SetColor(colorVoiceSearchInactive) && LiveSearchString.Length == 0) {
                UpdateMeshText(MICROPHONE_ERROR_TEXT);
            }
        }
        return;
    }

    private void PerformSearch(string search) {
        bool isDebug = debugTriggerWord.Length > 0 && search.Contains(debugTriggerWord);
        if (isDebug) {
            // Debug: "a" is contained in every search, and is used to easily load all available data from the database
            search = "a";
        }

        List<VRShopArticle> searchResultArticles = VRShopDBConnector.SearchForArticle(search);

        OfferResults(searchResultArticles);

        int resultsCount = searchResultArticles.Count;
        string resultText = string.Format("{0} Suchergebnisse", resultsCount);
        if (resultsCount == 1) {
            resultText = string.Format("{0} Suchergebniss", resultsCount);
        }
        string formatted = string.Format("({0}) {1}", resultText, search);
        if (isDebug) {
            formatted = string.Format("({0}) <DEBUG MODE>", resultText);
        }

        ResetSearch();
        UpdateMeshText(formatted);
        if (resultsCount == 0) {
            SetColor(colorSearchError);
        }
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

    private bool SetColor(Color color) {
        if (!color.Equals(_setColor)) {
            textBoxMaterial.SetColor(TINT_COLOR, color);
            _setColor = color;
            return true;
        }
        return false;
    }
}
