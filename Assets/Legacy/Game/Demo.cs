using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniVer.Legacy
{
    public abstract class WorldRenderer : MonoBehaviour
    {
        public World world;
        public InteractiveModel model;

        public virtual void BindModel(InteractiveModel model)
        {
            this.model = model;
            world = model.world;
        }

        public void RenderWorld()
        {
            DrawWorldBounds();

            foreach (var body in world.bodies)
                DrawBody(body);

            foreach (var c in world.globalConstraints)
                DrawConstraint(c);

            DrawDragConstraint();
        }

        public abstract void DrawBody(Body body);
        public abstract void DrawWorldBounds();
        public abstract void DrawConstraint(Constraint c);
        public abstract void DrawDragConstraint();
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

    public class DemoAttribute : Attribute
    {
        public bool isDefault;
        public DemoAttribute(bool isDefault = false)
        {
            this.isDefault = isDefault;
        }
    }

    public class DemoDesc
    {
        public string name;
        public Action<World> createDemo;
        public bool isDefault;
    }

    public abstract class Demo : MonoBehaviour
    {
        public bool live = true;
        public WorldRenderer worldRenderer;
        protected Camera mainCamera;
        protected InteractiveModel model;
        protected World world;
        protected List<DemoDesc> demos;
        protected int currentDemoIndex;

        // Use this for initialization
        protected virtual void Start()
        {

            mainCamera = GetComponent<Camera>();
            mainCamera.backgroundColor = DemoUtils.backgroundColor;

            demos = GetType().GetMethods().Select(m =>
            {
                var attrs = m.GetCustomAttributes(false).OfType<DemoAttribute>();
                if (attrs.Any())
                {
                    return new DemoDesc()
                    {
                        name = m.Name,
                        createDemo = world => m.Invoke(this, new[] { world }),
                        isDefault = attrs.First().isDefault
                    };
                }
                return null;
            }).Where(v => v != null).ToList();
            if (demos.Count == 0)
            {
                throw new Exception("No demo declared");
            }

            var defaultIndex = demos.FindIndex(d => d.isDefault);
            if (defaultIndex != -1)
            {
                currentDemoIndex = defaultIndex;
            }

            InitDemo();
        }

        protected abstract World CreateWorld();

        protected abstract void Init(World world);

        protected Vector2 GetWorldPosition(Vector2 screenPos)
        {
            screenPos -= DemoUtils.offset;
            screenPos.y = world.height - screenPos.y;
            return screenPos;
        }

        public virtual void OnGUI()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Prev"))
            {
                currentDemoIndex--;
                if (currentDemoIndex == -1)
                    currentDemoIndex = demos.Count - 1;
                InitDemo();
            }
            GUILayout.Label(demos[currentDemoIndex].name);
            if (GUILayout.Button("Next"))
            {
                currentDemoIndex++;
                if (currentDemoIndex == demos.Count)
                    currentDemoIndex = 0;
                InitDemo();
            }

            GUILayout.Space(20);
            if (GUILayout.Button("Step"))
            {
                world.Step(Time.deltaTime);
                Debug.Log(world.bodies[0].vertices[0].position);
            }

            if (GUILayout.Button("Reset"))
            {
                ResetWorld();
            }

            world.enableGravity = GUILayout.Toggle(world.enableGravity, "Gravity");
            
            GUILayout.Label("Frame: " + world.frame);
            GUILayout.EndHorizontal();
        }

        private void InitDemo()
        {
            world = CreateWorld();
            model = new InteractiveModel(world);
            worldRenderer.BindModel(model);

            ResetWorld();
        }

        private void ResetWorld()
        {
            world.Reset();
            world.height = Screen.height - (2 * DemoUtils.margin);
            world.width = Screen.width - (2 * DemoUtils.margin);

            Init(world);
            demos[currentDemoIndex].createDemo(world);
        }

        private void FixedUpdate()
        {
            if (live)
            {
                world.Step(Time.deltaTime);
            }
        }
    }
}