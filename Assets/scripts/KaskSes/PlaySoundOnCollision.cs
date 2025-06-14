using UnityEngine;

public class PlaySoundOnCollision : MonoBehaviour
{
    [Tooltip("Masa objesinin üzerinde tanýmlanan AudioSource bileþenini buraya sürükleyin.")]
    public AudioSource audioSource;

    [Tooltip("Çarpýþma yapýlacak diðer objenin tag'i; örneðin 'Mask'.")]
    public string colliderTag = "Mask";

    [Tooltip("Ses çalmak için gereken minimum çarpma hýzý.")]
    public float minImpactVelocity = 0.5f;

    private bool collisionEnabled = false;

    void Start()
    {
        // Çarpma sesini sahne baþladýktan 0.5 saniye sonra aktif et
        Invoke(nameof(EnableCollisionSound), 0.5f);
    }

    void EnableCollisionSound()
    {
        collisionEnabled = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collisionEnabled) return;

        if (collision.gameObject.CompareTag(colliderTag))
        {
            // Çarpmanýn þiddetini al
            float impactSpeed = collision.relativeVelocity.magnitude;

            if (impactSpeed >= minImpactVelocity)
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
            else
            {
                Debug.Log($"Çarpýþma algýlandý fakat hýz çok düþük ({impactSpeed:F2}). Ses çalýnmadý.");
            }
        }
    }
}
