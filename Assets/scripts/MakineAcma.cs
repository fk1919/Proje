using UnityEngine;
using Oculus;

public class MakineAcma : MonoBehaviour
{
    public Transform triggerVisual;  // Tetik parçasý
    public Vector3 pressedLocalPosition = new Vector3(0, -0.005f, 0); // Ýçeri gireceði pozisyon

    private Vector3 initialLocalPosition;
    private bool isPressed = false;  // Toggle durumu
    public float activationDistance = 0.5f; // Etkileþim mesafesi

    void Start()
    {
        if (triggerVisual != null)
            initialLocalPosition = triggerVisual.localPosition;
    }

    void Update()
    {
        if (triggerVisual == null) return;

        // Sað kontrolcünün dünya konumunu al
        Vector3 rightControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);

        // Eðer rig hareketliyse, local pozisyonu dünya pozisyonuna dönüþtürmen gerekebilir.
        // Bu durumda pozisyon doðru görünmüyorsa OVRCameraRig'in Transform'u üzerinden çevir:
        // Vector3 worldControllerPosition = cameraRigTransform.TransformPoint(rightControllerPosition);

        float distance = Vector3.Distance(rightControllerPosition, transform.position);

        if (distance <= activationDistance)
        {
            if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
            {
                isPressed = !isPressed;
            }
        }

        // Tetik konumunu güncelle
        triggerVisual.localPosition = isPressed ? pressedLocalPosition : initialLocalPosition;
    }
}
