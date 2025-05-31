using UnityEngine;
using Oculus;

public class WeldingTorchTriggerVisual : MonoBehaviour
{
    public Transform triggerVisual;  // Tetik parçasý
    public Vector3 pressedLocalPosition = new Vector3(0, -0.005f, 0); // Ýçeri gideceði pozisyon

    private Vector3 initialLocalPosition;

    void Start()
    {
        if (triggerVisual != null)
            initialLocalPosition = triggerVisual.localPosition;
    }

    void Update()
    {
        if (triggerVisual == null) return;

        float triggerValue = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
        triggerVisual.localPosition = Vector3.Lerp(initialLocalPosition, pressedLocalPosition, triggerValue);
    }
}
