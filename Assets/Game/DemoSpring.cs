using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniVer;

namespace UniVer
{
    public class DemoSpring : Demo
    {
        protected override void Init(World world)
        {
            SingleRectFalling2(world);
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
            
        }

        void SingleRectFalling2(World world)
        {
            // world.CreateRectangle(0, 2, 1, 2, 1);
            World.gravity = -0.2f;
            World.frictionFloor = 0.8f;
            World.friction = 1.0f;
            world.CreateRectangle(100, 100, 10, 10, 1);
        }
    }
}