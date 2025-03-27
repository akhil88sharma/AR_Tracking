/*/using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using Upft.MRRecorder.Runtime;
using System;
using System.IO;
using System.Collections;

namespace Upft.MRRecorder.Sample
{
    public class ARVideoRecorder : MonoBehaviour
    {
        [SerializeField] private ARCameraManager _cameraManager;
        [SerializeField] private Camera _sceneCamera;

        [Header("UI")]
        [SerializeField] private Button _startRecordingButton;
        [SerializeField] private TMP_Dropdown _qualityDropdown;

        [Header("Encoding Options")]
        [SerializeField] private bool _useDefaultBitrate = false;
        [SerializeField] private int _customBitrate = 5_000_000;
        [SerializeField] private bool _lowLatencyMode = false;
        [SerializeField] private int _keyFrameInterval = 30;
        [SerializeField] private int _priority = 0;

        public TextMeshProUGUI statuslog;
        private VideoRecorder _recorder;
        private string _outputPath;

        private void Start()
        {
            _startRecordingButton.onClick.AddListener(ToggleRecording);
            CheckPermissions();
        }

        private void OnEnable()
        {
            if (_cameraManager == null)
                _cameraManager = FindObjectOfType<ARCameraManager>();

            if (_sceneCamera == null)
                _sceneCamera = Camera.main;

            if (_cameraManager != null && _sceneCamera != null)
                _recorder = new VideoRecorder(_cameraManager, _sceneCamera);
        }

        private void ToggleRecording()
        {
            if (_recorder == null)
            {
                statuslog.text = "Error: Recorder not ready";
                return;
            }

            if (_recorder.IsRecording)
            {
                StopRecording();
            }
            else
            {
                StartRecording();
                *//*StartCoroutine(StartRecordingWithChecks());*//*
            }
        }
        *//*private IEnumerator StartRecordingWithChecks()
        {
            // Wait for AR session to stabilize
            yield return new WaitUntil(() => _cameraManager.enabled && _cameraManager.subsystem?.running == true);

            // Set output path (Android 10+ compatible)
            _outputPath = Path.Combine(Application.persistentDataPath, $"recording_{System.DateTime.Now:yyyyMMdd_HHmmss}.mp4");
            Directory.CreateDirectory(Path.GetDirectoryName(_outputPath));

            var resolution = new Runtime.Resolution(1280, 720);
            string outputPath = GetPersistentStoragePath();
            string fileName = $"recording_{DateTime.Now:yyyyMMdd_HHmmss}.mp4";

            _recorder.StartRecording(new(resolution, outputPath, fileName, _customBitrate, _lowLatencyMode, _keyFrameInterval, _priority));
            Debug.Log($"Recording started: {_outputPath}");

            // Stop after 10 seconds for testing
            yield return new WaitForSeconds(10);
            _recorder.StopRecording();
            Debug.Log($"Recording saved: {_outputPath}");
        }*//*
        private void StartRecording()
        {
            try
            {
                var resolution = new Runtime.Resolution(1920, 1080);
                string outputPath = GetPersistentStoragePath();
                string fileName = $"recording_{DateTime.Now:yyyyMMdd_HHmmss}.mp4";

                _recorder.StartRecording(new(resolution, outputPath, fileName, _customBitrate, _lowLatencyMode, _keyFrameInterval, _priority));
                statuslog.text = "Recording..";
            }
            catch (Exception ex)
            {
                statuslog.text = "Error: " + ex.Message;
            }
        }

        private void StopRecording()
        {
            _recorder.StopRecording();
            statuslog.text = "Recording Saved!";
        }

        private string GetPersistentStoragePath()
        {
            string folderName = "AR_Recordings";
            string path = Path.Combine(Application.persistentDataPath, folderName);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }

        private void CheckPermissions()
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
                Permission.RequestUserPermission(Permission.Camera);

            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
                Permission.RequestUserPermission(Permission.Microphone);


            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
                Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        }

        private void Update()
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (_recorder != null)
            {
                _startRecordingButton.image.color = _recorder.IsRecording ? Color.red : Color.green;
            }
        }

        private void OnDestroy()
        {
            if (_recorder != null && _recorder.IsRecording)
                _recorder.StopRecording();

            _startRecordingButton.onClick.RemoveAllListeners();
        }
    }
}
*/