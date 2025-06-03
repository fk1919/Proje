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
    public AudioSource makinaAudioSource;    // Inspector’dan sürükle-at yapacağınız AudioSource bileşeni
    public AudioClip makinaCalismaSesi;      // Inspector’dan atayacağınız, looplu oynayacak ses dosyası

    // ▼▼▼ Proximity (Yakınlık) Kontrolü ▼▼▼
    [Header("— Proximity Ayarları —")]
    public Transform KNOPKA1_LOW;           // Etkileşim için kontrol edeceğimiz obje
    public Transform rightHandAnchor;       // Oyuncunun sağ elinin Transform’u (OVR RightHandAnchor)
    public float interactionDistance = 0.5f; // Kaç metre yaklaşıldığında A tuşu aktif olsun

    void Start()
    {
        // ► Tetik Görselinin (triggerVisual) başlangıç pozisyon/rotasyon bilgilerini alıyoruz
        if (triggerVisual != null)
        {
            initialLocalPosition = triggerVisual.localPosition;
            initialLocalRotation = triggerVisual.localRotation;

            triggerRenderer = triggerVisual.GetComponent<Renderer>();
            if (triggerRenderer != null)
                originalColor = triggerRenderer.material.color;
        }

        // ► Eğer Inspector’dan AudioSource atamadıysanız, aynı GameObject üzerindeki AudioSource bileşenini otomatik al
        if (makinaAudioSource == null)
        {
            makinaAudioSource = GetComponent<AudioSource>();
        }
    }

    void Update()
    {
        // Eğer triggerVisual tanımlı değilse hiçbir işlem yapma
        if (triggerVisual == null) return;

        // ► Sağ el Transform’u tanımlı mı kontrol et
        if (rightHandAnchor == null || KNOPKA1_LOW == null)
        {
            Debug.LogWarning("MakineAcma: KNOPKA1_LOW veya rightHandAnchor atanmamış!");
            return;
        }

        // ► A tuşuna (Button.One) basılıp basılmadığını oku
        bool isAButtonPressedNow = OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.RTouch);

        // ► Sağ el ile KNOPKA1_LOW objesi arasındaki mesafeyi hesapla
        float distanceToKnopka = Vector3.Distance(rightHandAnchor.position, KNOPKA1_LOW.position);
        bool isNearKnopka = distanceToKnopka <= interactionDistance;

        // — Eğer yeterince yakınsa A tuşu işlenebilir; uzakken hiçbir şey yapma
        if (isNearKnopka)
        {
            // Yeni bir tıklama (false→true) algılandı mı?
            if (isAButtonPressedNow && !wasButtonPressedLastFrame)
            {
                // ► isPressed durumunu tersine çevir (false→true aç, true→false kapat)
                isPressed = !isPressed;
                // Görsel alanların gösterim durumunu tersine çevir
                showZARYDKAFields = !showZARYDKAFields;

                // ► Tetik görselinin rengini değiştir
                if (triggerRenderer != null)
                {
                    triggerRenderer.material.color = isPressed ? pressedColor : originalColor;
                }

                // ▼--------- SES OYNATMA / DURDURMA BÖLÜMÜ ---------▼
                if (isPressed)
                {
                    // Makine açıldıysa:
                    if (makinaAudioSource != null && makinaCalismaSesi != null)
                    {
                        makinaAudioSource.clip = makinaCalismaSesi;
                        makinaAudioSource.loop = true;   // Inspector’da da işaretli olabilir, burada garanti
                        makinaAudioSource.Play();
                    }
                }
                else
                {
                    // Makine kapandıysa:
                    if (makinaAudioSource != null)
                    {
                        makinaAudioSource.Stop();
                    }
                }
                // ▲----------------------------------------------▲
            }
        }
        // — Eğer uzakken A tuşa basılsa bile hiçbir ek işlem yapmayacağız (butonun durumunu değiştirmeyeceğiz)

        // ► Tetik görselinin pozisyon/rotasyonunu, her durumda isPressed değerine göre güncelle
        triggerVisual.localPosition = isPressed ? pressedLocalPosition : initialLocalPosition;
        triggerVisual.localRotation = isPressed ? pressedLocalRotation : initialLocalRotation;

        // ► ZARYDKA nesnelerini aktif/pasif yap
        if (ZARYDKA4 != null) ZARYDKA4.SetActive(showZARYDKAFields);
        if (ZARYDKA5 != null) ZARYDKA5.SetActive(showZARYDKAFields);
        if (ZARYDKA6 != null) ZARYDKA6.SetActive(showZARYDKAFields);

        // ► Bir sonraki karede, önceki basılı durumu karşılaştırabilmek için flag’i güncelle
        wasButtonPressedLastFrame = isAButtonPressedNow;
    }
}
