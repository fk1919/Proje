using UnityEngine;

public class MaskAttachDetector : MonoBehaviour
{
    public GameObject maskModel;
    public Transform headTarget;  // Kafa pozisyonu
    public Canvas blackTintCanvas; // Ekraný siyahlaþtýracak canvas

    private bool isAttached = false;

    void OnTriggerEnter(Collider other)
    {
        if (isAttached) return;

        if (other.CompareTag("MainCamera"))
        {
            isAttached = true;

            // Maskeyi kafaya "yapýþtýr"
            maskModel.transform.SetParent(headTarget);
            maskModel.transform.localPosition = Vector3.zero;
            maskModel.transform.localRotation = Quaternion.identity;

            // Siyah efekt aç
            if (blackTintCanvas != null)
                blackTintCanvas.enabled = true;
        }
    }
}