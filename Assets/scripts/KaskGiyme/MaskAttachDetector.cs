using UnityEngine;
using Oculus.VR;
using Oculus.Interaction; // Grabbable sınıfı için

public class MaskAttachDetector : MonoBehaviour
{
    [Header("Maske ve Kafa Ayarları")]
    public GameObject maskModel; // Maskenin görsel modelini içeren GameObject
    public Transform headTarget; // Kaskın takıldığı yer (kafa objesinin Transform'u)

    [Header("Mesafeye Göre Otomatik Takma")]
    [Tooltip("Maskeyi takmak için headTarget'e ne kadar yakın olması gerektiği (metre).")]
    public float attachProximityThreshold = 0.20f; // Maskeyi takmak için yakınlık mesafesi

    [Header("Işık Ayarları")]
    public Light directionalLight; // Sahnedeki yönsel ışık
    public Color attachedLightColor = Color.gray; // Takılıyken ışık rengi
    public float attachedIntensity = 0.95f; // Takılıyken ışık yoğunluğu

    [Header("VR Kontrolcü ve Çıkarma Ayarları")]
    public OVRInput.Controller detachController = OVRInput.Controller.RHand; // Hangi elin çıkaracağını belirle
    public OVRInput.Button detachButton = OVRInput.Button.PrimaryHandTrigger; // Hangi tuşla çıkarılacak (Grip tuşu)

    [Header("Interactor Referansları")]
    // Inspector'dan HandGrabInteractor veya TouchHandGrabInteractor gibi scriptlerin bağlı olduğu GameObject'leri atayın.
    // Bu GameObject'lerin üzerinde Box Collider (Is Trigger = true) ve "HandCollider" tag'i olmalı.
    public GameObject leftHandInteractorGameObject; // Sol el Interactor'ın GameObject'i
    public GameObject rightHandInteractorGameObject; // Sağ el Interactor'ın GameObject'i
    public string handColliderTag = "HandCollider"; // El kontrolcülerine vereceğimiz Tag

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
            Debug.LogError("MaskAttachDetector: Yönsel Işık Inspector'da atanmamış!");
        }

        // Grabbable komponentini al
        grabbableComponent = GetComponent<Grabbable>();
        if (grabbableComponent == null)
        {
            Debug.LogError("MaskAttachDetector: Grabbable komponenti bulunamadı! Maske objenizde Grabbable scripti olduğundan emin olun.");
        }

        // Interactor GameObject referanslarının atanıp atanmadığını kontrol et
        if (leftHandInteractorGameObject == null || rightHandInteractorGameObject == null)
        {
            Debug.LogWarning("MaskAttachDetector: Interactor GameObject referansları atanmadı. El ile çıkarma çalışmayabilir.");
        }
        Debug.Log($"MaskAttachDetector: Kafa Hedefi: {(headTarget != null ? headTarget.name : "Yok")}, Maske Modeli: {(maskModel != null ? maskModel.name : "Yok")}, El Collider Etiketi: {handColliderTag}");
    }

    private void Update()
    {
        // Cooldown süresini azalt
        if (attachCooldown > 0f)
        {
            attachCooldown -= Time.deltaTime;
        }

        // Maske takılı değilse ve cooldown yoksa ve kafa hedefi atanmışsa
        // Maskeyi takmak için mesafeyi kontrol et (burası değişmedi)
        if (!isAttached && attachCooldown <= 0f && headTarget != null)
        {
            float distanceToHead = Vector3.Distance(transform.position, headTarget.position);
            if (distanceToHead <= attachProximityThreshold)
            {
                AttachMask(); // Maskeyi tak
            }
        }

        // Maske takılıysa VE çıkarma için ön hazırlık yapılıyorsa VE maske gerçekten tutulduysa
        if (isAttached && isPreparingToDetach && grabbableComponent != null && grabbableComponent.SelectingPointsCount > 0)
        {
            Debug.Log("Update: Maske ön hazırlık durumunda ve Interactor tarafından tutuldu. Tam çıkarma işlemi başlatılıyor.");
            CompleteDetach(); // Tam çıkarma işlemini yap
        }
        if (!isAttached && directionalLight != null &&
    (directionalLight.color != originalLightColor || directionalLight.intensity != originalIntensity))
        {
            directionalLight.color = originalLightColor;
            directionalLight.intensity = originalIntensity;
            Debug.Log("Update: Maske takılı değil, ışık eski haline döndürüldü.");
        }
    }

    // El collider'ı maskeye temas ettiğinde (Trigger olarak ayarlı)
    private void OnTriggerStay(Collider other)
    {
        // Maske takılı DEĞİLSE veya cooldown varsa veya zaten hazırlanılıyorsa işlem yapma
        // Önemli Düzeltme: Maske takılıyken tetiklenmeli, bu yüzden !isAttached kontrolü KALDIRILDI
        if (attachCooldown > 0f || isPreparingToDetach) return;

        // Maske takılı değilse ve bu metod tetiklenirse, ilgilenmiyoruz. Sadece takılıykenki çıkarmayla ilgiliyiz.
        if (!isAttached) return;

        // Çarpan obje elin collider'ı mı?
        if (other.CompareTag(handColliderTag))
        {
            // Hangi elin temas ettiğini bul (Temas eden collider'ın transform'unu kullanacağız)
            Transform contactingHandTransform = other.transform;

            // Doğru kontrolcünün grip tuşuna basılı mı?
            bool isDetachButtonPressed = false;
            // Sağ elin kontrolcüsü ve el collider'ı eşleşiyorsa (ElTrigger, RightController'ın çocuğu ise parent'ını kontrol et)
            if (detachController == OVRInput.Controller.RHand && rightHandInteractorGameObject != null && contactingHandTransform.parent == rightHandInteractorGameObject.transform)
            {
                isDetachButtonPressed = OVRInput.Get(detachButton, OVRInput.Controller.RTouch);
            }
            // Sol elin kontrolcüsü ve el collider'ı eşleşiyorsa (ElTrigger, LeftController'ın çocuğu ise parent'ını kontrol et)
            else if (detachController == OVRInput.Controller.LHand && leftHandInteractorGameObject != null && contactingHandTransform.parent == leftHandInteractorGameObject.transform)
            {
                isDetachButtonPressed = OVRInput.Get(detachButton, OVRInput.Controller.LTouch);
            }

            if (isAttached)
            {
                if (maskModel != null && !maskModel.activeSelf)
                {
                    maskModel.SetActive(true);
                    Debug.Log("El maskeye temas etti, maske modeli görünür yapıldı.");
                    transform.SetParent(null); // Kafadan ayır
                    Rigidbody rb = GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.isKinematic = false;
                        rb.useGravity = true;
                    }
                    isAttached = false;
                }

                // Butona basılmışsa çıkarma başlat
                if (isDetachButtonPressed)
                {
                    Debug.Log($"OnTriggerStay: El ({other.gameObject.name}) maskeye temas ediyor ve çıkarma butonu ({detachButton}) basılı. PrepareToDetach çağrılıyor.");
                    PrepareToDetach(); // Çıkarma ön hazırlığını yap
                }
            }
            else
            {
                // Eğer el temas ediyorsa ama buton basılı değilse, ve ön hazırlık yapılıyorsa durumu sıfırla
                if (isPreparingToDetach)
                {
                    Debug.Log("OnTriggerStay: Buton basılı değilken el temasta, ön hazırlık sıfırlanıyor.");
                    ResetDetachPreparation();
                }
            }
        }
        else
        {
            // Eğer temas eden obje el collider'ı değilse ve ön hazırlık yapılıyorsa (yanlışlıkla tetiklendiyse), durumu sıfırla
            if (isPreparingToDetach)
            {
                Debug.LogWarning($"OnTriggerStay: Temas eden obje '{other.gameObject.name}' el collider'ı değil. Ön hazırlık sıfırlanıyor.");
                ResetDetachPreparation();
            }
        }
    }

    // El collider'ı maskeden ayrıldığında (buton basılı olmasa bile)
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(handColliderTag))
        {
            Debug.Log($"OnTriggerExit: El ({other.gameObject.name}) maskeden ayrıldı.");
            if (isPreparingToDetach)
            {
                // Eğer el temasını kaybederse, ön hazırlık durumunu sıfırla
                Debug.Log("OnTriggerExit: El teması kesildi, ön hazırlık sıfırlanıyor.");
                ResetDetachPreparation();
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

    // Çıkarma için ön hazırlık yapar (görünür ve tutulabilir hale getirir)
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

    // Çıkarma ön hazırlığı durumunu sıfırlar
    private void ResetDetachPreparation()
    {
        if (!isPreparingToDetach) return;

        Debug.Log("ResetDetachPreparation: Çıkarma ön hazırlığı sıfırlandı.");
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


    // Tam çıkarma işlemini yapar
    private void CompleteDetach()
    {
        Debug.Log("CompleteDetach: Maske tamamen çıkarılıyor.");

        isAttached = false; // Maske artık takılı değil
        isPreparingToDetach = false; // Ön hazırlık bitti

        // Maske kafa hedefinden ayrılır (parent kaldırılır)
        Transform originalParent = transform.parent; // Mevcut parent'ı kaydet
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
