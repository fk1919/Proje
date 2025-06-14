using UnityEngine;
using Oculus;

public class TorchPickupController : MonoBehaviour
{
    public Transform rightHandAnchor;
    public float pickupDistance = 0.3f;

    private Rigidbody rb;
    private bool isHeld = false;
    private bool aButtonLastState = false;
    public Vector3 rotationOffsetEuler = new Vector3(0, 0, 0);
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogWarning("Torch object requires a Rigidbody component.");
        }
    }

    void Update()
    {
        bool aButtonCurrentState = OVRInput.Get(OVRInput.RawButton.B);

        if (!aButtonLastState && aButtonCurrentState)
        {
            if (!isHeld)
            {
                float distance = Vector3.Distance(transform.position, rightHandAnchor.position);
                if (distance <= pickupDistance)
                {
                    PickUp();
                }
            }
            else
            {
                Drop();
            }
        }

        aButtonLastState = aButtonCurrentState;
    }

    void PickUp()
    {
        isHeld = true;
        // E�er WeldingTorchController varsa ona "elde" bilgisini g�nder
    

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        WeldingTorchController torchController = GetComponent<WeldingTorchController>();
        if (torchController != null) torchController.isHeld = true;

        transform.SetParent(rightHandAnchor);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(rotationOffsetEuler); // Avu� i�i y�n�yle hizalama
    }

    void Drop()
    {
        isHeld = false;


        transform.SetParent(null); // Parent'� bo�alt = sahneye sal

        if (rb != null)
        {
            rb.isKinematic = false;   // Fizikler tekrar aktif
            rb.useGravity = true;
        }
        WeldingTorchController torchController = GetComponent<WeldingTorchController>();
        if (torchController != null) torchController.isHeld = false;
    }
}