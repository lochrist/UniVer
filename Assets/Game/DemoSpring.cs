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
        }

        protected override World CreateWorld()
        {
            var w = new World()
            {
                enableCollision = false
            };
            World.gravity = 0.2f;
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
            model.dragPosition = GetWorldPosition(Input.mousePosition);
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

        #region Demos
        [Demo]
        public void SingleRectFalling2(World world)
        {
            // world.CreateRectangle(0, 2, 1, 2, 1);
            World.gravity = -0.2f;
            World.frictionFloor = 0.8f;
            World.friction = 1.0f;
            world.CreateRectangle(100, 100, 10, 10, 1);
        }

        [Demo(true)]
        public void LineSegments(World world)
        {
            var segment = world.LineSegments(new[] {
                new Vertex(20, 10),
                new Vertex(60, 10),
                new Vertex(100, 10),
                new Vertex(140, 10),
                new Vertex(180, 10) }, 1f);

            world.Pins(segment, 0, 4);
        }

        [Demo]
        public void Shape1(World world)
        {
            world.Tire(new Vector2(200, 50), 50, 30, 0.3f, 0.9f);
        }

        [Demo]
        public void Shape2(World world)
        {
            world.Tire(new Vector2(400, 50), 70, 7, 0.1f, 0.2f);
        }

        [Demo]
        public void Shape3(World world)
        {
            world.Tire(new Vector2(600, 50), 70, 3, 1, 1);
        }

        [Demo]
        public void Shapes(World world)
        {
            LineSegments(world);
            Shape1(world);
            Shape2(world);
            Shape3(world);
        }

        [Demo]
        public void Cloth(World world)
        {
            var min = Mathf.Min(world.width, world.height) * 0.5f;
            world.Cloth(new Vector2(world.width / 2, world.height / 3), min, min, 20, 6, 0.9f);
        }

        [Demo]
        public void Spider(World world)
        {
            world.SpiderWeb(new Vector2(world.width / 2, world.height / 2), Mathf.Min(world.width, world.height) / 2, 20, 7);
        }

        [Demo]
        public void Tree(World world)
        {
            World.gravity = 0;
            World.friction = 0.98f;
            world.Tree(new Vector2(100, 180), 5, 35, 0.95f, (Mathf.PI / 2) / 3);
        }
        #endregion
    }
}