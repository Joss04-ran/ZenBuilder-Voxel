using UnityEngine;
public class UnderwaterEffect : MonoBehaviour
{
    [Header("Underwater Settings")]
    public Color underwaterFogColor = new Color(0.05f, 0.3f, 0.5f, 1f);
    public float underwaterFogDensity = 0.08f;
    public Color underwaterAmbient = new Color(0.1f, 0.25f, 0.4f);

    private Color _originalFogColor;
    private float _originalFogDensity;
    private bool _originalFogEnabled;
    private Color _originalAmbient;
    private bool _isUnderwater;

    void Start()
    {
        _originalFogColor = RenderSettings.fogColor;
        _originalFogDensity = RenderSettings.fogDensity;
        _originalFogEnabled = RenderSettings.fog;
        _originalAmbient = RenderSettings.ambientLight;
    }

    void Update()
    {
        bool underwater = WorldManager.Instance != null &&
                          WorldManager.Instance.GetWaterLevelAt(transform.position) > 0;

        if (underwater == _isUnderwater) return;
        _isUnderwater = underwater;

        if (_isUnderwater)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = underwaterFogColor;
            RenderSettings.fogDensity = underwaterFogDensity;
            RenderSettings.ambientLight = underwaterAmbient;
        }
        else
        {
            RenderSettings.fog = _originalFogEnabled;
            RenderSettings.fogColor = _originalFogColor;
            RenderSettings.fogDensity = _originalFogDensity;
            RenderSettings.ambientLight = _originalAmbient;
        }
    }
}