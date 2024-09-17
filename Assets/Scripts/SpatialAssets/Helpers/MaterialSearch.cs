using System.Collections.Generic;
using UnityEngine;

namespace McKenna.SpatialAssets.Helpers
{
    public class MaterialSearch
    {
        private readonly List<Material> _materials = new();

        public List<Material> SearchMaterial(GameObject go)
        {
            _materials.Clear();
            Extensions.Log($"Root Mesh: {go.name}");
            if (TryGetMaterial(go, out var m))
            {
                Extensions.Log($"Material found in root: {m.name}");
                _materials.Add(m);
            }
        
            SearchMaterialInChildren(go.transform);
            return _materials;
        }

        public bool TryGetMaterial(GameObject go, out Material material)
        {
            if (go.TryGetComponent<MeshRenderer>(out var meshRenderer))
            {
                material = meshRenderer.material;
                return true;
            }
        
            if (go.TryGetComponent<SkinnedMeshRenderer>(out var skinnedMeshRenderer))
            {
                material = skinnedMeshRenderer.material;
                return true;
            }

            material = null;
            return false;
        }

        private void SearchMaterialInChildren(Transform parent)
        {
            foreach (Transform child in parent)
            {
                Extensions.Log($"Child: {child.name}");
                if (TryGetMaterial(child.gameObject, out var m))
                {
                    Extensions.Log($"Material found in child: {m.name}");
                    _materials.Add(m);
                }
            
                SearchMaterialInChildren(child);
            }
        }
    }
}