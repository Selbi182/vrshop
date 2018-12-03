using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

public class MicrophoneRecorder : MonoBehaviour {

    public bool isRunning;
    private string hypothesisResult;
    private string dictationResult;
    private AudioListener audioListener;
    private DictationRecognizer dictationRecognizer;

    void Start() {
        ResetResult();
        audioListener = GetComponent<AudioListener>();
        dictationRecognizer = InstantiateDictationRecognizer();
    }

    void Update() {
        isRunning = dictationRecognizer != null && dictationRecognizer.Status == SpeechSystemStatus.Running;
    }

    void OnDestroy() {
        if (dictationRecognizer != null) {
            dictationRecognizer.Dispose();    
        }
    }

    public void StartSpeechToText() {
        // Only allow dictation if a microphone is available and attached to the GameObject
        // Don't allow more than one instance
        bool isOkayToRecord = true;
        if (Microphone.devices.Length < 1) {
            Debug.LogWarning("No microphone found!");
            isOkayToRecord = false;
        }
        if (audioListener == null) {
            Debug.LogWarning("No audio listener found!");
            isOkayToRecord = false;
        }
        if (dictationRecognizer == null) {
            Debug.LogWarning("No dictation recognizer found!");
            isOkayToRecord = false;
        }
        if (dictationRecognizer != null && dictationRecognizer.Status.Equals(SpeechSystemStatus.Running)) {
            Debug.LogWarning("Dictation recognizer is already in use!");
            isOkayToRecord = false;
        }

        // Launch the recognizer
        if (isOkayToRecord) {
            ResetResult();
            dictationRecognizer.Start();
        }
    }

    private DictationRecognizer InstantiateDictationRecognizer() {
        DictationRecognizer dict = new DictationRecognizer();

        // Dictation result after a couple seconds of silence
        dict.DictationResult += (text, confidence) => {
            dictationResult = text;
        };

        // Dication result immediately during speech
        dict.DictationHypothesis += (text) => {
            hypothesisResult = text;
        };

        // Gets called every time a dictation is finished
        dict.DictationComplete += (completionCause) => {
            if (completionCause != DictationCompletionCause.Complete) {
                Debug.LogWarningFormat("Dictation completed unsuccessfully: {0}", completionCause);
            }
        };

        return dict;
    }

    public string DictationResult() {
        if (dictationResult.Length > 0) {
            string result = dictationResult;
            ResetResult();
            return result;
        }
        return null;
    }

    public string HypothesisResult() {
        if (hypothesisResult.Length > 0) {
            string result = hypothesisResult;
            ResetResult();
            return result;
        }
        return null;
    }

    public void FinishSearch() {
        if (dictationRecognizer != null && dictationRecognizer.Status.Equals(SpeechSystemStatus.Running)) {
            dictationRecognizer.Stop();
        }
    }

    private void ResetResult() {
        dictationResult = "";
        hypothesisResult = "";
    }
}
