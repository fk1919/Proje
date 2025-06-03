using UnityEngine;
using Oculus.VR;
using Oculus.Interaction;

public class MaskAttachDetector : MonoBehaviour
{
    public GameObject maskModel;
    public string triggerTag = "MaskTrigger";
    public Transform headTarget;

    // Iþýk ayarlarý
    public Light directionalLight;
    public Color attachedLightColor = Color.gray;
    public float attachedIntensity = 0.95f;

    private Grabbable grabbableComponent;

    // VR Kontrolcü ayarlarý
    public OVRInput.Controller detachController = OVRInput.Controller.RHand;
    public OVRInput.Button detachButton = OVRInput.Button.PrimaryHandTrigger;

    private Color originalLightColor;
    private float originalIntensity;

    private bool isAttached = false;
    private float attachCooldown = 0f;

    private void Start()
    {
        // Yönsel ýþýk referansýný kontrol et ve orijinal deðerleri kaydet
        if (directionalLight != null)
        {
            originalLightColor = directionalLight.color;
            originalIntensity = directionalLight.intensity;
            Debug.Log($"MaskAttachDetector Baþladý. Orijinal Iþýk Renk: {originalLightColor}, Yoðunluk: {originalIntensity}");
        }
        else
        {
            Debug.LogError("Directional Light Inspector'da atanmamýþ!");
        }

        // Grabbable komponentini al
        grabbableComponent = GetComponent<Grabbable>();
        if (grabbableComponent == null)
        {
            Debug.LogError("MaskAttachDetector: Grabbable komponenti bulunamadý! Maske objenizde Grabbable scripti olduðundan emin olun.");
        }
        Debug.Log($"Trigger Tag: {triggerTag}, Head Target: {(headTarget != null ? headTarget.name : "Yok")}, Mask Model: {(maskModel != null ? maskModel.name : "Yok")}");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"OnTriggerEnter tetiklendi. Çarpan obje: {other.gameObject.name}, Tag: {other.tag}");

        // Zaten takýlýysa veya cooldown varsa iþlemi durdur
        if (isAttached || attachCooldown > 0f)
        {
            Debug.Log($"OnTriggerEnter engellendi: isAttached={isAttached}, attachCooldown={attachCooldown:F2}");
            return;
        }

        // Kafa tetikleyicisiyle çarpýþtýysa
        if (other.CompareTag(triggerTag))
        {
            Debug.Log("Maske takýlma tetikleyicisi bulundu! Takýlma iþlemi baþlýyor...");

            isAttached = true; // Maskeyi takýlý olarak iþaretle

            // Maskenin görselini gizle
            if (maskModel != null)
            {
                maskModel.SetActive(false);
                Debug.Log("Maske modeli gizlendi.");
            }
            else
            {
                Debug.LogError("Maske modeli (maskModel) Inspector'da atanmamýþ!");
            }

            // Maskeyi kafa objesine parent yap
            if (headTarget != null)
            {
                Transform originalParent = transform.parent; // Mevcut parent'ý kaydet
                transform.SetParent(headTarget, true); // Dünya pozisyonunu koruyarak parent yap
                Debug.Log($"Maske, {headTarget.name} objesine parent yapýldý. Önceki parent: {(originalParent != null ? originalParent.name : "Yok")}, Yeni parent: {transform.parent.name}");
            }
            else
            {
                Debug.LogError("Kafa hedefi (headTarget) Inspector'da atanmamýþ! Maske parent olamayacak.");
            }

            // Rigidbody'yi kinematic yapýp fiziði durdur
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                Debug.Log("Maske Rigidbody kinematic yapýldý ve fizik durduruldu.");
            }
            else
            {
                Debug.LogError("Maske üzerinde Rigidbody bulunamadý!");
            }

            // Grabbable'ý pasif hale getir (takýlýyken tutulmasýný engelle)
            if (grabbableComponent != null)
            {
                grabbableComponent.enabled = false;
                Debug.Log("Grabbable komponenti kapatýldý.");
            }
            else
            {
                Debug.LogError("Grabbable komponenti (grabbableComponent) referansý boþ!");
            }

            // Ortam ýþýðýný karart
            if (directionalLight != null)
            {
                directionalLight.color = attachedLightColor;
                directionalLight.intensity = attachedIntensity;
                Debug.Log($"Iþýk ayarlarý deðiþtirildi: Renk={attachedLightColor}, Yoðunluk={attachedIntensity}");
            }
            else
            {
                Debug.LogError("Directional Light Inspector'da atanmamýþ!");
            }

            Debug.Log("Maske takýlma iþlemi tamamlandý (isAttached=true).");
        }
        else
        {
            Debug.LogWarning($"Çarpan obje '{other.gameObject.name}', tag '{other.tag}' ama beklenen tag '{triggerTag}' deðil. Takýlma gerçekleþmedi.");
        }
    }

    private void Update()
    {
        // Cooldown süresini azalt
        if (attachCooldown > 0f)
        {
            attachCooldown -= Time.deltaTime;
            // Debug.Log($"Attach Cooldown: {attachCooldown:F2}"); // Bu log çok fazla çýkabilir, þimdilik yorumda býrakýyorum
        }


        // Maske takýlýysa, cooldown yoksa ve çýkarma butonu basýlý tutuluyorsa
        if (isAttached && attachCooldown <= 0f)
        {
            if (OVRInput.Get(detachButton, detachController))
            {
                Debug.Log($"Çýkarma butonu ({detachButton}) basýlý tutuluyor! Kontrolcü: {detachController}. Maskeyi çýkarmayý deneme koþulu saðlandý.");
                DetachMask(); // Maskeyi çýkar
            }
            // else { Debug.Log("Maske takýlý ama çýkarma butonu basýlý deðil."); } // Bu da çok fazla çýkabilir
        }
    }

    private void DetachMask()
    {
        Debug.Log("DetachMask çaðrýldý: Maske çýkarýlýyor.");

        // Maske hala bir parent'a baðlý mý kontrol et
        if (transform.parent != null)
        {
            Debug.Log($"Maske hala bir parent'a baðlý: {transform.parent.name}. Ayrýlýyor.");
        }
        else
        {
            Debug.Log("Maskenin zaten parent'ý yoktu.");
        }

        isAttached = false; // Maskeyi çýkarýlmýþ olarak iþaretle

        // Maskenin görselini tekrar aktif et
        if (maskModel != null)
        {
            maskModel.SetActive(true);
            Debug.Log("Maske modeli aktif edildi.");
        }
        else { Debug.LogError("Maske modeli (maskModel) Inspector'da atanmamýþ!"); }


        // Maskeyi parent'tan ayýr ve dünya pozisyon/rotasyonunu koru
        Vector3 worldPos = transform.position;
        Quaternion worldRot = transform.rotation;
        transform.SetParent(null);
        transform.position = worldPos;
        transform.rotation = worldRot;
        Debug.Log($"Maske parent'tan ayrýldý. Konum: {transform.position}, Rotasyon: {transform.rotation}");

        // Rigidbody'yi fiziksel hale getir ve yer çekimini etkinleþtir
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            Debug.Log("Maske Rigidbody kinematic'ten çýkarýldý ve fizik aktif edildi.");
        }
        else { Debug.LogError("Maske üzerinde Rigidbody bulunamadý!"); }


        // Ortam ýþýðýný orijinal haline getir
        if (directionalLight != null)
        {
            directionalLight.color = originalLightColor;
            directionalLight.intensity = originalIntensity;
            Debug.Log($"Iþýk ayarlarý orijinal haline getirildi: Renk={originalLightColor}, Yoðunluk={originalIntensity}");
        }
        else { Debug.LogError("Directional Light Inspector'da atanmamýþ!"); }


        // Grabbable'ý tekrar aktif hale getir (tekrar tutulabilir yap)
        if (grabbableComponent != null)
        {
            grabbableComponent.enabled = true;
            Debug.Log($"Grabbable aktif edildi. Kullanýcý butonu basýlý tutmaya devam ediyorsa maske eline yapýþacak.");
        }
        else { Debug.LogError("Grabbable komponenti (grabbableComponent) referansý boþ!"); }


        attachCooldown = 1f; // Tekrar takýlmayý engellemek için cooldown baþlat
        Debug.Log("DetachMask iþlemi tamamlandý. Cooldown baþlatýldý.");
    }
}