﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniVer;

namespace UniVer
{
    public class DemoSpring : Demo
    {
        private Collision.ClosestInfo dragInfo;

        protected override void Init(World world)
        {
            LineSegments(world);
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
                }
            }
            else if (Input.GetMouseButton(0) && model.draggedBody != null)
            {
                if (dragInfo.pin != null)
                {
                    dragInfo.pin.position = model.dragPosition;
                }
                else
                {
                    dragInfo.v.position = model.dragPosition;
                }
            }
            else if (Input.GetMouseButtonUp(0) && model.draggedBody != null)
            {
                model.draggedBody = null;
                model.draggedVertex = null;
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
    }
}