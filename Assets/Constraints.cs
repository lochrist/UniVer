using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniVer
{
    public interface Constraint
    {
        void Solve(float dt, float stepCoef);
    }

    public class DistanceConstraint : Constraint
    {
        public bool edge;
        public Vertex v0;
        public Vertex v1;
        public float distance;
        public Body parent;

        public DistanceConstraint(Vertex v0, Vertex v1, bool edge = false)
        {
            this.v0 = v0;
            this.v1 = v1;
            this.edge = edge;
            distance = MathUtils.SquareDistance(v0.position, v1.position);
        }

        public bool IsEdge()
        {
            return edge;
        }

        public void Solve(float dt, float stepCoef)
        {
            var dx = v1.position.x - v0.position.x;
            var dy = v1.position.y - v0.position.y;

            // using square root approximation
            var delta = distance / (dx * dx + dy * dy + distance) - 0.5f;

            dx *= delta;
            dy *= delta;
            
            v1.position.x += dx;
            v1.position.y += dy;

            v0.position.x -= dx;
            v0.position.y -= dy;
        }
    }

    public class SpringConstraint : Constraint
    {
        public Vertex v0;
        public Vertex v1;
        public float distance;
        public float stiffness;

        public SpringConstraint(Vertex v0, Vertex v1, float stiffness, float distance = 0.0f)
        {
            this.v0 = v0;
            this.v1 = v1;
            this.stiffness = stiffness;
            distance = distance == 0.0f ? (v0.position - v1.position).magnitude : distance;
        }

        public void Solve(float dt, float stepCoef)
        {
            var normal = v0.position - v1.position;
            var m = normal.sqrMagnitude;
            normal *= (((distance * distance - m) / m) * stiffness * stepCoef);
            v0.position += normal;
            v1.position -= normal;
        }
    }

    public class PinConstraint : Constraint
    {
        public Vertex v;
        public Vector2 position;

        public PinConstraint(Vertex v, Vector2 pos)
        {
            this.v = v;
            position = pos;
        }

        public void Solve(float dt, float stepCoef)
        {
            v.position = position;
        }
    }

    public class AngleConstraint : Constraint
    {
        public Vertex a;
        public Vertex b;
        public Vertex c;
        public float angle;
        public float stiffness;

        public AngleConstraint(Vertex a, Vertex b, Vertex c, float stiffness)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.stiffness = stiffness;
            angle = MathUtils.Angle2(b.position, a.position, c.position);
        }

        public void Solve(float dt, float stepCoef)
        {
            var currentAngle = MathUtils.Angle2(b.position, a.position, c.position);
            var diff = angle - currentAngle;

            if (diff <= -Mathf.PI)
                diff += 2 * Mathf.PI;
            else if (diff >= Mathf.PI)
                diff -= 2 * Mathf.PI;

            diff *= stepCoef * stiffness;

            a.position = MathUtils.Rotate(a.position, b.position, diff);
            c.position = MathUtils.Rotate(c.position, b.position, -diff);
            b.position = MathUtils.Rotate(b.position, a.position, diff);
            b.position = MathUtils.Rotate(b.position, c.position, -diff);
        }
    }
}