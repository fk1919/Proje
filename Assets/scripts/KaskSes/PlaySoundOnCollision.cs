using UnityEngine;

public class PlaySoundOnCollision : MonoBehaviour
{
    [Tooltip("Masa objesinin �zerinde tan�mlanan AudioSource bile�enini buraya s�r�kleyin.")]
    public AudioSource audioSource;

    [Tooltip("�arp��ma yap�lacak di�er objenin tag'i; �rne�in 'Mask'.")]
    public string colliderTag = "Mask";

    private bool collisionEnabled = false;

    void Start()
    {
        // �arpma sesini sahne ba�lad�ktan 0.5 saniye sonra aktif et
        Invoke("EnableCollisionSound", 0.5f);
    }

    void EnableCollisionSound()
    {
        collisionEnabled = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Sahne ba��nda tetiklenen �arp��malar� engelle
        if (!collisionEnabled) return;

        if (collision.gameObject.CompareTag(colliderTag))
        {
            if (audioSource != null)
            {
                audioSource.Play();
            }
            else
            {
                Debug.LogWarning("PlaySoundOnCollision: AudioSource alan� atanmam��!");
            }
        }
    }
}
