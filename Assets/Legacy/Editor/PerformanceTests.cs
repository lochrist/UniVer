using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformanceTests {

    void Action1(int i)
    {
        i++;
    }

    void Action2(int i)
    {
        i++;
    }

    void Action3(int i )
    {
        i++;
    }

    void Action4(int i)
    {
        i++;
    }

    void Action5(int i)
    {
        i++;
    }

    void Action6(int i)
    {
        i++;
    }

    void Action7(int i)
    {
        i++;
    }

    void Action8(int i)
    {
        i++;
    }

    void Action9(int i)
    {
        i++;
    }

    void Action10(int i)
    {
        i++;
    }

    delegate void A(int i);

    [Test]
    public void DictionaryAccess()
    {
        var d = new Dictionary<int, A>();
        d.Add(0, Action1);
        d.Add(1, Action2);
        d.Add(2, Action3);
        d.Add(3, Action4);
        d.Add(4, Action5);
        d.Add(5, Action6);
        d.Add(6, Action7);
        d.Add(7, Action8);
        d.Add(8, Action9);
        d.Add(9, Action10);

        var w = new System.Diagnostics.Stopwatch();
        w.Start();
        for (var i = 0; i < 10000000; i++)
        {
            var v = d[Random.Range(0, 9)];
            v(i);
        }
        w.Stop();
        Debug.Log("Dict access: " + w.ElapsedMilliseconds);
    }

    [Test]
    public void ArrayAccess()
    {
        var keys = new int[] { 2, 4, 0, 1, 3, 7, 6, 9, 5, 8 };
        var actions = new A[] { Action3, Action5, Action1, Action2, Action4, Action8, Action7, Action3, Action10, Action6, Action9 };

        var w = new System.Diagnostics.Stopwatch();
        w.Start();
        for (var i = 0; i < 10000000; i++)
        {
            var searchFor = Random.Range(0, 9);
            for (var k = 0; k < keys.Length; ++k)
            {
                if (keys[k] == searchFor)
                {
                    actions[k](i);
                    break;
                }
            }
        }
        w.Stop();
        Debug.Log("Array access: " + w.ElapsedMilliseconds);
    }
}
