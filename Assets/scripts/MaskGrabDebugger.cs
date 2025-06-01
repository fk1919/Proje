using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MaskGrabDebugger : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (grabInteractable == null)
        {
            Debug.LogError("XRGrabInteractable bile�eni eksik!");
        }
        else
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
        }
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        Debug.Log("Maske tutuldu: " + args.interactorObject.transform.name);
    }

    void OnRelease(SelectExitEventArgs args)
    {
        Debug.Log("Maske b�rak�ld�: " + args.interactorObject.transform.name);
    }
}