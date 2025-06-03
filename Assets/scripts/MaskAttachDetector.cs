using UnityEngine;
using Oculus.VR; // OVRInput i�in bu kals�n (hata vermiyorsa sorun yok)
using Oculus;
using Oculus.Interaction; // <<< YEN� EKLENECEK SATIR: Grabbable s�n�f� i�in


public class MaskAttachDetector : MonoBehaviour
{
    public GameObject maskModel;
    public string triggerTag = "MaskTrigger";
    public Transform headTarget;

    [Header("Light Ayarlar�")]
    public Light directionalLight;
    public Color attachedLightColor = Color.gray;
    public float attachedIntensity = 0.95f;

    // Grabbable scriptine referans
    // Art�k GrabbableScriptAd� yerine direkt Grabbable yaz�yoruz, ��nk� using Oculus.Interaction; ekledik.
    private Grabbable grabbableComponent;

    // OVRGrabber referanslar�n� tamamen kald�r�yoruz, ihtiyac�m�z yok.
    // public OVRGrabber leftHandGrabber;
    // public OVRGrabber rightHandGrabber;

    // Hangi elin ��karma i�lemi yapaca��n� belirleyelim
    public OVRInput.Controller detachController = OVRInput.Controller.RHand; // Varsay�lan: Sa� El
    public OVRInput.Button detachButton = OVRInput.Button.PrimaryHandTrigger; // Varsay�lan: Grip tu�u

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
        grabbableComponent = GetComponent<Grabbable>(); // Art�k k�rm�z� yanmayacak!
        if (grabbableComponent == null)
        {
            Debug.LogError("MaskAttachDetector: Grabbable komponenti bulunamad�! Maske objenizde Grabbable scripti oldu�undan emin olun.");
        }
        // OVRGrabber referans kontrol� art�k yok.
    }

    private void OnTriggerEnter(Collider other)
    {
        // Debug mesajlar� ekliyoruz
        Debug.Log($"OnTriggerEnter tetiklendi. �arpan obje: {other.gameObject.name}, Tag: {other.tag}");

        if (isAttached || attachCooldown > 0f) return;

        if (other.CompareTag(triggerTag))
        {
            Debug.Log("Maske tak�lma tetikleyicisi bulundu!");

            isAttached = true;

            // 1. Maskenin g�rselini gizle
            if (maskModel != null)
            {
                maskModel.SetActive(false);
                Debug.Log("Maske modeli gizlendi.");
            }
            else
            {
                Debug.LogError("Maske modeli (maskModel) Inspector'da atanmam��!");
            }

            // 2. Kask� kafaya parent yap
            if (headTarget != null)
            {
                transform.SetParent(headTarget, true);
                Debug.Log($"Maske, {headTarget.name} objesine parent yap�ld�. Yeni parent: {transform.parent.name}");
            }
            else
            {
                Debug.LogError("Kafa hedefi (headTarget) Inspector'da atanmam��! Maske parent olamayacak.");
            }

            // 3. Rigidbody ayarlar�
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true; // Fiziksel etkile�imi durdur
                rb.useGravity = false;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                Debug.Log("Maske Rigidbody kinematic yap�ld� ve fizik durduruldu.");
            }
            else
            {
                Debug.LogError("Maske �zerinde Rigidbody bulunamad�!");
            }

            // 4. Grabbable'� pasif hale getir (tekrar tutulmas�n� engellemek i�in)
            if (grabbableComponent != null)
            {
                grabbableComponent.enabled = false;
                Debug.Log("Grabbable komponenti kapat�ld�.");
            }
            else
            {
                Debug.LogError("Grabbable komponenti (grabbableComponent) referans� bo�!");
            }

            // ... (���k ayarlar�) ...
        }
        else
        {
            Debug.Log($"�arpan obje '{other.gameObject.name}', tag '{other.tag}' ama beklenen tag '{triggerTag}' de�il.");
        }
    }


    private void Update()
    {
        if (attachCooldown > 0f)
            attachCooldown -= Time.deltaTime;

        // Maske tak�l�ysa VE ��karma butonu bas�l� tutuluyorsa VE cooldown yoksa
        if (isAttached && attachCooldown <= 0f)
        {
            if (OVRInput.Get(detachButton, detachController))
            {
                Debug.Log($"��karma butonu ({detachButton}) bas�l� tutuluyor. Maskeyi ��karmay� dene.");
                DetachMask(); // Art�k hi�bir arg�man g�ndermeye gerek yok
            }
        }
    }

    // DetachMask metodunu g�ncelle, art�k arg�man almayacak
    private void DetachMask()
    {
        Debug.Log("Maske ��kar�l�yor");

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

        // Grabbable'� tekrar aktif hale getir
        if (grabbableComponent != null)
        {
            grabbableComponent.enabled = true;
            // OVR Interaction SDK's�n�n mant���na g�re,
            // Grabbable aktif hale geldi�inde ve kullan�c� o s�rada butonu bas�l� tutmaya devam ediyorsa,
            // Interaction SDK otomatik olarak grab i�lemini tekrar ba�latmal�d�r.
            Debug.Log($"Grabbable aktif edildi. Kullan�c� butonu bas�l� tutmaya devam ediyorsa maske eline yap��acak.");
        }

        attachCooldown = 1f;
    }
}