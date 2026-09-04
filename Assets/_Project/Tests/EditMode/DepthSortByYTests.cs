using NUnit.Framework;
using UnityEngine;
using ProjectTheta.Core;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class DepthSortByYTests
    {
        [Test]
        public void Refresh_LowerY_IsRenderedInFront()
        {
            GameObject upper = new GameObject("Upper");
            GameObject lower = new GameObject("Lower");
            try
            {
                SpriteRenderer upperRenderer = upper.AddComponent<SpriteRenderer>();
                SpriteRenderer lowerRenderer = lower.AddComponent<SpriteRenderer>();
                upper.transform.position = new Vector3(0f, 0.5f, 0f);
                lower.transform.position = new Vector3(0f, -0.5f, 0f);

                DepthSortByY upperSort = upper.AddComponent<DepthSortByY>();
                DepthSortByY lowerSort = lower.AddComponent<DepthSortByY>();
                upperSort.Refresh();
                lowerSort.Refresh();

                Assert.Greater(lowerRenderer.sortingOrder, upperRenderer.sortingOrder);
            }
            finally
            {
                Object.DestroyImmediate(upper);
                Object.DestroyImmediate(lower);
            }
        }
    }
}
