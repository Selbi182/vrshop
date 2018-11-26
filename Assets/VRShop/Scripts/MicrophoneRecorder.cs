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
    private AudioSource audioSource;
    private DictationRecognizer dictationRecognizer;

    void Start() {
        ResetResult();
        audioSource = GetComponent<AudioSource>();
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
        if ((Microphone.devices.Length < 1 && audioSource != null)
            || (dictationRecognizer != null && dictationRecognizer.Status.Equals(SpeechSystemStatus.Running))) {
            return;
        }
        
        // Launch the recognizer
        ResetResult();
        dictationRecognizer.Start();
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
