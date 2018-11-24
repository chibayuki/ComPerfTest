/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
Copyright © 2018 chibayuki@foxmail.com

Com性能测试
Version 1.0.0.0

This file is part of "Com性能测试" (ComPerformanceTest)

"Com性能测试" (ComPerformanceTest) is released under the GPLv3 license
* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace Test
{
    static class TestResult // 测试结果
    {
        private static List<string> _ResultList = new List<string>(2048);

        //

        public static void Log(string result) // 记录测试结果
        {
            _ResultList.Add(result);
        }

        public static string Save(string fileDir) // 保存测试结果
        {
            string filePath = string.Empty;

            try
            {
                if (!Directory.Exists(fileDir))
                {
                    Directory.CreateDirectory(fileDir);
                }

                DateTime dt = DateTime.Now;
                string fileName = string.Concat("ComPerfTestLog_", (dt.Year % 100).ToString("D2"), dt.Month.ToString("D2"), dt.Day.ToString("D2"), ((int)dt.TimeOfDay.TotalMilliseconds).ToString("D8"), ".log");

                filePath = Path.Combine(fileDir, fileName);

                StreamWriter sw = new StreamWriter(filePath, false);

                for (int i = 0; i < _ResultList.Count; i++)
                {
                    sw.WriteLine(_ResultList[i]);
                }

                sw.Close();
            }
            catch { }

            return filePath;
        }

        public static void Clear()
        {
            _ResultList.Clear();

            Console.Clear();
        }
    }

    static class TestProgress // 测试进度
    {
        private const int _TotalMemberCount = 1363; // 成员总数量
        private static int _CompletedMemberCount = 0; // 已测试成员数量

        private static int _FullWidth => Math.Max(10, Math.Min(Console.WindowWidth * 3 / 4, 100)); // 进度条宽度

        //

        public static void Report(int delta) // 报告测试进度
        {
            _CompletedMemberCount += delta;

            double progress = (double)_CompletedMemberCount / _TotalMemberCount;

            Console.SetCursorPosition(2, 2);
            Console.BackgroundColor = ConsoleColor.Gray;
            Console.Write(new string(' ', _FullWidth));
            Console.SetCursorPosition(2, 2);
            Console.BackgroundColor = ConsoleColor.DarkCyan;
            Console.Write(new string(' ', (int)(progress * _FullWidth)));
            Console.BackgroundColor = ConsoleColor.White;
            Console.SetCursorPosition(2, 3);
            Console.Write(new string(' ', _FullWidth));
            Console.SetCursorPosition(2, 3);
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write((int)(progress * 100) + "% completed");
            Console.ForegroundColor = ConsoleColor.Black;
            Console.SetCursorPosition(0, 5);
            Console.WindowTop = 0;
        }

        public static void Reset() // 重置测试进度
        {
            _CompletedMemberCount = 0;

            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.White;
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("Executing test");
            Console.ForegroundColor = ConsoleColor.Black;
            Console.SetCursorPosition(2, 2);
            Console.BackgroundColor = ConsoleColor.Gray;
            Console.Write(new string(' ', _FullWidth));
            Console.BackgroundColor = ConsoleColor.White;
            Console.SetCursorPosition(2, 3);
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("0% completed");
            Console.ForegroundColor = ConsoleColor.Black;
            Console.SetCursorPosition(0, 5);
            Console.WindowTop = 0;
        }

        public static void ClearExtra() // 清理额外输出
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;

            int bottom = Console.WindowHeight;
            string blank = new string(' ', Console.WindowWidth);

            for (int i = 5; i < bottom; i++)
            {
                Console.SetCursorPosition(0, i);
                Console.Write(blank);
            }

            Console.SetCursorPosition(0, 5);
            Console.WindowTop = 0;
        }
    }

    class ClassPerformanceTestBase // 类性能测试类的基类
    {
        private const int _MSOfPerMember = 100; // 被测试类每个成员的最短执行时长的毫秒数

        //

        private static string _GetScientificNotationString(double value, int significance, bool useNaturalExpression, bool useMagnitudeOrderCode, string unit) // 科学记数法
        {
            const string _PositiveMagnitudeOrderCode = "kMGTPEZY";
            const string _NegativeMagnitudeOrderCode = "mμnpfazy";

            try
            {
                if (double.IsNaN(value) || double.IsInfinity(value))
                {
                    return "N/A";
                }
                else
                {
                    string part1 = string.Empty, part2 = string.Empty, part3 = string.Empty, part4 = string.Empty;

                    int sign = Math.Sign(value);

                    part1 = (sign < 0 ? "-" : string.Empty);

                    value = Math.Abs(value);

                    significance = Math.Max(0, significance);

                    int exp = (int)Math.Floor(Math.Log10(value));

                    if (significance > 0)
                    {
                        exp -= (significance - 1);

                        value = Math.Round(value / Math.Pow(10, exp));
                    }
                    else
                    {
                        value /= Math.Pow(10, exp);
                    }

                    while (value >= 10)
                    {
                        value /= 10;
                        exp++;
                    }

                    if (useMagnitudeOrderCode)
                    {
                        if (exp >= -24 && exp < 27)
                        {
                            int mod = 0;

                            if (exp >= 0)
                            {
                                mod = exp % 3;
                            }
                            else
                            {
                                mod = (-exp) % 3;

                                if (mod > 0)
                                {
                                    mod = 3 - mod;
                                }
                            }

                            if (mod > 0)
                            {
                                value *= Math.Pow(10, mod);
                            }

                            part2 = (significance > 0 ? value.ToString("N" + Math.Max(0, significance - 1 - mod)) : value.ToString());

                            int mag = 0;

                            if (exp >= 0)
                            {
                                mag = exp / 3;
                            }
                            else
                            {
                                mag = (exp + 1) / 3 - 1;
                            }

                            string magCode = (mag > 0 ? _PositiveMagnitudeOrderCode[mag - 1].ToString() : (mag < 0 ? _NegativeMagnitudeOrderCode[-mag - 1].ToString() : string.Empty));

                            if (string.IsNullOrEmpty(unit))
                            {
                                part3 = magCode;
                                part4 = string.Empty;
                            }
                            else
                            {
                                part3 = " " + magCode;
                                part4 = unit;
                            }
                        }
                        else
                        {
                            part2 = (significance > 0 ? value.ToString("N" + Math.Max(0, significance - 1)) : value.ToString());
                            part3 = (useNaturalExpression ? "×10^" + exp : (exp > 0 ? "E+" + exp : "E" + exp));
                            part4 = (string.IsNullOrEmpty(unit) ? string.Empty : " " + unit);
                        }
                    }
                    else
                    {
                        part2 = (significance > 0 ? value.ToString("N" + Math.Max(0, significance - 1)) : value.ToString());
                        part3 = (exp == 0 ? string.Empty : (useNaturalExpression ? "×10^" + exp : (exp > 0 ? "E+" + exp : "E" + exp)));
                        part4 = (string.IsNullOrEmpty(unit) ? string.Empty : " " + unit);
                    }

                    return string.Concat(part1, part2, part3, part4);
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        //

        protected static void ExecuteTest(Action method, string methodName, string comment) // 执行测试
        {
            string result = string.Empty;

            if (method != null)
            {
                if (methodName == null)
                {
                    methodName = string.Empty;
                }

                if (comment == null)
                {
                    comment = string.Empty;
                }

                //

                double tryMS = _MSOfPerMember * 0.1;
                int tryCycle = 1;
                int cycle = 0;

                double totalMS = 0;
                DateTime dt = DateTime.Now;

                while (true)
                {
                    method();

                    cycle++;

                    if (cycle >= tryCycle)
                    {
                        totalMS = (DateTime.Now - dt).TotalMilliseconds;

                        if (totalMS < tryMS)
                        {
                            tryCycle <<= 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                tryCycle = (int)Math.Ceiling(_MSOfPerMember / (totalMS / cycle));

                if (tryCycle > 1)
                {
                    cycle = 0;
                    totalMS = 0;

                    while (true)
                    {
                        dt = DateTime.Now;

                        while (true)
                        {
                            method();

                            cycle++;

                            if (cycle >= tryCycle)
                            {
                                break;
                            }
                        }

                        totalMS += (DateTime.Now - dt).TotalMilliseconds;

                        if (totalMS <= 0)
                        {
                            tryCycle *= 10;
                            cycle = 0;
                            totalMS = 0;
                        }
                        else if (totalMS < _MSOfPerMember * 0.9)
                        {
                            tryCycle += (int)Math.Ceiling((_MSOfPerMember - totalMS) / (totalMS / cycle));
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                double msPerCycle = totalMS / cycle;

                result = string.Concat("[", methodName, "] ", _GetScientificNotationString(msPerCycle / 1000, 4, true, true, "s"), ", ", _GetScientificNotationString(1000 / msPerCycle, 4, true, true, "Hz"), (comment.Length > 0 ? ", " + comment : string.Empty));
            }
            else
            {
                result = string.Concat("[", methodName, "] Untested", (comment.Length > 0 ? ", " + comment : string.Empty));
            }

            TestResult.Log(result);

            TestProgress.Report(1);

            TestProgress.ClearExtra();

            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.Write("Latest result: ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(result);
        }

        protected static void ExecuteTest(Action method, string methodName) // 执行测试
        {
            ExecuteTest(method, methodName, string.Empty);
        }

        //

        protected virtual void Constructor() { } // 构造函数测试

        protected virtual void Property() { } // 属性测试

        protected virtual void StaticProperty() { } // 静态属性测试

        protected virtual void Method() { } // 方法测试

        protected virtual void StaticMethod() { } // 静态方法测试

        protected virtual void Operator() { } // 运算符测试

        //

        public void Run() // 执行类测试
        {
            Constructor();
            Property();
            StaticProperty();
            Method();
            StaticMethod();
            Operator();
        }
    }

    sealed class AnimationTest : ClassPerformanceTestBase
    {
        protected override void Constructor()
        {

        }

        protected override void Property()
        {

        }

        protected override void StaticProperty()
        {

        }

        protected override void Method()
        {

        }

        protected override void StaticMethod()
        {
            ExecuteTest(null, "Com.Animation.Show(Com.Animation.Frame, int, int, int, System.Collections.Generic.List<int>)");

            ExecuteTest(null, "Com.Animation.Show(Com.Animation.Frame, int, int, int)");

            ExecuteTest(null, "Com.Animation.Show(Com.Animation.Frame, int, int, System.Collections.Generic.List<int>)");

            ExecuteTest(null, "Com.Animation.Show(Com.Animation.Frame, int, int)");
        }

        protected override void Operator()
        {

        }
    }

    sealed class BitOperationTest : ClassPerformanceTestBase
    {
        protected override void Constructor()
        {

        }

        protected override void Property()
        {

        }

        protected override void StaticProperty()
        {

        }

        protected override void Method()
        {

        }

        protected override void StaticMethod()
        {
            // 8 位

            {
                int bit = Com.Statistics.RandomInteger() % 8;

                Action method = () =>
                {
                    byte result = Com.BitOperation.GetBinary8WithSingleBit1(bit);
                };

                ExecuteTest(method, "Com.BitOperation.GetBinary8WithSingleBit1(int)");
            }

            {
                int bit = Com.Statistics.RandomInteger() % 8;

                Action method = () =>
                {
                    byte result = Com.BitOperation.GetBinary8WithSingleBit0(bit);
                };

                ExecuteTest(method, "Com.BitOperation.GetBinary8WithSingleBit0(int)");
            }

            {
                byte bin = (byte)(Com.Statistics.RandomInteger() % 255);
                int bit = Com.Statistics.RandomInteger() % 8;

                Action method = () =>
                {
                    Com.BitOperation.AddBitToBinary(ref bin, bit);
                };

                ExecuteTest(method, "Com.BitOperation.AddBitToBinary(ref byte, int)");
            }

            {
                byte bin = (byte)(Com.Statistics.RandomInteger() % 255);
                int bit = Com.Statistics.RandomInteger() % 8;

                Action method = () =>
                {
                    Com.BitOperation.RemoveBitFromBinary(ref bin, bit);
                };

                ExecuteTest(method, "Com.BitOperation.RemoveBitFromBinary(ref byte, int)");
            }

            {
                byte bin = (byte)(Com.Statistics.RandomInteger() % 255);
                int bit = Com.Statistics.RandomInteger() % 8;

                Action method = () =>
                {
                    Com.BitOperation.InverseBitOfBinary(ref bin, bit);
                };

                ExecuteTest(method, "Com.BitOperation.InverseBitOfBinary(ref byte, int)");
            }

            {
                byte bin = (byte)(Com.Statistics.RandomInteger() % 255);
                int bit = Com.Statistics.RandomInteger() % 8;

                Action method = () =>
                {
                    bool result = Com.BitOperation.BinaryHasBit(bin, bit);
                };

                ExecuteTest(method, "Com.BitOperation.BinaryHasBit(byte, int)");
            }

            {
                byte bin = (byte)(Com.Statistics.RandomInteger() % 255);

                Action method = () =>
                {
                    int result = Com.BitOperation.GetBit1CountOfBinary(bin);
                };

                ExecuteTest(method, "Com.BitOperation.GetBit1CountOfBinary(byte)");
            }

            {
                byte bin = (byte)(Com.Statistics.RandomInteger() % 255);

                Action method = () =>
                {
                    int result = Com.BitOperation.GetBit0CountOfBinary(bin);
                };

                ExecuteTest(method, "Com.BitOperation.GetBit0CountOfBinary(byte)");
            }

            {
                byte bin = (byte)(Com.Statistics.RandomInteger() % 255);

                Action method = () =>
                {
                    List<int> result = Com.BitOperation.GetBit1IndexOfBinary(bin);
                };

                ExecuteTest(method, "Com.BitOperation.GetBit1IndexOfBinary(byte)");
            }

            {
                byte bin = (byte)(Com.Statistics.RandomInteger() % 255);

                Action method = () =>
                {
                    List<int> result = Com.BitOperation.GetBit0IndexOfBinary(bin);
                };

                ExecuteTest(method, "Com.BitOperation.GetBit0IndexOfBinary(byte)");
            }

            // 16 位

            {
                int bit = Com.Statistics.RandomInteger() % 16;

                Action method = () =>
                {
                    ushort result = Com.BitOperation.GetBinary16WithSingleBit1(bit);
                };

                ExecuteTest(method, "Com.BitOperation.GetBinary16WithSingleBit1(int)");
            }

            {
                int bit = Com.Statistics.RandomInteger() % 16;

                Action method = () =>
                {
                    ushort result = Com.BitOperation.GetBinary16WithSingleBit0(bit);
                };

                ExecuteTest(method, "Com.BitOperation.GetBinary16WithSingleBit0(int)");
            }

            {
                ushort bin = (ushort)Com.Statistics.RandomInteger();
                int bit = Com.Statistics.RandomInteger() % 16;

                Action method = () =>
                {
                    Com.BitOperation.AddBitToBinary(ref bin, bit);
                };

                ExecuteTest(method, "Com.BitOperation.AddBitToBinary(ref ushort, int)");
            }

            {
                ushort bin = (ushort)Com.Statistics.RandomInteger();
                int bit = Com.Statistics.RandomInteger() % 16;

                Action method = () =>
                {
                    Com.BitOperation.RemoveBitFromBinary(ref bin, bit);
                };

                ExecuteTest(method, "Com.BitOperation.RemoveBitFromBinary(ref ushort, int)");
            }

            {
                ushort bin = (ushort)Com.Statistics.RandomInteger();
                int bit = Com.Statistics.RandomInteger() % 16;

                Action method = () =>
                {
                    Com.BitOperation.InverseBitOfBinary(ref bin, bit);
                };

                ExecuteTest(method, "Com.BitOperation.InverseBitOfBinary(ref ushort, int)");
            }

            {
                ushort bin = (ushort)Com.Statistics.RandomInteger();
                int bit = Com.Statistics.RandomInteger() % 16;

                Action method = () =>
                {
                    bool result = Com.BitOperation.BinaryHasBit(bin, bit);
                };

                ExecuteTest(method, "Com.BitOperation.BinaryHasBit(ushort, int)");
            }

            {
                ushort bin = (ushort)Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    int result = Com.BitOperation.GetBit1CountOfBinary(bin);
                };

                ExecuteTest(method, "Com.BitOperation.GetBit1CountOfBinary(ushort)");
            }

            {
                ushort bin = (ushort)Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    int result = Com.BitOperation.GetBit0CountOfBinary(bin);
                };

                ExecuteTest(method, "Com.BitOperation.GetBit0CountOfBinary(ushort)");
            }

            {
                ushort bin = (ushort)Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    List<int> result = Com.BitOperation.GetBit1IndexOfBinary(bin);
                };

                ExecuteTest(method, "Com.BitOperation.GetBit1IndexOfBinary(ushort)");
            }

            {
                ushort bin = (ushort)Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    List<int> result = Com.BitOperation.GetBit0IndexOfBinary(bin);
                };

                ExecuteTest(method, "Com.BitOperation.GetBit0IndexOfBinary(ushort)");
            }

            // 32 位

            {
                int bit = Com.Statistics.RandomInteger() % 32;

                Action method = () =>
                {
                    uint result = Com.BitOperation.GetBinary32WithSingleBit1(bit);
                };

                ExecuteTest(method, "Com.BitOperation.GetBinary32WithSingleBit1(int)");
            }

            {
                int bit = Com.Statistics.RandomInteger() % 32;

                Action method = () =>
                {
                    uint result = Com.BitOperation.GetBinary32WithSingleBit0(bit);
                };

                ExecuteTest(method, "Com.BitOperation.GetBinary32WithSingleBit0(int)");
            }

            {
                uint bin = (uint)Com.Statistics.RandomInteger();
                int bit = Com.Statistics.RandomInteger() % 32;

                Action method = () =>
                {
                    Com.BitOperation.AddBitToBinary(ref bin, bit);
                };

                ExecuteTest(method, "Com.BitOperation.AddBitToBinary(ref uint, int)");
            }

            {
                uint bin = (uint)Com.Statistics.RandomInteger();
                int bit = Com.Statistics.RandomInteger() % 32;

                Action method = () =>
                {
                    Com.BitOperation.RemoveBitFromBinary(ref bin, bit);
                };

                ExecuteTest(method, "Com.BitOperation.RemoveBitFromBinary(ref uint, int)");
            }

            {
                uint bin = (uint)Com.Statistics.RandomInteger();
                int bit = Com.Statistics.RandomInteger() % 32;

                Action method = () =>
                {
                    Com.BitOperation.InverseBitOfBinary(ref bin, bit);
                };

                ExecuteTest(method, "Com.BitOperation.InverseBitOfBinary(ref uint, int)");
            }

            {
                uint bin = (uint)Com.Statistics.RandomInteger();
                int bit = Com.Statistics.RandomInteger() % 32;

                Action method = () =>
                {
                    bool result = Com.BitOperation.BinaryHasBit(bin, bit);
                };

                ExecuteTest(method, "Com.BitOperation.BinaryHasBit(uint, int)");
            }

            {
                uint bin = (uint)Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    int result = Com.BitOperation.GetBit1CountOfBinary(bin);
                };

                ExecuteTest(method, "Com.BitOperation.GetBit1CountOfBinary(uint)");
            }

            {
                uint bin = (uint)Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    int result = Com.BitOperation.GetBit0CountOfBinary(bin);
                };

                ExecuteTest(method, "Com.BitOperation.GetBit0CountOfBinary(uint)");
            }

            {
                uint bin = (uint)Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    List<int> result = Com.BitOperation.GetBit1IndexOfBinary(bin);
                };

                ExecuteTest(method, "Com.BitOperation.GetBit1IndexOfBinary(uint)");
            }

            {
                uint bin = (uint)Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    List<int> result = Com.BitOperation.GetBit0IndexOfBinary(bin);
                };

                ExecuteTest(method, "Com.BitOperation.GetBit0IndexOfBinary(uint)");
            }

            // 64 位

            {
                int bit = Com.Statistics.RandomInteger() % 64;

                Action method = () =>
                {
                    ulong result = Com.BitOperation.GetBinary64WithSingleBit1(bit);
                };

                ExecuteTest(method, "Com.BitOperation.GetBinary64WithSingleBit1(int)");
            }

            {
                int bit = Com.Statistics.RandomInteger() % 64;

                Action method = () =>
                {
                    ulong result = Com.BitOperation.GetBinary64WithSingleBit0(bit);
                };

                ExecuteTest(method, "Com.BitOperation.GetBinary64WithSingleBit0(int)");
            }

            {
                ulong bin = (ulong)Com.Statistics.RandomInteger();
                int bit = Com.Statistics.RandomInteger() % 64;

                Action method = () =>
                {
                    Com.BitOperation.AddBitToBinary(ref bin, bit);
                };

                ExecuteTest(method, "Com.BitOperation.AddBitToBinary(ref ulong, int)");
            }

            {
                ulong bin = (ulong)Com.Statistics.RandomInteger();
                int bit = Com.Statistics.RandomInteger() % 64;

                Action method = () =>
                {
                    Com.BitOperation.RemoveBitFromBinary(ref bin, bit);
                };

                ExecuteTest(method, "Com.BitOperation.RemoveBitFromBinary(ref ulong, int)");
            }

            {
                ulong bin = (ulong)Com.Statistics.RandomInteger();
                int bit = Com.Statistics.RandomInteger() % 64;

                Action method = () =>
                {
                    Com.BitOperation.InverseBitOfBinary(ref bin, bit);
                };

                ExecuteTest(method, "Com.BitOperation.InverseBitOfBinary(ref ulong, int)");
            }

            {
                ulong bin = (ulong)Com.Statistics.RandomInteger();
                int bit = Com.Statistics.RandomInteger() % 64;

                Action method = () =>
                {
                    bool result = Com.BitOperation.BinaryHasBit(bin, bit);
                };

                ExecuteTest(method, "Com.BitOperation.BinaryHasBit(ulong, int)");
            }

            {
                ulong bin = (ulong)Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    int result = Com.BitOperation.GetBit1CountOfBinary(bin);
                };

                ExecuteTest(method, "Com.BitOperation.GetBit1CountOfBinary(ulong)");
            }

            {
                ulong bin = (ulong)Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    int result = Com.BitOperation.GetBit0CountOfBinary(bin);
                };

                ExecuteTest(method, "Com.BitOperation.GetBit0CountOfBinary(ulong)");
            }

            {
                ulong bin = (ulong)Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    List<int> result = Com.BitOperation.GetBit1IndexOfBinary(bin);
                };

                ExecuteTest(method, "Com.BitOperation.GetBit1IndexOfBinary(ulong)");
            }

            {
                ulong bin = (ulong)Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    List<int> result = Com.BitOperation.GetBit0IndexOfBinary(bin);
                };

                ExecuteTest(method, "Com.BitOperation.GetBit0IndexOfBinary(ulong)");
            }
        }

        protected override void Operator()
        {

        }
    }

    sealed class BitSetTest : ClassPerformanceTestBase
    {
        private static Com.BitSet _GetRandomBitSet(int length)
        {
            if (length > 0)
            {
                bool[] array = new bool[length];

                for (int i = 0; i < array.Length; i++)
                {
                    if (Com.Statistics.RandomInteger() % 2 == 0)
                    {
                        array[i] = true;
                    }
                }

                return new Com.BitSet(array);
            }
            else
            {
                return Com.BitSet.Empty;
            }
        }

        //

        protected override void Constructor()
        {
            {
                int length = 1024;

                Action method = () =>
                {
                    Com.BitSet bitSet = new Com.BitSet(length);
                };

                ExecuteTest(method, "Com.BitSet.BitSet(int)", "size at 1024 bits");
            }

            {
                int length = 1024;
                bool bitValue = true;

                Action method = () =>
                {
                    Com.BitSet bitSet = new Com.BitSet(length, bitValue);
                };

                ExecuteTest(method, "Com.BitSet.BitSet(int, bool)", "size at 1024 bits");
            }

            {
                bool[] array = new bool[1024];

                for (int i = 0; i < array.Length; i++)
                {
                    if (Com.Statistics.RandomInteger() % 2 == 0)
                    {
                        array[i] = true;
                    }
                }

                Action method = () =>
                {
                    Com.BitSet bitSet = new Com.BitSet(array);
                };

                ExecuteTest(method, "Com.BitSet.BitSet(bool[])", "size at 1024 bits");
            }

            {
                int[] array = new int[1024];

                for (int i = 0; i < array.Length; i++)
                {
                    if (Com.Statistics.RandomInteger() % 2 == 0)
                    {
                        array[i] = 1;
                    }
                }

                Action method = () =>
                {
                    Com.BitSet bitSet = new Com.BitSet(array);
                };

                ExecuteTest(method, "Com.BitSet.BitSet(int[])", "size at 1024 bits");
            }
        }

        protected override void Property()
        {
            // 索引器

            {
                Com.BitSet bitSet = new Com.BitSet(1024);
                int index = Com.Statistics.RandomInteger(1024);

                Action method = () =>
                {
                    bool result = bitSet[index];
                };

                ExecuteTest(method, "Com.BitSet.this[int].get()", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = new Com.BitSet(1024);
                int index = Com.Statistics.RandomInteger(1024);
                bool value = true;

                Action method = () =>
                {
                    bitSet[index] = value;
                };

                ExecuteTest(method, "Com.BitSet.this[int].set(bool)", "size at 1024 bits");
            }

            // Is

            {
                Com.BitSet bitSet = new Com.BitSet(1024);

                Action method = () =>
                {
                    bool result = bitSet.IsEmpty;
                };

                ExecuteTest(method, "Com.BitSet.IsEmpty.get()", "size at 1024 bits");
            }

            // Size

            {
                Com.BitSet bitSet = new Com.BitSet(1024);

                Action method = () =>
                {
                    int result = bitSet.Size;
                };

                ExecuteTest(method, "Com.BitSet.Size.get()", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = new Com.BitSet(1024);
                int value = 8;

                Action method = () =>
                {
                    bitSet.Size += value;
                };

                ExecuteTest(method, "Com.BitSet.Size.set(int)", "size at 1024 bits, increase by 8 bits");
            }

            {
                Com.BitSet bitSet = new Com.BitSet(1024);

                Action method = () =>
                {
                    int result = bitSet.Count;
                };

                ExecuteTest(method, "Com.BitSet.Count.get()", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = new Com.BitSet(1024);

                Action method = () =>
                {
                    int result = bitSet.Length;
                };

                ExecuteTest(method, "Com.BitSet.Length.get()", "size at 1024 bits");
            }

            // Capacity

            {
                Com.BitSet bitSet = new Com.BitSet(1024);

                Action method = () =>
                {
                    int result = bitSet.Capacity;
                };

                ExecuteTest(method, "Com.BitSet.Capacity.get()", "size at 1024 bits");
            }
        }

        protected override void StaticProperty()
        {

        }

        protected override void Method()
        {
            // object

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);
                object obj = (object)bitSet.Copy();

                Action method = () =>
                {
                    bool result = bitSet.Equals(obj);
                };

                ExecuteTest(method, "Com.BitSet.Equals(object)", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    int result = bitSet.GetHashCode();
                };

                ExecuteTest(method, "Com.BitSet.GetHashCode()", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    string result = bitSet.ToString();
                };

                ExecuteTest(method, "Com.BitSet.ToString()", "size at 1024 bits");
            }

            // Equals

            {
                Com.BitSet left = _GetRandomBitSet(1024);
                Com.BitSet right = left.Copy();

                Action method = () =>
                {
                    bool result = left.Equals(right);
                };

                ExecuteTest(method, "Com.BitSet.Equals(Com.BitSet)", "size at 1024 bits");
            }

            // Copy

            {
                Com.BitSet bitSet = new Com.BitSet(1024);

                Action method = () =>
                {
                    Com.BitSet result = bitSet.Copy();
                };

                ExecuteTest(method, "Com.BitSet.Copy()", "size at 1024 bits");
            }

            // Trim

            {
                Action method = () =>
                {
                    Com.BitSet bitSet = new Com.BitSet(1280);

                    bitSet.Size = 1024;

                    bitSet.Trim();
                };

                ExecuteTest(method, "Com.BitSet.Trim()", "new at 1280 bits and set Size to 1024 bits before every Trim");
            }

            // Get，Set

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);
                int index = Com.Statistics.RandomInteger(1024);

                Action method = () =>
                {
                    bool result = bitSet.Get(index);
                };

                ExecuteTest(method, "Com.BitSet.Get(int)", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);
                int index = Com.Statistics.RandomInteger(1024);
                bool bitValue = true;

                Action method = () =>
                {
                    bitSet.Set(index, bitValue);
                };

                ExecuteTest(method, "Com.BitSet.Set(int, bool)", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);
                bool bitValue = true;

                Action method = () =>
                {
                    bitSet.SetAll(bitValue);
                };

                ExecuteTest(method, "Com.BitSet.SetAll(bool)", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);
                int index = Com.Statistics.RandomInteger(1024);

                Action method = () =>
                {
                    bitSet.TrueForBit(index);
                };

                ExecuteTest(method, "Com.BitSet.TrueForBit(int)", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);
                int index = Com.Statistics.RandomInteger(1024);

                Action method = () =>
                {
                    bitSet.FalseForBit(index);
                };

                ExecuteTest(method, "Com.BitSet.FalseForBit(int)", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);
                int index = Com.Statistics.RandomInteger(1024);

                Action method = () =>
                {
                    bitSet.InverseBit(index);
                };

                ExecuteTest(method, "Com.BitSet.InverseBit(int)", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    bitSet.TrueForAll();
                };

                ExecuteTest(method, "Com.BitSet.TrueForAll()", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    bitSet.FalseForAll();
                };

                ExecuteTest(method, "Com.BitSet.FalseForAll()", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    bitSet.InverseAll();
                };

                ExecuteTest(method, "Com.BitSet.InverseAll()", "size at 1024 bits");
            }

            // BitCount，BitIndex

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    int result = bitSet.TrueBitCount();
                };

                ExecuteTest(method, "Com.BitSet.TrueBitCount()", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    int result = bitSet.FalseBitCount();
                };

                ExecuteTest(method, "Com.BitSet.FalseBitCount()", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    int[] result = bitSet.TrueBitIndex();
                };

                ExecuteTest(method, "Com.BitSet.TrueBitIndex()", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    int[] result = bitSet.FalseBitIndex();
                };

                ExecuteTest(method, "Com.BitSet.FalseBitIndex()", "size at 1024 bits");
            }

            // Logical

            {
                Com.BitSet left = _GetRandomBitSet(1024);
                Com.BitSet right = left.Not();

                Action method = () =>
                {
                    Com.BitSet result = left.And(right);
                };

                ExecuteTest(method, "Com.BitSet.And(Com.BitSet)", "size at 1024 bits");
            }

            {
                Com.BitSet left = _GetRandomBitSet(1024);
                Com.BitSet right = left.Not();

                Action method = () =>
                {
                    Com.BitSet result = left.Or(right);
                };

                ExecuteTest(method, "Com.BitSet.Or(Com.BitSet)", "size at 1024 bits");
            }

            {
                Com.BitSet left = _GetRandomBitSet(1024);
                Com.BitSet right = left.Not();

                Action method = () =>
                {
                    Com.BitSet result = left.Xor(right);
                };

                ExecuteTest(method, "Com.BitSet.Xor(Com.BitSet)", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    Com.BitSet result = bitSet.Not();
                };

                ExecuteTest(method, "Com.BitSet.Not()", "size at 1024 bits");
            }

            // ToArray

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    bool[] result = bitSet.ToBoolArray();
                };

                ExecuteTest(method, "Com.BitSet.ToBoolArray()", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    int[] result = bitSet.ToIntArray();
                };

                ExecuteTest(method, "Com.BitSet.ToIntArray()", "size at 1024 bits");
            }

            // ToBitString

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    string result = bitSet.ToBitString();
                };

                ExecuteTest(method, "Com.BitSet.ToBitString()", "size at 1024 bits");
            }
        }

        protected override void StaticMethod()
        {
            // IsNullOrEmpty

            {
                Com.BitSet bitSet = new Com.BitSet(1024);

                Action method = () =>
                {
                    bool result = Com.BitSet.IsNullOrEmpty(bitSet);
                };

                ExecuteTest(method, "Com.BitSet.IsNullOrEmpty(Com.BitSet)", "size at 1024 bits");
            }

            // Equals

            {
                Com.BitSet left = _GetRandomBitSet(1024);
                Com.BitSet right = left.Copy();

                Action method = () =>
                {
                    bool result = Com.BitSet.Equals(left, right);
                };

                ExecuteTest(method, "Com.BitSet.Equals(Com.BitSet, Com.BitSet)", "size at 1024 bits");
            }
        }

        protected override void Operator()
        {
            // 比较

            {
                Com.BitSet left = _GetRandomBitSet(1024);
                Com.BitSet right = left.Copy();

                Action method = () =>
                {
                    bool result = (left == right);
                };

                ExecuteTest(method, "Com.BitSet.operator ==(Com.BitSet, Com.BitSet)", "size at 1024 bits");
            }

            {
                Com.BitSet left = _GetRandomBitSet(1024);
                Com.BitSet right = left.Copy();

                Action method = () =>
                {
                    bool result = (left != right);
                };

                ExecuteTest(method, "Com.BitSet.operator !=(Com.BitSet, Com.BitSet)", "size at 1024 bits");
            }
        }
    }

    sealed class ColorManipulationTest : ClassPerformanceTestBase
    {
        protected override void Constructor()
        {

        }

        protected override void Property()
        {

        }

        protected override void StaticProperty()
        {

        }

        protected override void Method()
        {

        }

        protected override void StaticMethod()
        {
            // ColorName

            {
                Com.ColorX color = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    string result = Com.ColorManipulation.GetColorName(color);
                };

                ExecuteTest(method, "Com.ColorManipulation.GetColorName(Com.ColorX)");
            }

            {
                Color color = Com.ColorManipulation.GetRandomColor();

                Action method = () =>
                {
                    string result = Com.ColorManipulation.GetColorName(color);
                };

                ExecuteTest(method, "Com.ColorManipulation.GetColorName(System.Drawing.Color)");
            }

            // RandomColor

            {
                Action method = () =>
                {
                    Com.ColorX result = Com.ColorManipulation.GetRandomColorX();
                };

                ExecuteTest(method, "Com.ColorManipulation.GetRandomColorX()");
            }

            {
                Action method = () =>
                {
                    Color result = Com.ColorManipulation.GetRandomColor();
                };

                ExecuteTest(method, "Com.ColorManipulation.GetRandomColor()");
            }

            // 互补色，灰度

            {
                Com.ColorX color = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    Com.ColorX result = Com.ColorManipulation.GetComplementaryColor(color);
                };

                ExecuteTest(method, "Com.ColorManipulation.GetComplementaryColor(Com.ColorX)");
            }

            {
                Color color = Com.ColorManipulation.GetRandomColor();

                Action method = () =>
                {
                    Color result = Com.ColorManipulation.GetComplementaryColor(color);
                };

                ExecuteTest(method, "Com.ColorManipulation.GetComplementaryColor(System.Drawing.Color)");
            }

            {
                Com.ColorX color = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    Com.ColorX result = Com.ColorManipulation.GetGrayscaleColor(color);
                };

                ExecuteTest(method, "Com.ColorManipulation.GetGrayscaleColor(Com.ColorX)");
            }

            {
                Color color = Com.ColorManipulation.GetRandomColor();

                Action method = () =>
                {
                    Color result = Com.ColorManipulation.GetGrayscaleColor(color);
                };

                ExecuteTest(method, "Com.ColorManipulation.GetGrayscaleColor(System.Drawing.Color)");
            }

            // Blend

            {
                Com.ColorX color1 = Com.ColorManipulation.GetRandomColorX();
                Com.ColorX color2 = Com.ColorManipulation.GetRandomColorX();
                double proportion = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Com.ColorX result = Com.ColorManipulation.BlendByRGB(color1, color2, proportion);
                };

                ExecuteTest(method, "Com.ColorManipulation.BlendByRGB(Com.ColorX, Com.ColorX, double)");
            }

            {
                Color color1 = Com.ColorManipulation.GetRandomColor();
                Color color2 = Com.ColorManipulation.GetRandomColor();
                double proportion = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Color result = Com.ColorManipulation.BlendByRGB(color1, color2, proportion);
                };

                ExecuteTest(method, "Com.ColorManipulation.BlendByRGB(System.Drawing.Color, System.Drawing.Color, double)");
            }

            {
                Com.ColorX color1 = Com.ColorManipulation.GetRandomColorX();
                Com.ColorX color2 = Com.ColorManipulation.GetRandomColorX();
                double proportion = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Com.ColorX result = Com.ColorManipulation.BlendByHSV(color1, color2, proportion);
                };

                ExecuteTest(method, "Com.ColorManipulation.BlendByHSV(Com.ColorX, Com.ColorX, double)");
            }

            {
                Color color1 = Com.ColorManipulation.GetRandomColor();
                Color color2 = Com.ColorManipulation.GetRandomColor();
                double proportion = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Color result = Com.ColorManipulation.BlendByHSV(color1, color2, proportion);
                };

                ExecuteTest(method, "Com.ColorManipulation.BlendByHSV(System.Drawing.Color, System.Drawing.Color, double)");
            }

            {
                Com.ColorX color1 = Com.ColorManipulation.GetRandomColorX();
                Com.ColorX color2 = Com.ColorManipulation.GetRandomColorX();
                double proportion = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Com.ColorX result = Com.ColorManipulation.BlendByHSL(color1, color2, proportion);
                };

                ExecuteTest(method, "Com.ColorManipulation.BlendByHSL(Com.ColorX, Com.ColorX, double)");
            }

            {
                Color color1 = Com.ColorManipulation.GetRandomColor();
                Color color2 = Com.ColorManipulation.GetRandomColor();
                double proportion = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Color result = Com.ColorManipulation.BlendByHSL(color1, color2, proportion);
                };

                ExecuteTest(method, "Com.ColorManipulation.BlendByHSL(System.Drawing.Color, System.Drawing.Color, double)");
            }

            {
                Com.ColorX color1 = Com.ColorManipulation.GetRandomColorX();
                Com.ColorX color2 = Com.ColorManipulation.GetRandomColorX();
                double proportion = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Com.ColorX result = Com.ColorManipulation.BlendByCMYK(color1, color2, proportion);
                };

                ExecuteTest(method, "Com.ColorManipulation.BlendByCMYK(Com.ColorX, Com.ColorX, double)");
            }

            {
                Color color1 = Com.ColorManipulation.GetRandomColor();
                Color color2 = Com.ColorManipulation.GetRandomColor();
                double proportion = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Color result = Com.ColorManipulation.BlendByCMYK(color1, color2, proportion);
                };

                ExecuteTest(method, "Com.ColorManipulation.BlendByCMYK(System.Drawing.Color, System.Drawing.Color, double)");
            }

            {
                Com.ColorX color1 = Com.ColorManipulation.GetRandomColorX();
                Com.ColorX color2 = Com.ColorManipulation.GetRandomColorX();
                double proportion = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Com.ColorX result = Com.ColorManipulation.BlendByLAB(color1, color2, proportion);
                };

                ExecuteTest(method, "Com.ColorManipulation.BlendByLAB(Com.ColorX, Com.ColorX, double)");
            }

            {
                Color color1 = Com.ColorManipulation.GetRandomColor();
                Color color2 = Com.ColorManipulation.GetRandomColor();
                double proportion = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Color result = Com.ColorManipulation.BlendByLAB(color1, color2, proportion);
                };

                ExecuteTest(method, "Com.ColorManipulation.BlendByLAB(System.Drawing.Color, System.Drawing.Color, double)");
            }

            // Shift

            {
                Com.ColorX color = Com.ColorManipulation.GetRandomColorX();
                double level = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Com.ColorX result = Com.ColorManipulation.ShiftLightnessByHSV(color, level);
                };

                ExecuteTest(method, "Com.ColorManipulation.ShiftLightnessByHSV(Com.ColorX, double)");
            }

            {
                Color color = Com.ColorManipulation.GetRandomColor();
                double level = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Color result = Com.ColorManipulation.ShiftLightnessByHSV(color, level);
                };

                ExecuteTest(method, "Com.ColorManipulation.ShiftLightnessByHSV(System.Drawing.Color, double)");
            }

            {
                Com.ColorX color = Com.ColorManipulation.GetRandomColorX();
                double level = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Com.ColorX result = Com.ColorManipulation.ShiftLightnessByHSL(color, level);
                };

                ExecuteTest(method, "Com.ColorManipulation.ShiftLightnessByHSL(Com.ColorX, double)");
            }

            {
                Color color = Com.ColorManipulation.GetRandomColor();
                double level = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Color result = Com.ColorManipulation.ShiftLightnessByHSL(color, level);
                };

                ExecuteTest(method, "Com.ColorManipulation.ShiftLightnessByHSL(System.Drawing.Color, double)");
            }

            {
                Com.ColorX color = Com.ColorManipulation.GetRandomColorX();
                double level = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Com.ColorX result = Com.ColorManipulation.ShiftLightnessByLAB(color, level);
                };

                ExecuteTest(method, "Com.ColorManipulation.ShiftLightnessByLAB(Com.ColorX, double)");
            }

            {
                Color color = Com.ColorManipulation.GetRandomColor();
                double level = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Color result = Com.ColorManipulation.ShiftLightnessByLAB(color, level);
                };

                ExecuteTest(method, "Com.ColorManipulation.ShiftLightnessByLAB(System.Drawing.Color, double)");
            }

            {
                Com.ColorX color = Com.ColorManipulation.GetRandomColorX();
                double level = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Com.ColorX result = Com.ColorManipulation.ShiftSaturationByHSV(color, level);
                };

                ExecuteTest(method, "Com.ColorManipulation.ShiftSaturationByHSV(Com.ColorX, double)");
            }

            {
                Color color = Com.ColorManipulation.GetRandomColor();
                double level = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Color result = Com.ColorManipulation.ShiftSaturationByHSV(color, level);
                };

                ExecuteTest(method, "Com.ColorManipulation.ShiftSaturationByHSV(System.Drawing.Color, double)");
            }

            {
                Com.ColorX color = Com.ColorManipulation.GetRandomColorX();
                double level = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Com.ColorX result = Com.ColorManipulation.ShiftSaturationByHSL(color, level);
                };

                ExecuteTest(method, "Com.ColorManipulation.ShiftSaturationByHSL(Com.ColorX, double)");
            }

            {
                Color color = Com.ColorManipulation.GetRandomColor();
                double level = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Color result = Com.ColorManipulation.ShiftSaturationByHSL(color, level);
                };

                ExecuteTest(method, "Com.ColorManipulation.ShiftSaturationByHSL(System.Drawing.Color, double)");
            }
        }

        protected override void Operator()
        {

        }
    }

    sealed class ColorXTest : ClassPerformanceTestBase
    {
        protected override void Constructor()
        {
            {
                Color color = Com.ColorManipulation.GetRandomColor();

                Action method = () =>
                {
                    Com.ColorX colorX = new Com.ColorX(color);
                };

                ExecuteTest(method, "Com.ColorX.ColorX(System.Drawing.Color)");
            }

            {
                int argb = Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    Com.ColorX colorX = new Com.ColorX(argb);
                };

                ExecuteTest(method, "Com.ColorX.ColorX(int)");
            }
        }

        protected override void Property()
        {
            // Is

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    bool result = colorX.IsEmpty;
                };

                ExecuteTest(method, "Com.ColorX.IsEmpty.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    bool result = colorX.IsTransparent;
                };

                ExecuteTest(method, "Com.ColorX.IsTransparent.get()");
            }

            // Opacity

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    double result = colorX.Opacity;
                };

                ExecuteTest(method, "Com.ColorX.Opacity.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    colorX.Opacity = value;
                };

                ExecuteTest(method, "Com.ColorX.Opacity.set(double)");
            }

            // RGB

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    double result = colorX.Alpha;
                };

                ExecuteTest(method, "Com.ColorX.Alpha.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(255);

                Action method = () =>
                {
                    colorX.Alpha = value;
                };

                ExecuteTest(method, "Com.ColorX.Alpha.set(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    double result = colorX.Red;
                };

                ExecuteTest(method, "Com.ColorX.Red.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(255);

                Action method = () =>
                {
                    colorX.Red = value;
                };

                ExecuteTest(method, "Com.ColorX.Red.set(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    double result = colorX.Green;
                };

                ExecuteTest(method, "Com.ColorX.Green.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(255);

                Action method = () =>
                {
                    colorX.Green = value;
                };

                ExecuteTest(method, "Com.ColorX.Green.set(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    double result = colorX.Blue;
                };

                ExecuteTest(method, "Com.ColorX.Blue.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(255);

                Action method = () =>
                {
                    colorX.Blue = value;
                };

                ExecuteTest(method, "Com.ColorX.Blue.set(double)");
            }

            // HSV

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    double result = colorX.Hue_HSV;
                };

                ExecuteTest(method, "Com.ColorX.Hue_HSV.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(360);

                Action method = () =>
                {
                    colorX.Hue_HSV = value;
                };

                ExecuteTest(method, "Com.ColorX.Hue_HSV.set(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    double result = colorX.Saturation_HSV;
                };

                ExecuteTest(method, "Com.ColorX.Saturation_HSV.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    colorX.Saturation_HSV = value;
                };

                ExecuteTest(method, "Com.ColorX.Saturation_HSV.set(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    double result = colorX.Brightness;
                };

                ExecuteTest(method, "Com.ColorX.Brightness.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    colorX.Brightness = value;
                };

                ExecuteTest(method, "Com.ColorX.Brightness.set(double)");
            }

            // HSL

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    double result = colorX.Hue_HSL;
                };

                ExecuteTest(method, "Com.ColorX.Hue_HSL.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(360);

                Action method = () =>
                {
                    colorX.Hue_HSL = value;
                };

                ExecuteTest(method, "Com.ColorX.Hue_HSL.set(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    double result = colorX.Saturation_HSL;
                };

                ExecuteTest(method, "Com.ColorX.Saturation_HSL.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    colorX.Saturation_HSL = value;
                };

                ExecuteTest(method, "Com.ColorX.Saturation_HSL.set(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    double result = colorX.Lightness_HSL;
                };

                ExecuteTest(method, "Com.ColorX.Lightness_HSL.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    colorX.Lightness_HSL = value;
                };

                ExecuteTest(method, "Com.ColorX.Lightness_HSL.set(double)");
            }

            // CMYK

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    double result = colorX.Cyan;
                };

                ExecuteTest(method, "Com.ColorX.Cyan.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    colorX.Cyan = value;
                };

                ExecuteTest(method, "Com.ColorX.Cyan.set(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    double result = colorX.Magenta;
                };

                ExecuteTest(method, "Com.ColorX.Magenta.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    colorX.Magenta = value;
                };

                ExecuteTest(method, "Com.ColorX.Magenta.set(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    double result = colorX.Yellow;
                };

                ExecuteTest(method, "Com.ColorX.Yellow.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    colorX.Yellow = value;
                };

                ExecuteTest(method, "Com.ColorX.Yellow.set(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    double result = colorX.Black;
                };

                ExecuteTest(method, "Com.ColorX.Black.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    colorX.Black = value;
                };

                ExecuteTest(method, "Com.ColorX.Black.set(double)");
            }

            // LAB

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    double result = colorX.Lightness_LAB;
                };

                ExecuteTest(method, "Com.ColorX.Lightness_LAB.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    colorX.Lightness_LAB = value;
                };

                ExecuteTest(method, "Com.ColorX.Lightness_LAB.set(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    double result = colorX.GreenRed;
                };

                ExecuteTest(method, "Com.ColorX.GreenRed.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(-128, 128);

                Action method = () =>
                {
                    colorX.GreenRed = value;
                };

                ExecuteTest(method, "Com.ColorX.GreenRed.set(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    double result = colorX.BlueYellow;
                };

                ExecuteTest(method, "Com.ColorX.BlueYellow.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(-128, 128);

                Action method = () =>
                {
                    colorX.BlueYellow = value;
                };

                ExecuteTest(method, "Com.ColorX.BlueYellow.set(double)");
            }

            // 向量

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    Com.PointD3D result = colorX.RGB;
                };

                ExecuteTest(method, "Com.ColorX.RGB.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                Com.PointD3D value = Com.ColorManipulation.GetRandomColorX().RGB;

                Action method = () =>
                {
                    colorX.RGB = value;
                };

                ExecuteTest(method, "Com.ColorX.RGB.set(Com.PointD3D)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    Com.PointD3D result = colorX.HSV;
                };

                ExecuteTest(method, "Com.ColorX.HSV.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                Com.PointD3D value = Com.ColorManipulation.GetRandomColorX().HSV;

                Action method = () =>
                {
                    colorX.HSV = value;
                };

                ExecuteTest(method, "Com.ColorX.HSV.set(Com.PointD3D)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    Com.PointD3D result = colorX.HSL;
                };

                ExecuteTest(method, "Com.ColorX.HSL.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                Com.PointD3D value = Com.ColorManipulation.GetRandomColorX().HSL;

                Action method = () =>
                {
                    colorX.HSL = value;
                };

                ExecuteTest(method, "Com.ColorX.HSL.set(Com.PointD3D)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    Com.PointD4D result = colorX.CMYK;
                };

                ExecuteTest(method, "Com.ColorX.CMYK.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                Com.PointD4D value = Com.ColorManipulation.GetRandomColorX().CMYK;

                Action method = () =>
                {
                    colorX.CMYK = value;
                };

                ExecuteTest(method, "Com.ColorX.CMYK.set(Com.PointD4D)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    Com.PointD3D result = colorX.LAB;
                };

                ExecuteTest(method, "Com.ColorX.LAB.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                Com.PointD3D value = Com.ColorManipulation.GetRandomColorX().LAB;

                Action method = () =>
                {
                    colorX.LAB = value;
                };

                ExecuteTest(method, "Com.ColorX.LAB.set(Com.PointD3D)");
            }

            // 互补色，灰度

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    Com.ColorX result = colorX.ComplementaryColor;
                };

                ExecuteTest(method, "Com.ColorX.ComplementaryColor.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    Com.ColorX result = colorX.GrayscaleColor;
                };

                ExecuteTest(method, "Com.ColorX.GrayscaleColor.get()");
            }

            // HexCode

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    string result = colorX.ARGBHexCode;
                };

                ExecuteTest(method, "Com.ColorX.ARGBHexCode.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    string result = colorX.RGBHexCode;
                };

                ExecuteTest(method, "Com.ColorX.RGBHexCode.get()");
            }
        }

        protected override void StaticProperty()
        {

        }

        protected override void Method()
        {
            // object

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                object obj = (object)colorX;

                Action method = () =>
                {
                    bool result = colorX.Equals(obj);
                };

                ExecuteTest(method, "Com.ColorX.Equals(object)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    int result = colorX.GetHashCode();
                };

                ExecuteTest(method, "Com.ColorX.GetHashCode()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    string result = colorX.ToString();
                };

                ExecuteTest(method, "Com.ColorX.ToString()");
            }

            // Equals

            {
                Com.ColorX left = Com.ColorManipulation.GetRandomColorX();
                Com.ColorX right = left;

                Action method = () =>
                {
                    bool result = left.Equals(right);
                };

                ExecuteTest(method, "Com.ColorX.Equals(Com.ColorX)");
            }

            // To

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    Color result = colorX.ToColor();
                };

                ExecuteTest(method, "Com.ColorX.ToColor()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    int result = colorX.ToARGB();
                };

                ExecuteTest(method, "Com.ColorX.ToARGB()");
            }

            // AtOpacity

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Com.ColorX result = colorX.AtOpacity(value);
                };

                ExecuteTest(method, "Com.ColorX.AtOpacity(double)");
            }

            // AtRGB

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(255);

                Action method = () =>
                {
                    Com.ColorX result = colorX.AtAlpha(value);
                };

                ExecuteTest(method, "Com.ColorX.AtAlpha(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(255);

                Action method = () =>
                {
                    Com.ColorX result = colorX.AtRed(value);
                };

                ExecuteTest(method, "Com.ColorX.AtRed(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(255);

                Action method = () =>
                {
                    Com.ColorX result = colorX.AtGreen(value);
                };

                ExecuteTest(method, "Com.ColorX.AtGreen(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(255);

                Action method = () =>
                {
                    Com.ColorX result = colorX.AtBlue(value);
                };

                ExecuteTest(method, "Com.ColorX.AtBlue(double)");
            }

            // AtHSV

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(360);

                Action method = () =>
                {
                    Com.ColorX result = colorX.AtHue_HSV(value);
                };

                ExecuteTest(method, "Com.ColorX.AtHue_HSV(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Com.ColorX result = colorX.AtSaturation_HSV(value);
                };

                ExecuteTest(method, "Com.ColorX.AtSaturation_HSV(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Com.ColorX result = colorX.AtBrightness(value);
                };

                ExecuteTest(method, "Com.ColorX.AtBrightness(double)");
            }

            // HSL

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(360);

                Action method = () =>
                {
                    Com.ColorX result = colorX.AtHue_HSL(value);
                };

                ExecuteTest(method, "Com.ColorX.AtHue_HSL(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Com.ColorX result = colorX.AtSaturation_HSL(value);
                };

                ExecuteTest(method, "Com.ColorX.AtSaturation_HSL(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Com.ColorX result = colorX.AtLightness_HSL(value);
                };

                ExecuteTest(method, "Com.ColorX.AtLightness_HSL(double)");
            }

            // AtCMYK

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Com.ColorX result = colorX.AtCyan(value);
                };

                ExecuteTest(method, "Com.ColorX.AtCyan(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Com.ColorX result = colorX.AtMagenta(value);
                };

                ExecuteTest(method, "Com.ColorX.AtMagenta(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Com.ColorX result = colorX.AtYellow(value);
                };

                ExecuteTest(method, "Com.ColorX.AtYellow(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Com.ColorX result = colorX.AtBlack(value);
                };

                ExecuteTest(method, "Com.ColorX.AtBlack(double)");
            }

            // AtLAB

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Com.ColorX result = colorX.AtLightness_LAB(value);
                };

                ExecuteTest(method, "Com.ColorX.AtLightness_LAB(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(-128, 128);

                Action method = () =>
                {
                    Com.ColorX result = colorX.AtGreenRed(value);
                };

                ExecuteTest(method, "Com.ColorX.AtGreenRed(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(-128, 128);

                Action method = () =>
                {
                    Com.ColorX result = colorX.AtBlueYellow(value);
                };

                ExecuteTest(method, "Com.ColorX.AtBlueYellow(double)");
            }
        }

        protected override void StaticMethod()
        {
            // Equals

            {
                Com.ColorX left = Com.ColorManipulation.GetRandomColorX();
                Com.ColorX right = left;

                Action method = () =>
                {
                    bool result = Com.ColorX.Equals(left, right);
                };

                ExecuteTest(method, "Com.ColorX.Equals(Com.ColorX, Com.ColorX)");
            }

            // FromColor

            {
                int alpha = Com.Statistics.RandomInteger();
                Color color = Com.ColorManipulation.GetRandomColor();

                Action method = () =>
                {
                    Com.ColorX result = Com.ColorX.FromColor(alpha, color);
                };

                ExecuteTest(method, "Com.ColorX.FromColor(int, System.Drawing.Color)");
            }

            {
                Color color = Com.ColorManipulation.GetRandomColor();

                Action method = () =>
                {
                    Com.ColorX result = Com.ColorX.FromColor(color);
                };

                ExecuteTest(method, "Com.ColorX.FromColor(System.Drawing.Color)");
            }

            // FromRGB

            {
                double alpha = Com.Statistics.RandomDouble(255);
                double red = Com.Statistics.RandomDouble(255);
                double green = Com.Statistics.RandomDouble(255);
                double blue = Com.Statistics.RandomDouble(255);

                Action method = () =>
                {
                    Com.ColorX result = Com.ColorX.FromRGB(alpha, red, green, blue);
                };

                ExecuteTest(method, "Com.ColorX.FromRGB(double, double, double, double)");
            }

            {
                double red = Com.Statistics.RandomDouble(255);
                double green = Com.Statistics.RandomDouble(255);
                double blue = Com.Statistics.RandomDouble(255);

                Action method = () =>
                {
                    Com.ColorX result = Com.ColorX.FromRGB(red, green, blue);
                };

                ExecuteTest(method, "Com.ColorX.FromRGB( double, double, double)");
            }

            {
                double alpha = Com.Statistics.RandomDouble(255);
                double red = Com.Statistics.RandomDouble(255);
                double green = Com.Statistics.RandomDouble(255);
                double blue = Com.Statistics.RandomDouble(255);
                Com.PointD3D rgb = new Com.PointD3D(red, green, blue);

                Action method = () =>
                {
                    Com.ColorX result = Com.ColorX.FromRGB(alpha, rgb);
                };

                ExecuteTest(method, "Com.ColorX.FromRGB(double, Com.PointD3D)");
            }

            {
                double red = Com.Statistics.RandomDouble(255);
                double green = Com.Statistics.RandomDouble(255);
                double blue = Com.Statistics.RandomDouble(255);
                Com.PointD3D rgb = new Com.PointD3D(red, green, blue);

                Action method = () =>
                {
                    Com.ColorX result = Com.ColorX.FromRGB(rgb);
                };

                ExecuteTest(method, "Com.ColorX.FromRGB(Com.PointD3D)");
            }

            {
                int argb = Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    Com.ColorX result = Com.ColorX.FromRGB(argb);
                };

                ExecuteTest(method, "Com.ColorX.FromRGB(int)");
            }

            // FromHSV

            {
                double hue = Com.Statistics.RandomDouble(360);
                double saturation = Com.Statistics.RandomDouble(100);
                double brightness = Com.Statistics.RandomDouble(100);
                double opacity = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Com.ColorX result = Com.ColorX.FromHSV(hue, saturation, brightness, opacity);
                };

                ExecuteTest(method, "Com.ColorX.FromHSV(double, double, double, double)");
            }

            {
                double hue = Com.Statistics.RandomDouble(360);
                double saturation = Com.Statistics.RandomDouble(100);
                double brightness = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Com.ColorX result = Com.ColorX.FromHSV(hue, saturation, brightness);
                };

                ExecuteTest(method, "Com.ColorX.FromHSV(double, double, double)");
            }

            {
                double hue = Com.Statistics.RandomDouble(360);
                double saturation = Com.Statistics.RandomDouble(100);
                double brightness = Com.Statistics.RandomDouble(100);
                Com.PointD3D hsv = new Com.PointD3D(hue, saturation, brightness);
                double opacity = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Com.ColorX result = Com.ColorX.FromHSV(hsv, opacity);
                };

                ExecuteTest(method, "Com.ColorX.FromHSV(Com.PointD3D, double)");
            }

            {
                double hue = Com.Statistics.RandomDouble(360);
                double saturation = Com.Statistics.RandomDouble(100);
                double brightness = Com.Statistics.RandomDouble(100);
                Com.PointD3D hsv = new Com.PointD3D(hue, saturation, brightness);

                Action method = () =>
                {
                    Com.ColorX result = Com.ColorX.FromHSV(hsv);
                };

                ExecuteTest(method, "Com.ColorX.FromHSV(Com.PointD3D)");
            }

            // FromHSL

            {
                double hue = Com.Statistics.RandomDouble(360);
                double saturation = Com.Statistics.RandomDouble(100);
                double lightness = Com.Statistics.RandomDouble(100);
                double opacity = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Com.ColorX result = Com.ColorX.FromHSL(hue, saturation, lightness, opacity);
                };

                ExecuteTest(method, "Com.ColorX.FromHSL(double, double, double, double)");
            }

            {
                double hue = Com.Statistics.RandomDouble(360);
                double saturation = Com.Statistics.RandomDouble(100);
                double lightness = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Com.ColorX result = Com.ColorX.FromHSL(hue, saturation, lightness);
                };

                ExecuteTest(method, "Com.ColorX.FromHSL(double, double, double)");
            }

            {
                double hue = Com.Statistics.RandomDouble(360);
                double saturation = Com.Statistics.RandomDouble(100);
                double lightness = Com.Statistics.RandomDouble(100);
                Com.PointD3D hsl = new Com.PointD3D(hue, saturation, lightness);
                double opacity = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Com.ColorX result = Com.ColorX.FromHSL(hsl, opacity);
                };

                ExecuteTest(method, "Com.ColorX.FromHSL(Com.PointD3D, double)");
            }

            {
                double hue = Com.Statistics.RandomDouble(360);
                double saturation = Com.Statistics.RandomDouble(100);
                double lightness = Com.Statistics.RandomDouble(100);
                Com.PointD3D hsl = new Com.PointD3D(hue, saturation, lightness);

                Action method = () =>
                {
                    Com.ColorX result = Com.ColorX.FromHSL(hsl);
                };

                ExecuteTest(method, "Com.ColorX.FromHSL(Com.PointD3D)");
            }

            // FromCMYK

            {
                double cyan = Com.Statistics.RandomDouble(100);
                double magenta = Com.Statistics.RandomDouble(100);
                double yellow = Com.Statistics.RandomDouble(100);
                double black = Com.Statistics.RandomDouble(100);
                double opacity = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Com.ColorX result = Com.ColorX.FromCMYK(cyan, magenta, yellow, black, opacity);
                };

                ExecuteTest(method, "Com.ColorX.FromCMYK(double, double, double, double, double)");
            }

            {
                double cyan = Com.Statistics.RandomDouble(100);
                double magenta = Com.Statistics.RandomDouble(100);
                double yellow = Com.Statistics.RandomDouble(100);
                double black = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Com.ColorX result = Com.ColorX.FromCMYK(cyan, magenta, yellow, black);
                };

                ExecuteTest(method, "Com.ColorX.FromCMYK(double, double, double, double)");
            }

            {
                double cyan = Com.Statistics.RandomDouble(100);
                double magenta = Com.Statistics.RandomDouble(100);
                double yellow = Com.Statistics.RandomDouble(100);
                double black = Com.Statistics.RandomDouble(100);
                Com.PointD4D cmyk = new Com.PointD4D(cyan, magenta, yellow, black);
                double opacity = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Com.ColorX result = Com.ColorX.FromCMYK(cmyk, opacity);
                };

                ExecuteTest(method, "Com.ColorX.FromCMYK(Com.PointD4D, double)");
            }

            {
                double cyan = Com.Statistics.RandomDouble(100);
                double magenta = Com.Statistics.RandomDouble(100);
                double yellow = Com.Statistics.RandomDouble(100);
                double black = Com.Statistics.RandomDouble(100);
                Com.PointD4D cmyk = new Com.PointD4D(cyan, magenta, yellow, black);

                Action method = () =>
                {
                    Com.ColorX result = Com.ColorX.FromCMYK(cmyk);
                };

                ExecuteTest(method, "Com.ColorX.FromCMYK(Com.PointD4D)");
            }

            // FromLAB

            {
                double lightness = Com.Statistics.RandomDouble(100);
                double greenRed = Com.Statistics.RandomDouble(-128, 128);
                double blueYellow = Com.Statistics.RandomDouble(-128, 128);
                double opacity = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Com.ColorX result = Com.ColorX.FromLAB(lightness, greenRed, blueYellow, opacity);
                };

                ExecuteTest(method, "Com.ColorX.FromLAB(double, double, double, double)");
            }

            {
                double lightness = Com.Statistics.RandomDouble(100);
                double greenRed = Com.Statistics.RandomDouble(-128, 128);
                double blueYellow = Com.Statistics.RandomDouble(-128, 128);

                Action method = () =>
                {
                    Com.ColorX result = Com.ColorX.FromLAB(lightness, greenRed, blueYellow);
                };

                ExecuteTest(method, "Com.ColorX.FromLAB(double, double, double)");
            }

            {
                double lightness = Com.Statistics.RandomDouble(100);
                double greenRed = Com.Statistics.RandomDouble(-128, 128);
                double blueYellow = Com.Statistics.RandomDouble(-128, 128);
                Com.PointD3D lab = new Com.PointD3D(lightness, greenRed, blueYellow);
                double opacity = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    Com.ColorX result = Com.ColorX.FromLAB(lab, opacity);
                };

                ExecuteTest(method, "Com.ColorX.FromLAB(Com.PointD3D, double)");
            }

            {
                double lightness = Com.Statistics.RandomDouble(100);
                double greenRed = Com.Statistics.RandomDouble(-128, 128);
                double blueYellow = Com.Statistics.RandomDouble(-128, 128);
                Com.PointD3D lab = new Com.PointD3D(lightness, greenRed, blueYellow);

                Action method = () =>
                {
                    Com.ColorX result = Com.ColorX.FromLAB(lab);
                };

                ExecuteTest(method, "Com.ColorX.FromLAB(Com.PointD3D)");
            }

            // FromHexCode

            {
                string hexCode = Com.ColorManipulation.GetRandomColorX().ARGBHexCode;

                Action method = () =>
                {
                    Com.ColorX result = Com.ColorX.FromHexCode(hexCode);
                };

                ExecuteTest(method, "Com.ColorX.FromHexCode(string)");
            }

            // RandomColor

            {
                Action method = () =>
                {
                    Com.ColorX result = Com.ColorX.RandomColor();
                };

                ExecuteTest(method, "Com.ColorX.RandomColor()");
            }
        }

        protected override void Operator()
        {
            // 比较

            {
                Com.ColorX left = Com.ColorManipulation.GetRandomColorX();
                Com.ColorX right = left;

                Action method = () =>
                {
                    bool result = (left == right);
                };

                ExecuteTest(method, "Com.ColorX.operator ==(Com.ColorX, Com.ColorX)");
            }

            {
                Com.ColorX left = Com.ColorManipulation.GetRandomColorX();
                Com.ColorX right = left;

                Action method = () =>
                {
                    bool result = (left != right);
                };

                ExecuteTest(method, "Com.ColorX.operator !=(Com.ColorX, Com.ColorX)");
            }
        }
    }

    sealed class ComplexTest : ClassPerformanceTestBase
    {
        private static Com.Complex _GetRandomComplex()
        {
            return new Com.Complex(Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18));
        }

        //

        protected override void Constructor()
        {
            {
                double real = Com.Statistics.RandomDouble(-1E18, 1E18);
                double image = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Complex complex = new Com.Complex(real, image);
                };

                ExecuteTest(method, "Com.Complex.Complex(double, double)");
            }

            {
                double real = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Complex complex = new Com.Complex(real);
                };

                ExecuteTest(method, "Com.Complex.Complex(double)");
            }

            {
                Com.PointD pt = new Com.PointD(Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    Com.Complex complex = new Com.Complex(pt);
                };

                ExecuteTest(method, "Com.Complex.Complex(Com.PointD)");
            }
        }

        protected override void Property()
        {
            // Is

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    bool result = comp.IsNaN;
                };

                ExecuteTest(method, "Com.Complex.IsNaN.get()");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    bool result = comp.IsInfinity;
                };

                ExecuteTest(method, "Com.Complex.IsInfinity.get()");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    bool result = comp.IsNaNOrInfinity;
                };

                ExecuteTest(method, "Com.Complex.IsNaNOrInfinity.get()");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    bool result = comp.IsZero;
                };

                ExecuteTest(method, "Com.Complex.IsZero.get()");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    bool result = comp.IsOne;
                };

                ExecuteTest(method, "Com.Complex.IsOne.get()");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    bool result = comp.IsI;
                };

                ExecuteTest(method, "Com.Complex.IsI.get()");
            }

            // 分量

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    double result = comp.Real;
                };

                ExecuteTest(method, "Com.Complex.Real.get()");
            }

            {
                Com.Complex comp = _GetRandomComplex();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    comp.Real = value;
                };

                ExecuteTest(method, "Com.Complex.Real.set(double)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    double result = comp.Image;
                };

                ExecuteTest(method, "Com.Complex.Image.get()");
            }

            {
                Com.Complex comp = _GetRandomComplex();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    comp.Image = value;
                };

                ExecuteTest(method, "Com.Complex.Image.set(double)");
            }

            // Mod，Arg，Conj

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    double result = comp.Module;
                };

                ExecuteTest(method, "Com.Complex.Module.get()");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    double result = comp.ModuleSquared;
                };

                ExecuteTest(method, "Com.Complex.ModuleSquared.get()");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    double result = comp.Argument;
                };

                ExecuteTest(method, "Com.Complex.Argument.get()");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    Com.Complex result = comp.Conjugate;
                };

                ExecuteTest(method, "Com.Complex.Conjugate.get()");
            }
        }

        protected override void StaticProperty()
        {

        }

        protected override void Method()
        {
            // object

            {
                Com.Complex comp = _GetRandomComplex();
                object obj = (object)comp;

                Action method = () =>
                {
                    bool result = comp.Equals(obj);
                };

                ExecuteTest(method, "Com.Complex.Equals(object)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    int result = comp.GetHashCode();
                };

                ExecuteTest(method, "Com.Complex.GetHashCode()");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    string result = comp.ToString();
                };

                ExecuteTest(method, "Com.Complex.ToString()");
            }

            // Equals

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = left;

                Action method = () =>
                {
                    bool result = left.Equals(right);
                };

                ExecuteTest(method, "Com.Complex.Equals(Com.Complex)");
            }

            // To

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    Com.PointD result = comp.ToPointD();
                };

                ExecuteTest(method, "Com.Complex.ToPointD()");
            }
        }

        protected override void StaticMethod()
        {
            // Equals

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = left;

                Action method = () =>
                {
                    bool result = Com.Complex.Equals(left, right);
                };

                ExecuteTest(method, "Com.Complex.Equals(Com.Complex, Com.Complex)");
            }

            // From

            {
                Com.PointD pt = new Com.PointD(Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    Com.Complex result = Com.Complex.FromPointD(pt);
                };

                ExecuteTest(method, "Com.Complex.FromPointD(Com.PointD)");
            }

            // 幂函数，指数函数，对数函数

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    Com.Complex result = Com.Complex.Sqr(comp);
                };

                ExecuteTest(method, "Com.Complex.Sqr(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    Com.Complex result = Com.Complex.Sqrt(comp);
                };

                ExecuteTest(method, "Com.Complex.Sqrt(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    Com.Complex result = Com.Complex.Exp(comp);
                };

                ExecuteTest(method, "Com.Complex.Exp(Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    Com.Complex result = Com.Complex.Pow(left, right);
                };

                ExecuteTest(method, "Com.Complex.Pow(Com.Complex, Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    Com.Complex result = Com.Complex.Log(comp);
                };

                ExecuteTest(method, "Com.Complex.Log(Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    Com.Complex result = Com.Complex.Log(left, right);
                };

                ExecuteTest(method, "Com.Complex.Log(Com.Complex, Com.Complex)");
            }

            // 三角函数

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    Com.Complex result = Com.Complex.Sin(comp);
                };

                ExecuteTest(method, "Com.Complex.Sin(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    Com.Complex result = Com.Complex.Cos(comp);
                };

                ExecuteTest(method, "Com.Complex.Cos(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    Com.Complex result = Com.Complex.Tan(comp);
                };

                ExecuteTest(method, "Com.Complex.Tan(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    Com.Complex result = Com.Complex.Asin(comp);
                };

                ExecuteTest(method, "Com.Complex.Asin(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    Com.Complex result = Com.Complex.Acos(comp);
                };

                ExecuteTest(method, "Com.Complex.Acos(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    Com.Complex result = Com.Complex.Atan(comp);
                };

                ExecuteTest(method, "Com.Complex.Atan(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    Com.Complex result = Com.Complex.Sinh(comp);
                };

                ExecuteTest(method, "Com.Complex.Sinh(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    Com.Complex result = Com.Complex.Cosh(comp);
                };

                ExecuteTest(method, "Com.Complex.Cosh(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    Com.Complex result = Com.Complex.Tanh(comp);
                };

                ExecuteTest(method, "Com.Complex.Tanh(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    Com.Complex result = Com.Complex.Asinh(comp);
                };

                ExecuteTest(method, "Com.Complex.Asinh(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    Com.Complex result = Com.Complex.Acosh(comp);
                };

                ExecuteTest(method, "Com.Complex.Acosh(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    Com.Complex result = Com.Complex.Atanh(comp);
                };

                ExecuteTest(method, "Com.Complex.Atanh(Com.Complex)");
            }

            // 初等函数

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    Com.Complex result = Com.Complex.Abs(comp);
                };

                ExecuteTest(method, "Com.Complex.Abs(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    Com.Complex result = Com.Complex.Sign(comp);
                };

                ExecuteTest(method, "Com.Complex.Sign(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    Com.Complex result = Com.Complex.Ceiling(comp);
                };

                ExecuteTest(method, "Com.Complex.Ceiling(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    Com.Complex result = Com.Complex.Floor(comp);
                };

                ExecuteTest(method, "Com.Complex.Floor(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    Com.Complex result = Com.Complex.Round(comp);
                };

                ExecuteTest(method, "Com.Complex.Round(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    Com.Complex result = Com.Complex.Truncate(comp);
                };

                ExecuteTest(method, "Com.Complex.Truncate(Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    Com.Complex result = Com.Complex.Max(left, right);
                };

                ExecuteTest(method, "Com.Complex.Max(Com.Complex, Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    Com.Complex result = Com.Complex.Min(left, right);
                };

                ExecuteTest(method, "Com.Complex.Min(Com.Complex, Com.Complex)");
            }
        }

        protected override void Operator()
        {
            // 比较

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = left;

                Action method = () =>
                {
                    bool result = (left == right);
                };

                ExecuteTest(method, "Com.Complex.operator ==(Com.Complex, Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = left;

                Action method = () =>
                {
                    bool result = (left != right);
                };

                ExecuteTest(method, "Com.Complex.operator !=(Com.Complex, Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    bool result = (left < right);
                };

                ExecuteTest(method, "Com.Complex.operator <(Com.Complex, Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    bool result = (left > right);
                };

                ExecuteTest(method, "Com.Complex.operator >(Com.Complex, Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    bool result = (left <= right);
                };

                ExecuteTest(method, "Com.Complex.operator <=(Com.Complex, Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    bool result = (left >= right);
                };

                ExecuteTest(method, "Com.Complex.operator >=(Com.Complex, Com.Complex)");
            }

            // 运算

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    Com.Complex result = +comp;
                };

                ExecuteTest(method, "Com.Complex.operator +(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    Com.Complex result = -comp;
                };

                ExecuteTest(method, "Com.Complex.operator -(Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    Com.Complex result = left + right;
                };

                ExecuteTest(method, "Com.Complex.operator +(Com.Complex, Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    Com.Complex result = left - right;
                };

                ExecuteTest(method, "Com.Complex.operator -(Com.Complex, Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    Com.Complex result = left * right;
                };

                ExecuteTest(method, "Com.Complex.operator *(Com.Complex, Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    Com.Complex result = left / right;
                };

                ExecuteTest(method, "Com.Complex.operator /(Com.Complex, Com.Complex)");
            }
        }
    }

    sealed class DateTimeXTest : ClassPerformanceTestBase
    {
        private static Com.DateTimeX _GetRandomDateTimeX()
        {
            decimal totalMilliseconds = (decimal)Com.Statistics.RandomDouble(-1E16, 1E16);
            double utcOffset = Com.Statistics.RandomDouble(-12, 12);

            return new Com.DateTimeX(totalMilliseconds, utcOffset);
        }

        private static DateTime _GetRandomDateTime()
        {
            int year = Com.Statistics.RandomInteger(1, 10000);
            int month = Com.Statistics.RandomInteger(1, 13);
            int day = Com.Statistics.RandomInteger(1, DateTime.DaysInMonth(year, month) + 1);
            int hour = Com.Statistics.RandomInteger(0, 24);
            int minute = Com.Statistics.RandomInteger(0, 60);
            int second = Com.Statistics.RandomInteger(0, 60);
            int millisecond = Com.Statistics.RandomInteger(0, 1000);

            return new DateTime(year, month, day, hour, minute, second, millisecond);
        }

        //

        protected override void Constructor()
        {
            // UtcOffset，TotalMS，YMD-hms

            {
                decimal totalMilliseconds = (decimal)Com.Statistics.RandomDouble(-1E16, 1E16);
                double utcOffset = Com.Statistics.RandomDouble(-12, 12);

                Action method = () =>
                {
                    Com.DateTimeX dateTimeX = new Com.DateTimeX(totalMilliseconds, utcOffset);
                };

                ExecuteTest(method, "Com.DateTimeX.DateTimeX(decimal, double)");
            }

            {
                decimal totalMilliseconds = (decimal)Com.Statistics.RandomDouble(-1E16, 1E16);

                Action method = () =>
                {
                    Com.DateTimeX dateTimeX = new Com.DateTimeX(totalMilliseconds);
                };

                ExecuteTest(method, "Com.DateTimeX.DateTimeX(decimal)");
            }

            {
                long year = Com.Statistics.RandomInteger(-9999, 10000);
                int month = Com.Statistics.RandomInteger(1, 13);
                int day = Com.Statistics.RandomInteger(1, Com.DateTimeX.DaysInMonth(year, month) + 1);
                int hour = Com.Statistics.RandomInteger(0, 24);
                int minute = Com.Statistics.RandomInteger(0, 60);
                int second = Com.Statistics.RandomInteger(0, 60);
                int millisecond = Com.Statistics.RandomInteger(0, 1000);
                double utcOffset = Com.Statistics.RandomDouble(-12, 12);

                Action method = () =>
                {
                    Com.DateTimeX dateTimeX = new Com.DateTimeX(year, month, day, hour, minute, second, millisecond, utcOffset);
                };

                ExecuteTest(method, "Com.DateTimeX.DateTimeX(long, int, int, int, int, int, int, double)");
            }

            {
                long year = Com.Statistics.RandomInteger(-9999, 10000);
                int month = Com.Statistics.RandomInteger(1, 13);
                int day = Com.Statistics.RandomInteger(1, Com.DateTimeX.DaysInMonth(year, month) + 1);
                int hour = Com.Statistics.RandomInteger(0, 24);
                int minute = Com.Statistics.RandomInteger(0, 60);
                int second = Com.Statistics.RandomInteger(0, 60);
                int millisecond = Com.Statistics.RandomInteger(0, 1000);

                Action method = () =>
                {
                    Com.DateTimeX dateTimeX = new Com.DateTimeX(year, month, day, hour, minute, second, millisecond);
                };

                ExecuteTest(method, "Com.DateTimeX.DateTimeX(long, int, int, int, int, int, int)");
            }

            {
                long year = Com.Statistics.RandomInteger(-9999, 10000);
                int month = Com.Statistics.RandomInteger(1, 13);
                int day = Com.Statistics.RandomInteger(1, Com.DateTimeX.DaysInMonth(year, month) + 1);
                int hour = Com.Statistics.RandomInteger(0, 24);
                int minute = Com.Statistics.RandomInteger(0, 60);
                int second = Com.Statistics.RandomInteger(0, 60);
                double utcOffset = Com.Statistics.RandomDouble(-12, 12);

                Action method = () =>
                {
                    Com.DateTimeX dateTimeX = new Com.DateTimeX(year, month, day, hour, minute, second, utcOffset);
                };

                ExecuteTest(method, "Com.DateTimeX.DateTimeX(long, int, int, int, int, int, double)");
            }

            {
                long year = Com.Statistics.RandomInteger(-9999, 10000);
                int month = Com.Statistics.RandomInteger(1, 13);
                int day = Com.Statistics.RandomInteger(1, Com.DateTimeX.DaysInMonth(year, month) + 1);
                int hour = Com.Statistics.RandomInteger(0, 24);
                int minute = Com.Statistics.RandomInteger(0, 60);
                int second = Com.Statistics.RandomInteger(0, 60);

                Action method = () =>
                {
                    Com.DateTimeX dateTimeX = new Com.DateTimeX(year, month, day, hour, minute, second);
                };

                ExecuteTest(method, "Com.DateTimeX.DateTimeX(long, int, int, int, int, int)");
            }

            {
                long year = Com.Statistics.RandomInteger(-9999, 10000);
                int month = Com.Statistics.RandomInteger(1, 13);
                int day = Com.Statistics.RandomInteger(1, Com.DateTimeX.DaysInMonth(year, month) + 1);
                int hour = Com.Statistics.RandomInteger(0, 24);
                int minute = Com.Statistics.RandomInteger(0, 60);
                double utcOffset = Com.Statistics.RandomDouble(-12, 12);

                Action method = () =>
                {
                    Com.DateTimeX dateTimeX = new Com.DateTimeX(year, month, day, hour, minute, utcOffset);
                };

                ExecuteTest(method, "Com.DateTimeX.DateTimeX(long, int, int, int, int, double)");
            }

            {
                long year = Com.Statistics.RandomInteger(-9999, 10000);
                int month = Com.Statistics.RandomInteger(1, 13);
                int day = Com.Statistics.RandomInteger(1, Com.DateTimeX.DaysInMonth(year, month) + 1);
                int hour = Com.Statistics.RandomInteger(0, 24);
                int minute = Com.Statistics.RandomInteger(0, 60);

                Action method = () =>
                {
                    Com.DateTimeX dateTimeX = new Com.DateTimeX(year, month, day, hour, minute);
                };

                ExecuteTest(method, "Com.DateTimeX.DateTimeX(long, int, int, int, int)");
            }

            {
                long year = Com.Statistics.RandomInteger(-9999, 10000);
                int month = Com.Statistics.RandomInteger(1, 13);
                int day = Com.Statistics.RandomInteger(1, Com.DateTimeX.DaysInMonth(year, month) + 1);
                int hour = Com.Statistics.RandomInteger(0, 24);
                double utcOffset = Com.Statistics.RandomDouble(-12, 12);

                Action method = () =>
                {
                    Com.DateTimeX dateTimeX = new Com.DateTimeX(year, month, day, hour, utcOffset);
                };

                ExecuteTest(method, "Com.DateTimeX.DateTimeX(long, int, int, int, double)");
            }

            {
                long year = Com.Statistics.RandomInteger(-9999, 10000);
                int month = Com.Statistics.RandomInteger(1, 13);
                int day = Com.Statistics.RandomInteger(1, Com.DateTimeX.DaysInMonth(year, month) + 1);
                int hour = Com.Statistics.RandomInteger(0, 24);

                Action method = () =>
                {
                    Com.DateTimeX dateTimeX = new Com.DateTimeX(year, month, day, hour);
                };

                ExecuteTest(method, "Com.DateTimeX.DateTimeX(long, int, int, int)");
            }

            {
                long year = Com.Statistics.RandomInteger(-9999, 10000);
                int month = Com.Statistics.RandomInteger(1, 13);
                int day = Com.Statistics.RandomInteger(1, Com.DateTimeX.DaysInMonth(year, month) + 1);
                double utcOffset = Com.Statistics.RandomDouble(-12, 12);

                Action method = () =>
                {
                    Com.DateTimeX dateTimeX = new Com.DateTimeX(year, month, day, utcOffset);
                };

                ExecuteTest(method, "Com.DateTimeX.DateTimeX(long, int, int, double)");
            }

            {
                long year = Com.Statistics.RandomInteger(-9999, 10000);
                int month = Com.Statistics.RandomInteger(1, 13);
                int day = Com.Statistics.RandomInteger(1, Com.DateTimeX.DaysInMonth(year, month) + 1);

                Action method = () =>
                {
                    Com.DateTimeX dateTimeX = new Com.DateTimeX(year, month, day);
                };

                ExecuteTest(method, "Com.DateTimeX.DateTimeX(long, int, int)");
            }

            {
                long year = Com.Statistics.RandomInteger(-9999, 10000);
                int month = Com.Statistics.RandomInteger(1, 13);
                double utcOffset = Com.Statistics.RandomDouble(-12, 12);

                Action method = () =>
                {
                    Com.DateTimeX dateTimeX = new Com.DateTimeX(year, month, utcOffset);
                };

                ExecuteTest(method, "Com.DateTimeX.DateTimeX(long, int, double)");
            }

            {
                long year = Com.Statistics.RandomInteger(-9999, 10000);
                int month = Com.Statistics.RandomInteger(1, 13);

                Action method = () =>
                {
                    Com.DateTimeX dateTimeX = new Com.DateTimeX(year, month);
                };

                ExecuteTest(method, "Com.DateTimeX.DateTimeX(long, int)");
            }

            {
                long year = Com.Statistics.RandomInteger(-9999, 10000);
                double utcOffset = Com.Statistics.RandomDouble(-12, 12);

                Action method = () =>
                {
                    Com.DateTimeX dateTimeX = new Com.DateTimeX(year, utcOffset);
                };

                ExecuteTest(method, "Com.DateTimeX.DateTimeX(long, double)");
            }

            {
                long year = Com.Statistics.RandomInteger(-9999, 10000);

                Action method = () =>
                {
                    Com.DateTimeX dateTimeX = new Com.DateTimeX(year);
                };

                ExecuteTest(method, "Com.DateTimeX.DateTimeX(long)");
            }

            // UtcOffset，DateTimeX，DateTime

            {
                Com.DateTimeX dateTime = _GetRandomDateTimeX();
                double utcOffset = Com.Statistics.RandomDouble(-12, 12);

                Action method = () =>
                {
                    Com.DateTimeX dateTimeX = new Com.DateTimeX(dateTime, utcOffset);
                };

                ExecuteTest(method, "Com.DateTimeX.DateTimeX(Com.DateTimeX, double)");
            }

            {
                Com.DateTimeX dateTime = _GetRandomDateTimeX();

                Action method = () =>
                {
                    Com.DateTimeX dateTimeX = new Com.DateTimeX(dateTime);
                };

                ExecuteTest(method, "Com.DateTimeX.DateTimeX(Com.DateTimeX)");
            }

            {
                DateTime dateTime = _GetRandomDateTime();
                double utcOffset = Com.Statistics.RandomDouble(-12, 12);

                Action method = () =>
                {
                    Com.DateTimeX dateTimeX = new Com.DateTimeX(dateTime, utcOffset);
                };

                ExecuteTest(method, "Com.DateTimeX.DateTimeX(System.DateTime, double)");
            }

            {
                DateTime dateTime = _GetRandomDateTime();

                Action method = () =>
                {
                    Com.DateTimeX dateTimeX = new Com.DateTimeX(dateTime);
                };

                ExecuteTest(method, "Com.DateTimeX.DateTimeX(System.DateTime)");
            }
        }

        protected override void Property()
        {
            // Is

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    bool result = dateTimeX.IsEmpty;
                };

                ExecuteTest(method, "Com.DateTimeX.IsEmpty.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    bool result = dateTimeX.IsChristianEra;
                };

                ExecuteTest(method, "Com.DateTimeX.IsChristianEra.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    bool result = dateTimeX.IsMinValue;
                };

                ExecuteTest(method, "Com.DateTimeX.IsMinValue.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    bool result = dateTimeX.IsMaxValue;
                };

                ExecuteTest(method, "Com.DateTimeX.IsMaxValue.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    bool result = dateTimeX.IsAnnoDomini;
                };

                ExecuteTest(method, "Com.DateTimeX.IsAnnoDomini.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    bool result = dateTimeX.IsBeforeChrist;
                };

                ExecuteTest(method, "Com.DateTimeX.IsBeforeChrist.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    bool result = dateTimeX.IsLeap;
                };

                ExecuteTest(method, "Com.DateTimeX.IsLeap.get()");
            }

            // UtcOffset，TotalMS，YMD-hms

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    double result = dateTimeX.UtcOffset;
                };

                ExecuteTest(method, "Com.DateTimeX.UtcOffset.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                double value = Com.Statistics.RandomDouble(-12, 12);

                Action method = () =>
                {
                    dateTimeX.UtcOffset = value;
                };

                ExecuteTest(method, "Com.DateTimeX.UtcOffset.set(double)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    decimal result = dateTimeX.TotalMilliseconds;
                };

                ExecuteTest(method, "Com.DateTimeX.TotalMilliseconds.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                decimal value = (decimal)Com.Statistics.RandomDouble(-1E16, 1E16);

                Action method = () =>
                {
                    dateTimeX.TotalMilliseconds = value;
                };

                ExecuteTest(method, "Com.DateTimeX.TotalMilliseconds.set(decimal)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    long result = dateTimeX.Year;
                };

                ExecuteTest(method, "Com.DateTimeX.Year.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                long value = Com.Statistics.RandomInteger(-9999, 10000);

                Action method = () =>
                {
                    dateTimeX.Year = value;
                };

                ExecuteTest(method, "Com.DateTimeX.Year.set(long)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    int result = dateTimeX.Month;
                };

                ExecuteTest(method, "Com.DateTimeX.Month.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                int value = Com.Statistics.RandomInteger(1, 13);

                Action method = () =>
                {
                    dateTimeX.Month = value;
                };

                ExecuteTest(method, "Com.DateTimeX.Month.set(int)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    int result = dateTimeX.Day;
                };

                ExecuteTest(method, "Com.DateTimeX.Day.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                int value = Com.Statistics.RandomInteger(1, Com.DateTimeX.DaysInMonth(dateTimeX.Year, dateTimeX.Month) + 1);

                Action method = () =>
                {
                    dateTimeX.Day = value;
                };

                ExecuteTest(method, "Com.DateTimeX.Day.set(int)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    int result = dateTimeX.Hour;
                };

                ExecuteTest(method, "Com.DateTimeX.Hour.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                int value = Com.Statistics.RandomInteger(0, 24);

                Action method = () =>
                {
                    dateTimeX.Hour = value;
                };

                ExecuteTest(method, "Com.DateTimeX.Hour.set(int)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    int result = dateTimeX.Minute;
                };

                ExecuteTest(method, "Com.DateTimeX.Minute.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                int value = Com.Statistics.RandomInteger(0, 60);

                Action method = () =>
                {
                    dateTimeX.Minute = value;
                };

                ExecuteTest(method, "Com.DateTimeX.Minute.set(int)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    int result = dateTimeX.Second;
                };

                ExecuteTest(method, "Com.DateTimeX.Second.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                int value = Com.Statistics.RandomInteger(0, 60);

                Action method = () =>
                {
                    dateTimeX.Second = value;
                };

                ExecuteTest(method, "Com.DateTimeX.Second.set(int)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    int result = dateTimeX.Millisecond;
                };

                ExecuteTest(method, "Com.DateTimeX.Millisecond.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                int value = Com.Statistics.RandomInteger(0, 1000);

                Action method = () =>
                {
                    dateTimeX.Millisecond = value;
                };

                ExecuteTest(method, "Com.DateTimeX.Millisecond.set(int)");
            }

            // Date，TimeOfDay

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    Com.DateTimeX result = dateTimeX.Date;
                };

                ExecuteTest(method, "Com.DateTimeX.Date.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    TimeSpan result = dateTimeX.TimeOfDay;
                };

                ExecuteTest(method, "Com.DateTimeX.TimeOfDay.get()");
            }

            // Of

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    int result = dateTimeX.WeekOfYear;
                };

                ExecuteTest(method, "Com.DateTimeX.WeekOfYear.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    int result = dateTimeX.DayOfYear;
                };

                ExecuteTest(method, "Com.DateTimeX.DayOfYear.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    int result = dateTimeX.HourOfYear;
                };

                ExecuteTest(method, "Com.DateTimeX.HourOfYear.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    int result = dateTimeX.MinuteOfYear;
                };

                ExecuteTest(method, "Com.DateTimeX.MinuteOfYear.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    int result = dateTimeX.SecondOfYear;
                };

                ExecuteTest(method, "Com.DateTimeX.SecondOfYear.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    long result = dateTimeX.MillisecondOfYear;
                };

                ExecuteTest(method, "Com.DateTimeX.MillisecondOfYear.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    int result = dateTimeX.DayOfThisWeek;
                };

                ExecuteTest(method, "Com.DateTimeX.DayOfThisWeek.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    DayOfWeek result = dateTimeX.DayOfWeek;
                };

                ExecuteTest(method, "Com.DateTimeX.DayOfWeek.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    int result = dateTimeX.HourOfDay;
                };

                ExecuteTest(method, "Com.DateTimeX.HourOfDay.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    int result = dateTimeX.MinuteOfDay;
                };

                ExecuteTest(method, "Com.DateTimeX.MinuteOfDay.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    int result = dateTimeX.SecondOfDay;
                };

                ExecuteTest(method, "Com.DateTimeX.SecondOfDay.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    int result = dateTimeX.MillisecondOfDay;
                };

                ExecuteTest(method, "Com.DateTimeX.MillisecondOfDay.get()");
            }

            // String

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    string result = dateTimeX.MonthStringInChinese;
                };

                ExecuteTest(method, "Com.DateTimeX.MonthStringInChinese.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    string result = dateTimeX.MonthLongStringInEnglish;
                };

                ExecuteTest(method, "Com.DateTimeX.MonthLongStringInEnglish.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    string result = dateTimeX.MonthShortStringInEnglish;
                };

                ExecuteTest(method, "Com.DateTimeX.MonthShortStringInEnglish.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    string result = dateTimeX.MonthStringInJapaneseKanji;
                };

                ExecuteTest(method, "Com.DateTimeX.MonthStringInJapaneseKanji.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    string result = dateTimeX.MonthStringInJapaneseHiragana;
                };

                ExecuteTest(method, "Com.DateTimeX.MonthStringInJapaneseHiragana.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    string result = dateTimeX.WeekdayLongStringInChinese;
                };

                ExecuteTest(method, "Com.DateTimeX.WeekdayLongStringInChinese.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    string result = dateTimeX.WeekdayShortStringInChinese;
                };

                ExecuteTest(method, "Com.DateTimeX.WeekdayShortStringInChinese.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    string result = dateTimeX.WeekdayLongStringInEnglish;
                };

                ExecuteTest(method, "Com.DateTimeX.WeekdayLongStringInEnglish.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    string result = dateTimeX.WeekdayShortStringInEnglish;
                };

                ExecuteTest(method, "Com.DateTimeX.WeekdayShortStringInEnglish.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    string result = dateTimeX.WeekdayLongStringInJapanese;
                };

                ExecuteTest(method, "Com.DateTimeX.WeekdayLongStringInJapanese.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    string result = dateTimeX.WeekdayShortStringInJapanese;
                };

                ExecuteTest(method, "Com.DateTimeX.WeekdayShortStringInJapanese.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    string result = dateTimeX.DateLongString;
                };

                ExecuteTest(method, "Com.DateTimeX.DateLongString.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    string result = dateTimeX.DateShortString;
                };

                ExecuteTest(method, "Com.DateTimeX.DateShortString.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    string result = dateTimeX.TimeLongString;
                };

                ExecuteTest(method, "Com.DateTimeX.TimeLongString.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    string result = dateTimeX.TimeShortString;
                };

                ExecuteTest(method, "Com.DateTimeX.TimeShortString.get()");
            }
        }

        protected override void StaticProperty()
        {
            {
                Action method = () =>
                {
                    Com.DateTimeX result = Com.DateTimeX.Now;
                };

                ExecuteTest(method, "Com.DateTimeX.Now.get()");
            }

            {
                Action method = () =>
                {
                    Com.DateTimeX result = Com.DateTimeX.UtcNow;
                };

                ExecuteTest(method, "Com.DateTimeX.UtcNow.get()");
            }

            {
                Action method = () =>
                {
                    Com.DateTimeX result = Com.DateTimeX.Today;
                };

                ExecuteTest(method, "Com.DateTimeX.Today.get()");
            }

            {
                Action method = () =>
                {
                    Com.DateTimeX result = Com.DateTimeX.UtcToday;
                };

                ExecuteTest(method, "Com.DateTimeX.UtcToday.get()");
            }
        }

        protected override void Method()
        {
            // object

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                object obj = (object)dateTimeX;

                Action method = () =>
                {
                    bool result = dateTimeX.Equals(obj);
                };

                ExecuteTest(method, "Com.DateTimeX.Equals(object)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    int result = dateTimeX.GetHashCode();
                };

                ExecuteTest(method, "Com.DateTimeX.GetHashCode()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    string result = dateTimeX.ToString();
                };

                ExecuteTest(method, "Com.DateTimeX.ToString()");
            }

            // Equals

            {
                Com.DateTimeX left = _GetRandomDateTimeX();
                Com.DateTimeX right = left;

                Action method = () =>
                {
                    bool result = left.Equals(right);
                };

                ExecuteTest(method, "Com.DateTimeX.Equals(Com.DateTimeX)");
            }

            // Add

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                TimeSpan timeSpan = TimeSpan.FromMilliseconds(Com.Statistics.RandomInteger(-999, 1000) * 365.25 * 86400000);

                Action method = () =>
                {
                    Com.DateTimeX result = dateTimeX.Add(timeSpan);
                };

                ExecuteTest(method, "Com.DateTimeX.Add(TimeSpan)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                long years = Com.Statistics.RandomInteger(-999, 1000);

                Action method = () =>
                {
                    Com.DateTimeX result = dateTimeX.AddYears(years);
                };

                ExecuteTest(method, "Com.DateTimeX.AddYears(long)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                long months = Com.Statistics.RandomInteger(-999, 1000) * 12;

                Action method = () =>
                {
                    Com.DateTimeX result = dateTimeX.AddMonths(months);
                };

                ExecuteTest(method, "Com.DateTimeX.AddMonths(long)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                double weeks = Com.Statistics.RandomInteger(-999, 1000) * 52.18;

                Action method = () =>
                {
                    Com.DateTimeX result = dateTimeX.AddWeeks(weeks);
                };

                ExecuteTest(method, "Com.DateTimeX.AddWeeks(double)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                double days = Com.Statistics.RandomInteger(-999, 1000) * 365.25;

                Action method = () =>
                {
                    Com.DateTimeX result = dateTimeX.AddDays(days);
                };

                ExecuteTest(method, "Com.DateTimeX.AddDays(double)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                double hours = Com.Statistics.RandomInteger(-999, 1000) * 365.25 * 24;

                Action method = () =>
                {
                    Com.DateTimeX result = dateTimeX.AddHours(hours);
                };

                ExecuteTest(method, "Com.DateTimeX.AddHours(double)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                double minutes = Com.Statistics.RandomInteger(-999, 1000) * 365.25 * 1440;

                Action method = () =>
                {
                    Com.DateTimeX result = dateTimeX.AddMinutes(minutes);
                };

                ExecuteTest(method, "Com.DateTimeX.AddMinutes(double)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                double seconds = Com.Statistics.RandomInteger(-999, 1000) * 365.25 * 86400;

                Action method = () =>
                {
                    Com.DateTimeX result = dateTimeX.AddSeconds(seconds);
                };

                ExecuteTest(method, "Com.DateTimeX.AddSeconds(double)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                decimal milliseconds = (decimal)(Com.Statistics.RandomInteger(-999, 1000) * 365.25 * 86400000);

                Action method = () =>
                {
                    Com.DateTimeX result = dateTimeX.AddMilliseconds(milliseconds);
                };

                ExecuteTest(method, "Com.DateTimeX.AddMilliseconds(decimal)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                double milliseconds = Com.Statistics.RandomInteger(-999, 1000) * 365.25 * 86400000;

                Action method = () =>
                {
                    Com.DateTimeX result = dateTimeX.AddMilliseconds(milliseconds);
                };

                ExecuteTest(method, "Com.DateTimeX.AddMilliseconds(double)");
            }

            // To

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    Com.DateTimeX result = dateTimeX.ToLocalTime();
                };

                ExecuteTest(method, "Com.DateTimeX.ToLocalTime()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    Com.DateTimeX result = dateTimeX.ToUniversalTime();
                };

                ExecuteTest(method, "Com.DateTimeX.ToUniversalTime()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    string result = dateTimeX.ToLongDateString();
                };

                ExecuteTest(method, "Com.DateTimeX.ToLongDateString()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    string result = dateTimeX.ToShortDateString();
                };

                ExecuteTest(method, "Com.DateTimeX.ToShortDateString()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    string result = dateTimeX.ToLongTimeString();
                };

                ExecuteTest(method, "Com.DateTimeX.ToLongTimeString()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    string result = dateTimeX.ToShortTimeString();
                };

                ExecuteTest(method, "Com.DateTimeX.ToShortTimeString()");
            }
        }

        protected override void StaticMethod()
        {
            // Equals

            {
                Com.DateTimeX left = _GetRandomDateTimeX();
                Com.DateTimeX right = left;

                Action method = () =>
                {
                    bool result = Com.DateTimeX.Equals(left, right);
                };

                ExecuteTest(method, "Com.DateTimeX.Equals(Com.DateTimeX, Com.DateTimeX)");
            }

            // IsLeapYear

            {
                long year = Com.Statistics.RandomInteger(-999999, 1000000);

                Action method = () =>
                {
                    bool result = Com.DateTimeX.IsLeapYear(year);
                };

                ExecuteTest(method, "Com.DateTimeX.IsLeapYear(long)");
            }

            // Days

            {
                long year = Com.Statistics.RandomInteger(-999999, 1000000);

                Action method = () =>
                {
                    int result = Com.DateTimeX.DaysInYear(year);
                };

                ExecuteTest(method, "Com.DateTimeX.DaysInYear(long)");
            }

            {
                long year = Com.Statistics.RandomInteger(-999999, 1000000);
                int month = Com.Statistics.RandomInteger(1, 13);

                Action method = () =>
                {
                    int result = Com.DateTimeX.DaysInMonth(year, month);
                };

                ExecuteTest(method, "Com.DateTimeX.DaysInMonth(long, int)");
            }
        }

        protected override void Operator()
        {
            // 比较

            {
                Com.DateTimeX left = _GetRandomDateTimeX();
                Com.DateTimeX right = left;

                Action method = () =>
                {
                    bool result = (left == right);
                };

                ExecuteTest(method, "Com.DateTimeX.operator ==(Com.DateTimeX, Com.DateTimeX)");
            }

            {
                Com.DateTimeX left = _GetRandomDateTimeX();
                Com.DateTimeX right = left;

                Action method = () =>
                {
                    bool result = (left != right);
                };

                ExecuteTest(method, "Com.DateTimeX.operator !=(Com.DateTimeX, Com.DateTimeX)");
            }

            {
                Com.DateTimeX left = _GetRandomDateTimeX();
                Com.DateTimeX right = _GetRandomDateTimeX();

                Action method = () =>
                {
                    bool result = (left < right);
                };

                ExecuteTest(method, "Com.DateTimeX.operator <(Com.DateTimeX, Com.DateTimeX)");
            }

            {
                Com.DateTimeX left = _GetRandomDateTimeX();
                Com.DateTimeX right = _GetRandomDateTimeX();

                Action method = () =>
                {
                    bool result = (left > right);
                };

                ExecuteTest(method, "Com.DateTimeX.operator >(Com.DateTimeX, Com.DateTimeX)");
            }

            {
                Com.DateTimeX left = _GetRandomDateTimeX();
                Com.DateTimeX right = _GetRandomDateTimeX();

                Action method = () =>
                {
                    bool result = (left <= right);
                };

                ExecuteTest(method, "Com.DateTimeX.operator <=(Com.DateTimeX, Com.DateTimeX)");
            }

            {
                Com.DateTimeX left = _GetRandomDateTimeX();
                Com.DateTimeX right = _GetRandomDateTimeX();

                Action method = () =>
                {
                    bool result = (left >= right);
                };

                ExecuteTest(method, "Com.DateTimeX.operator >=(Com.DateTimeX, Com.DateTimeX)");
            }

            // 运算

            {
                Com.DateTimeX dateTime = _GetRandomDateTimeX();
                TimeSpan timeSpan = TimeSpan.FromMilliseconds(Com.Statistics.RandomInteger(-999, 1000) * 365.25 * 86400000);

                Action method = () =>
                {
                    Com.DateTimeX result = dateTime - timeSpan;
                };

                ExecuteTest(method, "Com.DateTimeX.operator +(Com.DateTimeX, TimeSpan)");
            }

            {
                Com.DateTimeX dateTime = _GetRandomDateTimeX();
                TimeSpan timeSpan = TimeSpan.FromMilliseconds(Com.Statistics.RandomInteger(-999, 1000) * 365.25 * 86400000);

                Action method = () =>
                {
                    Com.DateTimeX result = dateTime - timeSpan;
                };

                ExecuteTest(method, "Com.DateTimeX.operator -(Com.DateTimeX, TimeSpan)");
            }
        }
    }

    sealed class GeometryTest : ClassPerformanceTestBase
    {
        private static Com.PointD _GetRandomPointD()
        {
            return new Com.PointD(Com.Statistics.RandomDouble(-1E9, 1E9), Com.Statistics.RandomDouble(-1E9, 1E9));
        }

        private static RectangleF _GetRandomRectangleF()
        {
            return new RectangleF(new PointF((float)Com.Statistics.RandomDouble(-65536, 65536), (float)Com.Statistics.RandomDouble(-65536, 65536)), new SizeF((float)Com.Statistics.RandomDouble(65536), (float)Com.Statistics.RandomDouble(65536)));
        }

        private static Rectangle _GetRandomRectangle()
        {
            return new Rectangle(new Point(Com.Statistics.RandomInteger(-65536, 65536), Com.Statistics.RandomInteger(-65536, 65536)), new Size(Com.Statistics.RandomInteger(65536), Com.Statistics.RandomInteger(65536)));
        }

        //

        protected override void Constructor()
        {

        }

        protected override void Property()
        {

        }

        protected override void StaticProperty()
        {

        }

        protected override void Method()
        {

        }

        protected override void StaticMethod()
        {
            // 平面直角坐标系

            {
                Com.PointD pt = _GetRandomPointD();
                Com.PointD pt1 = _GetRandomPointD();
                Com.PointD pt2 = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = Com.Geometry.GetFootPoint(pt, pt1, pt2);
                };

                ExecuteTest(method, "Com.Geometry.GetFootPoint(Com.PointD, Com.PointD, Com.PointD)");
            }

            // 角度

            {
                Com.PointD pt1 = _GetRandomPointD();
                Com.PointD pt2 = _GetRandomPointD();

                Action method = () =>
                {
                    double result = Com.Geometry.GetAngleOfTwoPoints(pt1, pt2);
                };

                ExecuteTest(method, "Com.Geometry.GetAngleOfTwoPoints(Com.PointD, Com.PointD)");
            }

            {
                double angle = Com.Statistics.RandomDouble(-1E9, 1E9);

                Action method = () =>
                {
                    double result = Com.Geometry.AngleMapping(angle);
                };

                ExecuteTest(method, "Com.Geometry.AngleMapping(double)");
            }

            // 控件

            ExecuteTest(null, "Com.Geometry.GetCursorPositionOfControl(System.Windows.Forms.Control)");

            ExecuteTest(null, "Com.Geometry.CursorIsInControl(System.Windows.Forms.Control)");

            ExecuteTest(null, "Com.Geometry.PointIsInControl(System.Drawing.Point, System.Windows.Forms.Control)");

            ExecuteTest(null, "Com.Geometry.ScreenPointIsInControl(System.Drawing.Point, System.Windows.Forms.Control)");

            ExecuteTest(null, "Com.Geometry.GetMinimumBoundingRectangleOfControls(System.Windows.Forms.Control[], int)");

            ExecuteTest(null, "Com.Geometry.GetMinimumBoundingRectangleOfControls(System.Windows.Forms.Control[])");

            // 图形可见性

            {
                Com.PointD pt = _GetRandomPointD();
                RectangleF rect = _GetRandomRectangleF();

                Action method = () =>
                {
                    bool result = Com.Geometry.PointIsVisibleInRectangle(pt, rect);
                };

                ExecuteTest(method, "Com.Geometry.PointIsVisibleInRectangle(Com.PointD, System.Drawing.RectangleF)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                Com.PointD offset = _GetRandomPointD();
                double radius = Com.Statistics.RandomDouble(1E9);

                Action method = () =>
                {
                    bool result = Com.Geometry.PointIsVisibleInCircle(pt, offset, radius);
                };

                ExecuteTest(method, "Com.Geometry.PointIsVisibleInCircle(Com.PointD, Com.PointD, double)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                Com.PointD offset = _GetRandomPointD();
                double semiMajorAxis = Com.Statistics.RandomDouble(1E9);
                double eccentricity = Com.Statistics.RandomDouble();
                double rotateAngle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    bool result = Com.Geometry.PointIsVisibleInEllipse(pt, offset, semiMajorAxis, eccentricity, rotateAngle);
                };

                ExecuteTest(method, "Com.Geometry.PointIsVisibleInEllipse(Com.PointD, Com.PointD, double, double, double)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                Com.PointD offset = _GetRandomPointD();
                double semiMajorAxis = Com.Statistics.RandomDouble(1E9);
                double semiMinorAxis = Com.Statistics.RandomDouble(1E9);
                double rotateAngle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    bool result = Com.Geometry.PointIsVisibleInRhombus(pt, offset, semiMajorAxis, semiMinorAxis, rotateAngle);
                };

                ExecuteTest(method, "Com.Geometry.PointIsVisibleInRhombus(Com.PointD, Com.PointD, double, double, double)");
            }

            {
                Com.PointD pt1 = _GetRandomPointD();
                Com.PointD pt2 = _GetRandomPointD();
                RectangleF rect = _GetRandomRectangleF();

                Action method = () =>
                {
                    bool result = Com.Geometry.LineIsVisibleInRectangle(pt1, pt2, rect);
                };

                ExecuteTest(method, "Com.Geometry.LineIsVisibleInRectangle(Com.PointD, Com.PointD, System.Drawing.RectangleF)");
            }

            {
                Com.PointD pt1 = _GetRandomPointD();
                Com.PointD pt2 = _GetRandomPointD();
                Com.PointD offset = _GetRandomPointD();
                double radius = Com.Statistics.RandomDouble(1E9);

                Action method = () =>
                {
                    bool result = Com.Geometry.LineIsVisibleInCircle(pt1, pt2, offset, radius);
                };

                ExecuteTest(method, "Com.Geometry.LineIsVisibleInCircle(Com.PointD, Com.PointD, Com.PointD, double)");
            }

            {
                Com.PointD offset = _GetRandomPointD();
                double radius = Com.Statistics.RandomDouble(1E9);
                RectangleF rect = _GetRandomRectangleF();

                Action method = () =>
                {
                    bool result = Com.Geometry.CircleInnerIsVisibleInRectangle(offset, radius, rect);
                };

                ExecuteTest(method, "Com.Geometry.CircleInnerIsVisibleInRectangle(Com.PointD, double, System.Drawing.RectangleF)");
            }

            {
                Com.PointD offset = _GetRandomPointD();
                double radius = Com.Statistics.RandomDouble(1E9);
                RectangleF rect = _GetRandomRectangleF();

                Action method = () =>
                {
                    bool result = Com.Geometry.CircumferenceIsVisibleInRectangle(offset, radius, rect);
                };

                ExecuteTest(method, "Com.Geometry.CircumferenceIsVisibleInRectangle(Com.PointD, double, System.Drawing.RectangleF)");
            }

            // 圆锥曲线

            {
                double semiMajorAxis = Com.Statistics.RandomDouble(1E9);
                double eccentricity = Com.Statistics.RandomDouble();
                double phase = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    double result = Com.Geometry.GetRadiusOfEllipse(semiMajorAxis, eccentricity, phase);
                };

                ExecuteTest(method, "Com.Geometry.GetRadiusOfEllipse(double, double, double)");
            }

            {
                double semiMajorAxis = Com.Statistics.RandomDouble(1E9);
                double eccentricity = Com.Statistics.RandomDouble();
                double phase = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    double result = Com.Geometry.GetFocalRadiusOfEllipse(semiMajorAxis, eccentricity, phase);
                };

                ExecuteTest(method, "Com.Geometry.GetFocalRadiusOfEllipse(double, double, double)");
            }

            {
                double centralAngle = Com.Statistics.RandomDouble(2 * Math.PI);
                double eccentricity = Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    double result = Com.Geometry.EllipseCentralAngleToPhase(centralAngle, eccentricity);
                };

                ExecuteTest(method, "Com.Geometry.EllipseCentralAngleToPhase(double, double, double)");
            }

            // 位图

            {
                Bitmap bmp = new Bitmap(1024, 1024);
                double rotateAngle = Com.Statistics.RandomDouble();
                bool antiAlias = true;

                Action method = () =>
                {
                    Bitmap result = Com.Geometry.RotateBitmap(bmp, rotateAngle, antiAlias);
                };

                ExecuteTest(method, "Com.Geometry.RotateBitmap(System.Drawing.Bitmap, double, bool)", "bmp at 1024x1024 pixels, enable antiAlias");
            }

            // 路径

            {
                Rectangle rect = _GetRandomRectangle();
                int cornerRadius = Com.Statistics.RandomInteger(32768);

                Action method = () =>
                {
                    GraphicsPath result = Com.Geometry.CreateRoundedRectanglePath(rect, cornerRadius);
                };

                ExecuteTest(method, "Com.Geometry.CreateRoundedRectanglePath(System.Drawing.Rectangle, int)");
            }

            {
                Rectangle rect = _GetRandomRectangle();
                int cornerRadiusLT = Com.Statistics.RandomInteger(32768);
                int cornerRadiusRT = Com.Statistics.RandomInteger(32768);
                int cornerRadiusRB = Com.Statistics.RandomInteger(32768);
                int cornerRadiusLB = Com.Statistics.RandomInteger(32768);

                Action method = () =>
                {
                    GraphicsPath result = Com.Geometry.CreateRoundedRectanglePath(rect, cornerRadiusLT, cornerRadiusRT, cornerRadiusRB, cornerRadiusLB);
                };

                ExecuteTest(method, "Com.Geometry.CreateRoundedRectanglePath(System.Drawing.Rectangle, int, int, int, int)");
            }

            {
                Rectangle rect = _GetRandomRectangle();
                int cornerRadius = Com.Statistics.RandomInteger(32768);

                Action method = () =>
                {
                    GraphicsPath[] result = Com.Geometry.CreateRoundedRectangleOuterPaths(rect, cornerRadius);
                };

                ExecuteTest(method, "Com.Geometry.CreateRoundedRectangleOuterPaths(System.Drawing.Rectangle, int)");
            }

            {
                Rectangle rect = _GetRandomRectangle();
                int cornerRadiusLT = Com.Statistics.RandomInteger(32768);
                int cornerRadiusRT = Com.Statistics.RandomInteger(32768);
                int cornerRadiusRB = Com.Statistics.RandomInteger(32768);
                int cornerRadiusLB = Com.Statistics.RandomInteger(32768);

                Action method = () =>
                {
                    GraphicsPath[] result = Com.Geometry.CreateRoundedRectangleOuterPaths(rect, cornerRadiusLT, cornerRadiusRT, cornerRadiusRB, cornerRadiusLB);
                };

                ExecuteTest(method, "Com.Geometry.CreateRoundedRectangleOuterPaths(System.Drawing.Rectangle, int, int, int, int)");
            }
        }

        protected override void Operator()
        {

        }
    }

    sealed class IOTest : ClassPerformanceTestBase
    {
        protected override void Constructor()
        {

        }

        protected override void Property()
        {

        }

        protected override void StaticProperty()
        {

        }

        protected override void Method()
        {

        }

        protected override void StaticMethod()
        {
            ExecuteTest(null, "Com.IO.CopyFolder(string, string, bool, bool, bool)");

            ExecuteTest(null, "Com.IO.CopyFolder(string, string, bool, bool)");

            ExecuteTest(null, "Com.IO.CopyFolder(string, string, bool)");

            ExecuteTest(null, "Com.IO.CopyFolder(string, string)");
        }

        protected override void Operator()
        {

        }
    }

    sealed class MatrixTest : ClassPerformanceTestBase
    {
        private static Com.Matrix _GetRandomMatrix(int width, int height)
        {
            if (width > 0 && height > 0)
            {
                Com.Matrix matrix = new Com.Matrix(width, height);

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        matrix[x, y] = Com.Statistics.RandomDouble(-1E18, 1E18);
                    }
                }

                return matrix;
            }
            else
            {
                return Com.Matrix.NonMatrix;
            }
        }

        //

        protected override void Constructor()
        {
            {
                Size size = new Size(32, 32);

                Action method = () =>
                {
                    Com.Matrix matrix = new Com.Matrix(size);
                };

                ExecuteTest(method, "Com.Matrix.Matrix(System.Drawing.Size)", "size at 32x32");
            }

            {
                Size size = new Size(32, 32);
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Matrix matrix = new Com.Matrix(size, value);
                };

                ExecuteTest(method, "Com.Matrix.Matrix(System.Drawing.Size, double)", "size at 32x32");
            }

            {
                int width = 32;
                int height = 32;

                Action method = () =>
                {
                    Com.Matrix matrix = new Com.Matrix(width, height);
                };

                ExecuteTest(method, "Com.Matrix.Matrix(int, int)", "size at 32x32");
            }

            {
                int width = 32;
                int height = 32;
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Matrix matrix = new Com.Matrix(width, height, value);
                };

                ExecuteTest(method, "Com.Matrix.Matrix(int, int, double)", "size at 32x32");
            }

            {
                double[,] values = new double[32, 32];

                for (int x = 0; x < 32; x++)
                {
                    for (int y = 0; y < 32; y++)
                    {
                        values[x, y] = Com.Statistics.RandomDouble(-1E18, 1E18);
                    }
                }

                Action method = () =>
                {
                    Com.Matrix matrix = new Com.Matrix(values);
                };

                ExecuteTest(method, "Com.Matrix.Matrix(double[,])", "size at 32x32");
            }
        }

        protected override void Property()
        {
            // 索引器

            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);
                int x = Com.Statistics.RandomInteger(32);
                int y = Com.Statistics.RandomInteger(32);

                Action method = () =>
                {
                    double result = matrix[x, y];
                };

                ExecuteTest(method, "Com.Matrix.this[int, int].get()", "size at 32x32");
            }

            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);
                int x = Com.Statistics.RandomInteger(32);
                int y = Com.Statistics.RandomInteger(32);
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    matrix[x, y] = value;
                };

                ExecuteTest(method, "Com.Matrix.this[int, int].set(double)", "size at 32x32");
            }

            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);
                Point index = new Point(Com.Statistics.RandomInteger(32), Com.Statistics.RandomInteger(32));

                Action method = () =>
                {
                    double result = matrix[index];
                };

                ExecuteTest(method, "Com.Matrix.this[System.Drawing.Point].get()", "size at 32x32");
            }

            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);
                Point index = new Point(Com.Statistics.RandomInteger(32), Com.Statistics.RandomInteger(32));
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    matrix[index] = value;
                };

                ExecuteTest(method, "Com.Matrix.this[System.Drawing.Point].set(double)", "size at 32x32");
            }

            // Is

            {
                Com.Matrix matrix = new Com.Matrix(32, 32);

                Action method = () =>
                {
                    bool result = matrix.IsNonMatrix;
                };

                ExecuteTest(method, "Com.Matrix.IsNonMatrix.get()", "size at 32x32");
            }

            // Size

            {
                Com.Matrix matrix = new Com.Matrix(32, 32);

                Action method = () =>
                {
                    Size result = matrix.Size;
                };

                ExecuteTest(method, "Com.Matrix.Size.get()", "size at 32x32");
            }

            {
                Com.Matrix matrix = new Com.Matrix(32, 32);

                Action method = () =>
                {
                    int result = matrix.Width;
                };

                ExecuteTest(method, "Com.Matrix.Width.get()", "size at 32x32");
            }

            {
                Com.Matrix matrix = new Com.Matrix(32, 32);

                Action method = () =>
                {
                    int result = matrix.Column;
                };

                ExecuteTest(method, "Com.Matrix.Column.get()", "size at 32x32");
            }

            {
                Com.Matrix matrix = new Com.Matrix(32, 32);

                Action method = () =>
                {
                    int result = matrix.Height;
                };

                ExecuteTest(method, "Com.Matrix.Height.get()", "size at 32x32");
            }

            {
                Com.Matrix matrix = new Com.Matrix(32, 32);

                Action method = () =>
                {
                    int result = matrix.Row;
                };

                ExecuteTest(method, "Com.Matrix.Row.get()", "size at 32x32");
            }

            {
                Com.Matrix matrix = new Com.Matrix(32, 32);

                Action method = () =>
                {
                    int result = matrix.Count;
                };

                ExecuteTest(method, "Com.Matrix.Count.get()", "size at 32x32");
            }

            // 线性代数属性

            {
                Com.Matrix matrix = _GetRandomMatrix(8, 8);

                Action method = () =>
                {
                    double result = matrix.Determinant;
                };

                ExecuteTest(method, "Com.Matrix.Determinant.get()", "size at 8x8");
            }

            {
                Com.Matrix matrix = _GetRandomMatrix(8, 8);

                Action method = () =>
                {
                    int result = matrix.Rank;
                };

                ExecuteTest(method, "Com.Matrix.Rank.get()", "size at 8x8");
            }

            {
                Com.Matrix matrix = _GetRandomMatrix(8, 8);

                Action method = () =>
                {
                    Com.Matrix result = matrix.Transport;
                };

                ExecuteTest(method, "Com.Matrix.Transport.get()", "size at 8x8");
            }

            {
                Com.Matrix matrix = _GetRandomMatrix(8, 8);

                Action method = () =>
                {
                    Com.Matrix result = matrix.Adjoint;
                };

                ExecuteTest(method, "Com.Matrix.Adjoint.get()", "size at 8x8");
            }

            {
                Com.Matrix matrix = _GetRandomMatrix(8, 8);

                Action method = () =>
                {
                    Com.Matrix result = matrix.Invert;
                };

                ExecuteTest(method, "Com.Matrix.Invert.get()", "size at 8x8");
            }
        }

        protected override void StaticProperty()
        {
            {
                Action method = () =>
                {
                    Com.Matrix result = Com.Matrix.NonMatrix;
                };

                ExecuteTest(method, "Com.Matrix.NonMatrix.get()");
            }
        }

        protected override void Method()
        {
            // object

            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);
                object obj = (object)matrix.Copy();

                Action method = () =>
                {
                    bool result = matrix.Equals(obj);
                };

                ExecuteTest(method, "Com.Matrix.Equals(object)", "size at 32x32");
            }

            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);

                Action method = () =>
                {
                    int result = matrix.GetHashCode();
                };

                ExecuteTest(method, "Com.Matrix.GetHashCode()", "size at 32x32");
            }

            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);

                Action method = () =>
                {
                    string result = matrix.ToString();
                };

                ExecuteTest(method, "Com.Matrix.ToString()", "size at 32x32");
            }

            // Equals

            {
                Com.Matrix left = _GetRandomMatrix(32, 32);
                Com.Matrix right = left.Copy();

                Action method = () =>
                {
                    bool result = left.Equals(right);
                };

                ExecuteTest(method, "Com.Matrix.Equals(Com.Matrix)", "size at 32x32");
            }

            // Copy

            {
                Com.Matrix matrix = new Com.Matrix(32, 32);

                Action method = () =>
                {
                    Com.Matrix result = matrix.Copy();
                };

                ExecuteTest(method, "Com.Matrix.Copy()", "size at 32x32");
            }

            // 子矩阵

            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);
                Point index = new Point(8, 8);
                Size size = new Size(16, 16);

                Action method = () =>
                {
                    Com.Matrix result = matrix.SubMatrix(index, size);
                };

                ExecuteTest(method, "Com.Matrix.SubMatrix(System.Drawing.Point, System.Drawing.Size)", "size at 32x32, SubMatrix is 16x16 at (8,8)");
            }

            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);
                int x = 8;
                int y = 8;
                int width = 16;
                int height = 16;

                Action method = () =>
                {
                    Com.Matrix result = matrix.SubMatrix(x, y, width, height);
                };

                ExecuteTest(method, "Com.Matrix.SubMatrix(int, int, int, int)", "size at 32x32, SubMatrix is 16x16 at (8,8)");
            }

            // 获取行列

            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);
                int x = Com.Statistics.RandomInteger(32);

                Action method = () =>
                {
                    Com.Vector result = matrix.GetColumn(x);
                };

                ExecuteTest(method, "Com.Matrix.GetColumn(int)", "size at 32x32");
            }

            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);
                int y = Com.Statistics.RandomInteger(32);

                Action method = () =>
                {
                    Com.Vector result = matrix.GetRow(y);
                };

                ExecuteTest(method, "Com.Matrix.GetRow(int)", "size at 32x32");
            }

            // ToArray

            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);

                Action method = () =>
                {
                    double[,] result = matrix.ToArray();
                };

                ExecuteTest(method, "Com.Matrix.ToArray()", "size at 32x32");
            }
        }

        protected override void StaticMethod()
        {
            // IsNullOrNonMatrix

            {
                Com.Matrix matrix = new Com.Matrix(32, 32);

                Action method = () =>
                {
                    bool result = Com.Matrix.IsNullOrNonMatrix(matrix);
                };

                ExecuteTest(method, "Com.Matrix.IsNullOrNonMatrix(Com.Matrix)", "size at 32x32");
            }

            // Equals

            {
                Com.Matrix left = _GetRandomMatrix(32, 32);
                Com.Matrix right = left.Copy();

                Action method = () =>
                {
                    bool result = Com.Matrix.Equals(left, right);
                };

                ExecuteTest(method, "Com.Matrix.Equals(Com.Matrix, Com.Matrix)", "size at 32x32");
            }

            // 矩阵生成

            {
                int order = 32;

                Action method = () =>
                {
                    Com.Matrix result = Com.Matrix.Identity(order);
                };

                ExecuteTest(method, "Com.Matrix.Identity(int)", "size at 32x32");
            }

            {
                Size size = new Size(32, 32);

                Action method = () =>
                {
                    Com.Matrix result = Com.Matrix.Zeros(size);
                };

                ExecuteTest(method, "Com.Matrix.Zeros(System.Drawing.Size)", "size at 32x32");
            }

            {
                int width = 32;
                int height = 32;

                Action method = () =>
                {
                    Com.Matrix result = Com.Matrix.Zeros(width, height);
                };

                ExecuteTest(method, "Com.Matrix.Zeros(int, int)", "size at 32x32");
            }

            {
                Size size = new Size(32, 32);

                Action method = () =>
                {
                    Com.Matrix result = Com.Matrix.Ones(size);
                };

                ExecuteTest(method, "Com.Matrix.Ones(System.Drawing.Size)", "size at 32x32");
            }

            {
                int width = 32;
                int height = 32;

                Action method = () =>
                {
                    Com.Matrix result = Com.Matrix.Ones(width, height);
                };

                ExecuteTest(method, "Com.Matrix.Ones(int, int)", "size at 32x32");
            }

            {
                double[] array = new double[32];

                for (int i = 0; i < 32; i++)
                {
                    array[i] = Com.Statistics.RandomDouble(-1E18, 1E18);
                }

                int rowsUponMainDiag = 0;

                Action method = () =>
                {
                    Com.Matrix result = Com.Matrix.Diagonal(array, rowsUponMainDiag);
                };

                ExecuteTest(method, "Com.Matrix.Diagonal(double[], int)", "size at 32x32");
            }

            {
                double[] array = new double[32];

                for (int i = 0; i < 32; i++)
                {
                    array[i] = Com.Statistics.RandomDouble(-1E18, 1E18);
                }

                Action method = () =>
                {
                    Com.Matrix result = Com.Matrix.Diagonal(array);
                };

                ExecuteTest(method, "Com.Matrix.Diagonal(double[])", "size at 32x32");
            }

            // 增广矩阵

            {
                Com.Matrix left = _GetRandomMatrix(32, 32);
                Com.Matrix right = _GetRandomMatrix(32, 32);

                Action method = () =>
                {
                    Com.Matrix result = Com.Matrix.Augment(left, right);
                };

                ExecuteTest(method, "Com.Matrix.Augment(Com.Matrix, Com.Matrix)", "size at 32x32");
            }

            // 线性代数运算

            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Matrix result = Com.Matrix.Add(matrix, n);
                };

                ExecuteTest(method, "Com.Matrix.Add(Com.Matrix, double)", "size at 32x32");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.Matrix matrix = _GetRandomMatrix(32, 32);

                Action method = () =>
                {
                    Com.Matrix result = Com.Matrix.Add(n, matrix);
                };

                ExecuteTest(method, "Com.Matrix.Add(double, Com.Matrix)", "size at 32x32");
            }

            {
                Com.Matrix left = _GetRandomMatrix(32, 32);
                Com.Matrix right = _GetRandomMatrix(32, 32);

                Action method = () =>
                {
                    Com.Matrix result = Com.Matrix.Add(left, right);
                };

                ExecuteTest(method, "Com.Matrix.Add(Com.Matrix, Com.Matrix)", "size at 32x32");
            }

            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Matrix result = Com.Matrix.Subtract(matrix, n);
                };

                ExecuteTest(method, "Com.Matrix.Subtract(Com.Matrix, double)", "size at 32x32");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.Matrix matrix = _GetRandomMatrix(32, 32);

                Action method = () =>
                {
                    Com.Matrix result = Com.Matrix.Subtract(n, matrix);
                };

                ExecuteTest(method, "Com.Matrix.Subtract(double, Com.Matrix)", "size at 32x32");
            }

            {
                Com.Matrix left = _GetRandomMatrix(32, 32);
                Com.Matrix right = _GetRandomMatrix(32, 32);

                Action method = () =>
                {
                    Com.Matrix result = Com.Matrix.Subtract(left, right);
                };

                ExecuteTest(method, "Com.Matrix.Subtract(Com.Matrix, Com.Matrix)", "size at 32x32");
            }

            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Matrix result = Com.Matrix.Multiply(matrix, n);
                };

                ExecuteTest(method, "Com.Matrix.Multiply(Com.Matrix, double)", "size at 32x32");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.Matrix matrix = _GetRandomMatrix(32, 32);

                Action method = () =>
                {
                    Com.Matrix result = Com.Matrix.Multiply(n, matrix);
                };

                ExecuteTest(method, "Com.Matrix.Multiply(double, Com.Matrix)", "size at 32x32");
            }

            {
                Com.Matrix left = _GetRandomMatrix(8, 8);
                Com.Matrix right = _GetRandomMatrix(8, 8);

                Action method = () =>
                {
                    Com.Matrix result = Com.Matrix.Multiply(left, right);
                };

                ExecuteTest(method, "Com.Matrix.Multiply(Com.Matrix, Com.Matrix)", "size at 8x8");
            }

            {
                List<Com.Matrix> list = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    list.Add(_GetRandomMatrix(32, 32));
                }

                Action method = () =>
                {
                    Com.Matrix result = Com.Matrix.MultiplyLeft(list);
                };

                ExecuteTest(method, "Com.Matrix.MultiplyLeft(System.Collections.Generic.List<Com.Matrix>)", "size at 32x32, total 8 matrices");
            }

            {
                List<Com.Matrix> list = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    list.Add(_GetRandomMatrix(32, 32));
                }

                Action method = () =>
                {
                    Com.Matrix result = Com.Matrix.MultiplyRight(list);
                };

                ExecuteTest(method, "Com.Matrix.MultiplyRight(System.Collections.Generic.List<Com.Matrix>)", "size at 32x32, total 8 matrices");
            }

            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Matrix result = Com.Matrix.Divide(matrix, n);
                };

                ExecuteTest(method, "Com.Matrix.Divide(Com.Matrix, double)", "size at 32x32");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.Matrix matrix = _GetRandomMatrix(32, 32);

                Action method = () =>
                {
                    Com.Matrix result = Com.Matrix.Divide(n, matrix);
                };

                ExecuteTest(method, "Com.Matrix.Divide(double, Com.Matrix)", "size at 32x32");
            }

            {
                Com.Matrix left = _GetRandomMatrix(8, 8);
                Com.Matrix right = _GetRandomMatrix(8, 8);

                Action method = () =>
                {
                    Com.Matrix result = Com.Matrix.DivideLeft(left, right);
                };

                ExecuteTest(method, "Com.Matrix.DivideLeft(Com.Matrix, Com.Matrix)", "size at 8x8");
            }

            {
                Com.Matrix left = _GetRandomMatrix(8, 8);
                Com.Matrix right = _GetRandomMatrix(8, 8);

                Action method = () =>
                {
                    Com.Matrix result = Com.Matrix.DivideRight(left, right);
                };

                ExecuteTest(method, "Com.Matrix.DivideRight(Com.Matrix, Com.Matrix)", "size at 8x8");
            }

            // 求解线性方程组

            {
                Com.Matrix matrix = _GetRandomMatrix(8, 8);
                Com.Vector vector = new Com.Vector(new double[8]);

                for (int i = 0; i < 8; i++)
                {
                    vector[i] = Com.Statistics.RandomDouble(-1E18, 1E18);
                }

                Action method = () =>
                {
                    Com.Vector result = Com.Matrix.SolveLinearEquation(matrix, vector);
                };

                ExecuteTest(method, "Com.Matrix.SolveLinearEquation(Com.Matrix, Com.Vector)", "size at 8x8");
            }
        }

        protected override void Operator()
        {
            // 比较

            {
                Com.Matrix left = _GetRandomMatrix(32, 32);
                Com.Matrix right = left.Copy();

                Action method = () =>
                {
                    bool result = (left == right);
                };

                ExecuteTest(method, "Com.Matrix.operator ==(Com.Matrix, Com.Matrix)", "size at 32x32");
            }

            {
                Com.Matrix left = _GetRandomMatrix(32, 32);
                Com.Matrix right = left.Copy();

                Action method = () =>
                {
                    bool result = (left != right);
                };

                ExecuteTest(method, "Com.Matrix.operator !=(Com.Matrix, Com.Matrix)", "size at 32x32");
            }
        }
    }

    sealed class Painting2DTest : ClassPerformanceTestBase
    {
        protected override void Constructor()
        {

        }

        protected override void Property()
        {

        }

        protected override void StaticProperty()
        {

        }

        protected override void Method()
        {

        }

        protected override void StaticMethod()
        {
            {
                Bitmap bmp = new Bitmap(1024, 1024);
                Com.PointD pt1 = new Com.PointD(0, 0);
                Com.PointD pt2 = new Com.PointD(bmp.Size) - 1;
                Color color = Com.ColorManipulation.GetRandomColor();
                float width = 1.0F;
                bool antiAlias = true;

                Action method = () =>
                {
                    bool result = Com.Painting2D.PaintLine(bmp, pt1, pt2, color, width, antiAlias);
                };

                ExecuteTest(method, "Com.Painting2D.PaintLine(System.Drawing.Bitmap, Com.PointD, Com.PointD, System.Drawing.Color, float, bool)", "bmp at 1024x1024 pixels, width at 1.0F, enable antiAlias");
            }

            {
                Bitmap bmp = new Bitmap(1024, 1024);
                Com.PointD offset = new Com.PointD(bmp.Size) / 2;
                double radius = new Com.PointD(bmp.Size).VectorModule / 2;
                double deltaRadius = radius / 9;
                int normalIncreasePeriod = 3;
                Color color = Com.ColorManipulation.GetRandomColor();
                bool antiAlias = true;

                Action method = () =>
                {
                    bool result = Com.Painting2D.PaintPolarGrid(bmp, offset, radius, deltaRadius, normalIncreasePeriod, color, antiAlias);
                };

                ExecuteTest(method, "Com.Painting2D.PaintPolarGrid(System.Drawing.Bitmap, Com.PointD, double, double, int, System.Drawing.Color, bool)", "bmp at 1024x1024 pixels, radius at half diagonal of bmp, deltaRadius at 1/9 of radius, normalIncreasePeriod at 3, enable antiAlias");
            }

            {
                Bitmap bmp = new Bitmap(1024, 1024);
                Com.PointD offset = new Com.PointD(bmp.Size) / 2;
                double radius = bmp.Width / 2;
                Color color = Com.ColorManipulation.GetRandomColor();
                float width = 1.0F;
                bool antiAlias = true;

                Action method = () =>
                {
                    bool result = Com.Painting2D.PaintCircle(bmp, offset, radius, color, width, antiAlias);
                };

                ExecuteTest(method, "Com.Painting2D.PaintCircle(System.Drawing.Bitmap, Com.PointD, double, System.Drawing.Color, float, bool)", "bmp at 1024x1024 pixels, radius at half width of bmp, width at 1.0F, enable antiAlias");
            }

            {
                Bitmap bmp = new Bitmap(1024, 1024);
                Com.PointD offset = -new Com.PointD(bmp.Size);
                double radius = bmp.Width * Math.Sqrt(5);
                double refPhase = Math.PI / 4;
                Color color = Com.ColorManipulation.GetRandomColor();
                float width = 1.0F;
                bool antiAlias = true;
                int minDiv = 32;
                int maxDiv = 256;
                double divArc = 4;

                Action method = () =>
                {
                    bool result = Com.Painting2D.PaintLargeCircle(bmp, offset, radius, refPhase, color, width, antiAlias, minDiv, maxDiv, divArc);
                };

                ExecuteTest(method, "Com.Painting2D.PaintLargeCircle(System.Drawing.Bitmap, Com.PointD, double, double, System.Drawing.Color, float, bool, int, int, double)", "bmp at 1024x1024 pixels, radius at sqrt(5) width of bmp, width at 1.0F, enable antiAlias, minDiv at 32, maxDiv at 256, divArc at 4");
            }

            {
                Bitmap bmp = new Bitmap(1024, 1024);
                Com.PointD offset = -new Com.PointD(bmp.Size);
                double radius = bmp.Width * Math.Sqrt(5);
                double refPhase = Math.PI / 4;
                Color color = Com.ColorManipulation.GetRandomColor();
                float width = 1.0F;
                bool antiAlias = true;

                Action method = () =>
                {
                    bool result = Com.Painting2D.PaintLargeCircle(bmp, offset, radius, refPhase, color, width, antiAlias);
                };

                ExecuteTest(method, "Com.Painting2D.PaintLargeCircle(System.Drawing.Bitmap, Com.PointD, double, double, System.Drawing.Color, float, bool, int, int, double)", "bmp at 1024x1024 pixels, radius at sqrt(5) width of bmp, width at 1.0F, enable antiAlias");
            }

            {
                Bitmap bmp = new Bitmap(1024, 1024);
                Com.PointD offset = new Com.PointD(0, bmp.Height / 2);
                double semiMajorAxis = bmp.Width * 5;
                double eccentricity = 0.8;
                double rotateAngle = 0;
                double refPhase = 0;
                Color color = Com.ColorManipulation.GetRandomColor();
                float width = 1.0F;
                bool antiAlias = true;
                int minDiv = 32;
                int maxDiv = 256;
                double divArc = 4;

                Action method = () =>
                {
                    bool result = Com.Painting2D.PaintLargeEllipse(bmp, offset, semiMajorAxis, eccentricity, rotateAngle, refPhase, color, width, antiAlias, minDiv, maxDiv, divArc);
                };

                ExecuteTest(method, "Com.Painting2D.PaintLargeEllipse(System.Drawing.Bitmap, Com.PointD, double, double, double, double, System.Drawing.Color, float, bool, int, int, double)", "bmp at 1024x1024 pixels, semiMajorAxis at 5 width of bmp, eccentricity at 0.8, width at 1.0F, enable antiAlias, minDiv at 32, maxDiv at 256, divArc at 4");
            }

            {
                Bitmap bmp = new Bitmap(1024, 1024);
                Com.PointD offset = new Com.PointD(0, bmp.Height / 2);
                double semiMajorAxis = bmp.Width * 5;
                double eccentricity = 0.8;
                double rotateAngle = 0;
                double refPhase = 0;
                Color color = Com.ColorManipulation.GetRandomColor();
                float width = 1.0F;
                bool antiAlias = true;

                Action method = () =>
                {
                    bool result = Com.Painting2D.PaintLargeEllipse(bmp, offset, semiMajorAxis, eccentricity, rotateAngle, refPhase, color, width, antiAlias);
                };

                ExecuteTest(method, "Com.Painting2D.PaintLargeEllipse(System.Drawing.Bitmap, Com.PointD, double, double, double, double, System.Drawing.Color, float, bool)", "bmp at 1024x1024 pixels, semiMajorAxis at 5 width of bmp, eccentricity at 0.8, width at 1.0F, enable antiAlias");
            }

            {
                Bitmap bmp = new Bitmap(1024, 1024);
                string text = "0123456789:;<=>?@ABCDEF";
                Font font = new Font("微软雅黑", 42F, FontStyle.Regular, GraphicsUnit.Point, 134);
                Color frontColor = Com.ColorManipulation.GetRandomColor();
                Color backColor = Com.ColorManipulation.GetRandomColor();
                PointF pt = new PointF(0, 0);
                float offset = 0.1F;
                bool antiAlias = true;

                Action method = () =>
                {
                    bool result = Com.Painting2D.PaintTextWithShadow(bmp, text, font, frontColor, backColor, pt, offset, antiAlias);
                };

                ExecuteTest(method, "Com.Painting2D.PaintTextWithShadow(System.Drawing.Bitmap, string, System.Drawing.Font, System.Drawing.Color, System.Drawing.Color, System.Drawing.PointF, float, bool)", "bmp at 1024x1024 pixels, font at 42 pt, enable antiAlias");
            }

            ExecuteTest(null, "Com.Painting2D.PaintImageOnTransparentForm(System.Windows.Forms.Form, System.Drawing.Bitmap, double)");
        }

        protected override void Operator()
        {

        }
    }

    sealed class Painting3DTest : ClassPerformanceTestBase
    {
        protected override void Constructor()
        {

        }

        protected override void Property()
        {

        }

        protected override void StaticProperty()
        {

        }

        protected override void Method()
        {

        }

        protected override void StaticMethod()
        {
            {
                Bitmap bmp = new Bitmap(1024, 1024);
                Com.PointD3D center = new Com.PointD3D(bmp.Width / 2, bmp.Height / 2, 2048);
                Com.PointD3D size = new Com.PointD3D(512, 512, 512);
                Color color = Com.ColorManipulation.GetRandomColorX().AtAlpha(128).ToColor();
                float edgeWidth = 1.0F;
                List<Com.Matrix> affineMatrixList = new List<Com.Matrix>(4) { Com.PointD3D.IdentityMatrix(), Com.PointD3D.IdentityMatrix(), Com.PointD3D.IdentityMatrix(), Com.PointD3D.IdentityMatrix() };
                double trueLenDist = 512;
                Com.PointD3D illuminationDirection = new Com.PointD3D(1, 1, 1);
                bool illuminationDirectionIsAfterAffineTransform = false;
                double exposure = 0;
                bool antiAlias = true;

                Action method = () =>
                {
                    bool result = Com.Painting3D.PaintCuboid(bmp, center, size, color, edgeWidth, affineMatrixList, trueLenDist, illuminationDirection, illuminationDirectionIsAfterAffineTransform, exposure, antiAlias);
                };

                ExecuteTest(method, "Com.Painting3D.PaintCuboid(System.Drawing.Bitmap, Com.PointD3D, Com.PointD3D, System.Drawing.Color, float, List<Com.Matrix>, double, Com.PointD3D, bool, double, bool)", "bmp at 1024x1024 pixels, cuboid size at 512x512x512, edgeWidth at 1.0F, total 4 matrices, trueLenDist at 1024, enable antiAlias");
            }

            {
                Bitmap bmp = new Bitmap(1024, 1024);
                Com.PointD3D center = new Com.PointD3D(bmp.Width / 2, bmp.Height / 2, 2048);
                Com.PointD3D size = new Com.PointD3D(512, 512, 512);
                Color color = Com.ColorManipulation.GetRandomColorX().AtAlpha(128).ToColor();
                float edgeWidth = 1.0F;
                Com.Matrix affineMatrix = Com.PointD3D.IdentityMatrix();
                double trueLenDist = 512;
                Com.PointD3D illuminationDirection = new Com.PointD3D(1, 1, 1);
                bool illuminationDirectionIsAfterAffineTransform = false;
                double exposure = 0;
                bool antiAlias = true;

                Action method = () =>
                {
                    bool result = Com.Painting3D.PaintCuboid(bmp, center, size, color, edgeWidth, affineMatrix, trueLenDist, illuminationDirection, illuminationDirectionIsAfterAffineTransform, exposure, antiAlias);
                };

                ExecuteTest(method, "Com.Painting3D.PaintCuboid(System.Drawing.Bitmap, Com.PointD3D, Com.PointD3D, System.Drawing.Color, float, Com.Matrix, double, Com.PointD3D, bool, double, bool)", "bmp at 1024x1024 pixels, cuboid size at 512x512x512, edgeWidth at 1.0F, trueLenDist at 1024, enable antiAlias");
            }

            {
                Bitmap bmp = new Bitmap(1024, 1024);
                Com.PointD3D center = new Com.PointD3D(bmp.Width / 2, bmp.Height / 2, 2048);
                Com.PointD3D size = new Com.PointD3D(512, 512, 512);
                Color color = Com.ColorManipulation.GetRandomColorX().AtAlpha(128).ToColor();
                float edgeWidth = 1.0F;
                List<Com.Matrix> affineMatrixList = new List<Com.Matrix>(4) { Com.PointD3D.IdentityMatrix(), Com.PointD3D.IdentityMatrix(), Com.PointD3D.IdentityMatrix(), Com.PointD3D.IdentityMatrix() };
                double trueLenDist = 512;
                bool antiAlias = true;

                Action method = () =>
                {
                    bool result = Com.Painting3D.PaintCuboid(bmp, center, size, color, edgeWidth, affineMatrixList, trueLenDist, antiAlias);
                };

                ExecuteTest(method, "Com.Painting3D.PaintCuboid(System.Drawing.Bitmap, Com.PointD3D, Com.PointD3D, System.Drawing.Color, float, List<Com.Matrix>, double, bool)", "bmp at 1024x1024 pixels, cuboid size at 512x512x512, edgeWidth at 1.0F, total 4 matrices, trueLenDist at 1024, enable antiAlias");
            }

            {
                Bitmap bmp = new Bitmap(1024, 1024);
                Com.PointD3D center = new Com.PointD3D(bmp.Width / 2, bmp.Height / 2, 2048);
                Com.PointD3D size = new Com.PointD3D(512, 512, 512);
                Color color = Com.ColorManipulation.GetRandomColorX().AtAlpha(128).ToColor();
                float edgeWidth = 1.0F;
                Com.Matrix affineMatrix = Com.PointD3D.IdentityMatrix();
                double trueLenDist = 512;
                bool antiAlias = true;

                Action method = () =>
                {
                    bool result = Com.Painting3D.PaintCuboid(bmp, center, size, color, edgeWidth, affineMatrix, trueLenDist, antiAlias);
                };

                ExecuteTest(method, "Com.Painting3D.PaintCuboid(System.Drawing.Bitmap, Com.PointD3D, Com.PointD3D, System.Drawing.Color, float, Com.Matrix, double, bool)", "bmp at 1024x1024 pixels, cuboid size at 512x512x512, edgeWidth at 1.0F, trueLenDist at 1024, enable antiAlias");
            }
        }

        protected override void Operator()
        {

        }
    }

    sealed class PointDTest : ClassPerformanceTestBase
    {
        private static Com.PointD _GetRandomPointD()
        {
            return new Com.PointD(Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18));
        }

        private static Com.Matrix _GetRandomMatrix(int width, int height)
        {
            if (width > 0 && height > 0)
            {
                Com.Matrix matrix = new Com.Matrix(width, height);

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        matrix[x, y] = Com.Statistics.RandomDouble(-1E18, 1E18);
                    }
                }

                return matrix;
            }
            else
            {
                return Com.Matrix.NonMatrix;
            }
        }

        //

        protected override void Constructor()
        {
            {
                double x = Com.Statistics.RandomDouble(-1E18, 1E18);
                double y = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD pointD = new Com.PointD(x, y);
                };

                ExecuteTest(method, "Com.PointD.PointD(double, double)");
            }

            {
                Point pt = new Point(Com.Statistics.RandomInteger(), Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    Com.PointD pointD = new Com.PointD(pt);
                };

                ExecuteTest(method, "Com.PointD.PointD(System.Drawing.Point)");
            }

            {
                PointF pt = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    Com.PointD pointD = new Com.PointD(pt);
                };

                ExecuteTest(method, "Com.PointD.PointD(System.Drawing.PointF)");
            }

            {
                Size sz = new Size(Com.Statistics.RandomInteger(), Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    Com.PointD pointD = new Com.PointD(sz);
                };

                ExecuteTest(method, "Com.PointD.PointD(System.Drawing.Size)");
            }

            {
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    Com.PointD pointD = new Com.PointD(sz);
                };

                ExecuteTest(method, "Com.PointD.PointD(System.Drawing.SizeF)");
            }

            {
                Com.Complex comp = new Com.Complex(Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    Com.PointD pointD = new Com.PointD(comp);
                };

                ExecuteTest(method, "Com.PointD.PointD(Com.Complex)");
            }
        }

        protected override void Property()
        {
            // 索引器

            {
                Com.PointD pointD = _GetRandomPointD();
                int index = Com.Statistics.RandomInteger(2);

                Action method = () =>
                {
                    double result = pointD[index];
                };

                ExecuteTest(method, "Com.PointD.this[int].get()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                int index = Com.Statistics.RandomInteger(2);
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD[index] = value;
                };

                ExecuteTest(method, "Com.PointD.this[int].set(double)");
            }

            // Is

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    bool result = pointD.IsEmpty;
                };

                ExecuteTest(method, "Com.PointD.IsEmpty.get()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    bool result = pointD.IsNaN;
                };

                ExecuteTest(method, "Com.PointD.IsNaN.get()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    bool result = pointD.IsInfinity;
                };

                ExecuteTest(method, "Com.PointD.IsInfinity.get()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    bool result = pointD.IsNaNOrInfinity;
                };

                ExecuteTest(method, "Com.PointD.IsNaNOrInfinity.get()");
            }

            // 分量

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    double result = pointD.X;
                };

                ExecuteTest(method, "Com.PointD.X.get()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD.X = value;
                };

                ExecuteTest(method, "Com.PointD.X.set(double)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    double result = pointD.Y;
                };

                ExecuteTest(method, "Com.PointD.Y.get()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD.Y = value;
                };

                ExecuteTest(method, "Com.PointD.Y.set(double)");
            }

            // 角度

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    double result = pointD.AngleX;
                };

                ExecuteTest(method, "Com.PointD.AngleX.get()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    double result = pointD.AngleY;
                };

                ExecuteTest(method, "Com.PointD.AngleY.get()");
            }

            // 向量

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    double result = pointD.VectorModule;
                };

                ExecuteTest(method, "Com.PointD.VectorModule.get()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    double result = pointD.VectorModuleSquared;
                };

                ExecuteTest(method, "Com.PointD.VectorModuleSquared.get()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    double result = pointD.VectorAngle;
                };

                ExecuteTest(method, "Com.PointD.VectorAngle.get()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = pointD.VectorNegate;
                };

                ExecuteTest(method, "Com.PointD.VectorNegate.get()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = pointD.VectorNormalize;
                };

                ExecuteTest(method, "Com.PointD.VectorNormalize.get()");
            }
        }

        protected override void StaticProperty()
        {

        }

        protected override void Method()
        {
            // object

            {
                Com.PointD pointD = _GetRandomPointD();
                object obj = (object)pointD;

                Action method = () =>
                {
                    bool result = pointD.Equals(obj);
                };

                ExecuteTest(method, "Com.PointD.Equals(object)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    int result = pointD.GetHashCode();
                };

                ExecuteTest(method, "Com.PointD.GetHashCode()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    string result = pointD.ToString();
                };

                ExecuteTest(method, "Com.PointD.ToString()");
            }

            // Equals

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = left;

                Action method = () =>
                {
                    bool result = left.Equals(right);
                };

                ExecuteTest(method, "Com.PointD.Equals(Com.PointD)");
            }

            // Offset

            {
                Com.PointD pointD = _GetRandomPointD();
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD.Offset(d);
                };

                ExecuteTest(method, "Com.PointD.Offset(double)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                double dx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dy = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD.Offset(dx, dy);
                };

                ExecuteTest(method, "Com.PointD.Offset(double, double)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    pointD.Offset(pt);
                };

                ExecuteTest(method, "Com.PointD.Offset(Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Point pt = new Point(Com.Statistics.RandomInteger(), Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    pointD.Offset(pt);
                };

                ExecuteTest(method, "Com.PointD.Offset(System.Drawing.Point)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                PointF pt = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    pointD.Offset(pt);
                };

                ExecuteTest(method, "Com.PointD.Offset(System.Drawing.PointF)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Size sz = new Size(Com.Statistics.RandomInteger(), Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    pointD.Offset(sz);
                };

                ExecuteTest(method, "Com.PointD.Offset(System.Drawing.Size)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    pointD.Offset(sz);
                };

                ExecuteTest(method, "Com.PointD.Offset(System.Drawing.SizeF)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD result = pointD.OffsetCopy(d);
                };

                ExecuteTest(method, "Com.PointD.OffsetCopy(double)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                double dx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dy = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD result = pointD.OffsetCopy(dx, dy);
                };

                ExecuteTest(method, "Com.PointD.OffsetCopy(double, double)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = pointD.OffsetCopy(pt);
                };

                ExecuteTest(method, "Com.PointD.OffsetCopy(Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Point pt = new Point(Com.Statistics.RandomInteger(), Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    Com.PointD result = pointD.OffsetCopy(pt);
                };

                ExecuteTest(method, "Com.PointD.OffsetCopy(System.Drawing.Point)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                PointF pt = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    Com.PointD result = pointD.OffsetCopy(pt);
                };

                ExecuteTest(method, "Com.PointD.OffsetCopy(System.Drawing.PointF)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Size sz = new Size(Com.Statistics.RandomInteger(), Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    Com.PointD result = pointD.OffsetCopy(sz);
                };

                ExecuteTest(method, "Com.PointD.OffsetCopy(System.Drawing.Size)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    Com.PointD result = pointD.OffsetCopy(sz);
                };

                ExecuteTest(method, "Com.PointD.OffsetCopy(System.Drawing.SizeF)");
            }

            // Scale

            {
                Com.PointD pointD = _GetRandomPointD();
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD.Scale(s);
                };

                ExecuteTest(method, "Com.PointD.Scale(double)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                double sx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sy = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD.Scale(sx, sy);
                };

                ExecuteTest(method, "Com.PointD.Scale(double, double)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    pointD.Scale(pt);
                };

                ExecuteTest(method, "Com.PointD.Scale(Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Point pt = new Point(Com.Statistics.RandomInteger(), Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    pointD.Scale(pt);
                };

                ExecuteTest(method, "Com.PointD.Scale(System.Drawing.Point)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                PointF pt = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    pointD.Scale(pt);
                };

                ExecuteTest(method, "Com.PointD.Scale(System.Drawing.PointF)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Size sz = new Size(Com.Statistics.RandomInteger(), Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    pointD.Scale(sz);
                };

                ExecuteTest(method, "Com.PointD.Scale(System.Drawing.Size)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    pointD.Scale(sz);
                };

                ExecuteTest(method, "Com.PointD.Scale(System.Drawing.SizeF)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD result = pointD.ScaleCopy(s);
                };

                ExecuteTest(method, "Com.PointD.ScaleCopy(double)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                double sx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sy = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD result = pointD.ScaleCopy(sx, sy);
                };

                ExecuteTest(method, "Com.PointD.ScaleCopy(double, double)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = pointD.ScaleCopy(pt);
                };

                ExecuteTest(method, "Com.PointD.ScaleCopy(Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Point pt = new Point(Com.Statistics.RandomInteger(), Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    Com.PointD result = pointD.ScaleCopy(pt);
                };

                ExecuteTest(method, "Com.PointD.ScaleCopy(System.Drawing.Point)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                PointF pt = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    Com.PointD result = pointD.ScaleCopy(pt);
                };

                ExecuteTest(method, "Com.PointD.ScaleCopy(System.Drawing.PointF)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Size sz = new Size(Com.Statistics.RandomInteger(), Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    Com.PointD result = pointD.ScaleCopy(sz);
                };

                ExecuteTest(method, "Com.PointD.ScaleCopy(System.Drawing.Size)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    Com.PointD result = pointD.ScaleCopy(sz);
                };

                ExecuteTest(method, "Com.PointD.ScaleCopy(System.Drawing.SizeF)");
            }

            // Rotate

            {
                Com.PointD pointD = _GetRandomPointD();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD.Rotate(angle);
                };

                ExecuteTest(method, "Com.PointD.Rotate(double)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.PointD pt = _GetRandomPointD();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD.Rotate(angle, pt);
                };

                ExecuteTest(method, "Com.PointD.Rotate(double, Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.PointD result = pointD.RotateCopy(angle);
                };

                ExecuteTest(method, "Com.PointD.RotateCopy(double)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.PointD pt = _GetRandomPointD();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.PointD result = pointD.RotateCopy(angle, pt);
                };

                ExecuteTest(method, "Com.PointD.RotateCopy(double, Com.PointD)");
            }

            // Affine

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.PointD ex = _GetRandomPointD().VectorNormalize;
                Com.PointD ey = _GetRandomPointD().VectorNormalize;
                Com.PointD offset = _GetRandomPointD();

                Action method = () =>
                {
                    pointD.AffineTransform(ex, ey, offset);
                };

                ExecuteTest(method, "Com.PointD.AffineTransform(Com.PointD, Com.PointD, Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.Matrix matrixLeft = _GetRandomMatrix(3, 3);

                Action method = () =>
                {
                    pointD.AffineTransform(matrixLeft);
                };

                ExecuteTest(method, "Com.PointD.AffineTransform(Com.Matrix)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(3, 3));
                }

                Action method = () =>
                {
                    pointD.AffineTransform(matrixLeftList);
                };

                ExecuteTest(method, "Com.PointD.AffineTransform(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.PointD ex = _GetRandomPointD().VectorNormalize;
                Com.PointD ey = _GetRandomPointD().VectorNormalize;
                Com.PointD offset = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = pointD.AffineTransformCopy(ex, ey, offset);
                };

                ExecuteTest(method, "Com.PointD.AffineTransformCopy(Com.PointD, Com.PointD, Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.Matrix matrixLeft = _GetRandomMatrix(3, 3);

                Action method = () =>
                {
                    Com.PointD result = pointD.AffineTransformCopy(matrixLeft);
                };

                ExecuteTest(method, "Com.PointD.AffineTransformCopy(Com.Matrix)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(3, 3));
                }

                Action method = () =>
                {
                    Com.PointD result = pointD.AffineTransformCopy(matrixLeftList);
                };

                ExecuteTest(method, "Com.PointD.AffineTransformCopy(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.PointD ex = _GetRandomPointD().VectorNormalize;
                Com.PointD ey = _GetRandomPointD().VectorNormalize;
                Com.PointD offset = _GetRandomPointD();

                Action method = () =>
                {
                    pointD.InverseAffineTransform(ex, ey, offset);
                };

                ExecuteTest(method, "Com.PointD.InverseAffineTransform(Com.PointD, Com.PointD, Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.Matrix matrixLeft = _GetRandomMatrix(3, 3);

                Action method = () =>
                {
                    pointD.InverseAffineTransform(matrixLeft);
                };

                ExecuteTest(method, "Com.PointD.InverseAffineTransform(Com.Matrix)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(3, 3));
                }

                Action method = () =>
                {
                    pointD.InverseAffineTransform(matrixLeftList);
                };

                ExecuteTest(method, "Com.PointD.InverseAffineTransform(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.PointD ex = _GetRandomPointD().VectorNormalize;
                Com.PointD ey = _GetRandomPointD().VectorNormalize;
                Com.PointD offset = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = pointD.InverseAffineTransformCopy(ex, ey, offset);
                };

                ExecuteTest(method, "Com.PointD.InverseAffineTransformCopy(Com.PointD, Com.PointD, Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.Matrix matrixLeft = _GetRandomMatrix(3, 3);

                Action method = () =>
                {
                    Com.PointD result = pointD.InverseAffineTransformCopy(matrixLeft);
                };

                ExecuteTest(method, "Com.PointD.InverseAffineTransformCopy(Com.Matrix)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(3, 3));
                }

                Action method = () =>
                {
                    Com.PointD result = pointD.InverseAffineTransformCopy(matrixLeftList);
                };

                ExecuteTest(method, "Com.PointD.InverseAffineTransformCopy(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            // 距离与角度

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    double result = pointD.DistanceFrom(pt);
                };

                ExecuteTest(method, "Com.PointD.DistanceFrom(Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    double result = pointD.AngleFrom(pt);
                };

                ExecuteTest(method, "Com.PointD.AngleFrom(Com.PointD)");
            }

            // 坐标系转换

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = pointD.ToPolar();
                };

                ExecuteTest(method, "Com.PointD.ToPolar()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = pointD.ToCartesian();
                };

                ExecuteTest(method, "Com.PointD.ToCartesian()");
            }

            // ToVector

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    Com.Vector result = pointD.ToVector();
                };

                ExecuteTest(method, "Com.PointD.ToVector()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    Com.Vector result = pointD.ToVectorColumn();
                };

                ExecuteTest(method, "Com.PointD.ToVectorColumn()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    Com.Vector result = pointD.ToVectorRow();
                };

                ExecuteTest(method, "Com.PointD.ToVectorRow()");
            }

            // To

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    Point result = pointD.ToPoint();
                };

                ExecuteTest(method, "Com.PointD.ToPoint()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    PointF result = pointD.ToPointF();
                };

                ExecuteTest(method, "Com.PointD.ToPointF()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    Size result = pointD.ToSize();
                };

                ExecuteTest(method, "Com.PointD.ToSize()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    SizeF result = pointD.ToSizeF();
                };

                ExecuteTest(method, "Com.PointD.ToSizeF()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    Com.Complex result = pointD.ToComplex();
                };

                ExecuteTest(method, "Com.PointD.ToComplex()");
            }

            // ToArray

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    double[] result = pointD.ToArray();
                };

                ExecuteTest(method, "Com.PointD.ToArray()");
            }
        }

        protected override void StaticMethod()
        {
            // Equals

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = left;

                Action method = () =>
                {
                    bool result = Com.PointD.Equals(left, right);
                };

                ExecuteTest(method, "Com.PointD.Equals(Com.PointD, Com.PointD)");
            }

            // From

            {
                Point pt = new Point(Com.Statistics.RandomInteger(), Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    Com.PointD result = Com.PointD.FromPoint(pt);
                };

                ExecuteTest(method, "Com.PointD.FromPoint(System.Drawing.Point)");
            }

            {
                PointF pt = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    Com.PointD result = Com.PointD.FromPointF(pt);
                };

                ExecuteTest(method, "Com.PointD.FromPointF(System.Drawing.PointF)");
            }

            {
                Size sz = new Size(Com.Statistics.RandomInteger(), Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    Com.PointD result = Com.PointD.FromSize(sz);
                };

                ExecuteTest(method, "Com.PointD.FromSize(System.Drawing.Size)");
            }

            {
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    Com.PointD result = Com.PointD.FromSizeF(sz);
                };

                ExecuteTest(method, "Com.PointD.FromSizeF(System.Drawing.SizeF)");
            }

            {
                Com.Complex comp = new Com.Complex(Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    Com.PointD result = Com.PointD.FromComplex(comp);
                };

                ExecuteTest(method, "Com.PointD.FromComplex(Com.Complex)");
            }

            // Matrix

            {
                Action method = () =>
                {
                    Com.Matrix result = Com.PointD.IdentityMatrix();
                };

                ExecuteTest(method, "Com.PointD.IdentityMatrix()");
            }

            {
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD.OffsetMatrix(d);
                };

                ExecuteTest(method, "Com.PointD.OffsetMatrix(double)");
            }

            {
                double dx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dy = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD.OffsetMatrix(dx, dy);
                };

                ExecuteTest(method, "Com.PointD.OffsetMatrix(double, double)");
            }

            {
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD.OffsetMatrix(pt);
                };

                ExecuteTest(method, "Com.PointD.OffsetMatrix(Com.PointD)");
            }

            {
                Point pt = new Point(Com.Statistics.RandomInteger(), Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD.OffsetMatrix(pt);
                };

                ExecuteTest(method, "Com.PointD.OffsetMatrix(System.Drawing.Point)");
            }

            {
                PointF pt = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD.OffsetMatrix(pt);
                };

                ExecuteTest(method, "Com.PointD.OffsetMatrix(System.Drawing.PointF)");
            }

            {
                Size sz = new Size(Com.Statistics.RandomInteger(), Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD.OffsetMatrix(sz);
                };

                ExecuteTest(method, "Com.PointD.OffsetMatrix(System.Drawing.Size)");
            }

            {
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD.OffsetMatrix(sz);
                };

                ExecuteTest(method, "Com.PointD.OffsetMatrix(System.Drawing.SizeF)");
            }

            {
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD.ScaleMatrix(s);
                };

                ExecuteTest(method, "Com.PointD.ScaleMatrix(double)");
            }

            {
                double sx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sy = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD.ScaleMatrix(sx, sy);
                };

                ExecuteTest(method, "Com.PointD.ScaleMatrix(double, double)");
            }

            {
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD.ScaleMatrix(pt);
                };

                ExecuteTest(method, "Com.PointD.ScaleMatrix(Com.PointD)");
            }

            {
                Point pt = new Point(Com.Statistics.RandomInteger(), Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD.ScaleMatrix(pt);
                };

                ExecuteTest(method, "Com.PointD.ScaleMatrix(System.Drawing.Point)");
            }

            {
                PointF pt = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD.ScaleMatrix(pt);
                };

                ExecuteTest(method, "Com.PointD.ScaleMatrix(System.Drawing.PointF)");
            }

            {
                Size sz = new Size(Com.Statistics.RandomInteger(), Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD.ScaleMatrix(sz);
                };

                ExecuteTest(method, "Com.PointD.ScaleMatrix(System.Drawing.Size)");
            }

            {
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD.ScaleMatrix(sz);
                };

                ExecuteTest(method, "Com.PointD.ScaleMatrix(System.Drawing.SizeF)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD.RotateMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD.RotateMatrix(double)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD.RotateMatrix(angle, pt);
                };

                ExecuteTest(method, "Com.PointD.RotateMatrix(double, Com.PointD)");
            }

            // 距离与角度

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    double result = Com.PointD.DistanceBetween(left, right);
                };

                ExecuteTest(method, "Com.PointD.DistanceBetween(Com.PointD, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    double result = Com.PointD.AngleBetween(left, right);
                };

                ExecuteTest(method, "Com.PointD.AngleBetween(Com.PointD, Com.PointD)");
            }

            // 向量乘积

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    double result = Com.PointD.DotProduct(left, right);
                };

                ExecuteTest(method, "Com.PointD.DotProduct(Com.PointD, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    Com.Vector result = Com.PointD.CrossProduct(left, right);
                };

                ExecuteTest(method, "Com.PointD.CrossProduct(Com.PointD, Com.PointD)");
            }

            // 初等函数

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = Com.PointD.Abs(pointD);
                };

                ExecuteTest(method, "Com.PointD.Abs(Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = Com.PointD.Sign(pointD);
                };

                ExecuteTest(method, "Com.PointD.Sign(Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = Com.PointD.Ceiling(pointD);
                };

                ExecuteTest(method, "Com.PointD.Ceiling(Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = Com.PointD.Floor(pointD);
                };

                ExecuteTest(method, "Com.PointD.Floor(Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = Com.PointD.Round(pointD);
                };

                ExecuteTest(method, "Com.PointD.Round(Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = Com.PointD.Truncate(pointD);
                };

                ExecuteTest(method, "Com.PointD.Truncate(Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = Com.PointD.Max(left, right);
                };

                ExecuteTest(method, "Com.PointD.Max(Com.PointD, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = Com.PointD.Min(left, right);
                };

                ExecuteTest(method, "Com.PointD.Min(Com.PointD, Com.PointD)");
            }
        }

        protected override void Operator()
        {
            // 比较

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = left;

                Action method = () =>
                {
                    bool result = (left == right);
                };

                ExecuteTest(method, "Com.PointD.operator ==(Com.PointD, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = left;

                Action method = () =>
                {
                    bool result = (left != right);
                };

                ExecuteTest(method, "Com.PointD.operator !=(Com.PointD, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    bool result = (left < right);
                };

                ExecuteTest(method, "Com.PointD.operator <(Com.PointD, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    bool result = (left > right);
                };

                ExecuteTest(method, "Com.PointD.operator >(Com.PointD, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    bool result = (left <= right);
                };

                ExecuteTest(method, "Com.PointD.operator <=(Com.PointD, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    bool result = (left >= right);
                };

                ExecuteTest(method, "Com.PointD.operator >=(Com.PointD, Com.PointD)");
            }

            // 运算

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = +pointD;
                };

                ExecuteTest(method, "Com.PointD.operator +(Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = -pointD;
                };

                ExecuteTest(method, "Com.PointD.operator -(Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD result = pt + n;
                };

                ExecuteTest(method, "Com.PointD.operator +(Com.PointD, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = n + pt;
                };

                ExecuteTest(method, "Com.PointD.operator +(double, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = left + right;
                };

                ExecuteTest(method, "Com.PointD.operator +(Com.PointD, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Point right = new Point(Com.Statistics.RandomInteger(), Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    Com.PointD result = left + right;
                };

                ExecuteTest(method, "Com.PointD.operator +(Com.PointD, System.Drawing.Point)");
            }

            {
                Point left = new Point(Com.Statistics.RandomInteger(), Com.Statistics.RandomInteger());
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = left + right;
                };

                ExecuteTest(method, "Com.PointD.operator +(System.Drawing.Point, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                PointF right = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    Com.PointD result = left + right;
                };

                ExecuteTest(method, "Com.PointD.operator +(Com.PointD, System.Drawing.PointF)");
            }

            {
                PointF left = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = left + right;
                };

                ExecuteTest(method, "Com.PointD.operator +(System.Drawing.PointF, Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                Size sz = new Size(Com.Statistics.RandomInteger(), Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    Com.PointD result = pt + sz;
                };

                ExecuteTest(method, "Com.PointD.operator +(Com.PointD, System.Drawing.Size)");
            }

            {
                Size sz = new Size(Com.Statistics.RandomInteger(), Com.Statistics.RandomInteger());
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = sz + pt;
                };

                ExecuteTest(method, "Com.PointD.operator +(System.Drawing.Size, Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    Com.PointD result = pt + sz;
                };

                ExecuteTest(method, "Com.PointD.operator +(Com.PointD, System.Drawing.SizeF)");
            }

            {
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = sz + pt;
                };

                ExecuteTest(method, "Com.PointD.operator +(System.Drawing.SizeF, Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD result = pt - n;
                };

                ExecuteTest(method, "Com.PointD.operator -(Com.PointD, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = n - pt;
                };

                ExecuteTest(method, "Com.PointD.operator -(double, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = left - right;
                };

                ExecuteTest(method, "Com.PointD.operator -(Com.PointD, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Point right = new Point(Com.Statistics.RandomInteger(), Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    Com.PointD result = left - right;
                };

                ExecuteTest(method, "Com.PointD.operator -(Com.PointD, System.Drawing.Point)");
            }

            {
                Point left = new Point(Com.Statistics.RandomInteger(), Com.Statistics.RandomInteger());
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = left - right;
                };

                ExecuteTest(method, "Com.PointD.operator -(System.Drawing.Point, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                PointF right = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    Com.PointD result = left - right;
                };

                ExecuteTest(method, "Com.PointD.operator -(Com.PointD, System.Drawing.PointF)");
            }

            {
                PointF left = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = left - right;
                };

                ExecuteTest(method, "Com.PointD.operator -(System.Drawing.PointF, Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                Size sz = new Size(Com.Statistics.RandomInteger(), Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    Com.PointD result = pt - sz;
                };

                ExecuteTest(method, "Com.PointD.operator -(Com.PointD, System.Drawing.Size)");
            }

            {
                Size sz = new Size(Com.Statistics.RandomInteger(), Com.Statistics.RandomInteger());
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = sz - pt;
                };

                ExecuteTest(method, "Com.PointD.operator -(System.Drawing.Size, Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    Com.PointD result = pt - sz;
                };

                ExecuteTest(method, "Com.PointD.operator -(Com.PointD, System.Drawing.SizeF)");
            }

            {
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = sz - pt;
                };

                ExecuteTest(method, "Com.PointD.operator -(System.Drawing.SizeF, Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD result = pt * n;
                };

                ExecuteTest(method, "Com.PointD.operator *(Com.PointD, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = n * pt;
                };

                ExecuteTest(method, "Com.PointD.operator *(double, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = left * right;
                };

                ExecuteTest(method, "Com.PointD.operator *(Com.PointD, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Point right = new Point(Com.Statistics.RandomInteger(), Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    Com.PointD result = left * right;
                };

                ExecuteTest(method, "Com.PointD.operator *(Com.PointD, System.Drawing.Point)");
            }

            {
                Point left = new Point(Com.Statistics.RandomInteger(), Com.Statistics.RandomInteger());
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = left * right;
                };

                ExecuteTest(method, "Com.PointD.operator *(System.Drawing.Point, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                PointF right = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    Com.PointD result = left * right;
                };

                ExecuteTest(method, "Com.PointD.operator *(Com.PointD, System.Drawing.PointF)");
            }

            {
                PointF left = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = left * right;
                };

                ExecuteTest(method, "Com.PointD.operator *(System.Drawing.PointF, Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                Size sz = new Size(Com.Statistics.RandomInteger(), Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    Com.PointD result = pt * sz;
                };

                ExecuteTest(method, "Com.PointD.operator *(Com.PointD, System.Drawing.Size)");
            }

            {
                Size sz = new Size(Com.Statistics.RandomInteger(), Com.Statistics.RandomInteger());
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = sz * pt;
                };

                ExecuteTest(method, "Com.PointD.operator *(System.Drawing.Size, Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    Com.PointD result = pt * sz;
                };

                ExecuteTest(method, "Com.PointD.operator *(Com.PointD, System.Drawing.SizeF)");
            }

            {
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = sz * pt;
                };

                ExecuteTest(method, "Com.PointD.operator *(System.Drawing.SizeF, Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD result = pt / n;
                };

                ExecuteTest(method, "Com.PointD.operator /(Com.PointD, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = n / pt;
                };

                ExecuteTest(method, "Com.PointD.operator /(double, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = left / right;
                };

                ExecuteTest(method, "Com.PointD.operator /(Com.PointD, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Point right = new Point(Com.Statistics.RandomInteger(), Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    Com.PointD result = left / right;
                };

                ExecuteTest(method, "Com.PointD.operator /(Com.PointD, System.Drawing.Point)");
            }

            {
                Point left = new Point(Com.Statistics.RandomInteger(), Com.Statistics.RandomInteger());
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = left / right;
                };

                ExecuteTest(method, "Com.PointD.operator /(System.Drawing.Point, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                PointF right = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    Com.PointD result = left / right;
                };

                ExecuteTest(method, "Com.PointD.operator /(Com.PointD, System.Drawing.PointF)");
            }

            {
                PointF left = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = left / right;
                };

                ExecuteTest(method, "Com.PointD.operator /(System.Drawing.PointF, Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                Size sz = new Size(Com.Statistics.RandomInteger(), Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    Com.PointD result = pt / sz;
                };

                ExecuteTest(method, "Com.PointD.operator /(Com.PointD, System.Drawing.Size)");
            }

            {
                Size sz = new Size(Com.Statistics.RandomInteger(), Com.Statistics.RandomInteger());
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = sz / pt;
                };

                ExecuteTest(method, "Com.PointD.operator /(System.Drawing.Size, Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    Com.PointD result = pt / sz;
                };

                ExecuteTest(method, "Com.PointD.operator /(Com.PointD, System.Drawing.SizeF)");
            }

            {
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    Com.PointD result = sz / pt;
                };

                ExecuteTest(method, "Com.PointD.operator /(System.Drawing.SizeF, Com.PointD)");
            }
        }
    }

    sealed class PointD3DTest : ClassPerformanceTestBase
    {
        private static Com.PointD3D _GetRandomPointD3D()
        {
            return new Com.PointD3D(Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18));
        }

        private static Com.PointD _GetRandomPointD()
        {
            return new Com.PointD(Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18));
        }

        private static Com.Matrix _GetRandomMatrix(int width, int height)
        {
            if (width > 0 && height > 0)
            {
                Com.Matrix matrix = new Com.Matrix(width, height);

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        matrix[x, y] = Com.Statistics.RandomDouble(-1E18, 1E18);
                    }
                }

                return matrix;
            }
            else
            {
                return Com.Matrix.NonMatrix;
            }
        }

        //

        protected override void Constructor()
        {
            {
                double x = Com.Statistics.RandomDouble(-1E18, 1E18);
                double y = Com.Statistics.RandomDouble(-1E18, 1E18);
                double z = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD3D pointD3D = new Com.PointD3D(x, y, z);
                };

                ExecuteTest(method, "Com.PointD3D.PointD3D(double, double, double)");
            }
        }

        protected override void Property()
        {
            // 索引器

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                int index = Com.Statistics.RandomInteger(3);

                Action method = () =>
                {
                    double result = pointD3D[index];
                };

                ExecuteTest(method, "Com.PointD3D.this[int].get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                int index = Com.Statistics.RandomInteger(3);
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD3D[index] = value;
                };

                ExecuteTest(method, "Com.PointD3D.this[int].set(double)");
            }

            // Is

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    bool result = pointD3D.IsEmpty;
                };

                ExecuteTest(method, "Com.PointD3D.IsEmpty.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    bool result = pointD3D.IsNaN;
                };

                ExecuteTest(method, "Com.PointD3D.IsNaN.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    bool result = pointD3D.IsInfinity;
                };

                ExecuteTest(method, "Com.PointD3D.IsInfinity.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    bool result = pointD3D.IsNaNOrInfinity;
                };

                ExecuteTest(method, "Com.PointD3D.IsNaNOrInfinity.get()");
            }

            // 分量

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    double result = pointD3D.X;
                };

                ExecuteTest(method, "Com.PointD3D.X.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD3D.X = value;
                };

                ExecuteTest(method, "Com.PointD3D.X.set(double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    double result = pointD3D.Y;
                };

                ExecuteTest(method, "Com.PointD3D.Y.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD3D.Y = value;
                };

                ExecuteTest(method, "Com.PointD3D.Y.set(double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    double result = pointD3D.Z;
                };

                ExecuteTest(method, "Com.PointD3D.Z.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD3D.Z = value;
                };

                ExecuteTest(method, "Com.PointD3D.Z.set(double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    Com.PointD result = pointD3D.XY;
                };

                ExecuteTest(method, "Com.PointD3D.XY.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD value = _GetRandomPointD();

                Action method = () =>
                {
                    pointD3D.XY = value;
                };

                ExecuteTest(method, "Com.PointD3D.XY.set(Com.PointD)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    Com.PointD result = pointD3D.YZ;
                };

                ExecuteTest(method, "Com.PointD3D.YZ.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD value = _GetRandomPointD();

                Action method = () =>
                {
                    pointD3D.YZ = value;
                };

                ExecuteTest(method, "Com.PointD3D.YZ.set(Com.PointD)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    Com.PointD result = pointD3D.ZX;
                };

                ExecuteTest(method, "Com.PointD3D.ZX.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD value = _GetRandomPointD();

                Action method = () =>
                {
                    pointD3D.ZX = value;
                };

                ExecuteTest(method, "Com.PointD3D.ZX.set(Com.PointD)");
            }

            // 角度

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    double result = pointD3D.AngleX;
                };

                ExecuteTest(method, "Com.PointD3D.AngleX.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    double result = pointD3D.AngleY;
                };

                ExecuteTest(method, "Com.PointD3D.AngleY.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    double result = pointD3D.AngleZ;
                };

                ExecuteTest(method, "Com.PointD3D.AngleZ.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    double result = pointD3D.AngleXY;
                };

                ExecuteTest(method, "Com.PointD3D.AngleXY.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    double result = pointD3D.AngleYZ;
                };

                ExecuteTest(method, "Com.PointD3D.AngleYZ.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    double result = pointD3D.AngleZX;
                };

                ExecuteTest(method, "Com.PointD3D.AngleZX.get()");
            }

            // 向量

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    double result = pointD3D.VectorModule;
                };

                ExecuteTest(method, "Com.PointD3D.VectorModule.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    double result = pointD3D.VectorModuleSquared;
                };

                ExecuteTest(method, "Com.PointD3D.VectorModuleSquared.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    double result = pointD3D.VectorAngleZ;
                };

                ExecuteTest(method, "Com.PointD3D.VectorAngleZ.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    double result = pointD3D.VectorAngleXY;
                };

                ExecuteTest(method, "Com.PointD3D.VectorAngleXY.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    Com.PointD3D result = pointD3D.VectorNegate;
                };

                ExecuteTest(method, "Com.PointD3D.VectorNegate.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    Com.PointD3D result = pointD3D.VectorNormalize;
                };

                ExecuteTest(method, "Com.PointD3D.VectorNormalize.get()");
            }
        }

        protected override void StaticProperty()
        {

        }

        protected override void Method()
        {
            // object

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                object obj = (object)pointD3D;

                Action method = () =>
                {
                    bool result = pointD3D.Equals(obj);
                };

                ExecuteTest(method, "Com.PointD3D.Equals(object)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    int result = pointD3D.GetHashCode();
                };

                ExecuteTest(method, "Com.PointD3D.GetHashCode()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    string result = pointD3D.ToString();
                };

                ExecuteTest(method, "Com.PointD3D.ToString()");
            }

            // Equals

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = left;

                Action method = () =>
                {
                    bool result = left.Equals(right);
                };

                ExecuteTest(method, "Com.PointD3D.Equals(Com.PointD3D)");
            }

            // Offset

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD3D.Offset(d);
                };

                ExecuteTest(method, "Com.PointD3D.Offset(double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double dx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dy = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dz = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD3D.Offset(dx, dy, dz);
                };

                ExecuteTest(method, "Com.PointD3D.Offset(double, double, double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD3D pt = _GetRandomPointD3D();

                Action method = () =>
                {
                    pointD3D.Offset(pt);
                };

                ExecuteTest(method, "Com.PointD3D.Offset(Com.PointD3D)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD3D result = pointD3D.OffsetCopy(d);
                };

                ExecuteTest(method, "Com.PointD3D.OffsetCopy(double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double dx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dy = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dz = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD3D result = pointD3D.OffsetCopy(dx, dy, dz);
                };

                ExecuteTest(method, "Com.PointD3D.OffsetCopy(double, double, double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD3D pt = _GetRandomPointD3D();

                Action method = () =>
                {
                    Com.PointD3D result = pointD3D.OffsetCopy(pt);
                };

                ExecuteTest(method, "Com.PointD3D.OffsetCopy(Com.PointD3D)");
            }

            // Scale

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD3D.Scale(s);
                };

                ExecuteTest(method, "Com.PointD3D.Scale(double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double sx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sy = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sz = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD3D.Scale(sx, sy, sz);
                };

                ExecuteTest(method, "Com.PointD3D.Scale(double, double, double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD3D pt = _GetRandomPointD3D();

                Action method = () =>
                {
                    pointD3D.Scale(pt);
                };

                ExecuteTest(method, "Com.PointD3D.Scale(Com.PointD3D)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD3D result = pointD3D.ScaleCopy(s);
                };

                ExecuteTest(method, "Com.PointD3D.ScaleCopy(double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double sx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sy = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sz = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD3D result = pointD3D.ScaleCopy(sx, sy, sz);
                };

                ExecuteTest(method, "Com.PointD3D.ScaleCopy(double, double, double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD3D pt = _GetRandomPointD3D();

                Action method = () =>
                {
                    Com.PointD3D result = pointD3D.ScaleCopy(pt);
                };

                ExecuteTest(method, "Com.PointD3D.ScaleCopy(Com.PointD3D)");
            }

            // Rotate

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD3D.RotateX(angle);
                };

                ExecuteTest(method, "Com.PointD3D.RotateX(double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD3D.RotateY(angle);
                };

                ExecuteTest(method, "Com.PointD3D.RotateY(double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD3D.RotateZ(angle);
                };

                ExecuteTest(method, "Com.PointD3D.RotateZ(double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.PointD3D result = pointD3D.RotateXCopy(angle);
                };

                ExecuteTest(method, "Com.PointD3D.RotateXCopy(double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.PointD3D result = pointD3D.RotateYCopy(angle);
                };

                ExecuteTest(method, "Com.PointD3D.RotateYCopy(double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.PointD3D result = pointD3D.RotateZCopy(angle);
                };

                ExecuteTest(method, "Com.PointD3D.RotateZCopy(double)");
            }

            // Affine

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD3D ex = _GetRandomPointD3D().VectorNormalize;
                Com.PointD3D ey = _GetRandomPointD3D().VectorNormalize;
                Com.PointD3D ez = _GetRandomPointD3D().VectorNormalize;
                Com.PointD3D offset = _GetRandomPointD3D();

                Action method = () =>
                {
                    pointD3D.AffineTransform(ex, ey, ez, offset);
                };

                ExecuteTest(method, "Com.PointD3D.AffineTransform(Com.PointD3D, Com.PointD3D, Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.Matrix matrixLeft = _GetRandomMatrix(4, 4);

                Action method = () =>
                {
                    pointD3D.AffineTransform(matrixLeft);
                };

                ExecuteTest(method, "Com.PointD3D.AffineTransform(Com.Matrix)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(4, 4));
                }

                Action method = () =>
                {
                    pointD3D.AffineTransform(matrixLeftList);
                };

                ExecuteTest(method, "Com.PointD3D.AffineTransform(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD3D ex = _GetRandomPointD3D().VectorNormalize;
                Com.PointD3D ey = _GetRandomPointD3D().VectorNormalize;
                Com.PointD3D ez = _GetRandomPointD3D().VectorNormalize;
                Com.PointD3D offset = _GetRandomPointD3D();

                Action method = () =>
                {
                    Com.PointD3D result = pointD3D.AffineTransformCopy(ex, ey, ez, offset);
                };

                ExecuteTest(method, "Com.PointD3D.AffineTransformCopy(Com.PointD3D, Com.PointD3D, Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.Matrix matrixLeft = _GetRandomMatrix(4, 4);

                Action method = () =>
                {
                    Com.PointD3D result = pointD3D.AffineTransformCopy(matrixLeft);
                };

                ExecuteTest(method, "Com.PointD3D.AffineTransformCopy(Com.Matrix)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(4, 4));
                }

                Action method = () =>
                {
                    Com.PointD3D result = pointD3D.AffineTransformCopy(matrixLeftList);
                };

                ExecuteTest(method, "Com.PointD3D.AffineTransformCopy(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD3D ex = _GetRandomPointD3D().VectorNormalize;
                Com.PointD3D ey = _GetRandomPointD3D().VectorNormalize;
                Com.PointD3D ez = _GetRandomPointD3D().VectorNormalize;
                Com.PointD3D offset = _GetRandomPointD3D();

                Action method = () =>
                {
                    pointD3D.InverseAffineTransform(ex, ey, ez, offset);
                };

                ExecuteTest(method, "Com.PointD3D.InverseAffineTransform(Com.PointD3D, Com.PointD3D, Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.Matrix matrixLeft = _GetRandomMatrix(4, 4);

                Action method = () =>
                {
                    pointD3D.InverseAffineTransform(matrixLeft);
                };

                ExecuteTest(method, "Com.PointD3D.InverseAffineTransform(Com.Matrix)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(4, 4));
                }

                Action method = () =>
                {
                    pointD3D.InverseAffineTransform(matrixLeftList);
                };

                ExecuteTest(method, "Com.PointD3D.InverseAffineTransform(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD3D ex = _GetRandomPointD3D().VectorNormalize;
                Com.PointD3D ey = _GetRandomPointD3D().VectorNormalize;
                Com.PointD3D ez = _GetRandomPointD3D().VectorNormalize;
                Com.PointD3D offset = _GetRandomPointD3D();

                Action method = () =>
                {
                    Com.PointD3D result = pointD3D.InverseAffineTransformCopy(ex, ey, ez, offset);
                };

                ExecuteTest(method, "Com.PointD3D.InverseAffineTransformCopy(Com.PointD3D, Com.PointD3D, Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.Matrix matrixLeft = _GetRandomMatrix(4, 4);

                Action method = () =>
                {
                    Com.PointD3D result = pointD3D.InverseAffineTransformCopy(matrixLeft);
                };

                ExecuteTest(method, "Com.PointD3D.InverseAffineTransformCopy(Com.Matrix)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(4, 4));
                }

                Action method = () =>
                {
                    Com.PointD3D result = pointD3D.InverseAffineTransformCopy(matrixLeftList);
                };

                ExecuteTest(method, "Com.PointD3D.InverseAffineTransformCopy(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            // Project

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD3D prjCenter = _GetRandomPointD3D();
                double trueLenDist = (pointD3D.Z - prjCenter.Z) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    Com.PointD result = pointD3D.ProjectToXY(prjCenter, trueLenDist);
                };

                ExecuteTest(method, "Com.PointD3D.ProjectToXY(Com.PointD3D, double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD3D prjCenter = _GetRandomPointD3D();
                double trueLenDist = (pointD3D.X - prjCenter.X) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    Com.PointD result = pointD3D.ProjectToYZ(prjCenter, trueLenDist);
                };

                ExecuteTest(method, "Com.PointD3D.ProjectToYZ(Com.PointD3D, double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD3D prjCenter = _GetRandomPointD3D();
                double trueLenDist = (pointD3D.Y - prjCenter.Y) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    Com.PointD result = pointD3D.ProjectToZX(prjCenter, trueLenDist);
                };

                ExecuteTest(method, "Com.PointD3D.ProjectToZX(Com.PointD3D, double)");
            }

            // 距离与角度

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD3D pt = _GetRandomPointD3D();

                Action method = () =>
                {
                    double result = pointD3D.DistanceFrom(pt);
                };

                ExecuteTest(method, "Com.PointD3D.DistanceFrom(Com.PointD3D)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD3D pt = _GetRandomPointD3D();

                Action method = () =>
                {
                    double result = pointD3D.AngleFrom(pt);
                };

                ExecuteTest(method, "Com.PointD3D.AngleFrom(Com.PointD3D)");
            }

            // 坐标系转换

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    Com.PointD3D result = pointD3D.ToSpherical();
                };

                ExecuteTest(method, "Com.PointD3D.ToSpherical()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    Com.PointD3D result = pointD3D.ToCartesian();
                };

                ExecuteTest(method, "Com.PointD3D.ToCartesian()");
            }

            // ToVector

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    Com.Vector result = pointD3D.ToVector();
                };

                ExecuteTest(method, "Com.PointD3D.ToVector()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    Com.Vector result = pointD3D.ToVectorColumn();
                };

                ExecuteTest(method, "Com.PointD3D.ToVectorColumn()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    Com.Vector result = pointD3D.ToVectorRow();
                };

                ExecuteTest(method, "Com.PointD3D.ToVectorRow()");
            }

            // ToArray

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    double[] result = pointD3D.ToArray();
                };

                ExecuteTest(method, "Com.PointD3D.ToArray()");
            }
        }

        protected override void StaticMethod()
        {
            // Equals

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = left;

                Action method = () =>
                {
                    bool result = Com.PointD3D.Equals(left, right);
                };

                ExecuteTest(method, "Com.PointD3D.Equals(Com.PointD3D, Com.PointD3D)");
            }

            // Matrix

            {
                Action method = () =>
                {
                    Com.Matrix result = Com.PointD3D.IdentityMatrix();
                };

                ExecuteTest(method, "Com.PointD3D.IdentityMatrix()");
            }

            {
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD3D.OffsetMatrix(d);
                };

                ExecuteTest(method, "Com.PointD3D.OffsetMatrix(double)");
            }

            {
                double dx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dy = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dz = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD3D.OffsetMatrix(dx, dy, dz);
                };

                ExecuteTest(method, "Com.PointD3D.OffsetMatrix(double, double, double)");
            }

            {
                Com.PointD3D pt = _GetRandomPointD3D();

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD3D.OffsetMatrix(pt);
                };

                ExecuteTest(method, "Com.PointD3D.OffsetMatrix(Com.PointD3D)");
            }

            {
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD3D.ScaleMatrix(s);
                };

                ExecuteTest(method, "Com.PointD3D.ScaleMatrix(double)");
            }

            {
                double sx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sy = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sz = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD3D.ScaleMatrix(sx, sy, sz);
                };

                ExecuteTest(method, "Com.PointD3D.ScaleMatrix(double, double, double)");
            }

            {
                Com.PointD3D pt = _GetRandomPointD3D();

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD3D.ScaleMatrix(pt);
                };

                ExecuteTest(method, "Com.PointD3D.ScaleMatrix(Com.PointD3D)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD3D.RotateXMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD3D.RotateXMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD3D.RotateYMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD3D.RotateYMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD3D.RotateZMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD3D.RotateZMatrix(double)");
            }

            // 距离与角度

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    double result = Com.PointD3D.DistanceBetween(left, right);
                };

                ExecuteTest(method, "Com.PointD3D.DistanceBetween(Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    double result = Com.PointD3D.AngleBetween(left, right);
                };

                ExecuteTest(method, "Com.PointD3D.AngleBetween(Com.PointD3D, Com.PointD3D)");
            }

            // 向量乘积

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    double result = Com.PointD3D.DotProduct(left, right);
                };

                ExecuteTest(method, "Com.PointD3D.DotProduct(Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    Com.PointD3D result = Com.PointD3D.CrossProduct(left, right);
                };

                ExecuteTest(method, "Com.PointD3D.CrossProduct(Com.PointD3D, Com.PointD3D)");
            }

            // 初等函数

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    Com.PointD3D result = Com.PointD3D.Abs(pointD3D);
                };

                ExecuteTest(method, "Com.PointD3D.Abs(Com.PointD3D)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    Com.PointD3D result = Com.PointD3D.Sign(pointD3D);
                };

                ExecuteTest(method, "Com.PointD3D.Sign(Com.PointD3D)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    Com.PointD3D result = Com.PointD3D.Ceiling(pointD3D);
                };

                ExecuteTest(method, "Com.PointD3D.Ceiling(Com.PointD3D)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    Com.PointD3D result = Com.PointD3D.Floor(pointD3D);
                };

                ExecuteTest(method, "Com.PointD3D.Floor(Com.PointD3D)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    Com.PointD3D result = Com.PointD3D.Round(pointD3D);
                };

                ExecuteTest(method, "Com.PointD3D.Round(Com.PointD3D)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    Com.PointD3D result = Com.PointD3D.Truncate(pointD3D);
                };

                ExecuteTest(method, "Com.PointD3D.Truncate(Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    Com.PointD3D result = Com.PointD3D.Max(left, right);
                };

                ExecuteTest(method, "Com.PointD3D.Max(Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    Com.PointD3D result = Com.PointD3D.Min(left, right);
                };

                ExecuteTest(method, "Com.PointD3D.Min(Com.PointD3D, Com.PointD3D)");
            }
        }

        protected override void Operator()
        {
            // 比较

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = left;

                Action method = () =>
                {
                    bool result = (left == right);
                };

                ExecuteTest(method, "Com.PointD3D.operator ==(Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = left;

                Action method = () =>
                {
                    bool result = (left != right);
                };

                ExecuteTest(method, "Com.PointD3D.operator !=(Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    bool result = (left < right);
                };

                ExecuteTest(method, "Com.PointD3D.operator <(Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    bool result = (left > right);
                };

                ExecuteTest(method, "Com.PointD3D.operator >(Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    bool result = (left <= right);
                };

                ExecuteTest(method, "Com.PointD3D.operator <=(Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    bool result = (left >= right);
                };

                ExecuteTest(method, "Com.PointD3D.operator >=(Com.PointD3D, Com.PointD3D)");
            }

            // 运算

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    Com.PointD3D result = +pointD3D;
                };

                ExecuteTest(method, "Com.PointD3D.operator +(Com.PointD3D)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    Com.PointD3D result = -pointD3D;
                };

                ExecuteTest(method, "Com.PointD3D.operator -(Com.PointD3D)");
            }

            {
                Com.PointD3D pt = _GetRandomPointD3D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD3D result = pt + n;
                };

                ExecuteTest(method, "Com.PointD3D.operator +(Com.PointD3D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD3D pt = _GetRandomPointD3D();

                Action method = () =>
                {
                    Com.PointD3D result = n + pt;
                };

                ExecuteTest(method, "Com.PointD3D.operator +(double, Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    Com.PointD3D result = left + right;
                };

                ExecuteTest(method, "Com.PointD3D.operator +(Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D pt = _GetRandomPointD3D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD3D result = pt - n;
                };

                ExecuteTest(method, "Com.PointD3D.operator -(Com.PointD3D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD3D pt = _GetRandomPointD3D();

                Action method = () =>
                {
                    Com.PointD3D result = n - pt;
                };

                ExecuteTest(method, "Com.PointD3D.operator -(double, Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    Com.PointD3D result = left - right;
                };

                ExecuteTest(method, "Com.PointD3D.operator -(Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D pt = _GetRandomPointD3D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD3D result = pt * n;
                };

                ExecuteTest(method, "Com.PointD3D.operator *(Com.PointD3D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD3D pt = _GetRandomPointD3D();

                Action method = () =>
                {
                    Com.PointD3D result = n * pt;
                };

                ExecuteTest(method, "Com.PointD3D.operator *(double, Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    Com.PointD3D result = left * right;
                };

                ExecuteTest(method, "Com.PointD3D.operator *(Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D pt = _GetRandomPointD3D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD3D result = pt / n;
                };

                ExecuteTest(method, "Com.PointD3D.operator /(Com.PointD3D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD3D pt = _GetRandomPointD3D();

                Action method = () =>
                {
                    Com.PointD3D result = n / pt;
                };

                ExecuteTest(method, "Com.PointD3D.operator /(double, Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    Com.PointD3D result = left / right;
                };

                ExecuteTest(method, "Com.PointD3D.operator /(Com.PointD3D, Com.PointD3D)");
            }
        }
    }

    sealed class PointD4DTest : ClassPerformanceTestBase
    {
        private static Com.PointD4D _GetRandomPointD4D()
        {
            return new Com.PointD4D(Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18));
        }

        private static Com.PointD3D _GetRandomPointD3D()
        {
            return new Com.PointD3D(Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18));
        }

        private static Com.Matrix _GetRandomMatrix(int width, int height)
        {
            if (width > 0 && height > 0)
            {
                Com.Matrix matrix = new Com.Matrix(width, height);

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        matrix[x, y] = Com.Statistics.RandomDouble(-1E18, 1E18);
                    }
                }

                return matrix;
            }
            else
            {
                return Com.Matrix.NonMatrix;
            }
        }

        //

        protected override void Constructor()
        {
            {
                double x = Com.Statistics.RandomDouble(-1E18, 1E18);
                double y = Com.Statistics.RandomDouble(-1E18, 1E18);
                double z = Com.Statistics.RandomDouble(-1E18, 1E18);
                double u = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD4D pointD4D = new Com.PointD4D(x, y, z, u);
                };

                ExecuteTest(method, "Com.PointD4D.PointD4D(double, double, double, double)");
            }
        }

        protected override void Property()
        {
            // 索引器

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                int index = Com.Statistics.RandomInteger(4);

                Action method = () =>
                {
                    double result = pointD4D[index];
                };

                ExecuteTest(method, "Com.PointD4D.this[int].get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                int index = Com.Statistics.RandomInteger(4);
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD4D[index] = value;
                };

                ExecuteTest(method, "Com.PointD4D.this[int].set(double)");
            }

            // Is

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    bool result = pointD4D.IsEmpty;
                };

                ExecuteTest(method, "Com.PointD4D.IsEmpty.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    bool result = pointD4D.IsNaN;
                };

                ExecuteTest(method, "Com.PointD4D.IsNaN.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    bool result = pointD4D.IsInfinity;
                };

                ExecuteTest(method, "Com.PointD4D.IsInfinity.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    bool result = pointD4D.IsNaNOrInfinity;
                };

                ExecuteTest(method, "Com.PointD4D.IsNaNOrInfinity.get()");
            }

            // 分量

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    double result = pointD4D.X;
                };

                ExecuteTest(method, "Com.PointD4D.X.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD4D.X = value;
                };

                ExecuteTest(method, "Com.PointD4D.X.set(double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    double result = pointD4D.Y;
                };

                ExecuteTest(method, "Com.PointD4D.Y.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD4D.Y = value;
                };

                ExecuteTest(method, "Com.PointD4D.Y.set(double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    double result = pointD4D.Z;
                };

                ExecuteTest(method, "Com.PointD4D.Z.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD4D.Z = value;
                };

                ExecuteTest(method, "Com.PointD4D.Z.set(double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    double result = pointD4D.U;
                };

                ExecuteTest(method, "Com.PointD4D.U.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD4D.U = value;
                };

                ExecuteTest(method, "Com.PointD4D.U.set(double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    Com.PointD3D result = pointD4D.XYZ;
                };

                ExecuteTest(method, "Com.PointD4D.XYZ.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD3D value = _GetRandomPointD3D();

                Action method = () =>
                {
                    pointD4D.XYZ = value;
                };

                ExecuteTest(method, "Com.PointD4D.XYZ.set(Com.PointD3D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    Com.PointD3D result = pointD4D.YZU;
                };

                ExecuteTest(method, "Com.PointD4D.YZU.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD3D value = _GetRandomPointD3D();

                Action method = () =>
                {
                    pointD4D.YZU = value;
                };

                ExecuteTest(method, "Com.PointD4D.YZU.set(Com.PointD3D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    Com.PointD3D result = pointD4D.ZUX;
                };

                ExecuteTest(method, "Com.PointD4D.ZUX.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD3D value = _GetRandomPointD3D();

                Action method = () =>
                {
                    pointD4D.ZUX = value;
                };

                ExecuteTest(method, "Com.PointD4D.ZUX.set(Com.PointD3D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    Com.PointD3D result = pointD4D.UXY;
                };

                ExecuteTest(method, "Com.PointD4D.UXY.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD3D value = _GetRandomPointD3D();

                Action method = () =>
                {
                    pointD4D.UXY = value;
                };

                ExecuteTest(method, "Com.PointD4D.UXY.set(Com.PointD3D)");
            }

            // 角度

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    double result = pointD4D.AngleX;
                };

                ExecuteTest(method, "Com.PointD4D.AngleX.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    double result = pointD4D.AngleY;
                };

                ExecuteTest(method, "Com.PointD4D.AngleY.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    double result = pointD4D.AngleZ;
                };

                ExecuteTest(method, "Com.PointD4D.AngleZ.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    double result = pointD4D.AngleU;
                };

                ExecuteTest(method, "Com.PointD4D.AngleU.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    double result = pointD4D.AngleXYZ;
                };

                ExecuteTest(method, "Com.PointD4D.AngleXYZ.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    double result = pointD4D.AngleYZU;
                };

                ExecuteTest(method, "Com.PointD4D.AngleYZU.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    double result = pointD4D.AngleZUX;
                };

                ExecuteTest(method, "Com.PointD4D.AngleZUX.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    double result = pointD4D.AngleUXY;
                };

                ExecuteTest(method, "Com.PointD4D.AngleUXY.get()");
            }

            // 向量

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    double result = pointD4D.VectorModule;
                };

                ExecuteTest(method, "Com.PointD4D.VectorModule.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    double result = pointD4D.VectorModuleSquared;
                };

                ExecuteTest(method, "Com.PointD4D.VectorModuleSquared.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    double result = pointD4D.VectorAngleX;
                };

                ExecuteTest(method, "Com.PointD4D.VectorAngleX.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    double result = pointD4D.VectorAngleY;
                };

                ExecuteTest(method, "Com.PointD4D.VectorAngleY.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    double result = pointD4D.VectorAngleZU;
                };

                ExecuteTest(method, "Com.PointD4D.VectorAngleZU.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    Com.PointD4D result = pointD4D.VectorNegate;
                };

                ExecuteTest(method, "Com.PointD4D.VectorNegate.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    Com.PointD4D result = pointD4D.VectorNormalize;
                };

                ExecuteTest(method, "Com.PointD4D.VectorNormalize.get()");
            }
        }

        protected override void StaticProperty()
        {

        }

        protected override void Method()
        {
            // object

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                object obj = (object)pointD4D;

                Action method = () =>
                {
                    bool result = pointD4D.Equals(obj);
                };

                ExecuteTest(method, "Com.PointD4D.Equals(object)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    int result = pointD4D.GetHashCode();
                };

                ExecuteTest(method, "Com.PointD4D.GetHashCode()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    string result = pointD4D.ToString();
                };

                ExecuteTest(method, "Com.PointD4D.ToString()");
            }

            // Equals

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = left;

                Action method = () =>
                {
                    bool result = left.Equals(right);
                };

                ExecuteTest(method, "Com.PointD4D.Equals(Com.PointD4D)");
            }

            // Offset

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD4D.Offset(d);
                };

                ExecuteTest(method, "Com.PointD4D.Offset(double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double dx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dy = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dz = Com.Statistics.RandomDouble(-1E18, 1E18);
                double du = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD4D.Offset(dx, dy, dz, du);
                };

                ExecuteTest(method, "Com.PointD4D.Offset(double, double, double, double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD4D pt = _GetRandomPointD4D();

                Action method = () =>
                {
                    pointD4D.Offset(pt);
                };

                ExecuteTest(method, "Com.PointD4D.Offset(Com.PointD4D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD4D result = pointD4D.OffsetCopy(d);
                };

                ExecuteTest(method, "Com.PointD4D.OffsetCopy(double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double dx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dy = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dz = Com.Statistics.RandomDouble(-1E18, 1E18);
                double du = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD4D result = pointD4D.OffsetCopy(dx, dy, dz, du);
                };

                ExecuteTest(method, "Com.PointD4D.OffsetCopy(double, double, double, double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD4D pt = _GetRandomPointD4D();

                Action method = () =>
                {
                    Com.PointD4D result = pointD4D.OffsetCopy(pt);
                };

                ExecuteTest(method, "Com.PointD4D.OffsetCopy(Com.PointD4D)");
            }

            // Scale

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD4D.Scale(s);
                };

                ExecuteTest(method, "Com.PointD4D.Scale(double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double sx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sy = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sz = Com.Statistics.RandomDouble(-1E18, 1E18);
                double su = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD4D.Scale(sx, sy, sz, su);
                };

                ExecuteTest(method, "Com.PointD4D.Scale(double, double, double, double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD4D pt = _GetRandomPointD4D();

                Action method = () =>
                {
                    pointD4D.Scale(pt);
                };

                ExecuteTest(method, "Com.PointD4D.Scale(Com.PointD4D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD4D result = pointD4D.ScaleCopy(s);
                };

                ExecuteTest(method, "Com.PointD4D.ScaleCopy(double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double sx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sy = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sz = Com.Statistics.RandomDouble(-1E18, 1E18);
                double su = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD4D result = pointD4D.ScaleCopy(sx, sy, sz, su);
                };

                ExecuteTest(method, "Com.PointD4D.ScaleCopy(double, double, double, double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD4D pt = _GetRandomPointD4D();

                Action method = () =>
                {
                    Com.PointD4D result = pointD4D.ScaleCopy(pt);
                };

                ExecuteTest(method, "Com.PointD4D.ScaleCopy(Com.PointD4D)");
            }

            // Rotate

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD4D.RotateXY(angle);
                };

                ExecuteTest(method, "Com.PointD4D.RotateXY(double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD4D.RotateXZ(angle);
                };

                ExecuteTest(method, "Com.PointD4D.RotateXZ(double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD4D.RotateXU(angle);
                };

                ExecuteTest(method, "Com.PointD4D.RotateXU(double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD4D.RotateYZ(angle);
                };

                ExecuteTest(method, "Com.PointD4D.RotateYZ(double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD4D.RotateYU(angle);
                };

                ExecuteTest(method, "Com.PointD4D.RotateYU(double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD4D.RotateZU(angle);
                };

                ExecuteTest(method, "Com.PointD4D.RotateZU(double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.PointD4D result = pointD4D.RotateXYCopy(angle);
                };

                ExecuteTest(method, "Com.PointD4D.RotateXYCopy(double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.PointD4D result = pointD4D.RotateXZCopy(angle);
                };

                ExecuteTest(method, "Com.PointD4D.RotateXZCopy(double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.PointD4D result = pointD4D.RotateXUCopy(angle);
                };

                ExecuteTest(method, "Com.PointD4D.RotateXUCopy(double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.PointD4D result = pointD4D.RotateYZCopy(angle);
                };

                ExecuteTest(method, "Com.PointD4D.RotateYZCopy(double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.PointD4D result = pointD4D.RotateYUCopy(angle);
                };

                ExecuteTest(method, "Com.PointD4D.RotateYUCopy(double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.PointD4D result = pointD4D.RotateZUCopy(angle);
                };

                ExecuteTest(method, "Com.PointD4D.RotateZUCopy(double)");
            }

            // Affine

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD4D ex = _GetRandomPointD4D().VectorNormalize;
                Com.PointD4D ey = _GetRandomPointD4D().VectorNormalize;
                Com.PointD4D ez = _GetRandomPointD4D().VectorNormalize;
                Com.PointD4D eu = _GetRandomPointD4D().VectorNormalize;
                Com.PointD4D offset = _GetRandomPointD4D();

                Action method = () =>
                {
                    pointD4D.AffineTransform(ex, ey, ez, eu, offset);
                };

                ExecuteTest(method, "Com.PointD4D.AffineTransform(Com.PointD4D, Com.PointD4D, Com.PointD4D, Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.Matrix matrixLeft = _GetRandomMatrix(5, 5);

                Action method = () =>
                {
                    pointD4D.AffineTransform(matrixLeft);
                };

                ExecuteTest(method, "Com.PointD4D.AffineTransform(Com.Matrix)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(5, 5));
                }

                Action method = () =>
                {
                    pointD4D.AffineTransform(matrixLeftList);
                };

                ExecuteTest(method, "Com.PointD4D.AffineTransform(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD4D ex = _GetRandomPointD4D().VectorNormalize;
                Com.PointD4D ey = _GetRandomPointD4D().VectorNormalize;
                Com.PointD4D ez = _GetRandomPointD4D().VectorNormalize;
                Com.PointD4D eu = _GetRandomPointD4D().VectorNormalize;
                Com.PointD4D offset = _GetRandomPointD4D();

                Action method = () =>
                {
                    Com.PointD4D result = pointD4D.AffineTransformCopy(ex, ey, ez, eu, offset);
                };

                ExecuteTest(method, "Com.PointD4D.AffineTransformCopy(Com.PointD4D, Com.PointD4D, Com.PointD4D, Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.Matrix matrixLeft = _GetRandomMatrix(5, 5);

                Action method = () =>
                {
                    Com.PointD4D result = pointD4D.AffineTransformCopy(matrixLeft);
                };

                ExecuteTest(method, "Com.PointD4D.AffineTransformCopy(Com.Matrix)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(5, 5));
                }

                Action method = () =>
                {
                    Com.PointD4D result = pointD4D.AffineTransformCopy(matrixLeftList);
                };

                ExecuteTest(method, "Com.PointD4D.AffineTransformCopy(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD4D ex = _GetRandomPointD4D().VectorNormalize;
                Com.PointD4D ey = _GetRandomPointD4D().VectorNormalize;
                Com.PointD4D ez = _GetRandomPointD4D().VectorNormalize;
                Com.PointD4D eu = _GetRandomPointD4D().VectorNormalize;
                Com.PointD4D offset = _GetRandomPointD4D();

                Action method = () =>
                {
                    pointD4D.InverseAffineTransform(ex, ey, ez, eu, offset);
                };

                ExecuteTest(method, "Com.PointD4D.InverseAffineTransform(Com.PointD4D, Com.PointD4D, Com.PointD4D, Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.Matrix matrixLeft = _GetRandomMatrix(5, 5);

                Action method = () =>
                {
                    pointD4D.InverseAffineTransform(matrixLeft);
                };

                ExecuteTest(method, "Com.PointD4D.InverseAffineTransform(Com.Matrix)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(5, 5));
                }

                Action method = () =>
                {
                    pointD4D.InverseAffineTransform(matrixLeftList);
                };

                ExecuteTest(method, "Com.PointD4D.InverseAffineTransform(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD4D ex = _GetRandomPointD4D().VectorNormalize;
                Com.PointD4D ey = _GetRandomPointD4D().VectorNormalize;
                Com.PointD4D ez = _GetRandomPointD4D().VectorNormalize;
                Com.PointD4D eu = _GetRandomPointD4D().VectorNormalize;
                Com.PointD4D offset = _GetRandomPointD4D();

                Action method = () =>
                {
                    Com.PointD4D result = pointD4D.InverseAffineTransformCopy(ex, ey, ez, eu, offset);
                };

                ExecuteTest(method, "Com.PointD4D.InverseAffineTransformCopy(Com.PointD4D, Com.PointD4D, Com.PointD4D, Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.Matrix matrixLeft = _GetRandomMatrix(5, 5);

                Action method = () =>
                {
                    Com.PointD4D result = pointD4D.InverseAffineTransformCopy(matrixLeft);
                };

                ExecuteTest(method, "Com.PointD4D.InverseAffineTransformCopy(Com.Matrix)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(5, 5));
                }

                Action method = () =>
                {
                    Com.PointD4D result = pointD4D.InverseAffineTransformCopy(matrixLeftList);
                };

                ExecuteTest(method, "Com.PointD4D.InverseAffineTransformCopy(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            // Project

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD4D prjCenter = _GetRandomPointD4D();
                double trueLenDist = (pointD4D.U - prjCenter.U) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    Com.PointD3D result = pointD4D.ProjectToXYZ(prjCenter, trueLenDist);
                };

                ExecuteTest(method, "Com.PointD4D.ProjectToXYZ(Com.PointD4D, double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD4D prjCenter = _GetRandomPointD4D();
                double trueLenDist = (pointD4D.X - prjCenter.X) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    Com.PointD3D result = pointD4D.ProjectToYZU(prjCenter, trueLenDist);
                };

                ExecuteTest(method, "Com.PointD4D.ProjectToYZU(Com.PointD4D, double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD4D prjCenter = _GetRandomPointD4D();
                double trueLenDist = (pointD4D.Y - prjCenter.Y) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    Com.PointD3D result = pointD4D.ProjectToZUX(prjCenter, trueLenDist);
                };

                ExecuteTest(method, "Com.PointD4D.ProjectToZUX(Com.PointD4D, double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD4D prjCenter = _GetRandomPointD4D();
                double trueLenDist = (pointD4D.Z - prjCenter.Z) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    Com.PointD3D result = pointD4D.ProjectToUXY(prjCenter, trueLenDist);
                };

                ExecuteTest(method, "Com.PointD4D.ProjectToUXY(Com.PointD4D, double)");
            }

            // 距离与角度

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD4D pt = _GetRandomPointD4D();

                Action method = () =>
                {
                    double result = pointD4D.DistanceFrom(pt);
                };

                ExecuteTest(method, "Com.PointD4D.DistanceFrom(Com.PointD4D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD4D pt = _GetRandomPointD4D();

                Action method = () =>
                {
                    double result = pointD4D.AngleFrom(pt);
                };

                ExecuteTest(method, "Com.PointD4D.AngleFrom(Com.PointD4D)");
            }

            // 坐标系转换

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    Com.PointD4D result = pointD4D.ToSpherical();
                };

                ExecuteTest(method, "Com.PointD4D.ToSpherical()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    Com.PointD4D result = pointD4D.ToCartesian();
                };

                ExecuteTest(method, "Com.PointD4D.ToCartesian()");
            }

            // ToVector

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    Com.Vector result = pointD4D.ToVector();
                };

                ExecuteTest(method, "Com.PointD4D.ToVector()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    Com.Vector result = pointD4D.ToVectorColumn();
                };

                ExecuteTest(method, "Com.PointD4D.ToVectorColumn()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    Com.Vector result = pointD4D.ToVectorRow();
                };

                ExecuteTest(method, "Com.PointD4D.ToVectorRow()");
            }

            // ToArray

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    double[] result = pointD4D.ToArray();
                };

                ExecuteTest(method, "Com.PointD4D.ToArray()");
            }
        }

        protected override void StaticMethod()
        {
            // Equals

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = left;

                Action method = () =>
                {
                    bool result = Com.PointD4D.Equals(left, right);
                };

                ExecuteTest(method, "Com.PointD4D.Equals(Com.PointD4D, Com.PointD4D)");
            }

            // Matrix

            {
                Action method = () =>
                {
                    Com.Matrix result = Com.PointD4D.IdentityMatrix();
                };

                ExecuteTest(method, "Com.PointD4D.IdentityMatrix()");
            }

            {
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD4D.OffsetMatrix(d);
                };

                ExecuteTest(method, "Com.PointD4D.OffsetMatrix(double)");
            }

            {
                double dx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dy = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dz = Com.Statistics.RandomDouble(-1E18, 1E18);
                double du = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD4D.OffsetMatrix(dx, dy, dz, du);
                };

                ExecuteTest(method, "Com.PointD4D.OffsetMatrix(double, double, double, double)");
            }

            {
                Com.PointD4D pt = _GetRandomPointD4D();

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD4D.OffsetMatrix(pt);
                };

                ExecuteTest(method, "Com.PointD4D.OffsetMatrix(Com.PointD4D)");
            }

            {
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD4D.ScaleMatrix(s);
                };

                ExecuteTest(method, "Com.PointD4D.ScaleMatrix(double)");
            }

            {
                double sx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sy = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sz = Com.Statistics.RandomDouble(-1E18, 1E18);
                double su = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD4D.ScaleMatrix(sx, sy, sz, su);
                };

                ExecuteTest(method, "Com.PointD4D.ScaleMatrix(double, double, double, double)");
            }

            {
                Com.PointD4D pt = _GetRandomPointD4D();

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD4D.ScaleMatrix(pt);
                };

                ExecuteTest(method, "Com.PointD4D.ScaleMatrix(Com.PointD4D)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD4D.RotateXYMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD4D.RotateXYMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD4D.RotateXZMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD4D.RotateXZMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD4D.RotateXUMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD4D.RotateXUMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD4D.RotateYZMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD4D.RotateYZMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD4D.RotateYUMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD4D.RotateYUMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD4D.RotateZUMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD4D.RotateZUMatrix(double)");
            }

            // 距离与角度

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    double result = Com.PointD4D.DistanceBetween(left, right);
                };

                ExecuteTest(method, "Com.PointD4D.DistanceBetween(Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    double result = Com.PointD4D.AngleBetween(left, right);
                };

                ExecuteTest(method, "Com.PointD4D.AngleBetween(Com.PointD4D, Com.PointD4D)");
            }

            // 向量乘积

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    double result = Com.PointD4D.DotProduct(left, right);
                };

                ExecuteTest(method, "Com.PointD4D.DotProduct(Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    Com.Vector result = Com.PointD4D.CrossProduct(left, right);
                };

                ExecuteTest(method, "Com.PointD4D.CrossProduct(Com.PointD4D, Com.PointD4D)");
            }

            // 初等函数

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    Com.PointD4D result = Com.PointD4D.Abs(pointD4D);
                };

                ExecuteTest(method, "Com.PointD4D.Abs(Com.PointD4D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    Com.PointD4D result = Com.PointD4D.Sign(pointD4D);
                };

                ExecuteTest(method, "Com.PointD4D.Sign(Com.PointD4D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    Com.PointD4D result = Com.PointD4D.Ceiling(pointD4D);
                };

                ExecuteTest(method, "Com.PointD4D.Ceiling(Com.PointD4D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    Com.PointD4D result = Com.PointD4D.Floor(pointD4D);
                };

                ExecuteTest(method, "Com.PointD4D.Floor(Com.PointD4D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    Com.PointD4D result = Com.PointD4D.Round(pointD4D);
                };

                ExecuteTest(method, "Com.PointD4D.Round(Com.PointD4D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    Com.PointD4D result = Com.PointD4D.Truncate(pointD4D);
                };

                ExecuteTest(method, "Com.PointD4D.Truncate(Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    Com.PointD4D result = Com.PointD4D.Max(left, right);
                };

                ExecuteTest(method, "Com.PointD4D.Max(Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    Com.PointD4D result = Com.PointD4D.Min(left, right);
                };

                ExecuteTest(method, "Com.PointD4D.Min(Com.PointD4D, Com.PointD4D)");
            }
        }

        protected override void Operator()
        {
            // 比较

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = left;

                Action method = () =>
                {
                    bool result = (left == right);
                };

                ExecuteTest(method, "Com.PointD4D.operator ==(Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = left;

                Action method = () =>
                {
                    bool result = (left != right);
                };

                ExecuteTest(method, "Com.PointD4D.operator !=(Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    bool result = (left < right);
                };

                ExecuteTest(method, "Com.PointD4D.operator <(Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    bool result = (left > right);
                };

                ExecuteTest(method, "Com.PointD4D.operator >(Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    bool result = (left <= right);
                };

                ExecuteTest(method, "Com.PointD4D.operator <=(Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    bool result = (left >= right);
                };

                ExecuteTest(method, "Com.PointD4D.operator >=(Com.PointD4D, Com.PointD4D)");
            }

            // 运算

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    Com.PointD4D result = +pointD4D;
                };

                ExecuteTest(method, "Com.PointD4D.operator +(Com.PointD4D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    Com.PointD4D result = -pointD4D;
                };

                ExecuteTest(method, "Com.PointD4D.operator -(Com.PointD4D)");
            }

            {
                Com.PointD4D pt = _GetRandomPointD4D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD4D result = pt + n;
                };

                ExecuteTest(method, "Com.PointD4D.operator +(Com.PointD4D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD4D pt = _GetRandomPointD4D();

                Action method = () =>
                {
                    Com.PointD4D result = n + pt;
                };

                ExecuteTest(method, "Com.PointD4D.operator +(double, Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    Com.PointD4D result = left + right;
                };

                ExecuteTest(method, "Com.PointD4D.operator +(Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D pt = _GetRandomPointD4D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD4D result = pt - n;
                };

                ExecuteTest(method, "Com.PointD4D.operator -(Com.PointD4D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD4D pt = _GetRandomPointD4D();

                Action method = () =>
                {
                    Com.PointD4D result = n - pt;
                };

                ExecuteTest(method, "Com.PointD4D.operator -(double, Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    Com.PointD4D result = left - right;
                };

                ExecuteTest(method, "Com.PointD4D.operator -(Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D pt = _GetRandomPointD4D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD4D result = pt * n;
                };

                ExecuteTest(method, "Com.PointD4D.operator *(Com.PointD4D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD4D pt = _GetRandomPointD4D();

                Action method = () =>
                {
                    Com.PointD4D result = n * pt;
                };

                ExecuteTest(method, "Com.PointD4D.operator *(double, Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    Com.PointD4D result = left * right;
                };

                ExecuteTest(method, "Com.PointD4D.operator *(Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D pt = _GetRandomPointD4D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD4D result = pt / n;
                };

                ExecuteTest(method, "Com.PointD4D.operator /(Com.PointD4D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD4D pt = _GetRandomPointD4D();

                Action method = () =>
                {
                    Com.PointD4D result = n / pt;
                };

                ExecuteTest(method, "Com.PointD4D.operator /(double, Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    Com.PointD4D result = left / right;
                };

                ExecuteTest(method, "Com.PointD4D.operator /(Com.PointD4D, Com.PointD4D)");
            }
        }
    }

    sealed class PointD5DTest : ClassPerformanceTestBase
    {
        private static Com.PointD5D _GetRandomPointD5D()
        {
            return new Com.PointD5D(Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18));
        }

        private static Com.PointD4D _GetRandomPointD4D()
        {
            return new Com.PointD4D(Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18));
        }

        private static Com.Matrix _GetRandomMatrix(int width, int height)
        {
            if (width > 0 && height > 0)
            {
                Com.Matrix matrix = new Com.Matrix(width, height);

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        matrix[x, y] = Com.Statistics.RandomDouble(-1E18, 1E18);
                    }
                }

                return matrix;
            }
            else
            {
                return Com.Matrix.NonMatrix;
            }
        }

        //

        protected override void Constructor()
        {
            {
                double x = Com.Statistics.RandomDouble(-1E18, 1E18);
                double y = Com.Statistics.RandomDouble(-1E18, 1E18);
                double z = Com.Statistics.RandomDouble(-1E18, 1E18);
                double u = Com.Statistics.RandomDouble(-1E18, 1E18);
                double v = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD5D pointD5D = new Com.PointD5D(x, y, z, u, v);
                };

                ExecuteTest(method, "Com.PointD5D.PointD5D(double, double, double, double, double)");
            }
        }

        protected override void Property()
        {
            // 索引器

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                int index = Com.Statistics.RandomInteger(5);

                Action method = () =>
                {
                    double result = pointD5D[index];
                };

                ExecuteTest(method, "Com.PointD5D.this[int].get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                int index = Com.Statistics.RandomInteger(5);
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD5D[index] = value;
                };

                ExecuteTest(method, "Com.PointD5D.this[int].set(double)");
            }

            // Is

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    bool result = pointD5D.IsEmpty;
                };

                ExecuteTest(method, "Com.PointD5D.IsEmpty.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    bool result = pointD5D.IsNaN;
                };

                ExecuteTest(method, "Com.PointD5D.IsNaN.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    bool result = pointD5D.IsInfinity;
                };

                ExecuteTest(method, "Com.PointD5D.IsInfinity.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    bool result = pointD5D.IsNaNOrInfinity;
                };

                ExecuteTest(method, "Com.PointD5D.IsNaNOrInfinity.get()");
            }

            // 分量

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    double result = pointD5D.X;
                };

                ExecuteTest(method, "Com.PointD5D.X.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD5D.X = value;
                };

                ExecuteTest(method, "Com.PointD5D.X.set(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    double result = pointD5D.Y;
                };

                ExecuteTest(method, "Com.PointD5D.Y.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD5D.Y = value;
                };

                ExecuteTest(method, "Com.PointD5D.Y.set(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    double result = pointD5D.Z;
                };

                ExecuteTest(method, "Com.PointD5D.Z.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD5D.Z = value;
                };

                ExecuteTest(method, "Com.PointD5D.Z.set(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    double result = pointD5D.U;
                };

                ExecuteTest(method, "Com.PointD5D.U.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD5D.U = value;
                };

                ExecuteTest(method, "Com.PointD5D.U.set(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    double result = pointD5D.V;
                };

                ExecuteTest(method, "Com.PointD5D.V.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD5D.V = value;
                };

                ExecuteTest(method, "Com.PointD5D.V.set(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    Com.PointD4D result = pointD5D.XYZU;
                };

                ExecuteTest(method, "Com.PointD5D.XYZU.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD4D value = _GetRandomPointD4D();

                Action method = () =>
                {
                    pointD5D.XYZU = value;
                };

                ExecuteTest(method, "Com.PointD5D.XYZU.set(Com.PointD4D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    Com.PointD4D result = pointD5D.YZUV;
                };

                ExecuteTest(method, "Com.PointD5D.YZUV.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD4D value = _GetRandomPointD4D();

                Action method = () =>
                {
                    pointD5D.YZUV = value;
                };

                ExecuteTest(method, "Com.PointD5D.YZUV.set(Com.PointD4D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    Com.PointD4D result = pointD5D.ZUVX;
                };

                ExecuteTest(method, "Com.PointD5D.ZUVX.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD4D value = _GetRandomPointD4D();

                Action method = () =>
                {
                    pointD5D.ZUVX = value;
                };

                ExecuteTest(method, "Com.PointD5D.ZUVX.set(Com.PointD4D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    Com.PointD4D result = pointD5D.UVXY;
                };

                ExecuteTest(method, "Com.PointD5D.UVXY.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD4D value = _GetRandomPointD4D();

                Action method = () =>
                {
                    pointD5D.UVXY = value;
                };

                ExecuteTest(method, "Com.PointD5D.UVXY.set(Com.PointD4D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    Com.PointD4D result = pointD5D.VXYZ;
                };

                ExecuteTest(method, "Com.PointD5D.VXYZ.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD4D value = _GetRandomPointD4D();

                Action method = () =>
                {
                    pointD5D.VXYZ = value;
                };

                ExecuteTest(method, "Com.PointD5D.VXYZ.set(Com.PointD4D)");
            }

            // 角度

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    double result = pointD5D.AngleX;
                };

                ExecuteTest(method, "Com.PointD5D.AngleX.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    double result = pointD5D.AngleY;
                };

                ExecuteTest(method, "Com.PointD5D.AngleY.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    double result = pointD5D.AngleZ;
                };

                ExecuteTest(method, "Com.PointD5D.AngleZ.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    double result = pointD5D.AngleU;
                };

                ExecuteTest(method, "Com.PointD5D.AngleU.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    double result = pointD5D.AngleV;
                };

                ExecuteTest(method, "Com.PointD5D.AngleV.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    double result = pointD5D.AngleXYZU;
                };

                ExecuteTest(method, "Com.PointD5D.AngleXYZU.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    double result = pointD5D.AngleYZUV;
                };

                ExecuteTest(method, "Com.PointD5D.AngleYZUV.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    double result = pointD5D.AngleZUVX;
                };

                ExecuteTest(method, "Com.PointD5D.AngleZUVX.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    double result = pointD5D.AngleUVXY;
                };

                ExecuteTest(method, "Com.PointD5D.AngleUVXY.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    double result = pointD5D.AngleVXYZ;
                };

                ExecuteTest(method, "Com.PointD5D.AngleVXYZ.get()");
            }

            // 向量

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    double result = pointD5D.VectorModule;
                };

                ExecuteTest(method, "Com.PointD5D.VectorModule.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    double result = pointD5D.VectorModuleSquared;
                };

                ExecuteTest(method, "Com.PointD5D.VectorModuleSquared.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    double result = pointD5D.VectorAngleX;
                };

                ExecuteTest(method, "Com.PointD5D.VectorAngleX.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    double result = pointD5D.VectorAngleY;
                };

                ExecuteTest(method, "Com.PointD5D.VectorAngleY.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    double result = pointD5D.VectorAngleZ;
                };

                ExecuteTest(method, "Com.PointD5D.VectorAngleZ.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    double result = pointD5D.VectorAngleUV;
                };

                ExecuteTest(method, "Com.PointD5D.VectorAngleUV.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    Com.PointD5D result = pointD5D.VectorNegate;
                };

                ExecuteTest(method, "Com.PointD5D.VectorNegate.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    Com.PointD5D result = pointD5D.VectorNormalize;
                };

                ExecuteTest(method, "Com.PointD5D.VectorNormalize.get()");
            }
        }

        protected override void StaticProperty()
        {

        }

        protected override void Method()
        {
            // object

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                object obj = (object)pointD5D;

                Action method = () =>
                {
                    bool result = pointD5D.Equals(obj);
                };

                ExecuteTest(method, "Com.PointD5D.Equals(object)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    int result = pointD5D.GetHashCode();
                };

                ExecuteTest(method, "Com.PointD5D.GetHashCode()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    string result = pointD5D.ToString();
                };

                ExecuteTest(method, "Com.PointD5D.ToString()");
            }

            // Equals

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = left;

                Action method = () =>
                {
                    bool result = left.Equals(right);
                };

                ExecuteTest(method, "Com.PointD5D.Equals(Com.PointD5D)");
            }

            // Offset

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD5D.Offset(d);
                };

                ExecuteTest(method, "Com.PointD5D.Offset(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double dx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dy = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dz = Com.Statistics.RandomDouble(-1E18, 1E18);
                double du = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dv = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD5D.Offset(dx, dy, dz, du, dv);
                };

                ExecuteTest(method, "Com.PointD5D.Offset(double, double, double, double, double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D pt = _GetRandomPointD5D();

                Action method = () =>
                {
                    pointD5D.Offset(pt);
                };

                ExecuteTest(method, "Com.PointD5D.Offset(Com.PointD5D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD5D result = pointD5D.OffsetCopy(d);
                };

                ExecuteTest(method, "Com.PointD5D.OffsetCopy(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double dx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dy = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dz = Com.Statistics.RandomDouble(-1E18, 1E18);
                double du = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dv = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD5D result = pointD5D.OffsetCopy(dx, dy, dz, du, dv);
                };

                ExecuteTest(method, "Com.PointD5D.OffsetCopy(double, double, double, double, double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D pt = _GetRandomPointD5D();

                Action method = () =>
                {
                    Com.PointD5D result = pointD5D.OffsetCopy(pt);
                };

                ExecuteTest(method, "Com.PointD5D.OffsetCopy(Com.PointD5D)");
            }

            // Scale

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD5D.Scale(s);
                };

                ExecuteTest(method, "Com.PointD5D.Scale(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double sx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sy = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sz = Com.Statistics.RandomDouble(-1E18, 1E18);
                double su = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sv = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD5D.Scale(sx, sy, sz, su, sv);
                };

                ExecuteTest(method, "Com.PointD5D.Scale(double, double, double, double, double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D pt = _GetRandomPointD5D();

                Action method = () =>
                {
                    pointD5D.Scale(pt);
                };

                ExecuteTest(method, "Com.PointD5D.Scale(Com.PointD5D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD5D result = pointD5D.ScaleCopy(s);
                };

                ExecuteTest(method, "Com.PointD5D.ScaleCopy(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double sx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sy = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sz = Com.Statistics.RandomDouble(-1E18, 1E18);
                double su = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sv = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD5D result = pointD5D.ScaleCopy(sx, sy, sz, su, sv);
                };

                ExecuteTest(method, "Com.PointD5D.ScaleCopy(double, double, double, double, double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D pt = _GetRandomPointD5D();

                Action method = () =>
                {
                    Com.PointD5D result = pointD5D.ScaleCopy(pt);
                };

                ExecuteTest(method, "Com.PointD5D.ScaleCopy(Com.PointD5D)");
            }

            // Rotate

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD5D.RotateXY(angle);
                };

                ExecuteTest(method, "Com.PointD5D.RotateXY(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD5D.RotateXZ(angle);
                };

                ExecuteTest(method, "Com.PointD5D.RotateXZ(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD5D.RotateXU(angle);
                };

                ExecuteTest(method, "Com.PointD5D.RotateXU(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD5D.RotateXV(angle);
                };

                ExecuteTest(method, "Com.PointD5D.RotateXV(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD5D.RotateYZ(angle);
                };

                ExecuteTest(method, "Com.PointD5D.RotateYZ(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD5D.RotateYU(angle);
                };

                ExecuteTest(method, "Com.PointD5D.RotateYU(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD5D.RotateYV(angle);
                };

                ExecuteTest(method, "Com.PointD5D.RotateYV(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD5D.RotateZU(angle);
                };

                ExecuteTest(method, "Com.PointD5D.RotateZU(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD5D.RotateZV(angle);
                };

                ExecuteTest(method, "Com.PointD5D.RotateZV(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD5D.RotateUV(angle);
                };

                ExecuteTest(method, "Com.PointD5D.RotateUV(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.PointD5D result = pointD5D.RotateXYCopy(angle);
                };

                ExecuteTest(method, "Com.PointD5D.RotateXYCopy(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.PointD5D result = pointD5D.RotateXZCopy(angle);
                };

                ExecuteTest(method, "Com.PointD5D.RotateXZCopy(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.PointD5D result = pointD5D.RotateXUCopy(angle);
                };

                ExecuteTest(method, "Com.PointD5D.RotateXUCopy(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.PointD5D result = pointD5D.RotateXVCopy(angle);
                };

                ExecuteTest(method, "Com.PointD5D.RotateXVCopy(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.PointD5D result = pointD5D.RotateYZCopy(angle);
                };

                ExecuteTest(method, "Com.PointD5D.RotateYZCopy(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.PointD5D result = pointD5D.RotateYUCopy(angle);
                };

                ExecuteTest(method, "Com.PointD5D.RotateYUCopy(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.PointD5D result = pointD5D.RotateYVCopy(angle);
                };

                ExecuteTest(method, "Com.PointD5D.RotateYVCopy(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.PointD5D result = pointD5D.RotateZUCopy(angle);
                };

                ExecuteTest(method, "Com.PointD5D.RotateZUCopy(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.PointD5D result = pointD5D.RotateZVCopy(angle);
                };

                ExecuteTest(method, "Com.PointD5D.RotateZVCopy(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.PointD5D result = pointD5D.RotateUVCopy(angle);
                };

                ExecuteTest(method, "Com.PointD5D.RotateUVCopy(double)");
            }

            // Affine

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D ex = _GetRandomPointD5D().VectorNormalize;
                Com.PointD5D ey = _GetRandomPointD5D().VectorNormalize;
                Com.PointD5D ez = _GetRandomPointD5D().VectorNormalize;
                Com.PointD5D eu = _GetRandomPointD5D().VectorNormalize;
                Com.PointD5D ev = _GetRandomPointD5D().VectorNormalize;
                Com.PointD5D offset = _GetRandomPointD5D();

                Action method = () =>
                {
                    pointD5D.AffineTransform(ex, ey, ez, eu, ev, offset);
                };

                ExecuteTest(method, "Com.PointD5D.AffineTransform(Com.PointD5D, Com.PointD5D, Com.PointD5D, Com.PointD5D, Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.Matrix matrixLeft = _GetRandomMatrix(6, 6);

                Action method = () =>
                {
                    pointD5D.AffineTransform(matrixLeft);
                };

                ExecuteTest(method, "Com.PointD5D.AffineTransform(Com.Matrix)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(6, 6));
                }

                Action method = () =>
                {
                    pointD5D.AffineTransform(matrixLeftList);
                };

                ExecuteTest(method, "Com.PointD5D.AffineTransform(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D ex = _GetRandomPointD5D().VectorNormalize;
                Com.PointD5D ey = _GetRandomPointD5D().VectorNormalize;
                Com.PointD5D ez = _GetRandomPointD5D().VectorNormalize;
                Com.PointD5D eu = _GetRandomPointD5D().VectorNormalize;
                Com.PointD5D ev = _GetRandomPointD5D().VectorNormalize;
                Com.PointD5D offset = _GetRandomPointD5D();

                Action method = () =>
                {
                    Com.PointD5D result = pointD5D.AffineTransformCopy(ex, ey, ez, eu, ev, offset);
                };

                ExecuteTest(method, "Com.PointD5D.AffineTransformCopy(Com.PointD5D, Com.PointD5D, Com.PointD5D, Com.PointD5D, Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.Matrix matrixLeft = _GetRandomMatrix(6, 6);

                Action method = () =>
                {
                    Com.PointD5D result = pointD5D.AffineTransformCopy(matrixLeft);
                };

                ExecuteTest(method, "Com.PointD5D.AffineTransformCopy(Com.Matrix)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(6, 6));
                }

                Action method = () =>
                {
                    Com.PointD5D result = pointD5D.AffineTransformCopy(matrixLeftList);
                };

                ExecuteTest(method, "Com.PointD5D.AffineTransformCopy(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D ex = _GetRandomPointD5D().VectorNormalize;
                Com.PointD5D ey = _GetRandomPointD5D().VectorNormalize;
                Com.PointD5D ez = _GetRandomPointD5D().VectorNormalize;
                Com.PointD5D eu = _GetRandomPointD5D().VectorNormalize;
                Com.PointD5D ev = _GetRandomPointD5D().VectorNormalize;
                Com.PointD5D offset = _GetRandomPointD5D();

                Action method = () =>
                {
                    pointD5D.InverseAffineTransform(ex, ey, ez, eu, ev, offset);
                };

                ExecuteTest(method, "Com.PointD5D.InverseAffineTransform(Com.PointD5D, Com.PointD5D, Com.PointD5D, Com.PointD5D, Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.Matrix matrixLeft = _GetRandomMatrix(6, 6);

                Action method = () =>
                {
                    pointD5D.InverseAffineTransform(matrixLeft);
                };

                ExecuteTest(method, "Com.PointD5D.InverseAffineTransform(Com.Matrix)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(6, 6));
                }

                Action method = () =>
                {
                    pointD5D.InverseAffineTransform(matrixLeftList);
                };

                ExecuteTest(method, "Com.PointD5D.InverseAffineTransform(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D ex = _GetRandomPointD5D().VectorNormalize;
                Com.PointD5D ey = _GetRandomPointD5D().VectorNormalize;
                Com.PointD5D ez = _GetRandomPointD5D().VectorNormalize;
                Com.PointD5D eu = _GetRandomPointD5D().VectorNormalize;
                Com.PointD5D ev = _GetRandomPointD5D().VectorNormalize;
                Com.PointD5D offset = _GetRandomPointD5D();

                Action method = () =>
                {
                    Com.PointD5D result = pointD5D.InverseAffineTransformCopy(ex, ey, ez, eu, ev, offset);
                };

                ExecuteTest(method, "Com.PointD5D.InverseAffineTransformCopy(Com.PointD5D, Com.PointD5D, Com.PointD5D, Com.PointD5D, Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.Matrix matrixLeft = _GetRandomMatrix(6, 6);

                Action method = () =>
                {
                    Com.PointD5D result = pointD5D.InverseAffineTransformCopy(matrixLeft);
                };

                ExecuteTest(method, "Com.PointD5D.InverseAffineTransformCopy(Com.Matrix)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(6, 6));
                }

                Action method = () =>
                {
                    Com.PointD5D result = pointD5D.InverseAffineTransformCopy(matrixLeftList);
                };

                ExecuteTest(method, "Com.PointD5D.InverseAffineTransformCopy(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            // Project

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D prjCenter = _GetRandomPointD5D();
                double trueLenDist = (pointD5D.V - prjCenter.V) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    Com.PointD4D result = pointD5D.ProjectToXYZU(prjCenter, trueLenDist);
                };

                ExecuteTest(method, "Com.PointD5D.ProjectToXYZU(Com.PointD5D, double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D prjCenter = _GetRandomPointD5D();
                double trueLenDist = (pointD5D.X - prjCenter.X) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    Com.PointD4D result = pointD5D.ProjectToYZUV(prjCenter, trueLenDist);
                };

                ExecuteTest(method, "Com.PointD5D.ProjectToYZUV(Com.PointD5D, double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D prjCenter = _GetRandomPointD5D();
                double trueLenDist = (pointD5D.Y - prjCenter.Y) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    Com.PointD4D result = pointD5D.ProjectToZUVX(prjCenter, trueLenDist);
                };

                ExecuteTest(method, "Com.PointD5D.ProjectToZUVX(Com.PointD5D, double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D prjCenter = _GetRandomPointD5D();
                double trueLenDist = (pointD5D.Z - prjCenter.Z) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    Com.PointD4D result = pointD5D.ProjectToUVXY(prjCenter, trueLenDist);
                };

                ExecuteTest(method, "Com.PointD5D.ProjectToUVXY(Com.PointD5D, double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D prjCenter = _GetRandomPointD5D();
                double trueLenDist = (pointD5D.U - prjCenter.U) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    Com.PointD4D result = pointD5D.ProjectToVXYZ(prjCenter, trueLenDist);
                };

                ExecuteTest(method, "Com.PointD5D.ProjectToVXYZ(Com.PointD5D, double)");
            }

            // 距离与角度

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D pt = _GetRandomPointD5D();

                Action method = () =>
                {
                    double result = pointD5D.DistanceFrom(pt);
                };

                ExecuteTest(method, "Com.PointD5D.DistanceFrom(Com.PointD5D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D pt = _GetRandomPointD5D();

                Action method = () =>
                {
                    double result = pointD5D.AngleFrom(pt);
                };

                ExecuteTest(method, "Com.PointD5D.AngleFrom(Com.PointD5D)");
            }

            // 坐标系转换

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    Com.PointD5D result = pointD5D.ToSpherical();
                };

                ExecuteTest(method, "Com.PointD5D.ToSpherical()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    Com.PointD5D result = pointD5D.ToCartesian();
                };

                ExecuteTest(method, "Com.PointD5D.ToCartesian()");
            }

            // ToVector

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    Com.Vector result = pointD5D.ToVector();
                };

                ExecuteTest(method, "Com.PointD5D.ToVector()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    Com.Vector result = pointD5D.ToVectorColumn();
                };

                ExecuteTest(method, "Com.PointD5D.ToVectorColumn()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    Com.Vector result = pointD5D.ToVectorRow();
                };

                ExecuteTest(method, "Com.PointD5D.ToVectorRow()");
            }

            // ToArray

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    double[] result = pointD5D.ToArray();
                };

                ExecuteTest(method, "Com.PointD5D.ToArray()");
            }
        }

        protected override void StaticMethod()
        {
            // Equals

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = left;

                Action method = () =>
                {
                    bool result = Com.PointD5D.Equals(left, right);
                };

                ExecuteTest(method, "Com.PointD5D.Equals(Com.PointD5D, Com.PointD5D)");
            }

            // Matrix

            {
                Action method = () =>
                {
                    Com.Matrix result = Com.PointD5D.IdentityMatrix();
                };

                ExecuteTest(method, "Com.PointD5D.IdentityMatrix()");
            }

            {
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD5D.OffsetMatrix(d);
                };

                ExecuteTest(method, "Com.PointD5D.OffsetMatrix(double)");
            }

            {
                double dx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dy = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dz = Com.Statistics.RandomDouble(-1E18, 1E18);
                double du = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dv = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD5D.OffsetMatrix(dx, dy, dz, du, dv);
                };

                ExecuteTest(method, "Com.PointD5D.OffsetMatrix(double, double, double, double, double)");
            }

            {
                Com.PointD5D pt = _GetRandomPointD5D();

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD5D.OffsetMatrix(pt);
                };

                ExecuteTest(method, "Com.PointD5D.OffsetMatrix(Com.PointD5D)");
            }

            {
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD5D.ScaleMatrix(s);
                };

                ExecuteTest(method, "Com.PointD5D.ScaleMatrix(double)");
            }

            {
                double sx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sy = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sz = Com.Statistics.RandomDouble(-1E18, 1E18);
                double su = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sv = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD5D.ScaleMatrix(sx, sy, sz, su, sv);
                };

                ExecuteTest(method, "Com.PointD5D.ScaleMatrix(double, double, double, double, double)");
            }

            {
                Com.PointD5D pt = _GetRandomPointD5D();

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD5D.ScaleMatrix(pt);
                };

                ExecuteTest(method, "Com.PointD5D.ScaleMatrix(Com.PointD5D)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD5D.RotateXYMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD5D.RotateXYMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD5D.RotateXZMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD5D.RotateXZMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD5D.RotateXUMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD5D.RotateXUMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD5D.RotateXVMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD5D.RotateXVMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD5D.RotateYZMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD5D.RotateYZMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD5D.RotateYUMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD5D.RotateYUMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD5D.RotateYVMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD5D.RotateYVMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD5D.RotateZUMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD5D.RotateZUMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD5D.RotateZVMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD5D.RotateZVMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD5D.RotateUVMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD5D.RotateUVMatrix(double)");
            }

            // 距离与角度

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    double result = Com.PointD5D.DistanceBetween(left, right);
                };

                ExecuteTest(method, "Com.PointD5D.DistanceBetween(Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    double result = Com.PointD5D.AngleBetween(left, right);
                };

                ExecuteTest(method, "Com.PointD5D.AngleBetween(Com.PointD5D, Com.PointD5D)");
            }

            // 向量乘积

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    double result = Com.PointD5D.DotProduct(left, right);
                };

                ExecuteTest(method, "Com.PointD5D.DotProduct(Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    Com.Vector result = Com.PointD5D.CrossProduct(left, right);
                };

                ExecuteTest(method, "Com.PointD5D.CrossProduct(Com.PointD5D, Com.PointD5D)");
            }

            // 初等函数

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    Com.PointD5D result = Com.PointD5D.Abs(pointD5D);
                };

                ExecuteTest(method, "Com.PointD5D.Abs(Com.PointD5D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    Com.PointD5D result = Com.PointD5D.Sign(pointD5D);
                };

                ExecuteTest(method, "Com.PointD5D.Sign(Com.PointD5D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    Com.PointD5D result = Com.PointD5D.Ceiling(pointD5D);
                };

                ExecuteTest(method, "Com.PointD5D.Ceiling(Com.PointD5D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    Com.PointD5D result = Com.PointD5D.Floor(pointD5D);
                };

                ExecuteTest(method, "Com.PointD5D.Floor(Com.PointD5D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    Com.PointD5D result = Com.PointD5D.Round(pointD5D);
                };

                ExecuteTest(method, "Com.PointD5D.Round(Com.PointD5D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    Com.PointD5D result = Com.PointD5D.Truncate(pointD5D);
                };

                ExecuteTest(method, "Com.PointD5D.Truncate(Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    Com.PointD5D result = Com.PointD5D.Max(left, right);
                };

                ExecuteTest(method, "Com.PointD5D.Max(Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    Com.PointD5D result = Com.PointD5D.Min(left, right);
                };

                ExecuteTest(method, "Com.PointD5D.Min(Com.PointD5D, Com.PointD5D)");
            }
        }

        protected override void Operator()
        {
            // 比较

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = left;

                Action method = () =>
                {
                    bool result = (left == right);
                };

                ExecuteTest(method, "Com.PointD5D.operator ==(Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = left;

                Action method = () =>
                {
                    bool result = (left != right);
                };

                ExecuteTest(method, "Com.PointD5D.operator !=(Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    bool result = (left < right);
                };

                ExecuteTest(method, "Com.PointD5D.operator <(Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    bool result = (left > right);
                };

                ExecuteTest(method, "Com.PointD5D.operator >(Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    bool result = (left <= right);
                };

                ExecuteTest(method, "Com.PointD5D.operator <=(Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    bool result = (left >= right);
                };

                ExecuteTest(method, "Com.PointD5D.operator >=(Com.PointD5D, Com.PointD5D)");
            }

            // 运算

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    Com.PointD5D result = +pointD5D;
                };

                ExecuteTest(method, "Com.PointD5D.operator +(Com.PointD5D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    Com.PointD5D result = -pointD5D;
                };

                ExecuteTest(method, "Com.PointD5D.operator -(Com.PointD5D)");
            }

            {
                Com.PointD5D pt = _GetRandomPointD5D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD5D result = pt + n;
                };

                ExecuteTest(method, "Com.PointD5D.operator +(Com.PointD5D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD5D pt = _GetRandomPointD5D();

                Action method = () =>
                {
                    Com.PointD5D result = n + pt;
                };

                ExecuteTest(method, "Com.PointD5D.operator +(double, Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    Com.PointD5D result = left + right;
                };

                ExecuteTest(method, "Com.PointD5D.operator +(Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D pt = _GetRandomPointD5D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD5D result = pt - n;
                };

                ExecuteTest(method, "Com.PointD5D.operator -(Com.PointD5D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD5D pt = _GetRandomPointD5D();

                Action method = () =>
                {
                    Com.PointD5D result = n - pt;
                };

                ExecuteTest(method, "Com.PointD5D.operator -(double, Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    Com.PointD5D result = left - right;
                };

                ExecuteTest(method, "Com.PointD5D.operator -(Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D pt = _GetRandomPointD5D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD5D result = pt * n;
                };

                ExecuteTest(method, "Com.PointD5D.operator *(Com.PointD5D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD5D pt = _GetRandomPointD5D();

                Action method = () =>
                {
                    Com.PointD5D result = n * pt;
                };

                ExecuteTest(method, "Com.PointD5D.operator *(double, Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    Com.PointD5D result = left * right;
                };

                ExecuteTest(method, "Com.PointD5D.operator *(Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D pt = _GetRandomPointD5D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD5D result = pt / n;
                };

                ExecuteTest(method, "Com.PointD5D.operator /(Com.PointD5D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD5D pt = _GetRandomPointD5D();

                Action method = () =>
                {
                    Com.PointD5D result = n / pt;
                };

                ExecuteTest(method, "Com.PointD5D.operator /(double, Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    Com.PointD5D result = left / right;
                };

                ExecuteTest(method, "Com.PointD5D.operator /(Com.PointD5D, Com.PointD5D)");
            }
        }
    }

    sealed class PointD6DTest : ClassPerformanceTestBase
    {
        private static Com.PointD6D _GetRandomPointD6D()
        {
            return new Com.PointD6D(Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18));
        }

        private static Com.PointD5D _GetRandomPointD5D()
        {
            return new Com.PointD5D(Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18));
        }

        private static Com.Matrix _GetRandomMatrix(int width, int height)
        {
            if (width > 0 && height > 0)
            {
                Com.Matrix matrix = new Com.Matrix(width, height);

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        matrix[x, y] = Com.Statistics.RandomDouble(-1E18, 1E18);
                    }
                }

                return matrix;
            }
            else
            {
                return Com.Matrix.NonMatrix;
            }
        }

        //

        protected override void Constructor()
        {
            {
                double x = Com.Statistics.RandomDouble(-1E18, 1E18);
                double y = Com.Statistics.RandomDouble(-1E18, 1E18);
                double z = Com.Statistics.RandomDouble(-1E18, 1E18);
                double u = Com.Statistics.RandomDouble(-1E18, 1E18);
                double v = Com.Statistics.RandomDouble(-1E18, 1E18);
                double w = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD6D pointD6D = new Com.PointD6D(x, y, z, u, v, w);
                };

                ExecuteTest(method, "Com.PointD6D.PointD6D(double, double, double, double, double, double)");
            }
        }

        protected override void Property()
        {
            // 索引器

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                int index = Com.Statistics.RandomInteger(6);

                Action method = () =>
                {
                    double result = pointD6D[index];
                };

                ExecuteTest(method, "Com.PointD6D.this[int].get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                int index = Com.Statistics.RandomInteger(6);
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD6D[index] = value;
                };

                ExecuteTest(method, "Com.PointD6D.this[int].set(double)");
            }

            // Is

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    bool result = pointD6D.IsEmpty;
                };

                ExecuteTest(method, "Com.PointD6D.IsEmpty.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    bool result = pointD6D.IsNaN;
                };

                ExecuteTest(method, "Com.PointD6D.IsNaN.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    bool result = pointD6D.IsInfinity;
                };

                ExecuteTest(method, "Com.PointD6D.IsInfinity.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    bool result = pointD6D.IsNaNOrInfinity;
                };

                ExecuteTest(method, "Com.PointD6D.IsNaNOrInfinity.get()");
            }

            // 分量

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    double result = pointD6D.X;
                };

                ExecuteTest(method, "Com.PointD6D.X.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD6D.X = value;
                };

                ExecuteTest(method, "Com.PointD6D.X.set(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    double result = pointD6D.Y;
                };

                ExecuteTest(method, "Com.PointD6D.Y.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD6D.Y = value;
                };

                ExecuteTest(method, "Com.PointD6D.Y.set(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    double result = pointD6D.Z;
                };

                ExecuteTest(method, "Com.PointD6D.Z.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD6D.Z = value;
                };

                ExecuteTest(method, "Com.PointD6D.Z.set(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    double result = pointD6D.U;
                };

                ExecuteTest(method, "Com.PointD6D.U.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD6D.U = value;
                };

                ExecuteTest(method, "Com.PointD6D.U.set(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    double result = pointD6D.V;
                };

                ExecuteTest(method, "Com.PointD6D.V.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD6D.V = value;
                };

                ExecuteTest(method, "Com.PointD6D.V.set(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    double result = pointD6D.W;
                };

                ExecuteTest(method, "Com.PointD6D.W.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD6D.W = value;
                };

                ExecuteTest(method, "Com.PointD6D.W.set(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.PointD5D result = pointD6D.XYZUV;
                };

                ExecuteTest(method, "Com.PointD6D.XYZUV.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD5D value = _GetRandomPointD5D();

                Action method = () =>
                {
                    pointD6D.XYZUV = value;
                };

                ExecuteTest(method, "Com.PointD6D.XYZUV.set(Com.PointD5D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.PointD5D result = pointD6D.YZUVW;
                };

                ExecuteTest(method, "Com.PointD6D.YZUVW.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD5D value = _GetRandomPointD5D();

                Action method = () =>
                {
                    pointD6D.YZUVW = value;
                };

                ExecuteTest(method, "Com.PointD6D.YZUVW.set(Com.PointD5D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.PointD5D result = pointD6D.ZUVWX;
                };

                ExecuteTest(method, "Com.PointD6D.ZUVWX.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD5D value = _GetRandomPointD5D();

                Action method = () =>
                {
                    pointD6D.ZUVWX = value;
                };

                ExecuteTest(method, "Com.PointD6D.ZUVWX.set(Com.PointD5D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.PointD5D result = pointD6D.UVWXY;
                };

                ExecuteTest(method, "Com.PointD6D.UVWXY.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD5D value = _GetRandomPointD5D();

                Action method = () =>
                {
                    pointD6D.UVWXY = value;
                };

                ExecuteTest(method, "Com.PointD6D.UVWXY.set(Com.PointD5D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.PointD5D result = pointD6D.VWXYZ;
                };

                ExecuteTest(method, "Com.PointD6D.VWXYZ.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD5D value = _GetRandomPointD5D();

                Action method = () =>
                {
                    pointD6D.VWXYZ = value;
                };

                ExecuteTest(method, "Com.PointD6D.VWXYZ.set(Com.PointD5D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.PointD5D result = pointD6D.WXYZU;
                };

                ExecuteTest(method, "Com.PointD6D.WXYZU.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD5D value = _GetRandomPointD5D();

                Action method = () =>
                {
                    pointD6D.WXYZU = value;
                };

                ExecuteTest(method, "Com.PointD6D.WXYZU.set(Com.PointD5D)");
            }

            // 角度

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    double result = pointD6D.AngleX;
                };

                ExecuteTest(method, "Com.PointD6D.AngleX.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    double result = pointD6D.AngleY;
                };

                ExecuteTest(method, "Com.PointD6D.AngleY.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    double result = pointD6D.AngleZ;
                };

                ExecuteTest(method, "Com.PointD6D.AngleZ.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    double result = pointD6D.AngleU;
                };

                ExecuteTest(method, "Com.PointD6D.AngleU.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    double result = pointD6D.AngleV;
                };

                ExecuteTest(method, "Com.PointD6D.AngleV.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    double result = pointD6D.AngleW;
                };

                ExecuteTest(method, "Com.PointD6D.AngleW.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    double result = pointD6D.AngleXYZUV;
                };

                ExecuteTest(method, "Com.PointD6D.AngleXYZUV.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    double result = pointD6D.AngleYZUVW;
                };

                ExecuteTest(method, "Com.PointD6D.AngleYZUVW.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    double result = pointD6D.AngleZUVWX;
                };

                ExecuteTest(method, "Com.PointD6D.AngleZUVWX.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    double result = pointD6D.AngleUVWXY;
                };

                ExecuteTest(method, "Com.PointD6D.AngleUVWXY.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    double result = pointD6D.AngleVWXYZ;
                };

                ExecuteTest(method, "Com.PointD6D.AngleVWXYZ.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    double result = pointD6D.AngleWXYZU;
                };

                ExecuteTest(method, "Com.PointD6D.AngleWXYZU.get()");
            }

            // 向量

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    double result = pointD6D.VectorModule;
                };

                ExecuteTest(method, "Com.PointD6D.VectorModule.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    double result = pointD6D.VectorModuleSquared;
                };

                ExecuteTest(method, "Com.PointD6D.VectorModuleSquared.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    double result = pointD6D.VectorAngleX;
                };

                ExecuteTest(method, "Com.PointD6D.VectorAngleX.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    double result = pointD6D.VectorAngleY;
                };

                ExecuteTest(method, "Com.PointD6D.VectorAngleY.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    double result = pointD6D.VectorAngleZ;
                };

                ExecuteTest(method, "Com.PointD6D.VectorAngleZ.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    double result = pointD6D.VectorAngleU;
                };

                ExecuteTest(method, "Com.PointD6D.VectorAngleU.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    double result = pointD6D.VectorAngleVW;
                };

                ExecuteTest(method, "Com.PointD6D.VectorAngleVW.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.PointD6D result = pointD6D.VectorNegate;
                };

                ExecuteTest(method, "Com.PointD6D.VectorNegate.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.PointD6D result = pointD6D.VectorNormalize;
                };

                ExecuteTest(method, "Com.PointD6D.VectorNormalize.get()");
            }
        }

        protected override void StaticProperty()
        {

        }

        protected override void Method()
        {
            // object

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                object obj = (object)pointD6D;

                Action method = () =>
                {
                    bool result = pointD6D.Equals(obj);
                };

                ExecuteTest(method, "Com.PointD6D.Equals(object)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    int result = pointD6D.GetHashCode();
                };

                ExecuteTest(method, "Com.PointD6D.GetHashCode()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    string result = pointD6D.ToString();
                };

                ExecuteTest(method, "Com.PointD6D.ToString()");
            }

            // Equals

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = left;

                Action method = () =>
                {
                    bool result = left.Equals(right);
                };

                ExecuteTest(method, "Com.PointD6D.Equals(Com.PointD6D)");
            }

            // Offset

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD6D.Offset(d);
                };

                ExecuteTest(method, "Com.PointD6D.Offset(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double dx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dy = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dz = Com.Statistics.RandomDouble(-1E18, 1E18);
                double du = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dv = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dw = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD6D.Offset(dx, dy, dz, du, dv, dw);
                };

                ExecuteTest(method, "Com.PointD6D.Offset(double, double, double, double, double, double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D pt = _GetRandomPointD6D();

                Action method = () =>
                {
                    pointD6D.Offset(pt);
                };

                ExecuteTest(method, "Com.PointD6D.Offset(Com.PointD6D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD6D result = pointD6D.OffsetCopy(d);
                };

                ExecuteTest(method, "Com.PointD6D.OffsetCopy(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double dx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dy = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dz = Com.Statistics.RandomDouble(-1E18, 1E18);
                double du = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dv = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dw = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD6D result = pointD6D.OffsetCopy(dx, dy, dz, du, dv, dw);
                };

                ExecuteTest(method, "Com.PointD6D.OffsetCopy(double, double, double, double, double, double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D pt = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.PointD6D result = pointD6D.OffsetCopy(pt);
                };

                ExecuteTest(method, "Com.PointD6D.OffsetCopy(Com.PointD6D)");
            }

            // Scale

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD6D.Scale(s);
                };

                ExecuteTest(method, "Com.PointD6D.Scale(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double sx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sy = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sz = Com.Statistics.RandomDouble(-1E18, 1E18);
                double su = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sv = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sw = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD6D.Scale(sx, sy, sz, su, sv, sw);
                };

                ExecuteTest(method, "Com.PointD6D.Scale(double, double, double, double, double, double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D pt = _GetRandomPointD6D();

                Action method = () =>
                {
                    pointD6D.Scale(pt);
                };

                ExecuteTest(method, "Com.PointD6D.Scale(Com.PointD6D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD6D result = pointD6D.ScaleCopy(s);
                };

                ExecuteTest(method, "Com.PointD6D.ScaleCopy(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double sx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sy = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sz = Com.Statistics.RandomDouble(-1E18, 1E18);
                double su = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sv = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sw = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD6D result = pointD6D.ScaleCopy(sx, sy, sz, su, sv, sw);
                };

                ExecuteTest(method, "Com.PointD6D.ScaleCopy(double, double, double, double, double, double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D pt = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.PointD6D result = pointD6D.ScaleCopy(pt);
                };

                ExecuteTest(method, "Com.PointD6D.ScaleCopy(Com.PointD6D)");
            }

            // Rotate

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD6D.RotateXY(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateXY(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD6D.RotateXZ(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateXZ(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD6D.RotateXU(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateXU(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD6D.RotateXV(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateXV(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD6D.RotateXW(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateXW(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD6D.RotateYZ(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateYZ(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD6D.RotateYU(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateYU(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD6D.RotateYV(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateYV(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD6D.RotateYW(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateYW(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD6D.RotateZU(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateZU(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD6D.RotateZV(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateZV(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD6D.RotateZW(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateZW(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD6D.RotateUV(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateUV(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD6D.RotateUW(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateUW(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD6D.RotateVW(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateVW(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.PointD6D result = pointD6D.RotateXYCopy(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateXYCopy(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.PointD6D result = pointD6D.RotateXZCopy(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateXZCopy(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.PointD6D result = pointD6D.RotateXUCopy(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateXUCopy(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.PointD6D result = pointD6D.RotateXVCopy(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateXVCopy(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.PointD6D result = pointD6D.RotateXWCopy(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateXWCopy(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.PointD6D result = pointD6D.RotateYZCopy(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateYZCopy(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.PointD6D result = pointD6D.RotateYUCopy(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateYUCopy(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.PointD6D result = pointD6D.RotateYVCopy(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateYVCopy(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.PointD6D result = pointD6D.RotateYWCopy(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateYWCopy(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.PointD6D result = pointD6D.RotateZUCopy(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateZUCopy(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.PointD6D result = pointD6D.RotateZVCopy(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateZVCopy(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.PointD6D result = pointD6D.RotateZWCopy(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateZWCopy(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.PointD6D result = pointD6D.RotateUVCopy(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateUVCopy(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.PointD6D result = pointD6D.RotateUWCopy(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateUWCopy(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.PointD6D result = pointD6D.RotateVWCopy(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateVWCopy(double)");
            }

            // Affine

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D ex = _GetRandomPointD6D().VectorNormalize;
                Com.PointD6D ey = _GetRandomPointD6D().VectorNormalize;
                Com.PointD6D ez = _GetRandomPointD6D().VectorNormalize;
                Com.PointD6D eu = _GetRandomPointD6D().VectorNormalize;
                Com.PointD6D ev = _GetRandomPointD6D().VectorNormalize;
                Com.PointD6D ew = _GetRandomPointD6D().VectorNormalize;
                Com.PointD6D offset = _GetRandomPointD6D();

                Action method = () =>
                {
                    pointD6D.AffineTransform(ex, ey, ez, eu, ev, ew, offset);
                };

                ExecuteTest(method, "Com.PointD6D.AffineTransform(Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.Matrix matrixLeft = _GetRandomMatrix(6, 6);

                Action method = () =>
                {
                    pointD6D.AffineTransform(matrixLeft);
                };

                ExecuteTest(method, "Com.PointD6D.AffineTransform(Com.Matrix)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(6, 6));
                }

                Action method = () =>
                {
                    pointD6D.AffineTransform(matrixLeftList);
                };

                ExecuteTest(method, "Com.PointD6D.AffineTransform(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D ex = _GetRandomPointD6D().VectorNormalize;
                Com.PointD6D ey = _GetRandomPointD6D().VectorNormalize;
                Com.PointD6D ez = _GetRandomPointD6D().VectorNormalize;
                Com.PointD6D eu = _GetRandomPointD6D().VectorNormalize;
                Com.PointD6D ev = _GetRandomPointD6D().VectorNormalize;
                Com.PointD6D ew = _GetRandomPointD6D().VectorNormalize;
                Com.PointD6D offset = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.PointD6D result = pointD6D.AffineTransformCopy(ex, ey, ez, eu, ev, ew, offset);
                };

                ExecuteTest(method, "Com.PointD6D.AffineTransformCopy(Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.Matrix matrixLeft = _GetRandomMatrix(6, 6);

                Action method = () =>
                {
                    Com.PointD6D result = pointD6D.AffineTransformCopy(matrixLeft);
                };

                ExecuteTest(method, "Com.PointD6D.AffineTransformCopy(Com.Matrix)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(6, 6));
                }

                Action method = () =>
                {
                    Com.PointD6D result = pointD6D.AffineTransformCopy(matrixLeftList);
                };

                ExecuteTest(method, "Com.PointD6D.AffineTransformCopy(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D ex = _GetRandomPointD6D().VectorNormalize;
                Com.PointD6D ey = _GetRandomPointD6D().VectorNormalize;
                Com.PointD6D ez = _GetRandomPointD6D().VectorNormalize;
                Com.PointD6D eu = _GetRandomPointD6D().VectorNormalize;
                Com.PointD6D ev = _GetRandomPointD6D().VectorNormalize;
                Com.PointD6D ew = _GetRandomPointD6D().VectorNormalize;
                Com.PointD6D offset = _GetRandomPointD6D();

                Action method = () =>
                {
                    pointD6D.InverseAffineTransform(ex, ey, ez, eu, ev, ew, offset);
                };

                ExecuteTest(method, "Com.PointD6D.InverseAffineTransform(Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.Matrix matrixLeft = _GetRandomMatrix(6, 6);

                Action method = () =>
                {
                    pointD6D.InverseAffineTransform(matrixLeft);
                };

                ExecuteTest(method, "Com.PointD6D.InverseAffineTransform(Com.Matrix)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(6, 6));
                }

                Action method = () =>
                {
                    pointD6D.InverseAffineTransform(matrixLeftList);
                };

                ExecuteTest(method, "Com.PointD6D.InverseAffineTransform(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D ex = _GetRandomPointD6D().VectorNormalize;
                Com.PointD6D ey = _GetRandomPointD6D().VectorNormalize;
                Com.PointD6D ez = _GetRandomPointD6D().VectorNormalize;
                Com.PointD6D eu = _GetRandomPointD6D().VectorNormalize;
                Com.PointD6D ev = _GetRandomPointD6D().VectorNormalize;
                Com.PointD6D ew = _GetRandomPointD6D().VectorNormalize;
                Com.PointD6D offset = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.PointD6D result = pointD6D.InverseAffineTransformCopy(ex, ey, ez, eu, ev, ew, offset);
                };

                ExecuteTest(method, "Com.PointD6D.InverseAffineTransformCopy(Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.Matrix matrixLeft = _GetRandomMatrix(6, 6);

                Action method = () =>
                {
                    Com.PointD6D result = pointD6D.InverseAffineTransformCopy(matrixLeft);
                };

                ExecuteTest(method, "Com.PointD6D.InverseAffineTransformCopy(Com.Matrix)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(6, 6));
                }

                Action method = () =>
                {
                    Com.PointD6D result = pointD6D.InverseAffineTransformCopy(matrixLeftList);
                };

                ExecuteTest(method, "Com.PointD6D.InverseAffineTransformCopy(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            // Project

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D prjCenter = _GetRandomPointD6D();
                double trueLenDist = (pointD6D.W - prjCenter.W) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    Com.PointD5D result = pointD6D.ProjectToXYZUV(prjCenter, trueLenDist);
                };

                ExecuteTest(method, "Com.PointD6D.ProjectToXYZUV(Com.PointD6D, double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D prjCenter = _GetRandomPointD6D();
                double trueLenDist = (pointD6D.X - prjCenter.X) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    Com.PointD5D result = pointD6D.ProjectToYZUVW(prjCenter, trueLenDist);
                };

                ExecuteTest(method, "Com.PointD6D.ProjectToYZUVW(Com.PointD6D, double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D prjCenter = _GetRandomPointD6D();
                double trueLenDist = (pointD6D.Y - prjCenter.Y) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    Com.PointD5D result = pointD6D.ProjectToZUVWX(prjCenter, trueLenDist);
                };

                ExecuteTest(method, "Com.PointD6D.ProjectToZUVWX(Com.PointD6D, double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D prjCenter = _GetRandomPointD6D();
                double trueLenDist = (pointD6D.Z - prjCenter.Z) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    Com.PointD5D result = pointD6D.ProjectToUVWXY(prjCenter, trueLenDist);
                };

                ExecuteTest(method, "Com.PointD6D.ProjectToUVWXY(Com.PointD6D, double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D prjCenter = _GetRandomPointD6D();
                double trueLenDist = (pointD6D.U - prjCenter.U) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    Com.PointD5D result = pointD6D.ProjectToVWXYZ(prjCenter, trueLenDist);
                };

                ExecuteTest(method, "Com.PointD6D.ProjectToVWXYZ(Com.PointD6D, double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D prjCenter = _GetRandomPointD6D();
                double trueLenDist = (pointD6D.V - prjCenter.V) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    Com.PointD5D result = pointD6D.ProjectToWXYZU(prjCenter, trueLenDist);
                };

                ExecuteTest(method, "Com.PointD6D.ProjectToWXYZU(Com.PointD6D, double)");
            }

            // 距离与角度

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D pt = _GetRandomPointD6D();

                Action method = () =>
                {
                    double result = pointD6D.DistanceFrom(pt);
                };

                ExecuteTest(method, "Com.PointD6D.DistanceFrom(Com.PointD6D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D pt = _GetRandomPointD6D();

                Action method = () =>
                {
                    double result = pointD6D.AngleFrom(pt);
                };

                ExecuteTest(method, "Com.PointD6D.AngleFrom(Com.PointD6D)");
            }

            // 坐标系转换

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.PointD6D result = pointD6D.ToSpherical();
                };

                ExecuteTest(method, "Com.PointD6D.ToSpherical()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.PointD6D result = pointD6D.ToCartesian();
                };

                ExecuteTest(method, "Com.PointD6D.ToCartesian()");
            }

            // ToVector

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.Vector result = pointD6D.ToVector();
                };

                ExecuteTest(method, "Com.PointD6D.ToVector()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.Vector result = pointD6D.ToVectorColumn();
                };

                ExecuteTest(method, "Com.PointD6D.ToVectorColumn()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.Vector result = pointD6D.ToVectorRow();
                };

                ExecuteTest(method, "Com.PointD6D.ToVectorRow()");
            }

            // ToArray

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    double[] result = pointD6D.ToArray();
                };

                ExecuteTest(method, "Com.PointD6D.ToArray()");
            }
        }

        protected override void StaticMethod()
        {
            // Equals

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = left;

                Action method = () =>
                {
                    bool result = Com.PointD6D.Equals(left, right);
                };

                ExecuteTest(method, "Com.PointD6D.Equals(Com.PointD6D, Com.PointD6D)");
            }

            // Matrix

            {
                Action method = () =>
                {
                    Com.Matrix result = Com.PointD6D.IdentityMatrix();
                };

                ExecuteTest(method, "Com.PointD6D.IdentityMatrix()");
            }

            {
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD6D.OffsetMatrix(d);
                };

                ExecuteTest(method, "Com.PointD6D.OffsetMatrix(double)");
            }

            {
                double dx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dy = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dz = Com.Statistics.RandomDouble(-1E18, 1E18);
                double du = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dv = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dw = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD6D.OffsetMatrix(dx, dy, dz, du, dv, dw);
                };

                ExecuteTest(method, "Com.PointD6D.OffsetMatrix(double, double, double, double, double, double)");
            }

            {
                Com.PointD6D pt = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD6D.OffsetMatrix(pt);
                };

                ExecuteTest(method, "Com.PointD6D.OffsetMatrix(Com.PointD6D)");
            }

            {
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD6D.ScaleMatrix(s);
                };

                ExecuteTest(method, "Com.PointD6D.ScaleMatrix(double)");
            }

            {
                double sx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sy = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sz = Com.Statistics.RandomDouble(-1E18, 1E18);
                double su = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sv = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sw = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD6D.ScaleMatrix(sx, sy, sz, su, sv, sw);
                };

                ExecuteTest(method, "Com.PointD6D.ScaleMatrix(double, double, double, double, double, double)");
            }

            {
                Com.PointD6D pt = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD6D.ScaleMatrix(pt);
                };

                ExecuteTest(method, "Com.PointD6D.ScaleMatrix(Com.PointD6D)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD6D.RotateXYMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateXYMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD6D.RotateXZMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateXZMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD6D.RotateXUMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateXUMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD6D.RotateXVMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateXVMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD6D.RotateXWMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateXWMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD6D.RotateYZMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateYZMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD6D.RotateYUMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateYUMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD6D.RotateYVMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateYVMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD6D.RotateYWMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateYWMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD6D.RotateZUMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateZUMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD6D.RotateZVMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateZVMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD6D.RotateZWMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateZWMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD6D.RotateUVMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateUVMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD6D.RotateUWMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateUWMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Matrix result = Com.PointD6D.RotateVWMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateVWMatrix(double)");
            }

            // 距离与角度

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    double result = Com.PointD6D.DistanceBetween(left, right);
                };

                ExecuteTest(method, "Com.PointD6D.DistanceBetween(Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    double result = Com.PointD6D.AngleBetween(left, right);
                };

                ExecuteTest(method, "Com.PointD6D.AngleBetween(Com.PointD6D, Com.PointD6D)");
            }

            // 向量乘积

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    double result = Com.PointD6D.DotProduct(left, right);
                };

                ExecuteTest(method, "Com.PointD6D.DotProduct(Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.Vector result = Com.PointD6D.CrossProduct(left, right);
                };

                ExecuteTest(method, "Com.PointD6D.CrossProduct(Com.PointD6D, Com.PointD6D)");
            }

            // 初等函数

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.PointD6D result = Com.PointD6D.Abs(pointD6D);
                };

                ExecuteTest(method, "Com.PointD6D.Abs(Com.PointD6D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.PointD6D result = Com.PointD6D.Sign(pointD6D);
                };

                ExecuteTest(method, "Com.PointD6D.Sign(Com.PointD6D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.PointD6D result = Com.PointD6D.Ceiling(pointD6D);
                };

                ExecuteTest(method, "Com.PointD6D.Ceiling(Com.PointD6D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.PointD6D result = Com.PointD6D.Floor(pointD6D);
                };

                ExecuteTest(method, "Com.PointD6D.Floor(Com.PointD6D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.PointD6D result = Com.PointD6D.Round(pointD6D);
                };

                ExecuteTest(method, "Com.PointD6D.Round(Com.PointD6D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.PointD6D result = Com.PointD6D.Truncate(pointD6D);
                };

                ExecuteTest(method, "Com.PointD6D.Truncate(Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.PointD6D result = Com.PointD6D.Max(left, right);
                };

                ExecuteTest(method, "Com.PointD6D.Max(Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.PointD6D result = Com.PointD6D.Min(left, right);
                };

                ExecuteTest(method, "Com.PointD6D.Min(Com.PointD6D, Com.PointD6D)");
            }
        }

        protected override void Operator()
        {
            // 比较

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = left;

                Action method = () =>
                {
                    bool result = (left == right);
                };

                ExecuteTest(method, "Com.PointD6D.operator ==(Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = left;

                Action method = () =>
                {
                    bool result = (left != right);
                };

                ExecuteTest(method, "Com.PointD6D.operator !=(Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    bool result = (left < right);
                };

                ExecuteTest(method, "Com.PointD6D.operator <(Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    bool result = (left > right);
                };

                ExecuteTest(method, "Com.PointD6D.operator >(Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    bool result = (left <= right);
                };

                ExecuteTest(method, "Com.PointD6D.operator <=(Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    bool result = (left >= right);
                };

                ExecuteTest(method, "Com.PointD6D.operator >=(Com.PointD6D, Com.PointD6D)");
            }

            // 运算

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.PointD6D result = +pointD6D;
                };

                ExecuteTest(method, "Com.PointD6D.operator +(Com.PointD6D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.PointD6D result = -pointD6D;
                };

                ExecuteTest(method, "Com.PointD6D.operator -(Com.PointD6D)");
            }

            {
                Com.PointD6D pt = _GetRandomPointD6D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD6D result = pt + n;
                };

                ExecuteTest(method, "Com.PointD6D.operator +(Com.PointD6D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD6D pt = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.PointD6D result = n + pt;
                };

                ExecuteTest(method, "Com.PointD6D.operator +(double, Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.PointD6D result = left + right;
                };

                ExecuteTest(method, "Com.PointD6D.operator +(Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D pt = _GetRandomPointD6D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD6D result = pt - n;
                };

                ExecuteTest(method, "Com.PointD6D.operator -(Com.PointD6D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD6D pt = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.PointD6D result = n - pt;
                };

                ExecuteTest(method, "Com.PointD6D.operator -(double, Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.PointD6D result = left - right;
                };

                ExecuteTest(method, "Com.PointD6D.operator -(Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D pt = _GetRandomPointD6D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD6D result = pt * n;
                };

                ExecuteTest(method, "Com.PointD6D.operator *(Com.PointD6D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD6D pt = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.PointD6D result = n * pt;
                };

                ExecuteTest(method, "Com.PointD6D.operator *(double, Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.PointD6D result = left * right;
                };

                ExecuteTest(method, "Com.PointD6D.operator *(Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D pt = _GetRandomPointD6D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.PointD6D result = pt / n;
                };

                ExecuteTest(method, "Com.PointD6D.operator /(Com.PointD6D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD6D pt = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.PointD6D result = n / pt;
                };

                ExecuteTest(method, "Com.PointD6D.operator /(double, Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    Com.PointD6D result = left / right;
                };

                ExecuteTest(method, "Com.PointD6D.operator /(Com.PointD6D, Com.PointD6D)");
            }
        }
    }

    sealed class StatisticsTest : ClassPerformanceTestBase
    {
        protected override void Constructor()
        {

        }

        protected override void Property()
        {

        }

        protected override void StaticProperty()
        {

        }

        protected override void Method()
        {

        }

        protected override void StaticMethod()
        {
            // RandomInteger

            {
                Action method = () =>
                {
                    int result = Com.Statistics.RandomInteger();
                };

                ExecuteTest(method, "Com.Statistics.RandomInteger()");
            }

            {
                int right = Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    int result = Com.Statistics.RandomInteger(right);
                };

                ExecuteTest(method, "Com.Statistics.RandomInteger(int)");
            }

            {
                int right = Com.Statistics.RandomInteger();
                int left = -Com.Statistics.RandomInteger(right / 2);

                Action method = () =>
                {
                    int result = Com.Statistics.RandomInteger(left, right);
                };

                ExecuteTest(method, "Com.Statistics.RandomInteger(int, int)");
            }

            // RandomDouble

            {
                Action method = () =>
                {
                    double result = Com.Statistics.RandomDouble();
                };

                ExecuteTest(method, "Com.Statistics.RandomDouble()");
            }

            {
                double right = Com.Statistics.RandomDouble(1E18);

                Action method = () =>
                {
                    double result = Com.Statistics.RandomDouble(right);
                };

                ExecuteTest(method, "Com.Statistics.RandomDouble(double)");
            }

            {
                double right = Com.Statistics.RandomDouble(1E18);
                double left = -Com.Statistics.RandomDouble(right / 2);

                Action method = () =>
                {
                    double result = Com.Statistics.RandomDouble(left, right);
                };

                ExecuteTest(method, "Com.Statistics.RandomDouble(double, double)");
            }

            // GaussDistribution，GaussRandom

            {
                double ev = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sd = Com.Statistics.RandomDouble(-1E18, 1E18);
                double x = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    double result = Com.Statistics.GaussDistributionProbabilityDensity(ev, sd, x);
                };

                ExecuteTest(method, "Com.Statistics.GaussDistributionProbabilityDensity(double, double, double)");
            }

            {
                Action method = () =>
                {
                    double result = Com.Statistics.GaussRandom();
                };

                ExecuteTest(method, "Com.Statistics.GaussRandom()");
            }

            ExecuteTest(null, "Com.Statistics.GaussRandom(double, double, double, double)");
        }

        protected override void Operator()
        {

        }
    }

    sealed class TextTest : ClassPerformanceTestBase
    {
        protected override void Constructor()
        {

        }

        protected override void Property()
        {

        }

        protected override void StaticProperty()
        {

        }

        protected override void Method()
        {

        }

        protected override void StaticMethod()
        {
            {
                string sourceString = "!\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
                string startString = sourceString.Substring(5, 5);
                string endString = sourceString.Substring(sourceString.Length - 10, 5);
                bool includeStartString = true;
                bool includeEndString = true;

                Action method = () =>
                {
                    string result = Com.Text.GetIntervalString(sourceString, startString, endString, includeStartString, includeEndString);
                };

                ExecuteTest(method, "Com.Text.GetIntervalString(string, string, string, bool, bool)");
            }

            {
                string str = "!\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
                Font font = new Font("微软雅黑", 42F, FontStyle.Regular, GraphicsUnit.Point, 134);
                int width = 1024;

                Action method = () =>
                {
                    string result = Com.Text.StringIntercept(str, font, width);
                };

                ExecuteTest(method, "Com.Text.StringIntercept(string, System.Drawing.Font, int)");
            }

            {
                string text = "!\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
                Font font = new Font("微软雅黑", 42F, FontStyle.Regular, GraphicsUnit.Point, 134);
                SizeF size = new SizeF(1024, 1024);

                Action method = () =>
                {
                    Font result = Com.Text.GetSuitableFont(text, font, size);
                };

                ExecuteTest(method, "Com.Text.GetSuitableFont(string, System.Drawing.Font, System.Drawing.SizeF)");
            }

            {
                DateTime dateTime = DateTime.Now;

                Action method = () =>
                {
                    long result = Com.Text.GetBinaryFromDateTime(dateTime);
                };

                ExecuteTest(method, "Com.Text.GetBinaryFromDateTime(System.DateTime)");
            }

            {
                TimeSpan timeSpan = TimeSpan.FromDays(1) + DateTime.Now.TimeOfDay;

                Action method = () =>
                {
                    string result = Com.Text.GetLongTimeStringFromTimeSpan(timeSpan);
                };

                ExecuteTest(method, "Com.Text.GetLongTimeStringFromTimeSpan(System.TimeSpan)");
            }

            {
                TimeSpan timeSpan = TimeSpan.FromDays(1) + DateTime.Now.TimeOfDay;

                Action method = () =>
                {
                    string result = Com.Text.GetTimeStringFromTimeSpan(timeSpan);
                };

                ExecuteTest(method, "Com.Text.GetTimeStringFromTimeSpan(System.TimeSpan)");
            }

            {
                double second = (TimeSpan.FromDays(1) + DateTime.Now.TimeOfDay).TotalSeconds;
                int significance = 6;

                Action method = () =>
                {
                    double result = Com.Text.GetStandardizationTimespanOfSecond(second, significance);
                };

                ExecuteTest(method, "Com.Text.GetStandardizationTimespanOfSecond(double, int)");
            }

            {
                double second = (TimeSpan.FromDays(1) + DateTime.Now.TimeOfDay).TotalSeconds;

                Action method = () =>
                {
                    string result = Com.Text.GetLargeTimespanStringOfSecond(second);
                };

                ExecuteTest(method, "Com.Text.GetLargeTimespanStringOfSecond(double)");
            }

            {
                double meter = Com.Statistics.RandomDouble(1E12);
                int significance = 6;

                Action method = () =>
                {
                    double result = Com.Text.GetStandardizationDistanceOfMeter(meter, significance);
                };

                ExecuteTest(method, "Com.Text.GetStandardizationDistanceOfMeter(double, int)");
            }

            {
                double meter = Com.Statistics.RandomDouble(1E12);

                Action method = () =>
                {
                    string result = Com.Text.GetLargeDistanceStringOfMeter(meter);
                };

                ExecuteTest(method, "Com.Text.GetLargeDistanceStringOfMeter(double)");
            }

            {
                double degree = Com.Statistics.RandomDouble(360);
                int decimalDigits = 3;
                bool cutdownIdleZeros = true;

                Action method = () =>
                {
                    string result = Com.Text.GetAngleStringOfDegree(degree, decimalDigits, cutdownIdleZeros);
                };

                ExecuteTest(method, "Com.Text.GetAngleStringOfDegree(double, int, bool)");
            }

            {
                long b = (long)Com.Statistics.RandomDouble(1E18);

                Action method = () =>
                {
                    string result = Com.Text.GetSize64StringFromByte(b);
                };

                ExecuteTest(method, "Com.Text.GetSize64StringFromByte(long)");
            }
        }

        protected override void Operator()
        {

        }
    }

    sealed class VectorTest : ClassPerformanceTestBase
    {
        private static Com.Vector _GetRandomVector(Com.Vector.Type type, int dimension)
        {
            if (dimension > 0)
            {
                Com.Vector vector = Com.Vector.Zero(type, dimension);

                for (int i = 0; i < dimension; i++)
                {
                    vector[i] = Com.Statistics.RandomDouble(-1E18, 1E18);
                }

                return vector;
            }
            else
            {
                return Com.Vector.NonVector;
            }
        }

        private static Com.Vector _GetRandomVector(int dimension)
        {
            if (dimension > 0)
            {
                Com.Vector vector = Com.Vector.Zero(dimension);

                for (int i = 0; i < dimension; i++)
                {
                    vector[i] = Com.Statistics.RandomDouble(-1E18, 1E18);
                }

                return vector;
            }
            else
            {
                return Com.Vector.NonVector;
            }
        }

        private static Com.Matrix _GetRandomMatrix(int width, int height)
        {
            if (width > 0 && height > 0)
            {
                Com.Matrix matrix = new Com.Matrix(width, height);

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        matrix[x, y] = Com.Statistics.RandomDouble(-1E18, 1E18);
                    }
                }

                return matrix;
            }
            else
            {
                return Com.Matrix.NonMatrix;
            }
        }

        //

        protected override void Constructor()
        {
            {
                Com.Vector.Type type = Com.Vector.Type.ColumnVector;
                double[] values = new double[32];

                for (int i = 0; i < 32; i++)
                {
                    values[i] = Com.Statistics.RandomDouble(-1E18, 1E18);
                }

                Action method = () =>
                {
                    Com.Vector vector = new Com.Vector(type, values);
                };

                ExecuteTest(method, "Com.Vector.Vector(Com.Vector.Type, params double[])", "dimension at 32");
            }

            {
                double[] values = new double[32];

                for (int i = 0; i < 32; i++)
                {
                    values[i] = Com.Statistics.RandomDouble(-1E18, 1E18);
                }

                Action method = () =>
                {
                    Com.Vector vector = new Com.Vector(values);
                };

                ExecuteTest(method, "Com.Vector.Vector(params double[])", "dimension at 32");
            }
        }

        protected override void Property()
        {
            // 索引器

            {
                Com.Vector vector = _GetRandomVector(32);
                int index = Com.Statistics.RandomInteger(32);

                Action method = () =>
                {
                    double result = vector[index];
                };

                ExecuteTest(method, "Com.Vector.this[int].get()", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);
                int index = Com.Statistics.RandomInteger(32);
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    vector[index] = value;
                };

                ExecuteTest(method, "Com.Vector.this[int].set(double)", "dimension at 32");
            }

            // Is

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    bool result = vector.IsNonVector;
                };

                ExecuteTest(method, "Com.Vector.IsNonVector.get()", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    bool result = vector.IsColumnVector;
                };

                ExecuteTest(method, "Com.Vector.IsColumnVector.get()", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    bool result = vector.IsRowVector;
                };

                ExecuteTest(method, "Com.Vector.IsRowVector.get()", "dimension at 32");
            }

            // Size

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    int result = vector.Dimension;
                };

                ExecuteTest(method, "Com.Vector.Dimension.get()", "dimension at 32");
            }

            // 向量

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    double result = vector.Module;
                };

                ExecuteTest(method, "Com.Vector.Module.get()", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    double result = vector.ModuleSquared;
                };

                ExecuteTest(method, "Com.Vector.ModuleSquared.get()", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    Com.Vector result = vector.Transport;
                };

                ExecuteTest(method, "Com.Vector.Transport.get()", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    Com.Vector result = vector.Negate;
                };

                ExecuteTest(method, "Com.Vector.Negate.get()", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    Com.Vector result = vector.Normalize;
                };

                ExecuteTest(method, "Com.Vector.Normalize.get()", "dimension at 32");
            }
        }

        protected override void StaticProperty()
        {
            {
                Action method = () =>
                {
                    Com.Vector result = Com.Vector.NonVector;
                };

                ExecuteTest(method, "Com.Vector.NonVector.get()", "dimension at 32");
            }
        }

        protected override void Method()
        {
            // object

            {
                Com.Vector vector = _GetRandomVector(32);
                object obj = (object)vector.Copy();

                Action method = () =>
                {
                    bool result = vector.Equals(obj);
                };

                ExecuteTest(method, "Com.Vector.Equals(object)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    int result = vector.GetHashCode();
                };

                ExecuteTest(method, "Com.Vector.GetHashCode()", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    string result = vector.ToString();
                };

                ExecuteTest(method, "Com.Vector.ToString()", "dimension at 32");
            }

            // Equals

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = left.Copy();

                Action method = () =>
                {
                    bool result = left.Equals(right);
                };

                ExecuteTest(method, "Com.Vector.Equals(Com.Vector)", "dimension at 32");
            }

            // Offset

            {
                Com.Vector vector = _GetRandomVector(32);
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    vector.Offset(d);
                };

                ExecuteTest(method, "Com.Vector.Offset(double)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);
                Com.Vector vector_d = _GetRandomVector(32);

                Action method = () =>
                {
                    vector.Offset(vector_d);
                };

                ExecuteTest(method, "Com.Vector.Offset(Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Vector result = vector.OffsetCopy(d);
                };

                ExecuteTest(method, "Com.Vector.OffsetCopy(double)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);
                Com.Vector vector_d = _GetRandomVector(32);

                Action method = () =>
                {
                    Com.Vector result = vector.OffsetCopy(vector_d);
                };

                ExecuteTest(method, "Com.Vector.OffsetCopy(Com.Vector)", "dimension at 32");
            }

            // Scale

            {
                Com.Vector vector = _GetRandomVector(32);
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    vector.Scale(d);
                };

                ExecuteTest(method, "Com.Vector.Scale(double)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);
                Com.Vector vector_d = _GetRandomVector(32);

                Action method = () =>
                {
                    vector.Scale(vector_d);
                };

                ExecuteTest(method, "Com.Vector.Scale(Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Vector result = vector.ScaleCopy(s);
                };

                ExecuteTest(method, "Com.Vector.ScaleCopy(double)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);
                Com.Vector vector_s = _GetRandomVector(32);

                Action method = () =>
                {
                    Com.Vector result = vector.ScaleCopy(vector_s);
                };

                ExecuteTest(method, "Com.Vector.ScaleCopy(Com.Vector)", "dimension at 32");
            }

            // Rotate

            {
                Com.Vector vector = _GetRandomVector(32);
                int index1 = Com.Statistics.RandomInteger(32);
                int index2 = Com.Statistics.RandomInteger(32);
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    vector.Rotate(index1, index2, angle);
                };

                ExecuteTest(method, "Com.Vector.Rotate(int, int, double)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);
                int index1 = Com.Statistics.RandomInteger(32);
                int index2 = Com.Statistics.RandomInteger(32);
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Vector result = vector.RotateCopy(index1, index2, angle);
                };

                ExecuteTest(method, "Com.Vector.RotateCopy(int, int, double)", "dimension at 32");
            }

            // Affine

            {
                Com.Vector vector = _GetRandomVector(8);
                Com.Matrix matrixLeft = _GetRandomMatrix(9, 9);

                Action method = () =>
                {
                    vector.AffineTransform(matrixLeft);
                };

                ExecuteTest(method, "Com.Vector.AffineTransform(Com.Matrix)", "dimension at 8");
            }

            {
                Com.Vector vector = _GetRandomVector(8);
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(9, 9));
                }

                Action method = () =>
                {
                    vector.AffineTransform(matrixLeftList);
                };

                ExecuteTest(method, "Com.Vector.AffineTransform(System.Collections.Generic.List<Com.Matrix>)", "dimension at 8, total 8 matrices");
            }

            {
                Com.Vector vector = _GetRandomVector(8);
                Com.Matrix matrixLeft = _GetRandomMatrix(9, 9);

                Action method = () =>
                {
                    Com.Vector result = vector.AffineTransformCopy(matrixLeft);
                };

                ExecuteTest(method, "Com.Vector.AffineTransformCopy(Com.Matrix)", "dimension at 8");
            }

            {
                Com.Vector vector = _GetRandomVector(8);
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(9, 9));
                }

                Action method = () =>
                {
                    Com.Vector result = vector.AffineTransformCopy(matrixLeftList);
                };

                ExecuteTest(method, "Com.Vector.AffineTransformCopy(System.Collections.Generic.List<Com.Matrix>)", "dimension at 8, total 8 matrices");
            }

            {
                Com.Vector vector = _GetRandomVector(8);
                Com.Matrix matrixLeft = _GetRandomMatrix(9, 9);

                Action method = () =>
                {
                    vector.InverseAffineTransform(matrixLeft);
                };

                ExecuteTest(method, "Com.Vector.InverseAffineTransform(Com.Matrix)", "dimension at 8");
            }

            {
                Com.Vector vector = _GetRandomVector(8);
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(9, 9));
                }

                Action method = () =>
                {
                    vector.InverseAffineTransform(matrixLeftList);
                };

                ExecuteTest(method, "Com.Vector.InverseAffineTransform(System.Collections.Generic.List<Com.Matrix>)", "dimension at 8, total 8 matrices");
            }

            {
                Com.Vector vector = _GetRandomVector(8);
                Com.Matrix matrixLeft = _GetRandomMatrix(9, 9);

                Action method = () =>
                {
                    Com.Vector result = vector.InverseAffineTransformCopy(matrixLeft);
                };

                ExecuteTest(method, "Com.Vector.InverseAffineTransformCopy(Com.Matrix)", "dimension at 8");
            }

            {
                Com.Vector vector = _GetRandomVector(8);
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(9, 9));
                }

                Action method = () =>
                {
                    Com.Vector result = vector.InverseAffineTransformCopy(matrixLeftList);
                };

                ExecuteTest(method, "Com.Vector.InverseAffineTransformCopy(System.Collections.Generic.List<Com.Matrix>)", "dimension at 8, total 8 matrices");
            }

            // 距离与角度

            {
                Com.Vector vector = _GetRandomVector(32);
                Com.Vector vector_d = _GetRandomVector(32);

                Action method = () =>
                {
                    double result = vector.DistanceFrom(vector_d);
                };

                ExecuteTest(method, "Com.Vector.DistanceFrom(Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);
                Com.Vector vector_a = _GetRandomVector(32);

                Action method = () =>
                {
                    double result = vector.AngleFrom(vector_a);
                };

                ExecuteTest(method, "Com.Vector.AngleFrom(Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);
                int index = Com.Statistics.RandomInteger(32);

                Action method = () =>
                {
                    double result = vector.AngleOfBasis(index);
                };

                ExecuteTest(method, "Com.Vector.AngleOfBasis(int)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);
                int index = Com.Statistics.RandomInteger(32);

                Action method = () =>
                {
                    double result = vector.AngleOfSpace(index);
                };

                ExecuteTest(method, "Com.Vector.AngleOfSpace(int)", "dimension at 32");
            }

            // 坐标系转换

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    Com.Vector result = vector.ToSpherical();
                };

                ExecuteTest(method, "Com.Vector.ToSpherical()", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    Com.Vector result = vector.ToCartesian();
                };

                ExecuteTest(method, "Com.Vector.ToCartesian()", "dimension at 32");
            }

            // ToMatrix

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    Com.Matrix result = vector.ToMatrix();
                };

                ExecuteTest(method, "Com.Vector.ToMatrix()", "dimension at 32");
            }

            // ToArray

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    double[] result = vector.ToArray();
                };

                ExecuteTest(method, "Com.Vector.ToArray()", "dimension at 32");
            }
        }

        protected override void StaticMethod()
        {
            // IsNullOrNonVector

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    bool result = Com.Vector.IsNullOrNonVector(vector);
                };

                ExecuteTest(method, "Com.Vector.IsNullOrNonVector(Com.Vector)", "dimension at 32");
            }

            // Equals

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = left.Copy();

                Action method = () =>
                {
                    bool result = Com.Vector.Equals(left, right);
                };

                ExecuteTest(method, "Com.Vector.Equals(Com.Vector, Com.Vector)", "dimension at 32");
            }

            // Zero，Basis

            {
                Com.Vector.Type type = Com.Vector.Type.ColumnVector;
                int dimension = 32;

                Action method = () =>
                {
                    Com.Vector result = Com.Vector.Zero(type, dimension);
                };

                ExecuteTest(method, "Com.Vector.Zero(Com.Vector.Type, int)", "dimension at 32");
            }

            {
                int dimension = 32;

                Action method = () =>
                {
                    Com.Vector result = Com.Vector.Zero(dimension);
                };

                ExecuteTest(method, "Com.Vector.Zero(int)", "dimension at 32");
            }

            {
                Com.Vector.Type type = Com.Vector.Type.ColumnVector;
                int dimension = 32;
                int index = Com.Statistics.RandomInteger(32);

                Action method = () =>
                {
                    Com.Vector result = Com.Vector.Basis(type, dimension, index);
                };

                ExecuteTest(method, "Com.Vector.Basis(Com.Vector.Type, int, int)", "dimension at 32");
            }

            {
                int dimension = 32;
                int index = Com.Statistics.RandomInteger(32);

                Action method = () =>
                {
                    Com.Vector result = Com.Vector.Basis(dimension, index);
                };

                ExecuteTest(method, "Com.Vector.Basis(int, int)", "dimension at 32");
            }

            // Matrix

            {
                Com.Vector.Type type = Com.Vector.Type.ColumnVector;
                int dimension = 32;
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Matrix result = Com.Vector.OffsetMatrix(type, dimension, d);
                };

                ExecuteTest(method, "Com.Vector.OffsetMatrix(Com.Vector.Type, int, double)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    Com.Matrix result = Com.Vector.OffsetMatrix(vector);
                };

                ExecuteTest(method, "Com.Vector.OffsetMatrix(Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector.Type type = Com.Vector.Type.ColumnVector;
                int dimension = 32;
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Matrix result = Com.Vector.ScaleMatrix(type, dimension, s);
                };

                ExecuteTest(method, "Com.Vector.ScaleMatrix(Com.Vector.Type, int, double)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    Com.Matrix result = Com.Vector.ScaleMatrix(vector);
                };

                ExecuteTest(method, "Com.Vector.ScaleMatrix(Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector.Type type = Com.Vector.Type.ColumnVector;
                int dimension = 32;
                int index1 = Com.Statistics.RandomInteger(16);
                int index2 = Com.Statistics.RandomInteger(16, 32);
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    Com.Matrix result = Com.Vector.RotateMatrix(type, dimension, index1, index2, angle);
                };

                ExecuteTest(method, "Com.Vector.RotateMatrix(Com.Vector.Type, int, int, int, double)", "dimension at 32");
            }

            // 距离与角度

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    double result = Com.Vector.DistanceBetween(left, right);
                };

                ExecuteTest(method, "Com.Vector.DistanceBetween(Com.Vector, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    double result = Com.Vector.AngleBetween(left, right);
                };

                ExecuteTest(method, "Com.Vector.AngleBetween(Com.Vector, Com.Vector)", "dimension at 32");
            }

            // 向量乘积

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    double result = Com.Vector.DotProduct(left, right);
                };

                ExecuteTest(method, "Com.Vector.DotProduct(Com.Vector, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    Com.Vector result = Com.Vector.CrossProduct(left, right);
                };

                ExecuteTest(method, "Com.Vector.CrossProduct(Com.Vector, Com.Vector)", "dimension at 32");
            }

            // 初等函数

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    Com.Vector result = Com.Vector.Abs(vector);
                };

                ExecuteTest(method, "Com.Vector.Abs(Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    Com.Vector result = Com.Vector.Sign(vector);
                };

                ExecuteTest(method, "Com.Vector.Sign(Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    Com.Vector result = Com.Vector.Ceiling(vector);
                };

                ExecuteTest(method, "Com.Vector.Ceiling(Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    Com.Vector result = Com.Vector.Floor(vector);
                };

                ExecuteTest(method, "Com.Vector.Floor(Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    Com.Vector result = Com.Vector.Round(vector);
                };

                ExecuteTest(method, "Com.Vector.Round(Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    Com.Vector result = Com.Vector.Truncate(vector);
                };

                ExecuteTest(method, "Com.Vector.Truncate(Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    Com.Vector result = Com.Vector.Max(left, right);
                };

                ExecuteTest(method, "Com.Vector.Max(Com.Vector, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    Com.Vector result = Com.Vector.Min(left, right);
                };

                ExecuteTest(method, "Com.Vector.Min(Com.Vector, Com.Vector)", "dimension at 32");
            }
        }

        protected override void Operator()
        {
            // 比较

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    bool result = (left == right);
                };

                ExecuteTest(method, "Com.Vector.operator ==(Com.Vector, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    bool result = (left != right);
                };

                ExecuteTest(method, "Com.Vector.operator !=(Com.Vector, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    bool result = (left < right);
                };

                ExecuteTest(method, "Com.Vector.operator <(Com.Vector, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    bool result = (left > right);
                };

                ExecuteTest(method, "Com.Vector.operator >(Com.Vector, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    bool result = (left <= right);
                };

                ExecuteTest(method, "Com.Vector.operator <=(Com.Vector, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    bool result = (left >= right);
                };

                ExecuteTest(method, "Com.Vector.operator >=(Com.Vector, Com.Vector)", "dimension at 32");
            }

            // 运算

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    Com.Vector result = +vector;
                };

                ExecuteTest(method, "Com.Vector.operator +(Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    Com.Vector result = -vector;
                };

                ExecuteTest(method, "Com.Vector.operator -(Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector pt = _GetRandomVector(32);
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Vector result = pt + n;
                };

                ExecuteTest(method, "Com.Vector.operator +(Com.Vector, double)", "dimension at 32");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.Vector pt = _GetRandomVector(32);

                Action method = () =>
                {
                    Com.Vector result = n + pt;
                };

                ExecuteTest(method, "Com.Vector.operator +(double, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    Com.Vector result = left + right;
                };

                ExecuteTest(method, "Com.Vector.operator +(Com.Vector, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector pt = _GetRandomVector(32);
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Vector result = pt - n;
                };

                ExecuteTest(method, "Com.Vector.operator -(Com.Vector, double)", "dimension at 32");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.Vector pt = _GetRandomVector(32);

                Action method = () =>
                {
                    Com.Vector result = n - pt;
                };

                ExecuteTest(method, "Com.Vector.operator -(double, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    Com.Vector result = left - right;
                };

                ExecuteTest(method, "Com.Vector.operator -(Com.Vector, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector pt = _GetRandomVector(32);
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Vector result = pt * n;
                };

                ExecuteTest(method, "Com.Vector.operator *(Com.Vector, double)", "dimension at 32");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.Vector pt = _GetRandomVector(32);

                Action method = () =>
                {
                    Com.Vector result = n * pt;
                };

                ExecuteTest(method, "Com.Vector.operator *(double, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    Com.Vector result = left * right;
                };

                ExecuteTest(method, "Com.Vector.operator *(Com.Vector, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector pt = _GetRandomVector(32);
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Vector result = pt / n;
                };

                ExecuteTest(method, "Com.Vector.operator /(Com.Vector, double)", "dimension at 32");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.Vector pt = _GetRandomVector(32);

                Action method = () =>
                {
                    Com.Vector result = n / pt;
                };

                ExecuteTest(method, "Com.Vector.operator /(double, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    Com.Vector result = left / right;
                };

                ExecuteTest(method, "Com.Vector.operator /(Com.Vector, Com.Vector)", "dimension at 32");
            }
        }
    }
}