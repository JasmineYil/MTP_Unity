using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class ARTargetHandler : MonoBehaviour
{
    private GameObject selectedObject;
    private bool isDragging = false;
    private bool isDetachedFromMarker = false;
    private float initialPinchDistance;
    private Vector3 initialScale;
    private float initialRotationAngle;
    private Vector2 initialTouch1, initialTouch2;
    private ARTrackedImageManager imageManager;

    void Start()
    {
        imageManager = FindObjectOfType<ARTrackedImageManager>();
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 touchPosition = touch.position;
            Debug.Log("Touch recognized: " + touchPosition);

            if (Input.touchCount == 2 && selectedObject != null)
            {
                HandlePinchToScaleAndRotate();
                return;
            }

            if (isDragging && selectedObject != null)
            {
                MoveSelectedObject(touchPosition);
                if (touch.phase == TouchPhase.Ended)
                {
                    isDragging = false;
                    Debug.Log("Dragging stopped!");
                }
                return;
            }

            if (touch.phase == TouchPhase.Began && TrySelectExistingCube(touchPosition))
            {
                return;
            }
        }
    }

    private bool TrySelectExistingCube(Vector2 touchPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(touchPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log($"Object recognized: {hit.collider.gameObject.name}");

            if (hit.collider.gameObject.CompareTag("RedCube"))
            {
                selectedObject = hit.collider.gameObject;
                isDragging = true;
                Debug.Log("Red Cube recognized and ready for interactin!");

                EnableInteraction(selectedObject);
                StartCoroutine(DetachFromMarker(selectedObject));
                return true;
            }
        }

        return false;
    }

    private IEnumerator DetachFromMarker(GameObject cube)
    {
        Debug.Log("DetachFromMarker started!");
        if (!isDetachedFromMarker)
        {
            cube.transform.parent = null;
            cube.transform.SetParent(null, true);
            Debug.Log($"New Parent: {cube.transform.parent}");
            Debug.Log($"Parent before Detach (Erzwingen): {cube.transform.parent}");
            isDetachedFromMarker = true;
            Debug.Log("Detach Cube from Marker, successful.");
            Debug.Log($"Parent after Detach: {cube.transform.parent}");
            Debug.Log($"ARTrackedImageManager Status: {imageManager.enabled}");

            if (imageManager != null)
            {
                Debug.Log("ARTrackedImageManager restarting!...");
                imageManager.enabled = false;
                StartCoroutine(ReenableImageManager());
                Debug.Log($"ARTrackedImageManager deactivated: {imageManager.enabled}");
                yield return null; 
                imageManager.enabled = true;
            }
        }
    }
    
    private IEnumerator ReenableImageManager()
    {
        yield return new WaitForSeconds(0.5f);
        imageManager.enabled = true;
    }
    
    private void MoveSelectedObject(Vector2 touchPosition)
    {
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        ARRaycastManager raycastManager = FindObjectOfType<ARRaycastManager>();

        if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            selectedObject.transform.position = hitPose.position;
            Debug.Log("RedCube moved: " + hitPose.position);
        }
    }

    private void EnableInteraction(GameObject cube)
    {
        if (!cube.TryGetComponent<ARTransformer>(out _))
        {
            cube.AddComponent<ARTransformer>();
        }
    }

    private void HandlePinchToScaleAndRotate()
    {
        Touch touch1 = Input.GetTouch(0);
        Touch touch2 = Input.GetTouch(1);

        if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
        {
            initialPinchDistance = Vector2.Distance(touch1.position, touch2.position);
            initialScale = selectedObject.transform.localScale;
            initialTouch1 = touch1.position;
            initialTouch2 = touch2.position;
            initialRotationAngle = Mathf.Atan2(initialTouch2.y - initialTouch1.y, initialTouch2.x - initialTouch1.x) * Mathf.Rad2Deg;
        }
        else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
        {
            float currentPinchDistance = Vector2.Distance(touch1.position, touch2.position);
            float scaleFactor = currentPinchDistance / initialPinchDistance;
            selectedObject.transform.localScale = initialScale * scaleFactor;

            float currentAngle = Mathf.Atan2(touch2.position.y - touch1.position.y, touch2.position.x - touch1.position.x) * Mathf.Rad2Deg;
            float angleDifference = currentAngle - initialRotationAngle;
            selectedObject.transform.Rotate(Vector3.up, angleDifference);
            initialRotationAngle = currentAngle;

            Debug.Log($"New Scale: {selectedObject.transform.localScale}");
            Debug.Log($"New Rotation: {selectedObject.transform.rotation.eulerAngles}");
        }
    }

    public void DeselectObject()
    {
        Debug.Log("Interaction finished!");
        selectedObject = null;
        isDragging = false;
    }
}
