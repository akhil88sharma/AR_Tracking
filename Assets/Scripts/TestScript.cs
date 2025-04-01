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
    public GameObject virtualObject;
    public GameObject recorder;

    public float detectionThreshold = 0.3f;
    public float maxDetectionDistance = 25f;
    public float raycastDistance = 30f;
    public Transform arCamera;

    private List<Vector3> detectedFeaturePoints = new List<Vector3>();
    private ARAnchor anchor;

    void Start()
    {
        header.text = "Place Stumps at the marked location";

        if (xrOrigin != null)
        {
            pointCloudManager = xrOrigin.GetComponent<ARPointCloudManager>();
            planeManager = xrOrigin.GetComponent<ARPlaneManager>();
            anchorManager = xrOrigin.GetComponent<ARAnchorManager>();
            raycastManager = xrOrigin.GetComponent<ARRaycastManager>();
        }

        if (virtualObject == null)
        {
            return;
        }

        if (pointCloudManager == null || planeManager == null || anchorManager == null || raycastManager == null)
        {
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
            header.text = "Stumps Aligned!";
            recorder.gameObject.SetActive(true);
        }
        else
        {
            header.text = "Align the stumps!";
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
                    if (distanceToCamera < maxDetectionDistance)
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
            return true;
        }

        // Downward Raycast for better accuracy
        if (Physics.Raycast(virtualPos + Vector3.up * 1.0f, Vector3.down, out hit, raycastDistance))
        {
            return true;
        }

        return false;
    }
}
