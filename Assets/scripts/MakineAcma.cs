using UnityEngine;
using Oculus;

public class MakineAcma : MonoBehaviour
{
    public Transform triggerVisual;  // Tetik par�as�
    public Vector3 pressedLocalPosition = new Vector3(0, -0.005f, 0); // ��eri girece�i pozisyon

    private Vector3 initialLocalPosition;
    private bool isPressed = false;  // Toggle durumu
    public float activationDistance = 0.5f; // Etkile�im mesafesi

    void Start()
    {
        if (triggerVisual != null)
            initialLocalPosition = triggerVisual.localPosition;
    }

    void Update()
    {
        if (triggerVisual == null) return;

        // Sa� kontrolc�n�n d�nya konumunu al
        Vector3 rightControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);

        // E�er rig hareketliyse, local pozisyonu d�nya pozisyonuna d�n��t�rmen gerekebilir.
        // Bu durumda pozisyon do�ru g�r�nm�yorsa OVRCameraRig'in Transform'u �zerinden �evir:
        // Vector3 worldControllerPosition = cameraRigTransform.TransformPoint(rightControllerPosition);

        float distance = Vector3.Distance(rightControllerPosition, transform.position);

        if (distance <= activationDistance)
        {
            if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
            {
                isPressed = !isPressed;
            }
        }

        // Tetik konumunu g�ncelle
        triggerVisual.localPosition = isPressed ? pressedLocalPosition : initialLocalPosition;
    }
}
