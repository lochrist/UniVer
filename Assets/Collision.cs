using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniVer
{

    public class Collision
    {
        private static Vector2 m_TestAxis = new Vector2();
        private static Vector2 m_Axis = new Vector2();
        private static Vector2 m_Center = new Vector2();
        private static Vector2 m_Line = new Vector2();
        private static Vector2 m_Response = new Vector2();
        private static Vector2 m_RelativeVelocity = new Vector2();
        private static Vector2 m_Tangent = new Vector2();
        private static Vector2 m_RelativeTangentVelocity = new Vector2();
        private static float m_Depth;
        private static DistanceConstraint m_Edge = null;
        private static Vertex m_Vertex = null;
        private static Body m_Body = null;

        public static bool CheckCollide(Body b0, Body b1)
        {
            if (
                !(0 > Mathf.Abs(b1.center.x - b0.center.x) - (b1.halfExtent.x + b0.halfExtent.x) &&
                    0 > Mathf.Abs(b1.center.y - b0.center.y) - (b1.halfExtent.y + b0.halfExtent.y))
            )
            {
                return false; // no aabb overlap
            }

            var minDistance = float.MaxValue;
            var n0 = b0.edges.Length;
            var n1 = b1.edges.Length;
            var dist = float.MaxValue;

            // Iterate through all of the edges of both bodies
            for (var i = 0; i < n1 + n0; i++)
            {
                // get edge
                var edge = i < n0 ? b0.edges[i] : b1.edges[i - n0];

                // Calculate the perpendicular to this edge and normalize it
                m_TestAxis = MathUtils.Normal(edge.v0.position, edge.v1.position);

                // Project both bodies onto the normal
                b0.ProjectAxis(m_TestAxis);
                b1.ProjectAxis(m_TestAxis);

                //Calculate the distance between the two intervals
                dist = b0.min < b1.min ? b1.min - b0.max : b0.min - b1.max;

                // If the intervals don't overlap, return, since there is no collision
                if (dist > 0) return false;
                else if (Mathf.Abs(dist) < minDistance)
                {
                    minDistance = Mathf.Abs(dist);

                    // Save collision information
                    m_Axis = m_TestAxis;
                    m_Edge = edge;
                }
            }

            m_Depth = minDistance;

            // Ensure collision edge in B1 and collision vertex in B0
            if (m_Edge.parent != b1)
            {
                var t = b1;
                b1 = b0;
                b0 = t;
            }

            // Make sure that the collision normal is pointing at B1
            m_Center = b0.center - b1.center;
            var n = Vector2.Dot(m_Center, m_Axis);

            // Revert the collision normal if it points away from B1
            if (n < 0)
            {
                m_Axis = MathUtils.Neg(m_Axis);
            }

            var smallestDist = float.MaxValue;
            for (var i = 0; i < b0.vertices.Length; i++)
            {
                // Measure the distance of the vertex from the line using the line equation
                var v = b0.vertices[i];
                m_Line = v.position - b1.center;
                dist = Vector2.Dot(m_Axis, m_Line);

                // Set the smallest distance and the collision vertex
                if (dist < smallestDist)
                {
                    smallestDist = dist;
                    m_Vertex = v;
                }
            }
            m_Body = b0;

            // There is no separating axis. Report a collision!
            return true;
        }

        public static bool Resolve()
        {
            // cache vertices positions
            var p0 = m_Edge.v0.position;
            var p1 = m_Edge.v1.position;
            var o0 = m_Edge.v0.oldPosition;
            var o1 = m_Edge.v1.oldPosition;
            var vp = m_Vertex.position;
            var vo = m_Vertex.oldPosition;

            // response vector
            m_Response = m_Axis * m_Depth;

            // calculate where on the edge the collision vertex lies
            var t = Mathf.Abs(p0.x - p1.x) > Mathf.Abs(p0.y - p1.y)
                ? (vp.x - m_Response.x - p0.x) / (p1.x - p0.x)
                : (vp.y - m_Response.y - p0.y) / (p1.y - p0.y);
            var lambda = 1 / (t * t + (1 - t) * (1 - t));

            // mass coefficient
            var m0 = m_Body.mass;
            var m1 = m_Edge.parent.mass;
            var tm = m0 + m1;
            m0 = m0 / tm;
            m1 = m1 / tm;

            // apply the collision response
            p0.x -= m_Response.x * (1 - t) * lambda * m0;
            p0.y -= m_Response.y * (1 - t) * lambda * m0;
            m_Edge.v0.position = p0;

            p1.x -= m_Response.x * t * lambda * m0;
            p1.y -= m_Response.y * t * lambda * m0;
            m_Edge.v1.position = p1;

            vp.x += m_Response.x * m1;
            vp.y += m_Response.y * m1;
            m_Vertex.position = vp;

            //
            // collision friction
            //

            // compute relative velocity
            m_RelativeVelocity.Set(
                vp.x - vo.x - (p0.x + p1.x - o0.x - o1.x) * 0.5f,
                vp.y - vo.y - (p0.y + p1.y - o0.y - o1.y) * 0.5f
            );

            // axis perpendicular
            m_Tangent = Vector2.Perpendicular(m_Axis);

            // project the relative velocity onto tangent
            var relTv = Vector2.Dot(m_RelativeVelocity, m_Tangent);
            m_RelativeTangentVelocity.Set(m_Tangent.x * relTv, m_Tangent.y * relTv);


            // apply tangent friction
            vo.x += m_RelativeTangentVelocity.x * World.friction * m1;
            vo.y += m_RelativeTangentVelocity.y * World.friction * m1;
            m_Vertex.oldPosition = vo;

            o0.x -= m_RelativeTangentVelocity.x * (1 - t) * World.friction * lambda * m0;
            o0.y -= m_RelativeTangentVelocity.y * (1 - t) * World.friction * lambda * m0;
            m_Edge.v0.oldPosition = o0;

            o1.x -= m_RelativeTangentVelocity.x * t * World.friction * lambda * m0;
            o1.y -= m_RelativeTangentVelocity.y * t * World.friction * lambda * m0;
            m_Edge.v1.oldPosition = o1;

            return true;
        }

        public static bool IsPointInsideBody(Vector2 p, Body body)
        {
            // Outside of AABB
            if (p.x < (body.center.x - body.halfExtent.x) ||
                p.x > (body.center.x + body.halfExtent.x) ||
                p.y < (body.center.y - body.halfExtent.y) ||
                p.y > (body.center.y + body.halfExtent.y))
            {
                return false;
            }

            // http://www.ecse.rpi.edu/Homepages/wrf/Research/Short_Notes/pnpoly.html
            var inside = false;
            var vertices = body.vertices;
            for (int i = 0, j = vertices.Length - 1; i < vertices.Length; j = i++)
            {
                if ((vertices[i].position.y > p.y) != (vertices[j].position.y > p.y) &&
                     p.x < (vertices[j].position.x - vertices[i].position.x) * 
                     (p.y - vertices[i].position.y) / (vertices[j].position.y - vertices[i].position.y) + vertices[i].position.x)
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        public static Vertex GetClosestVertex(Vertex[] vertices, Vector2 p)
        {
            Vertex v = null;
            var minDistance = float.MaxValue;
            for(int i = 0; i < vertices.Length; ++i)
            {
                var dist = MathUtils.SquareDistance(vertices[i].position, p);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    v = vertices[i];
                }
            }
            return v;
        }

        public struct ClosestInfo
        {
            public Vertex v;
            public Body body;
            public PinConstraint pin;
        }

        public static ClosestInfo GetClosestVertex(World world, Vector2 p, float selectionRadius = 3f)
        {
            var minDistance = float.MaxValue;
            var info = new ClosestInfo();
            var selection2 = selectionRadius * selectionRadius;
            for (var i = 0; i < world.bodies.Count; ++i)
            {
                var b = world.bodies[i];
                for (var vIndex = 0; vIndex < b.vertices.Length; ++vIndex)
                {
                    var dist = MathUtils.SquareDistance(b.vertices[vIndex].position, p);
                    if (dist< selection2 && dist < minDistance)
                    {
                        minDistance = dist;
                        info.v = b.vertices[vIndex];
                        info.body = b;
                    }
                }
            }

            if (info.body != null)
            {
                for (var i = 0; i < info.body.constraints.Length; ++i)
                {
                    var pin = info.body.constraints[i] as PinConstraint;
                    if (pin != null && pin.v == info.v)
                    {
                        info.pin = pin;
                    }
                }
            }

            return info;
        }
    }
}