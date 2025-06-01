using UnityEngine;

public class MaskGrabDebugger : MonoBehaviour
{
    void OnEnable()
    {
        Debug.Log("MaskGrabDebugger ENABLED on: " + gameObject.name);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger ENTER: " + other.name);
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision ENTER: " + collision.gameObject.name);
    }

    void Update()
    {
        if (transform.hasChanged)
        {
            Debug.Log(gameObject.name + " position changed to: " + transform.position);
            transform.hasChanged = false;
        }
    }
}