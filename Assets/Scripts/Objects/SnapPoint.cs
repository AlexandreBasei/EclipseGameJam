using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class SnapPoint : MonoBehaviour
{
    private static readonly HashSet<Item> _visualizationHolders = new HashSet<Item>();
    private static readonly Vector3[] VisualizationCorners =
    {
        new Vector3(-1f, -1f, -1f),
        new Vector3( 1f, -1f, -1f),
        new Vector3( 1f,  1f, -1f),
        new Vector3(-1f,  1f, -1f),
        new Vector3(-1f, -1f, -1f),
        new Vector3(-1f, -1f,  1f),
        new Vector3( 1f, -1f,  1f),
        new Vector3( 1f, -1f, -1f),
        new Vector3( 1f, -1f,  1f),
        new Vector3( 1f,  1f,  1f),
        new Vector3( 1f,  1f, -1f),
        new Vector3( 1f,  1f,  1f),
        new Vector3(-1f,  1f,  1f),
        new Vector3(-1f,  1f, -1f),
        new Vector3(-1f,  1f,  1f),
        new Vector3(-1f, -1f,  1f),
        new Vector3( 1f, -1f,  1f),
        new Vector3( 1f, -1f, -1f),
        new Vector3( 1f, -1f, -1f),
        new Vector3( 1f, -1f,  1f)
    };
    private static Material _visualizationMaterial;
    private static bool _warnedMissingVisualizationShader;

    public Item currentItem;
    public SnapPoint snapPointInContact;
    public BoxCollider collider;

    private readonly HashSet<SnapPoint> _contactingSnapPoints = new HashSet<SnapPoint>();
    private readonly Vector3[] _visualizationPositions = new Vector3[VisualizationCorners.Length];
    private GameObject _visualizationObject;
    private LineRenderer _visualizationLine;

    public static void SetVisualizationActive(Item item, bool active)
    {
        if (item == null)
            return;

        bool changed = active
            ? _visualizationHolders.Add(item)
            : _visualizationHolders.Remove(item);

        int staleHoldersRemoved = _visualizationHolders.RemoveWhere(holder => holder == null);
        if (!changed && staleHoldersRemoved == 0)
            return;

        bool showVisualizations = _visualizationHolders.Count > 0;

        foreach (SnapPoint snapPoint in FindObjectsByType<SnapPoint>(
                     FindObjectsInactive.Include,
                     FindObjectsSortMode.None))
        {
            snapPoint.SetVisualizationVisible(showVisualizations);
        }
    }

    void Awake()
    {
        currentItem = GetComponentInParent<Item>();
        collider = GetComponent<BoxCollider>();
    }

    void OnEnable()
    {
        if (_visualizationHolders.Count > 0)
            SetVisualizationVisible(true);
    }

    void OnDisable()
    {
        if (_visualizationLine != null)
            _visualizationLine.enabled = false;
    }

    void OnDestroy()
    {
        if (_visualizationObject != null)
            Destroy(_visualizationObject);
    }

    void LateUpdate()
    {
        if (_visualizationLine != null && _visualizationLine.enabled)
            UpdateVisualizationPositions();
    }

    void OnTriggerEnter(Collider other)
    {
        SnapPoint otherSnapPoint = other.GetComponent<SnapPoint>();
        if (otherSnapPoint != null &&
            otherSnapPoint != this &&
            otherSnapPoint.currentItem != null)
        {
            _contactingSnapPoints.Add(otherSnapPoint);
            snapPointInContact = otherSnapPoint;
        }
    }

    void OnTriggerExit(Collider other)
    {
        SnapPoint otherSnapPoint = other.GetComponent<SnapPoint>();
        if (otherSnapPoint == null)
            return;

        _contactingSnapPoints.Remove(otherSnapPoint);
        if (snapPointInContact == otherSnapPoint)
        {
            snapPointInContact = null;
            foreach (SnapPoint contactingSnapPoint in _contactingSnapPoints)
            {
                if (contactingSnapPoint != null)
                {
                    snapPointInContact = contactingSnapPoint;
                    break;
                }
            }
        }
    }

    public bool TryGetContact(out SnapPoint contactingSnapPoint)
    {
        _contactingSnapPoints.RemoveWhere(point => !IsContactValid(point));
        if (currentItem == null || collider == null || !collider.enabled)
        {
            snapPointInContact = null;
            contactingSnapPoint = null;
            return false;
        }

        foreach (SnapPoint point in _contactingSnapPoints)
        {
            if (point.currentItem != null &&
                point.currentItem.GetAssemblyRoot() != currentItem.GetAssemblyRoot())
            {
                snapPointInContact = point;
                contactingSnapPoint = point;
                return true;
            }
        }

        snapPointInContact = null;
        contactingSnapPoint = null;
        return false;
    }

    private bool IsContactValid(SnapPoint point)
    {
        return point != null &&
               point.currentItem != null &&
               point.collider != null &&
               collider != null &&
               point.collider.enabled &&
               collider.enabled &&
               gameObject.activeInHierarchy &&
               point.gameObject.activeInHierarchy &&
               Physics.ComputePenetration(
                   collider,
                   collider.transform.position,
                   collider.transform.rotation,
                   point.collider,
                   point.collider.transform.position,
                   point.collider.transform.rotation,
                   out _,
                   out _);
    }

    private void SetVisualizationVisible(bool visible)
    {
        if (!visible)
        {
            if (_visualizationLine != null)
                _visualizationLine.enabled = false;
            return;
        }

        if (collider == null)
            collider = GetComponent<BoxCollider>();

        if (collider == null || !TryGetVisualizationMaterial())
            return;

        if (_visualizationLine == null)
        {
            _visualizationObject = new GameObject($"{name} Snap Point Outline");
            _visualizationLine = _visualizationObject.AddComponent<LineRenderer>();
            _visualizationLine.useWorldSpace = true;
            _visualizationLine.loop = false;
            _visualizationLine.positionCount = VisualizationCorners.Length;
            _visualizationLine.startWidth = 0.015f;
            _visualizationLine.endWidth = 0.015f;
            _visualizationLine.numCapVertices = 2;
            _visualizationLine.shadowCastingMode = ShadowCastingMode.Off;
            _visualizationLine.receiveShadows = false;
            _visualizationLine.sharedMaterial = _visualizationMaterial;
        }

        UpdateVisualizationPositions();
        _visualizationLine.enabled = true;
    }

    private void UpdateVisualizationPositions()
    {
        Vector3 halfSize = collider.size * 0.5f;
        Vector3 center = collider.center;

        for (int i = 0; i < VisualizationCorners.Length; i++)
        {
            Vector3 localPosition = center + Vector3.Scale(VisualizationCorners[i], halfSize);
            _visualizationPositions[i] = collider.transform.TransformPoint(localPosition);
        }

        _visualizationLine.SetPositions(_visualizationPositions);
    }

    private static bool TryGetVisualizationMaterial()
    {
        if (_visualizationMaterial != null)
            return true;

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
            shader = Shader.Find("Sprites/Default");

        if (shader == null)
        {
            if (!_warnedMissingVisualizationShader)
            {
                Debug.LogError("Unable to find a shader for SnapPoint visualizations.");
                _warnedMissingVisualizationShader = true;
            }

            return false;
        }

        _visualizationMaterial = new Material(shader)
        {
            name = "SnapPoint Visualization Material",
            color = Color.cyan
        };
        return true;
    }
}
