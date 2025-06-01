using UnityEngine;

public class MaskAttachDetector : MonoBehaviour
{
    public GameObject maskModel;
    public Transform headTarget;  // Kafa pozisyonu
    public Canvas blackTintCanvas; // Ekran� siyahla�t�racak canvas

    private bool isAttached = false;

    void OnTriggerEnter(Collider other)
    {
        if (isAttached) return;

        if (other.CompareTag("MainCamera"))
        {
            isAttached = true;

            // Maskeyi kafaya "yap��t�r"
            maskModel.transform.SetParent(headTarget);
            maskModel.transform.localPosition = Vector3.zero;
            maskModel.transform.localRotation = Quaternion.identity;

            // Siyah efekt a�
            if (blackTintCanvas != null)
                blackTintCanvas.enabled = true;
        }
    }
}