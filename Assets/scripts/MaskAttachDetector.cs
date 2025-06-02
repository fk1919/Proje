using UnityEngine;

public class MaskAttachDetector : MonoBehaviour
{
    public GameObject maskModel;         // Maskeyi temsil eden 3D model
    public GameObject blackTintCanvas;   // Ekran� karartan canvas panel
    public string triggerTag = "MaskTrigger";

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(triggerTag))
        {
            Debug.Log("Maske kafaya tak�ld�!");

            // Maskeyi g�r�nmez yap
            if (maskModel != null)
                maskModel.SetActive(false);

            // Canvas�� aktif et
            if (blackTintCanvas != null)
                blackTintCanvas.SetActive(true);
        }
    }
}