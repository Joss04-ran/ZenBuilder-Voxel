using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public float dayDuration = 600f;   
    public float nightDuration = 420f; 

    [Range(0f, 1f)]
    public float timeOfDay = 0.25f; 

    public Light sunLight;
    public Gradient sunColor;
    public AnimationCurve sunIntensity;

    public Gradient skyColor;
    public Gradient equatorColor;
    public Gradient groundColor;

    void Update()
    {
        bool isDaytime = timeOfDay >= 0.25f && timeOfDay <= 0.75f;

        float timeMultiplier = isDaytime ? (0.5f / dayDuration) : (0.5f / nightDuration);

        timeOfDay += Time.deltaTime * timeMultiplier;

        if (timeOfDay >= 1f) timeOfDay = 0f; 

        UpdateSun();
        UpdateSky();
    }

    void UpdateSun()
    {
        float sunAngle = (timeOfDay * 360f) - 90f;
        sunLight.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);

        sunLight.color = sunColor.Evaluate(timeOfDay);
        sunLight.intensity = sunIntensity.Evaluate(timeOfDay);
    }

    void UpdateSky()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = skyColor.Evaluate(timeOfDay);
        RenderSettings.ambientEquatorColor = equatorColor.Evaluate(timeOfDay);
        RenderSettings.ambientGroundColor = groundColor.Evaluate(timeOfDay);
    }
}