using UnityEngine;
using Oculus;

public class MakineAcma : MonoBehaviour
{
    // ▼▼▼ Buton/Görsel Kontrol Değişkenleri ▼▼▼
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
    private bool showZARYDKAFields = true;

    // Renk kontrolü
    private Renderer triggerRenderer;
    private Color originalColor;
    public Color pressedColor = Color.green;

    // ▼▼▼ Ses ile İlgili Değişkenler ▼▼▼
    [Header("— Ses Ayarları —")]
    public AudioSource makinaAudioSource;
    public AudioClip makinaCalismaSesi;

    // ▼▼▼ Proximity (Yakınlık) Kontrolü ▼▼▼
    [Header("— Proximity Ayarları —")]
    public Transform KNOPKA1_LOW;
    public Transform rightHandAnchor;
    public float interactionDistance = 0.5f;

    // ▼▼▼ Fan Dönme Ayarları ▼▼▼
    [Header("— Fan Ayarları —")]
    public Transform fanTransform;           // Dönecek fan objesi
    public float fanRotationSpeed = 360f;    // Derece/sn cinsinden dönüş hızı
    public Transform fanPivot;               // Fanın döneceği merkez noktası

    void Start()
    {
        if (triggerVisual != null)
        {
            initialLocalPosition = triggerVisual.localPosition;
            initialLocalRotation = triggerVisual.localRotation;

            triggerRenderer = triggerVisual.GetComponent<Renderer>();
            if (triggerRenderer != null)
                originalColor = triggerRenderer.material.color;
        }

        if (makinaAudioSource == null)
        {
            makinaAudioSource = GetComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (triggerVisual == null) return;

        if (rightHandAnchor == null || KNOPKA1_LOW == null)
        {
            Debug.LogWarning("MakineAcma: KNOPKA1_LOW veya rightHandAnchor atanmamış!");
            return;
        }

        bool isAButtonPressedNow = OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.RTouch);
        float distanceToKnopka = Vector3.Distance(rightHandAnchor.position, KNOPKA1_LOW.position);
        bool isNearKnopka = distanceToKnopka <= interactionDistance;

        if (isNearKnopka)
        {
            if (isAButtonPressedNow && !wasButtonPressedLastFrame)
            {
                isPressed = !isPressed;
                showZARYDKAFields = !showZARYDKAFields;

                if (triggerRenderer != null)
                    triggerRenderer.material.color = isPressed ? pressedColor : originalColor;

                if (isPressed)
                {
                    if (makinaAudioSource != null && makinaCalismaSesi != null)
                    {
                        makinaAudioSource.clip = makinaCalismaSesi;
                        makinaAudioSource.loop = true;
                        makinaAudioSource.Play();
                    }
                }
                else if (makinaAudioSource != null)
                {
                    makinaAudioSource.Stop();
                }
            }
        }

        // Tetik görselinin güncellenmesi
        triggerVisual.localPosition = isPressed ? pressedLocalPosition : initialLocalPosition;
        triggerVisual.localRotation = isPressed ? pressedLocalRotation : initialLocalRotation;

        // ZARYDKA nesneleri
        if (ZARYDKA4 != null) ZARYDKA4.SetActive(showZARYDKAFields);
        if (ZARYDKA5 != null) ZARYDKA5.SetActive(showZARYDKAFields);
        if (ZARYDKA6 != null) ZARYDKA6.SetActive(showZARYDKAFields);

        // Fanı belirli bir merkez noktası etrafında Z ekseninde döndür (makine çalışırken)
        if (isPressed && fanTransform != null && fanPivot != null)
        {
            // fanPivot.position etrafında döner
            fanTransform.RotateAround(fanPivot.position, Vector3.forward, fanRotationSpeed * Time.deltaTime);
        }

        wasButtonPressedLastFrame = isAButtonPressedNow;
    }
}
