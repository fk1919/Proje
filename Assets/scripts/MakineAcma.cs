using UnityEngine;
using Oculus;

public class MakineAcma : MonoBehaviour
{
    public Transform triggerVisual;

    public Vector3 pressedLocalPosition = new Vector3(0, -0.005f, 0);
    public Quaternion pressedLocalRotation = Quaternion.Euler(0, 0, 0);

    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;

    private bool isPressed = false;
    private bool wasButtonPressedLastFrame = false;

    // Inspector’da görünürlüðü kontrol etmek için
    public bool showZARYDKAFields = true;

    public bool ZARYDKA4_LOW = true;
    public bool ZARYDKA5_LOW = true;
    public bool ZARYDKA6_LOW = true;

    void Start()
    {
        if (triggerVisual != null)
        {
            initialLocalPosition = triggerVisual.localPosition;
            initialLocalRotation = triggerVisual.localRotation;
        }
    }

    void Update()
    {
        if (triggerVisual == null) return;

        bool isAButtonPressedNow = OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.RTouch);

        if (isAButtonPressedNow && !wasButtonPressedLastFrame)
        {
            isPressed = !isPressed;
            showZARYDKAFields = !showZARYDKAFields; // Toggle görünürlük
        }

        triggerVisual.localPosition = isPressed ? pressedLocalPosition : initialLocalPosition;
        triggerVisual.localRotation = isPressed ? pressedLocalRotation : initialLocalRotation;

        wasButtonPressedLastFrame = isAButtonPressedNow;
    }
}
