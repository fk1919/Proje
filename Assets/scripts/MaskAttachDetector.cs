using UnityEngine;

public class MaskAttachDetector : MonoBehaviour
{
    public GameObject maskModel;         // Maskeyi temsil eden 3D model
    public GameObject blackTintCanvas;   // Ekraný karartan canvas panel
    public string triggerTag = "MaskTrigger";

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(triggerTag))
        {
            Debug.Log("Maske kafaya takýldý!");

            // Maskeyi görünmez yap
            if (maskModel != null)
                maskModel.SetActive(false);

            // Canvas’ý aktif et
            if (blackTintCanvas != null)
                blackTintCanvas.SetActive(true);
        }
    }
}