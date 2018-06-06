using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniVer;

namespace UniVer
{
    public class DemoSpring : Demo
    {
        private Collision.ClosestInfo dragInfo;
        private DragConstraint dragConstraint;

        protected override void Init(World world)
        {
            dragConstraint = new DragConstraint();
            world.AddGlobalConstraint(dragConstraint);
            Spider(world);
        }

        protected override World CreateWorld()
        {
            var w = new World()
            {
                width = 200f,
                height = 200f,
                enableCollision = false
            };
            World.gravity = -0.2f;
            World.frictionFloor = 0.8f;
            World.friction = 1.0f;
            return w;
        }

        public override void OnGUI()
        {
            base.OnGUI();
            DragHandle();
        }

        private void DragHandle()
        {
            model.dragPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            model.dragPosition.y = height - model.dragPosition.y;

            if (Input.GetMouseButtonDown(0))
            {
                dragInfo = Collision.GetClosestVertex(world, model.dragPosition);
                if (dragInfo.body != null)
                {
                    model.draggedBody = dragInfo.body;
                    dragConstraint.Activate(dragInfo);
                }
            }
            else if (Input.GetMouseButton(0) && model.draggedBody != null)
            {
                dragConstraint.dragPosition = model.dragPosition;
            }
            else if (Input.GetMouseButtonUp(0) && model.draggedBody != null)
            {
                model.draggedBody = null;
                model.draggedVertex = null;
                dragConstraint.Deactivate();
            }
        }

        void SingleRectFalling2(World world)
        {
            // world.CreateRectangle(0, 2, 1, 2, 1);
            World.gravity = -0.2f;
            World.frictionFloor = 0.8f;
            World.friction = 1.0f;
            world.CreateRectangle(100, 100, 10, 10, 1);
        }

        void LineSegments(World world)
        {
            var segment = world.LineSegments(new[] {
                new Vertex(20, 10),
                new Vertex(40, 10),
                new Vertex(60, 10),
                new Vertex(80, 10),
                new Vertex(100, 10) }, 1f);

            world.Pins(segment, 0, 4);
        }

        void Shape1(World world)
        {
            world.Tire(new Vector2(50, 50), 30, 30, 0.3f, 0.9f);
        }

        void Shape2(World world)
        {
            world.Tire(new Vector2(50, 50), 30, 7, 0.1f, 0.2f);
            
        }

        void Shape3(World world)
        {
            world.Tire(new Vector2(50, 50), 30, 3, 1, 1);
        }

        void Cloth(World world)
        {
            var min = Mathf.Min(width, height) * 0.5f;
            world.Cloth(new Vector2(width / 2, height / 3), min, min, 20, 6, 0.9f);
        }

        void Spider(World world)
        {
            world.SpiderWeb(new Vector2(width / 2, height / 2), Mathf.Min(width, height) / 2, 20, 7);
        }
    }
}