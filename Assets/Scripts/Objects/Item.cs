using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class Item : MonoBehaviour, IInteractable
{
    private const int MaxAssemblyItems = 3;
    private const float HeldCollisionPadding = 0.05f;
    private const int DepenetrationIterations = 8;
    private readonly Collider[] _overlapBuffer = new Collider[32];
    private int _debugFallFramesLeft;
    private bool _warnedUnsupportedPenetration;
    private bool _pendingRelease;
    private Transform _pendingPlayerRoot;
    private Vector3 _pendingRetreatPoint;
    private const float DropLiftOffset = 0.03f;
    private const float GroundRecoveryY = 0.5f;
    private const float MinHeldDistance = 0.5f;          // distance minimale caméra -> centre de l'objet tenu
    private const float MaxHeldOffset = 1f;              // décalage max autorisé après un snap
    private const float MaxPenetrationStep = 0.5f;       // une correction plus grande est jugée aberrante
    private const int RetreatSteps = 30;                 // pas pour ramener l'objet vers le joueur
    private const float LiftSearchHeight = 3f;           // hauteur max testée en dernier recours
    private const float LiftSearchStep = 0.05f;
    private const float HeldOffsetRecenterSpeed = 2f;    // m/s : vitesse de recentrage après un snap
    private const float MaxDepenetrationVelocity = 8f;

    public ObjectSO itemData;
    private readonly List<Item> _assemblyItems = new List<Item>();
    private readonly RaycastHit[] _heldPositionHits = new RaycastHit[32];
    private IReadOnlyList<Item> _assemblyItemsReadOnly;
    [SerializeField] private TextMeshProUGUI _itemNameText;
    [SerializeField] private Color _outlineColor = Color.white;
    [SerializeField] private Color _highlightedOutlineColor = Color.blue;
    [SerializeField] private Color _chestHighlightColor = Color.yellow;
    [SerializeField, Min(0.1f)] private float _heldDistance = 2f;
    public Color OutlineColor => _outlineColor;
    public Color HighlightedOutlineColor => _highlightedOutlineColor;
    [HideInInspector] public bool isInChest = false;
    public bool CanBeAddedToChest =>
    GetAssemblyRoot() == this &&
    GetComponentsInChildren<Item>(true).Length == 1;
    private Outline _outline;
    private bool _isGrabbed = false;
    private Rigidbody _rb;
    private Camera _heldCamera;
    private Vector3 _heldPositionOffset;
    private Color defaultOutlineColor;
    private Color defaultHighlightColor;

    public IReadOnlyList<Item> AssemblyItems
    {
        get
        {
            Item assemblyRoot = GetAssemblyRoot();
            if (assemblyRoot._assemblyItemsReadOnly == null)
                assemblyRoot._assemblyItemsReadOnly = assemblyRoot._assemblyItems.AsReadOnly();

            return assemblyRoot._assemblyItemsReadOnly;
        }
    }

    public bool CanUnFuse =>
        _isGrabbed &&
        GetAssemblyRoot() == this &&
        GetComponentsInChildren<Item>(true).Length > 1;

    public bool CanSnap
    {
        get
        {
            if (!DaysManager.Instance.isInWorkshop || !_isGrabbed)
                return false;

            foreach (SnapPoint snapPoint in GetComponentsInChildren<SnapPoint>(true))
            {
                if (snapPoint.TryGetContact(out SnapPoint contactingSnapPoint) &&
                    CanMergeWith(contactingSnapPoint.currentItem.GetAssemblyRoot()))
                    return true;
            }

            return false;
        }
    }

    void Start()
    {
        RefreshAssemblyItems();
        _outline = GetComponent<Outline>();
        _rb = GetComponent<Rigidbody>();
        if (_rb != null)
            _rb.maxDepenetrationVelocity = MaxDepenetrationVelocity;

        if (_itemNameText != null && itemData != null)
        {
            _itemNameText.text = itemData.objectName;
        }

        _outline.OutlineColor = _outlineColor;
        defaultOutlineColor = _outlineColor;
        defaultHighlightColor = _highlightedOutlineColor;
    }

    void LateUpdate()
    {
        if (!_isGrabbed || _heldCamera == null)
            return;

        UpdateHeldPosition();
        print(AssemblyItems.Count);
    }

    // Point devant le joueur où l'objet tenu est censé se trouver (sans offset de snap)
    private Vector3 ComputeHeldBasePosition()
    {
        Vector3 cameraForward = _heldCamera.transform.forward;
        Vector3 horizontalForward = Vector3.ProjectOnPlane(cameraForward, Vector3.up);

        if (horizontalForward.sqrMagnitude < 0.0001f)
            horizontalForward = Vector3.ProjectOnPlane(_heldCamera.transform.root.forward, Vector3.up);

        horizontalForward.Normalize();

        float verticalComponent = Mathf.Clamp(cameraForward.y, -1f, 1f);
        Vector3 direction =
            horizontalForward * Mathf.Sqrt(1f - verticalComponent * verticalComponent) +
            Vector3.up * verticalComponent;

        return _heldCamera.transform.position + direction * _heldDistance;
    }

    // Recalcule l'offset pour que le centre ACTUEL de l'assemblage ne saute pas
    // (appelé après un merge ou un unfuse, quand les bounds changent d'un coup).
    private void RebaseHeldOffset()
    {
        if (_heldCamera == null || !TryGetAssemblyBounds(out Bounds bounds))
        {
            _heldPositionOffset = Vector3.zero;
            return;
        }

        Vector3 worldOffset = Vector3.ClampMagnitude(
            bounds.center - ComputeHeldBasePosition(),
            MaxHeldOffset);
        _heldPositionOffset = _heldCamera.transform.InverseTransformVector(worldOffset);
    }

    private void UpdateHeldPosition()
    {
        // L'offset de snap se résorbe doucement : l'assemblage glisse vers le point de tenue
        _heldPositionOffset = Vector3.MoveTowards(
            _heldPositionOffset,
            Vector3.zero,
            HeldOffsetRecenterSpeed * Time.deltaTime);

        Vector3 heldPosition =
            ComputeHeldBasePosition() +
            _heldCamera.transform.TransformVector(_heldPositionOffset);

        bool hasBounds = TryGetAssemblyBounds(out Bounds bounds);
        Vector3 halfExtents = hasBounds
            ? Vector3.Max(bounds.extents, Vector3.one * 0.025f)
            : Vector3.one * 0.025f;
        Vector3 unobstructedPosition = GetUnobstructedHeldPosition(
            _heldCamera.transform.position,
            heldPosition,
            halfExtents);

        // Empêche l'objet de se retrouver collé dans la caméra (gros assemblage près d'un obstacle)
        Vector3 cameraPosition = _heldCamera.transform.position;
        Vector3 fromCamera = unobstructedPosition - cameraPosition;
        if (fromCamera.magnitude < MinHeldDistance)
        {
            Vector3 pushDirection = fromCamera.sqrMagnitude > 0.0001f
                ? fromCamera.normalized
                : _heldCamera.transform.forward;
            unobstructedPosition = cameraPosition + pushDirection * MinHeldDistance;
        }

        if (hasBounds)
            transform.position += unobstructedPosition - bounds.center;
        else
            transform.position = unobstructedPosition;
    }

    private Vector3 GetUnobstructedHeldPosition(Vector3 origin, Vector3 target, Vector3 halfExtents)
    {
        Vector3 offset = target - origin;
        float distance = offset.magnitude;
        if (distance <= 0f)
            return target;

        Vector3 direction = offset / distance;
        int hitCount = Physics.BoxCastNonAlloc(
            origin,
            halfExtents,
            direction,
            _heldPositionHits,
            Quaternion.identity,
            distance,
            ~0,
            QueryTriggerInteraction.Ignore);

        RaycastHit[] hits = _heldPositionHits;
        if (hitCount == hits.Length)
        {
            hits = Physics.BoxCastAll(
                origin,
                halfExtents,
                direction,
                Quaternion.identity,
                distance,
                ~0,
                QueryTriggerInteraction.Ignore);
            hitCount = hits.Length;
        }

        float closestDistance = distance;
        Transform playerRoot = _heldCamera.transform.root;
        for (int i = 0; i < hitCount; i++)
        {
            Collider hitCollider = hits[i].collider;
            if (hitCollider == null)
                continue;

            Transform hitTransform = hitCollider.transform;
            if (hitTransform == playerRoot || hitTransform.IsChildOf(playerRoot))
                continue;

            closestDistance = Mathf.Min(closestDistance, hits[i].distance);
        }

        return origin + direction * Mathf.Max(0f, closestDistance - HeldCollisionPadding);
    }

    public void SetItemNameVisible(bool visible)
    {
        if (_itemNameText != null)
            _itemNameText.gameObject.SetActive(visible);
    }

    public void SetOutlineVisible(bool visible)
    {
        if (_outline != null)
            _outline.enabled = visible;
    }

    public void PickUp(Camera playerCamera = null)
    {
        if (DaysManager.Instance.isInWorkshop)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.isKinematic = true;
            _pendingRelease = false;
            _pendingPlayerRoot = null;
            SetAssemblyCollidersEnabled(false);

            SetAssemblyPresentationVisible(false);
            if (isInChest)
                SetOutlineVisible(true);

            _heldCamera = playerCamera;
            _heldPositionOffset = Vector3.zero;
            _isGrabbed = true;
            transform.SetParent(playerCamera.transform, true);
            UpdateHeldPosition();
        }
        else
        {
            int weight = itemData.tags == Tag.Heavy ? 2 : 1;

            if (WeightManager.Instance.SliderBar.value + weight <= WeightManager.Instance.SliderBar.maxValue)
            {
                WeightManager.Instance.AddSliderValue(weight);
                DaysManager.Instance.AddToTruck(this);

                // Son de ramassage d'objet

                Destroy(gameObject);
            }
        }
    }

    public void Drop()
    {
        if (!_isGrabbed)
            return;

        Transform playerRoot = _heldCamera != null ? _heldCamera.transform.root : null;
        Vector3 retreatPoint = _heldCamera != null ? _heldCamera.transform.position : transform.position;
        Vector3 dropPosition = transform.position + Vector3.up * DropLiftOffset;
        Quaternion dropRotation = transform.rotation;

        transform.SetParent(null, true);
        transform.SetPositionAndRotation(dropPosition, dropRotation);
        _isGrabbed = false;
        _heldCamera = null;
        _heldPositionOffset = Vector3.zero;

        // Le rigidbody reste kinematic : on réactive les colliders et on sort
        // manuellement l'objet de tout ce qu'il chevauche AVANT que la physique ne s'en mêle.
        SetAssemblyCollidersEnabled(true);
        Physics.SyncTransforms();

        // Debug.Log($"[Drop] pos={transform.position} interp={_rb.interpolation} kinematic={_rb.isKinematic} gravity={_rb.useGravity} mass={_rb.mass}", this);
        ResolveDropPenetration(playerRoot, retreatPoint);

        _rb.position = transform.position;
        _rb.rotation = transform.rotation;

        // Le Rigidbody reste kinematic : il ne devient dynamique qu'au prochain FixedUpdate
        _pendingPlayerRoot = playerRoot;
        _pendingRetreatPoint = retreatPoint;
        _pendingRelease = true;
    }

    void FixedUpdate()
    {
        if (_debugFallFramesLeft > 0 && _rb != null && !_rb.isKinematic)
        {
            // Debug.Log($"[Fall] frames restantes={_debugFallFramesLeft} pos={_rb.position} vel={_rb.linearVelocity} |vel|={_rb.linearVelocity.magnitude:F2}", this);
            _debugFallFramesLeft--;
        }

        if (!_pendingRelease)
            return;

        _pendingRelease = false;
        if (_rb == null)
            return;

        // Dernière vérification juste avant que la physique ne reprenne la main
        ResolveDropPenetration(_pendingPlayerRoot, _pendingRetreatPoint);
        _pendingPlayerRoot = null;

        _rb.position = transform.position;
        _rb.rotation = transform.rotation;
        _rb.isKinematic = false;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rb.maxDepenetrationVelocity = MaxDepenetrationVelocity;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.WakeUp();
        _debugFallFramesLeft = 15;
    }

    // Déplace l'objet hors de tous les colliders qu'il chevauche (sol, murs, meubles...).
    private void ResolveDropPenetration(Transform playerRoot, Vector3 retreatPoint)
    {
        Collider[] myColliders = GetComponentsInChildren<Collider>();
        Vector3 startPosition = transform.position;
        bool suspicious = false;
        bool penetratesGround = false;

        for (int iteration = 0; iteration < DepenetrationIterations && !suspicious; iteration++)
        {
            bool moved = false;

            foreach (Collider mine in myColliders)
            {
                if (!mine.enabled || mine.isTrigger)
                    continue;

                Bounds b = mine.bounds;
                int count = Physics.OverlapBoxNonAlloc(
                    b.center,
                    b.extents,
                    _overlapBuffer,
                    Quaternion.identity,
                    ~0,
                    QueryTriggerInteraction.Ignore);

                for (int i = 0; i < count; i++)
                {
                    Collider other = _overlapBuffer[i];
                    if (other == null || other == mine || other.transform.IsChildOf(transform))
                        continue;

                    if (playerRoot != null && other.transform.IsChildOf(playerRoot))
                        continue;

                    if (TryComputePenetration(mine, other, out Vector3 direction, out float distance, out bool supported))
                    {
                        if (direction.y > 0.7f && other.bounds.center.y < mine.bounds.center.y)
                            penetratesGround = true;

                        bool hasRendererBounds = TryGetAssemblyBounds(out Bounds rendererBounds);
                        // Debug.Log(
                        //     $"[Drop] PÉNÈTRE : {other.name} ({other.GetType().Name}, layer {LayerMask.LayerToName(other.gameObject.layer)}) dir={direction} dist={distance:F3}\n" +
                        //     $"  autre.bounds = {other.bounds}\n" +
                        //     $"  item ({mine.GetType().Name} sur '{mine.name}').bounds = {mine.bounds}\n" +
                        //     $"  renderers.bounds = {(hasRendererBounds ? rendererBounds.ToString() : "aucun")}\n" +
                        //     $"  pivot de l'item = {transform.position}",
                        //     other);
                        if (distance > MaxPenetrationStep)
                        {
                            suspicious = true;
                            break;
                        }

                        transform.position += direction * (distance + 0.001f);
                        Physics.SyncTransforms();
                        moved = true;
                    }
                    else if (!supported)
                    {
                        WarnUnsupportedPenetration(mine, other);
                    }
                }

                if (suspicious)
                    break;
            }

            if (!moved)
                break;
        }

        // Une correction énorme (ex. plusieurs mètres) n'est pas crédible pour un objet tenu devant
        // le joueur : on annule tout et on ramène l'objet vers le joueur à la place.
        float totalMoved = (transform.position - startPosition).magnitude;
        if (suspicious || totalMoved > MaxPenetrationStep * 2f)
        {
            Debug.LogWarning($"[Drop] Correction aberrante annulée (déplacement total={totalMoved:F2}) -> repli vers le joueur", this);
            transform.position = startPosition;
            Physics.SyncTransforms();
            RetreatToFreeSpace(myColliders, playerRoot, retreatPoint);
        }

        if (penetratesGround)
        {
            Vector3 recoveredPosition = transform.position;
            recoveredPosition.y = GroundRecoveryY;
            transform.position = recoveredPosition;
            Physics.SyncTransforms();
            Debug.LogWarning($"[Drop] Item détecté dans le sol, téléporté à Y={GroundRecoveryY:F1}.", this);
        }
    }

    // Dernier recours : ramène l'objet vers le joueur (zone forcément libre), puis vers le haut.
    private void RetreatToFreeSpace(Collider[] myColliders, Transform playerRoot, Vector3 retreatPoint)
    {
        Vector3 start = transform.position;

        for (int step = 1; step <= RetreatSteps; step++)
        {
            transform.position = Vector3.Lerp(start, retreatPoint, step / (float)RetreatSteps);
            if (!IsOverlappingWorld(myColliders, playerRoot))
            {
                Physics.SyncTransforms();
                return;
            }
        }

        for (float lift = LiftSearchStep; lift <= LiftSearchHeight; lift += LiftSearchStep)
        {
            transform.position = start + Vector3.up * lift;
            if (!IsOverlappingWorld(myColliders, playerRoot))
            {
                Physics.SyncTransforms();
                return;
            }
        }

        transform.position = start;
        Physics.SyncTransforms();
        Debug.LogWarning("[Drop] Aucune position libre trouvée autour de l'objet", this);
    }

    // Le test par bounds sert de broadphase ; ComputePenetration filtre les faux positifs.
    private bool IsOverlappingWorld(Collider[] myColliders, Transform playerRoot)
    {
        foreach (Collider mine in myColliders)
        {
            if (!mine.enabled || mine.isTrigger)
                continue;

            int count;
            if (mine is BoxCollider box)
            {
                Transform t = box.transform;
                Vector3 scale = t.lossyScale;
                Vector3 half = Vector3.Scale(
                    box.size * 0.5f,
                    new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
                half = Vector3.Max(half - Vector3.one * 0.005f, Vector3.one * 0.001f);

                count = Physics.OverlapBoxNonAlloc(
                    t.TransformPoint(box.center),
                    half,
                    _overlapBuffer,
                    t.rotation,
                    ~0,
                    QueryTriggerInteraction.Ignore);
            }
            else
            {
                Bounds b = mine.bounds;
                count = Physics.OverlapBoxNonAlloc(
                    b.center,
                    b.extents,
                    _overlapBuffer,
                    Quaternion.identity,
                    ~0,
                    QueryTriggerInteraction.Ignore);
            }

            for (int i = 0; i < count; i++)
            {
                Collider other = _overlapBuffer[i];
                if (other == null || other == mine || other.transform.IsChildOf(transform))
                    continue;

                if (playerRoot != null && other.transform.IsChildOf(playerRoot))
                    continue;

                if (TryComputePenetration(mine, other, out _, out _, out bool supported))
                    return true;

                if (!supported)
                {
                    WarnUnsupportedPenetration(mine, other);
                    return true;
                }
            }
        }

        return false;
    }

    private static bool CanBePenetrationSource(Collider collider)
    {
        return collider is BoxCollider ||
               collider is SphereCollider ||
               collider is CapsuleCollider ||
               (collider is MeshCollider meshCollider && meshCollider.convex);
    }

    // ComputePenetration requires a primitive or convex mesh as its first collider.
    // Reverse the pair when only the other collider satisfies that requirement.
    private static bool TryComputePenetration(
        Collider mine,
        Collider other,
        out Vector3 direction,
        out float distance,
        out bool supported)
    {
        if (CanBePenetrationSource(mine))
        {
            supported = true;
            return Physics.ComputePenetration(
                mine, mine.transform.position, mine.transform.rotation,
                other, other.transform.position, other.transform.rotation,
                out direction, out distance);
        }

        if (CanBePenetrationSource(other))
        {
            supported = true;
            bool penetrates = Physics.ComputePenetration(
                other, other.transform.position, other.transform.rotation,
                mine, mine.transform.position, mine.transform.rotation,
                out direction, out distance);
            direction = -direction;
            return penetrates;
        }

        supported = false;
        direction = Vector3.zero;
        distance = 0f;
        return false;
    }

    private void WarnUnsupportedPenetration(Collider mine, Collider other)
    {
        if (_warnedUnsupportedPenetration)
            return;

        _warnedUnsupportedPenetration = true;
        // Debug.LogWarning(
        //     $"[Drop] Impossible de calculer la pénétration entre '{mine.name}' ({mine.GetType().Name}) et " +
        //     $"'{other.name}' ({other.GetType().Name}) : aucun n'est une primitive ou un MeshCollider convexe. " +
        //     "Utilisez un MeshCollider convexe ou des colliders primitifs pour ces objets.",
        //     this);
    }

    public void AddToChest()
    {
        if (!CanBeAddedToChest)
        {
            Debug.LogWarning("Only individual items can be added to the chest.", this);
            return;
        }

        _outline.OutlineColor = _chestHighlightColor;
        _outlineColor = _chestHighlightColor;
        _highlightedOutlineColor = _chestHighlightColor;
        SetOutlineVisible(true);
        DaysManager.Instance.AddToChest(this);
        isInChest = true;
    }

    public void RemoveFromChest()
    {
        _outlineColor = defaultOutlineColor;
        _outline.OutlineColor = _outlineColor;
        _highlightedOutlineColor = defaultHighlightColor;
        DaysManager.Instance.RemoveFromChest(this);
        isInChest = false;
    }

    public void Rotate()
    {
        if (!Input.GetKey(KeyCode.E))
            return;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        float rotationSpeed = 100f;

        transform.Rotate(Vector3.up, mouseX * rotationSpeed * Time.deltaTime, Space.World);
        transform.Rotate(Vector3.right, mouseY * rotationSpeed * Time.deltaTime, Space.World);
    }

    public void TrySnap()
    {
        if (!CanSnap)
            return;

        foreach (SnapPoint snapPoint in GetComponentsInChildren<SnapPoint>(true))
        {
            if (!snapPoint.TryGetContact(out SnapPoint contactingSnapPoint))
                continue;

            Item otherRoot = contactingSnapPoint.currentItem.GetAssemblyRoot();
            if (!CanMergeWith(otherRoot))
                continue;

            Vector3 snapOffset = contactingSnapPoint.transform.position - snapPoint.transform.position;
            transform.position += snapOffset;

            if (isInChest)
                RemoveFromChest();
            if (otherRoot.isInChest)
                otherRoot.RemoveFromChest();

            otherRoot.MergeInto(this);
            RebaseHeldOffset();
            return;
        }
    }

    public void UnFuse()
    {
        if (!CanUnFuse)
            return;

        Item[] assemblyItems = GetComponentsInChildren<Item>(true);
        foreach (Item item in assemblyItems)
        {
            if (item == this)
                continue;

            item.transform.SetParent(null, true);
            item._isGrabbed = false;
            item._heldCamera = null;
            item._heldPositionOffset = Vector3.zero;
            if (item._rb == null)
                item._rb = item.gameObject.AddComponent<Rigidbody>();

            item._rb.maxDepenetrationVelocity = MaxDepenetrationVelocity;
            item._rb.detectCollisions = true;
            item._rb.isKinematic = true; // reste kinematic jusqu'au prochain FixedUpdate

            item.SetItemCollidersEnabled(true);

            item.SetOutlineVisible(true);
            item.SetItemNameVisible(false);
        }

        Physics.SyncTransforms();
        RebaseHeldOffset();

        // Une fois TOUS les items détachés, on les écarte de ce qu'ils chevauchent
        // (sol, joueur ignoré, et les autres pièces de l'assemblage), puis on diffère la physique.
        Transform playerRoot = _heldCamera != null ? _heldCamera.transform.root : null;
        Vector3 retreatPoint = _heldCamera != null ? _heldCamera.transform.position : transform.position;
        foreach (Item item in assemblyItems)
        {
            if (item == this)
                continue;

            item.ResolveDropPenetration(playerRoot, retreatPoint);
            item._rb.position = item.transform.position;
            item._rb.rotation = item.transform.rotation;
            item._pendingPlayerRoot = playerRoot;
            item._pendingRetreatPoint = retreatPoint;
            item._pendingRelease = true;
        }

        RefreshAssemblyItems();
        foreach (Item item in assemblyItems)
        {
            if (item != this)
                item.RefreshAssemblyItems();
        }
    }

    public Item GetAssemblyRoot()
    {
        Item assemblyRoot = this;
        Transform parent = transform.parent;
        while (parent != null)
        {
            Item parentItem = parent.GetComponent<Item>();
            if (parentItem != null)
                assemblyRoot = parentItem;

            parent = parent.parent;
        }

        return assemblyRoot;
    }

    private void MergeInto(Item assemblyRoot)
    {
        Item[] mergedItems = GetComponentsInChildren<Item>(true);
        transform.SetParent(assemblyRoot.transform, true);

        foreach (Item mergedItem in mergedItems)
        {
            mergedItem._isGrabbed = false;
            mergedItem._heldCamera = null;
            mergedItem.SetItemNameVisible(false);
            mergedItem.SetOutlineVisible(false);

            mergedItem.SetItemCollidersEnabled(false);

            if (mergedItem._rb != null)
            {
                mergedItem._rb.linearVelocity = Vector3.zero;
                mergedItem._rb.angularVelocity = Vector3.zero;
                mergedItem._rb.detectCollisions = false;
                mergedItem._rb.isKinematic = true;
                Destroy(mergedItem._rb);
                mergedItem._rb = null;
            }
        }

        assemblyRoot.SetAssemblyCollidersEnabled(false);
        assemblyRoot.SetAssemblyPresentationVisible(false);
        assemblyRoot.RefreshAssemblyItems();
    }

    private bool CanMergeWith(Item otherRoot)
    {
        return otherRoot != null &&
               otherRoot != this &&
               !otherRoot._isGrabbed &&
               GetAssemblyRoot() == this &&
               GetComponentsInChildren<Item>(true).Length +
               otherRoot.GetComponentsInChildren<Item>(true).Length <= MaxAssemblyItems;
    }

    private void RefreshAssemblyItems()
    {
        Item assemblyRoot = GetAssemblyRoot();
        if (assemblyRoot != this)
        {
            assemblyRoot.RefreshAssemblyItems();
            return;
        }

        _assemblyItems.Clear();
        _assemblyItems.AddRange(GetComponentsInChildren<Item>(true));
    }

    private void SetAssemblyCollidersEnabled(bool enabled)
    {
        foreach (Item item in GetComponentsInChildren<Item>(true))
            item.SetItemCollidersEnabled(enabled);
    }

    private void SetItemCollidersEnabled(bool enabled)
    {
        foreach (Collider itemCollider in GetComponents<Collider>())
            itemCollider.enabled = enabled;
    }

    private bool TryGetAssemblyBounds(out Bounds bounds)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            bounds = default;
            return false;
        }

        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return true;
    }

    private void SetAssemblyPresentationVisible(bool visible)
    {
        foreach (Item item in GetComponentsInChildren<Item>(true))
        {
            item.SetItemNameVisible(visible);
            item.SetOutlineVisible(visible);
        }
    }
}