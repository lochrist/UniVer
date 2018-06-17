using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniVer.Legacy
{
    public interface WorldSolverListener
    {
        void BeginFrame(float dt);
        void EndFrame();
        void BeginSolvingStep(string name, RecordinInfo recInfo);
        void EndSolvingStep();
    }

    public class World
    {
        public static float gravity = -0.2f;
        public static int numIterations = 5;
        public static float friction = 0.2f; // 0.99f
        public static float frictionFloor = 0.1f; // 0.8f
        public static float viscosity = 1.0f;
        public static float forceDrag = 5f;

        public float height;
        public float width;

        public Recorder recorder;
        public int frame;

        public List<Body> bodies = new List<Body>();
        public List<Vertex> vertices = new List<Vertex>();
        public List<Constraint> constraints = new List<Constraint>();
        public List<Constraint> globalConstraints = new List<Constraint>();
        public bool enableCollision = true;
        public bool softIntegration = true;
        public bool enableGravity = true;

        public World()
        {
        }

        public void Step(float dt)
        {
            recorder?.BeginFrame(dt);

            recorder?.BeginSolvingStep("beforeIntegrate", RecordinInfo.All());
            recorder?.EndSolvingStep();
            recorder?.BeginSolvingStep("integrate", RecordinInfo.BodyVertices());
            if (softIntegration)
            {
                SoftIntegrate(dt);
            }
            else
            {
                HardIntegrate(dt);
            }
            recorder?.EndSolvingStep();
            
            var stepCoef = 1.0f / numIterations;

            for (var i = 0; i < numIterations; ++i)
            {
                recorder?.BeginSolvingStep("constrain_solve_" + i, RecordinInfo.BodyVertices());
                Solve(dt, stepCoef);
                recorder?.EndSolvingStep();

                recorder?.BeginSolvingStep("boundingbox_" + i, RecordinInfo.BodyData());
                UpdateBoundingBox(dt);
                recorder?.EndSolvingStep();

                if (enableCollision)
                {
                    recorder?.BeginSolvingStep("collision_" + i, RecordinInfo.BodyVertices());
                    CollisionDetection(dt);
                    recorder?.EndSolvingStep();
                }
            }

            recorder?.BeginSolvingStep("boundsChecking", RecordinInfo.BodyVertices());
            BoundsChecking();
            recorder?.EndSolvingStep();

            recorder?.EndFrame();
            ++frame;
        }

        public void AddBody(Body body)
        {
            bodies.Add(body);
            foreach (var v in body.vertices)
            {
                vertices.Add(v);
            }
            foreach (var c in body.constraints)
            {
                constraints.Add(c);
            }

            body.UpdateBoundingBox();
        }

        public void AddGlobalConstraint(Constraint constraint)
        {
            globalConstraints.Add(constraint);
            constraints.Add(constraint);
        }

        public void Reset()
        {
            bodies.Clear();
            vertices.Clear();
            constraints.Clear();
            globalConstraints.Clear();
            frame = 0;
        }

        public Body GetBodyAt(Vector2 p)
        {
            for (var i = 0; i < bodies.Count; i++)
            {
                if (Collision.IsPointInsideBody(p, bodies[i]))
                {
                    return bodies[i];
                }
            }
            return null;
        }

        private void SoftIntegrate(float dt)
        {
            for (var i = 0; i < vertices.Count; ++i)
            {
                var v = vertices[i];
                var velocity = (v.position - v.oldPosition) * friction;

                // ground friction
                if (v.position.y >= height - 1 && velocity.sqrMagnitude > 0.000001)
                {
                    var m = velocity.magnitude;
                    velocity /= m;
                    velocity *= (m * frictionFloor);
                }

                // save last good state
                v.oldPosition = v.position;

                // gravity
                if (enableGravity)
                {
                    v.position.y += gravity;
                }

                // inertia  
                v.position += velocity;
            }
        }

        private void HardIntegrate(float dt)
        {
            for(var i = 0; i < vertices.Count; ++i)
            {
                var v = vertices[i];
                var x = v.position.x;
                var y = v.position.y;
                v.position.x += viscosity * x - viscosity * v.oldPosition.x;
                v.position.y += viscosity * y - viscosity * v.oldPosition.y;

                if (enableGravity)
                {
                    v.position.y += gravity;
                }

                v.oldPosition.Set(x, y);

                //
                // screen limits
                //
                if (v.position.y < 0)
                {
                    // Ceiling
                    v.position.y = 0;
                }
                else if (v.position.y > height)
                {
                    // Floor
                    v.position.x -= (v.position.y - height) * (v.position.x - v.oldPosition.x) * frictionFloor;
                    v.position.y = height;
                }

                if (v.position.x < 0)
                    v.position.x = 0;
                else if (v.position.x > width)
                    v.position.x = width;
            }
        }

        private void BoundsChecking()
        {
            for (var i = 0; i < vertices.Count; ++i)
            {
                var v = vertices[i];
                if (v.position.y > height - 1)
                    v.position.y = height - 1;

                if (v.position.y < 0)
                    v.position.y = 0;

                if (v.position.x < 0)
                    v.position.x = 0;

                if (v.position.x > width - 1)
                    v.position.x = width - 1;
            }
        }

        private void Solve(float dt, float stepCoef)
        {
            for (var i = 0; i < constraints.Count; ++i)
            {
                constraints[i].Solve(dt, stepCoef);
            }
        }

        private void UpdateBoundingBox(float dt)
        {
            for (var i = 0; i < bodies.Count; i++)
            {
                bodies[i].UpdateBoundingBox();
            }
        }

        private void CollisionDetection(float dt)
        {
            for (var i = 0; i < bodies.Count - 1; i++)
            {
                var b0 = bodies[i];
                for (var j = i + 1; j < bodies.Count; j++)
                {
                    var b1 = bodies[j];
                    if (Collision.CheckCollide(b0, b1))
                    {
                        Collision.Resolve();
                    }
                }
            }
        }
    }
}