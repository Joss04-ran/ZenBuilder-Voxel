using UnityEngine;
public class GhostBlockController : MonoBehaviour
{
    public Transform playerBody;
    public float buildDistance = 5f;

    public Material validMaterial;    
    public Material invalidMaterial;  

    private GameObject _ghost;
    private MeshRenderer _ghostRenderer;
    private bool _isValid;
    private bool _isVisible;

    void Start()
    {
        _ghost = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _ghost.name = "GhostBlock";
        _ghost.transform.localScale = Vector3.one * 1.002f;
        Destroy(_ghost.GetComponent<BoxCollider>());
        _ghostRenderer = _ghost.GetComponent<MeshRenderer>();
        _ghost.SetActive(false);
    }

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, buildDistance))
        {
            Vector3 raw = hit.point + hit.normal * 0.501f;
            Vector3 snapped = new Vector3(
                Mathf.Floor(raw.x) + 0.5f,
                Mathf.Floor(raw.y) + 0.5f,
                Mathf.Floor(raw.z) + 0.5f);

            _ghost.SetActive(true);
            _ghost.transform.position = snapped;
            _isVisible = true;

            _isValid = !OverlapsPlayer(snapped);
            _ghostRenderer.material = _isValid ? validMaterial : invalidMaterial;
        }
        else
        {
            _ghost.SetActive(false);
            _isVisible = false;
            _isValid = false;
        }
    }
    public bool CanPlace() => _isVisible && _isValid;
    private bool OverlapsPlayer(Vector3 center)
    {
        if (playerBody == null) return false;
        Collider col = playerBody.GetComponent<Collider>();
        if (col == null) return false;

        Bounds ghost = new Bounds(center, Vector3.one * 0.85f);
        return ghost.Intersects(col.bounds);
    }

    void OnDestroy()
    {
        if (_ghost != null) Destroy(_ghost);
    }
}