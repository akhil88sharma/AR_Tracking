using System.Collections.Generic;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class TestScript : MonoBehaviour
{
    public XROrigin xrOrigin;
    private ARPointCloudManager pointCloudManager;
    private ARPlaneManager planeManager;
    private ARAnchorManager anchorManager;
    private ARRaycastManager raycastManager;

    public TextMeshProUGUI header;
    public TextMeshProUGUI logStats;
    public GameObject virtualObject;

    public float detectionThreshold = 0.3f;  // Adjusted for long-range detection
    public float maxDetectionDistance = 25f; // Increased to detect distant objects
    public float raycastDistance = 30f;      // Longer raycast distance
    public Transform arCamera;               // Reference to AR Camera

    private List<Vector3> detectedFeaturePoints = new List<Vector3>();
    private ARAnchor anchor;

    void Start()
    {
        header.text = "Place Stumps at the marked location";
        logStats.text = "";

        if (xrOrigin != null)
        {
            pointCloudManager = xrOrigin.GetComponent<ARPointCloudManager>();
            planeManager = xrOrigin.GetComponent<ARPlaneManager>();
            anchorManager = xrOrigin.GetComponent<ARAnchorManager>();
            raycastManager = xrOrigin.GetComponent<ARRaycastManager>();
        }

        if (virtualObject == null)
        {
            Debug.LogError("Virtual Object not assigned!");
            logStats.text = "Virtual Object not assigned";
            return;
        }

        if (pointCloudManager == null || planeManager == null || anchorManager == null || raycastManager == null)
        {
            Debug.LogError("Missing AR Managers!");
            logStats.text = "Missing AR Managers";
            return;
        }

        PlaceVirtualObjectAtFixedPosition();
    }

    void Update()
    {
        UpdateFeaturePoints();

        bool isObjectPlacedCorrectly = IsRealObjectAtSamePosition(virtualObject.transform.position) ||
                                       CheckObjectUsingRaycast(virtualObject.transform.position);

        if (isObjectPlacedCorrectly)
        {
            Debug.Log("Real object detected at the correct position!");
            header.text = "Stumps Aligned!";
        }
        else
        {
            header.text = "Align Stumps Properly";
        }
    }

    void PlaceVirtualObjectAtFixedPosition()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        List<ARRaycastHit> hits = new List<ARRaycastHit>();

        if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinBounds))
        {
            Pose hitPose = hits[0].pose;
            anchor = anchorManager.AttachAnchor(hits[0].trackable as ARPlane, hitPose);

            if (anchor != null)
            {
                virtualObject.transform.SetParent(anchor.transform);
                virtualObject.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
            }
        }
    }

    void UpdateFeaturePoints()
    {
        detectedFeaturePoints.Clear();
        foreach (ARPointCloud pointCloud in pointCloudManager.trackables)
        {
            if (pointCloud.positions.HasValue)
            {
                foreach (Vector3 point in pointCloud.positions.Value)
                {
                    float distanceToCamera = Vector3.Distance(arCamera.position, point);
                    if (distanceToCamera < maxDetectionDistance) // Only store points within the max range
                    {
                        detectedFeaturePoints.Add(point);
                    }
                }
            }
        }
    }

    bool IsRealObjectAtSamePosition(Vector3 virtualPos)
    {
        foreach (Vector3 realPoint in detectedFeaturePoints)
        {
            float distance = Vector3.Distance(realPoint, virtualPos);
            if (distance < detectionThreshold)
            {
                logStats.text = "Positions aligned!";
                return true;
            }
        }
        return false;
    }

    bool CheckObjectUsingRaycast(Vector3 virtualPos)
    {
        // Forward Raycast from AR Camera
        RaycastHit hit;
        Vector3 direction = virtualPos - arCamera.position;
        if (Physics.Raycast(arCamera.position, direction.normalized, out hit, raycastDistance))
        {
            Debug.Log("Real object detected using Camera Raycast: " + hit.collider.gameObject.name);
            logStats.text = "Detected using Camera Raycast: " + hit.collider.gameObject.name;
            return true;
        }

        // Downward Raycast for better accuracy
        if (Physics.Raycast(virtualPos + Vector3.up * 1.0f, Vector3.down, out hit, raycastDistance))
        {
            Debug.Log("Real object detected using Downward Raycast: " + hit.collider.gameObject.name);
            logStats.text = "Detected using Downward Raycast: " + hit.collider.gameObject.name;
            return true;
        }

        return false;
    }
}
