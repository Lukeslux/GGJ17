﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


using System.Diagnostics;
using System.IO;
using Application = UnityEngine.Application;

public class SelectAudioFile : MonoBehaviour {
    public AudioSource audioSource;
    public AudioImageImporter aii;
    public Canvas loadScreen;
    private AudioClip resultClip;



    void Start() {
        SimpleFileBrowser.SetFilters(".mp3", ".wav", ".ogg", ".aiff", ".flac");

        SimpleFileBrowser.SetDefaultFilter(".mp3");
        SimpleFileBrowser.AddQuickLink(null, "Users", "C:\\Users");
        StartCoroutine(ShowLoadDialogCoroutine());
    }

    IEnumerator ShowLoadDialogCoroutine() {
        yield return StartCoroutine(SimpleFileBrowser.WaitForLoadDialog(false, null, "Load File", "Load"));
        Instantiate(loadScreen);
        LoadAudioFile(SimpleFileBrowser.Result);
    }


    public void LoadAudioFile( string audioPath ) {
        StartCoroutine(LoadAudio(audioPath));
    }

    private string SoxFilename()
    {
        #if UNITY_EDITOR_LINUX
        return Path.Combine("sox", "sox_linux");
        #elif UNITY_EDITOR_WINDOWS
        return Path.Combine("sox", "sox.exe");
        #elif UNITY_STANDALONE_LINUX
        return Path.Combine("EyeCantHear_Data", Path.Combine("sox","sox_linux"));
        #else
        return Path.Combine("sox", "sox.exe");
        #endif

    }

    private string convertSoundToWav(string audioPath)
    {
        string tmpWavPath =Path.Combine(Application.dataPath, "tmp.wav");

        Process process = new Process();
        process.StartInfo.FileName = SoxFilename();
        process.StartInfo.WorkingDirectory = Application.dataPath;
        process.StartInfo.Arguments = "\"" + audioPath + "\" " + tmpWavPath;
        process.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.Start();

        //UnityEngine.Debug.Log("Starting Done");

        var standardOutput = new System.Text.StringBuilder();
        var standardError = new System.Text.StringBuilder();

        // read chunk-wise while process is running.
        while (!process.HasExited)
        {
            standardOutput.Append(process.StandardOutput.ReadToEnd());
            standardError.Append(process.StandardError.ReadToEnd());
        }

        // make sure not to miss out on any remaindings.
        standardOutput.Append(process.StandardOutput.ReadToEnd());
        standardError.Append(process.StandardError.ReadToEnd());


        //process.WaitForExit();
        //UnityEngine.Debug.Log(standardOutput);
        if (standardError.ToString() != "")
        {
            UnityEngine.Debug.Log(standardError);
        }
        return tmpWavPath;
    }



    IEnumerator LoadAudio( string audioPath ) {
        if(!audioPath.EndsWith(".wav"))
        {
            audioPath = convertSoundToWav(audioPath);
        }
        string url = "file:///" + audioPath;
        WWW www = new WWW(url);
        while (!www.isDone) {
            yield return new WaitForEndOfFrame();
        }


        // resultClip = (audioPath.EndsWith(".mp3")) ? NAudioPlayer.FromMp3Data(www.bytes) : www.audioClip;

        audioSource.clip = resultClip = www.audioClip; //resultClip;
        StartAudioImageImporter(audioPath);
        yield return null;
    }

    private void StartAudioImageImporter( string audioPath ) {
        UnityEngine.Debug.Log("size = " + resultClip.length);
        aii.ProcessAudioFile(audioPath, resultClip.length);
    }


    //deze wegdoen als er muziek niet meer automatisch gestart moet worden
    void Update() {
        if (audioSource.clip != null) {
            if (!audioSource.isPlaying && audioSource.clip.isReadyToPlay)
                audioSource.Play();
        }
    }
}
