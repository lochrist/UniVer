using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

namespace UniVer
{

    public class VectorTests
    {
        class Holder
        {
            public Vector2 position = new Vector2();
        }

        [Test]
        public void VectorInPlaceModification()
        {
            var h = new Holder();
            h.position.x = 12;
            Assert.AreEqual(h.position.x, 12);

            h.position.x += 4;
            Assert.AreEqual(h.position.x, 16);

            h.position.Set(1, 2);
            Assert.AreEqual(h.position.x, 1);
            Assert.AreEqual(h.position.y, 2);

            h.position.Set(0, 0);
            h.position += new Vector2(2, 4);
            Assert.AreEqual(h.position.x, 2);
            Assert.AreEqual(h.position.y, 4);
        }

        [Test]
        public void Normal()
        {
            var v1 = new Vector2(2, 4);
            var v2 = new Vector2(4, 7);

            var normal = MathUtils.Normal(v1, v2);
        }

        [Test]
        public void Perp()
        {
            var v1 = new Vector2(2, 4);
            var perp = MathUtils.Perp(v1);

            var perp2 = Vector2.Perpendicular(v1);
            Assert.AreEqual(perp, perp2);
        }
    }

}