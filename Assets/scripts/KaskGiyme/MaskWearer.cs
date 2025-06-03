using Oculus.Interaction;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


public class MaskWearer : MonoBehaviour
{
    public Transform headTransform; // OVRCameraRig -> CenterEyeAnchor
    public float wearDistance = 0.1f;
    public CanvasGroup darkenOverlay; // UI'de hafif siyahl�k i�in
    private bool isWorn = false;

    private XRGrabInteractable grab;

    void Start()
    {
        grab = GetComponent<XRGrabInteractable>();
    }

    void Update()
    {
        if (isWorn) return;

        float distance = Vector3.Distance(transform.position, headTransform.position);

        if (distance < wearDistance)
        {
            WearMask();
        }
    }

    void WearMask()
    {
        isWorn = true;

        // Maske kafaya yap��s�n
        transform.SetParent(headTransform);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // Maskeyi b�rak, art�k grab yap�lamas�n
        if (grab != null)
        {
            grab.enabled = false;
            Destroy(grab); // ya da disable et
        }

        // Ekran� karart
        if (darkenOverlay != null)
        {
            StartCoroutine(FadeInOverlay());
        }

        Debug.Log("Maske tak�ld�.");
    }

    System.Collections.IEnumerator FadeInOverlay()
    {
        float duration = 1f;
        float time = 0;
        while (time < duration)
        {
            darkenOverlay.alpha = Mathf.Lerp(0, 0.4f, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        darkenOverlay.alpha = 0.4f; // son karartma seviyesi
    }
}