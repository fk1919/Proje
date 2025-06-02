using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class MaskAttachDetector : MonoBehaviour
{
    public GameObject maskModel; // Görsel kýsmý
    public Image blackTintImage;
    public string triggerTag = "MaskTrigger";
    public Transform headTarget; // CenterEyeAnchor gibi
    private XRGrabInteractable grabInteractable;

    private bool isAttached = false;

    private void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
        }
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

            // Ekraný karart (þimdilik opsiyonel)
            if (blackTintImage != null)
            {
                Color color = blackTintImage.color;
                color.a = 0.8f;
                blackTintImage.color = color;
            }
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
        }
    }
}