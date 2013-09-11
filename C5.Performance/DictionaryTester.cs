using System;
using System.Collections;
using NPerf.Framework;

[PerfTester(typeof(IDictionary), 10)]
public class DictionaryTester
{
    private int count;
    private Random rnd = new Random();
    [PerfRunDescriptor]
    public double Count(int index)
    {
        return index * 1000;
    }

    [PerfSetUp]
    public void SetUp(int index, IDictionary dic)
    {
        count = (int) Math.Floor(Count(index));
    }

    [PerfTest]
    public void ItemAssign(IDictionary dic)
    {
        for (int i = 0; i < count; ++i)
            dic[rnd.Next()] = null;
    }
}