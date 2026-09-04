using NUnit.Framework;
using UnityEngine;
using ProjectTheta.Core;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class SchoolHallwayPrototypeBuilderTests
    {
        [TearDown]
        public void TearDown()
        {
            GameObject root = GameObject.Find("Day02_SchoolHallway");
            if (root != null)
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Build_CreatesSchoolHallwayLandmarks()
        {
            SchoolHallwayPrototypeBuilder.Build();

            Assert.IsNotNull(GameObject.Find("Day02_SchoolHallway"));
            Assert.IsNotNull(GameObject.Find("WindowFrame_0"));
            Assert.IsNotNull(GameObject.Find("Locker_00"));
            Assert.IsNotNull(GameObject.Find("NoticeBoardFrame"));
            Assert.IsNotNull(GameObject.Find("ClassroomDoor_A"));
        }

        [Test]
        public void Build_CreatesWalkAreaBoundaries()
        {
            SchoolHallwayPrototypeBuilder.Build();

            Assert.IsNotNull(GameObject.Find("Boundary_Left")?.GetComponent<BoxCollider2D>());
            Assert.IsNotNull(GameObject.Find("Boundary_Right")?.GetComponent<BoxCollider2D>());
            Assert.IsNotNull(GameObject.Find("Boundary_Back")?.GetComponent<BoxCollider2D>());
            Assert.IsNotNull(GameObject.Find("Boundary_Front")?.GetComponent<BoxCollider2D>());
        }
    }
}
