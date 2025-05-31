using UnityEngine;

public class WeldingTorchController : MonoBehaviour
{
    [Header("References")]
    public ParticleSystem flameParticles;
    public Light flameLight;
    public AudioSource weldingSound;
    public OVRInput.RawButton weldingButton = OVRInput.RawButton.RIndexTrigger;

    [Header("Settings")]
    public float targetOnIntensity = 3f;  // Alev aktifken ulaþýlacak ýþýk seviyesi

    private float targetIntensity;
    private float lightChangeSpeed = 10f;

    void Start()
    {
        if (flameParticles != null)
        {
            flameParticles.Stop();

            var main = flameParticles.main;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;

            var shape = flameParticles.shape;
            shape.angle = 0f;
            shape.radius = 0f;
            shape.shapeType = ParticleSystemShapeType.Cone;

            var noise = flameParticles.noise;
            noise.enabled = false;
        }

        if (flameLight != null)
            flameLight.intensity = 0;

        if (weldingSound != null)
            weldingSound.Stop();
    }

    void Update()
    {
        if (OVRInput.GetDown(weldingButton)) StartWelding();
        if (OVRInput.GetUp(weldingButton)) StopWelding();

        if (flameLight != null)
        {
            flameLight.intensity = Mathf.Lerp(
                flameLight.intensity,
                targetIntensity,
                Time.deltaTime * lightChangeSpeed
            );
        }
    }

    public void StartWelding()
    {
        if (flameParticles != null)
        {
            flameParticles.Clear();
            flameParticles.Play();
        }

        if (weldingSound != null)
            weldingSound.Play();

        targetIntensity = targetOnIntensity;
    }

    public void StopWelding()
    {
        if (flameParticles != null)
            flameParticles.Stop();

        if (weldingSound != null)
            weldingSound.Stop();

        targetIntensity = 0f;
    }
}
