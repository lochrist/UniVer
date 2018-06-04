using System.Linq;
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

        public static List<Constraint> Pins(this World world, Body body, params int[] vertexIndices)
        {
            var constraints = new List<Constraint>();
            for(var i = 0; i < vertexIndices.Length; ++i)
            {
                var vertexIndex = vertexIndices[i];
                var c = new PinConstraint(body.vertices[vertexIndex], body.vertices[vertexIndex].position);
                constraints.Add(c);
            }

            body.constraints = body.constraints.Concat(constraints).ToArray();

            world.constraints.AddRange(constraints);
            return constraints;
        }

        public static Body Point(this World world, Vertex pos)
        {
            var b = new Body(new[] { pos }, new Constraint[0], 0.1f);
            world.AddBody(b);
            return b;
        }

        public static Body LineSegments(this World world, Vertex[] vertices, float stiffness)
        {
            var constraints = new List<Constraint>();
            for (var i = 0; i < vertices.Length; ++i)
            {
                if (i > 0)
                    constraints.Add(new SpringConstraint(vertices[i], vertices[i - 1], stiffness));
            }

            var b = new Body(vertices, constraints.ToArray(), 0.1f);
            world.AddBody(b);
            return b;
        }

        public static Body Cloth(this World world, Vector2 origin, float width, float height, int segments, int pinMod, float stiffness)
        {
            var xStride = width / segments;
            var yStride = height / segments;

            var vertices = new List<Vertex>();
            var constraints = new List<Constraint>();

            for (var y = 0; y < segments; ++y)
            {
                for (var x = 0; x < segments; ++x)
                {
                    var posX = origin.x + x * xStride - width / 2 + xStride / 2;
                    var posY = origin.y + y * yStride - height / 2 + yStride / 2;
                    vertices.Add(new Vertex(posX, posY));

                    if (x > 0)
                    {
                        constraints.Add(new SpringConstraint(vertices[y * segments + x], vertices[y * segments + x - 1], stiffness));
                    }

                    if (y > 0)
                    {
                        constraints.Add(new SpringConstraint(vertices[y * segments + x], vertices[(y - 1) * segments + x], stiffness));
                    }
                }
            }

            var b = new Body(vertices.ToArray(), constraints.ToArray(), 0.1f);
            world.AddBody(b);
            for (var x = 0; x < segments; ++x)
            {
                if (x % pinMod == 0)
                    world.Pins(b, x);
            }

            return b;
        }

        public static Body Tire(this World world, Vector2 origin, float radius, int segments, float spokeStiffness, float treadStiffness)
        {
            var stride = (2 * Mathf.PI) / segments;
            var vertices = new List<Vertex>();
            var constraints = new List<Constraint>();

            for (var i = 0; i < segments; ++i)
            {
                var theta = i * stride;
                vertices.Add(new Vertex(origin.x + Mathf.Cos(theta) * radius, origin.y + Mathf.Sin(theta) * radius));
            }

            // TODO Handle drawing
            var center = new Vertex(origin);
            vertices.Add(center);

            // constraints
            for (var i = 0; i < segments; ++i)
            {
                constraints.Add(new SpringConstraint(vertices[i], vertices[(i + 1) % segments], treadStiffness));
                constraints.Add(new SpringConstraint(vertices[i], center, spokeStiffness));
                constraints.Add(new SpringConstraint(vertices[i], vertices[(i + 5) % segments], treadStiffness));
            }

            var b = new Body(vertices.ToArray(), constraints.ToArray(), 0.1f);
            world.AddBody(b);
            return b;
        }
    }
}