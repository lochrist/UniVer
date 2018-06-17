using System;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework.Internal;

namespace UniVer.Legacy
{
    [Serializable]
    public class JsonTestData
    {
        public JsonTestData(double v)
        {
            doubleData = v;
            floatData = (float)v;
            floatConvertedData = Convert.ToSingle(v);
            floatTruncatedData = MathUtils.Round((float)v, 4);
            floatTruncatedFromStringData = Convert.ToSingle(string.Format("{0:0.###}", v));
            doubleTruncatedData = Math.Round(v, 4);
        }

        public JsonTestData(float v)
        {
            doubleData = v;
            floatData = v;
            floatConvertedData = Convert.ToSingle(v);
            floatTruncatedData = MathUtils.Round((float)v, 4);
            floatTruncatedFromStringData = Convert.ToSingle(string.Format("{0:0.###}", v));
            doubleTruncatedData = Math.Round(v, 4);
        }

        public double doubleData;
        public float floatData;
        public float floatConvertedData;
        public float floatTruncatedData;
        public float floatTruncatedFromStringData;
        public double doubleTruncatedData;
    }

    [Serializable]
    public class JsonTestDatas
    {
        public List<JsonTestData> tests;
    }

    public class JsonSerialization
    {
        [Test]
        public void JsonSerializing()
        {
            var test = new JsonTestDatas() { tests = new List<JsonTestData>() };
            test.tests.Add(new JsonTestData(196.24789428710938));
            test.tests.Add(new JsonTestData(0.10000000149011612));
            test.tests.Add(new JsonTestData(13.600000381469727));
            test.tests.Add(new JsonTestData(196.2479));
            test.tests.Add(new JsonTestData(0.1));
            test.tests.Add(new JsonTestData(13.6));

            File.WriteAllText(TestUtils.folder + "/json_serialized_from_double.txt",
                JsonUtility.ToJson(test, true));

            var content = File.ReadAllText(TestUtils.folder + "/json_serialized_from_double.txt");
            var test2 = JsonUtility.FromJson<JsonTestDatas>(content);
            Debug.Log(test2);
        }

        [Test]
        public void JsonSerializing2()
        {
            var test = new JsonTestDatas() { tests = new List<JsonTestData>() };
            test.tests.Add(new JsonTestData(196.24789428710938f));
            test.tests.Add(new JsonTestData(0.10000000149011612f));
            test.tests.Add(new JsonTestData(13.600000381469727f));
            test.tests.Add(new JsonTestData(196.2479f));
            test.tests.Add(new JsonTestData(0.1f));
            test.tests.Add(new JsonTestData(13.6f));

            File.WriteAllText(TestUtils.folder + "/json_serialized_from_single.txt",
                JsonUtility.ToJson(test, true));

            var content = File.ReadAllText(TestUtils.folder + "/json_serialized_from_single.txt");
            var test2 = JsonUtility.FromJson<JsonTestDatas>(content);
            Debug.Log(test2);
        }

        [Test]
        public void JsonSerializing3()
        {
            var baseDataFile = TestUtils.folder + "/single_body_data";
            var srcData = RecordingData.Read(baseDataFile + "_src_js.txt");
            srcData.Write(baseDataFile + "_test1.txt");

            var src2Data = RecordingData.Read(baseDataFile + "_test1.txt");
            src2Data.Write(baseDataFile + "_test2.txt");

            var src3Data = RecordingData.Read(baseDataFile + "_test2.txt");
            src3Data.Write(baseDataFile + "_test3.txt");
        }
    }
}