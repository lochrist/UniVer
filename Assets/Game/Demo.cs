using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UniVer
{
    public abstract class WorldRenderer : MonoBehaviour
    {
        public abstract void BindModel(InteractiveModel model);
    }

    public class InteractiveModel
    {
        public InteractiveModel(World world)
        {
            this.world = world;
        }
        public World world;
        public Body draggedBody;
        public Vertex draggedVertex;
        public Vector2 dragPosition;
    }

    public abstract class Demo : MonoBehaviour
    {
        public float width = 200f;
        public float height = 200f;
        public bool live = true;
        public bool gravity = true;
        public WorldRenderer worldRenderer;

        protected Camera mainCamera;
        protected InteractiveModel model;
        protected World world;

        // Use this for initialization
        protected virtual void Start()
        {
            world = CreateWorld();
            model = new InteractiveModel(world);
            worldRenderer.BindModel(model);
            mainCamera = GetComponent<Camera>();

            Init(world);
        }

        protected abstract World CreateWorld();

        protected abstract void Init(World world);

        public virtual void OnGUI()
        {
            // Debug.Log("OnGui");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Step"))
            {
                world.Step(Time.deltaTime);
                Debug.Log(world.bodies[0].vertices[0].position);
            }

            if (GUILayout.Button("Reset"))
            {
                world.Reset();
                Init(world);
            }

            if (GUILayout.Button("Jump"))
            {
                world.bodies[0].vertices[0].position += new Vector2(20, -20);
                /*
                world.bodies[0].vertices[1].position += new Vector2(20, -20);
                world.bodies[0].vertices[2].position += new Vector2(20, -20);
                world.bodies[0].vertices[3].position += new Vector2(20, -20);
                */
            }

            GUILayout.Label("Frame: " + world.frame);
            GUILayout.EndHorizontal();
        }

        private void FixedUpdate()
        {
            if (live)
            {
                // Debug.Log("FixedUpdate");
                // World.gravity = gravity ? 0.1f : 0.0f;
                world.Step(Time.deltaTime);
            }
        }
    }
}