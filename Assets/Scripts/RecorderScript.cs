using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;

public class RecorderScript : MonoBehaviour
{
    public Button recordButton;
    public TextMeshProUGUI statusText;

    private bool isRecording = false;
    private NativeCamera.Permission cameraPermission;
    private string recordedVideoPath;

    void Start()
    {
        recordButton.onClick.AddListener(ToggleRecording);
        statusText.text = "Start Recording";
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
        var permission = NativeCamera.CheckPermission(true);

        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        if (permission == NativeCamera.Permission.Granted)
        {
            // For recording device camera (not AR content)
            NativeCamera.RecordVideo((path) =>
            {
                recordedVideoPath = path;
                statusText.text = $"Video saved!"+ path;
                isRecording = false;
                recordButton.GetComponentInChildren<Text>().text = "Start Recording";
            },
            NativeCamera.Quality.High,
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

    void StopRecording()
    {
        
        isRecording = false;
        statusText.text = "Ready to record";
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
