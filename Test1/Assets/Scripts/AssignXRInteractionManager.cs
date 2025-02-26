using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class AssignXRInteractionManager : MonoBehaviour
{
    void Start()
    {
        XRGrabInteractable grabInteractable = GetComponent<XRGrabInteractable>();
        
        if (grabInteractable != null)
        {
            XRInteractionManager interactionManager = FindObjectOfType<XRInteractionManager>();

            if (interactionManager != null)
            {
                grabInteractable.interactionManager = interactionManager;
                Debug.Log("Interaction Manager assigned successfully.");
            }
            else
            {
                Debug.LogError("No XRInteractionManager found in the scene!");
            }
        }
        else
        {
            Debug.LogError("No XRGrabInteractable found on this object!");
        }
    }
}