/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
Copyright © 2019 chibayuki@foxmail.com

Com性能测试 (ComPerformanceTest)
Version 19.11.14.0000

This file is part of "Com性能测试" (ComPerformanceTest)

"Com性能测试" (ComPerformanceTest) is released under the GPLv3 license
* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
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

            //

            TestResult.Clear();
            TestResult.LogCsv("Com Version:", ComInfo.ComVersionString);
            TestResult.LogCsv("Class Count:", TestClass.Length.ToString());
            TestResult.LogCsv("Member Count:", ComInfo.TotalMemberCount.ToString());
            TestResult.LogCsv();
            TestResult.LogCsv("Member", "----", "----", "Result", "----", "Raw Result", "----", "----", "----", "Comment");
            TestResult.LogCsv("Namespace", "Class", "Method", "Period", "Frequency", "Count", "Timecost [ms]", "Period [ns]", "Frequency [Hz]", "----");

            TestProgress.Reset();
            TestProgress.Report(0);

            //

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