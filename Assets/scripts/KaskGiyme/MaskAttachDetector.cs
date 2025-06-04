using UnityEngine;
using Oculus.VR;
using Oculus.Interaction; // Grabbable i�in (IInteractor'� do�rudan kullanmayaca��z)

public class MaskAttachDetector : MonoBehaviour
{
    public GameObject maskModel; // Maskenin g�rsel modelini i�eren GameObject
    public string triggerTag = "MaskTrigger"; // Kafa collider'�n�n tag'i
    public Transform headTarget; // Kask�n tak�ld��� yer (kafa collider'�)

    // I��k ayarlar�
    public Light directionalLight;
    public Color attachedLightColor = Color.gray;
    public float attachedIntensity = 0.95f;

    private Grabbable grabbableComponent;

    // VR Kontrolc� ayarlar�
    public OVRInput.Controller detachController = OVRInput.Controller.RHand; // Hangi elin ��karaca��n� belirle
    public OVRInput.Button detachButton = OVRInput.Button.PrimaryHandTrigger; // Hangi tu�la ��kar�lacak

    // YEN�: Hangi Interactor'�n maskeyi ��karaca��n� kontrol etmek ve tutmak i�in
    [Header("Interactor Referanslar�")]
    // MonoBehaviour olarak tan�ml�yoruz. Inspector'dan HandGrabInteractor veya TouchHandGrabInteractor gibi objeleri atay�n.
    // Bu objelerin �zerinde Oculus.Interaction.IInteractor implement eden bir script olmal�, ancak kodda do�rudan IInteractor'� kullanmayaca��z.
    public MonoBehaviour leftHandInteractorObject; // Sol el Interactor objesi (Inspector'dan ata)
    public MonoBehaviour rightHandInteractorObject; // Sa� el Interactor objesi (Inspector'dan ata)
    public float detachProximityThreshold = 0.2f; // ��karma i�in elin maskeye ne kadar yak�n olmas� gerekti�i (metre)

    private Color originalLightColor;
    private float originalIntensity;

    private bool isAttached = false;
    private float attachCooldown = 0f;

    private void Start()
    {
        // Y�nsel ���k referans�n� kontrol et ve orijinal de�erleri kaydet
        if (directionalLight != null)
        {
            originalLightColor = directionalLight.color;
            originalIntensity = directionalLight.intensity;
            Debug.Log($"MaskAttachDetector Ba�lad�. Orijinal I��k Renk: {originalLightColor}, Yo�unluk: {originalIntensity}");
        }
        else
        {
            Debug.LogError("Directional Light Inspector'da atanmam��!");
        }

        // Grabbable komponentini al
        grabbableComponent = GetComponent<Grabbable>();
        if (grabbableComponent == null)
        {
            Debug.LogError("MaskAttachDetector: Grabbable komponenti bulunamad�! Maske objenizde Grabbable scripti oldu�undan emin olun.");
        }
        Debug.Log($"Trigger Tag: {triggerTag}, Head Target: {(headTarget != null ? headTarget.name : "Yok")}, Mask Model: {(maskModel != null ? maskModel.name : "Yok")}");

        // Interactor referanslar�n�n atan�p atanmad���n� kontrol et
        if (leftHandInteractorObject == null || rightHandInteractorObject == null)
        {
            Debug.LogWarning("LeftHandInteractorObject veya RightHandInteractorObject Inspector'da atanmam��. ��karma i�lemi i�in el yak�nl�k kontrol� �al��mayabilir.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"OnTriggerEnter tetiklendi. �arpan obje: {other.gameObject.name}, Tag: {other.tag}");

        // Zaten tak�l�ysa veya cooldown varsa i�lemi durdur
        if (isAttached || attachCooldown > 0f)
        {
            Debug.Log($"OnTriggerEnter engellendi: isAttached={isAttached}, attachCooldown={attachCooldown:F2}");
            return;
        }

        // Kafa tetikleyicisiyle �arp��t�ysa
        if (other.CompareTag(triggerTag))
        {
            Debug.Log("Maske tak�lma tetikleyicisi bulundu! Tak�lma i�lemi ba�l�yor...");

            isAttached = true; // Maskeyi tak�l� olarak i�aretle

            // Maskenin g�rselini gizle
            if (maskModel != null)
            {
                maskModel.SetActive(false);
                Debug.Log("Maske modeli gizlendi.");
            }
            else
            {
                Debug.LogError("Maske modeli (maskModel) Inspector'da atanmam��!");
            }

            // Maskeyi kafa objesine parent yap
            if (headTarget != null)
            {
                Transform originalParent = transform.parent; // Mevcut parent'� kaydet
                transform.SetParent(headTarget, true); // D�nya pozisyonunu koruyarak parent yap
                Debug.Log($"Maske, {headTarget.name} objesine parent yap�ld�. �nceki parent: {(originalParent != null ? originalParent.name : "Yok")}, Yeni parent: {transform.parent.name}");
            }
            else
            {
                Debug.LogError("Kafa hedefi (headTarget) Inspector'da atanmam��! Maske parent olamayacak.");
            }

            // Rigidbody'yi kinematic yap�p fizi�i durdur
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                Debug.Log("Maske Rigidbody kinematic yap�ld� ve fizik durduruldu.");
            }
            else
            {
                Debug.LogError("Maske �zerinde Rigidbody bulunamad�!");
            }

            // Grabbable'� pasif hale getir (tak�l�yken tutulmas�n� engelle)
            if (grabbableComponent != null)
            {
                grabbableComponent.enabled = false;
                Debug.Log("Grabbable komponenti kapat�ld�.");
            }
            else
            {
                Debug.LogError("Grabbable komponenti (grabbableComponent) referans� bo�!");
            }

            // Ortam �����n� karart
            if (directionalLight != null)
            {
                directionalLight.color = attachedLightColor;
                directionalLight.intensity = attachedIntensity;
                Debug.Log($"I��k ayarlar� de�i�tirildi: Renk={attachedLightColor}, Yo�unluk={attachedIntensity}");
            }
            else
            {
                Debug.LogError("Directional Light Inspector'da atanmam��!");
            }

            Debug.Log("Maske tak�lma i�lemi tamamland� (isAttached=true).");
        }
        else
        {
            Debug.LogWarning($"�arpan obje '{other.gameObject.name}', tag '{other.tag}' ama beklenen tag '{triggerTag}' de�il. Tak�lma ger�ekle�medi.");
        }
    }

    private void Update()
    {
        // Cooldown s�resini azalt
        if (attachCooldown > 0f)
        {
            attachCooldown -= Time.deltaTime;
        }

        // Maske tak�l�ysa, cooldown yoksa ve ��karma butonu bas�l� tutuluyorsa
        if (isAttached && attachCooldown <= 0f)
        {
            if (OVRInput.Get(detachButton, detachController))
            {
                Debug.Log($"��karma butonu ({detachButton}) bas�l� tutuluyor! Kontrolc�: {detachController}.");

                // YEN� KONTROL: El maskeye yeterince yak�n m�?
                Transform activeInteractorTransform = null; // Interactor'�n Transform'unu tutaca��z
                if (detachController == OVRInput.Controller.RHand && rightHandInteractorObject != null)
                {
                    activeInteractorTransform = rightHandInteractorObject.transform; // MonoBehaviour.transform
                }
                else if (detachController == OVRInput.Controller.LHand && leftHandInteractorObject != null)
                {
                    activeInteractorTransform = leftHandInteractorObject.transform; // MonoBehaviour.transform
                }

                if (activeInteractorTransform != null)
                {
                    // Maskenin �u anki d�nya pozisyonu (kafan�n �zerinde)
                    Vector3 maskCurrentWorldPos = transform.position;
                    // Interactor'�n (elin) d�nya pozisyonu
                    Vector3 interactorWorldPos = activeInteractorTransform.position; // Sadece .position

                    float distance = Vector3.Distance(maskCurrentWorldPos, interactorWorldPos);
                    Debug.Log($"El ile maske aras� mesafe: {distance:F2}m. E�ik: {detachProximityThreshold:F2}m.");

                    if (distance <= detachProximityThreshold)
                    {
                        Debug.Log("El maskeye yeterince yak�n. Maskeyi ��karmay� deneme ko�ulu sa�land�.");
                        // DetachMask metoduna hangi interactor'�n transformunu g�nderece�imizi belirtelim
                        DetachMask(activeInteractorTransform);
                    }
                    else
                    {
                        Debug.Log("El maskeye yeterince yak�n de�il. ��karma i�lemi tetiklenmedi.");
                    }
                }
                else
                {
                    Debug.LogWarning("Detach i�lemi i�in atanm�� Interactor Object bulunamad� veya yanl�� kontrolc� se�ildi.");
                }
            }
        }
    }

    // YEN�: DetachMask metodu Interactor Transform'u arg�man alacak
    private void DetachMask(Transform interactorTransformToGrabWith)
    {
        Debug.Log("DetachMask �a�r�ld�: Maske ��kar�l�yor.");

        // Maske hala bir parent'a ba�l� m� kontrol et
        if (transform.parent != null)
        {
            Debug.Log($"Maske hala bir parent'a ba�l�: {transform.parent.name}. Ayr�l�yor.");
        }
        else
        {
            Debug.Log("Maskenin zaten parent'� yoktu.");
        }

        isAttached = false; // Maskeyi ��kar�lm�� olarak i�aretle

        // Maskenin g�rselini tekrar aktif et
        if (maskModel != null)
        {
            maskModel.SetActive(true);
            Debug.Log("Maske modeli aktif edildi.");
        }
        else { Debug.LogError("Maske modeli (maskModel) Inspector'da atanmam��!"); }


        // Maskeyi parent'tan ay�r ve d�nya pozisyon/rotasyonunu koru
        Vector3 worldPos = transform.position;
        Quaternion worldRot = transform.rotation;
        transform.SetParent(null); // Parent'� ay�r
        transform.position = worldPos; // D�nya pozisyonunu koru
        transform.rotation = worldRot; // D�nya rotasyonunu koru
        Debug.Log($"Maske parent'tan ayr�ld�. Konum: {transform.position}, Rotasyon: {transform.rotation}");

        // Rigidbody'yi fiziksel hale getir ve yer �ekimini etkinle�tir
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            Debug.Log("Maske Rigidbody kinematic'ten ��kar�ld� ve fizik aktif edildi.");
        }
        else { Debug.LogError("Maske �zerinde Rigidbody bulunamad�!"); }


        // Ortam �����n� orijinal haline getir
        if (directionalLight != null)
        {
            directionalLight.color = originalLightColor;
            directionalLight.intensity = originalIntensity;
            Debug.Log($"I��k ayarlar� orijinal haline getirildi: Renk={originalLightColor}, Yo�unluk={originalIntensity}");
        }
        else { Debug.LogError("Directional Light Inspector'da atanmam��!"); }


        // Grabbable'� tekrar aktif hale getir (tekrar tutulabilir yap)
        if (grabbableComponent != null)
        {
            grabbableComponent.enabled = true;
            Debug.Log($"Grabbable aktif edildi.");

            // YEN�: Maskeyi ��karan elin transformunu kullanarak bir yakalama denemesi
            // IInteractor'�n Transform ve CanSelect �yelerine do�rudan eri�emedi�imiz i�in,
            // burada sadece basit bir fiziksel yakalama davran��� beklenir.
            if (interactorTransformToGrabWith != null)
            {
                // Art�k IInteractor'� direkt olarak kullanmayaca��z.
                // Sadece Debug.Log i�in Interactor objesinin ad�n� al�yoruz.
                Debug.Log($"Maske {interactorTransformToGrabWith.gameObject.name} objesi taraf�ndan se�ilmeye �al���ld� (otomatik yakalama bekleniyor).");
            }
            else
            {
                Debug.LogWarning("Maske ��kar�ld� ancak Interactor Transform referans� null. Kullan�c�n�n maskeyi tekrar tutmas� gerekebilir.");
            }
        }
        else { Debug.LogError("Grabbable komponenti (grabbableComponent) referans� bo�!"); }


        attachCooldown = 1f; // Tekrar tak�lmay� engellemek i�in cooldown ba�lat
        Debug.Log("DetachMask i�lemi tamamland�. Cooldown ba�lat�ld�.");
    }
}