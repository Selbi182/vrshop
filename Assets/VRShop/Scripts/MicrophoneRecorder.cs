using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MicrophoneRecorder : MonoBehaviour {

    public bool isRecording = false;

    private const int RECORDING_MAX_LENGTH = 60;
    private const int RECORDING_FREQUENCY = 44100;
    
    private AudioSource audioSource;

    // Steam VR Stuff
    private SteamVR_Controller.Device Controller { get { return SteamVR_Controller.Input((int)trackedObj.index); } }
    private SteamVR_TrackedObject trackedObj;

    void Start () {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        isRecording = false;
        audioSource = GetComponent<AudioSource>();
    }
	
	void Update () {
        if (Controller == null || Microphone.devices.Length < 1) {
            Debug.Log(Microphone.devices.Length);
            return;
        }

        if (Controller.GetPressDown(SteamVR_Controller.ButtonMask.Grip)) {
            // Toggle recording mode
            isRecording = !isRecording;

            // Handle whether we just started or ended a recording
            if (isRecording) {
                // Default microphone, no loop, max 60 seconds recording, 44100 frequency
                audioSource.clip = Microphone.Start(null, false, RECORDING_MAX_LENGTH, RECORDING_FREQUENCY);
            } else {
                // Stop the recording
                Microphone.End(null);

                // Play the recorded audio
                audioSource.Play();
            }
        }

		if (isRecording) {
            SendMessage("HapticPulseDo", 0.1f);
        }
	}
}
