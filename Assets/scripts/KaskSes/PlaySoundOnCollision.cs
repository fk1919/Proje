using UnityEngine;

public class PlaySoundOnCollision : MonoBehaviour
{
    [Tooltip("Masa objesinin �zerinde tan�mlanan AudioSource bile�enini buraya s�r�kleyin.")]
    public AudioSource audioSource;

    [Tooltip("�arp��ma yap�lacak di�er objenin tag'i; �rne�in 'Mask'.")]
    public string colliderTag = "Mask";

    [Tooltip("Ses �almak i�in gereken minimum �arpma h�z�.")]
    public float minImpactVelocity = 0.5f;

    private bool collisionEnabled = false;

    void Start()
    {
        // �arpma sesini sahne ba�lad�ktan 0.5 saniye sonra aktif et
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
            // �arpman�n �iddetini al
            float impactSpeed = collision.relativeVelocity.magnitude;

            if (impactSpeed >= minImpactVelocity)
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
            else
            {
                Debug.Log($"�arp��ma alg�land� fakat h�z �ok d���k ({impactSpeed:F2}). Ses �al�nmad�.");
            }
        }
    }
}
