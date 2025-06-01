using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HandDebug : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Right Hand Trigger touched: " + other.name);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Right Hand Collision with: " + collision.gameObject.name);
    }

    private void Update()
    {
        if (transform.hasChanged)
        {
            Debug.Log("Hand position changed: " + transform.position);
            transform.hasChanged = false;
        }
    }
}