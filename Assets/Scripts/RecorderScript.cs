using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;
using System.IO;
using System;

public class RecorderScript : MonoBehaviour
{
    public Button recordButton;
    public TextMeshProUGUI statusText;

    private bool isRecording = false;
    private string recordedVideoPath;

    void Start()
    {
        recordButton.gameObject.SetActive(true);
        recordButton.onClick.AddListener(ToggleRecording);
        statusText.text = "Start Recording";
        
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
    }

    void ToggleRecording()
    {
        if (!isRecording)
        {
            StartRecording();
        }
        else
        {
            StopRecording();
        }
    }

    void StartRecording()
    {
        string customPath = Path.Combine(Application.persistentDataPath, "MyVideos");
        if (!Directory.Exists(customPath))
        {
            Directory.CreateDirectory(customPath);
        }
        string savePath = Path.Combine(customPath, "video_" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".mp4");

        var permission = NativeCamera.CheckPermission(true);

        if (permission == NativeCamera.Permission.Granted)
        {
            // For recording device camera (not AR content)
            NativeCamera.RecordVideo((path) =>
            {
                statusText.text = $"Video saved!"+ path;
                isRecording = false;
                if (path != savePath)
                {
                    if (File.Exists(savePath))
                    {
                        File.Delete(savePath);
                    }
                    
                    File.Move(path, savePath);
                    path = savePath;

                    SaveToGallery(savePath);
                }
                recordedVideoPath = path;
            },
            NativeCamera.Quality.Low,
            maxDuration: 0);

            isRecording = true;
            statusText.text = "Recording...";
        }
        else if (permission == NativeCamera.Permission.ShouldAsk)
        {
            NativeCamera.RequestPermission(true);
            statusText.text = "Please grant camera permissions";
        }
        else
        {
            statusText.text = "Camera permission denied";
        }
    }

    private void SaveToGallery(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        // Use Native Gallery to save to gallery
        string albumName = "MyARVideos";
        string galleryFileName = "AR_Recording_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".mp4";

        NativeGallery.Permission permission = NativeGallery.SaveVideoToGallery(
            path,
            albumName,
            galleryFileName,
            (success, newPath) =>
            {
                if (success)
                    statusText.text = "Video saved!";
            }
        );

        // Check the permission result
        switch (permission)
        {
            case NativeGallery.Permission.Granted:
                break;
            case NativeGallery.Permission.Denied:
                statusText.text = "Permission denied to save video to gallery";
                break;
            case NativeGallery.Permission.ShouldAsk:
                break;
        }
    }

    void StopRecording()
    {
        
        isRecording = false;
        statusText.text = "Start Recording";
    }

    // Optional: Preview the recorded video
    public void PlayRecordedVideo()
    {
        if (!string.IsNullOrEmpty(recordedVideoPath))
        {
            Handheld.PlayFullScreenMovie("file://" + recordedVideoPath);
        }
    }
}
