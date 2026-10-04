using System.Collections.Generic;
using UnityEngine;

public class SellManager : MonoBehaviour
{
    private readonly Dictionary<Collider, Item> _colliderAssemblies = new Dictionary<Collider, Item>();
    private readonly Dictionary<Item, int> _colliderCountsByAssembly = new Dictionary<Item, int>();
    private readonly List<Collider> _staleColliders = new List<Collider>();
    public bool canSell = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _staleColliders.Clear();
        foreach (KeyValuePair<Collider, Item> entry in _colliderAssemblies)
        {
            Collider trackedCollider = entry.Key;
            if (trackedCollider == null ||
                !trackedCollider.enabled ||
                !trackedCollider.gameObject.activeInHierarchy ||
                entry.Value == null ||
                !entry.Value.gameObject.activeInHierarchy)
            {
                _staleColliders.Add(trackedCollider);
            }
        }

        foreach (Collider staleCollider in _staleColliders)
            RemoveCollider(staleCollider);
    }

    void OnTriggerEnter(Collider other)
    {
        Item item = other.GetComponentInParent<Item>();
        if (item == null)
            return;

        Item assemblyRoot = item.GetAssemblyRoot();
        if (assemblyRoot.AssemblyItems.Count <= 1 || _colliderAssemblies.ContainsKey(other))
            return;

        _colliderAssemblies.Add(other, assemblyRoot);
        if (_colliderCountsByAssembly.TryGetValue(assemblyRoot, out int colliderCount))
            _colliderCountsByAssembly[assemblyRoot] = colliderCount + 1;
        else
        {
            _colliderCountsByAssembly.Add(assemblyRoot, 1);
            print("Item entered sell zone : " + assemblyRoot.itemData.name);
        }

        UpdateCanSell();
    }

    void OnTriggerExit(Collider other)
    {
        RemoveCollider(other);
    }

    private void RemoveCollider(Collider collider)
    {
        if (!_colliderAssemblies.TryGetValue(collider, out Item assemblyRoot))
            return;

        _colliderAssemblies.Remove(collider);
        int colliderCount = _colliderCountsByAssembly[assemblyRoot] - 1;
        if (colliderCount > 0)
            _colliderCountsByAssembly[assemblyRoot] = colliderCount;
        else
        {
            _colliderCountsByAssembly.Remove(assemblyRoot);
            print("Item exited sell zone : " + assemblyRoot.itemData.name);
        }

        UpdateCanSell();
    }

    private void UpdateCanSell()
    {
        canSell = _colliderCountsByAssembly.Count == 1;
    }
}
