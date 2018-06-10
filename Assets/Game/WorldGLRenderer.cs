using GraphicDNA;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniVer
{
    public class WorldGLRenderer : WorldRenderer
    {
        private Dictionary<int, Action<Body>> bodyRenderers;
        private Dictionary<int, Action<Constraint>> constraintRenderers;

        static Material lineMaterial;
        static void CreateLineMaterial()
        {
            if (!lineMaterial)
            {
                // Unity has a built-in shader that is useful for drawing
                // simple colored things.
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                lineMaterial = new Material(shader);
                lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                // Turn on alpha blending
                lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                // Turn backface culling off
                lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                // Turn off depth writes
                lineMaterial.SetInt("_ZWrite", 0);
            }
        }

        // Will be called after all regular rendering is done
        public void OnRenderObject()
        {
            RenderWorld();
            // RenderWorldShaper();
        }

        private void RenderWorldShaper()
        {
            Drawing2D.ScreenWidth = world.width;
            Drawing2D.ScreenHeight = world.height;
            Drawing2D.DrawRect(new Rect(0, 0, world.width, world.height), DemoUtils.ToColor(0, 220, 20));

            foreach (var body in world.bodies)
            {
                Drawing2D.DrawPolygon(body.vertices.Select(v => v.position).ToArray(), DemoUtils.ToColor(220, 20, 20));
            }
        }

        private void RenderWorldGL()
        {
            CreateLineMaterial();
            // Apply the line material
            lineMaterial.SetPass(0);

            GL.PushMatrix();
            // Set transformation matrix for drawing to
            // match our transform
            // GL.MultMatrix(transform.localToWorldMatrix);

            RenderWorld();

            GL.PopMatrix();
        }

        public override void DrawWorldBounds()
        {
            GL.Begin(GL.LINE_STRIP);

            GL.Color(new Color(0, 255, 0));
            
            GL.Vertex3(0, 0, 0);

            GL.Vertex3(world.width, 0, 0);

            GL.Vertex3(world.width, world.height, 0);

            GL.Vertex3(0, world.height, 0);

            GL.Vertex3(0, 0, 0);

            GL.End();
        }

        public override void DrawBody(Body body)
        {
            switch(body.tag)
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

        public override void DrawDragConstraint()
        {
            if (model.draggedBody == null || model.draggedVertex == null)
                return;

            GL.Begin(GL.LINE_STRIP);

            GL.Color(Color.green);

            GL.Vertex3(model.draggedVertex.position.x, world.height - model.draggedVertex.position.y, 0);
            GL.Vertex3(model.dragPosition.x, world.height - model.dragPosition.y, 0);

            GL.End();
        }

        public override void DrawConstraint(Constraint c)
        {
            switch (c.tag)
            {
                case Tags.DistanceConstraint:
                    var distance = c as DistanceConstraint;
                    DrawLine(distance.v0.position, distance.v1.position, Color.magenta);
                    break;
                case Tags.SpringConstraint:
                    var spring = c as SpringConstraint;
                    DrawLine(spring.v0.position, spring.v1.position, Color.magenta);
                    break;
                case Tags.DragConstraint:
                    break;
                case Tags.PinConstraint:
                    break;
                case Tags.AngleConstraint:
                    break;
            }
        }

        private void DrawVertex(Vertex v)
        {

        }

        private void GLVertex(float x, float y)
        {
            GL.Vertex3(x, world.height - y, 0);
        }

        private void DrawLine(Vector2 v0, Vector2 v1, Color c)
        {
            GL.Begin(GL.LINES);
            GL.Color(c);
            GLVertex(v0.x, v0.y);
            GLVertex(v1.x, v1.y);

            GL.End();
        }
    }

}