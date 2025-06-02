using UnityEngine;

public class MaskAttachDetector : MonoBehaviour
{
    public GameObject maskModel;
    public string triggerTag = "MaskTrigger";
    public Transform headTarget;

    [Header("Light Ayarlarý")]
    public Light directionalLight;
    public Color attachedLightColor = Color.gray;
    public float attachedIntensity = 0.95f;

    private Color originalLightColor;
    private float originalIntensity;

    private bool isAttached = false;
    private float attachCooldown = 0f; // tetikleme kilidi için

    private void Start()
    {
        if (directionalLight != null)
        {
            originalLightColor = directionalLight.color;
            originalIntensity = directionalLight.intensity;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isAttached || attachCooldown > 0f) return;

        if (other.CompareTag(triggerTag))
        {
            Debug.Log("Maske takýldý!");

            isAttached = true;

            if (maskModel != null)
                maskModel.SetActive(false);

            if (headTarget != null)
                transform.SetParent(headTarget, true);

            if (directionalLight != null)
            {
                directionalLight.color = attachedLightColor;
                directionalLight.intensity = attachedIntensity;
            }

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    private void Update()
    {
        // Cooldown sayacý
        if (attachCooldown > 0f)
            attachCooldown -= Time.deltaTime;

        if (isAttached && Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("Maske çýkarýlýyor");

            isAttached = false;

            if (maskModel != null)
                maskModel.SetActive(true);

            Vector3 worldPos = transform.position;
            Quaternion worldRot = transform.rotation;

            transform.SetParent(null);
            transform.position = worldPos;
            transform.rotation = worldRot;

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;

                // Maskeyi ileri fýrlat
                Vector3 throwDir = transform.forward + Vector3.up * 0.3f;
                rb.AddForce(throwDir * 2f, ForceMode.VelocityChange);
            }

            if (directionalLight != null)
            {
                directionalLight.color = originalLightColor;
                directionalLight.intensity = originalIntensity;
            }

            // Tetiklemeyi 1 saniye engelle
            attachCooldown = 1f;
        }
    }
}