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

    // Görsel nesneler
    public GameObject ZARYDKA4;
    public GameObject ZARYDKA5;
    public GameObject ZARYDKA6;

    public bool showZARYDKAFields = true;

    // Renk kontrolü
    private Renderer triggerRenderer;
    private Color originalColor;
    public Color pressedColor = Color.green;

    void Start()
    {
        if (triggerVisual != null)
        {
            initialLocalPosition = triggerVisual.localPosition;
            initialLocalRotation = triggerVisual.localRotation;

            triggerRenderer = triggerVisual.GetComponent<Renderer>();
            if (triggerRenderer != null)
            {
                originalColor = triggerRenderer.material.color;
            }
        }
    }

    void Update()
    {
        if (triggerVisual == null) return;

        bool isAButtonPressedNow = OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.RTouch);

        if (isAButtonPressedNow && !wasButtonPressedLastFrame)
        {
            isPressed = !isPressed;
            showZARYDKAFields = !showZARYDKAFields;

            // Rengi deðiþtir
            if (triggerRenderer != null)
            {
                triggerRenderer.material.color = isPressed ? pressedColor : originalColor;
            }
        }

        // Düðmenin pozisyon ve rotasyonunu güncelle
        triggerVisual.localPosition = isPressed ? pressedLocalPosition : initialLocalPosition;
        triggerVisual.localRotation = isPressed ? pressedLocalRotation : initialLocalRotation;

        // ZARYDKA nesnelerini aktif/pasif yap
        if (ZARYDKA4 != null) ZARYDKA4.SetActive(showZARYDKAFields);
        if (ZARYDKA5 != null) ZARYDKA5.SetActive(showZARYDKAFields);
        if (ZARYDKA6 != null) ZARYDKA6.SetActive(showZARYDKAFields);

        wasButtonPressedLastFrame = isAButtonPressedNow;
    }
}

