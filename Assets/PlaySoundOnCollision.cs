using UnityEngine;

public class PlaySoundOnCollision : MonoBehaviour
{
    [Tooltip("Masa objesinin üzerinde tanýmlanan AudioSource bileþenini buraya sürükleyin.")]
    public AudioSource audioSource;

    [Tooltip("Çarpýþma yapýlacak diðer objenin tag'i; örneðin 'Mask'.")]
    public string colliderTag = "Mask";

    private void OnCollisionEnter(Collision collision)
    {
        // Çarpýþan objenin tag'i 'Mask' ise çalýþýr
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
