using UnityEngine;

public class PlaySoundOnCollision : MonoBehaviour
{
    [Tooltip("Masa objesinin �zerinde tan�mlanan AudioSource bile�enini buraya s�r�kleyin.")]
    public AudioSource audioSource;

    [Tooltip("�arp��ma yap�lacak di�er objenin tag'i; �rne�in 'Mask'.")]
    public string colliderTag = "Mask";

    private void OnCollisionEnter(Collision collision)
    {
        // �arp��an objenin tag'i 'Mask' ise �al���r
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
