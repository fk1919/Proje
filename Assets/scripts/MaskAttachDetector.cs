using UnityEngine;
using Oculus.VR; // OVRInput için bu kalsýn (hata vermiyorsa sorun yok)
using Oculus;
using Oculus.Interaction; // <<< YENÝ EKLENECEK SATIR: Grabbable sýnýfý için


public class MaskAttachDetector : MonoBehaviour
{
    public GameObject maskModel;
    public string triggerTag = "MaskTrigger";
    public Transform headTarget;

    [Header("Light Ayarlarý")]
    public Light directionalLight;
    public Color attachedLightColor = Color.gray;
    public float attachedIntensity = 0.95f;

    // Grabbable scriptine referans
    // Artýk GrabbableScriptAdý yerine direkt Grabbable yazýyoruz, çünkü using Oculus.Interaction; ekledik.
    private Grabbable grabbableComponent;

    // OVRGrabber referanslarýný tamamen kaldýrýyoruz, ihtiyacýmýz yok.
    // public OVRGrabber leftHandGrabber;
    // public OVRGrabber rightHandGrabber;

    // Hangi elin çýkarma iþlemi yapacaðýný belirleyelim
    public OVRInput.Controller detachController = OVRInput.Controller.RHand; // Varsayýlan: Sað El
    public OVRInput.Button detachButton = OVRInput.Button.PrimaryHandTrigger; // Varsayýlan: Grip tuþu

    private Color originalLightColor;
    private float originalIntensity;

    private bool isAttached = false;
    private float attachCooldown = 0f;

    private void Start()
    {
        if (directionalLight != null)
        {
            originalLightColor = directionalLight.color;
            originalIntensity = directionalLight.intensity;
        }

        // Grabbable komponentini al
        grabbableComponent = GetComponent<Grabbable>(); // Artýk kýrmýzý yanmayacak!
        if (grabbableComponent == null)
        {
            Debug.LogError("MaskAttachDetector: Grabbable komponenti bulunamadý! Maske objenizde Grabbable scripti olduðundan emin olun.");
        }
        // OVRGrabber referans kontrolü artýk yok.
    }

    private void OnTriggerEnter(Collider other)
    {
        // Debug mesajlarý ekliyoruz
        Debug.Log($"OnTriggerEnter tetiklendi. Çarpan obje: {other.gameObject.name}, Tag: {other.tag}");

        if (isAttached || attachCooldown > 0f) return;

        if (other.CompareTag(triggerTag))
        {
            Debug.Log("Maske takýlma tetikleyicisi bulundu!");

            isAttached = true;

            // 1. Maskenin görselini gizle
            if (maskModel != null)
            {
                maskModel.SetActive(false);
                Debug.Log("Maske modeli gizlendi.");
            }
            else
            {
                Debug.LogError("Maske modeli (maskModel) Inspector'da atanmamýþ!");
            }

            // 2. Kaský kafaya parent yap
            if (headTarget != null)
            {
                transform.SetParent(headTarget, true);
                Debug.Log($"Maske, {headTarget.name} objesine parent yapýldý. Yeni parent: {transform.parent.name}");
            }
            else
            {
                Debug.LogError("Kafa hedefi (headTarget) Inspector'da atanmamýþ! Maske parent olamayacak.");
            }

            // 3. Rigidbody ayarlarý
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true; // Fiziksel etkileþimi durdur
                rb.useGravity = false;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                Debug.Log("Maske Rigidbody kinematic yapýldý ve fizik durduruldu.");
            }
            else
            {
                Debug.LogError("Maske üzerinde Rigidbody bulunamadý!");
            }

            // 4. Grabbable'ý pasif hale getir (tekrar tutulmasýný engellemek için)
            if (grabbableComponent != null)
            {
                grabbableComponent.enabled = false;
                Debug.Log("Grabbable komponenti kapatýldý.");
            }
            else
            {
                Debug.LogError("Grabbable komponenti (grabbableComponent) referansý boþ!");
            }

            // ... (ýþýk ayarlarý) ...
        }
        else
        {
            Debug.Log($"Çarpan obje '{other.gameObject.name}', tag '{other.tag}' ama beklenen tag '{triggerTag}' deðil.");
        }
    }


    private void Update()
    {
        if (attachCooldown > 0f)
            attachCooldown -= Time.deltaTime;

        // Maske takýlýysa VE çýkarma butonu basýlý tutuluyorsa VE cooldown yoksa
        if (isAttached && attachCooldown <= 0f)
        {
            if (OVRInput.Get(detachButton, detachController))
            {
                Debug.Log($"Çýkarma butonu ({detachButton}) basýlý tutuluyor. Maskeyi çýkarmayý dene.");
                DetachMask(); // Artýk hiçbir argüman göndermeye gerek yok
            }
        }
    }

    // DetachMask metodunu güncelle, artýk argüman almayacak
    private void DetachMask()
    {
        Debug.Log("Maske çýkarýlýyor");

        isAttached = false;

        if (maskModel != null)
            maskModel.SetActive(true);

        Vector3 worldPos = transform.position;
        Quaternion worldRot = transform.rotation;

        transform.SetParent(null);
        transform.position = worldPos;
        transform.rotation = worldRot;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (directionalLight != null)
        {
            directionalLight.color = originalLightColor;
            directionalLight.intensity = originalIntensity;
        }

        // Grabbable'ý tekrar aktif hale getir
        if (grabbableComponent != null)
        {
            grabbableComponent.enabled = true;
            // OVR Interaction SDK'sýnýn mantýðýna göre,
            // Grabbable aktif hale geldiðinde ve kullanýcý o sýrada butonu basýlý tutmaya devam ediyorsa,
            // Interaction SDK otomatik olarak grab iþlemini tekrar baþlatmalýdýr.
            Debug.Log($"Grabbable aktif edildi. Kullanýcý butonu basýlý tutmaya devam ediyorsa maske eline yapýþacak.");
        }

        attachCooldown = 1f;
    }
}