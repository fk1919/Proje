using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class MaskAttachDetector : MonoBehaviour
{
    public GameObject maskModel;         // Maske g�rseli
    public string triggerTag = "MaskTrigger";
    public Transform headTarget;         // CenterEyeAnchor vb.

    // -------------- Buraya eklemeler ba�l�yor --------------
    [Header("Light Ayarlar�")]
    public Light directionalLight;       // Inspector�dan sa�lanacak Directional Light referans�
    public Color attachedLightColor = Color.gray; // Maske tak�ld���nda kullanaca��m�z renk
    public float attachedIntensity = 0.95f;        // Maske tak�ld���nda ���k yo�unlu�u (d���k yaparak karartma)
    private Color originalLightColor;    // Ba�lang��taki ���k rengini saklamak i�in
    private float originalIntensity;     // Ba�lang��taki ���k yo�unlu�unu saklamak i�in
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

        // -------------- Burada Light referanslar�n� kaydediyoruz --------------
        if (directionalLight != null)
        {
            originalLightColor = directionalLight.color;
            originalIntensity = directionalLight.intensity;
        }
        else
        {
            Debug.LogWarning("MaskAttachDetector: Inspector'dan 'directionalLight' atanmam��!");
        }
        // -----------------------------------------------------------------------
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isAttached) return;

        if (other.CompareTag(triggerTag))
        {
            Debug.Log("Maske kafaya tak�ld�!");

            isAttached = true;

            // G�rseli gizle
            if (maskModel != null)
                maskModel.SetActive(false);

            // Kafaya yap��t�r
            if (headTarget != null)
                transform.SetParent(headTarget, true); // worldPosition korunsun

            // -------------- Burada ���k ayarlar�n� de�i�tiriyoruz --------------
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
            Debug.Log("Maske ��kar�ld�!");

            isAttached = false;

            // G�rseli geri getir
            if (maskModel != null)
                maskModel.SetActive(true);

            // Kafadan ay�r
            transform.SetParent(null);

            // -------------- I���� eski haline d�nd�r�yoruz --------------
            if (directionalLight != null)
            {
                directionalLight.color = originalLightColor;
                directionalLight.intensity = originalIntensity;
            }
            // -------------------------------------------------------------
        }
    }
}


