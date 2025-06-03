using UnityEngine;

public class PlaySoundOnCollision : MonoBehaviour
{
    [Tooltip("Masa objesinin üzerinde tanýmlanan AudioSource bileþenini buraya sürükleyin.")]
    public AudioSource audioSource;

    [Tooltip("Çarpýþma yapýlacak diðer objenin tag'i; örneðin 'Mask'.")]
    public string colliderTag = "Mask";

    private bool collisionEnabled = false;

    void Start()
    {
        // Çarpma sesini sahne baþladýktan 0.5 saniye sonra aktif et
        Invoke("EnableCollisionSound", 0.5f);
    }

    void EnableCollisionSound()
    {
        collisionEnabled = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Sahne baþýnda tetiklenen çarpýþmalarý engelle
        if (!collisionEnabled) return;

        if (collision.gameObject.CompareTag(colliderTag))
        {
            if (audioSource != null)
            {
                audioSource.Play();
            }
            else
            {
                Debug.LogWarning("PlaySoundOnCollision: AudioSource alaný atanmamýþ!");
            }
        }
    }
}
