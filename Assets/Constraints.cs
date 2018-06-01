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

    public class SpringConstraint
    {

    }
}