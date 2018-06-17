using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Schema;
using NUnit.Framework;
using UnityEngine;

namespace UniVer.Legacy
{
    public static class TestUtils {
        public static string folder = "Assets/Uniphys/Verl3t/Editor/TestData";
        public static string numberComparer = "{0:0.###}";
        public static double maxDiff = 0.02;
    }

    public class SolveTests
    {
        private static void Validate(string name, double f1, double f2)
        {
            if (Math.Abs(f1 - f2) > TestUtils.maxDiff)
            {
                throw new Exception(string.Format("{0} ({1} != {2})", name, string.Format(TestUtils.numberComparer, f1), string.Format(TestUtils.numberComparer, f2)));
            }
        }

        private static void Validate(string name, Vector2d f1, Vector2d f2)
        {
            Validate(name + " - x ", f1.x, f2.x);
            Validate(name + " - y ", f1.y, f2.y);
        }

        private static void Validate(RecordingData refData, RecordingData dstData)
        {
            if (refData.frames.Count != dstData.frames.Count)
            {
                throw new Exception("Not same number of frame");
            }

            for (var frameIndex = 0; frameIndex < dstData.frames.Count; ++frameIndex)
            {
                var refFrame = refData.frames[frameIndex];
                var dstFrame = dstData.frames[frameIndex];

                if (refFrame.steps.Count != dstFrame.steps.Count)
                {
                    throw new Exception("Not same number of steps in frame: " + frameIndex);
                }

                for (var stepIndex = 0; stepIndex < refFrame.steps.Count; ++stepIndex)
                {
                    var refStep = refFrame.steps[stepIndex];
                    var dstStep = dstFrame.steps[stepIndex];

                    Assert.AreEqual(refStep.name, dstStep.name);
                    if (refStep.bodies.Count != dstStep.bodies.Count)
                    {
                        throw new Exception(string.Format("Not same number of bodies in frame: {0} and step {1}",
                            frameIndex, refStep.name));
                    }

                    for (var bodyIndex = 0; bodyIndex < refStep.bodies.Count; ++bodyIndex)
                    {
                        var refBody = refStep.bodies[bodyIndex];
                        var dstBody = dstStep.bodies[bodyIndex];

                        Assert.AreEqual(refBody.id, dstBody.id);

                        var msg = string.Format("Not same value in frame: {0} and step {1} and body {2}", frameIndex,
                            refStep.name, refBody.id);
                        Validate(msg + " - mass", refBody.mass, dstBody.mass);
                        Validate(msg + " - min", refBody.min, dstBody.min);
                        Validate(msg + " - max", refBody.max, dstBody.max);
                        Validate(msg + " - center", refBody.center, dstBody.center);
                        Validate(msg + " - halfExtent", refBody.halfExtent, dstBody.halfExtent);

                        if (dstBody.vertices == null && refBody.vertices != null && refBody.vertices.Count > 0)
                        {
                            throw new Exception(string.Format(
                                "Not same number of vertices in frame: {0} and step {1} and body {2}", frameIndex,
                                refStep.name, refBody.id));
                        }

                        if (dstBody.vertices != null)
                        {
                            if (refBody.vertices.Count != dstBody.vertices.Count)
                            {
                                throw new Exception(string.Format(
                                    "Not same number of vertices in frame: {0} and step {1} and body {2}", frameIndex,
                                    refStep.name, refBody.id));
                            }

                            for (var vertexIndex = 0; vertexIndex < dstBody.vertices.Count; ++vertexIndex)
                            {
                                var refVertex = refBody.vertices[vertexIndex];
                                var dstVertex = dstBody.vertices[vertexIndex];

                                var vertextMsg = msg + " - vertices " + vertexIndex;
                                Validate(vertextMsg + " - x ", refVertex.x, dstVertex.x);
                                Validate(vertextMsg + " - y ", refVertex.y, dstVertex.y);
                                Validate(vertextMsg + " - ox ", refVertex.ox, dstVertex.ox);
                                Validate(vertextMsg + " - oy ", refVertex.oy, dstVertex.oy);
                            }
                        }
                    }
                }
            }
        }

        private void ValidateWithSrcData(World world, int nbFrames, string testDataBaseName)
        {
            for (var i = 0; i < nbFrames; ++i)
            {
                world.Step(0.16f);
            }

            var baseDataFile = TestUtils.folder + "/" + testDataBaseName;
            if (!File.Exists(baseDataFile + "_src_sharp.txt"))
            {
                var srcData = RecordingData.Read(baseDataFile + "_src_js.txt");
                srcData.Write(baseDataFile + "_src_sharp.txt");
            }

            var refData = RecordingData.Read(baseDataFile + "_src_sharp.txt");
            world.recorder.data.Write(baseDataFile + "_dst_sharp.txt");
            Validate(refData, world.recorder.data);
        }

        World CreateWorld()
        {
            var world = new World();
            World.gravity = 0.0f;
            world.width = 200;
            world.height = 200;
            world.recorder = new Recorder(world);
            return world;
        }

        [Test]
        public void SingleObjectMoveWithGravity()
        {
            var w = CreateWorld();
            World.gravity = 0.1f;
            
            w.CreateRectangle(0, 0, 10, 10, 1);

            ValidateWithSrcData(w, 200, "single_body_data");
        }

        [Test]
        public void TriangleCollision()
        {
            var world = CreateWorld();
            World.gravity = 0.0f;
            
            var w = world.width / 35f;
            var h = world.height / 2 - 4.5f * w;
            world.CreateTriangle(w * 7.5f, h + w * 11, w * 10, w * 2, 20);

            var triangleHeight = world.height - w * 1.8f + 0.4f * w * 0.5f;
            var triSize = w * 4;
            world.CreateTriangle(
                world.width / 4,
                triangleHeight,
                triSize,
                triSize,
                10
            );

            
            ValidateWithSrcData(world, 100, "triangle_collision");
        }

    }
}