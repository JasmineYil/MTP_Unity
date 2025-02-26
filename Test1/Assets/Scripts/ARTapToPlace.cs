using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class ARTapToPlace : MonoBehaviour
{
    public GameObject blueCubePrefab; 
    private ARRaycastManager raycastManager;
    public static GameObject spawnedObject; 

    private GameObject selectedObject;
    private bool isDragging = false;
    private float initialPinchDistance;
    private Vector3 initialScale;
    private float initialRotationAngle;
    private Vector2 initialTouch1, initialTouch2;

    void Start()
    {
        raycastManager = FindObjectOfType<ARRaycastManager>();
        Debug.Log("ARTapToPlace started!");
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
                    Debug.Log("Dragging finished!");
                }
                return;
            }

            if (touch.phase == TouchPhase.Began && TrySelectExistingCube(touchPosition))
            {
                return;
            }

            if (touch.phase == TouchPhase.Began)
            {
                TryPlaceOrMoveCube(touchPosition);
            }
        }
    }

    private bool TrySelectExistingCube(Vector2 touchPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(touchPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log($"Objekt recognized: {hit.collider.gameObject.name}");

            if (hit.collider.gameObject.CompareTag("Spawnable"))
            {
                selectedObject = hit.collider.gameObject;
                isDragging = true;
                Debug.Log("Object with ARTransformer recognized!");

                EnableInteraction(selectedObject);
                return true;
            }
        }

        return false;
    }

    private void TryPlaceOrMoveCube(Vector2 touchPosition)
    {
        List<ARRaycastHit> hits = new List<ARRaycastHit>();

        if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            Debug.Log("Surface recognized at: " + hitPose.position);

            if (spawnedObject == null)
            {
                spawnedObject = Instantiate(blueCubePrefab, hitPose.position, hitPose.rotation);
                spawnedObject.tag = "Spawnable"; 
                EnableInteraction(spawnedObject);
                Debug.Log("New Blue Cube spawned!");
            }
            else
            {
                spawnedObject.transform.position = hitPose.position;
                Debug.Log("Cube moved!");
            }
        }
        else
        {
            Debug.Log("No valid Raycast results! No plane recognized.");
        }
    }

    private void MoveSelectedObject(Vector2 touchPosition)
    {
        List<ARRaycastHit> hits = new List<ARRaycastHit>();

        if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            selectedObject.transform.position = hitPose.position;
            Debug.Log("Cube moved: " + hitPose.position);
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

            Debug.Log($"Scale changed to: {selectedObject.transform.localScale}");
            Debug.Log($"Rotation changed to: {selectedObject.transform.rotation.eulerAngles}");
        }
    }

    public void DeselectObject()
    {
        Debug.Log("Interaktion stopped!");
        selectedObject = null;
        isDragging = false;
    }
}
