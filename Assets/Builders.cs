using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UniVer
{
    public static class Builders
    {
        public static Body CreateRectangle(this World world, float x, float y, float w, float h, float mass = 1.0f)
        {
            var vertices = new[]
            {
                new Vertex(x, y),
                new Vertex(x + w, y),
                new Vertex(x + w, y + h),
                new Vertex(x, y + h)
            };

            var constraints = new[]
            {
                new DistanceConstraint(vertices[0], vertices[1], true),
                new DistanceConstraint(vertices[1], vertices[2], true),
                new DistanceConstraint(vertices[2], vertices[3], true),
                new DistanceConstraint(vertices[3], vertices[0], true),
                new DistanceConstraint(vertices[0], vertices[2]),
                new DistanceConstraint(vertices[3], vertices[1])
            };

            var b = new Body(vertices, constraints, mass);
            world.AddBody(b);
            return b;
        }

        public static Body CreateTriangle(this World world, float x, float y, float w, float h, float mass = 1.0f)
        {
            w /= 2;
            h /= 2;

            var vertices = new[]
            {
                new Vertex(x - w, y + h),
                new Vertex(x, y - h),
                new Vertex(x + w, y + h)
            };

            var constraints = new[]
            {
                new DistanceConstraint(vertices[0], vertices[1], true),
                new DistanceConstraint(vertices[1], vertices[2], true),
                new DistanceConstraint(vertices[2], vertices[0], true),
            };

            var b = new Body(vertices, constraints, mass);
            world.AddBody(b);
            return b;
        }

        public static DistanceConstraint CreateJoint(this World world, Vertex v0, Vertex v1)
        {
            var constraint = new DistanceConstraint(
                v0,
                v1,
                false
            );

            world.constraints.Add(constraint);
            return constraint;
        }
    }

}