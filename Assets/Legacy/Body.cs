using System.Linq;
using UnityEngine;

namespace UniVer.Legacy
{
    public class Vertex
    {
        public Vertex(Vector2 pos)
        {
            position = pos;
            oldPosition = position;
        }

        public Vertex(float x, float y)
        {
            position.x = x;
            position.y = y;
            oldPosition = position;
        }
        public Vector2 oldPosition;
        public Vector2 position;

        public override string ToString()
        {
            return position.ToString();
        }
    }

    public class Body
    {
        public Vertex[] vertices;
        public DistanceConstraint[] edges;
        public Constraint[] constraints;
        public Vector2 center = new Vector2();
        public Vector2 halfExtent = new Vector2();
        public float min = 0f;
        public float max = 0f;
        public float mass = 1.0f;
        public int tag = Tags.NormalBody;
        public int id = idPool++;
        public object data;

        private static int idPool = 0;

        public Body(Vertex[] vertices, Constraint[] constraints, float mass = 1f)
        {
            this.mass = mass;
            this.vertices = vertices;
            this.constraints = constraints;
            edges = this.constraints.Select(c => c as DistanceConstraint).Where(c => c != null && (c.parent = this) != null && c.edge).ToArray();
        }

        public void UpdateBoundingBox()
        {
            var minX = float.MaxValue;
            var minY = float.MaxValue;
            var maxX = float.MinValue;
            var maxY = float.MinValue;

            for (var i = 0; i < vertices.Length; i++)
            {
                var p = vertices[i].position;
                if (p.x > maxX) maxX = p.x;
                if (p.y > maxY) maxY = p.y;
                if (p.x < minX) minX = p.x;
                if (p.y < minY) minY = p.y;
            }

            // center
            center.Set((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);

            // half extents
            halfExtent.Set((maxX - minX) * 0.5f, (maxY - minY) * 0.5f);
        }

        public void ProjectAxis(Vector2 axis)
        {
            var d = Vector2.Dot(vertices[0].position, axis);
            min = max = d;

            for (var i = 1; i < vertices.Length; i++)
            {
                d = Vector2.Dot(vertices[i].position, axis);
                if (d > max) max = d;
                if (d < min) min = d;
            }
        }
    }

}