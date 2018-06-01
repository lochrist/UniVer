using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Experimental.AI;

namespace UniVer
{
    public struct RecordinInfo
    {
        public bool recordBody;
        public bool bodyVertices;
        public bool bodyData;
        public bool recordConstraints;

        public static RecordinInfo All()
        {
            return new RecordinInfo()
            {
                recordBody = true,
                bodyVertices = true,
                bodyData = true,
                recordConstraints = true
            };
        }

        public static RecordinInfo BodyVertices()
        {
            return new RecordinInfo()
            {
                recordBody = true,
                bodyVertices = true,
                bodyData = false,
                recordConstraints = false
            };
        }

        public static RecordinInfo BodyData()
        {
            return new RecordinInfo()
            {
                recordBody = true,
                bodyVertices = false,
                bodyData = true,
                recordConstraints = false
            };
        }
    }

    [Serializable]
    public class VertexInfo
    {
        public double x;
        public double y;
        public double ox;
        public double oy;
    }

    [Serializable]
    public class Vector2d
    {
        public Vector2d(Vector2 v)
        {
            x = v.x;
            y = v.y;
        }

        public double x;
        public double y;
    }

    [Serializable]
    public class BodyInfo
    {
        public int id;
        public List<VertexInfo> vertices;
        public double mass;
        public double min;
        public double max;
        public Vector2d center;
        public Vector2d halfExtent;

        public void CleanData()
        {
            mass = Math.Round(mass, 4);
            min = Math.Round(mass, 4);
            max = Math.Round(mass, 4);
            if (center != null)
            {
                center.x = Math.Round(center.x, 4);
                center.y = Math.Round(center.y, 4);
            }

            if (halfExtent != null)
            {
                halfExtent.x = Math.Round(halfExtent.x, 4);
                halfExtent.y = Math.Round(halfExtent.y, 4);
            }

            if (vertices != null)
            {
                foreach (var vertex in vertices)
                {
                    vertex.x = Math.Round(vertex.x, 4);
                    vertex.y = Math.Round(vertex.y, 4);
                    vertex.ox = Math.Round(vertex.ox, 4);
                    vertex.oy = Math.Round(vertex.oy, 4);
                }
            }
        }
    }

    [Serializable]
    public class Step
    {
        public string name;
        public List<BodyInfo> bodies;

        public void CleanData()
        {
            foreach (var body in bodies)
            {
                body.CleanData();
            }
        }
    }

    [Serializable]
    public class Frame
    {
        public int frame;
        public List<Step> steps;

        public void CleanData()
        {
            foreach (var step in steps)
            {
                step.CleanData();
            }
        }
    }

    [Serializable]
    public class RecordingData
    {
        public List<Frame> frames;
        public double gravity;
        public double friction;
        public double frictionFloor;
        public double viscosity;
        public double forceDrag;

        public void Write(string filePath)
        {
            foreach (var frame in frames)
            {
                frame.CleanData();
            }

            var content = JsonUtility.ToJson(this, true);
            File.WriteAllText(filePath, content);
        }

        public static RecordingData Read(string path)
        {
            var content = File.ReadAllText(path);
            return JsonUtility.FromJson<RecordingData>(content);
        }
    }

    public class Recorder
    {
        public RecordinInfo recInfo;
        public World world;
        public int frame = 0;
        public RecordingData data;
        public Frame currentFrame;
        public Step currentStep;

        public Recorder(World world)
        {
            this.world = world;
            data = new RecordingData()
            {
                frames = new List<Frame>(),
                gravity = World.gravity,
                friction = World.friction,
                frictionFloor = World.frictionFloor,
                viscosity = World.viscosity,
                forceDrag = World.forceDrag
            };
        }

        public void BeginFrame(float dt)
        {
            currentFrame = new Frame {frame = frame++, steps = new List<Step>()};
            data.frames.Add(currentFrame);
        }

        public void EndFrame()
        {
        }

        public void BeginSolvingStep(string name, RecordinInfo recInfo)
        {
            this.recInfo = recInfo;
            currentStep = new Step {name = name, bodies = new List<BodyInfo>()};
            currentFrame.steps.Add(currentStep);
        }

        public void EndSolvingStep()
        {
            RecordWorld();
        }

        public void RecordWorld()
        {
            if (recInfo.recordBody)
            {
                currentStep.bodies = new List<BodyInfo>();
                for (var i = 0; i < world.bodies.Count; ++i)
                {
                    var body = world.bodies[i];
                    var bodyInfo = new BodyInfo {id = i};
                    currentStep.bodies.Add(bodyInfo);
                    if (recInfo.bodyVertices)
                    {
                        bodyInfo.vertices = body.vertices.Select(v => new VertexInfo()
                        {
                            x = v.position.x, y = v.position.y, ox = v.oldPosition.x, oy = v.oldPosition.y
                        }).ToList();
                    }

                    if (recInfo.bodyData)
                    {
                        bodyInfo.mass = body.mass;
                        bodyInfo.min = body.min;
                        bodyInfo.max = body.max;
                        bodyInfo.center = new Vector2d(body.center);;
                        bodyInfo.halfExtent = new Vector2d(body.halfExtent);
                    }
                }
            }
        }
    }
}