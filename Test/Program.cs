﻿/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
Copyright © 2019 chibayuki@foxmail.com

Com性能测试 (ComPerformanceTest)
Version 19.11.3.0000

This file is part of "Com性能测试" (ComPerformanceTest)

"Com性能测试" (ComPerformanceTest) is released under the GPLv3 license
* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

#define ComVer1910
#define ComVer1905
#define ComVer1809

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
#if ComVer1910
        private const string _ComVersionString = "19.10.14.2100";
#elif ComVer1905
        private const string _ComVersionString = "19.5.11.1720";
#elif ComVer1809
        private const string _ComVersionString = "18.9.28.2200";
#else
        private const string _ComVersionString = "<Unknown>";
#endif

        static void Main(string[] args)
        {
            TestResult.Clear();
            TestResult.Log(string.Concat("[Com.Properties.AssemblyVersion=", _ComVersionString, "],Period,Frequency,Period [ns],Frequency [Hz],Comment"));

            TestProgress.Reset();
            TestProgress.Report(0);

            //

            ClassPerfTestBase[] TestClass = new ClassPerfTestBase[]
            {
                new AnimationTest(),
                new BitOperationTest(),
                new BitSetTest(),
                new ColorManipulationTest(),
                new ColorXTest(),
                new ComplexTest(),
                new DateTimeXTest(),
                new GeometryTest(),
                new IOTest(),
                new MatrixTest(),
                new Painting2DTest(),
                new Painting3DTest(),
                new PointDTest(),
                new PointD3DTest(),
                new PointD4DTest(),
                new PointD5DTest(),
                new PointD6DTest(),
                new RealTest(),
                new StatisticsTest(),
                new TextTest(),
                new VectorTest()
            };

            for (int i = 0; i < TestClass.Length; i++)
            {
                TestClass[i].Run();
            }

            //

            string filePath = TestResult.Save(Environment.CurrentDirectory);

            TestProgress.Report(0);

            TestProgress.ClearExtra();

            if (!string.IsNullOrEmpty(filePath))
            {
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.Write("Test completed. Log file has been saved at \"");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write(filePath);
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.Write("\". Press any key to exit.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.Write("Test completed. Press any key to exit.");
            }

            //

            Console.ReadKey();
        }
    }
}