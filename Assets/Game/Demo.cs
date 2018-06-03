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

    public class Demo : MonoBehaviour
    {
        public float width = 200f;
        public float height = 200f;
        public bool live = true;
        public bool gravity = true;
        public WorldRenderer worldRenderer;

        Camera mainCamera;
        InteractiveModel model;
        World world;

        // Use this for initialization
        void Start()
        {
            world = new World
            {

                width = width,
                height = height

                // width = 12f,
                // height = 9f
            };
            model = new InteractiveModel(world);
            worldRenderer.BindModel(model);
            mainCamera = GetComponent<Camera>();

            Init(world);
        }

        void Init(World world)
        {
            SingleRectFalling2(world);
        }

        void SingleRectFalling(World world)
        {
            // world.CreateRectangle(0, 2, 1, 2, 1);
            world.CreateRectangle(0, 0, 10, 10, 1);
        }

        void SingleRectFalling2(World world)
        {
            // world.CreateRectangle(0, 2, 1, 2, 1);
            World.gravity = -0.2f;
            World.frictionFloor = 0.8f;
            World.friction = 1.0f;
            world.CreateRectangle(100, 100, 10, 10, 1);
        }

        void RectOnFloor(World world)
        {
            // world.CreateRectangle(0, 2, 1, 2, 1);
            World.gravity = -0.3f;
            world.CreateRectangle(100, 189, 10, 10, 1);
        }

        void TwoBodies(World world)
        {
            world.CreateRectangle(50, 170, 4, 4);

            world.CreateRectangle(40, 180, 32, 4);
        }

        void FourRectsColliding(World world)
        {
            var s = 4.57f;
            world.CreateRectangle(77f, 74f, s, s, 1);
            world.CreateRectangle(77f, 80f, s, s, 1);

            world.CreateRectangle(117f, 85, s, s, 1);
            world.CreateRectangle(117f, 91f, s, s, 1);
        }

        void TriangleCollision(World world)
        {
            var w = width / 35f;
            var h = height / 2 - 4.5f * w;
            world.CreateTriangle(w * 7.5f, h + w * 11, w * 10, w * 2, 20);

            var triangleHeight = height - w * 1.8f + 0.4f * w * 0.5f;
            var triSize = w * 4;
            world.CreateTriangle(
                width / 4,
                triangleHeight,
                triSize,
                triSize,
                10
            );
        }

        void CodePenDemo(World world)
        {
            var codepen = new []
            {
                "             *                    ",
                "             *                    ",
                "**** **** **** **** **** **** ****",
                "*    *  * *  * *  * *  * *  * *  *",
                "*    *  * *  * **** *  * **** *  *",
                "*    *  * *  * *    *  * *    *  *",
                "**** **** **** **** **** **** *  *",
                "                    *             ",
                "                    *             "
            };

            var w = width / 35f;
            var h = height / 2 - 4.5f * w;
            var rectSize = w * 0.8f;
            for (var i = 0; i < codepen.Length; i++)
            {
                var line = codepen[i];
                for (var j = 0; j < line.Length; j++)
                {
                    var c = line[j];
                    if (c != ' ')
                        world.CreateRectangle(w * 0.5f + w * j, h + w * i, rectSize, rectSize);
                }
            }

            world.CreateTriangle(w * 7.5f, h + w * 11, w * 10, w * 2, 20);
            world.CreateTriangle(w * 27.5f, h + w * 11, w * 10, w * 2, 20);

            var triangleHeight = height - w * 1.8f + 0.4f * w * 0.5f;
            var triSize = w * 4;
            world.CreateTriangle(
                width / 2,
                triangleHeight,
                triSize,
                triSize,
                10
            );
            
            world.CreateTriangle(
                width / 4,
                triangleHeight,
                triSize,
                triSize,
                10
            );
            
        }

        public void OnGUI()
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

            DragHandle();
        }

        private void Update()
        {
            // Debug.Log("Update");
        }

        private void OnPostRender()
        {
            // Debug.Log("OnPostRender");
        }

        private void DragHandle()
        {
            model.dragPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            model.dragPosition.y = height - model.dragPosition.y;

            if (Input.GetMouseButtonDown(0))
            {
                var body = world.GetBodyAt(model.dragPosition);
                if (body != null)
                {
                    model.draggedBody = body;
                    model.draggedVertex = Collision.GetClosestVertex(body.vertices, model.dragPosition);
                }
            }
            else if (Input.GetMouseButton(0) && model.draggedBody != null)
            {
                var s = model.draggedBody.mass * World.forceDrag;
                model.draggedVertex.position += ((Vector2)model.dragPosition - model.draggedVertex.position) / s;
            }
            else if (Input.GetMouseButtonUp(0) && model.draggedBody != null)
            {
                model.draggedBody = null;
                model.draggedVertex = null;
            }
        }

        private void FixedUpdate()
        {
            if (live)
            {
                // Debug.Log("FixedUpdate");
                World.gravity = gravity ? 0.1f : 0.0f;
                world.Step(Time.deltaTime);
            }
        }
    }

}