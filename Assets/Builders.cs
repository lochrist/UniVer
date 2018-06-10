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
            b.tag = Tags.SolidBody;
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
            b.tag = Tags.SolidBody;
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

            world.AddGlobalConstraint(constraint);
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

        public static Body SpiderWeb(this World world, Vector2 origin, float radius, int segments, int depth)
        {
            var stiffness = 0.6f;
            var tensor = 0.3f;
            var stride = (2 * Mathf.PI) / segments;
            var n = segments * depth;
            var radiusStride = radius / n;

            var vertices = new List<Vertex>();
            // particles
            for (var i = 0; i < n; ++i)
            {
                var theta = i * stride + Mathf.Cos(i * 0.4f) * 0.05f + Mathf.Cos(i * 0.05f) * 0.2f;
                var shrinkingRadius = radius - radiusStride * i + Mathf.Cos(i * 0.1f) * 20;

                var offy = Mathf.Cos(theta * 2.1f) * (radius / depth) * 0.2f;
                vertices.Add(new Vertex(origin.x + Mathf.Cos(theta) * shrinkingRadius, origin.y + Mathf.Sin(theta) * shrinkingRadius + offy));
            }

            // constraints
            var constraints = new List<SpringConstraint>();
            for (var i = 0; i < n - 1; ++i)
            {
                // neighbor
                constraints.Add(new SpringConstraint(vertices[i], vertices[i + 1], stiffness));

                // span rings
                var off = i + segments;
                if (off < n - 1)
                    constraints.Add(new SpringConstraint(vertices[i], vertices[off], stiffness));
                else
                    constraints.Add(new SpringConstraint(vertices[i], vertices[n - 1], stiffness));
            }

            constraints.Add(new SpringConstraint(vertices[0], vertices[segments - 1], stiffness));

            foreach (var c in constraints)
                c.distance *= tensor;

            var body = new Body(vertices.ToArray(), constraints.Cast<Constraint>().ToArray());
            world.AddBody(body);
            var pinIndices = new List<int>();
            for (var i = 0; i < segments; i += 4)
            {
                pinIndices.Add(i);
            }
            world.Pins(body, pinIndices.ToArray());

            return body;
        }

        public static Body Tree(this World world, Vector2 origin, int depth, float branchLength, float segmentCoef, float theta)
        {
            var treeBase = new Vertex(origin);
            var root = new Vertex(origin + new Vector2(0, 10));

            var vertices = new List<Vertex>();
            var constraints = new List<Constraint>();

            vertices.Add(treeBase);
            vertices.Add(root);
            
            var firstBranch = Branch(vertices, constraints, treeBase, 0, depth, segmentCoef, new Vector2(0, -1), branchLength, theta);
            constraints.Add(new AngleConstraint(root, treeBase, firstBranch, 1));

            // animates the tree at the beginning
            var noise = 0;
            for (var i = 2; i < vertices.Count; ++i)
                vertices[i].position += new Vector2(Mathf.Floor(Random.value * noise), 0);

            var body = new Body(vertices.ToArray(), constraints.ToArray());
            world.AddBody(body);
            world.Pins(body, 0, 1);
            return body;
        }

        private static Vertex Branch(List<Vertex> vertices, List<Constraint> constraints, Vertex parent, int i, int nMax, float coef, Vector2 normal, float branchLength, float theta)
        {
            const float lineCoef = 0.7f;

            var particle = new Vertex(parent.position + (normal * (branchLength * coef)));
            vertices.Add(particle);

            var dc = new SpringConstraint(parent, particle, lineCoef);
            constraints.Add(dc);
            if (i < nMax)
            {
                var a = Branch(vertices, constraints, particle, i + 1, nMax, coef * coef, MathUtils.Rotate(normal, new Vector2(0, 0), -theta), branchLength, theta);
                var b = Branch(vertices, constraints, particle, i + 1, nMax, coef * coef, MathUtils.Rotate(normal, new Vector2(0, 0), theta), branchLength, theta);

                var jointStrength = Mathf.Lerp(0.7f, 0, (float)i / nMax);
                constraints.Add(new AngleConstraint(parent, particle, a, jointStrength));
                constraints.Add(new AngleConstraint(parent, particle, b, jointStrength));
            }

            return particle;
        }
    }
}