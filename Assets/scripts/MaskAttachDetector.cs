using UnityEngine;
using Oculus.VR;
using Oculus.Interaction;

public class MaskAttachDetector : MonoBehaviour
{
    public GameObject maskModel;
    public string triggerTag = "MaskTrigger";
    public Transform headTarget;

    // I��k ayarlar�
    public Light directionalLight;
    public Color attachedLightColor = Color.gray;
    public float attachedIntensity = 0.95f;

    private Grabbable grabbableComponent;

    // VR Kontrolc� ayarlar�
    public OVRInput.Controller detachController = OVRInput.Controller.RHand;
    public OVRInput.Button detachButton = OVRInput.Button.PrimaryHandTrigger;

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
            // Debug.Log($"Attach Cooldown: {attachCooldown:F2}"); // Bu log �ok fazla ��kabilir, �imdilik yorumda b�rak�yorum
        }


        // Maske tak�l�ysa, cooldown yoksa ve ��karma butonu bas�l� tutuluyorsa
        if (isAttached && attachCooldown <= 0f)
        {
            if (OVRInput.Get(detachButton, detachController))
            {
                Debug.Log($"��karma butonu ({detachButton}) bas�l� tutuluyor! Kontrolc�: {detachController}. Maskeyi ��karmay� deneme ko�ulu sa�land�.");
                DetachMask(); // Maskeyi ��kar
            }
            // else { Debug.Log("Maske tak�l� ama ��karma butonu bas�l� de�il."); } // Bu da �ok fazla ��kabilir
        }
    }

    private void DetachMask()
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
        transform.SetParent(null);
        transform.position = worldPos;
        transform.rotation = worldRot;
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
            Debug.Log($"Grabbable aktif edildi. Kullan�c� butonu bas�l� tutmaya devam ediyorsa maske eline yap��acak.");
        }
        else { Debug.LogError("Grabbable komponenti (grabbableComponent) referans� bo�!"); }


        attachCooldown = 1f; // Tekrar tak�lmay� engellemek i�in cooldown ba�lat
        Debug.Log("DetachMask i�lemi tamamland�. Cooldown ba�lat�ld�.");
    }
}