using UnityEngine;
using Oculus.VR;
using Oculus.Interaction;

public class MaskAttachDetector : MonoBehaviour
{
    [Header("Mask ve Head Ayarları")]
    public GameObject maskModel;
    public Transform headTarget;

    [Header("Mesafeye Göre Otomatik Takma")]
    [Tooltip("Maskeyi takmak için headTarget'e ne kadar yakın olması gerektiği (metre).")]
    public float attachProximityThreshold = 0.20f;

    [Header("Işık Ayarları")]
    public Light directionalLight;
    public Color attachedLightColor = Color.gray;
    public float attachedIntensity = 0.95f;

    [Header("VR Kontrolcü ve Detach Ayarları")]
    public OVRInput.Controller detachController = OVRInput.Controller.RHand;
    public OVRInput.Button detachButton = OVRInput.Button.PrimaryHandTrigger;
    public float detachProximityThreshold = 0.20f;

    [Header("Interactor Referansları")]
    public MonoBehaviour leftHandInteractorObject;
    public MonoBehaviour rightHandInteractorObject;

    private bool isAttached = false;
    private float attachCooldown = 0f;
    private Color originalLightColor;
    private float originalIntensity;
    private Grabbable grabbableComponent;

    private void Start()
    {
        if (directionalLight != null)
        {
            originalLightColor = directionalLight.color;
            originalIntensity = directionalLight.intensity;
        }
        else
        {
            Debug.LogError("Directional Light Inspector'da atanmamış!");
        }

        grabbableComponent = GetComponent<Grabbable>();
        if (grabbableComponent == null)
        {
            Debug.LogError("Grabbable komponenti bulunamadı!");
        }

        if (leftHandInteractorObject == null || rightHandInteractorObject == null)
        {
            Debug.LogWarning("Interactor referansları atanmadı.");
        }
    }

    private void Update()
    {
        if (attachCooldown > 0f)
        {
            attachCooldown -= Time.deltaTime;
        }

        if (!isAttached && attachCooldown <= 0f && headTarget != null)
        {
            float distanceToHead = Vector3.Distance(transform.position, headTarget.position);
            if (distanceToHead <= attachProximityThreshold)
            {
                AttachMask();
            }
        }

        if (isAttached && attachCooldown <= 0f)
        {
            if (OVRInput.Get(detachButton, detachController))
            {
                Transform activeInteractorTransform = null;
                if (detachController == OVRInput.Controller.RHand && rightHandInteractorObject != null)
                {
                    activeInteractorTransform = rightHandInteractorObject.transform;
                }
                else if (detachController == OVRInput.Controller.LHand && leftHandInteractorObject != null)
                {
                    activeInteractorTransform = leftHandInteractorObject.transform;
                }

                if (activeInteractorTransform != null)
                {
                    float distanceHandToMask = Vector3.Distance(transform.position, activeInteractorTransform.position);
                    if (distanceHandToMask <= detachProximityThreshold)
                    {
                        DetachMask(activeInteractorTransform);
                    }
                }
                else
                {
                    Debug.LogWarning("Detach işlemi için geçerli bir Interactor yok.");
                }
            }
        }
    }

    private void AttachMask()
    {
        isAttached = true;

        if (maskModel != null)
        {
            maskModel.SetActive(false);
        }
        else
        {
            Debug.LogError("Maske modeli atanmamış!");
        }

        transform.SetParent(headTarget, true);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (grabbableComponent != null)
        {
            grabbableComponent.enabled = false;
        }

        if (directionalLight != null)
        {
            directionalLight.color = attachedLightColor;
            directionalLight.intensity = attachedIntensity;
        }

        attachCooldown = 0.5f;
        Debug.Log("AttachMask: Maske başarıyla takıldı.");
    }

    private void DetachMask(Transform interactorTransformToGrabWith)
    {
        isAttached = false;

        if (maskModel != null)
        {
            maskModel.SetActive(true);
        }

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
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (directionalLight != null)
        {
            directionalLight.color = originalLightColor;
            directionalLight.intensity = originalIntensity;
        }

        if (grabbableComponent != null)
        {
            grabbableComponent.enabled = true;
        }

        attachCooldown = 0.5f;
        Debug.Log("DetachMask: Maske çıkarıldı.");
    }
}
