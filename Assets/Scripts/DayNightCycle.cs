using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Light sunLight;

    [Header("Time Settings")]
    [Range(0f, 1f)]
    public float timeOfDay = 0.25f;
    public float dayDuration = 600f;
    public float nightDuration = 420f;

    [Header("Orbit")]
    public float orbitRadius = 150f;

    [Header("Sun Visual")]
    public Material sunMaterial;
    public float sunScale = 10f;

    [Header("Moon Visual")]
    public Material moonMaterial;
    public float moonScale = 6f;

    [Header("Stars")]
    public Material starMaterial;
    public int starCount = 200;
    public float starRadius = 140f;
    public float starScale = 0.5f;

    [Header("Sun Light Settings")]
    public Gradient sunColor;
    public AnimationCurve sunIntensity;

    [Header("Sky Colors")]
    public Gradient skyColor;
    public Gradient equatorColor;
    public Gradient groundColor;

    private Transform _sunTransform;
    private Transform _moonTransform;
    private Transform _starParent;
    private List<Material> _starMaterials = new List<Material>();

    void Start()
    {
        CreateSun();
        CreateMoon();
        CreateStars();
    }
    private void CreateSun()
    {
        GameObject sun = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sun.name = "Sun";
        sun.transform.localScale = Vector3.one * sunScale;
        Destroy(sun.GetComponent<BoxCollider>());

        if (sunMaterial != null)
            sun.GetComponent<MeshRenderer>().material = sunMaterial;

        _sunTransform = sun.transform;
    }
    private void CreateMoon()
    {
        GameObject moon = GameObject.CreatePrimitive(PrimitiveType.Cube);
        moon.name = "Moon";
        moon.transform.localScale = Vector3.one * moonScale;
        Destroy(moon.GetComponent<BoxCollider>());

        if (moonMaterial != null)
            moon.GetComponent<MeshRenderer>().material = moonMaterial;

        _moonTransform = moon.transform;
    }
    private void CreateStars()
    {
        _starParent = new GameObject("StarField").transform;
        _starParent.position = player != null ? player.position : Vector3.zero;

        for (int i = 0; i < starCount; i++)
        {
            GameObject star = GameObject.CreatePrimitive(PrimitiveType.Cube);
            star.name = $"Star_{i}";
            star.transform.SetParent(_starParent);
            star.transform.localScale = Vector3.one * Random.Range(starScale * 0.5f, starScale);
            Destroy(star.GetComponent<BoxCollider>());

            Vector3 dir = Random.onUnitSphere;
            dir.y = Mathf.Abs(dir.y) + Random.Range(0f, 0.5f);
            star.transform.localPosition = dir.normalized * starRadius;

            Material mat = starMaterial != null
                ? new Material(starMaterial)
                : new Material(Shader.Find("Universal Render Pipeline/Lit"));

            mat.EnableKeyword("_EMISSION");
            star.GetComponent<MeshRenderer>().material = mat;
            _starMaterials.Add(mat);
        }
    }
    void Update()
    {
        AdvanceTime();
        UpdateSun();
        UpdateMoon();
        UpdateStars();
        UpdateLight();
        UpdateSky();
    }
    private void AdvanceTime()
    {
        bool isDay = timeOfDay >= 0.25f && timeOfDay <= 0.75f;
        float rate = isDay ? (0.5f / dayDuration) : (0.5f / nightDuration);
        timeOfDay += Time.deltaTime * rate;
        if (timeOfDay >= 1f) timeOfDay = 0f;
    }
    private void UpdateSun()
    {
        if (_sunTransform == null || player == null) return;

        float angle = timeOfDay * 360f - 90f;
        Vector3 offset = new Vector3(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad),
            0f) * orbitRadius;

        _sunTransform.position = player.position + offset;
    }
    private void UpdateMoon()
    {
        if (_moonTransform == null || player == null) return;

        float angle = (timeOfDay + 0.5f) * 360f - 90f;
        Vector3 offset = new Vector3(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad),
            0f) * orbitRadius;

        _moonTransform.position = player.position + offset;
    }
    private void UpdateStars()
    {
        if (_starParent != null && player != null)
            _starParent.position = player.position;

        float nightBlend = GetNightBlend();

        foreach (Material mat in _starMaterials)
        {
            if (mat == null) continue;
            Color emission = Color.white * nightBlend * 3f;
            mat.SetColor("_EmissionColor", emission);
        }
    }
    private float GetNightBlend()
    {
        if (timeOfDay > 0.78f)
            return Mathf.InverseLerp(0.78f, 0.88f, timeOfDay);
        if (timeOfDay < 0.22f)
            return Mathf.InverseLerp(0.22f, 0.12f, timeOfDay);
        return 0f;
    }
    private void UpdateLight()
    {
        if (sunLight == null || _sunTransform == null) return;

        Vector3 toSun = _sunTransform.position - player.position;
        sunLight.transform.rotation = Quaternion.LookRotation(-toSun.normalized);

        sunLight.color = sunColor.Evaluate(timeOfDay);
        sunLight.intensity = sunIntensity.Evaluate(timeOfDay);
    }

    private void UpdateSky()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = skyColor.Evaluate(timeOfDay);
        RenderSettings.ambientEquatorColor = equatorColor.Evaluate(timeOfDay);
        RenderSettings.ambientGroundColor = groundColor.Evaluate(timeOfDay);
    }
}