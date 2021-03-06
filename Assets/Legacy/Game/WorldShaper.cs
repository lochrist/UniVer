﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GraphicDNA;

namespace UniVer.Legacy {
    public class WorldShaper : WorldRenderer
    {
        private Rect mParentRect;
        private Rect worldRect;

        private void OnGUI()
        {
            Render();
        }

        private void OnRenderObject()
        {
        }

        private void Render()
        {
            // Set the 0,0 at the top-left corner of this panel
            mParentRect = Drawing2D.GetWorldRect(this.transform as RectTransform);
            Drawing2D.SetParentBounds(mParentRect);

            if (Event.current.type == EventType.Repaint)
            {
                RenderWorld();
            }

            Drawing2D.ClearParentBounds();
        }

        #region Wolrd Draw API
        public override void BindModel(InteractiveModel model)
        {
            base.BindModel(model);
        }

        public override void DrawWorldBounds()
        {
            var worldRect = new Rect(DemoUtils.offset.x, DemoUtils.offset.y, world.width, world.height);
            Drawing2D.DrawRect(worldRect, DemoUtils.worldBoundsColor);
        }

        public override void DrawBody(Body body)
        {
            switch (body.tag)
            {
                case Tags.Cloth:
                    break;
                case Tags.SolidBody:
                    DrawSolidBody(body);
                    break;
                case Tags.Tree:
                    Drawing2D.DrawLine(body.vertices[0].position + DemoUtils.offset, body.vertices[1].position + DemoUtils.offset, DemoUtils.constraintColor);
                    DrawNormalBody(body);
                    break;
                case Tags.TreeWithFoliage:
                    var desc = body.data as Builders.TreeFoliageDesc;
                    for (var i = 0; i < desc.branches.Count; ++i)
                    {
                        if (desc.branches[i] < 0)
                            continue;

                        var constraint = body.constraints[i] as SpringConstraint;
                        var trunkWidth = Mathf.Lerp(10f, 2f, desc.branches[i]);
                        Drawing2D.DrawLine(constraint.v0.position + DemoUtils.offset, constraint.v1.position + DemoUtils.offset, DemoUtils.trunkColor, trunkWidth);
                    }

                    for(var i = 0; i < desc.leaves.Count; ++i)
                    {
                        var vertex = body.vertices[desc.leaves[i]];
                        Drawing2D.FillCircle(vertex.position + DemoUtils.offset, 25, DemoUtils.foliageColor);
                    }

                    break;
                case Tags.NormalBody:
                default:
                    DrawNormalBody(body);
                    break;
            }
        }

        public override void DrawConstraint(Constraint c)
        {
            switch (c.tag)
            {
                case Tags.DistanceConstraint:
                    var distance = c as DistanceConstraint;
                    Drawing2D.DrawLine(distance.v0.position + DemoUtils.offset, distance.v1.position + DemoUtils.offset, DemoUtils.constraintColor);
                    break;
                case Tags.SpringConstraint:
                    var spring = c as SpringConstraint;
                    Drawing2D.DrawLine(spring.v0.position + DemoUtils.offset, spring.v1.position + DemoUtils.offset, DemoUtils.constraintColor);
                    break;
                case Tags.DragConstraint:
                    break;
                case Tags.PinConstraint:
                    var pin = c as PinConstraint;
                    Drawing2D.FillCircle(pin.position + DemoUtils.offset, DemoUtils.pinSize, DemoUtils.pinColor);
                    break;
                case Tags.AngleConstraint:
                    break;
            }
        }

        public override void DrawDragConstraint()
        {
            if (model.draggedVertex != null)
            {
                if (model.draggedBody == null)
                {
                    Drawing2D.DrawLine(model.draggedVertex.position + DemoUtils.offset, model.dragPosition + DemoUtils.offset, DemoUtils.dragConstraintColor);
                }

                Drawing2D.DrawCircle(model.draggedVertex.position + DemoUtils.offset, DemoUtils.pinSize, 8, DemoUtils.dragConstraintColor);
            }
        }
        #endregion

        private void DrawNormalBody(Body body)
        {
            foreach (var c in body.constraints)
                DrawConstraint(c);

            foreach (var v in body.vertices)
                DrawVertex(v);
        }

        private void DrawSolidBody(Body body)
        {
            switch (body.vertices.Length)
            {
                case 3:
                    Drawing2D.FillTriangle(
                        body.vertices[0].position + DemoUtils.offset,
                        body.vertices[1].position + DemoUtils.offset,
                        body.vertices[2].position + DemoUtils.offset,
                        DemoUtils.triangleColor
                        );
                    DrawBodyOutline(body);
                    break;
                case 4:
                    Drawing2D.FillTriangle(
                        body.vertices[0].position + DemoUtils.offset,
                        body.vertices[1].position + DemoUtils.offset,
                        body.vertices[2].position + DemoUtils.offset,
                        DemoUtils.rectangleColor
                        );
                    Drawing2D.FillTriangle(
                        body.vertices[2].position + DemoUtils.offset,
                        body.vertices[3].position + DemoUtils.offset,
                        body.vertices[0].position + DemoUtils.offset,
                        DemoUtils.rectangleColor
                        );
                    DrawBodyOutline(body);
                    break;
                default:
                    DrawNormalBody(body);
                    break;
            }
        }

        private void DrawBodyOutline(Body body)
        {
            for(var i = 0; i < body.vertices.Length - 1; ++i)
            {
                var v0 = body.vertices[i];
                var v1 = body.vertices[i + 1];
                Drawing2D.DrawLine(v0.position + DemoUtils.offset, v1.position + DemoUtils.offset, DemoUtils.constraintColor);
            }

            Drawing2D.DrawLine(body.vertices[body.vertices.Length - 1].position + DemoUtils.offset, body.vertices[0].position + DemoUtils.offset, DemoUtils.constraintColor);
        }

        private void DrawVertex(Vertex v)
        {
            Drawing2D.FillCircle(v.position + DemoUtils.offset, DemoUtils.vertexSize, DemoUtils.vertexColor);
        }
    }

}