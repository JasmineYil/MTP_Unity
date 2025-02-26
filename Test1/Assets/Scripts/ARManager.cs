using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections;
using System.Collections.Generic;

public class ARManager : MonoBehaviour
{
    public ARSession arSession;
    public ARTrackedImageManager imageManager;
    public ARPlaneManager planeManager;
    public ARRaycastManager raycastManager;

    private bool useMarkerTracking = false;

    void Start()
    {
        SetTrackingMode(useMarkerTracking);
    }

    public void ToggleTrackingMode()
    {
        useMarkerTracking = !useMarkerTracking;
        StartCoroutine(RestartARSession(useMarkerTracking));
    }

    private IEnumerator RestartARSession(bool markerTracking)
    {
        Debug.Log("AR Session restart.");

        if (imageManager != null) imageManager.enabled = false;
        if (planeManager != null) planeManager.enabled = false;
        if (raycastManager != null) raycastManager.enabled = false;

        yield return new WaitForEndOfFrame();

        ClearSpawnedObjects();

        yield return new WaitForEndOfFrame();

        if (arSession != null)
        {
            Debug.Log("Reset ARSession.");
            arSession.Reset();
        }

        yield return new WaitForSeconds(0.5f); 

        SetTrackingMode(markerTracking);
    }

    private void ClearSpawnedObjects()
    {
        Debug.Log("deleted old Objects.");

        if (ARTapToPlace.spawnedObject != null)
        {
            Destroy(ARTapToPlace.spawnedObject);
            ARTapToPlace.spawnedObject = null;
            Debug.Log("Blue Cube deleted!");
        }

        if (imageManager != null)
        {
            List<ARTrackedImage> trackedImagesToRemove = new List<ARTrackedImage>();
            foreach (var trackedImage in imageManager.trackables)
            {
                trackedImagesToRemove.Add(trackedImage);
            }

            foreach (var trackedImage in trackedImagesToRemove)
            {
                if (trackedImage != null && trackedImage.gameObject != null)
                {
                    Destroy(trackedImage.gameObject);
                }
            }
        }

        if (planeManager != null)
        {
            List<ARPlane> planesToRemove = new List<ARPlane>();
            foreach (var plane in planeManager.trackables)
            {
                planesToRemove.Add(plane);
            }

            foreach (var plane in planesToRemove)
            {
                if (plane != null && plane.gameObject != null)
                {
                    Destroy(plane.gameObject);
                    Debug.Log($"ARPlane deleted: {plane.trackableId}");
                }
            }
        }
    }

    private void SetTrackingMode(bool markerTracking)
    {
        Debug.Log("Change AR mode...");

        if (imageManager != null) imageManager.enabled = markerTracking;

        if (planeManager != null) planeManager.enabled = !markerTracking;
        if (raycastManager != null) raycastManager.enabled = !markerTracking;

        Debug.Log(markerTracking ? "Marker-based Tracking activated" : "Markerless Tracking activated");
    }
}
