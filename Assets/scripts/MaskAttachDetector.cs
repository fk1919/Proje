using UnityEngine;

public class MaskAttachDetector : MonoBehaviour
{
    public GameObject maskModel;
    public Transform headTarget; // VR kafan�n hedefi
    public Canvas blackTintCanvas;

    private bool isAttached = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isAttached) return;

        if (other.CompareTag("MainCamera"))
        {
            AttachMask();
        }
    }

    void AttachMask()
    {
        isAttached = true;

        if (maskModel != null)
            maskModel.SetActive(false); // maskeyi g�r�nmez yap

        if (blackTintCanvas != null)
            blackTintCanvas.enabled = true; // ekran� karart
    }
}