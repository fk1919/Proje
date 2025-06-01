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

    // Inspector’dan ayarlamak için GameObject referanslarý
    public GameObject ZARYDKA4;
    public GameObject ZARYDKA5;
    public GameObject ZARYDKA6;

    // Görünürlük kontrolü
    public bool showZARYDKAFields = true;

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

        // Oculus A tuþuna basýlýyor mu kontrol et
        bool isAButtonPressedNow = OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.RTouch);

        // Tuþ bu frame’de ilk defa basýldýysa
        if (isAButtonPressedNow && !wasButtonPressedLastFrame)
        {
            isPressed = !isPressed;
            showZARYDKAFields = !showZARYDKAFields; // Toggle görünürlük
        }

        // Düðmenin konum ve rotasyonunu güncelle
        triggerVisual.localPosition = isPressed ? pressedLocalPosition : initialLocalPosition;
        triggerVisual.localRotation = isPressed ? pressedLocalRotation : initialLocalRotation;

        // ZARYDKA nesnelerinin görünürlüðünü ayarla
        if (ZARYDKA4 != null) ZARYDKA4.SetActive(showZARYDKAFields);
        if (ZARYDKA5 != null) ZARYDKA5.SetActive(showZARYDKAFields);
        if (ZARYDKA6 != null) ZARYDKA6.SetActive(showZARYDKAFields);

        // Bu frame'deki tuþ durumu, bir sonraki frame için saklanýr
        wasButtonPressedLastFrame = isAButtonPressedNow;
    }
}
