using UnityEngine;
using Oculus.VR;
using Oculus.Interaction; // Grabbable için (IInteractor'ı doğrudan kullanmayacağız)

public class MaskAttachDetector : MonoBehaviour
{
    [Header("Mask ve Head Ayarları")]
    public GameObject maskModel; // Maskenin görsel modelini içeren GameObject
    public Transform headTarget; // Kaskın takıldığı yer (kafa objesinin Transform'u)

    [Header("Mesafeye Gööre Otomatik Takma")]
    [Tooltip("Maskeyi takmak için headTarget'e ne kadar yakın olması gerektiği (metre).")]
    public float attachProximityThreshold = 0.20f;

    [Header("Işık Ayarları")]
    public Light directionalLight; // Sahnedeki yönsel ışık
    public Color attachedLightColor = Color.gray; // Takılıyken ışık rengi
    public float attachedIntensity = 0.95f; // Takılıyken ışık yoğunluğu

    [Header("VR Kontrolcü ve Detach Ayarları")]
    public OVRInput.Controller detachController = OVRInput.Controller.RHand; // Hangi elin çıkaracağını belirle
    public OVRInput.Button detachButton = OVRInput.Button.PrimaryHandTrigger; // Hangi tuşla çıkarılacak (Grip tuşu)
    public float detachProximityThreshold = 0.20f; // Çıkarma için elin maskeye ne kadar yakın olması gerektiği

    [Header("Interactor Referansları")]
    // Inspector'dan HandGrabInteractor veya TouchHandGrabInteractor gibi scriptlerin bağlı olduğu objeleri atayın.
    public MonoBehaviour leftHandInteractorObject; // Sol el Interactor objesi
    public MonoBehaviour rightHandInteractorObject; // Sağ el Interactor objesi

    private bool isAttached = false; // Maske takılı mı?
    private float attachCooldown = 0f; // Hızlı takma/çıkarma döngüsünü engellemek için
    private Color originalLightColor; // Orijinal ışık rengi
    private float originalIntensity; // Orijinal ışık yoğunluğu
    private Grabbable grabbableComponent; // Maskenin Grabbable komponenti
    private bool isPreparingToDetach = false; // Çıkarma için ön hazırlık yapılıyor mu?

    private void Start()
    {
        // Yönsel ışık referansını kontrol et ve orijinal değerleri kaydet
        if (directionalLight != null)
        {
            originalLightColor = directionalLight.color;
            originalIntensity = directionalLight.intensity;
            Debug.Log($"MaskAttachDetector Başladı. Orijinal Işık Renk: {originalLightColor}, Yoğunluk: {originalIntensity}");
        }
        else
        {
            Debug.LogError("MaskAttachDetector: Directional Light Inspector'da atanmamış!");
        }

        // Grabbable komponentini al
        grabbableComponent = GetComponent<Grabbable>();
        if (grabbableComponent == null)
        {
            Debug.LogError("MaskAttachDetector: Grabbable komponenti bulunamadı! Maske objenizde Grabbable scripti olduğundan emin olun.");
        }

        // Interactor referanslarının atanıp atanmadığını kontrol et
        if (leftHandInteractorObject == null || rightHandInteractorObject == null)
        {
            Debug.LogWarning("MaskAttachDetector: Interactor referansları atanmadı. El yakınlık kontrolü ve çıkarma çalışmayabilir.");
        }
        Debug.Log($"MaskAttachDetector: Trigger Tag: Yok (OnTriggerEnter kullanılmıyor), Head Target: {(headTarget != null ? headTarget.name : "Yok")}, Mask Model: {(maskModel != null ? maskModel.name : "Yok")}");
    }

    private void Update()
    {
        // Cooldown süresini azalt
        if (attachCooldown > 0f)
        {
            attachCooldown -= Time.deltaTime;
        }

        // Maske takılı değilse ve cooldown yoksa ve kafa hedefi atanmışsa
        // Maskeyi takmak için mesafeyi kontrol et
        if (!isAttached && attachCooldown <= 0f && headTarget != null)
        {
            float distanceToHead = Vector3.Distance(transform.position, headTarget.position);
            // Debug.Log($"Attach Kontrol: Maske ile kafa arası mesafe: {distanceToHead:F2}m."); // Çok fazla loglamamak için yorumda
            if (distanceToHead <= attachProximityThreshold)
            {
                AttachMask(); // Maskeyi tak
            }
        }

        // Maske takılıysa ve cooldown yoksa
        if (isAttached && attachCooldown <= 0f)
        {
            // Eğer çıkarma butonu basılı tutuluyorsa
            if (OVRInput.Get(detachButton, detachController))
            {
                // Aktif Interactor'ın transform'unu al (elin pozisyonu için)
                Transform activeInteractorTransform = null;
                if (detachController == OVRInput.Controller.RHand && rightHandInteractorObject != null)
                {
                    activeInteractorTransform = rightHandInteractorObject.transform;
                }
                else if (detachController == OVRInput.Controller.LHand && leftHandInteractorObject != null)
                {
                    activeInteractorTransform = leftHandInteractorObject.transform;
                }

                if (activeInteractorTransform != null)
                {
                    // El ile maske arasındaki mesafeyi hesapla
                    float distanceHandToMask = Vector3.Distance(transform.position, activeInteractorTransform.position);
                    Debug.Log($"Update: El ile maske arası mesafe: {distanceHandToMask:F2}m. Eşik: {detachProximityThreshold:F2}m.");

                    // Eğer el maskeye yeterince yakınsa
                    if (distanceHandToMask <= detachProximityThreshold)
                    {
                        // Maskeyi çıkarmak için ön hazırlık yap (görünür yap, tutulabilir yap)
                        PrepareToDetach();
                    }
                    else
                    {
                        Debug.Log("Update: El maskeye yeterince yakın değil. Çıkarma ön hazırlığı tetiklenmedi.");
                        // Eğer el uzaklaştıysa ve ön hazırlık yapılıyorsa, durumu sıfırla
                        if (isPreparingToDetach)
                        {
                            ResetDetachPreparation();
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("Update: Detach işlemi için atanmış Interactor Object bulunamadı veya yanlış kontrolcü seçildi.");
                }
            }
            else // Eğer detach butonu basılı değilse
            {
                // Eğer ön hazırlık yapılıyorsa ama buton bırakıldıysa, durumu sıfırla
                if (isPreparingToDetach)
                {
                    ResetDetachPreparation();
                }
            }

            // Maske takılıysa VE çıkarma için ön hazırlık yapılıyorsa VE maske gerçekten tutulduysa
            // grabbableComponent.IsGrabbed yerine grabbableComponent.SelectingPointsCount > 0 kullanıldı
            if (isPreparingToDetach && grabbableComponent != null && grabbableComponent.SelectingPointsCount > 0)
            {
                Debug.Log("Update: Maske ön hazırlık durumunda ve Interactor tarafından tutuldu. Tam çıkarma işlemi başlatılıyor.");
                CompleteDetach(); // Tam çıkarma işlemini yap
            }
        }
    }

    private void AttachMask()
    {
        Debug.Log("AttachMask çağrıldı: Maske takılıyor.");
        isAttached = true; // Maskeyi takılı olarak işaretle
        isPreparingToDetach = false; // Takılıyken ön hazırlık durumu kapalı

        // Maskenin görselini gizle
        if (maskModel != null)
        {
            maskModel.SetActive(false);
            Debug.Log("AttachMask: Maske modeli gizlendi.");
        }
        else { Debug.LogError("AttachMask: Maske modeli atanmamış!"); }

        // Maskeyi kafa objesine parent yap
        if (headTarget != null)
        {
            transform.SetParent(headTarget, true); // Dünya pozisyonunu koruyarak parent yap
            Debug.Log($"AttachMask: Maske, {headTarget.name} objesine parent yapıldı. Yeni parent: {transform.parent.name}");
        }
        else { Debug.LogError("AttachMask: Kafa hedefi atanmamış!"); }

        // Rigidbody'yi kinematic yapıp fiziği durdur
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            Debug.Log("AttachMask: Maske Rigidbody kinematic yapıldı.");
        }
        else { Debug.LogError("AttachMask: Rigidbody bulunamadı!"); }

        // Grabbable'ı pasif hale getir (takılıyken tutulmasını engelle)
        if (grabbableComponent != null)
        {
            grabbableComponent.enabled = false;
            Debug.Log("AttachMask: Grabbable kapatıldı.");
        }
        else { Debug.LogError("AttachMask: Grabbable komponenti boş!"); }

        // Ortam ışığını karart
        if (directionalLight != null)
        {
            directionalLight.color = attachedLightColor;
            directionalLight.intensity = attachedIntensity;
            Debug.Log("AttachMask: Işık karartıldı.");
        }
        else { Debug.LogError("AttachMask: Directional Light atanmamış!"); }

        attachCooldown = 0.5f; // Takma cooldown'ı başlat
        Debug.Log("AttachMask: Maske başarıyla takıldı.");
    }

    // YENİ METOT: Çıkarma için ön hazırlık yapar (görünür ve tutulabilir hale getirir)
    private void PrepareToDetach()
    {
        // Zaten hazırlanılıyorsa veya takılı değilse işlem yapma
        if (!isAttached || isPreparingToDetach) return;

        Debug.Log("PrepareToDetach: Maske çıkarma için ön hazırlık yapılıyor.");
        isPreparingToDetach = true;

        // Maskenin görselini tekrar aktif et
        if (maskModel != null)
        {
            maskModel.SetActive(true);
            Debug.Log("PrepareToDetach: Maske modeli görünür yapıldı.");
        }

        // Grabbable'ı tekrar aktif hale getir (elin tutabilmesi için)
        if (grabbableComponent != null)
        {
            grabbableComponent.enabled = true;
            Debug.Log("PrepareToDetach: Grabbable aktif edildi, maske tutulabilir.");
        }

        // Maske hala kinematic ve parent'lı kalacak, yere düşmeyecek.
        // Işık da hala karartılmış kalacak.
    }

    // YENİ METOT: Çıkarma ön hazırlığı durumunu sıfırlar
    private void ResetDetachPreparation()
    {
        if (!isPreparingToDetach) return;

        Debug.Log("ResetDetachPreparation: Çıırma ön hazırlığı sıfırlandı.");
        isPreparingToDetach = false;

        // Maskenin görselini tekrar gizle
        if (maskModel != null)
        {
            maskModel.SetActive(false);
            Debug.Log("ResetDetachPreparation: Maske modeli tekrar gizlendi.");
        }

        // Grabbable'ı tekrar pasif hale getir
        if (grabbableComponent != null)
        {
            grabbableComponent.enabled = false;
            Debug.Log("ResetDetachPreparation: Grabbable tekrar kapatıldı.");
        }
        // Işık ve Rigidbody durumu değişmez, hala takılı olduğu varsayılır.
    }


    // ESKİ DetachMask() metodunun yeni adı: Tam çıkarma işlemini yapar
    private void CompleteDetach()
    {
        Debug.Log("CompleteDetach: Maske tamamen çıkarılıyor.");

        isAttached = false; // Maske artık takılı değil
        isPreparingToDetach = false; // Ön hazırlık bitti

        // Maske kafa hedefinden ayrılır (parent kaldırılır)
        Transform originalParent = transform.parent; // Mevcut parent'ı kaydet
        // Transform.SetParent(Transform parent) metodunda ikinci argüman (worldPositionStays) varsayılan olarak true'dur.
        // Explicit olarak belirtmeye gerek yok, hata veriyorsa kaldırılır.
        transform.SetParent(null); // Parent'ı ayır
        Debug.Log($"CompleteDetach: Maske kafa hedefinden ayrıldı. Önceki parent: {(originalParent != null ? originalParent.name : "Yok")}");

        // Rigidbody yeniden fiziksel hale gelir
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.velocity = Vector3.zero; // Anlık hızları sıfırla
            rb.angularVelocity = Vector3.zero; // Anlık açısal hızları sıfırla
            Debug.Log("CompleteDetach: Rigidbody fiziksel hale getirildi.");
        }
        else { Debug.LogError("CompleteDetach: Rigidbody bulunamadı!"); }

        // Grabbable açık kalır (zaten el tarafından tutuluyor olabilir)
        if (grabbableComponent != null)
        {
            grabbableComponent.enabled = true;
            Debug.Log("CompleteDetach: Grabbable açık bırakıldı.");
        }
        else { Debug.LogError("CompleteDetach: Grabbable komponenti boş!"); }

        // Maske görseli açık kalır (elde tutulacak)
        if (maskModel != null)
        {
            maskModel.SetActive(true);
            Debug.Log("CompleteDetach: Maske modeli açık bırakıldı.");
        }
        else { Debug.LogError("CompleteDetach: Maske modeli atanmamış!"); }

        // Ortam ışığı eski haline döner
        if (directionalLight != null)
        {
            directionalLight.color = originalLightColor;
            directionalLight.intensity = originalIntensity;
            Debug.Log("CompleteDetach: Işık eski haline döndürüldü.");
        }
        else { Debug.LogError("CompleteDetach: Directional Light atanmamış!"); }

        // Cooldown başlat
        attachCooldown = 0.5f;
        Debug.Log("CompleteDetach: Maske çıkarma işlemi tamamlandı.");
    }
}