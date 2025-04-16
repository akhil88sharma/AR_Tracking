using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class NewTestScript : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float alignmentThreshold = 0.15f; // meters
    [SerializeField] private float heightTolerance = 0.1f; // meters
    [SerializeField] private float checkInterval = 0.05f; // seconds
    [SerializeField] private float minStumpWidth = 0.15f;

    [Header("References")]
    [SerializeField] private Transform middleVirtualStump;
    [SerializeField] private XROrigin xrOrigin;
    [SerializeField] private ARCameraManager arCameraManager;
    [SerializeField] private ARRaycastManager raycastManager;

    private float timeSinceLastCheck;
    private bool isAligned;
    private Vector3? lastDetectedStumpPosition;
    public TextMeshProUGUI header;
    public GameObject recorder;

    private void Start()
    {
        header.text = "Align the Stumps!";
        if (xrOrigin != null)
        {
            arCameraManager = xrOrigin.GetComponent<ARCameraManager>();
            raycastManager = xrOrigin.GetComponent<ARRaycastManager>();
        }
    }
    private void Update()
    {
        timeSinceLastCheck += Time.deltaTime;

        if (timeSinceLastCheck >= checkInterval)
        {
            timeSinceLastCheck = 0f;
            CheckMiddleStumpAlignment();
        }
    }

    private void CheckMiddleStumpAlignment()
    {
        Ray leftRay = new Ray(arCameraManager.transform.position,
                            arCameraManager.transform.forward - arCameraManager.transform.right * 0.1f);
        Ray rightRay = new Ray(arCameraManager.transform.position,
                             arCameraManager.transform.forward + arCameraManager.transform.right * 0.1f);

        bool leftHit = Physics.Raycast(leftRay, out RaycastHit leftHitInfo);
        bool rightHit = Physics.Raycast(rightRay, out RaycastHit rightHitInfo);

        if (leftHit && rightHit)
        {
            Vector3 estimatedMiddle = (leftHitInfo.point + rightHitInfo.point) / 2f;
            float detectedWidth = Vector3.Distance(leftHitInfo.point, rightHitInfo.point);

            if (detectedWidth >= minStumpWidth * 0.8f && detectedWidth <= minStumpWidth * 1.2f)
            {
                lastDetectedStumpPosition = estimatedMiddle;
                CheckVirtualAlignment(estimatedMiddle);
                return;
            }
        }

        if (lastDetectedStumpPosition.HasValue)
        {
            CheckVirtualAlignment(lastDetectedStumpPosition.Value);
        }
        else if (isAligned)
        {
            isAligned = false;
            OnAlignmentLost();
        }
    }

    private void CheckVirtualAlignment(Vector3 realMiddlePosition)
    {
        float distance = Vector3.Distance(middleVirtualStump.position, realMiddlePosition);
        float heightDifference = Mathf.Abs(middleVirtualStump.position.y - realMiddlePosition.y);

        bool newAlignmentState = distance <= alignmentThreshold &&
                               heightDifference <= heightTolerance;

        if (newAlignmentState != isAligned)
        {
            isAligned = newAlignmentState;

            if (isAligned)
            {
                OnAlignmentAchieved();
            }
            else
            {
                OnAlignmentLost();
            }
        }
    }

    private void OnAlignmentAchieved()
    {
        header.text = "Stumps Aligned!";
        recorder.SetActive(true);

#if UNITY_ANDROID || UNITY_IOS
        if (isAligned) Handheld.Vibrate();
#endif
    }

    private void OnAlignmentLost()
    {
        header.text = "Alignment lost!";
    }
}
