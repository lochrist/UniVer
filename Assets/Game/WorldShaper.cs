using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GraphicDNA;

namespace UniVer {
    public class WorldShaper : WorldRenderer
    {
        private Rect mParentRect;
        private Rect worldRect;

        public override void BindModel(InteractiveModel model)
        {
            base.BindModel(model);
            worldRect = new Rect(DemoUtils.offset.x, DemoUtils.offset.y, world.width, world.height);
        }

        private void OnGUI()
        {
            Render();
        }

        public void OnRenderObject()
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

        public override void DrawWorldBounds()
        {
            Drawing2D.DrawRect(worldRect, DemoUtils.worldBoundsColor);
        }

        public override void DrawBody(Body body)
        {
            switch (body.tag)
            {
                case Tags.NormalBody:
                    foreach (var c in body.constraints)
                        DrawConstraint(c);

                    foreach (var v in body.vertices)
                        DrawVertex(v);

                    break;
                case Tags.Cloth:
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
                    Drawing2D.FillCircle(pin.position + DemoUtils.offset, 1f, DemoUtils.pinColor);
                    break;
                case Tags.AngleConstraint:
                    break;
            }
        }

        private void DrawVertex(Vertex v)
        {
            Drawing2D.FillCircle(v.position + DemoUtils.offset, 3f, DemoUtils.vertexColor);
        }

        public override void DrawDragConstraint()
        {
            if (model.draggedBody == null || model.draggedVertex == null || model.dragPosition == null)
                return;

            Drawing2D.DrawLine(model.draggedVertex.position + DemoUtils.offset, model.dragPosition + DemoUtils.offset, DemoUtils.dragConstraintColor);
        }
    }

}