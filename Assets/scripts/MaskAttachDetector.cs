using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class MaskAttachDetector : MonoBehaviour
{
    public GameObject maskModel;         // Maske görseli
    public string triggerTag = "MaskTrigger";
    public Transform headTarget;         // CenterEyeAnchor vb.

    // -------------- Buraya eklemeler baþlýyor --------------
    [Header("Light Ayarlarý")]
    public Light directionalLight;       // Inspector’dan saðlanacak Directional Light referansý
    public Color attachedLightColor = Color.gray; // Maske takýldýðýnda kullanacaðýmýz renk
    public float attachedIntensity = 0.95f;        // Maske takýldýðýnda ýþýk yoðunluðu (düþük yaparak karartma)
    private Color originalLightColor;    // Baþlangýçtaki ýþýk rengini saklamak için
    private float originalIntensity;     // Baþlangýçtaki ýþýk yoðunluðunu saklamak için
    // -------------- Buraya eklemeler bitiyor --------------

    private XRGrabInteractable grabInteractable;
    private bool isAttached = false;

    private void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
        }

        // -------------- Burada Light referanslarýný kaydediyoruz --------------
        if (directionalLight != null)
        {
            originalLightColor = directionalLight.color;
            originalIntensity = directionalLight.intensity;
        }
        else
        {
            Debug.LogWarning("MaskAttachDetector: Inspector'dan 'directionalLight' atanmamýþ!");
        }
        // -----------------------------------------------------------------------
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isAttached) return;

        if (other.CompareTag(triggerTag))
        {
            Debug.Log("Maske kafaya takýldý!");

            isAttached = true;

            // Görseli gizle
            if (maskModel != null)
                maskModel.SetActive(false);

            // Kafaya yapýþtýr
            if (headTarget != null)
                transform.SetParent(headTarget, true); // worldPosition korunsun

            // -------------- Burada ýþýk ayarlarýný deðiþtiriyoruz --------------
            if (directionalLight != null)
            {
                directionalLight.color = attachedLightColor;
                directionalLight.intensity = attachedIntensity;
            }
            // -------------------------------------------------------------------
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        if (isAttached)
        {
            Debug.Log("Maske çýkarýldý!");

            isAttached = false;

            // Görseli geri getir
            if (maskModel != null)
                maskModel.SetActive(true);

            // Kafadan ayýr
            transform.SetParent(null);

            // -------------- Iþýðý eski haline döndürüyoruz --------------
            if (directionalLight != null)
            {
                directionalLight.color = originalLightColor;
                directionalLight.intensity = originalIntensity;
            }
            // -------------------------------------------------------------
        }
    }
}


