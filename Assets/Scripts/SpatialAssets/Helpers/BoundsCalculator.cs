using System.Collections.Generic;
using UnityEngine;

namespace McKenna.SpatialAssets.Helpers
{
    public class BoundsCalculator
    {
        public void AdjustBoxColliderToFitBounds(GameObject go, Transform scaledPivot, BoxCollider boxCollider)
        {
            if (boxCollider == null)
            {
                Extensions.LogError("No BoxCollider component found on this GameObject.");
                return;
            }

            // Calculate the bounds of all renderers under the pivot
            Bounds combinedBounds = CalculateBounds(scaledPivot.gameObject);

            // Transform the center of the combined bounds to the root's local space
            Vector3 localCenter = go.transform.InverseTransformPoint(combinedBounds.center);
            
            // Adjust the BoxCollider to fit these bounds
            boxCollider.center = localCenter;
            boxCollider.size = combinedBounds.size;

            Extensions.Log($"New BoxCollider Center: {boxCollider.center}, Size: {boxCollider.size}");
        }

        private Bounds CalculateBounds(GameObject go)
        {
            List<Renderer> renderers = new List<Renderer>();
            GetRenderersRecursive(go.transform, renderers);

            if (renderers.Count == 0)
            {
                return new Bounds(go.transform.position, Vector3.zero);
            }

            Bounds combinedBounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Count; i++)
            {
                combinedBounds.Encapsulate(renderers[i].bounds);
            }
            
            return combinedBounds;
        }

        private void GetRenderersRecursive(Transform parent, List<Renderer> renderers)
        {
            foreach (Transform child in parent)
            {
                Renderer renderer = child.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderers.Add(renderer);
                }
                // Recursive call to get renderers from child objects
                GetRenderersRecursive(child, renderers);
            }
        }
    }
}