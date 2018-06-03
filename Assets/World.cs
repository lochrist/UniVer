using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniVer
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
        public static float gravity = 0.1f;
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
        public bool enableCollision = true;

        public World()
        {
        }

        public void Step(float dt)
        {
            if (recorder != null)
            {
                recorder.BeginFrame(dt);
                recorder.BeginSolvingStep("beforeIntegrate", RecordinInfo.All());
                recorder.EndSolvingStep();
                recorder.BeginSolvingStep("integrate", RecordinInfo.BodyVertices());
            }

            Integrate(dt);

            if (recorder != null)
                recorder.EndSolvingStep();

            var stepCoef = 1 / numIterations;

            for (var i = 0; i < numIterations; ++i)
            {
                if (recorder != null)
                    recorder.BeginSolvingStep("constrain_solve_" + i, RecordinInfo.BodyVertices());
                Solve(dt, stepCoef);

                if (recorder != null)
                    recorder.EndSolvingStep();

                if (recorder != null)
                    recorder.BeginSolvingStep("boundingbox_" + i, RecordinInfo.BodyData());
                UpdateBoundingBox(dt);

                if (recorder != null)
                    recorder.EndSolvingStep();

                if (enableCollision)
                {
                    if (recorder != null)
                        recorder.BeginSolvingStep("collision_" + i, RecordinInfo.BodyVertices());
                    CollisionDetection(dt);
                    if (recorder != null)
                        recorder.EndSolvingStep();
                }
            }

            if (recorder != null)
            {
                recorder.BeginSolvingStep("boundsChecking", RecordinInfo.BodyVertices());
            }

            BoundsChecking();

            if (recorder != null)
            {
                recorder.EndSolvingStep();
                recorder.EndFrame();
            }
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

        public void Reset()
        {
            bodies.Clear();
            vertices.Clear();
            constraints.Clear();
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

        private void Integrate(float dt)
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
                v.position.y += gravity;

                // inertia  
                v.position += velocity;
            }
        }

        private void IntegrateOld(float dt)
        {
            for(var i = 0; i < vertices.Count; ++i)
            {
                var v = vertices[i];
                var x = v.position.x;
                var y = v.position.y;

                // TODO: check if this is the right place to add gravity.
                v.position.x += viscosity * x - viscosity * v.oldPosition.x;
                v.position.y += viscosity * y - viscosity * v.oldPosition.y + gravity;

                // TODO: check if that actually works
                v.oldPosition.Set(x, y);

                //
                // screen limits
                //
                if (v.position.y < 0)
                {
                    v.position.y = 0;
                }
                else if (v.position.y > height)
                {
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