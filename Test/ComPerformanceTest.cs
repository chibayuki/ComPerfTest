/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
Copyright © 2018 chibayuki@foxmail.com

Com性能测试
Version 19.4.13.0000

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
                string fileName = string.Concat("ComPerfTestLog_", (dt.Year % 100).ToString("D2"), dt.Month.ToString("D2"), dt.Day.ToString("D2"), dt.Hour.ToString("D2"), dt.Minute.ToString("D2"), dt.Second.ToString("D2"), dt.Millisecond.ToString("D3"), ".csv");

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

        public static void Clear() // 清除测试结果
        {
            _ResultList.Clear();

            Console.Clear();
        }
    }

    static class TestProgress // 测试进度
    {
        private const int _TotalMemberCount = 1703; // 成员总数量
        private static int _CompletedMemberCount = 0; // 已测试成员数量

        private static int _FullWidth => Math.Max(10, Math.Min(Console.WindowWidth * 3 / 4, 100)); // 进度条宽度

        //

        public static void Report(int delta) // 报告测试进度
        {
            _CompletedMemberCount += delta;

            double progress = (double)_CompletedMemberCount / _TotalMemberCount;

            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("Executing test (" + _CompletedMemberCount + " of " + _TotalMemberCount + ")");
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
            Console.Write((Math.Floor(progress * 1000) / 10) + "% completed");
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
            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("Executing test (0 of " + _TotalMemberCount + ")");
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

        public static void ClearExtra() // 清除额外输出
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
        private const int _MSOfPerMember =
#if DEBUG
            1
#else
            500
#endif
            ; // 被测试类每个成员的最短执行时长的毫秒数

        //

        protected static void ExecuteTest(Action method, string memberName, string comment) // 执行测试
        {
            string result = string.Empty;

            if (memberName == null)
            {
                memberName = string.Empty;
            }

            if (comment == null)
            {
                comment = string.Empty;
            }

            if (method == null)
            {
                result = string.Concat("[", memberName.Replace(',', ';'), "], Untested", (comment.Length > 0 ? ", " + comment.Replace(',', ';') : string.Empty));
            }
            else
            {
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

                result = string.Concat("[", memberName.Replace(',', ';'), "], ", Com.Text.GetScientificNotationString(msPerCycle / 1000, 4, true, true, "s").Replace('μ', 'u'), ", ", Com.Text.GetScientificNotationString(1000 / msPerCycle, 4, true, true, "Hz").Replace('μ', 'u'), (comment.Length > 0 ? ", " + comment.Replace(',', ';') : string.Empty));
            }

            TestResult.Log(result);

            TestProgress.Report(1);

            TestProgress.ClearExtra();

            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.Write("Latest result: ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(result);
        }

        protected static void ExecuteTest(Action method, string memberName) // 执行测试
        {
            ExecuteTest(method, memberName, string.Empty);
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
                    _ = Com.BitOperation.GetBinary8WithSingleBit1(bit);
                };

                ExecuteTest(method, "Com.BitOperation.GetBinary8WithSingleBit1(int)");
            }

            {
                int bit = Com.Statistics.RandomInteger() % 8;

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBinary8WithSingleBit0(bit);
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
                    _ = Com.BitOperation.BinaryHasBit(bin, bit);
                };

                ExecuteTest(method, "Com.BitOperation.BinaryHasBit(byte, int)");
            }

            {
                byte bin = (byte)(Com.Statistics.RandomInteger() % 255);

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBit1CountOfBinary(bin);
                };

                ExecuteTest(method, "Com.BitOperation.GetBit1CountOfBinary(byte)");
            }

            {
                byte bin = (byte)(Com.Statistics.RandomInteger() % 255);

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBit0CountOfBinary(bin);
                };

                ExecuteTest(method, "Com.BitOperation.GetBit0CountOfBinary(byte)");
            }

            {
                byte bin = (byte)(Com.Statistics.RandomInteger() % 255);

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBit1IndexOfBinary(bin);
                };

                ExecuteTest(method, "Com.BitOperation.GetBit1IndexOfBinary(byte)");
            }

            {
                byte bin = (byte)(Com.Statistics.RandomInteger() % 255);

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBit0IndexOfBinary(bin);
                };

                ExecuteTest(method, "Com.BitOperation.GetBit0IndexOfBinary(byte)");
            }

            // 16 位

            {
                int bit = Com.Statistics.RandomInteger() % 16;

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBinary16WithSingleBit1(bit);
                };

                ExecuteTest(method, "Com.BitOperation.GetBinary16WithSingleBit1(int)");
            }

            {
                int bit = Com.Statistics.RandomInteger() % 16;

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBinary16WithSingleBit0(bit);
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
                    _ = Com.BitOperation.BinaryHasBit(bin, bit);
                };

                ExecuteTest(method, "Com.BitOperation.BinaryHasBit(ushort, int)");
            }

            {
                ushort bin = (ushort)Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBit1CountOfBinary(bin);
                };

                ExecuteTest(method, "Com.BitOperation.GetBit1CountOfBinary(ushort)");
            }

            {
                ushort bin = (ushort)Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBit0CountOfBinary(bin);
                };

                ExecuteTest(method, "Com.BitOperation.GetBit0CountOfBinary(ushort)");
            }

            {
                ushort bin = (ushort)Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBit1IndexOfBinary(bin);
                };

                ExecuteTest(method, "Com.BitOperation.GetBit1IndexOfBinary(ushort)");
            }

            {
                ushort bin = (ushort)Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBit0IndexOfBinary(bin);
                };

                ExecuteTest(method, "Com.BitOperation.GetBit0IndexOfBinary(ushort)");
            }

            // 32 位

            {
                int bit = Com.Statistics.RandomInteger() % 32;

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBinary32WithSingleBit1(bit);
                };

                ExecuteTest(method, "Com.BitOperation.GetBinary32WithSingleBit1(int)");
            }

            {
                int bit = Com.Statistics.RandomInteger() % 32;

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBinary32WithSingleBit0(bit);
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
                    _ = Com.BitOperation.BinaryHasBit(bin, bit);
                };

                ExecuteTest(method, "Com.BitOperation.BinaryHasBit(uint, int)");
            }

            {
                uint bin = (uint)Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBit1CountOfBinary(bin);
                };

                ExecuteTest(method, "Com.BitOperation.GetBit1CountOfBinary(uint)");
            }

            {
                uint bin = (uint)Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBit0CountOfBinary(bin);
                };

                ExecuteTest(method, "Com.BitOperation.GetBit0CountOfBinary(uint)");
            }

            {
                uint bin = (uint)Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBit1IndexOfBinary(bin);
                };

                ExecuteTest(method, "Com.BitOperation.GetBit1IndexOfBinary(uint)");
            }

            {
                uint bin = (uint)Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBit0IndexOfBinary(bin);
                };

                ExecuteTest(method, "Com.BitOperation.GetBit0IndexOfBinary(uint)");
            }

            // 64 位

            {
                int bit = Com.Statistics.RandomInteger() % 64;

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBinary64WithSingleBit1(bit);
                };

                ExecuteTest(method, "Com.BitOperation.GetBinary64WithSingleBit1(int)");
            }

            {
                int bit = Com.Statistics.RandomInteger() % 64;

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBinary64WithSingleBit0(bit);
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
                    _ = Com.BitOperation.BinaryHasBit(bin, bit);
                };

                ExecuteTest(method, "Com.BitOperation.BinaryHasBit(ulong, int)");
            }

            {
                ulong bin = (ulong)Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBit1CountOfBinary(bin);
                };

                ExecuteTest(method, "Com.BitOperation.GetBit1CountOfBinary(ulong)");
            }

            {
                ulong bin = (ulong)Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBit0CountOfBinary(bin);
                };

                ExecuteTest(method, "Com.BitOperation.GetBit0CountOfBinary(ulong)");
            }

            {
                ulong bin = (ulong)Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBit1IndexOfBinary(bin);
                };

                ExecuteTest(method, "Com.BitOperation.GetBit1IndexOfBinary(ulong)");
            }

            {
                ulong bin = (ulong)Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBit0IndexOfBinary(bin);
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
        private static Com.BitSet _GetRandomBitSet(int size)
        {
            if (size > 0)
            {
                bool[] array = new bool[size];

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
                    _ = new Com.BitSet(length);
                };

                ExecuteTest(method, "Com.BitSet.BitSet(int)", "size at 1024 bits");
            }

            {
                int length = 1024;
                bool bitValue = true;

                Action method = () =>
                {
                    _ = new Com.BitSet(length, bitValue);
                };

                ExecuteTest(method, "Com.BitSet.BitSet(int, bool)", "size at 1024 bits");
            }

            {
                bool[] values = new bool[1024];

                for (int i = 0; i < values.Length; i++)
                {
                    if (Com.Statistics.RandomInteger() % 2 == 0)
                    {
                        values[i] = true;
                    }
                }

                Action method = () =>
                {
                    _ = new Com.BitSet(values);
                };

                ExecuteTest(method, "Com.BitSet.BitSet(bool[])", "size at 1024 bits");
            }

            {
                byte[] values = new byte[1024];

                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = (byte)Com.Statistics.RandomInteger(byte.MaxValue);
                }

                Action method = () =>
                {
                    _ = new Com.BitSet(values);
                };

                ExecuteTest(method, "Com.BitSet.BitSet(byte[])", "size at 1024 bits");
            }

            {
                ushort[] values = new ushort[1024];

                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = (ushort)Com.Statistics.RandomInteger(ushort.MaxValue);
                }

                Action method = () =>
                {
                    _ = new Com.BitSet(values);
                };

                ExecuteTest(method, "Com.BitSet.BitSet(ushort[])", "size at 1024 bits");
            }

            {
                uint[] values = new uint[1024];

                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = (uint)(((long)Com.Statistics.RandomInteger() * Com.Statistics.RandomInteger()) % uint.MaxValue);
                }

                Action method = () =>
                {
                    _ = new Com.BitSet(values);
                };

                ExecuteTest(method, "Com.BitSet.BitSet(uint[])", "size at 1024 bits");
            }

            {
                ulong[] values = new ulong[1024];

                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = (((ulong)Com.Statistics.RandomInteger()) << 32) + (ulong)Com.Statistics.RandomInteger();
                }

                Action method = () =>
                {
                    _ = new Com.BitSet(values);
                };

                ExecuteTest(method, "Com.BitSet.BitSet(ulong[])", "size at 1024 bits");
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
                    _ = bitSet[index];
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
                    _ = bitSet.IsEmpty;
                };

                ExecuteTest(method, "Com.BitSet.IsEmpty.get()", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = new Com.BitSet(1024);

                Action method = () =>
                {
                    _ = bitSet.IsReadOnly;
                };

                ExecuteTest(method, "Com.BitSet.IsReadOnly.get()", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = new Com.BitSet(1024);

                Action method = () =>
                {
                    _ = bitSet.IsFixedSize;
                };

                ExecuteTest(method, "Com.BitSet.IsFixedSize.get()", "size at 1024 bits");
            }

            // Size

            {
                Com.BitSet bitSet = new Com.BitSet(1024);

                Action method = () =>
                {
                    _ = bitSet.Size;
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
                    _ = bitSet.Count;
                };

                ExecuteTest(method, "Com.BitSet.Count.get()", "size at 1024 bits");
            }

            // Capacity

            {
                Com.BitSet bitSet = new Com.BitSet(1024);

                Action method = () =>
                {
                    _ = bitSet.Capacity;
                };

                ExecuteTest(method, "Com.BitSet.Capacity.get()", "size at 1024 bits");
            }
        }

        protected override void StaticProperty()
        {
            // Empty

            {
                Action method = () =>
                {
                    _ = Com.BitSet.Empty;
                };

                ExecuteTest(method, "Com.BitSet.Empty.get()");
            }
        }

        protected override void Method()
        {
            // object

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);
                object obj = bitSet.Copy();

                Action method = () =>
                {
                    _ = bitSet.Equals(obj);
                };

                ExecuteTest(method, "Com.BitSet.Equals(object)", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    _ = bitSet.GetHashCode();
                };

                ExecuteTest(method, "Com.BitSet.GetHashCode()", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    _ = bitSet.ToString();
                };

                ExecuteTest(method, "Com.BitSet.ToString()", "size at 1024 bits");
            }

            // Equals

            {
                Com.BitSet left = _GetRandomBitSet(1024);
                Com.BitSet right = left.Copy();

                Action method = () =>
                {
                    _ = left.Equals(right);
                };

                ExecuteTest(method, "Com.BitSet.Equals(Com.BitSet)", "size at 1024 bits");
            }

            // CompareTo

            {
                Com.BitSet left = _GetRandomBitSet(1024);
                object right = left.Copy();

                Action method = () =>
                {
                    _ = left.CompareTo(right);
                };

                ExecuteTest(method, "Com.BitSet.CompareTo(object)", "size at 1024 bits");
            }

            {
                Com.BitSet left = _GetRandomBitSet(1024);
                Com.BitSet right = left.Copy();

                Action method = () =>
                {
                    _ = left.CompareTo(right);
                };

                ExecuteTest(method, "Com.BitSet.CompareTo(Com.BitSet)", "size at 1024 bits");
            }

            // Copy

            {
                Com.BitSet bitSet = new Com.BitSet(1024);

                Action method = () =>
                {
                    _ = bitSet.Copy();
                };

                ExecuteTest(method, "Com.BitSet.Copy()", "size at 1024 bits");
            }

            // 检索

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);
                bool item1 = true;
                bool item2 = false;

                Action method = () =>
                {
                    int result1 = bitSet.IndexOf(item1);
                    int result2 = bitSet.IndexOf(item2);
                };

                ExecuteTest(method, "Com.BitSet.IndexOf(bool)", "size at 1024 bits, search for both true and false bit");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);
                bool item1 = true;
                bool item2 = false;
                int startIndex = 0;

                Action method = () =>
                {
                    int result1 = bitSet.IndexOf(item1, startIndex);
                    int result2 = bitSet.IndexOf(item2, startIndex);
                };

                ExecuteTest(method, "Com.BitSet.IndexOf(bool, int)", "size at 1024 bits, search for both true and false bit");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);
                bool item1 = true;
                bool item2 = false;
                int startIndex = 0;
                int count = 1024;

                Action method = () =>
                {
                    int result1 = bitSet.IndexOf(item1, startIndex, count);
                    int result2 = bitSet.IndexOf(item2, startIndex, count);
                };

                ExecuteTest(method, "Com.BitSet.IndexOf(bool, int, int)", "size at 1024 bits, search for both true and false bit");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);
                bool item1 = true;
                bool item2 = false;

                Action method = () =>
                {
                    int result1 = bitSet.LastIndexOf(item1);
                    int result2 = bitSet.LastIndexOf(item2);
                };

                ExecuteTest(method, "Com.BitSet.LastIndexOf(bool)", "size at 1024 bits, search for both true and false bit");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);
                bool item1 = true;
                bool item2 = false;
                int startIndex = 1023;

                Action method = () =>
                {
                    int result1 = bitSet.LastIndexOf(item1, startIndex);
                    int result2 = bitSet.LastIndexOf(item2, startIndex);
                };

                ExecuteTest(method, "Com.BitSet.LastIndexOf(bool, int)", "size at 1024 bits, search for both true and false bit");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);
                bool item1 = true;
                bool item2 = false;
                int startIndex = 1023;
                int count = 1024;

                Action method = () =>
                {
                    int result1 = bitSet.LastIndexOf(item1, startIndex, count);
                    int result2 = bitSet.LastIndexOf(item2, startIndex, count);
                };

                ExecuteTest(method, "Com.BitSet.LastIndexOf(bool, int, int)", "size at 1024 bits, search for both true and false bit");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);
                bool item1 = true;
                bool item2 = false;

                Action method = () =>
                {
                    bool result1 = bitSet.Contains(item1);
                    bool result2 = bitSet.Contains(item2);
                };

                ExecuteTest(method, "Com.BitSet.Contains(bool)", "size at 1024 bits, search for both true and false bit");
            }

            // ToArray，ToList

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    _ = bitSet.ToArray();
                };

                ExecuteTest(method, "Com.BitSet.ToArray()", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    _ = bitSet.ToList();
                };

                ExecuteTest(method, "Com.BitSet.ToList()", "size at 1024 bits");
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
                    _ = bitSet.Get(index);
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
                    _ = bitSet.TrueBitCount();
                };

                ExecuteTest(method, "Com.BitSet.TrueBitCount()", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    _ = bitSet.FalseBitCount();
                };

                ExecuteTest(method, "Com.BitSet.FalseBitCount()", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    _ = bitSet.TrueBitIndex();
                };

                ExecuteTest(method, "Com.BitSet.TrueBitIndex()", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    _ = bitSet.FalseBitIndex();
                };

                ExecuteTest(method, "Com.BitSet.FalseBitIndex()", "size at 1024 bits");
            }

            // 位运算

            {
                Com.BitSet left = _GetRandomBitSet(1024);
                Com.BitSet right = left.Not();

                Action method = () =>
                {
                    _ = left.And(right);
                };

                ExecuteTest(method, "Com.BitSet.And(Com.BitSet)", "size at 1024 bits");
            }

            {
                Com.BitSet left = _GetRandomBitSet(1024);
                Com.BitSet right = left.Not();

                Action method = () =>
                {
                    _ = left.Or(right);
                };

                ExecuteTest(method, "Com.BitSet.Or(Com.BitSet)", "size at 1024 bits");
            }

            {
                Com.BitSet left = _GetRandomBitSet(1024);
                Com.BitSet right = left.Not();

                Action method = () =>
                {
                    _ = left.Xor(right);
                };

                ExecuteTest(method, "Com.BitSet.Xor(Com.BitSet)", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    _ = bitSet.Not();
                };

                ExecuteTest(method, "Com.BitSet.Not()", "size at 1024 bits");
            }

            // 字符串

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    _ = bitSet.ToBinaryString();
                };

                ExecuteTest(method, "Com.BitSet.ToBinaryString()", "size at 1024 bits");
            }
        }

        protected override void StaticMethod()
        {
            // IsNullOrEmpty

            {
                Com.BitSet bitSet = new Com.BitSet(1024);

                Action method = () =>
                {
                    _ = Com.BitSet.IsNullOrEmpty(bitSet);
                };

                ExecuteTest(method, "Com.BitSet.IsNullOrEmpty(Com.BitSet)", "size at 1024 bits");
            }

            // Equals

            {
                Com.BitSet left = _GetRandomBitSet(1024);
                Com.BitSet right = left.Copy();

                Action method = () =>
                {
                    _ = Com.BitSet.Equals(left, right);
                };

                ExecuteTest(method, "Com.BitSet.Equals(Com.BitSet, Com.BitSet)", "size at 1024 bits");
            }

            // Compare

            {
                Com.BitSet left = _GetRandomBitSet(1024);
                Com.BitSet right = left.Copy();

                Action method = () =>
                {
                    _ = Com.BitSet.Compare(left, right);
                };

                ExecuteTest(method, "Com.BitSet.Compare(Com.BitSet, Com.BitSet)", "size at 1024 bits");
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
                    _ = (left == right);
                };

                ExecuteTest(method, "Com.BitSet.operator ==(Com.BitSet, Com.BitSet)", "size at 1024 bits");
            }

            {
                Com.BitSet left = _GetRandomBitSet(1024);
                Com.BitSet right = left.Copy();

                Action method = () =>
                {
                    _ = (left != right);
                };

                ExecuteTest(method, "Com.BitSet.operator !=(Com.BitSet, Com.BitSet)", "size at 1024 bits");
            }

            {
                Com.BitSet left = _GetRandomBitSet(1024);
                Com.BitSet right = left.Copy();

                Action method = () =>
                {
                    _ = (left < right);
                };

                ExecuteTest(method, "Com.BitSet.operator <(Com.BitSet, Com.BitSet)", "size at 1024 bits");
            }

            {
                Com.BitSet left = _GetRandomBitSet(1024);
                Com.BitSet right = left.Copy();

                Action method = () =>
                {
                    _ = (left > right);
                };

                ExecuteTest(method, "Com.BitSet.operator >(Com.BitSet, Com.BitSet)", "size at 1024 bits");
            }

            {
                Com.BitSet left = _GetRandomBitSet(1024);
                Com.BitSet right = left.Copy();

                Action method = () =>
                {
                    _ = (left <= right);
                };

                ExecuteTest(method, "Com.BitSet.operator <=(Com.BitSet, Com.BitSet)", "size at 1024 bits");
            }

            {
                Com.BitSet left = _GetRandomBitSet(1024);
                Com.BitSet right = left.Copy();

                Action method = () =>
                {
                    _ = (left >= right);
                };

                ExecuteTest(method, "Com.BitSet.operator >=(Com.BitSet, Com.BitSet)", "size at 1024 bits");
            }

            // 运算

            {
                Com.BitSet left = _GetRandomBitSet(1024);
                Com.BitSet right = left.Copy();

                Action method = () =>
                {
                    _ = left & right;
                };

                ExecuteTest(method, "Com.BitSet.operator &(Com.BitSet, Com.BitSet)", "size at 1024 bits");
            }

            {
                Com.BitSet left = _GetRandomBitSet(1024);
                Com.BitSet right = left.Copy();

                Action method = () =>
                {
                    _ = left | right;
                };

                ExecuteTest(method, "Com.BitSet.operator |(Com.BitSet, Com.BitSet)", "size at 1024 bits");
            }

            {
                Com.BitSet left = _GetRandomBitSet(1024);
                Com.BitSet right = left.Copy();

                Action method = () =>
                {
                    _ = left ^ right;
                };

                ExecuteTest(method, "Com.BitSet.operator ^(Com.BitSet, Com.BitSet)", "size at 1024 bits");
            }

            {
                Com.BitSet right = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    _ = ~right;
                };

                ExecuteTest(method, "Com.BitSet.operator ~(Com.BitSet)", "size at 1024 bits");
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
            // 颜色名

            {
                Com.ColorX color = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = Com.ColorManipulation.GetColorName(color);
                };

                ExecuteTest(method, "Com.ColorManipulation.GetColorName(Com.ColorX)");
            }

            {
                Color color = Com.ColorManipulation.GetRandomColor();

                Action method = () =>
                {
                    _ = Com.ColorManipulation.GetColorName(color);
                };

                ExecuteTest(method, "Com.ColorManipulation.GetColorName(System.Drawing.Color)");
            }

            // 随机色

            {
                Action method = () =>
                {
                    _ = Com.ColorManipulation.GetRandomColorX();
                };

                ExecuteTest(method, "Com.ColorManipulation.GetRandomColorX()");
            }

            {
                Action method = () =>
                {
                    _ = Com.ColorManipulation.GetRandomColor();
                };

                ExecuteTest(method, "Com.ColorManipulation.GetRandomColor()");
            }

            // 互补色，灰度

            {
                Com.ColorX color = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = Com.ColorManipulation.GetComplementaryColor(color);
                };

                ExecuteTest(method, "Com.ColorManipulation.GetComplementaryColor(Com.ColorX)");
            }

            {
                Color color = Com.ColorManipulation.GetRandomColor();

                Action method = () =>
                {
                    _ = Com.ColorManipulation.GetComplementaryColor(color);
                };

                ExecuteTest(method, "Com.ColorManipulation.GetComplementaryColor(System.Drawing.Color)");
            }

            {
                Com.ColorX color = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = Com.ColorManipulation.GetGrayscaleColor(color);
                };

                ExecuteTest(method, "Com.ColorManipulation.GetGrayscaleColor(Com.ColorX)");
            }

            {
                Color color = Com.ColorManipulation.GetRandomColor();

                Action method = () =>
                {
                    _ = Com.ColorManipulation.GetGrayscaleColor(color);
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
                    _ = Com.ColorManipulation.BlendByRGB(color1, color2, proportion);
                };

                ExecuteTest(method, "Com.ColorManipulation.BlendByRGB(Com.ColorX, Com.ColorX, double)");
            }

            {
                Color color1 = Com.ColorManipulation.GetRandomColor();
                Color color2 = Com.ColorManipulation.GetRandomColor();
                double proportion = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.BlendByRGB(color1, color2, proportion);
                };

                ExecuteTest(method, "Com.ColorManipulation.BlendByRGB(System.Drawing.Color, System.Drawing.Color, double)");
            }

            {
                Com.ColorX color1 = Com.ColorManipulation.GetRandomColorX();
                Com.ColorX color2 = Com.ColorManipulation.GetRandomColorX();
                double proportion = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.BlendByHSV(color1, color2, proportion);
                };

                ExecuteTest(method, "Com.ColorManipulation.BlendByHSV(Com.ColorX, Com.ColorX, double)");
            }

            {
                Color color1 = Com.ColorManipulation.GetRandomColor();
                Color color2 = Com.ColorManipulation.GetRandomColor();
                double proportion = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.BlendByHSV(color1, color2, proportion);
                };

                ExecuteTest(method, "Com.ColorManipulation.BlendByHSV(System.Drawing.Color, System.Drawing.Color, double)");
            }

            {
                Com.ColorX color1 = Com.ColorManipulation.GetRandomColorX();
                Com.ColorX color2 = Com.ColorManipulation.GetRandomColorX();
                double proportion = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.BlendByHSL(color1, color2, proportion);
                };

                ExecuteTest(method, "Com.ColorManipulation.BlendByHSL(Com.ColorX, Com.ColorX, double)");
            }

            {
                Color color1 = Com.ColorManipulation.GetRandomColor();
                Color color2 = Com.ColorManipulation.GetRandomColor();
                double proportion = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.BlendByHSL(color1, color2, proportion);
                };

                ExecuteTest(method, "Com.ColorManipulation.BlendByHSL(System.Drawing.Color, System.Drawing.Color, double)");
            }

            {
                Com.ColorX color1 = Com.ColorManipulation.GetRandomColorX();
                Com.ColorX color2 = Com.ColorManipulation.GetRandomColorX();
                double proportion = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.BlendByCMYK(color1, color2, proportion);
                };

                ExecuteTest(method, "Com.ColorManipulation.BlendByCMYK(Com.ColorX, Com.ColorX, double)");
            }

            {
                Color color1 = Com.ColorManipulation.GetRandomColor();
                Color color2 = Com.ColorManipulation.GetRandomColor();
                double proportion = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.BlendByCMYK(color1, color2, proportion);
                };

                ExecuteTest(method, "Com.ColorManipulation.BlendByCMYK(System.Drawing.Color, System.Drawing.Color, double)");
            }

            {
                Com.ColorX color1 = Com.ColorManipulation.GetRandomColorX();
                Com.ColorX color2 = Com.ColorManipulation.GetRandomColorX();
                double proportion = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.BlendByLAB(color1, color2, proportion);
                };

                ExecuteTest(method, "Com.ColorManipulation.BlendByLAB(Com.ColorX, Com.ColorX, double)");
            }

            {
                Color color1 = Com.ColorManipulation.GetRandomColor();
                Color color2 = Com.ColorManipulation.GetRandomColor();
                double proportion = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.BlendByLAB(color1, color2, proportion);
                };

                ExecuteTest(method, "Com.ColorManipulation.BlendByLAB(System.Drawing.Color, System.Drawing.Color, double)");
            }

            // Shift

            {
                Com.ColorX color = Com.ColorManipulation.GetRandomColorX();
                double level = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.ShiftLightnessByHSV(color, level);
                };

                ExecuteTest(method, "Com.ColorManipulation.ShiftLightnessByHSV(Com.ColorX, double)");
            }

            {
                Color color = Com.ColorManipulation.GetRandomColor();
                double level = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.ShiftLightnessByHSV(color, level);
                };

                ExecuteTest(method, "Com.ColorManipulation.ShiftLightnessByHSV(System.Drawing.Color, double)");
            }

            {
                Com.ColorX color = Com.ColorManipulation.GetRandomColorX();
                double level = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.ShiftLightnessByHSL(color, level);
                };

                ExecuteTest(method, "Com.ColorManipulation.ShiftLightnessByHSL(Com.ColorX, double)");
            }

            {
                Color color = Com.ColorManipulation.GetRandomColor();
                double level = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.ShiftLightnessByHSL(color, level);
                };

                ExecuteTest(method, "Com.ColorManipulation.ShiftLightnessByHSL(System.Drawing.Color, double)");
            }

            {
                Com.ColorX color = Com.ColorManipulation.GetRandomColorX();
                double level = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.ShiftLightnessByLAB(color, level);
                };

                ExecuteTest(method, "Com.ColorManipulation.ShiftLightnessByLAB(Com.ColorX, double)");
            }

            {
                Color color = Com.ColorManipulation.GetRandomColor();
                double level = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.ShiftLightnessByLAB(color, level);
                };

                ExecuteTest(method, "Com.ColorManipulation.ShiftLightnessByLAB(System.Drawing.Color, double)");
            }

            {
                Com.ColorX color = Com.ColorManipulation.GetRandomColorX();
                double level = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.ShiftSaturationByHSV(color, level);
                };

                ExecuteTest(method, "Com.ColorManipulation.ShiftSaturationByHSV(Com.ColorX, double)");
            }

            {
                Color color = Com.ColorManipulation.GetRandomColor();
                double level = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.ShiftSaturationByHSV(color, level);
                };

                ExecuteTest(method, "Com.ColorManipulation.ShiftSaturationByHSV(System.Drawing.Color, double)");
            }

            {
                Com.ColorX color = Com.ColorManipulation.GetRandomColorX();
                double level = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.ShiftSaturationByHSL(color, level);
                };

                ExecuteTest(method, "Com.ColorManipulation.ShiftSaturationByHSL(Com.ColorX, double)");
            }

            {
                Color color = Com.ColorManipulation.GetRandomColor();
                double level = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.ShiftSaturationByHSL(color, level);
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
                    _ = new Com.ColorX(color);
                };

                ExecuteTest(method, "Com.ColorX.ColorX(System.Drawing.Color)");
            }

            {
                int argb = Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = new Com.ColorX(argb);
                };

                ExecuteTest(method, "Com.ColorX.ColorX(int)");
            }

            {
                string hexCode = Com.ColorManipulation.GetRandomColorX().ARGBHexCode;

                Action method = () =>
                {
                    _ = new Com.ColorX(hexCode);
                };

                ExecuteTest(method, "Com.ColorX.ColorX(string)");
            }
        }

        protected override void Property()
        {
            // Is

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.IsEmpty;
                };

                ExecuteTest(method, "Com.ColorX.IsEmpty.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.IsTransparent;
                };

                ExecuteTest(method, "Com.ColorX.IsTransparent.get()");
            }

            // Opacity

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.Opacity;
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
                    _ = colorX.Alpha;
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
                    _ = colorX.Red;
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
                    _ = colorX.Green;
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
                    _ = colorX.Blue;
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
                    _ = colorX.Hue_HSV;
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
                    _ = colorX.Saturation_HSV;
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
                    _ = colorX.Brightness;
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
                    _ = colorX.Hue_HSL;
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
                    _ = colorX.Saturation_HSL;
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
                    _ = colorX.Lightness_HSL;
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
                    _ = colorX.Cyan;
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
                    _ = colorX.Magenta;
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
                    _ = colorX.Yellow;
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
                    _ = colorX.Black;
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
                    _ = colorX.Lightness_LAB;
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
                    _ = colorX.GreenRed;
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
                    _ = colorX.BlueYellow;
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
                    _ = colorX.RGB;
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
                    _ = colorX.HSV;
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
                    _ = colorX.HSL;
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
                    _ = colorX.CMYK;
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
                    _ = colorX.LAB;
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
                    _ = colorX.ComplementaryColor;
                };

                ExecuteTest(method, "Com.ColorX.ComplementaryColor.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.GrayscaleColor;
                };

                ExecuteTest(method, "Com.ColorX.GrayscaleColor.get()");
            }

            // HexCode

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.ARGBHexCode;
                };

                ExecuteTest(method, "Com.ColorX.ARGBHexCode.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.RGBHexCode;
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
                object obj = colorX;

                Action method = () =>
                {
                    _ = colorX.Equals(obj);
                };

                ExecuteTest(method, "Com.ColorX.Equals(object)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.GetHashCode();
                };

                ExecuteTest(method, "Com.ColorX.GetHashCode()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.ToString();
                };

                ExecuteTest(method, "Com.ColorX.ToString()");
            }

            // Equals

            {
                Com.ColorX left = Com.ColorManipulation.GetRandomColorX();
                Com.ColorX right = left;

                Action method = () =>
                {
                    _ = left.Equals(right);
                };

                ExecuteTest(method, "Com.ColorX.Equals(Com.ColorX)");
            }

            // To

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.ToColor();
                };

                ExecuteTest(method, "Com.ColorX.ToColor()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.ToARGB();
                };

                ExecuteTest(method, "Com.ColorX.ToARGB()");
            }

            // AtOpacity

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = colorX.AtOpacity(value);
                };

                ExecuteTest(method, "Com.ColorX.AtOpacity(double)");
            }

            // AtRGB

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(255);

                Action method = () =>
                {
                    _ = colorX.AtAlpha(value);
                };

                ExecuteTest(method, "Com.ColorX.AtAlpha(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(255);

                Action method = () =>
                {
                    _ = colorX.AtRed(value);
                };

                ExecuteTest(method, "Com.ColorX.AtRed(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(255);

                Action method = () =>
                {
                    _ = colorX.AtGreen(value);
                };

                ExecuteTest(method, "Com.ColorX.AtGreen(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(255);

                Action method = () =>
                {
                    _ = colorX.AtBlue(value);
                };

                ExecuteTest(method, "Com.ColorX.AtBlue(double)");
            }

            // AtHSV

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(360);

                Action method = () =>
                {
                    _ = colorX.AtHue_HSV(value);
                };

                ExecuteTest(method, "Com.ColorX.AtHue_HSV(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = colorX.AtSaturation_HSV(value);
                };

                ExecuteTest(method, "Com.ColorX.AtSaturation_HSV(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = colorX.AtBrightness(value);
                };

                ExecuteTest(method, "Com.ColorX.AtBrightness(double)");
            }

            // AtHSL

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(360);

                Action method = () =>
                {
                    _ = colorX.AtHue_HSL(value);
                };

                ExecuteTest(method, "Com.ColorX.AtHue_HSL(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = colorX.AtSaturation_HSL(value);
                };

                ExecuteTest(method, "Com.ColorX.AtSaturation_HSL(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = colorX.AtLightness_HSL(value);
                };

                ExecuteTest(method, "Com.ColorX.AtLightness_HSL(double)");
            }

            // AtCMYK

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = colorX.AtCyan(value);
                };

                ExecuteTest(method, "Com.ColorX.AtCyan(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = colorX.AtMagenta(value);
                };

                ExecuteTest(method, "Com.ColorX.AtMagenta(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = colorX.AtYellow(value);
                };

                ExecuteTest(method, "Com.ColorX.AtYellow(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = colorX.AtBlack(value);
                };

                ExecuteTest(method, "Com.ColorX.AtBlack(double)");
            }

            // AtLAB

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = colorX.AtLightness_LAB(value);
                };

                ExecuteTest(method, "Com.ColorX.AtLightness_LAB(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(-128, 128);

                Action method = () =>
                {
                    _ = colorX.AtGreenRed(value);
                };

                ExecuteTest(method, "Com.ColorX.AtGreenRed(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(-128, 128);

                Action method = () =>
                {
                    _ = colorX.AtBlueYellow(value);
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
                    _ = Com.ColorX.Equals(left, right);
                };

                ExecuteTest(method, "Com.ColorX.Equals(Com.ColorX, Com.ColorX)");
            }

            // FromColor

            {
                int alpha = Com.Statistics.RandomInteger(255);
                Color color = Com.ColorManipulation.GetRandomColor();

                Action method = () =>
                {
                    _ = Com.ColorX.FromColor(alpha, color);
                };

                ExecuteTest(method, "Com.ColorX.FromColor(int, System.Drawing.Color)");
            }

            {
                Color color = Com.ColorManipulation.GetRandomColor();

                Action method = () =>
                {
                    _ = Com.ColorX.FromColor(color);
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
                    _ = Com.ColorX.FromRGB(alpha, red, green, blue);
                };

                ExecuteTest(method, "Com.ColorX.FromRGB(double, double, double, double)");
            }

            {
                double red = Com.Statistics.RandomDouble(255);
                double green = Com.Statistics.RandomDouble(255);
                double blue = Com.Statistics.RandomDouble(255);

                Action method = () =>
                {
                    _ = Com.ColorX.FromRGB(red, green, blue);
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
                    _ = Com.ColorX.FromRGB(alpha, rgb);
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
                    _ = Com.ColorX.FromRGB(rgb);
                };

                ExecuteTest(method, "Com.ColorX.FromRGB(Com.PointD3D)");
            }

            {
                int argb = Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = Com.ColorX.FromRGB(argb);
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
                    _ = Com.ColorX.FromHSV(hue, saturation, brightness, opacity);
                };

                ExecuteTest(method, "Com.ColorX.FromHSV(double, double, double, double)");
            }

            {
                double hue = Com.Statistics.RandomDouble(360);
                double saturation = Com.Statistics.RandomDouble(100);
                double brightness = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorX.FromHSV(hue, saturation, brightness);
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
                    _ = Com.ColorX.FromHSV(hsv, opacity);
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
                    _ = Com.ColorX.FromHSV(hsv);
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
                    _ = Com.ColorX.FromHSL(hue, saturation, lightness, opacity);
                };

                ExecuteTest(method, "Com.ColorX.FromHSL(double, double, double, double)");
            }

            {
                double hue = Com.Statistics.RandomDouble(360);
                double saturation = Com.Statistics.RandomDouble(100);
                double lightness = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorX.FromHSL(hue, saturation, lightness);
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
                    _ = Com.ColorX.FromHSL(hsl, opacity);
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
                    _ = Com.ColorX.FromHSL(hsl);
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
                    _ = Com.ColorX.FromCMYK(cyan, magenta, yellow, black, opacity);
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
                    _ = Com.ColorX.FromCMYK(cyan, magenta, yellow, black);
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
                    _ = Com.ColorX.FromCMYK(cmyk, opacity);
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
                    _ = Com.ColorX.FromCMYK(cmyk);
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
                    _ = Com.ColorX.FromLAB(lightness, greenRed, blueYellow, opacity);
                };

                ExecuteTest(method, "Com.ColorX.FromLAB(double, double, double, double)");
            }

            {
                double lightness = Com.Statistics.RandomDouble(100);
                double greenRed = Com.Statistics.RandomDouble(-128, 128);
                double blueYellow = Com.Statistics.RandomDouble(-128, 128);

                Action method = () =>
                {
                    _ = Com.ColorX.FromLAB(lightness, greenRed, blueYellow);
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
                    _ = Com.ColorX.FromLAB(lab, opacity);
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
                    _ = Com.ColorX.FromLAB(lab);
                };

                ExecuteTest(method, "Com.ColorX.FromLAB(Com.PointD3D)");
            }

            // FromHexCode

            {
                string hexCode = Com.ColorManipulation.GetRandomColorX().ARGBHexCode;

                Action method = () =>
                {
                    _ = Com.ColorX.FromHexCode(hexCode);
                };

                ExecuteTest(method, "Com.ColorX.FromHexCode(string)");
            }

            // RandomColor

            {
                Action method = () =>
                {
                    _ = Com.ColorX.RandomColor();
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
                    _ = (left == right);
                };

                ExecuteTest(method, "Com.ColorX.operator ==(Com.ColorX, Com.ColorX)");
            }

            {
                Com.ColorX left = Com.ColorManipulation.GetRandomColorX();
                Com.ColorX right = left;

                Action method = () =>
                {
                    _ = (left != right);
                };

                ExecuteTest(method, "Com.ColorX.operator !=(Com.ColorX, Com.ColorX)");
            }

            // 类型转换

            {
                Com.ColorX color = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = (Color)color;
                };

                ExecuteTest(method, "Com.ColorX.explicit operator System.Drawing.Color(Com.ColorX)");
            }

            {
                Color color = Com.ColorManipulation.GetRandomColor();

                Action method = () =>
                {
                    _ = (Com.ColorX)color;
                };

                ExecuteTest(method, "Com.ColorX.implicit operator Com.ColorX(System.Drawing.Color)");
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
                double imaginary = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = new Com.Complex(real, imaginary);
                };

                ExecuteTest(method, "Com.Complex.Complex(double, double)");
            }

            {
                double real = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = new Com.Complex(real);
                };

                ExecuteTest(method, "Com.Complex.Complex(double)");
            }

            {
                Com.PointD pt = new Com.PointD(Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = new Com.Complex(pt);
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
                    _ = comp.IsNaN;
                };

                ExecuteTest(method, "Com.Complex.IsNaN.get()");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = comp.IsInfinity;
                };

                ExecuteTest(method, "Com.Complex.IsInfinity.get()");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = comp.IsNaNOrInfinity;
                };

                ExecuteTest(method, "Com.Complex.IsNaNOrInfinity.get()");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = comp.IsZero;
                };

                ExecuteTest(method, "Com.Complex.IsZero.get()");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = comp.IsOne;
                };

                ExecuteTest(method, "Com.Complex.IsOne.get()");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = comp.IsImaginaryOne;
                };

                ExecuteTest(method, "Com.Complex.IsImaginaryOne.get()");
            }

            // 分量

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = comp.Real;
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
                    _ = comp.Imaginary;
                };

                ExecuteTest(method, "Com.Complex.Imaginary.get()");
            }

            {
                Com.Complex comp = _GetRandomComplex();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    comp.Imaginary = value;
                };

                ExecuteTest(method, "Com.Complex.Imaginary.set(double)");
            }

            // 模与辐角

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = comp.Module;
                };

                ExecuteTest(method, "Com.Complex.Module.get()");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = comp.ModuleSquared;
                };

                ExecuteTest(method, "Com.Complex.ModuleSquared.get()");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = comp.Argument;
                };

                ExecuteTest(method, "Com.Complex.Argument.get()");
            }

            // 共轭与倒数

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = comp.Conjugate;
                };

                ExecuteTest(method, "Com.Complex.Conjugate.get()");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = comp.Reciprocal;
                };

                ExecuteTest(method, "Com.Complex.Reciprocal.get()");
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
                object obj = comp;

                Action method = () =>
                {
                    _ = comp.Equals(obj);
                };

                ExecuteTest(method, "Com.Complex.Equals(object)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = comp.GetHashCode();
                };

                ExecuteTest(method, "Com.Complex.GetHashCode()");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = comp.ToString();
                };

                ExecuteTest(method, "Com.Complex.ToString()");
            }

            // Equals

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = left;

                Action method = () =>
                {
                    _ = left.Equals(right);
                };

                ExecuteTest(method, "Com.Complex.Equals(Com.Complex)");
            }

            // CompareTo

            {
                Com.Complex left = _GetRandomComplex();
                object right = left;

                Action method = () =>
                {
                    _ = left.CompareTo(right);
                };

                ExecuteTest(method, "Com.Complex.CompareTo(object)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = left;

                Action method = () =>
                {
                    _ = left.CompareTo(right);
                };

                ExecuteTest(method, "Com.Complex.CompareTo(Com.Complex)");
            }

            // To

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = comp.ToPointD();
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
                    _ = Com.Complex.Equals(left, right);
                };

                ExecuteTest(method, "Com.Complex.Equals(Com.Complex, Com.Complex)");
            }

            // Compare

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = left;

                Action method = () =>
                {
                    _ = Com.Complex.Compare(left, right);
                };

                ExecuteTest(method, "Com.Complex.CompareTo(Com.Complex, Com.Complex)");
            }

            // From

            {
                Com.PointD pt = new Com.PointD(Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = Com.Complex.FromPointD(pt);
                };

                ExecuteTest(method, "Com.Complex.FromPointD(Com.PointD)");
            }

            {
                double module = Com.Statistics.RandomDouble(-1E18, 1E18);
                double argument = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Complex complex = Com.Complex.FromPolarCoordinates(module, argument);
                };

                ExecuteTest(method, "Com.Complex.FromPolarCoordinates(double, double)");
            }

            // 幂函数，指数函数，对数函数

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Sqr(comp);
                };

                ExecuteTest(method, "Com.Complex.Sqr(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Sqrt(comp);
                };

                ExecuteTest(method, "Com.Complex.Sqrt(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Exp(comp);
                };

                ExecuteTest(method, "Com.Complex.Exp(Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Pow(left, right);
                };

                ExecuteTest(method, "Com.Complex.Pow(Com.Complex, Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                double right = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.Complex.Pow(left, right);
                };

                ExecuteTest(method, "Com.Complex.Pow(Com.Complex, double)");
            }

            {
                double left = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Pow(left, right);
                };

                ExecuteTest(method, "Com.Complex.Pow(double, Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Log(comp);
                };

                ExecuteTest(method, "Com.Complex.Log(Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Log(left, right);
                };

                ExecuteTest(method, "Com.Complex.Log(Com.Complex, Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                double right = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.Complex.Log(left, right);
                };

                ExecuteTest(method, "Com.Complex.Log(Com.Complex, double)");
            }

            {
                double left = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Log(left, right);
                };

                ExecuteTest(method, "Com.Complex.Log(double, Com.Complex)");
            }

            // 三角函数

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Sin(comp);
                };

                ExecuteTest(method, "Com.Complex.Sin(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Cos(comp);
                };

                ExecuteTest(method, "Com.Complex.Cos(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Tan(comp);
                };

                ExecuteTest(method, "Com.Complex.Tan(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Asin(comp);
                };

                ExecuteTest(method, "Com.Complex.Asin(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Acos(comp);
                };

                ExecuteTest(method, "Com.Complex.Acos(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Atan(comp);
                };

                ExecuteTest(method, "Com.Complex.Atan(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Sinh(comp);
                };

                ExecuteTest(method, "Com.Complex.Sinh(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Cosh(comp);
                };

                ExecuteTest(method, "Com.Complex.Cosh(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Tanh(comp);
                };

                ExecuteTest(method, "Com.Complex.Tanh(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Asinh(comp);
                };

                ExecuteTest(method, "Com.Complex.Asinh(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Acosh(comp);
                };

                ExecuteTest(method, "Com.Complex.Acosh(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Atanh(comp);
                };

                ExecuteTest(method, "Com.Complex.Atanh(Com.Complex)");
            }

            // 初等函数

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Abs(comp);
                };

                ExecuteTest(method, "Com.Complex.Abs(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Sign(comp);
                };

                ExecuteTest(method, "Com.Complex.Sign(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Ceiling(comp);
                };

                ExecuteTest(method, "Com.Complex.Ceiling(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Floor(comp);
                };

                ExecuteTest(method, "Com.Complex.Floor(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Round(comp);
                };

                ExecuteTest(method, "Com.Complex.Round(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Truncate(comp);
                };

                ExecuteTest(method, "Com.Complex.Truncate(Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Max(left, right);
                };

                ExecuteTest(method, "Com.Complex.Max(Com.Complex, Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Min(left, right);
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
                    _ = (left == right);
                };

                ExecuteTest(method, "Com.Complex.operator ==(Com.Complex, Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = left;

                Action method = () =>
                {
                    _ = (left != right);
                };

                ExecuteTest(method, "Com.Complex.operator !=(Com.Complex, Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    _ = (left < right);
                };

                ExecuteTest(method, "Com.Complex.operator <(Com.Complex, Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    _ = (left > right);
                };

                ExecuteTest(method, "Com.Complex.operator >(Com.Complex, Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    _ = (left <= right);
                };

                ExecuteTest(method, "Com.Complex.operator <=(Com.Complex, Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    _ = (left >= right);
                };

                ExecuteTest(method, "Com.Complex.operator >=(Com.Complex, Com.Complex)");
            }

            // 运算

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = +comp;
                };

                ExecuteTest(method, "Com.Complex.operator +(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = -comp;
                };

                ExecuteTest(method, "Com.Complex.operator -(Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    _ = left + right;
                };

                ExecuteTest(method, "Com.Complex.operator +(Com.Complex, Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                double right = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = left + right;
                };

                ExecuteTest(method, "Com.Complex.operator +(Com.Complex, double)");
            }

            {
                double left = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    _ = left + right;
                };

                ExecuteTest(method, "Com.Complex.operator +(double, Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    _ = left - right;
                };

                ExecuteTest(method, "Com.Complex.operator -(Com.Complex, Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                double right = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = left - right;
                };

                ExecuteTest(method, "Com.Complex.operator -(Com.Complex, double)");
            }

            {
                double left = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    _ = left - right;
                };

                ExecuteTest(method, "Com.Complex.operator -(double, Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    _ = left * right;
                };

                ExecuteTest(method, "Com.Complex.operator *(Com.Complex, Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                double right = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = left * right;
                };

                ExecuteTest(method, "Com.Complex.operator *(Com.Complex, double)");
            }

            {
                double left = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    _ = left * right;
                };

                ExecuteTest(method, "Com.Complex.operator *(double, Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    _ = left / right;
                };

                ExecuteTest(method, "Com.Complex.operator /(Com.Complex, Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                double right = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = left / right;
                };

                ExecuteTest(method, "Com.Complex.operator /(Com.Complex, double)");
            }

            {
                double left = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    _ = left / right;
                };

                ExecuteTest(method, "Com.Complex.operator /(double, Com.Complex)");
            }

            // 类型转换

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = (Com.PointD)comp;
                };

                ExecuteTest(method, "Com.Complex.explicit operator Com.PointD(Com.Complex)");
            }

            {
                double real = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = (Com.Complex)real;
                };

                ExecuteTest(method, "Com.Complex.explicit operator Com.Complex(double)");
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
                    _ = new Com.DateTimeX(totalMilliseconds, utcOffset);
                };

                ExecuteTest(method, "Com.DateTimeX.DateTimeX(decimal, double)");
            }

            {
                decimal totalMilliseconds = (decimal)Com.Statistics.RandomDouble(-1E16, 1E16);

                Action method = () =>
                {
                    _ = new Com.DateTimeX(totalMilliseconds);
                };

                ExecuteTest(method, "Com.DateTimeX.DateTimeX(decimal)");
            }

            {
                long year = Com.Statistics.RandomInteger(1, 10000) * ((Com.Statistics.RandomInteger() % 2) * 2 - 1);
                int month = Com.Statistics.RandomInteger(1, 13);
                int day = Com.Statistics.RandomInteger(1, Com.DateTimeX.DaysInMonth(year, month) + 1);
                int hour = Com.Statistics.RandomInteger(0, 24);
                int minute = Com.Statistics.RandomInteger(0, 60);
                int second = Com.Statistics.RandomInteger(0, 60);
                int millisecond = Com.Statistics.RandomInteger(0, 1000);
                double utcOffset = Com.Statistics.RandomDouble(-12, 12);

                Action method = () =>
                {
                    _ = new Com.DateTimeX(year, month, day, hour, minute, second, millisecond, utcOffset);
                };

                ExecuteTest(method, "Com.DateTimeX.DateTimeX(long, int, int, int, int, int, int, double)");
            }

            {
                long year = Com.Statistics.RandomInteger(1, 10000) * ((Com.Statistics.RandomInteger() % 2) * 2 - 1);
                int month = Com.Statistics.RandomInteger(1, 13);
                int day = Com.Statistics.RandomInteger(1, Com.DateTimeX.DaysInMonth(year, month) + 1);
                int hour = Com.Statistics.RandomInteger(0, 24);
                int minute = Com.Statistics.RandomInteger(0, 60);
                int second = Com.Statistics.RandomInteger(0, 60);
                int millisecond = Com.Statistics.RandomInteger(0, 1000);

                Action method = () =>
                {
                    _ = new Com.DateTimeX(year, month, day, hour, minute, second, millisecond);
                };

                ExecuteTest(method, "Com.DateTimeX.DateTimeX(long, int, int, int, int, int, int)");
            }

            {
                long year = Com.Statistics.RandomInteger(1, 10000) * ((Com.Statistics.RandomInteger() % 2) * 2 - 1);
                int month = Com.Statistics.RandomInteger(1, 13);
                int day = Com.Statistics.RandomInteger(1, Com.DateTimeX.DaysInMonth(year, month) + 1);
                int hour = Com.Statistics.RandomInteger(0, 24);
                int minute = Com.Statistics.RandomInteger(0, 60);
                int second = Com.Statistics.RandomInteger(0, 60);
                double utcOffset = Com.Statistics.RandomDouble(-12, 12);

                Action method = () =>
                {
                    _ = new Com.DateTimeX(year, month, day, hour, minute, second, utcOffset);
                };

                ExecuteTest(method, "Com.DateTimeX.DateTimeX(long, int, int, int, int, int, double)");
            }

            {
                long year = Com.Statistics.RandomInteger(1, 10000) * ((Com.Statistics.RandomInteger() % 2) * 2 - 1);
                int month = Com.Statistics.RandomInteger(1, 13);
                int day = Com.Statistics.RandomInteger(1, Com.DateTimeX.DaysInMonth(year, month) + 1);
                int hour = Com.Statistics.RandomInteger(0, 24);
                int minute = Com.Statistics.RandomInteger(0, 60);
                int second = Com.Statistics.RandomInteger(0, 60);

                Action method = () =>
                {
                    _ = new Com.DateTimeX(year, month, day, hour, minute, second);
                };

                ExecuteTest(method, "Com.DateTimeX.DateTimeX(long, int, int, int, int, int)");
            }

            {
                long year = Com.Statistics.RandomInteger(1, 10000) * ((Com.Statistics.RandomInteger() % 2) * 2 - 1);
                int month = Com.Statistics.RandomInteger(1, 13);
                int day = Com.Statistics.RandomInteger(1, Com.DateTimeX.DaysInMonth(year, month) + 1);
                double utcOffset = Com.Statistics.RandomDouble(-12, 12);

                Action method = () =>
                {
                    _ = new Com.DateTimeX(year, month, day, utcOffset);
                };

                ExecuteTest(method, "Com.DateTimeX.DateTimeX(long, int, int, double)");
            }

            {
                long year = Com.Statistics.RandomInteger(1, 10000) * ((Com.Statistics.RandomInteger() % 2) * 2 - 1);
                int month = Com.Statistics.RandomInteger(1, 13);
                int day = Com.Statistics.RandomInteger(1, Com.DateTimeX.DaysInMonth(year, month) + 1);

                Action method = () =>
                {
                    _ = new Com.DateTimeX(year, month, day);
                };

                ExecuteTest(method, "Com.DateTimeX.DateTimeX(long, int, int)");
            }

            // UtcOffset，DateTimeX，DateTime

            {
                Com.DateTimeX dateTime = _GetRandomDateTimeX();
                double utcOffset = Com.Statistics.RandomDouble(-12, 12);

                Action method = () =>
                {
                    _ = new Com.DateTimeX(dateTime, utcOffset);
                };

                ExecuteTest(method, "Com.DateTimeX.DateTimeX(Com.DateTimeX, double)");
            }

            {
                Com.DateTimeX dateTime = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = new Com.DateTimeX(dateTime);
                };

                ExecuteTest(method, "Com.DateTimeX.DateTimeX(Com.DateTimeX)");
            }

            {
                DateTime dateTime = _GetRandomDateTime();
                double utcOffset = Com.Statistics.RandomDouble(-12, 12);

                Action method = () =>
                {
                    _ = new Com.DateTimeX(dateTime, utcOffset);
                };

                ExecuteTest(method, "Com.DateTimeX.DateTimeX(System.DateTime, double)");
            }

            {
                DateTime dateTime = _GetRandomDateTime();

                Action method = () =>
                {
                    _ = new Com.DateTimeX(dateTime);
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
                    _ = dateTimeX.IsEmpty;
                };

                ExecuteTest(method, "Com.DateTimeX.IsEmpty.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.IsChristianEra;
                };

                ExecuteTest(method, "Com.DateTimeX.IsChristianEra.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.IsMinValue;
                };

                ExecuteTest(method, "Com.DateTimeX.IsMinValue.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.IsMaxValue;
                };

                ExecuteTest(method, "Com.DateTimeX.IsMaxValue.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.IsAnnoDomini;
                };

                ExecuteTest(method, "Com.DateTimeX.IsAnnoDomini.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.IsBeforeChrist;
                };

                ExecuteTest(method, "Com.DateTimeX.IsBeforeChrist.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.IsLeap;
                };

                ExecuteTest(method, "Com.DateTimeX.IsLeap.get()");
            }

            // UtcOffset，TotalMS，YMD-hms

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.UtcOffset;
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
                    _ = dateTimeX.TotalMilliseconds;
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
                    _ = dateTimeX.Year;
                };

                ExecuteTest(method, "Com.DateTimeX.Year.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                dateTimeX.Day = Com.Statistics.RandomInteger(1, 29);
                long value = Com.Statistics.RandomInteger(1, 10000) * ((Com.Statistics.RandomInteger() % 2) * 2 - 1);

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
                    _ = dateTimeX.Month;
                };

                ExecuteTest(method, "Com.DateTimeX.Month.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                dateTimeX.Day = Com.Statistics.RandomInteger(1, 29);
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
                    _ = dateTimeX.Day;
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
                    _ = dateTimeX.Hour;
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
                    _ = dateTimeX.Minute;
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
                    _ = dateTimeX.Second;
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
                    _ = dateTimeX.Millisecond;
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
                    _ = dateTimeX.Date;
                };

                ExecuteTest(method, "Com.DateTimeX.Date.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.TimeOfDay;
                };

                ExecuteTest(method, "Com.DateTimeX.TimeOfDay.get()");
            }

            // Of

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.WeekOfYear;
                };

                ExecuteTest(method, "Com.DateTimeX.WeekOfYear.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.DayOfYear;
                };

                ExecuteTest(method, "Com.DateTimeX.DayOfYear.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.HourOfYear;
                };

                ExecuteTest(method, "Com.DateTimeX.HourOfYear.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.MinuteOfYear;
                };

                ExecuteTest(method, "Com.DateTimeX.MinuteOfYear.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.SecondOfYear;
                };

                ExecuteTest(method, "Com.DateTimeX.SecondOfYear.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.MillisecondOfYear;
                };

                ExecuteTest(method, "Com.DateTimeX.MillisecondOfYear.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.DayOfWeek;
                };

                ExecuteTest(method, "Com.DateTimeX.DayOfWeek.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.HourOfDay;
                };

                ExecuteTest(method, "Com.DateTimeX.HourOfDay.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.MinuteOfDay;
                };

                ExecuteTest(method, "Com.DateTimeX.MinuteOfDay.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.SecondOfDay;
                };

                ExecuteTest(method, "Com.DateTimeX.SecondOfDay.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.MillisecondOfDay;
                };

                ExecuteTest(method, "Com.DateTimeX.MillisecondOfDay.get()");
            }

            // MonthString

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.MonthStringInChinese;
                };

                ExecuteTest(method, "Com.DateTimeX.MonthStringInChinese.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.MonthLongStringInEnglish;
                };

                ExecuteTest(method, "Com.DateTimeX.MonthLongStringInEnglish.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.MonthShortStringInEnglish;
                };

                ExecuteTest(method, "Com.DateTimeX.MonthShortStringInEnglish.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.MonthStringInJapaneseKanji;
                };

                ExecuteTest(method, "Com.DateTimeX.MonthStringInJapaneseKanji.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.MonthStringInJapaneseHiragana;
                };

                ExecuteTest(method, "Com.DateTimeX.MonthStringInJapaneseHiragana.get()");
            }

            // WeekdayString

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.WeekdayLongStringInChinese;
                };

                ExecuteTest(method, "Com.DateTimeX.WeekdayLongStringInChinese.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.WeekdayShortStringInChinese;
                };

                ExecuteTest(method, "Com.DateTimeX.WeekdayShortStringInChinese.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.WeekdayLongStringInEnglish;
                };

                ExecuteTest(method, "Com.DateTimeX.WeekdayLongStringInEnglish.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.WeekdayShortStringInEnglish;
                };

                ExecuteTest(method, "Com.DateTimeX.WeekdayShortStringInEnglish.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.WeekdayLongStringInJapanese;
                };

                ExecuteTest(method, "Com.DateTimeX.WeekdayLongStringInJapanese.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.WeekdayShortStringInJapanese;
                };

                ExecuteTest(method, "Com.DateTimeX.WeekdayShortStringInJapanese.get()");
            }

            // DateTimeString

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.DateLongString;
                };

                ExecuteTest(method, "Com.DateTimeX.DateLongString.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.DateShortString;
                };

                ExecuteTest(method, "Com.DateTimeX.DateShortString.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.TimeLongString;
                };

                ExecuteTest(method, "Com.DateTimeX.TimeLongString.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.TimeShortString;
                };

                ExecuteTest(method, "Com.DateTimeX.TimeShortString.get()");
            }
        }

        protected override void StaticProperty()
        {
            {
                Action method = () =>
                {
                    _ = Com.DateTimeX.Now;
                };

                ExecuteTest(method, "Com.DateTimeX.Now.get()");
            }

            {
                Action method = () =>
                {
                    _ = Com.DateTimeX.UtcNow;
                };

                ExecuteTest(method, "Com.DateTimeX.UtcNow.get()");
            }

            {
                Action method = () =>
                {
                    _ = Com.DateTimeX.Today;
                };

                ExecuteTest(method, "Com.DateTimeX.Today.get()");
            }

            {
                Action method = () =>
                {
                    _ = Com.DateTimeX.UtcToday;
                };

                ExecuteTest(method, "Com.DateTimeX.UtcToday.get()");
            }
        }

        protected override void Method()
        {
            // object

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                object obj = dateTimeX;

                Action method = () =>
                {
                    _ = dateTimeX.Equals(obj);
                };

                ExecuteTest(method, "Com.DateTimeX.Equals(object)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.GetHashCode();
                };

                ExecuteTest(method, "Com.DateTimeX.GetHashCode()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.ToString();
                };

                ExecuteTest(method, "Com.DateTimeX.ToString()");
            }

            // Equals

            {
                Com.DateTimeX left = _GetRandomDateTimeX();
                Com.DateTimeX right = left;

                Action method = () =>
                {
                    _ = left.Equals(right);
                };

                ExecuteTest(method, "Com.DateTimeX.Equals(Com.DateTimeX)");
            }

            // CompareTo

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                object obj = dateTimeX;

                Action method = () =>
                {
                    _ = dateTimeX.CompareTo(obj);
                };

                ExecuteTest(method, "Com.DateTimeX.CompareTo(object)");
            }

            {
                Com.DateTimeX left = _GetRandomDateTimeX();
                Com.DateTimeX right = left;

                Action method = () =>
                {
                    _ = left.CompareTo(right);
                };

                ExecuteTest(method, "Com.DateTimeX.CompareTo(Com.DateTimeX)");
            }

            // Add

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                TimeSpan timeSpan = TimeSpan.FromMilliseconds(Com.Statistics.RandomInteger(-999, 1000) * 365.25 * 86400000);

                Action method = () =>
                {
                    _ = dateTimeX.Add(timeSpan);
                };

                ExecuteTest(method, "Com.DateTimeX.Add(TimeSpan)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                long years = Com.Statistics.RandomInteger(-999, 1000);

                Action method = () =>
                {
                    _ = dateTimeX.AddYears(years);
                };

                ExecuteTest(method, "Com.DateTimeX.AddYears(long)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                long months = Com.Statistics.RandomInteger(-999, 1000) * 12;

                Action method = () =>
                {
                    _ = dateTimeX.AddMonths(months);
                };

                ExecuteTest(method, "Com.DateTimeX.AddMonths(long)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                double weeks = Com.Statistics.RandomInteger(-999, 1000) * 52.18;

                Action method = () =>
                {
                    _ = dateTimeX.AddWeeks(weeks);
                };

                ExecuteTest(method, "Com.DateTimeX.AddWeeks(double)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                double days = Com.Statistics.RandomInteger(-999, 1000) * 365.25;

                Action method = () =>
                {
                    _ = dateTimeX.AddDays(days);
                };

                ExecuteTest(method, "Com.DateTimeX.AddDays(double)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                double hours = Com.Statistics.RandomInteger(-999, 1000) * 365.25 * 24;

                Action method = () =>
                {
                    _ = dateTimeX.AddHours(hours);
                };

                ExecuteTest(method, "Com.DateTimeX.AddHours(double)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                double minutes = Com.Statistics.RandomInteger(-999, 1000) * 365.25 * 1440;

                Action method = () =>
                {
                    _ = dateTimeX.AddMinutes(minutes);
                };

                ExecuteTest(method, "Com.DateTimeX.AddMinutes(double)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                double seconds = Com.Statistics.RandomInteger(-999, 1000) * 365.25 * 86400;

                Action method = () =>
                {
                    _ = dateTimeX.AddSeconds(seconds);
                };

                ExecuteTest(method, "Com.DateTimeX.AddSeconds(double)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                decimal milliseconds = (decimal)(Com.Statistics.RandomInteger(-999, 1000) * 365.25 * 86400000);

                Action method = () =>
                {
                    _ = dateTimeX.AddMilliseconds(milliseconds);
                };

                ExecuteTest(method, "Com.DateTimeX.AddMilliseconds(decimal)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                double milliseconds = Com.Statistics.RandomInteger(-999, 1000) * 365.25 * 86400000;

                Action method = () =>
                {
                    _ = dateTimeX.AddMilliseconds(milliseconds);
                };

                ExecuteTest(method, "Com.DateTimeX.AddMilliseconds(double)");
            }

            // To

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.ToLocalTime();
                };

                ExecuteTest(method, "Com.DateTimeX.ToLocalTime()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.ToUniversalTime();
                };

                ExecuteTest(method, "Com.DateTimeX.ToUniversalTime()");
            }

            // ToString

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.ToLongDateString();
                };

                ExecuteTest(method, "Com.DateTimeX.ToLongDateString()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.ToShortDateString();
                };

                ExecuteTest(method, "Com.DateTimeX.ToShortDateString()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.ToLongTimeString();
                };

                ExecuteTest(method, "Com.DateTimeX.ToLongTimeString()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.ToShortTimeString();
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
                    _ = Com.DateTimeX.Equals(left, right);
                };

                ExecuteTest(method, "Com.DateTimeX.Equals(Com.DateTimeX, Com.DateTimeX)");
            }

            // Compare

            {
                Com.DateTimeX left = _GetRandomDateTimeX();
                Com.DateTimeX right = left;

                Action method = () =>
                {
                    _ = Com.DateTimeX.Compare(left, right);
                };

                ExecuteTest(method, "Com.DateTimeX.Compare(Com.DateTimeX, Com.DateTimeX)");
            }

            // IsLeapYear

            {
                long year = Com.Statistics.RandomInteger(-999999, 1000000);

                Action method = () =>
                {
                    _ = Com.DateTimeX.IsLeapYear(year);
                };

                ExecuteTest(method, "Com.DateTimeX.IsLeapYear(long)");
            }

            // Days

            {
                long year = Com.Statistics.RandomInteger(-999999, 1000000);

                Action method = () =>
                {
                    _ = Com.DateTimeX.DaysInYear(year);
                };

                ExecuteTest(method, "Com.DateTimeX.DaysInYear(long)");
            }

            {
                long year = Com.Statistics.RandomInteger(-999999, 1000000);
                int month = Com.Statistics.RandomInteger(1, 13);

                Action method = () =>
                {
                    _ = Com.DateTimeX.DaysInMonth(year, month);
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
                    _ = (left == right);
                };

                ExecuteTest(method, "Com.DateTimeX.operator ==(Com.DateTimeX, Com.DateTimeX)");
            }

            {
                Com.DateTimeX left = _GetRandomDateTimeX();
                Com.DateTimeX right = left;

                Action method = () =>
                {
                    _ = (left != right);
                };

                ExecuteTest(method, "Com.DateTimeX.operator !=(Com.DateTimeX, Com.DateTimeX)");
            }

            {
                Com.DateTimeX left = _GetRandomDateTimeX();
                Com.DateTimeX right = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = (left < right);
                };

                ExecuteTest(method, "Com.DateTimeX.operator <(Com.DateTimeX, Com.DateTimeX)");
            }

            {
                Com.DateTimeX left = _GetRandomDateTimeX();
                Com.DateTimeX right = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = (left > right);
                };

                ExecuteTest(method, "Com.DateTimeX.operator >(Com.DateTimeX, Com.DateTimeX)");
            }

            {
                Com.DateTimeX left = _GetRandomDateTimeX();
                Com.DateTimeX right = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = (left <= right);
                };

                ExecuteTest(method, "Com.DateTimeX.operator <=(Com.DateTimeX, Com.DateTimeX)");
            }

            {
                Com.DateTimeX left = _GetRandomDateTimeX();
                Com.DateTimeX right = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = (left >= right);
                };

                ExecuteTest(method, "Com.DateTimeX.operator >=(Com.DateTimeX, Com.DateTimeX)");
            }

            // 运算

            {
                Com.DateTimeX dateTime = _GetRandomDateTimeX();
                TimeSpan timeSpan = TimeSpan.FromMilliseconds(Com.Statistics.RandomInteger(-999, 1000) * 365.25 * 86400000);

                Action method = () =>
                {
                    _ = dateTime - timeSpan;
                };

                ExecuteTest(method, "Com.DateTimeX.operator +(Com.DateTimeX, TimeSpan)");
            }

            {
                Com.DateTimeX dateTime = _GetRandomDateTimeX();
                TimeSpan timeSpan = TimeSpan.FromMilliseconds(Com.Statistics.RandomInteger(-999, 1000) * 365.25 * 86400000);

                Action method = () =>
                {
                    _ = dateTime - timeSpan;
                };

                ExecuteTest(method, "Com.DateTimeX.operator -(Com.DateTimeX, TimeSpan)");
            }

            // 类型转换

            {
                DateTime dateTime = _GetRandomDateTime();

                Action method = () =>
                {
                    _ = (Com.DateTimeX)dateTime;
                };

                ExecuteTest(method, "Com.DateTimeX.implicit operator Com.DateTimeX(System.DateTime)");
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
                Com.PointD pt1 = _GetRandomPointD();
                Com.PointD pt2 = _GetRandomPointD();
                double A;
                double B;
                double C;

                Action method = () =>
                {
                    Com.Geometry.CalcLineGeneralFunction(pt1, pt2, out A, out B, out C);
                };

                ExecuteTest(method, "Com.Geometry.CalcLineGeneralFunction(Com.PointD, Com.PointD, out double, out double, out double)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                double A = Com.Statistics.RandomDouble(-1E9, 1E9);
                double B = Com.Statistics.RandomDouble(-1E9, 1E9);
                double C = Com.Statistics.RandomDouble(-1E9, 1E9);

                Action method = () =>
                {
                    _ = Com.Geometry.GetDistanceBetweenPointAndLine(pt, A, B, C);
                };

                ExecuteTest(method, "Com.Geometry.GetDistanceBetweenPointAndLine(Com.PointD, double, double, double)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                Com.PointD pt1 = _GetRandomPointD();
                Com.PointD pt2 = _GetRandomPointD();

                Action method = () =>
                {
                    _ = Com.Geometry.GetDistanceBetweenPointAndLine(pt, pt1, pt2);
                };

                ExecuteTest(method, "Com.Geometry.GetDistanceBetweenPointAndLine(Com.PointD, Com.PointD, Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                Com.PointD pt1 = _GetRandomPointD();
                Com.PointD pt2 = _GetRandomPointD();

                Action method = () =>
                {
                    _ = Com.Geometry.GetFootPoint(pt, pt1, pt2);
                };

                ExecuteTest(method, "Com.Geometry.GetFootPoint(Com.PointD, Com.PointD, Com.PointD)");
            }

            // 角度

            {
                Com.PointD pt1 = _GetRandomPointD();
                Com.PointD pt2 = _GetRandomPointD();

                Action method = () =>
                {
                    _ = Com.Geometry.GetAngleOfTwoPoints(pt1, pt2);
                };

                ExecuteTest(method, "Com.Geometry.GetAngleOfTwoPoints(Com.PointD, Com.PointD)");
            }

            {
                double angle = Com.Statistics.RandomDouble(-1E9, 1E9);

                Action method = () =>
                {
                    _ = Com.Geometry.AngleMapping(angle);
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
                    _ = Com.Geometry.PointIsVisibleInRectangle(pt, rect);
                };

                ExecuteTest(method, "Com.Geometry.PointIsVisibleInRectangle(Com.PointD, System.Drawing.RectangleF)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                Com.PointD offset = _GetRandomPointD();
                double radius = Com.Statistics.RandomDouble(1E9);

                Action method = () =>
                {
                    _ = Com.Geometry.PointIsVisibleInCircle(pt, offset, radius);
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
                    _ = Com.Geometry.PointIsVisibleInEllipse(pt, offset, semiMajorAxis, eccentricity, rotateAngle);
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
                    _ = Com.Geometry.PointIsVisibleInRhombus(pt, offset, semiMajorAxis, semiMinorAxis, rotateAngle);
                };

                ExecuteTest(method, "Com.Geometry.PointIsVisibleInRhombus(Com.PointD, Com.PointD, double, double, double)");
            }

            {
                Com.PointD pt1 = _GetRandomPointD();
                Com.PointD pt2 = _GetRandomPointD();
                RectangleF rect = _GetRandomRectangleF();

                Action method = () =>
                {
                    _ = Com.Geometry.LineIsVisibleInRectangle(pt1, pt2, rect);
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
                    _ = Com.Geometry.LineIsVisibleInCircle(pt1, pt2, offset, radius);
                };

                ExecuteTest(method, "Com.Geometry.LineIsVisibleInCircle(Com.PointD, Com.PointD, Com.PointD, double)");
            }

            {
                Com.PointD offset = _GetRandomPointD();
                double radius = Com.Statistics.RandomDouble(1E9);
                RectangleF rect = _GetRandomRectangleF();

                Action method = () =>
                {
                    _ = Com.Geometry.CircleInnerIsVisibleInRectangle(offset, radius, rect);
                };

                ExecuteTest(method, "Com.Geometry.CircleInnerIsVisibleInRectangle(Com.PointD, double, System.Drawing.RectangleF)");
            }

            {
                Com.PointD offset = _GetRandomPointD();
                double radius = Com.Statistics.RandomDouble(1E9);
                RectangleF rect = _GetRandomRectangleF();

                Action method = () =>
                {
                    _ = Com.Geometry.CircumferenceIsVisibleInRectangle(offset, radius, rect);
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
                    _ = Com.Geometry.GetRadiusOfEllipse(semiMajorAxis, eccentricity, phase);
                };

                ExecuteTest(method, "Com.Geometry.GetRadiusOfEllipse(double, double, double)");
            }

            {
                double semiMajorAxis = Com.Statistics.RandomDouble(1E9);
                double eccentricity = Com.Statistics.RandomDouble();
                double phase = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.Geometry.GetFocalRadiusOfEllipse(semiMajorAxis, eccentricity, phase);
                };

                ExecuteTest(method, "Com.Geometry.GetFocalRadiusOfEllipse(double, double, double)");
            }

            {
                double centralAngle = Com.Statistics.RandomDouble(2 * Math.PI);
                double eccentricity = Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    _ = Com.Geometry.EllipseCentralAngleToPhase(centralAngle, eccentricity);
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
                    _ = Com.Geometry.RotateBitmap(bmp, rotateAngle, antiAlias);
                };

                ExecuteTest(method, "Com.Geometry.RotateBitmap(System.Drawing.Bitmap, double, bool)", "bmp at 1024x1024 pixels, enable antiAlias");
            }

            // 路径

            {
                Rectangle rect = _GetRandomRectangle();
                int cornerRadius = Com.Statistics.RandomInteger(32768);

                Action method = () =>
                {
                    _ = Com.Geometry.CreateRoundedRectanglePath(rect, cornerRadius);
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
                    _ = Com.Geometry.CreateRoundedRectanglePath(rect, cornerRadiusLT, cornerRadiusRT, cornerRadiusRB, cornerRadiusLB);
                };

                ExecuteTest(method, "Com.Geometry.CreateRoundedRectanglePath(System.Drawing.Rectangle, int, int, int, int)");
            }

            {
                Rectangle rect = _GetRandomRectangle();
                int cornerRadius = Com.Statistics.RandomInteger(32768);

                Action method = () =>
                {
                    _ = Com.Geometry.CreateRoundedRectangleOuterPaths(rect, cornerRadius);
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
                    _ = Com.Geometry.CreateRoundedRectangleOuterPaths(rect, cornerRadiusLT, cornerRadiusRT, cornerRadiusRB, cornerRadiusLB);
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
                return Com.Matrix.Empty;
            }
        }

        //

        protected override void Constructor()
        {
            {
                Size size = new Size(32, 32);

                Action method = () =>
                {
                    _ = new Com.Matrix(size);
                };

                ExecuteTest(method, "Com.Matrix.Matrix(System.Drawing.Size)", "size at 32x32");
            }

            {
                Size size = new Size(32, 32);
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = new Com.Matrix(size, value);
                };

                ExecuteTest(method, "Com.Matrix.Matrix(System.Drawing.Size, double)", "size at 32x32");
            }

            {
                int width = 32;
                int height = 32;

                Action method = () =>
                {
                    _ = new Com.Matrix(width, height);
                };

                ExecuteTest(method, "Com.Matrix.Matrix(int, int)", "size at 32x32");
            }

            {
                int width = 32;
                int height = 32;
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = new Com.Matrix(width, height, value);
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
                    _ = new Com.Matrix(values);
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
                    _ = matrix[x, y];
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
                    _ = matrix[index];
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
                    _ = matrix.IsEmpty;
                };

                ExecuteTest(method, "Com.Matrix.IsEmpty.get()", "size at 32x32");
            }

            // Size

            {
                Com.Matrix matrix = new Com.Matrix(32, 32);

                Action method = () =>
                {
                    _ = matrix.Size;
                };

                ExecuteTest(method, "Com.Matrix.Size.get()", "size at 32x32");
            }

            {
                Com.Matrix matrix = new Com.Matrix(32, 32);

                Action method = () =>
                {
                    _ = matrix.Width;
                };

                ExecuteTest(method, "Com.Matrix.Width.get()", "size at 32x32");
            }

            {
                Com.Matrix matrix = new Com.Matrix(32, 32);

                Action method = () =>
                {
                    _ = matrix.Column;
                };

                ExecuteTest(method, "Com.Matrix.Column.get()", "size at 32x32");
            }

            {
                Com.Matrix matrix = new Com.Matrix(32, 32);

                Action method = () =>
                {
                    _ = matrix.Height;
                };

                ExecuteTest(method, "Com.Matrix.Height.get()", "size at 32x32");
            }

            {
                Com.Matrix matrix = new Com.Matrix(32, 32);

                Action method = () =>
                {
                    _ = matrix.Row;
                };

                ExecuteTest(method, "Com.Matrix.Row.get()", "size at 32x32");
            }

            {
                Com.Matrix matrix = new Com.Matrix(32, 32);

                Action method = () =>
                {
                    _ = matrix.Count;
                };

                ExecuteTest(method, "Com.Matrix.Count.get()", "size at 32x32");
            }

            // 线性代数属性

            {
                Com.Matrix matrix = _GetRandomMatrix(8, 8);

                Action method = () =>
                {
                    _ = matrix.Determinant;
                };

                ExecuteTest(method, "Com.Matrix.Determinant.get()", "size at 8x8");
            }

            {
                Com.Matrix matrix = _GetRandomMatrix(8, 8);

                Action method = () =>
                {
                    _ = matrix.Rank;
                };

                ExecuteTest(method, "Com.Matrix.Rank.get()", "size at 8x8");
            }

            {
                Com.Matrix matrix = _GetRandomMatrix(8, 8);

                Action method = () =>
                {
                    _ = matrix.Transport;
                };

                ExecuteTest(method, "Com.Matrix.Transport.get()", "size at 8x8");
            }

            {
                Com.Matrix matrix = _GetRandomMatrix(8, 8);

                Action method = () =>
                {
                    _ = matrix.Adjoint;
                };

                ExecuteTest(method, "Com.Matrix.Adjoint.get()", "size at 8x8");
            }

            {
                Com.Matrix matrix = _GetRandomMatrix(8, 8);

                Action method = () =>
                {
                    _ = matrix.Invert;
                };

                ExecuteTest(method, "Com.Matrix.Invert.get()", "size at 8x8");
            }
        }

        protected override void StaticProperty()
        {
            // Empty

            {
                Action method = () =>
                {
                    _ = Com.Matrix.Empty;
                };

                ExecuteTest(method, "Com.Matrix.Empty.get()");
            }
        }

        protected override void Method()
        {
            // object

            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);
                object obj = matrix.Copy();

                Action method = () =>
                {
                    _ = matrix.Equals(obj);
                };

                ExecuteTest(method, "Com.Matrix.Equals(object)", "size at 32x32");
            }

            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);

                Action method = () =>
                {
                    _ = matrix.GetHashCode();
                };

                ExecuteTest(method, "Com.Matrix.GetHashCode()", "size at 32x32");
            }

            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);

                Action method = () =>
                {
                    _ = matrix.ToString();
                };

                ExecuteTest(method, "Com.Matrix.ToString()", "size at 32x32");
            }

            // Equals

            {
                Com.Matrix left = _GetRandomMatrix(32, 32);
                Com.Matrix right = left.Copy();

                Action method = () =>
                {
                    _ = left.Equals(right);
                };

                ExecuteTest(method, "Com.Matrix.Equals(Com.Matrix)", "size at 32x32");
            }

            // Copy

            {
                Com.Matrix matrix = new Com.Matrix(32, 32);

                Action method = () =>
                {
                    _ = matrix.Copy();
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
                    _ = matrix.SubMatrix(index, size);
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
                    _ = matrix.SubMatrix(x, y, width, height);
                };

                ExecuteTest(method, "Com.Matrix.SubMatrix(int, int, int, int)", "size at 32x32, SubMatrix is 16x16 at (8,8)");
            }

            // 获取行列

            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);
                int x = Com.Statistics.RandomInteger(32);

                Action method = () =>
                {
                    _ = matrix.GetColumn(x);
                };

                ExecuteTest(method, "Com.Matrix.GetColumn(int)", "size at 32x32");
            }

            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);
                int y = Com.Statistics.RandomInteger(32);

                Action method = () =>
                {
                    _ = matrix.GetRow(y);
                };

                ExecuteTest(method, "Com.Matrix.GetRow(int)", "size at 32x32");
            }

            // ToArray

            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);

                Action method = () =>
                {
                    _ = matrix.ToArray();
                };

                ExecuteTest(method, "Com.Matrix.ToArray()", "size at 32x32");
            }
        }

        protected override void StaticMethod()
        {
            // IsNullOrEmpty

            {
                Com.Matrix matrix = new Com.Matrix(32, 32);

                Action method = () =>
                {
                    _ = Com.Matrix.IsNullOrEmpty(matrix);
                };

                ExecuteTest(method, "Com.Matrix.IsNullOrEmpty(Com.Matrix)", "size at 32x32");
            }

            // Equals

            {
                Com.Matrix left = _GetRandomMatrix(32, 32);
                Com.Matrix right = left.Copy();

                Action method = () =>
                {
                    _ = Com.Matrix.Equals(left, right);
                };

                ExecuteTest(method, "Com.Matrix.Equals(Com.Matrix, Com.Matrix)", "size at 32x32");
            }

            // 矩阵生成

            {
                int order = 32;

                Action method = () =>
                {
                    _ = Com.Matrix.Identity(order);
                };

                ExecuteTest(method, "Com.Matrix.Identity(int)", "size at 32x32");
            }

            {
                Size size = new Size(32, 32);

                Action method = () =>
                {
                    _ = Com.Matrix.Zeros(size);
                };

                ExecuteTest(method, "Com.Matrix.Zeros(System.Drawing.Size)", "size at 32x32");
            }

            {
                int width = 32;
                int height = 32;

                Action method = () =>
                {
                    _ = Com.Matrix.Zeros(width, height);
                };

                ExecuteTest(method, "Com.Matrix.Zeros(int, int)", "size at 32x32");
            }

            {
                Size size = new Size(32, 32);

                Action method = () =>
                {
                    _ = Com.Matrix.Ones(size);
                };

                ExecuteTest(method, "Com.Matrix.Ones(System.Drawing.Size)", "size at 32x32");
            }

            {
                int width = 32;
                int height = 32;

                Action method = () =>
                {
                    _ = Com.Matrix.Ones(width, height);
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
                    _ = Com.Matrix.Diagonal(array, rowsUponMainDiag);
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
                    _ = Com.Matrix.Diagonal(array);
                };

                ExecuteTest(method, "Com.Matrix.Diagonal(double[])", "size at 32x32");
            }

            // 增广矩阵

            {
                Com.Matrix left = _GetRandomMatrix(32, 32);
                Com.Matrix right = _GetRandomMatrix(32, 32);

                Action method = () =>
                {
                    _ = Com.Matrix.Augment(left, right);
                };

                ExecuteTest(method, "Com.Matrix.Augment(Com.Matrix, Com.Matrix)", "size at 32x32");
            }

            // 线性代数运算

            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.Matrix.Add(matrix, n);
                };

                ExecuteTest(method, "Com.Matrix.Add(Com.Matrix, double)", "size at 32x32");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.Matrix matrix = _GetRandomMatrix(32, 32);

                Action method = () =>
                {
                    _ = Com.Matrix.Add(n, matrix);
                };

                ExecuteTest(method, "Com.Matrix.Add(double, Com.Matrix)", "size at 32x32");
            }

            {
                Com.Matrix left = _GetRandomMatrix(32, 32);
                Com.Matrix right = _GetRandomMatrix(32, 32);

                Action method = () =>
                {
                    _ = Com.Matrix.Add(left, right);
                };

                ExecuteTest(method, "Com.Matrix.Add(Com.Matrix, Com.Matrix)", "size at 32x32");
            }

            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.Matrix.Subtract(matrix, n);
                };

                ExecuteTest(method, "Com.Matrix.Subtract(Com.Matrix, double)", "size at 32x32");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.Matrix matrix = _GetRandomMatrix(32, 32);

                Action method = () =>
                {
                    _ = Com.Matrix.Subtract(n, matrix);
                };

                ExecuteTest(method, "Com.Matrix.Subtract(double, Com.Matrix)", "size at 32x32");
            }

            {
                Com.Matrix left = _GetRandomMatrix(32, 32);
                Com.Matrix right = _GetRandomMatrix(32, 32);

                Action method = () =>
                {
                    _ = Com.Matrix.Subtract(left, right);
                };

                ExecuteTest(method, "Com.Matrix.Subtract(Com.Matrix, Com.Matrix)", "size at 32x32");
            }

            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.Matrix.Multiply(matrix, n);
                };

                ExecuteTest(method, "Com.Matrix.Multiply(Com.Matrix, double)", "size at 32x32");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.Matrix matrix = _GetRandomMatrix(32, 32);

                Action method = () =>
                {
                    _ = Com.Matrix.Multiply(n, matrix);
                };

                ExecuteTest(method, "Com.Matrix.Multiply(double, Com.Matrix)", "size at 32x32");
            }

            {
                Com.Matrix left = _GetRandomMatrix(32, 32);
                Com.Matrix right = _GetRandomMatrix(32, 32);

                Action method = () =>
                {
                    _ = Com.Matrix.Multiply(left, right);
                };

                ExecuteTest(method, "Com.Matrix.Multiply(Com.Matrix, Com.Matrix)", "size at 32x32");
            }

            {
                List<Com.Matrix> list = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    list.Add(_GetRandomMatrix(32, 32));
                }

                Action method = () =>
                {
                    _ = Com.Matrix.MultiplyLeft(list);
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
                    _ = Com.Matrix.MultiplyRight(list);
                };

                ExecuteTest(method, "Com.Matrix.MultiplyRight(System.Collections.Generic.List<Com.Matrix>)", "size at 32x32, total 8 matrices");
            }

            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.Matrix.Divide(matrix, n);
                };

                ExecuteTest(method, "Com.Matrix.Divide(Com.Matrix, double)", "size at 32x32");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.Matrix matrix = _GetRandomMatrix(32, 32);

                Action method = () =>
                {
                    _ = Com.Matrix.Divide(n, matrix);
                };

                ExecuteTest(method, "Com.Matrix.Divide(double, Com.Matrix)", "size at 32x32");
            }

            {
                Com.Matrix left = _GetRandomMatrix(8, 8);
                Com.Matrix right = _GetRandomMatrix(8, 8);

                Action method = () =>
                {
                    _ = Com.Matrix.DivideLeft(left, right);
                };

                ExecuteTest(method, "Com.Matrix.DivideLeft(Com.Matrix, Com.Matrix)", "size at 8x8");
            }

            {
                Com.Matrix left = _GetRandomMatrix(8, 8);
                Com.Matrix right = _GetRandomMatrix(8, 8);

                Action method = () =>
                {
                    _ = Com.Matrix.DivideRight(left, right);
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
                    _ = Com.Matrix.SolveLinearEquation(matrix, vector);
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
                    _ = (left == right);
                };

                ExecuteTest(method, "Com.Matrix.operator ==(Com.Matrix, Com.Matrix)", "size at 32x32");
            }

            {
                Com.Matrix left = _GetRandomMatrix(32, 32);
                Com.Matrix right = left.Copy();

                Action method = () =>
                {
                    _ = (left != right);
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
                    _ = Com.Painting2D.PaintLine(bmp, pt1, pt2, color, width, antiAlias);
                };

                ExecuteTest(method, "Com.Painting2D.PaintLine(System.Drawing.Bitmap, Com.PointD, Com.PointD, System.Drawing.Color, float, bool)", "bmp at 1024x1024 pixels, width at 1.0F, enable antiAlias");
            }

            {
                Bitmap bmp = new Bitmap(1024, 1024);
                Com.PointD offset = new Com.PointD(bmp.Size) / 2;
                double radius = new Com.PointD(bmp.Size).Module / 2;
                double deltaRadius = radius / 9;
                int normalIncreasePeriod = 3;
                Color color = Com.ColorManipulation.GetRandomColor();
                bool antiAlias = true;

                Action method = () =>
                {
                    _ = Com.Painting2D.PaintPolarGrid(bmp, offset, radius, deltaRadius, normalIncreasePeriod, color, antiAlias);
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
                    _ = Com.Painting2D.PaintCircle(bmp, offset, radius, color, width, antiAlias);
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
                    _ = Com.Painting2D.PaintLargeCircle(bmp, offset, radius, refPhase, color, width, antiAlias, minDiv, maxDiv, divArc);
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
                    _ = Com.Painting2D.PaintLargeCircle(bmp, offset, radius, refPhase, color, width, antiAlias);
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
                    _ = Com.Painting2D.PaintLargeEllipse(bmp, offset, semiMajorAxis, eccentricity, rotateAngle, refPhase, color, width, antiAlias, minDiv, maxDiv, divArc);
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
                    _ = Com.Painting2D.PaintLargeEllipse(bmp, offset, semiMajorAxis, eccentricity, rotateAngle, refPhase, color, width, antiAlias);
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
                    _ = Com.Painting2D.PaintTextWithShadow(bmp, text, font, frontColor, backColor, pt, offset, antiAlias);
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
                    _ = Com.Painting3D.PaintCuboid(bmp, center, size, color, edgeWidth, affineMatrixList, trueLenDist, illuminationDirection, illuminationDirectionIsAfterAffineTransform, exposure, antiAlias);
                };

                ExecuteTest(method, "Com.Painting3D.PaintCuboid(System.Drawing.Bitmap, Com.PointD3D, Com.PointD3D, System.Drawing.Color, float, System.Collections.Generic.List<Com.Matrix>, double, Com.PointD3D, bool, double, bool)", "bmp at 1024x1024 pixels, cuboid size at 512x512x512, edgeWidth at 1.0F, total 4 matrices, trueLenDist at 1024, enable antiAlias");
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
                    _ = Com.Painting3D.PaintCuboid(bmp, center, size, color, edgeWidth, affineMatrix, trueLenDist, illuminationDirection, illuminationDirectionIsAfterAffineTransform, exposure, antiAlias);
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
                    _ = Com.Painting3D.PaintCuboid(bmp, center, size, color, edgeWidth, affineMatrixList, trueLenDist, antiAlias);
                };

                ExecuteTest(method, "Com.Painting3D.PaintCuboid(System.Drawing.Bitmap, Com.PointD3D, Com.PointD3D, System.Drawing.Color, float, System.Collections.Generic.List<Com.Matrix>, double, bool)", "bmp at 1024x1024 pixels, cuboid size at 512x512x512, edgeWidth at 1.0F, total 4 matrices, trueLenDist at 1024, enable antiAlias");
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
                    _ = Com.Painting3D.PaintCuboid(bmp, center, size, color, edgeWidth, affineMatrix, trueLenDist, antiAlias);
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
                return Com.Matrix.Empty;
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
                    _ = new Com.PointD(x, y);
                };

                ExecuteTest(method, "Com.PointD.PointD(double, double)");
            }

            {
                Point pt = new Point(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = new Com.PointD(pt);
                };

                ExecuteTest(method, "Com.PointD.PointD(System.Drawing.Point)");
            }

            {
                PointF pt = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = new Com.PointD(pt);
                };

                ExecuteTest(method, "Com.PointD.PointD(System.Drawing.PointF)");
            }

            {
                Size sz = new Size(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = new Com.PointD(sz);
                };

                ExecuteTest(method, "Com.PointD.PointD(System.Drawing.Size)");
            }

            {
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = new Com.PointD(sz);
                };

                ExecuteTest(method, "Com.PointD.PointD(System.Drawing.SizeF)");
            }

            {
                Com.Complex comp = new Com.Complex(Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = new Com.PointD(comp);
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
                    _ = pointD[index];
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

            // 分量

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.X;
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
                    _ = pointD.Y;
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

            // Dimension

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.Dimension;
                };

                ExecuteTest(method, "Com.PointD.Dimension.get()");
            }

            // Is

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.IsEmpty;
                };

                ExecuteTest(method, "Com.PointD.IsEmpty.get()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.IsZero;
                };

                ExecuteTest(method, "Com.PointD.IsZero.get()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.IsReadOnly;
                };

                ExecuteTest(method, "Com.PointD.IsReadOnly.get()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.IsFixedSize;
                };

                ExecuteTest(method, "Com.PointD.IsFixedSize.get()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.IsNaN;
                };

                ExecuteTest(method, "Com.PointD.IsNaN.get()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.IsInfinity;
                };

                ExecuteTest(method, "Com.PointD.IsInfinity.get()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.IsNaNOrInfinity;
                };

                ExecuteTest(method, "Com.PointD.IsNaNOrInfinity.get()");
            }

            // 模

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.Module;
                };

                ExecuteTest(method, "Com.PointD.Module.get()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.ModuleSquared;
                };

                ExecuteTest(method, "Com.PointD.ModuleSquared.get()");
            }

            // 向量

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.Opposite;
                };

                ExecuteTest(method, "Com.PointD.Opposite.get()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.Normalize;
                };

                ExecuteTest(method, "Com.PointD.Normalize.get()");
            }

            // 角度

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.AngleFromX;
                };

                ExecuteTest(method, "Com.PointD.AngleFromX.get()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.AngleFromY;
                };

                ExecuteTest(method, "Com.PointD.AngleFromY.get()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.Azimuth;
                };

                ExecuteTest(method, "Com.PointD.Azimuth.get()");
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
                object obj = pointD;

                Action method = () =>
                {
                    _ = pointD.Equals(obj);
                };

                ExecuteTest(method, "Com.PointD.Equals(object)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.GetHashCode();
                };

                ExecuteTest(method, "Com.PointD.GetHashCode()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.ToString();
                };

                ExecuteTest(method, "Com.PointD.ToString()");
            }

            // Equals

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = left;

                Action method = () =>
                {
                    _ = left.Equals(right);
                };

                ExecuteTest(method, "Com.PointD.Equals(Com.PointD)");
            }

            // CompareTo

            {
                Com.PointD pointD = _GetRandomPointD();
                object obj = pointD;

                Action method = () =>
                {
                    _ = pointD.CompareTo(obj);
                };

                ExecuteTest(method, "Com.PointD.CompareTo(object)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = left;

                Action method = () =>
                {
                    _ = left.CompareTo(right);
                };

                ExecuteTest(method, "Com.PointD.CompareTo(Com.PointD)");
            }

            // 检索

            {
                Com.PointD pointD = _GetRandomPointD();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD.IndexOf(item);
                };

                ExecuteTest(method, "Com.PointD.IndexOf(double)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 0;

                Action method = () =>
                {
                    _ = pointD.IndexOf(item, startIndex);
                };

                ExecuteTest(method, "Com.PointD.IndexOf(double, int)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 0;
                int count = 2;

                Action method = () =>
                {
                    _ = pointD.IndexOf(item, startIndex, count);
                };

                ExecuteTest(method, "Com.PointD.IndexOf(double, int, int)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD.LastIndexOf(item);
                };

                ExecuteTest(method, "Com.PointD.LastIndexOf(double)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 1;

                Action method = () =>
                {
                    _ = pointD.LastIndexOf(item, startIndex);
                };

                ExecuteTest(method, "Com.PointD.LastIndexOf(double, int)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 1;
                int count = 2;

                Action method = () =>
                {
                    _ = pointD.LastIndexOf(item, startIndex, count);
                };

                ExecuteTest(method, "Com.PointD.LastIndexOf(double, int, int)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD.Contains(item);
                };

                ExecuteTest(method, "Com.PointD.Contains(double)");
            }

            // ToArray，ToList

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.ToArray();
                };

                ExecuteTest(method, "Com.PointD.ToArray()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.ToList();
                };

                ExecuteTest(method, "Com.PointD.ToList()");
            }

            // 坐标系转换

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.ToSpherical();
                };

                ExecuteTest(method, "Com.PointD.ToSpherical()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.ToCartesian();
                };

                ExecuteTest(method, "Com.PointD.ToCartesian()");
            }

            // 距离与夹角

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.DistanceFrom(pt);
                };

                ExecuteTest(method, "Com.PointD.DistanceFrom(Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.AngleFrom(pt);
                };

                ExecuteTest(method, "Com.PointD.AngleFrom(Com.PointD)");
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
                Point pt = new Point(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

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
                Size sz = new Size(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

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
                    _ = pointD.OffsetCopy(d);
                };

                ExecuteTest(method, "Com.PointD.OffsetCopy(double)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                double dx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dy = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD.OffsetCopy(dx, dy);
                };

                ExecuteTest(method, "Com.PointD.OffsetCopy(double, double)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.OffsetCopy(pt);
                };

                ExecuteTest(method, "Com.PointD.OffsetCopy(Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Point pt = new Point(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = pointD.OffsetCopy(pt);
                };

                ExecuteTest(method, "Com.PointD.OffsetCopy(System.Drawing.Point)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                PointF pt = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = pointD.OffsetCopy(pt);
                };

                ExecuteTest(method, "Com.PointD.OffsetCopy(System.Drawing.PointF)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Size sz = new Size(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = pointD.OffsetCopy(sz);
                };

                ExecuteTest(method, "Com.PointD.OffsetCopy(System.Drawing.Size)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = pointD.OffsetCopy(sz);
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
                Point pt = new Point(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

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
                Size sz = new Size(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

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
                    _ = pointD.ScaleCopy(s);
                };

                ExecuteTest(method, "Com.PointD.ScaleCopy(double)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                double sx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sy = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD.ScaleCopy(sx, sy);
                };

                ExecuteTest(method, "Com.PointD.ScaleCopy(double, double)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.ScaleCopy(pt);
                };

                ExecuteTest(method, "Com.PointD.ScaleCopy(Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Point pt = new Point(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = pointD.ScaleCopy(pt);
                };

                ExecuteTest(method, "Com.PointD.ScaleCopy(System.Drawing.Point)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                PointF pt = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = pointD.ScaleCopy(pt);
                };

                ExecuteTest(method, "Com.PointD.ScaleCopy(System.Drawing.PointF)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Size sz = new Size(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = pointD.ScaleCopy(sz);
                };

                ExecuteTest(method, "Com.PointD.ScaleCopy(System.Drawing.Size)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = pointD.ScaleCopy(sz);
                };

                ExecuteTest(method, "Com.PointD.ScaleCopy(System.Drawing.SizeF)");
            }

            // Reflect

            {
                Com.PointD pointD = _GetRandomPointD();
                int index = Com.Statistics.RandomInteger(2);

                Action method = () =>
                {
                    pointD.Reflect(index);
                };

                ExecuteTest(method, "Com.PointD.Reflect(int)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    pointD.ReflectX();
                };

                ExecuteTest(method, "Com.PointD.ReflectX()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    pointD.ReflectY();
                };

                ExecuteTest(method, "Com.PointD.ReflectY()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                int index = Com.Statistics.RandomInteger(2);

                Action method = () =>
                {
                    _ = pointD.ReflectCopy(index);
                };

                ExecuteTest(method, "Com.PointD.ReflectCopy(int)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.ReflectXCopy();
                };

                ExecuteTest(method, "Com.PointD.ReflectXCopy()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.ReflectYCopy();
                };

                ExecuteTest(method, "Com.PointD.ReflectYCopy()");
            }

            // Shear

            {
                Com.PointD pointD = _GetRandomPointD();
                int index1 = Com.Statistics.RandomInteger(2);
                int index2 = 1 - index1;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD.Shear(index1, index2, angle);
                };

                ExecuteTest(method, "Com.PointD.Shear(int, int, double)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD.ShearX(angle);
                };

                ExecuteTest(method, "Com.PointD.ShearX(double)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD.ShearY(angle);
                };

                ExecuteTest(method, "Com.PointD.ShearY(double)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                int index1 = Com.Statistics.RandomInteger(2);
                int index2 = 1 - index1;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD.ShearCopy(index1, index2, angle);
                };

                ExecuteTest(method, "Com.PointD.ShearCopy(int, int, double)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD.ShearXCopy(angle);
                };

                ExecuteTest(method, "Com.PointD.ShearXCopy(double)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD.ShearYCopy(angle);
                };

                ExecuteTest(method, "Com.PointD.ShearYCopy(double)");
            }

            // Rotate

            {
                Com.PointD pointD = _GetRandomPointD();
                int index1 = Com.Statistics.RandomInteger(2);
                int index2 = 1 - index1;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD.Rotate(index1, index2, angle);
                };

                ExecuteTest(method, "Com.PointD.Rotate(int, int, double)");
            }

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
                int index1 = Com.Statistics.RandomInteger(2);
                int index2 = 1 - index1;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD.RotateCopy(index1, index2, angle);
                };

                ExecuteTest(method, "Com.PointD.RotateCopy(int, int, double)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD.RotateCopy(angle);
                };

                ExecuteTest(method, "Com.PointD.RotateCopy(double)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.PointD pt = _GetRandomPointD();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD.RotateCopy(angle, pt);
                };

                ExecuteTest(method, "Com.PointD.RotateCopy(double, Com.PointD)");
            }

            // Affine

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.PointD ex = _GetRandomPointD().Normalize;
                Com.PointD ey = _GetRandomPointD().Normalize;
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
                Com.PointD ex = _GetRandomPointD().Normalize;
                Com.PointD ey = _GetRandomPointD().Normalize;
                Com.PointD offset = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.AffineTransformCopy(ex, ey, offset);
                };

                ExecuteTest(method, "Com.PointD.AffineTransformCopy(Com.PointD, Com.PointD, Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.Matrix matrixLeft = _GetRandomMatrix(3, 3);

                Action method = () =>
                {
                    _ = pointD.AffineTransformCopy(matrixLeft);
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
                    _ = pointD.AffineTransformCopy(matrixLeftList);
                };

                ExecuteTest(method, "Com.PointD.AffineTransformCopy(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.PointD ex = _GetRandomPointD().Normalize;
                Com.PointD ey = _GetRandomPointD().Normalize;
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
                Com.PointD ex = _GetRandomPointD().Normalize;
                Com.PointD ey = _GetRandomPointD().Normalize;
                Com.PointD offset = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.InverseAffineTransformCopy(ex, ey, offset);
                };

                ExecuteTest(method, "Com.PointD.InverseAffineTransformCopy(Com.PointD, Com.PointD, Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.Matrix matrixLeft = _GetRandomMatrix(3, 3);

                Action method = () =>
                {
                    _ = pointD.InverseAffineTransformCopy(matrixLeft);
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
                    _ = pointD.InverseAffineTransformCopy(matrixLeftList);
                };

                ExecuteTest(method, "Com.PointD.InverseAffineTransformCopy(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            // ToVector

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.ToColumnVector();
                };

                ExecuteTest(method, "Com.PointD.ToColumnVector()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.ToRowVector();
                };

                ExecuteTest(method, "Com.PointD.ToRowVector()");
            }

            // To

            {
                Com.PointD pointD = new Com.PointD(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = pointD.ToPoint();
                };

                ExecuteTest(method, "Com.PointD.ToPoint()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.ToPointF();
                };

                ExecuteTest(method, "Com.PointD.ToPointF()");
            }

            {
                Com.PointD pointD = new Com.PointD(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = pointD.ToSize();
                };

                ExecuteTest(method, "Com.PointD.ToSize()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.ToSizeF();
                };

                ExecuteTest(method, "Com.PointD.ToSizeF()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.ToComplex();
                };

                ExecuteTest(method, "Com.PointD.ToComplex()");
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
                    _ = Com.PointD.Equals(left, right);
                };

                ExecuteTest(method, "Com.PointD.Equals(Com.PointD, Com.PointD)");
            }

            // Compare

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = left;

                Action method = () =>
                {
                    _ = Com.PointD.Compare(left, right);
                };

                ExecuteTest(method, "Com.PointD.Compare(Com.PointD, Com.PointD)");
            }

            // From

            {
                Point pt = new Point(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = Com.PointD.FromPoint(pt);
                };

                ExecuteTest(method, "Com.PointD.FromPoint(System.Drawing.Point)");
            }

            {
                PointF pt = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = Com.PointD.FromPointF(pt);
                };

                ExecuteTest(method, "Com.PointD.FromPointF(System.Drawing.PointF)");
            }

            {
                Size sz = new Size(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = Com.PointD.FromSize(sz);
                };

                ExecuteTest(method, "Com.PointD.FromSize(System.Drawing.Size)");
            }

            {
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = Com.PointD.FromSizeF(sz);
                };

                ExecuteTest(method, "Com.PointD.FromSizeF(System.Drawing.SizeF)");
            }

            {
                Com.Complex comp = new Com.Complex(Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = Com.PointD.FromComplex(comp);
                };

                ExecuteTest(method, "Com.PointD.FromComplex(Com.Complex)");
            }

            // Matrix

            {
                Action method = () =>
                {
                    _ = Com.PointD.IdentityMatrix();
                };

                ExecuteTest(method, "Com.PointD.IdentityMatrix()");
            }

            {
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD.OffsetMatrix(d);
                };

                ExecuteTest(method, "Com.PointD.OffsetMatrix(double)");
            }

            {
                double dx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dy = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD.OffsetMatrix(dx, dy);
                };

                ExecuteTest(method, "Com.PointD.OffsetMatrix(double, double)");
            }

            {
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = Com.PointD.OffsetMatrix(pt);
                };

                ExecuteTest(method, "Com.PointD.OffsetMatrix(Com.PointD)");
            }

            {
                Point pt = new Point(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = Com.PointD.OffsetMatrix(pt);
                };

                ExecuteTest(method, "Com.PointD.OffsetMatrix(System.Drawing.Point)");
            }

            {
                PointF pt = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = Com.PointD.OffsetMatrix(pt);
                };

                ExecuteTest(method, "Com.PointD.OffsetMatrix(System.Drawing.PointF)");
            }

            {
                Size sz = new Size(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = Com.PointD.OffsetMatrix(sz);
                };

                ExecuteTest(method, "Com.PointD.OffsetMatrix(System.Drawing.Size)");
            }

            {
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = Com.PointD.OffsetMatrix(sz);
                };

                ExecuteTest(method, "Com.PointD.OffsetMatrix(System.Drawing.SizeF)");
            }

            {
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD.ScaleMatrix(s);
                };

                ExecuteTest(method, "Com.PointD.ScaleMatrix(double)");
            }

            {
                double sx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sy = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD.ScaleMatrix(sx, sy);
                };

                ExecuteTest(method, "Com.PointD.ScaleMatrix(double, double)");
            }

            {
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = Com.PointD.ScaleMatrix(pt);
                };

                ExecuteTest(method, "Com.PointD.ScaleMatrix(Com.PointD)");
            }

            {
                Point pt = new Point(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = Com.PointD.ScaleMatrix(pt);
                };

                ExecuteTest(method, "Com.PointD.ScaleMatrix(System.Drawing.Point)");
            }

            {
                PointF pt = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = Com.PointD.ScaleMatrix(pt);
                };

                ExecuteTest(method, "Com.PointD.ScaleMatrix(System.Drawing.PointF)");
            }

            {
                Size sz = new Size(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = Com.PointD.ScaleMatrix(sz);
                };

                ExecuteTest(method, "Com.PointD.ScaleMatrix(System.Drawing.Size)");
            }

            {
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = Com.PointD.ScaleMatrix(sz);
                };

                ExecuteTest(method, "Com.PointD.ScaleMatrix(System.Drawing.SizeF)");
            }

            {
                int index = Com.Statistics.RandomInteger(2);

                Action method = () =>
                {
                    _ = Com.PointD.ReflectMatrix(index);
                };

                ExecuteTest(method, "Com.PointD.ReflectMatrix(int)");
            }

            {
                Action method = () =>
                {
                    _ = Com.PointD.ReflectXMatrix();
                };

                ExecuteTest(method, "Com.PointD.ReflectXMatrix()");
            }

            {
                Action method = () =>
                {
                    _ = Com.PointD.ReflectYMatrix();
                };

                ExecuteTest(method, "Com.PointD.ReflectYMatrix()");
            }

            {
                int index1 = Com.Statistics.RandomInteger(2);
                int index2 = 1 - index1;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD.ShearMatrix(index1, index2, angle);
                };

                ExecuteTest(method, "Com.PointD.ShearMatrix(int, int, double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD.ShearXMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD.ShearXMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD.ShearYMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD.ShearYMatrix(double)");
            }

            {
                int index1 = Com.Statistics.RandomInteger(2);
                int index2 = 1 - index1;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD.RotateMatrix(index1, index2, angle);
                };

                ExecuteTest(method, "Com.PointD.RotateMatrix(int, int, double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD.RotateMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD.RotateMatrix(double)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD.RotateMatrix(angle, pt);
                };

                ExecuteTest(method, "Com.PointD.RotateMatrix(double, Com.PointD)");
            }

            // 距离与夹角

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = Com.PointD.DistanceBetween(left, right);
                };

                ExecuteTest(method, "Com.PointD.DistanceBetween(Com.PointD, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = Com.PointD.AngleBetween(left, right);
                };

                ExecuteTest(method, "Com.PointD.AngleBetween(Com.PointD, Com.PointD)");
            }

            // 向量乘积

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = Com.PointD.DotProduct(left, right);
                };

                ExecuteTest(method, "Com.PointD.DotProduct(Com.PointD, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = Com.PointD.CrossProduct(left, right);
                };

                ExecuteTest(method, "Com.PointD.CrossProduct(Com.PointD, Com.PointD)");
            }

            // 初等函数

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = Com.PointD.Abs(pointD);
                };

                ExecuteTest(method, "Com.PointD.Abs(Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = Com.PointD.Sign(pointD);
                };

                ExecuteTest(method, "Com.PointD.Sign(Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = Com.PointD.Ceiling(pointD);
                };

                ExecuteTest(method, "Com.PointD.Ceiling(Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = Com.PointD.Floor(pointD);
                };

                ExecuteTest(method, "Com.PointD.Floor(Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = Com.PointD.Round(pointD);
                };

                ExecuteTest(method, "Com.PointD.Round(Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = Com.PointD.Truncate(pointD);
                };

                ExecuteTest(method, "Com.PointD.Truncate(Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = Com.PointD.Max(left, right);
                };

                ExecuteTest(method, "Com.PointD.Max(Com.PointD, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = Com.PointD.Min(left, right);
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
                    _ = (left == right);
                };

                ExecuteTest(method, "Com.PointD.operator ==(Com.PointD, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = left;

                Action method = () =>
                {
                    _ = (left != right);
                };

                ExecuteTest(method, "Com.PointD.operator !=(Com.PointD, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = (left < right);
                };

                ExecuteTest(method, "Com.PointD.operator <(Com.PointD, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = (left > right);
                };

                ExecuteTest(method, "Com.PointD.operator >(Com.PointD, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = (left <= right);
                };

                ExecuteTest(method, "Com.PointD.operator <=(Com.PointD, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = (left >= right);
                };

                ExecuteTest(method, "Com.PointD.operator >=(Com.PointD, Com.PointD)");
            }

            // 运算

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = +pointD;
                };

                ExecuteTest(method, "Com.PointD.operator +(Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = -pointD;
                };

                ExecuteTest(method, "Com.PointD.operator -(Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt + n;
                };

                ExecuteTest(method, "Com.PointD.operator +(Com.PointD, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = n + pt;
                };

                ExecuteTest(method, "Com.PointD.operator +(double, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = left + right;
                };

                ExecuteTest(method, "Com.PointD.operator +(Com.PointD, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Point right = new Point(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = left + right;
                };

                ExecuteTest(method, "Com.PointD.operator +(Com.PointD, System.Drawing.Point)");
            }

            {
                Point left = new Point(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = left + right;
                };

                ExecuteTest(method, "Com.PointD.operator +(System.Drawing.Point, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                PointF right = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = left + right;
                };

                ExecuteTest(method, "Com.PointD.operator +(Com.PointD, System.Drawing.PointF)");
            }

            {
                PointF left = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = left + right;
                };

                ExecuteTest(method, "Com.PointD.operator +(System.Drawing.PointF, Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                Size sz = new Size(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = pt + sz;
                };

                ExecuteTest(method, "Com.PointD.operator +(Com.PointD, System.Drawing.Size)");
            }

            {
                Size sz = new Size(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = sz + pt;
                };

                ExecuteTest(method, "Com.PointD.operator +(System.Drawing.Size, Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = pt + sz;
                };

                ExecuteTest(method, "Com.PointD.operator +(Com.PointD, System.Drawing.SizeF)");
            }

            {
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = sz + pt;
                };

                ExecuteTest(method, "Com.PointD.operator +(System.Drawing.SizeF, Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt - n;
                };

                ExecuteTest(method, "Com.PointD.operator -(Com.PointD, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = n - pt;
                };

                ExecuteTest(method, "Com.PointD.operator -(double, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = left - right;
                };

                ExecuteTest(method, "Com.PointD.operator -(Com.PointD, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Point right = new Point(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = left - right;
                };

                ExecuteTest(method, "Com.PointD.operator -(Com.PointD, System.Drawing.Point)");
            }

            {
                Point left = new Point(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = left - right;
                };

                ExecuteTest(method, "Com.PointD.operator -(System.Drawing.Point, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                PointF right = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = left - right;
                };

                ExecuteTest(method, "Com.PointD.operator -(Com.PointD, System.Drawing.PointF)");
            }

            {
                PointF left = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = left - right;
                };

                ExecuteTest(method, "Com.PointD.operator -(System.Drawing.PointF, Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                Size sz = new Size(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = pt - sz;
                };

                ExecuteTest(method, "Com.PointD.operator -(Com.PointD, System.Drawing.Size)");
            }

            {
                Size sz = new Size(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = sz - pt;
                };

                ExecuteTest(method, "Com.PointD.operator -(System.Drawing.Size, Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = pt - sz;
                };

                ExecuteTest(method, "Com.PointD.operator -(Com.PointD, System.Drawing.SizeF)");
            }

            {
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = sz - pt;
                };

                ExecuteTest(method, "Com.PointD.operator -(System.Drawing.SizeF, Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt * n;
                };

                ExecuteTest(method, "Com.PointD.operator *(Com.PointD, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = n * pt;
                };

                ExecuteTest(method, "Com.PointD.operator *(double, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = left * right;
                };

                ExecuteTest(method, "Com.PointD.operator *(Com.PointD, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Point right = new Point(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = left * right;
                };

                ExecuteTest(method, "Com.PointD.operator *(Com.PointD, System.Drawing.Point)");
            }

            {
                Point left = new Point(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = left * right;
                };

                ExecuteTest(method, "Com.PointD.operator *(System.Drawing.Point, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                PointF right = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = left * right;
                };

                ExecuteTest(method, "Com.PointD.operator *(Com.PointD, System.Drawing.PointF)");
            }

            {
                PointF left = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = left * right;
                };

                ExecuteTest(method, "Com.PointD.operator *(System.Drawing.PointF, Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                Size sz = new Size(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = pt * sz;
                };

                ExecuteTest(method, "Com.PointD.operator *(Com.PointD, System.Drawing.Size)");
            }

            {
                Size sz = new Size(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = sz * pt;
                };

                ExecuteTest(method, "Com.PointD.operator *(System.Drawing.Size, Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = pt * sz;
                };

                ExecuteTest(method, "Com.PointD.operator *(Com.PointD, System.Drawing.SizeF)");
            }

            {
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = sz * pt;
                };

                ExecuteTest(method, "Com.PointD.operator *(System.Drawing.SizeF, Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt / n;
                };

                ExecuteTest(method, "Com.PointD.operator /(Com.PointD, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = n / pt;
                };

                ExecuteTest(method, "Com.PointD.operator /(double, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = left / right;
                };

                ExecuteTest(method, "Com.PointD.operator /(Com.PointD, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Point right = new Point(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = left / right;
                };

                ExecuteTest(method, "Com.PointD.operator /(Com.PointD, System.Drawing.Point)");
            }

            {
                Point left = new Point(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = left / right;
                };

                ExecuteTest(method, "Com.PointD.operator /(System.Drawing.Point, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                PointF right = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = left / right;
                };

                ExecuteTest(method, "Com.PointD.operator /(Com.PointD, System.Drawing.PointF)");
            }

            {
                PointF left = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = left / right;
                };

                ExecuteTest(method, "Com.PointD.operator /(System.Drawing.PointF, Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                Size sz = new Size(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = pt / sz;
                };

                ExecuteTest(method, "Com.PointD.operator /(Com.PointD, System.Drawing.Size)");
            }

            {
                Size sz = new Size(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = sz / pt;
                };

                ExecuteTest(method, "Com.PointD.operator /(System.Drawing.Size, Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = pt / sz;
                };

                ExecuteTest(method, "Com.PointD.operator /(Com.PointD, System.Drawing.SizeF)");
            }

            {
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = sz / pt;
                };

                ExecuteTest(method, "Com.PointD.operator /(System.Drawing.SizeF, Com.PointD)");
            }

            // 类型转换

            {
                Com.PointD pt = new Com.PointD(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = (Point)pt;
                };

                ExecuteTest(method, "Com.PointD.explicit operator System.Drawing.Point(Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = (PointF)pt;
                };

                ExecuteTest(method, "Com.PointD.explicit operator System.Drawing.PointF(Com.PointD)");
            }

            {
                Com.PointD pt = new Com.PointD(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = (Size)pt;
                };

                ExecuteTest(method, "Com.PointD.explicit operator System.Drawing.Size(Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = (SizeF)pt;
                };

                ExecuteTest(method, "Com.PointD.explicit operator System.Drawing.SizeF(Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = (Com.Complex)pt;
                };

                ExecuteTest(method, "Com.PointD.explicit operator Com.Complex(Com.PointD)");
            }

            {
                Point pt = new Point(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = (Com.PointD)pt;
                };

                ExecuteTest(method, "Com.PointD.implicit operator Com.PointD(System.Drawing.Point)");
            }

            {
                PointF pt = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = (Com.PointD)pt;
                };

                ExecuteTest(method, "Com.PointD.implicit operator Com.PointD(System.Drawing.PointF)");
            }

            {
                Size sz = new Size(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = (Com.PointD)sz;
                };

                ExecuteTest(method, "Com.PointD.explicit operator Com.PointD(System.Drawing.Size)");
            }

            {
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = (Com.PointD)sz;
                };

                ExecuteTest(method, "Com.PointD.explicit operator Com.PointD(System.Drawing.SizeF)");
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
                return Com.Matrix.Empty;
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
                    _ = new Com.PointD3D(x, y, z);
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
                    _ = pointD3D[index];
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

            // 分量

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.X;
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
                    _ = pointD3D.Y;
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
                    _ = pointD3D.Z;
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

            // Dimension

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.Dimension;
                };

                ExecuteTest(method, "Com.PointD3D.Dimension.get()");
            }

            // Is

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.IsEmpty;
                };

                ExecuteTest(method, "Com.PointD3D.IsEmpty.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.IsZero;
                };

                ExecuteTest(method, "Com.PointD3D.IsZero.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.IsReadOnly;
                };

                ExecuteTest(method, "Com.PointD3D.IsReadOnly.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.IsFixedSize;
                };

                ExecuteTest(method, "Com.PointD3D.IsFixedSize.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.IsNaN;
                };

                ExecuteTest(method, "Com.PointD3D.IsNaN.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.IsInfinity;
                };

                ExecuteTest(method, "Com.PointD3D.IsInfinity.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.IsNaNOrInfinity;
                };

                ExecuteTest(method, "Com.PointD3D.IsNaNOrInfinity.get()");
            }

            // 模

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.Module;
                };

                ExecuteTest(method, "Com.PointD3D.Module.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.ModuleSquared;
                };

                ExecuteTest(method, "Com.PointD3D.ModuleSquared.get()");
            }

            // 向量

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.Opposite;
                };

                ExecuteTest(method, "Com.PointD3D.Opposite.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.Normalize;
                };

                ExecuteTest(method, "Com.PointD3D.Normalize.get()");
            }

            // 子空间分量

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.XY;
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
                    _ = pointD3D.YZ;
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
                    _ = pointD3D.ZX;
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
                    _ = pointD3D.AngleFromX;
                };

                ExecuteTest(method, "Com.PointD3D.AngleFromX.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.AngleFromY;
                };

                ExecuteTest(method, "Com.PointD3D.AngleFromY.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.AngleFromZ;
                };

                ExecuteTest(method, "Com.PointD3D.AngleFromZ.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.AngleFromXY;
                };

                ExecuteTest(method, "Com.PointD3D.AngleFromXY.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.AngleFromYZ;
                };

                ExecuteTest(method, "Com.PointD3D.AngleFromYZ.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.AngleFromZX;
                };

                ExecuteTest(method, "Com.PointD3D.AngleFromZX.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.Zenith;
                };

                ExecuteTest(method, "Com.PointD3D.Zenith.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.Azimuth;
                };

                ExecuteTest(method, "Com.PointD3D.Azimuth.get()");
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
                object obj = pointD3D;

                Action method = () =>
                {
                    _ = pointD3D.Equals(obj);
                };

                ExecuteTest(method, "Com.PointD3D.Equals(object)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.GetHashCode();
                };

                ExecuteTest(method, "Com.PointD3D.GetHashCode()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.ToString();
                };

                ExecuteTest(method, "Com.PointD3D.ToString()");
            }

            // Equals

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = left;

                Action method = () =>
                {
                    _ = left.Equals(right);
                };

                ExecuteTest(method, "Com.PointD3D.Equals(Com.PointD3D)");
            }

            // CompareTo

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                object obj = pointD3D;

                Action method = () =>
                {
                    _ = pointD3D.CompareTo(obj);
                };

                ExecuteTest(method, "Com.PointD3D.CompareTo(object)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = left;

                Action method = () =>
                {
                    _ = left.CompareTo(right);
                };

                ExecuteTest(method, "Com.PointD3D.CompareTo(Com.PointD3D)");
            }

            // 检索

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD3D.IndexOf(item);
                };

                ExecuteTest(method, "Com.PointD3D.IndexOf(double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 0;

                Action method = () =>
                {
                    _ = pointD3D.IndexOf(item, startIndex);
                };

                ExecuteTest(method, "Com.PointD3D.IndexOf(double, int)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 0;
                int count = 3;

                Action method = () =>
                {
                    _ = pointD3D.IndexOf(item, startIndex, count);
                };

                ExecuteTest(method, "Com.PointD3D.IndexOf(double, int, int)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD3D.LastIndexOf(item);
                };

                ExecuteTest(method, "Com.PointD3D.LastIndexOf(double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 2;

                Action method = () =>
                {
                    _ = pointD3D.LastIndexOf(item, startIndex);
                };

                ExecuteTest(method, "Com.PointD3D.LastIndexOf(double, int)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 2;
                int count = 3;

                Action method = () =>
                {
                    _ = pointD3D.LastIndexOf(item, startIndex, count);
                };

                ExecuteTest(method, "Com.PointD3D.LastIndexOf(double, int, int)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD3D.Contains(item);
                };

                ExecuteTest(method, "Com.PointD3D.Contains(double)");
            }

            // ToArray，ToList

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.ToArray();
                };

                ExecuteTest(method, "Com.PointD3D.ToArray()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.ToList();
                };

                ExecuteTest(method, "Com.PointD3D.ToList()");
            }

            // 坐标系转换

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.ToSpherical();
                };

                ExecuteTest(method, "Com.PointD3D.ToSpherical()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.ToCartesian();
                };

                ExecuteTest(method, "Com.PointD3D.ToCartesian()");
            }

            // 距离与夹角

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD3D pt = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.DistanceFrom(pt);
                };

                ExecuteTest(method, "Com.PointD3D.DistanceFrom(Com.PointD3D)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD3D pt = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.AngleFrom(pt);
                };

                ExecuteTest(method, "Com.PointD3D.AngleFrom(Com.PointD3D)");
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
                    _ = pointD3D.OffsetCopy(d);
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
                    _ = pointD3D.OffsetCopy(dx, dy, dz);
                };

                ExecuteTest(method, "Com.PointD3D.OffsetCopy(double, double, double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD3D pt = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.OffsetCopy(pt);
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
                    _ = pointD3D.ScaleCopy(s);
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
                    _ = pointD3D.ScaleCopy(sx, sy, sz);
                };

                ExecuteTest(method, "Com.PointD3D.ScaleCopy(double, double, double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD3D pt = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.ScaleCopy(pt);
                };

                ExecuteTest(method, "Com.PointD3D.ScaleCopy(Com.PointD3D)");
            }

            // Reflect

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                int index = Com.Statistics.RandomInteger(3);

                Action method = () =>
                {
                    pointD3D.Reflect(index);
                };

                ExecuteTest(method, "Com.PointD3D.Reflect(int)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    pointD3D.ReflectX();
                };

                ExecuteTest(method, "Com.PointD3D.ReflectX()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    pointD3D.ReflectY();
                };

                ExecuteTest(method, "Com.PointD3D.ReflectY()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    pointD3D.ReflectZ();
                };

                ExecuteTest(method, "Com.PointD3D.ReflectZ()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                int index = Com.Statistics.RandomInteger(3);

                Action method = () =>
                {
                    _ = pointD3D.ReflectCopy(index);
                };

                ExecuteTest(method, "Com.PointD3D.ReflectCopy(int)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.ReflectXCopy();
                };

                ExecuteTest(method, "Com.PointD3D.ReflectXCopy()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.ReflectYCopy();
                };

                ExecuteTest(method, "Com.PointD3D.ReflectYCopy()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.ReflectZCopy();
                };

                ExecuteTest(method, "Com.PointD3D.ReflectZCopy()");
            }

            // Shear

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                int index1 = Com.Statistics.RandomInteger(3);
                int index2 = Com.Statistics.RandomInteger(2);
                if (index2 == index1) index2++;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD3D.Shear(index1, index2, angle);
                };

                ExecuteTest(method, "Com.PointD3D.Shear(int, int, double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD3D.ShearXY(angle);
                };

                ExecuteTest(method, "Com.PointD3D.ShearXY(double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD3D.ShearYX(angle);
                };

                ExecuteTest(method, "Com.PointD3D.ShearYX(double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD3D.ShearYZ(angle);
                };

                ExecuteTest(method, "Com.PointD3D.ShearYZ(double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD3D.ShearZY(angle);
                };

                ExecuteTest(method, "Com.PointD3D.ShearZY(double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD3D.ShearZX(angle);
                };

                ExecuteTest(method, "Com.PointD3D.ShearZX(double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD3D.ShearXZ(angle);
                };

                ExecuteTest(method, "Com.PointD3D.ShearXZ(double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                int index1 = Com.Statistics.RandomInteger(3);
                int index2 = Com.Statistics.RandomInteger(2);
                if (index2 == index1) index2++;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD3D.ShearCopy(index1, index2, angle);
                };

                ExecuteTest(method, "Com.PointD3D.ShearCopy(int, int, double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD3D.ShearXYCopy(angle);
                };

                ExecuteTest(method, "Com.PointD3D.ShearXYCopy(double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD3D.ShearYXCopy(angle);
                };

                ExecuteTest(method, "Com.PointD3D.ShearYXCopy(double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD3D.ShearYZCopy(angle);
                };

                ExecuteTest(method, "Com.PointD3D.ShearYZCopy(double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD3D.ShearZYCopy(angle);
                };

                ExecuteTest(method, "Com.PointD3D.ShearZYCopy(double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD3D.ShearZXCopy(angle);
                };

                ExecuteTest(method, "Com.PointD3D.ShearZXCopy(double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD3D.ShearXZCopy(angle);
                };

                ExecuteTest(method, "Com.PointD3D.ShearXZCopy(double)");
            }

            // Rotate

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                int index1 = Com.Statistics.RandomInteger(3);
                int index2 = Com.Statistics.RandomInteger(2);
                if (index2 == index1) index2++;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD3D.Rotate(index1, index2, angle);
                };

                ExecuteTest(method, "Com.PointD3D.Rotate(int, int, double)");
            }

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
                int index1 = Com.Statistics.RandomInteger(3);
                int index2 = Com.Statistics.RandomInteger(2);
                if (index2 == index1) index2++;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD3D.RotateCopy(index1, index2, angle);
                };

                ExecuteTest(method, "Com.PointD3D.RotateCopy(int, int, double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD3D.RotateXCopy(angle);
                };

                ExecuteTest(method, "Com.PointD3D.RotateXCopy(double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD3D.RotateYCopy(angle);
                };

                ExecuteTest(method, "Com.PointD3D.RotateYCopy(double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD3D.RotateZCopy(angle);
                };

                ExecuteTest(method, "Com.PointD3D.RotateZCopy(double)");
            }

            // Affine

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD3D ex = _GetRandomPointD3D().Normalize;
                Com.PointD3D ey = _GetRandomPointD3D().Normalize;
                Com.PointD3D ez = _GetRandomPointD3D().Normalize;
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
                Com.PointD3D ex = _GetRandomPointD3D().Normalize;
                Com.PointD3D ey = _GetRandomPointD3D().Normalize;
                Com.PointD3D ez = _GetRandomPointD3D().Normalize;
                Com.PointD3D offset = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.AffineTransformCopy(ex, ey, ez, offset);
                };

                ExecuteTest(method, "Com.PointD3D.AffineTransformCopy(Com.PointD3D, Com.PointD3D, Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.Matrix matrixLeft = _GetRandomMatrix(4, 4);

                Action method = () =>
                {
                    _ = pointD3D.AffineTransformCopy(matrixLeft);
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
                    _ = pointD3D.AffineTransformCopy(matrixLeftList);
                };

                ExecuteTest(method, "Com.PointD3D.AffineTransformCopy(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD3D ex = _GetRandomPointD3D().Normalize;
                Com.PointD3D ey = _GetRandomPointD3D().Normalize;
                Com.PointD3D ez = _GetRandomPointD3D().Normalize;
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
                Com.PointD3D ex = _GetRandomPointD3D().Normalize;
                Com.PointD3D ey = _GetRandomPointD3D().Normalize;
                Com.PointD3D ez = _GetRandomPointD3D().Normalize;
                Com.PointD3D offset = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.InverseAffineTransformCopy(ex, ey, ez, offset);
                };

                ExecuteTest(method, "Com.PointD3D.InverseAffineTransformCopy(Com.PointD3D, Com.PointD3D, Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.Matrix matrixLeft = _GetRandomMatrix(4, 4);

                Action method = () =>
                {
                    _ = pointD3D.InverseAffineTransformCopy(matrixLeft);
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
                    _ = pointD3D.InverseAffineTransformCopy(matrixLeftList);
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
                    _ = pointD3D.ProjectToXY(prjCenter, trueLenDist);
                };

                ExecuteTest(method, "Com.PointD3D.ProjectToXY(Com.PointD3D, double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD3D prjCenter = _GetRandomPointD3D();
                double trueLenDist = (pointD3D.X - prjCenter.X) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    _ = pointD3D.ProjectToYZ(prjCenter, trueLenDist);
                };

                ExecuteTest(method, "Com.PointD3D.ProjectToYZ(Com.PointD3D, double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD3D prjCenter = _GetRandomPointD3D();
                double trueLenDist = (pointD3D.Y - prjCenter.Y) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    _ = pointD3D.ProjectToZX(prjCenter, trueLenDist);
                };

                ExecuteTest(method, "Com.PointD3D.ProjectToZX(Com.PointD3D, double)");
            }

            // ToVector

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.ToColumnVector();
                };

                ExecuteTest(method, "Com.PointD3D.ToColumnVector()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.ToRowVector();
                };

                ExecuteTest(method, "Com.PointD3D.ToRowVector()");
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
                    _ = Com.PointD3D.Equals(left, right);
                };

                ExecuteTest(method, "Com.PointD3D.Equals(Com.PointD3D, Com.PointD3D)");
            }

            // Compare

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = left;

                Action method = () =>
                {
                    _ = Com.PointD3D.Compare(left, right);
                };

                ExecuteTest(method, "Com.PointD3D.Compare(Com.PointD3D, Com.PointD3D)");
            }

            // Matrix

            {
                Action method = () =>
                {
                    _ = Com.PointD3D.IdentityMatrix();
                };

                ExecuteTest(method, "Com.PointD3D.IdentityMatrix()");
            }

            {
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD3D.OffsetMatrix(d);
                };

                ExecuteTest(method, "Com.PointD3D.OffsetMatrix(double)");
            }

            {
                double dx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dy = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dz = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD3D.OffsetMatrix(dx, dy, dz);
                };

                ExecuteTest(method, "Com.PointD3D.OffsetMatrix(double, double, double)");
            }

            {
                Com.PointD3D pt = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = Com.PointD3D.OffsetMatrix(pt);
                };

                ExecuteTest(method, "Com.PointD3D.OffsetMatrix(Com.PointD3D)");
            }

            {
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD3D.ScaleMatrix(s);
                };

                ExecuteTest(method, "Com.PointD3D.ScaleMatrix(double)");
            }

            {
                double sx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sy = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sz = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD3D.ScaleMatrix(sx, sy, sz);
                };

                ExecuteTest(method, "Com.PointD3D.ScaleMatrix(double, double, double)");
            }

            {
                Com.PointD3D pt = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = Com.PointD3D.ScaleMatrix(pt);
                };

                ExecuteTest(method, "Com.PointD3D.ScaleMatrix(Com.PointD3D)");
            }

            {
                int index = Com.Statistics.RandomInteger(3);

                Action method = () =>
                {
                    _ = Com.PointD3D.ReflectMatrix(index);
                };

                ExecuteTest(method, "Com.PointD3D.ReflectMatrix(int)");
            }

            {
                Action method = () =>
                {
                    _ = Com.PointD3D.ReflectXMatrix();
                };

                ExecuteTest(method, "Com.PointD3D.ReflectXMatrix()");
            }

            {
                Action method = () =>
                {
                    _ = Com.PointD3D.ReflectYMatrix();
                };

                ExecuteTest(method, "Com.PointD3D.ReflectYMatrix()");
            }

            {
                Action method = () =>
                {
                    _ = Com.PointD3D.ReflectZMatrix();
                };

                ExecuteTest(method, "Com.PointD3D.ReflectZMatrix()");
            }

            {
                int index1 = Com.Statistics.RandomInteger(3);
                int index2 = Com.Statistics.RandomInteger(2);
                if (index2 == index1) index2++;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD3D.ShearMatrix(index1, index2, angle);
                };

                ExecuteTest(method, "Com.PointD3D.ShearMatrix(int, int, double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD3D.ShearXYMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD3D.ShearXYMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD3D.ShearYXMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD3D.ShearYXMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD3D.ShearYZMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD3D.ShearYZMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD3D.ShearZYMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD3D.ShearZYMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD3D.ShearZXMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD3D.ShearZXMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD3D.ShearXZMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD3D.ShearXZMatrix(double)");
            }

            {
                int index1 = Com.Statistics.RandomInteger(3);
                int index2 = Com.Statistics.RandomInteger(2);
                if (index2 == index1) index2++;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD3D.RotateMatrix(index1, index2, angle);
                };

                ExecuteTest(method, "Com.PointD3D.RotateMatrix(int, int, double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD3D.RotateXMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD3D.RotateXMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD3D.RotateYMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD3D.RotateYMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD3D.RotateZMatrix(angle);
                };

                ExecuteTest(method, "Com.PointD3D.RotateZMatrix(double)");
            }

            // 距离与夹角

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = Com.PointD3D.DistanceBetween(left, right);
                };

                ExecuteTest(method, "Com.PointD3D.DistanceBetween(Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = Com.PointD3D.AngleBetween(left, right);
                };

                ExecuteTest(method, "Com.PointD3D.AngleBetween(Com.PointD3D, Com.PointD3D)");
            }

            // 向量乘积

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = Com.PointD3D.DotProduct(left, right);
                };

                ExecuteTest(method, "Com.PointD3D.DotProduct(Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = Com.PointD3D.CrossProduct(left, right);
                };

                ExecuteTest(method, "Com.PointD3D.CrossProduct(Com.PointD3D, Com.PointD3D)");
            }

            // 初等函数

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = Com.PointD3D.Abs(pointD3D);
                };

                ExecuteTest(method, "Com.PointD3D.Abs(Com.PointD3D)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = Com.PointD3D.Sign(pointD3D);
                };

                ExecuteTest(method, "Com.PointD3D.Sign(Com.PointD3D)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = Com.PointD3D.Ceiling(pointD3D);
                };

                ExecuteTest(method, "Com.PointD3D.Ceiling(Com.PointD3D)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = Com.PointD3D.Floor(pointD3D);
                };

                ExecuteTest(method, "Com.PointD3D.Floor(Com.PointD3D)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = Com.PointD3D.Round(pointD3D);
                };

                ExecuteTest(method, "Com.PointD3D.Round(Com.PointD3D)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = Com.PointD3D.Truncate(pointD3D);
                };

                ExecuteTest(method, "Com.PointD3D.Truncate(Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = Com.PointD3D.Max(left, right);
                };

                ExecuteTest(method, "Com.PointD3D.Max(Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = Com.PointD3D.Min(left, right);
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
                    _ = (left == right);
                };

                ExecuteTest(method, "Com.PointD3D.operator ==(Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = left;

                Action method = () =>
                {
                    _ = (left != right);
                };

                ExecuteTest(method, "Com.PointD3D.operator !=(Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = (left < right);
                };

                ExecuteTest(method, "Com.PointD3D.operator <(Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = (left > right);
                };

                ExecuteTest(method, "Com.PointD3D.operator >(Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = (left <= right);
                };

                ExecuteTest(method, "Com.PointD3D.operator <=(Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = (left >= right);
                };

                ExecuteTest(method, "Com.PointD3D.operator >=(Com.PointD3D, Com.PointD3D)");
            }

            // 运算

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = +pointD3D;
                };

                ExecuteTest(method, "Com.PointD3D.operator +(Com.PointD3D)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = -pointD3D;
                };

                ExecuteTest(method, "Com.PointD3D.operator -(Com.PointD3D)");
            }

            {
                Com.PointD3D pt = _GetRandomPointD3D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt + n;
                };

                ExecuteTest(method, "Com.PointD3D.operator +(Com.PointD3D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD3D pt = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = n + pt;
                };

                ExecuteTest(method, "Com.PointD3D.operator +(double, Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = left + right;
                };

                ExecuteTest(method, "Com.PointD3D.operator +(Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D pt = _GetRandomPointD3D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt - n;
                };

                ExecuteTest(method, "Com.PointD3D.operator -(Com.PointD3D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD3D pt = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = n - pt;
                };

                ExecuteTest(method, "Com.PointD3D.operator -(double, Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = left - right;
                };

                ExecuteTest(method, "Com.PointD3D.operator -(Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D pt = _GetRandomPointD3D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt * n;
                };

                ExecuteTest(method, "Com.PointD3D.operator *(Com.PointD3D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD3D pt = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = n * pt;
                };

                ExecuteTest(method, "Com.PointD3D.operator *(double, Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = left * right;
                };

                ExecuteTest(method, "Com.PointD3D.operator *(Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D pt = _GetRandomPointD3D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt / n;
                };

                ExecuteTest(method, "Com.PointD3D.operator /(Com.PointD3D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD3D pt = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = n / pt;
                };

                ExecuteTest(method, "Com.PointD3D.operator /(double, Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = left / right;
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
                return Com.Matrix.Empty;
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
                    _ = new Com.PointD4D(x, y, z, u);
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
                    _ = pointD4D[index];
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

            // 分量

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.X;
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
                    _ = pointD4D.Y;
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
                    _ = pointD4D.Z;
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
                    _ = pointD4D.U;
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

            // Dimension

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.Dimension;
                };

                ExecuteTest(method, "Com.PointD4D.Dimension.get()");
            }

            // Is

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.IsEmpty;
                };

                ExecuteTest(method, "Com.PointD4D.IsEmpty.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.IsZero;
                };

                ExecuteTest(method, "Com.PointD4D.IsZero.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.IsReadOnly;
                };

                ExecuteTest(method, "Com.PointD4D.IsReadOnly.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.IsFixedSize;
                };

                ExecuteTest(method, "Com.PointD4D.IsFixedSize.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.IsNaN;
                };

                ExecuteTest(method, "Com.PointD4D.IsNaN.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.IsInfinity;
                };

                ExecuteTest(method, "Com.PointD4D.IsInfinity.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.IsNaNOrInfinity;
                };

                ExecuteTest(method, "Com.PointD4D.IsNaNOrInfinity.get()");
            }

            // 模

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.Module;
                };

                ExecuteTest(method, "Com.PointD4D.Module.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.ModuleSquared;
                };

                ExecuteTest(method, "Com.PointD4D.ModuleSquared.get()");
            }

            // 向量

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.Opposite;
                };

                ExecuteTest(method, "Com.PointD4D.Opposite.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.Normalize;
                };

                ExecuteTest(method, "Com.PointD4D.Normalize.get()");
            }

            // 子空间分量

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.XYZ;
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
                    _ = pointD4D.YZU;
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
                    _ = pointD4D.ZUX;
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
                    _ = pointD4D.UXY;
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
                    _ = pointD4D.AngleFromX;
                };

                ExecuteTest(method, "Com.PointD4D.AngleFromX.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.AngleFromY;
                };

                ExecuteTest(method, "Com.PointD4D.AngleFromY.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.AngleFromZ;
                };

                ExecuteTest(method, "Com.PointD4D.AngleFromZ.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.AngleFromU;
                };

                ExecuteTest(method, "Com.PointD4D.AngleFromU.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.AngleFromXYZ;
                };

                ExecuteTest(method, "Com.PointD4D.AngleFromXYZ.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.AngleFromYZU;
                };

                ExecuteTest(method, "Com.PointD4D.AngleFromYZU.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.AngleFromZUX;
                };

                ExecuteTest(method, "Com.PointD4D.AngleFromZUX.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.AngleFromUXY;
                };

                ExecuteTest(method, "Com.PointD4D.AngleFromUXY.get()");
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
                object obj = pointD4D;

                Action method = () =>
                {
                    _ = pointD4D.Equals(obj);
                };

                ExecuteTest(method, "Com.PointD4D.Equals(object)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.GetHashCode();
                };

                ExecuteTest(method, "Com.PointD4D.GetHashCode()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.ToString();
                };

                ExecuteTest(method, "Com.PointD4D.ToString()");
            }

            // Equals

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = left;

                Action method = () =>
                {
                    _ = left.Equals(right);
                };

                ExecuteTest(method, "Com.PointD4D.Equals(Com.PointD4D)");
            }

            // CompareTo

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                object obj = pointD4D;

                Action method = () =>
                {
                    _ = pointD4D.CompareTo(obj);
                };

                ExecuteTest(method, "Com.PointD4D.CompareTo(object)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = left;

                Action method = () =>
                {
                    _ = left.CompareTo(right);
                };

                ExecuteTest(method, "Com.PointD4D.CompareTo(Com.PointD4D)");
            }

            // 检索

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD4D.IndexOf(item);
                };

                ExecuteTest(method, "Com.PointD4D.IndexOf(double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 0;

                Action method = () =>
                {
                    _ = pointD4D.IndexOf(item, startIndex);
                };

                ExecuteTest(method, "Com.PointD4D.IndexOf(double, int)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 0;
                int count = 4;

                Action method = () =>
                {
                    _ = pointD4D.IndexOf(item, startIndex, count);
                };

                ExecuteTest(method, "Com.PointD4D.IndexOf(double, int, int)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD4D.LastIndexOf(item);
                };

                ExecuteTest(method, "Com.PointD4D.LastIndexOf(double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 3;

                Action method = () =>
                {
                    _ = pointD4D.LastIndexOf(item, startIndex);
                };

                ExecuteTest(method, "Com.PointD4D.LastIndexOf(double, int)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 3;
                int count = 4;

                Action method = () =>
                {
                    _ = pointD4D.LastIndexOf(item, startIndex, count);
                };

                ExecuteTest(method, "Com.PointD4D.LastIndexOf(double, int, int)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD4D.Contains(item);
                };

                ExecuteTest(method, "Com.PointD4D.Contains(double)");
            }

            // ToArray，ToList

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.ToArray();
                };

                ExecuteTest(method, "Com.PointD4D.ToArray()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.ToList();
                };

                ExecuteTest(method, "Com.PointD4D.ToList()");
            }

            // 坐标系转换

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.ToSpherical();
                };

                ExecuteTest(method, "Com.PointD4D.ToSpherical()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.ToCartesian();
                };

                ExecuteTest(method, "Com.PointD4D.ToCartesian()");
            }

            // 距离与夹角

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD4D pt = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.DistanceFrom(pt);
                };

                ExecuteTest(method, "Com.PointD4D.DistanceFrom(Com.PointD4D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD4D pt = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.AngleFrom(pt);
                };

                ExecuteTest(method, "Com.PointD4D.AngleFrom(Com.PointD4D)");
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
                    _ = pointD4D.OffsetCopy(d);
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
                    _ = pointD4D.OffsetCopy(dx, dy, dz, du);
                };

                ExecuteTest(method, "Com.PointD4D.OffsetCopy(double, double, double, double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD4D pt = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.OffsetCopy(pt);
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
                    _ = pointD4D.ScaleCopy(s);
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
                    _ = pointD4D.ScaleCopy(sx, sy, sz, su);
                };

                ExecuteTest(method, "Com.PointD4D.ScaleCopy(double, double, double, double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD4D pt = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.ScaleCopy(pt);
                };

                ExecuteTest(method, "Com.PointD4D.ScaleCopy(Com.PointD4D)");
            }

            // Reflect

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                int index = Com.Statistics.RandomInteger(4);

                Action method = () =>
                {
                    pointD4D.Reflect(index);
                };

                ExecuteTest(method, "Com.PointD4D.Reflect(int)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                int index = Com.Statistics.RandomInteger(4);

                Action method = () =>
                {
                    _ = pointD4D.ReflectCopy(index);
                };

                ExecuteTest(method, "Com.PointD4D.ReflectCopy(int)");
            }

            // Shear

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                int index1 = Com.Statistics.RandomInteger(4);
                int index2 = Com.Statistics.RandomInteger(3);
                if (index2 == index1) index2++;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD4D.Shear(index1, index2, angle);
                };

                ExecuteTest(method, "Com.PointD4D.Shear(int, int, double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                int index1 = Com.Statistics.RandomInteger(4);
                int index2 = Com.Statistics.RandomInteger(3);
                if (index2 == index1) index2++;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD4D.ShearCopy(index1, index2, angle);
                };

                ExecuteTest(method, "Com.PointD4D.ShearCopy(int, int, double)");
            }

            // Rotate

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                int index1 = Com.Statistics.RandomInteger(4);
                int index2 = Com.Statistics.RandomInteger(3);
                if (index2 == index1) index2++;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD4D.Rotate(index1, index2, angle);
                };

                ExecuteTest(method, "Com.PointD4D.Rotate(int, int, double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                int index1 = Com.Statistics.RandomInteger(4);
                int index2 = Com.Statistics.RandomInteger(3);
                if (index2 == index1) index2++;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD4D.RotateCopy(index1, index2, angle);
                };

                ExecuteTest(method, "Com.PointD4D.RotateCopy(int, int, double)");
            }

            // Affine

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD4D ex = _GetRandomPointD4D().Normalize;
                Com.PointD4D ey = _GetRandomPointD4D().Normalize;
                Com.PointD4D ez = _GetRandomPointD4D().Normalize;
                Com.PointD4D eu = _GetRandomPointD4D().Normalize;
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
                Com.PointD4D ex = _GetRandomPointD4D().Normalize;
                Com.PointD4D ey = _GetRandomPointD4D().Normalize;
                Com.PointD4D ez = _GetRandomPointD4D().Normalize;
                Com.PointD4D eu = _GetRandomPointD4D().Normalize;
                Com.PointD4D offset = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.AffineTransformCopy(ex, ey, ez, eu, offset);
                };

                ExecuteTest(method, "Com.PointD4D.AffineTransformCopy(Com.PointD4D, Com.PointD4D, Com.PointD4D, Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.Matrix matrixLeft = _GetRandomMatrix(5, 5);

                Action method = () =>
                {
                    _ = pointD4D.AffineTransformCopy(matrixLeft);
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
                    _ = pointD4D.AffineTransformCopy(matrixLeftList);
                };

                ExecuteTest(method, "Com.PointD4D.AffineTransformCopy(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD4D ex = _GetRandomPointD4D().Normalize;
                Com.PointD4D ey = _GetRandomPointD4D().Normalize;
                Com.PointD4D ez = _GetRandomPointD4D().Normalize;
                Com.PointD4D eu = _GetRandomPointD4D().Normalize;
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
                Com.PointD4D ex = _GetRandomPointD4D().Normalize;
                Com.PointD4D ey = _GetRandomPointD4D().Normalize;
                Com.PointD4D ez = _GetRandomPointD4D().Normalize;
                Com.PointD4D eu = _GetRandomPointD4D().Normalize;
                Com.PointD4D offset = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.InverseAffineTransformCopy(ex, ey, ez, eu, offset);
                };

                ExecuteTest(method, "Com.PointD4D.InverseAffineTransformCopy(Com.PointD4D, Com.PointD4D, Com.PointD4D, Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.Matrix matrixLeft = _GetRandomMatrix(5, 5);

                Action method = () =>
                {
                    _ = pointD4D.InverseAffineTransformCopy(matrixLeft);
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
                    _ = pointD4D.InverseAffineTransformCopy(matrixLeftList);
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
                    _ = pointD4D.ProjectToXYZ(prjCenter, trueLenDist);
                };

                ExecuteTest(method, "Com.PointD4D.ProjectToXYZ(Com.PointD4D, double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD4D prjCenter = _GetRandomPointD4D();
                double trueLenDist = (pointD4D.X - prjCenter.X) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    _ = pointD4D.ProjectToYZU(prjCenter, trueLenDist);
                };

                ExecuteTest(method, "Com.PointD4D.ProjectToYZU(Com.PointD4D, double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD4D prjCenter = _GetRandomPointD4D();
                double trueLenDist = (pointD4D.Y - prjCenter.Y) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    _ = pointD4D.ProjectToZUX(prjCenter, trueLenDist);
                };

                ExecuteTest(method, "Com.PointD4D.ProjectToZUX(Com.PointD4D, double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD4D prjCenter = _GetRandomPointD4D();
                double trueLenDist = (pointD4D.Z - prjCenter.Z) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    _ = pointD4D.ProjectToUXY(prjCenter, trueLenDist);
                };

                ExecuteTest(method, "Com.PointD4D.ProjectToUXY(Com.PointD4D, double)");
            }

            // ToVector

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.ToColumnVector();
                };

                ExecuteTest(method, "Com.PointD4D.ToColumnVector()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.ToRowVector();
                };

                ExecuteTest(method, "Com.PointD4D.ToRowVector()");
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
                    _ = Com.PointD4D.Equals(left, right);
                };

                ExecuteTest(method, "Com.PointD4D.Equals(Com.PointD4D, Com.PointD4D)");
            }

            // Compare

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = left;

                Action method = () =>
                {
                    _ = Com.PointD4D.Compare(left, right);
                };

                ExecuteTest(method, "Com.PointD4D.Compare(Com.PointD4D, Com.PointD4D)");
            }

            // Matrix

            {
                Action method = () =>
                {
                    _ = Com.PointD4D.IdentityMatrix();
                };

                ExecuteTest(method, "Com.PointD4D.IdentityMatrix()");
            }

            {
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD4D.OffsetMatrix(d);
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
                    _ = Com.PointD4D.OffsetMatrix(dx, dy, dz, du);
                };

                ExecuteTest(method, "Com.PointD4D.OffsetMatrix(double, double, double, double)");
            }

            {
                Com.PointD4D pt = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = Com.PointD4D.OffsetMatrix(pt);
                };

                ExecuteTest(method, "Com.PointD4D.OffsetMatrix(Com.PointD4D)");
            }

            {
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD4D.ScaleMatrix(s);
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
                    _ = Com.PointD4D.ScaleMatrix(sx, sy, sz, su);
                };

                ExecuteTest(method, "Com.PointD4D.ScaleMatrix(double, double, double, double)");
            }

            {
                Com.PointD4D pt = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = Com.PointD4D.ScaleMatrix(pt);
                };

                ExecuteTest(method, "Com.PointD4D.ScaleMatrix(Com.PointD4D)");
            }

            {
                int index = Com.Statistics.RandomInteger(3);

                Action method = () =>
                {
                    _ = Com.PointD4D.ReflectMatrix(index);
                };

                ExecuteTest(method, "Com.PointD4D.ReflectMatrix(int)");
            }

            {
                int index1 = Com.Statistics.RandomInteger(4);
                int index2 = Com.Statistics.RandomInteger(3);
                if (index2 == index1) index2++;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD4D.ShearMatrix(index1, index2, angle);
                };

                ExecuteTest(method, "Com.PointD4D.ShearMatrix(int, int, double)");
            }

            {
                int index1 = Com.Statistics.RandomInteger(4);
                int index2 = Com.Statistics.RandomInteger(3);
                if (index2 == index1) index2++;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD4D.RotateMatrix(index1, index2, angle);
                };

                ExecuteTest(method, "Com.PointD4D.RotateMatrix(int, int, double)");
            }

            // 距离与夹角

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = Com.PointD4D.DistanceBetween(left, right);
                };

                ExecuteTest(method, "Com.PointD4D.DistanceBetween(Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = Com.PointD4D.AngleBetween(left, right);
                };

                ExecuteTest(method, "Com.PointD4D.AngleBetween(Com.PointD4D, Com.PointD4D)");
            }

            // 向量乘积

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = Com.PointD4D.DotProduct(left, right);
                };

                ExecuteTest(method, "Com.PointD4D.DotProduct(Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = Com.PointD4D.CrossProduct(left, right);
                };

                ExecuteTest(method, "Com.PointD4D.CrossProduct(Com.PointD4D, Com.PointD4D)");
            }

            // 初等函数

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = Com.PointD4D.Abs(pointD4D);
                };

                ExecuteTest(method, "Com.PointD4D.Abs(Com.PointD4D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = Com.PointD4D.Sign(pointD4D);
                };

                ExecuteTest(method, "Com.PointD4D.Sign(Com.PointD4D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = Com.PointD4D.Ceiling(pointD4D);
                };

                ExecuteTest(method, "Com.PointD4D.Ceiling(Com.PointD4D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = Com.PointD4D.Floor(pointD4D);
                };

                ExecuteTest(method, "Com.PointD4D.Floor(Com.PointD4D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = Com.PointD4D.Round(pointD4D);
                };

                ExecuteTest(method, "Com.PointD4D.Round(Com.PointD4D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = Com.PointD4D.Truncate(pointD4D);
                };

                ExecuteTest(method, "Com.PointD4D.Truncate(Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = Com.PointD4D.Max(left, right);
                };

                ExecuteTest(method, "Com.PointD4D.Max(Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = Com.PointD4D.Min(left, right);
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
                    _ = (left == right);
                };

                ExecuteTest(method, "Com.PointD4D.operator ==(Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = left;

                Action method = () =>
                {
                    _ = (left != right);
                };

                ExecuteTest(method, "Com.PointD4D.operator !=(Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = (left < right);
                };

                ExecuteTest(method, "Com.PointD4D.operator <(Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = (left > right);
                };

                ExecuteTest(method, "Com.PointD4D.operator >(Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = (left <= right);
                };

                ExecuteTest(method, "Com.PointD4D.operator <=(Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = (left >= right);
                };

                ExecuteTest(method, "Com.PointD4D.operator >=(Com.PointD4D, Com.PointD4D)");
            }

            // 运算

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = +pointD4D;
                };

                ExecuteTest(method, "Com.PointD4D.operator +(Com.PointD4D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = -pointD4D;
                };

                ExecuteTest(method, "Com.PointD4D.operator -(Com.PointD4D)");
            }

            {
                Com.PointD4D pt = _GetRandomPointD4D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt + n;
                };

                ExecuteTest(method, "Com.PointD4D.operator +(Com.PointD4D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD4D pt = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = n + pt;
                };

                ExecuteTest(method, "Com.PointD4D.operator +(double, Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = left + right;
                };

                ExecuteTest(method, "Com.PointD4D.operator +(Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D pt = _GetRandomPointD4D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt - n;
                };

                ExecuteTest(method, "Com.PointD4D.operator -(Com.PointD4D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD4D pt = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = n - pt;
                };

                ExecuteTest(method, "Com.PointD4D.operator -(double, Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = left - right;
                };

                ExecuteTest(method, "Com.PointD4D.operator -(Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D pt = _GetRandomPointD4D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt * n;
                };

                ExecuteTest(method, "Com.PointD4D.operator *(Com.PointD4D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD4D pt = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = n * pt;
                };

                ExecuteTest(method, "Com.PointD4D.operator *(double, Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = left * right;
                };

                ExecuteTest(method, "Com.PointD4D.operator *(Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D pt = _GetRandomPointD4D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt / n;
                };

                ExecuteTest(method, "Com.PointD4D.operator /(Com.PointD4D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD4D pt = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = n / pt;
                };

                ExecuteTest(method, "Com.PointD4D.operator /(double, Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = left / right;
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
                return Com.Matrix.Empty;
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
                    _ = new Com.PointD5D(x, y, z, u, v);
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
                    _ = pointD5D[index];
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

            // 分量

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.X;
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
                    _ = pointD5D.Y;
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
                    _ = pointD5D.Z;
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
                    _ = pointD5D.U;
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
                    _ = pointD5D.V;
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

            // Dimension

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.Dimension;
                };

                ExecuteTest(method, "Com.PointD5D.Dimension.get()");
            }

            // Is

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.IsEmpty;
                };

                ExecuteTest(method, "Com.PointD5D.IsEmpty.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.IsZero;
                };

                ExecuteTest(method, "Com.PointD5D.IsZero.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.IsReadOnly;
                };

                ExecuteTest(method, "Com.PointD5D.IsReadOnly.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.IsFixedSize;
                };

                ExecuteTest(method, "Com.PointD5D.IsFixedSize.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.IsNaN;
                };

                ExecuteTest(method, "Com.PointD5D.IsNaN.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.IsInfinity;
                };

                ExecuteTest(method, "Com.PointD5D.IsInfinity.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.IsNaNOrInfinity;
                };

                ExecuteTest(method, "Com.PointD5D.IsNaNOrInfinity.get()");
            }

            // 模

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.Module;
                };

                ExecuteTest(method, "Com.PointD5D.Module.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.ModuleSquared;
                };

                ExecuteTest(method, "Com.PointD5D.ModuleSquared.get()");
            }

            // 向量

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.Opposite;
                };

                ExecuteTest(method, "Com.PointD5D.Opposite.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.Normalize;
                };

                ExecuteTest(method, "Com.PointD5D.Normalize.get()");
            }

            // 子空间分量

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.XYZU;
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
                    _ = pointD5D.YZUV;
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
                    _ = pointD5D.ZUVX;
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
                    _ = pointD5D.UVXY;
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
                    _ = pointD5D.VXYZ;
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
                    _ = pointD5D.AngleFromX;
                };

                ExecuteTest(method, "Com.PointD5D.AngleFromX.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.AngleFromY;
                };

                ExecuteTest(method, "Com.PointD5D.AngleFromY.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.AngleFromZ;
                };

                ExecuteTest(method, "Com.PointD5D.AngleFromZ.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.AngleFromU;
                };

                ExecuteTest(method, "Com.PointD5D.AngleFromU.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.AngleFromV;
                };

                ExecuteTest(method, "Com.PointD5D.AngleFromV.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.AngleFromXYZU;
                };

                ExecuteTest(method, "Com.PointD5D.AngleFromXYZU.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.AngleFromYZUV;
                };

                ExecuteTest(method, "Com.PointD5D.AngleFromYZUV.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.AngleFromZUVX;
                };

                ExecuteTest(method, "Com.PointD5D.AngleFromZUVX.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.AngleFromUVXY;
                };

                ExecuteTest(method, "Com.PointD5D.AngleFromUVXY.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.AngleFromVXYZ;
                };

                ExecuteTest(method, "Com.PointD5D.AngleFromVXYZ.get()");
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
                object obj = pointD5D;

                Action method = () =>
                {
                    _ = pointD5D.Equals(obj);
                };

                ExecuteTest(method, "Com.PointD5D.Equals(object)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.GetHashCode();
                };

                ExecuteTest(method, "Com.PointD5D.GetHashCode()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.ToString();
                };

                ExecuteTest(method, "Com.PointD5D.ToString()");
            }

            // Equals

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = left;

                Action method = () =>
                {
                    _ = left.Equals(right);
                };

                ExecuteTest(method, "Com.PointD5D.Equals(Com.PointD5D)");
            }

            // CompareTo

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                object obj = pointD5D;

                Action method = () =>
                {
                    _ = pointD5D.CompareTo(obj);
                };

                ExecuteTest(method, "Com.PointD5D.CompareTo(object)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = left;

                Action method = () =>
                {
                    _ = left.CompareTo(right);
                };

                ExecuteTest(method, "Com.PointD5D.CompareTo(Com.PointD5D)");
            }

            // 检索

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD5D.IndexOf(item);
                };

                ExecuteTest(method, "Com.PointD5D.IndexOf(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 0;

                Action method = () =>
                {
                    _ = pointD5D.IndexOf(item, startIndex);
                };

                ExecuteTest(method, "Com.PointD5D.IndexOf(double, int)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 0;
                int count = 5;

                Action method = () =>
                {
                    _ = pointD5D.IndexOf(item, startIndex, count);
                };

                ExecuteTest(method, "Com.PointD5D.IndexOf(double, int, int)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD5D.LastIndexOf(item);
                };

                ExecuteTest(method, "Com.PointD5D.LastIndexOf(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 4;

                Action method = () =>
                {
                    _ = pointD5D.LastIndexOf(item, startIndex);
                };

                ExecuteTest(method, "Com.PointD5D.LastIndexOf(double, int)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 4;
                int count = 5;

                Action method = () =>
                {
                    _ = pointD5D.LastIndexOf(item, startIndex, count);
                };

                ExecuteTest(method, "Com.PointD5D.LastIndexOf(double, int, int)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD5D.Contains(item);
                };

                ExecuteTest(method, "Com.PointD5D.Contains(double)");
            }

            // ToArray，ToList

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.ToArray();
                };

                ExecuteTest(method, "Com.PointD5D.ToArray()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.ToList();
                };

                ExecuteTest(method, "Com.PointD5D.ToList()");
            }

            // 坐标系转换

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.ToSpherical();
                };

                ExecuteTest(method, "Com.PointD5D.ToSpherical()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.ToCartesian();
                };

                ExecuteTest(method, "Com.PointD5D.ToCartesian()");
            }

            // 距离与夹角

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D pt = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.DistanceFrom(pt);
                };

                ExecuteTest(method, "Com.PointD5D.DistanceFrom(Com.PointD5D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D pt = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.AngleFrom(pt);
                };

                ExecuteTest(method, "Com.PointD5D.AngleFrom(Com.PointD5D)");
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
                    _ = pointD5D.OffsetCopy(d);
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
                    _ = pointD5D.OffsetCopy(dx, dy, dz, du, dv);
                };

                ExecuteTest(method, "Com.PointD5D.OffsetCopy(double, double, double, double, double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D pt = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.OffsetCopy(pt);
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
                    _ = pointD5D.ScaleCopy(s);
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
                    _ = pointD5D.ScaleCopy(sx, sy, sz, su, sv);
                };

                ExecuteTest(method, "Com.PointD5D.ScaleCopy(double, double, double, double, double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D pt = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.ScaleCopy(pt);
                };

                ExecuteTest(method, "Com.PointD5D.ScaleCopy(Com.PointD5D)");
            }

            // Reflect

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                int index = Com.Statistics.RandomInteger(5);

                Action method = () =>
                {
                    pointD5D.Reflect(index);
                };

                ExecuteTest(method, "Com.PointD5D.Reflect(int)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                int index = Com.Statistics.RandomInteger(5);

                Action method = () =>
                {
                    _ = pointD5D.ReflectCopy(index);
                };

                ExecuteTest(method, "Com.PointD5D.ReflectCopy(int)");
            }

            // Shear

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                int index1 = Com.Statistics.RandomInteger(5);
                int index2 = Com.Statistics.RandomInteger(4);
                if (index2 == index1) index2++;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD5D.Shear(index1, index2, angle);
                };

                ExecuteTest(method, "Com.PointD5D.Shear(int, int, double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                int index1 = Com.Statistics.RandomInteger(5);
                int index2 = Com.Statistics.RandomInteger(4);
                if (index2 == index1) index2++;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD5D.ShearCopy(index1, index2, angle);
                };

                ExecuteTest(method, "Com.PointD5D.ShearCopy(int, int, double)");
            }

            // Rotate

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                int index1 = Com.Statistics.RandomInteger(5);
                int index2 = Com.Statistics.RandomInteger(4);
                if (index2 == index1) index2++;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD5D.Rotate(index1, index2, angle);
                };

                ExecuteTest(method, "Com.PointD5D.Rotate(int, int, double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                int index1 = Com.Statistics.RandomInteger(5);
                int index2 = Com.Statistics.RandomInteger(4);
                if (index2 == index1) index2++;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD5D.RotateCopy(index1, index2, angle);
                };

                ExecuteTest(method, "Com.PointD5D.RotateCopy(int, int, double)");
            }

            // Affine

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D ex = _GetRandomPointD5D().Normalize;
                Com.PointD5D ey = _GetRandomPointD5D().Normalize;
                Com.PointD5D ez = _GetRandomPointD5D().Normalize;
                Com.PointD5D eu = _GetRandomPointD5D().Normalize;
                Com.PointD5D ev = _GetRandomPointD5D().Normalize;
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
                Com.PointD5D ex = _GetRandomPointD5D().Normalize;
                Com.PointD5D ey = _GetRandomPointD5D().Normalize;
                Com.PointD5D ez = _GetRandomPointD5D().Normalize;
                Com.PointD5D eu = _GetRandomPointD5D().Normalize;
                Com.PointD5D ev = _GetRandomPointD5D().Normalize;
                Com.PointD5D offset = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.AffineTransformCopy(ex, ey, ez, eu, ev, offset);
                };

                ExecuteTest(method, "Com.PointD5D.AffineTransformCopy(Com.PointD5D, Com.PointD5D, Com.PointD5D, Com.PointD5D, Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.Matrix matrixLeft = _GetRandomMatrix(6, 6);

                Action method = () =>
                {
                    _ = pointD5D.AffineTransformCopy(matrixLeft);
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
                    _ = pointD5D.AffineTransformCopy(matrixLeftList);
                };

                ExecuteTest(method, "Com.PointD5D.AffineTransformCopy(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D ex = _GetRandomPointD5D().Normalize;
                Com.PointD5D ey = _GetRandomPointD5D().Normalize;
                Com.PointD5D ez = _GetRandomPointD5D().Normalize;
                Com.PointD5D eu = _GetRandomPointD5D().Normalize;
                Com.PointD5D ev = _GetRandomPointD5D().Normalize;
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
                Com.PointD5D ex = _GetRandomPointD5D().Normalize;
                Com.PointD5D ey = _GetRandomPointD5D().Normalize;
                Com.PointD5D ez = _GetRandomPointD5D().Normalize;
                Com.PointD5D eu = _GetRandomPointD5D().Normalize;
                Com.PointD5D ev = _GetRandomPointD5D().Normalize;
                Com.PointD5D offset = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.InverseAffineTransformCopy(ex, ey, ez, eu, ev, offset);
                };

                ExecuteTest(method, "Com.PointD5D.InverseAffineTransformCopy(Com.PointD5D, Com.PointD5D, Com.PointD5D, Com.PointD5D, Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.Matrix matrixLeft = _GetRandomMatrix(6, 6);

                Action method = () =>
                {
                    _ = pointD5D.InverseAffineTransformCopy(matrixLeft);
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
                    _ = pointD5D.InverseAffineTransformCopy(matrixLeftList);
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
                    _ = pointD5D.ProjectToXYZU(prjCenter, trueLenDist);
                };

                ExecuteTest(method, "Com.PointD5D.ProjectToXYZU(Com.PointD5D, double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D prjCenter = _GetRandomPointD5D();
                double trueLenDist = (pointD5D.X - prjCenter.X) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    _ = pointD5D.ProjectToYZUV(prjCenter, trueLenDist);
                };

                ExecuteTest(method, "Com.PointD5D.ProjectToYZUV(Com.PointD5D, double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D prjCenter = _GetRandomPointD5D();
                double trueLenDist = (pointD5D.Y - prjCenter.Y) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    _ = pointD5D.ProjectToZUVX(prjCenter, trueLenDist);
                };

                ExecuteTest(method, "Com.PointD5D.ProjectToZUVX(Com.PointD5D, double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D prjCenter = _GetRandomPointD5D();
                double trueLenDist = (pointD5D.Z - prjCenter.Z) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    _ = pointD5D.ProjectToUVXY(prjCenter, trueLenDist);
                };

                ExecuteTest(method, "Com.PointD5D.ProjectToUVXY(Com.PointD5D, double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D prjCenter = _GetRandomPointD5D();
                double trueLenDist = (pointD5D.U - prjCenter.U) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    _ = pointD5D.ProjectToVXYZ(prjCenter, trueLenDist);
                };

                ExecuteTest(method, "Com.PointD5D.ProjectToVXYZ(Com.PointD5D, double)");
            }

            // ToVector

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.ToColumnVector();
                };

                ExecuteTest(method, "Com.PointD5D.ToColumnVector()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.ToRowVector();
                };

                ExecuteTest(method, "Com.PointD5D.ToRowVector()");
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
                    _ = Com.PointD5D.Equals(left, right);
                };

                ExecuteTest(method, "Com.PointD5D.Equals(Com.PointD5D, Com.PointD5D)");
            }

            // Compare

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = left;

                Action method = () =>
                {
                    _ = Com.PointD5D.Compare(left, right);
                };

                ExecuteTest(method, "Com.PointD5D.Compare(Com.PointD5D, Com.PointD5D)");
            }

            // Matrix

            {
                Action method = () =>
                {
                    _ = Com.PointD5D.IdentityMatrix();
                };

                ExecuteTest(method, "Com.PointD5D.IdentityMatrix()");
            }

            {
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD5D.OffsetMatrix(d);
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
                    _ = Com.PointD5D.OffsetMatrix(dx, dy, dz, du, dv);
                };

                ExecuteTest(method, "Com.PointD5D.OffsetMatrix(double, double, double, double, double)");
            }

            {
                Com.PointD5D pt = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = Com.PointD5D.OffsetMatrix(pt);
                };

                ExecuteTest(method, "Com.PointD5D.OffsetMatrix(Com.PointD5D)");
            }

            {
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD5D.ScaleMatrix(s);
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
                    _ = Com.PointD5D.ScaleMatrix(sx, sy, sz, su, sv);
                };

                ExecuteTest(method, "Com.PointD5D.ScaleMatrix(double, double, double, double, double)");
            }

            {
                Com.PointD5D pt = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = Com.PointD5D.ScaleMatrix(pt);
                };

                ExecuteTest(method, "Com.PointD5D.ScaleMatrix(Com.PointD5D)");
            }

            {
                int index = Com.Statistics.RandomInteger(3);

                Action method = () =>
                {
                    _ = Com.PointD5D.ReflectMatrix(index);
                };

                ExecuteTest(method, "Com.PointD5D.ReflectMatrix(int)");
            }

            {
                int index1 = Com.Statistics.RandomInteger(5);
                int index2 = Com.Statistics.RandomInteger(4);
                if (index2 == index1) index2++;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD5D.ShearMatrix(index1, index2, angle);
                };

                ExecuteTest(method, "Com.PointD5D.ShearMatrix(int, int, double)");
            }

            {
                int index1 = Com.Statistics.RandomInteger(5);
                int index2 = Com.Statistics.RandomInteger(4);
                if (index2 == index1) index2++;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD5D.RotateMatrix(index1, index2, angle);
                };

                ExecuteTest(method, "Com.PointD5D.RotateMatrix(int, int, double)");
            }

            // 距离与夹角

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = Com.PointD5D.DistanceBetween(left, right);
                };

                ExecuteTest(method, "Com.PointD5D.DistanceBetween(Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = Com.PointD5D.AngleBetween(left, right);
                };

                ExecuteTest(method, "Com.PointD5D.AngleBetween(Com.PointD5D, Com.PointD5D)");
            }

            // 向量乘积

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = Com.PointD5D.DotProduct(left, right);
                };

                ExecuteTest(method, "Com.PointD5D.DotProduct(Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = Com.PointD5D.CrossProduct(left, right);
                };

                ExecuteTest(method, "Com.PointD5D.CrossProduct(Com.PointD5D, Com.PointD5D)");
            }

            // 初等函数

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = Com.PointD5D.Abs(pointD5D);
                };

                ExecuteTest(method, "Com.PointD5D.Abs(Com.PointD5D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = Com.PointD5D.Sign(pointD5D);
                };

                ExecuteTest(method, "Com.PointD5D.Sign(Com.PointD5D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = Com.PointD5D.Ceiling(pointD5D);
                };

                ExecuteTest(method, "Com.PointD5D.Ceiling(Com.PointD5D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = Com.PointD5D.Floor(pointD5D);
                };

                ExecuteTest(method, "Com.PointD5D.Floor(Com.PointD5D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = Com.PointD5D.Round(pointD5D);
                };

                ExecuteTest(method, "Com.PointD5D.Round(Com.PointD5D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = Com.PointD5D.Truncate(pointD5D);
                };

                ExecuteTest(method, "Com.PointD5D.Truncate(Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = Com.PointD5D.Max(left, right);
                };

                ExecuteTest(method, "Com.PointD5D.Max(Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = Com.PointD5D.Min(left, right);
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
                    _ = (left == right);
                };

                ExecuteTest(method, "Com.PointD5D.operator ==(Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = left;

                Action method = () =>
                {
                    _ = (left != right);
                };

                ExecuteTest(method, "Com.PointD5D.operator !=(Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = (left < right);
                };

                ExecuteTest(method, "Com.PointD5D.operator <(Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = (left > right);
                };

                ExecuteTest(method, "Com.PointD5D.operator >(Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = (left <= right);
                };

                ExecuteTest(method, "Com.PointD5D.operator <=(Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = (left >= right);
                };

                ExecuteTest(method, "Com.PointD5D.operator >=(Com.PointD5D, Com.PointD5D)");
            }

            // 运算

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = +pointD5D;
                };

                ExecuteTest(method, "Com.PointD5D.operator +(Com.PointD5D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = -pointD5D;
                };

                ExecuteTest(method, "Com.PointD5D.operator -(Com.PointD5D)");
            }

            {
                Com.PointD5D pt = _GetRandomPointD5D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt + n;
                };

                ExecuteTest(method, "Com.PointD5D.operator +(Com.PointD5D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD5D pt = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = n + pt;
                };

                ExecuteTest(method, "Com.PointD5D.operator +(double, Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = left + right;
                };

                ExecuteTest(method, "Com.PointD5D.operator +(Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D pt = _GetRandomPointD5D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt - n;
                };

                ExecuteTest(method, "Com.PointD5D.operator -(Com.PointD5D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD5D pt = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = n - pt;
                };

                ExecuteTest(method, "Com.PointD5D.operator -(double, Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = left - right;
                };

                ExecuteTest(method, "Com.PointD5D.operator -(Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D pt = _GetRandomPointD5D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt * n;
                };

                ExecuteTest(method, "Com.PointD5D.operator *(Com.PointD5D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD5D pt = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = n * pt;
                };

                ExecuteTest(method, "Com.PointD5D.operator *(double, Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = left * right;
                };

                ExecuteTest(method, "Com.PointD5D.operator *(Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D pt = _GetRandomPointD5D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt / n;
                };

                ExecuteTest(method, "Com.PointD5D.operator /(Com.PointD5D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD5D pt = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = n / pt;
                };

                ExecuteTest(method, "Com.PointD5D.operator /(double, Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = left / right;
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
                return Com.Matrix.Empty;
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
                    _ = new Com.PointD6D(x, y, z, u, v, w);
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
                    _ = pointD6D[index];
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

            // 分量

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.X;
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
                    _ = pointD6D.Y;
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
                    _ = pointD6D.Z;
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
                    _ = pointD6D.U;
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
                    _ = pointD6D.V;
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
                    _ = pointD6D.W;
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

            // Dimension

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.Dimension;
                };

                ExecuteTest(method, "Com.PointD6D.Dimension.get()");
            }

            // Is

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.IsEmpty;
                };

                ExecuteTest(method, "Com.PointD6D.IsEmpty.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.IsZero;
                };

                ExecuteTest(method, "Com.PointD6D.IsZero.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.IsReadOnly;
                };

                ExecuteTest(method, "Com.PointD6D.IsReadOnly.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.IsFixedSize;
                };

                ExecuteTest(method, "Com.PointD6D.IsFixedSize.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.IsNaN;
                };

                ExecuteTest(method, "Com.PointD6D.IsNaN.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.IsInfinity;
                };

                ExecuteTest(method, "Com.PointD6D.IsInfinity.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.IsNaNOrInfinity;
                };

                ExecuteTest(method, "Com.PointD6D.IsNaNOrInfinity.get()");
            }

            // 模

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.Module;
                };

                ExecuteTest(method, "Com.PointD6D.Module.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.ModuleSquared;
                };

                ExecuteTest(method, "Com.PointD6D.ModuleSquared.get()");
            }

            // 向量

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.Opposite;
                };

                ExecuteTest(method, "Com.PointD6D.Opposite.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.Normalize;
                };

                ExecuteTest(method, "Com.PointD6D.Normalize.get()");
            }

            // 子空间分量

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.XYZUV;
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
                    _ = pointD6D.YZUVW;
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
                    _ = pointD6D.ZUVWX;
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
                    _ = pointD6D.UVWXY;
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
                    _ = pointD6D.VWXYZ;
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
                    _ = pointD6D.WXYZU;
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
                    _ = pointD6D.AngleFromX;
                };

                ExecuteTest(method, "Com.PointD6D.AngleFromX.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AngleFromY;
                };

                ExecuteTest(method, "Com.PointD6D.AngleFromY.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AngleFromZ;
                };

                ExecuteTest(method, "Com.PointD6D.AngleFromZ.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AngleFromU;
                };

                ExecuteTest(method, "Com.PointD6D.AngleFromU.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AngleFromV;
                };

                ExecuteTest(method, "Com.PointD6D.AngleFromV.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AngleFromW;
                };

                ExecuteTest(method, "Com.PointD6D.AngleFromW.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AngleFromXYZUV;
                };

                ExecuteTest(method, "Com.PointD6D.AngleFromXYZUV.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AngleFromYZUVW;
                };

                ExecuteTest(method, "Com.PointD6D.AngleFromYZUVW.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AngleFromZUVWX;
                };

                ExecuteTest(method, "Com.PointD6D.AngleFromZUVWX.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AngleFromUVWXY;
                };

                ExecuteTest(method, "Com.PointD6D.AngleFromUVWXY.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AngleFromVWXYZ;
                };

                ExecuteTest(method, "Com.PointD6D.AngleFromVWXYZ.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AngleFromWXYZU;
                };

                ExecuteTest(method, "Com.PointD6D.AngleFromWXYZU.get()");
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
                object obj = pointD6D;

                Action method = () =>
                {
                    _ = pointD6D.Equals(obj);
                };

                ExecuteTest(method, "Com.PointD6D.Equals(object)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.GetHashCode();
                };

                ExecuteTest(method, "Com.PointD6D.GetHashCode()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.ToString();
                };

                ExecuteTest(method, "Com.PointD6D.ToString()");
            }

            // Equals

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = left;

                Action method = () =>
                {
                    _ = left.Equals(right);
                };

                ExecuteTest(method, "Com.PointD6D.Equals(Com.PointD6D)");
            }

            // CompareTo

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                object obj = pointD6D;

                Action method = () =>
                {
                    _ = pointD6D.CompareTo(obj);
                };

                ExecuteTest(method, "Com.PointD6D.CompareTo(object)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = left;

                Action method = () =>
                {
                    _ = left.CompareTo(right);
                };

                ExecuteTest(method, "Com.PointD6D.CompareTo(Com.PointD6D)");
            }

            // 检索

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD6D.IndexOf(item);
                };

                ExecuteTest(method, "Com.PointD6D.IndexOf(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 0;

                Action method = () =>
                {
                    _ = pointD6D.IndexOf(item, startIndex);
                };

                ExecuteTest(method, "Com.PointD6D.IndexOf(double, int)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 0;
                int count = 6;

                Action method = () =>
                {
                    _ = pointD6D.IndexOf(item, startIndex, count);
                };

                ExecuteTest(method, "Com.PointD6D.IndexOf(double, int, int)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD6D.LastIndexOf(item);
                };

                ExecuteTest(method, "Com.PointD6D.LastIndexOf(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 5;

                Action method = () =>
                {
                    _ = pointD6D.LastIndexOf(item, startIndex);
                };

                ExecuteTest(method, "Com.PointD6D.LastIndexOf(double, int)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 5;
                int count = 6;

                Action method = () =>
                {
                    _ = pointD6D.LastIndexOf(item, startIndex, count);
                };

                ExecuteTest(method, "Com.PointD6D.LastIndexOf(double, int, int)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD6D.Contains(item);
                };

                ExecuteTest(method, "Com.PointD6D.Contains(double)");
            }

            // ToArray，ToList

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.ToArray();
                };

                ExecuteTest(method, "Com.PointD6D.ToArray()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.ToList();
                };

                ExecuteTest(method, "Com.PointD6D.ToList()");
            }

            // 坐标系转换

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.ToSpherical();
                };

                ExecuteTest(method, "Com.PointD6D.ToSpherical()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.ToCartesian();
                };

                ExecuteTest(method, "Com.PointD6D.ToCartesian()");
            }

            // 距离与夹角

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D pt = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.DistanceFrom(pt);
                };

                ExecuteTest(method, "Com.PointD6D.DistanceFrom(Com.PointD6D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D pt = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AngleFrom(pt);
                };

                ExecuteTest(method, "Com.PointD6D.AngleFrom(Com.PointD6D)");
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
                    _ = pointD6D.OffsetCopy(d);
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
                    _ = pointD6D.OffsetCopy(dx, dy, dz, du, dv, dw);
                };

                ExecuteTest(method, "Com.PointD6D.OffsetCopy(double, double, double, double, double, double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D pt = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.OffsetCopy(pt);
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
                    _ = pointD6D.ScaleCopy(s);
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
                    _ = pointD6D.ScaleCopy(sx, sy, sz, su, sv, sw);
                };

                ExecuteTest(method, "Com.PointD6D.ScaleCopy(double, double, double, double, double, double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D pt = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.ScaleCopy(pt);
                };

                ExecuteTest(method, "Com.PointD6D.ScaleCopy(Com.PointD6D)");
            }

            // Reflect

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                int index = Com.Statistics.RandomInteger(6);

                Action method = () =>
                {
                    pointD6D.Reflect(index);
                };

                ExecuteTest(method, "Com.PointD6D.Reflect(int)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                int index = Com.Statistics.RandomInteger(6);

                Action method = () =>
                {
                    _ = pointD6D.ReflectCopy(index);
                };

                ExecuteTest(method, "Com.PointD6D.ReflectCopy(int)");
            }

            // Shear

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                int index1 = Com.Statistics.RandomInteger(6);
                int index2 = Com.Statistics.RandomInteger(5);
                if (index2 == index1) index2++;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD6D.Shear(index1, index2, angle);
                };

                ExecuteTest(method, "Com.PointD6D.Shear(int, int, double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                int index1 = Com.Statistics.RandomInteger(6);
                int index2 = Com.Statistics.RandomInteger(5);
                if (index2 == index1) index2++;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD6D.ShearCopy(index1, index2, angle);
                };

                ExecuteTest(method, "Com.PointD6D.ShearCopy(int, int, double)");
            }

            // Rotate

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                int index1 = Com.Statistics.RandomInteger(6);
                int index2 = Com.Statistics.RandomInteger(5);
                if (index2 == index1) index2++;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD6D.Rotate(index1, index2, angle);
                };

                ExecuteTest(method, "Com.PointD6D.Rotate(int, int, double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                int index1 = Com.Statistics.RandomInteger(6);
                int index2 = Com.Statistics.RandomInteger(5);
                if (index2 == index1) index2++;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD6D.RotateCopy(index1, index2, angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateCopy(int, int, double)");
            }

            // Affine

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D ex = _GetRandomPointD6D().Normalize;
                Com.PointD6D ey = _GetRandomPointD6D().Normalize;
                Com.PointD6D ez = _GetRandomPointD6D().Normalize;
                Com.PointD6D eu = _GetRandomPointD6D().Normalize;
                Com.PointD6D ev = _GetRandomPointD6D().Normalize;
                Com.PointD6D ew = _GetRandomPointD6D().Normalize;
                Com.PointD6D offset = _GetRandomPointD6D();

                Action method = () =>
                {
                    pointD6D.AffineTransform(ex, ey, ez, eu, ev, ew, offset);
                };

                ExecuteTest(method, "Com.PointD6D.AffineTransform(Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.Matrix matrixLeft = _GetRandomMatrix(7, 7);

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
                    matrixLeftList.Add(_GetRandomMatrix(7, 7));
                }

                Action method = () =>
                {
                    pointD6D.AffineTransform(matrixLeftList);
                };

                ExecuteTest(method, "Com.PointD6D.AffineTransform(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D ex = _GetRandomPointD6D().Normalize;
                Com.PointD6D ey = _GetRandomPointD6D().Normalize;
                Com.PointD6D ez = _GetRandomPointD6D().Normalize;
                Com.PointD6D eu = _GetRandomPointD6D().Normalize;
                Com.PointD6D ev = _GetRandomPointD6D().Normalize;
                Com.PointD6D ew = _GetRandomPointD6D().Normalize;
                Com.PointD6D offset = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AffineTransformCopy(ex, ey, ez, eu, ev, ew, offset);
                };

                ExecuteTest(method, "Com.PointD6D.AffineTransformCopy(Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.Matrix matrixLeft = _GetRandomMatrix(7, 7);

                Action method = () =>
                {
                    _ = pointD6D.AffineTransformCopy(matrixLeft);
                };

                ExecuteTest(method, "Com.PointD6D.AffineTransformCopy(Com.Matrix)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(7, 7));
                }

                Action method = () =>
                {
                    _ = pointD6D.AffineTransformCopy(matrixLeftList);
                };

                ExecuteTest(method, "Com.PointD6D.AffineTransformCopy(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D ex = _GetRandomPointD6D().Normalize;
                Com.PointD6D ey = _GetRandomPointD6D().Normalize;
                Com.PointD6D ez = _GetRandomPointD6D().Normalize;
                Com.PointD6D eu = _GetRandomPointD6D().Normalize;
                Com.PointD6D ev = _GetRandomPointD6D().Normalize;
                Com.PointD6D ew = _GetRandomPointD6D().Normalize;
                Com.PointD6D offset = _GetRandomPointD6D();

                Action method = () =>
                {
                    pointD6D.InverseAffineTransform(ex, ey, ez, eu, ev, ew, offset);
                };

                ExecuteTest(method, "Com.PointD6D.InverseAffineTransform(Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.Matrix matrixLeft = _GetRandomMatrix(7, 7);

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
                    matrixLeftList.Add(_GetRandomMatrix(7, 7));
                }

                Action method = () =>
                {
                    pointD6D.InverseAffineTransform(matrixLeftList);
                };

                ExecuteTest(method, "Com.PointD6D.InverseAffineTransform(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D ex = _GetRandomPointD6D().Normalize;
                Com.PointD6D ey = _GetRandomPointD6D().Normalize;
                Com.PointD6D ez = _GetRandomPointD6D().Normalize;
                Com.PointD6D eu = _GetRandomPointD6D().Normalize;
                Com.PointD6D ev = _GetRandomPointD6D().Normalize;
                Com.PointD6D ew = _GetRandomPointD6D().Normalize;
                Com.PointD6D offset = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.InverseAffineTransformCopy(ex, ey, ez, eu, ev, ew, offset);
                };

                ExecuteTest(method, "Com.PointD6D.InverseAffineTransformCopy(Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.Matrix matrixLeft = _GetRandomMatrix(7, 7);

                Action method = () =>
                {
                    _ = pointD6D.InverseAffineTransformCopy(matrixLeft);
                };

                ExecuteTest(method, "Com.PointD6D.InverseAffineTransformCopy(Com.Matrix)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(7, 7));
                }

                Action method = () =>
                {
                    _ = pointD6D.InverseAffineTransformCopy(matrixLeftList);
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
                    _ = pointD6D.ProjectToXYZUV(prjCenter, trueLenDist);
                };

                ExecuteTest(method, "Com.PointD6D.ProjectToXYZUV(Com.PointD6D, double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D prjCenter = _GetRandomPointD6D();
                double trueLenDist = (pointD6D.X - prjCenter.X) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    _ = pointD6D.ProjectToYZUVW(prjCenter, trueLenDist);
                };

                ExecuteTest(method, "Com.PointD6D.ProjectToYZUVW(Com.PointD6D, double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D prjCenter = _GetRandomPointD6D();
                double trueLenDist = (pointD6D.Y - prjCenter.Y) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    _ = pointD6D.ProjectToZUVWX(prjCenter, trueLenDist);
                };

                ExecuteTest(method, "Com.PointD6D.ProjectToZUVWX(Com.PointD6D, double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D prjCenter = _GetRandomPointD6D();
                double trueLenDist = (pointD6D.Z - prjCenter.Z) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    _ = pointD6D.ProjectToUVWXY(prjCenter, trueLenDist);
                };

                ExecuteTest(method, "Com.PointD6D.ProjectToUVWXY(Com.PointD6D, double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D prjCenter = _GetRandomPointD6D();
                double trueLenDist = (pointD6D.U - prjCenter.U) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    _ = pointD6D.ProjectToVWXYZ(prjCenter, trueLenDist);
                };

                ExecuteTest(method, "Com.PointD6D.ProjectToVWXYZ(Com.PointD6D, double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D prjCenter = _GetRandomPointD6D();
                double trueLenDist = (pointD6D.V - prjCenter.V) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    _ = pointD6D.ProjectToWXYZU(prjCenter, trueLenDist);
                };

                ExecuteTest(method, "Com.PointD6D.ProjectToWXYZU(Com.PointD6D, double)");
            }

            // ToVector

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.ToColumnVector();
                };

                ExecuteTest(method, "Com.PointD6D.ToColumnVector()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.ToRowVector();
                };

                ExecuteTest(method, "Com.PointD6D.ToRowVector()");
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
                    _ = Com.PointD6D.Equals(left, right);
                };

                ExecuteTest(method, "Com.PointD6D.Equals(Com.PointD6D, Com.PointD6D)");
            }

            // Compare

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = left;

                Action method = () =>
                {
                    _ = Com.PointD6D.Compare(left, right);
                };

                ExecuteTest(method, "Com.PointD6D.Compare(Com.PointD6D, Com.PointD6D)");
            }

            // Matrix

            {
                Action method = () =>
                {
                    _ = Com.PointD6D.IdentityMatrix();
                };

                ExecuteTest(method, "Com.PointD6D.IdentityMatrix()");
            }

            {
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD6D.OffsetMatrix(d);
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
                    _ = Com.PointD6D.OffsetMatrix(dx, dy, dz, du, dv, dw);
                };

                ExecuteTest(method, "Com.PointD6D.OffsetMatrix(double, double, double, double, double, double)");
            }

            {
                Com.PointD6D pt = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = Com.PointD6D.OffsetMatrix(pt);
                };

                ExecuteTest(method, "Com.PointD6D.OffsetMatrix(Com.PointD6D)");
            }

            {
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD6D.ScaleMatrix(s);
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
                    _ = Com.PointD6D.ScaleMatrix(sx, sy, sz, su, sv, sw);
                };

                ExecuteTest(method, "Com.PointD6D.ScaleMatrix(double, double, double, double, double, double)");
            }

            {
                Com.PointD6D pt = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = Com.PointD6D.ScaleMatrix(pt);
                };

                ExecuteTest(method, "Com.PointD6D.ScaleMatrix(Com.PointD6D)");
            }

            {
                int index = Com.Statistics.RandomInteger(3);

                Action method = () =>
                {
                    _ = Com.PointD6D.ReflectMatrix(index);
                };

                ExecuteTest(method, "Com.PointD6D.ReflectMatrix(int)");
            }

            {
                int index1 = Com.Statistics.RandomInteger(6);
                int index2 = Com.Statistics.RandomInteger(5);
                if (index2 == index1) index2++;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD6D.ShearMatrix(index1, index2, angle);
                };

                ExecuteTest(method, "Com.PointD6D.ShearMatrix(int, int, double)");
            }

            {
                int index1 = Com.Statistics.RandomInteger(6);
                int index2 = Com.Statistics.RandomInteger(5);
                if (index2 == index1) index2++;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD6D.RotateMatrix(index1, index2, angle);
                };

                ExecuteTest(method, "Com.PointD6D.RotateMatrix(int, int, double)");
            }

            // 距离与夹角

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = Com.PointD6D.DistanceBetween(left, right);
                };

                ExecuteTest(method, "Com.PointD6D.DistanceBetween(Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = Com.PointD6D.AngleBetween(left, right);
                };

                ExecuteTest(method, "Com.PointD6D.AngleBetween(Com.PointD6D, Com.PointD6D)");
            }

            // 向量乘积

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = Com.PointD6D.DotProduct(left, right);
                };

                ExecuteTest(method, "Com.PointD6D.DotProduct(Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = Com.PointD6D.CrossProduct(left, right);
                };

                ExecuteTest(method, "Com.PointD6D.CrossProduct(Com.PointD6D, Com.PointD6D)");
            }

            // 初等函数

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = Com.PointD6D.Abs(pointD6D);
                };

                ExecuteTest(method, "Com.PointD6D.Abs(Com.PointD6D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = Com.PointD6D.Sign(pointD6D);
                };

                ExecuteTest(method, "Com.PointD6D.Sign(Com.PointD6D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = Com.PointD6D.Ceiling(pointD6D);
                };

                ExecuteTest(method, "Com.PointD6D.Ceiling(Com.PointD6D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = Com.PointD6D.Floor(pointD6D);
                };

                ExecuteTest(method, "Com.PointD6D.Floor(Com.PointD6D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = Com.PointD6D.Round(pointD6D);
                };

                ExecuteTest(method, "Com.PointD6D.Round(Com.PointD6D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = Com.PointD6D.Truncate(pointD6D);
                };

                ExecuteTest(method, "Com.PointD6D.Truncate(Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = Com.PointD6D.Max(left, right);
                };

                ExecuteTest(method, "Com.PointD6D.Max(Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = Com.PointD6D.Min(left, right);
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
                    _ = (left == right);
                };

                ExecuteTest(method, "Com.PointD6D.operator ==(Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = left;

                Action method = () =>
                {
                    _ = (left != right);
                };

                ExecuteTest(method, "Com.PointD6D.operator !=(Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = (left < right);
                };

                ExecuteTest(method, "Com.PointD6D.operator <(Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = (left > right);
                };

                ExecuteTest(method, "Com.PointD6D.operator >(Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = (left <= right);
                };

                ExecuteTest(method, "Com.PointD6D.operator <=(Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = (left >= right);
                };

                ExecuteTest(method, "Com.PointD6D.operator >=(Com.PointD6D, Com.PointD6D)");
            }

            // 运算

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = +pointD6D;
                };

                ExecuteTest(method, "Com.PointD6D.operator +(Com.PointD6D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = -pointD6D;
                };

                ExecuteTest(method, "Com.PointD6D.operator -(Com.PointD6D)");
            }

            {
                Com.PointD6D pt = _GetRandomPointD6D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt + n;
                };

                ExecuteTest(method, "Com.PointD6D.operator +(Com.PointD6D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD6D pt = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = n + pt;
                };

                ExecuteTest(method, "Com.PointD6D.operator +(double, Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = left + right;
                };

                ExecuteTest(method, "Com.PointD6D.operator +(Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D pt = _GetRandomPointD6D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt - n;
                };

                ExecuteTest(method, "Com.PointD6D.operator -(Com.PointD6D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD6D pt = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = n - pt;
                };

                ExecuteTest(method, "Com.PointD6D.operator -(double, Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = left - right;
                };

                ExecuteTest(method, "Com.PointD6D.operator -(Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D pt = _GetRandomPointD6D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt * n;
                };

                ExecuteTest(method, "Com.PointD6D.operator *(Com.PointD6D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD6D pt = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = n * pt;
                };

                ExecuteTest(method, "Com.PointD6D.operator *(double, Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = left * right;
                };

                ExecuteTest(method, "Com.PointD6D.operator *(Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D pt = _GetRandomPointD6D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt / n;
                };

                ExecuteTest(method, "Com.PointD6D.operator /(Com.PointD6D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD6D pt = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = n / pt;
                };

                ExecuteTest(method, "Com.PointD6D.operator /(double, Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = left / right;
                };

                ExecuteTest(method, "Com.PointD6D.operator /(Com.PointD6D, Com.PointD6D)");
            }
        }
    }

    sealed class RealTest : ClassPerformanceTestBase
    {
        private static Com.Real _GetRandomReal()
        {
            return new Com.Real(Com.Statistics.RandomDouble(1, 10), Com.Statistics.RandomInteger());
        }

        //

        protected override void Constructor()
        {
            {
                double value = Com.Statistics.RandomDouble(-10, 10);
                long magnitude = Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = new Com.Real(value, magnitude);
                };

                ExecuteTest(method, "Com.Real.Real(double, long)");
            }

            {
                double value = Com.Statistics.RandomDouble(-1E150, 1E150);

                Action method = () =>
                {
                    _ = new Com.Real(value);
                };

                ExecuteTest(method, "Com.Real.Real(double)");
            }

            {
                float value = (float)Com.Statistics.RandomDouble(-1E30, 1E30);

                Action method = () =>
                {
                    _ = new Com.Real(value);
                };

                ExecuteTest(method, "Com.Real.Real(float)");
            }

            {
                decimal value = (decimal)Com.Statistics.RandomDouble(-1E20, 1E20);

                Action method = () =>
                {
                    _ = new Com.Real(value);
                };

                ExecuteTest(method, "Com.Real.Real(decimal)");
            }

            {
                ulong value = unchecked((ulong)Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    _ = new Com.Real(value);
                };

                ExecuteTest(method, "Com.Real.Real(ulong)");
            }

            {
                long value = Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = new Com.Real(value);
                };

                ExecuteTest(method, "Com.Real.Real(long)");
            }

            {
                uint value = unchecked((uint)Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    _ = new Com.Real(value);
                };

                ExecuteTest(method, "Com.Real.Real(uint)");
            }

            {
                int value = Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = new Com.Real(value);
                };

                ExecuteTest(method, "Com.Real.Real(int)");
            }

            {
                ushort value = unchecked((ushort)Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    _ = new Com.Real(value);
                };

                ExecuteTest(method, "Com.Real.Real(ushort)");
            }

            {
                short value = unchecked((short)Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    _ = new Com.Real(value);
                };

                ExecuteTest(method, "Com.Real.Real(short)");
            }

            {
                byte value = unchecked((byte)Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    _ = new Com.Real(value);
                };

                ExecuteTest(method, "Com.Real.Real(byte)");
            }

            {
                sbyte value = unchecked((sbyte)Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    _ = new Com.Real(value);
                };

                ExecuteTest(method, "Com.Real.Real(sbyte)");
            }
        }

        protected override void Property()
        {
            // Is

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.IsNaN;
                };

                ExecuteTest(method, "Com.Real.IsNaN.get()");
            }

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.IsPositiveInfinity;
                };

                ExecuteTest(method, "Com.Real.IsPositiveInfinity.get()");
            }

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.IsNegativeInfinity;
                };

                ExecuteTest(method, "Com.Real.IsNegativeInfinity.get()");
            }

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.IsInfinity;
                };

                ExecuteTest(method, "Com.Real.IsInfinity.get()");
            }

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.IsNaNOrInfinity;
                };

                ExecuteTest(method, "Com.Real.IsNaNOrInfinity.get()");
            }

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.IsZero;
                };

                ExecuteTest(method, "Com.Real.IsZero.get()");
            }

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.IsOne;
                };

                ExecuteTest(method, "Com.Real.IsOne.get()");
            }

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.IsMinusOne;
                };

                ExecuteTest(method, "Com.Real.IsMinusOne.get()");
            }

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.IsPositive;
                };

                ExecuteTest(method, "Com.Real.IsPositive.get()");
            }

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.IsNegative;
                };

                ExecuteTest(method, "Com.Real.IsNegative.get()");
            }

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.IsInteger;
                };

                ExecuteTest(method, "Com.Real.IsInteger.get()");
            }

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.IsDecimal;
                };

                ExecuteTest(method, "Com.Real.IsDecimal.get()");
            }

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.IsEven;
                };

                ExecuteTest(method, "Com.Real.IsEven.get()");
            }

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.IsOdd;
                };

                ExecuteTest(method, "Com.Real.IsOdd.get()");
            }

            // 分量

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.Value;
                };

                ExecuteTest(method, "Com.Real.Value.get()");
            }

            {
                Com.Real real = _GetRandomReal();
                double value = Com.Statistics.RandomDouble(1, 10);

                Action method = () =>
                {
                    real.Value = value;
                };

                ExecuteTest(method, "Com.Real.Value.set(double)");
            }

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.Magnitude;
                };

                ExecuteTest(method, "Com.Real.Magnitude.get()");
            }

            {
                Com.Real real = _GetRandomReal();
                long value = Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    real.Magnitude = value;
                };

                ExecuteTest(method, "Com.Real.Magnitude.set(long)");
            }
        }

        protected override void StaticProperty()
        {

        }

        protected override void Method()
        {
            // object

            {
                Com.Real real = _GetRandomReal();
                object obj = real;

                Action method = () =>
                {
                    _ = real.Equals(obj);
                };

                ExecuteTest(method, "Com.Real.Equals(object)");
            }

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.GetHashCode();
                };

                ExecuteTest(method, "Com.Real.GetHashCode()");
            }

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.ToString();
                };

                ExecuteTest(method, "Com.Real.ToString()");
            }

            // Equals

            {
                Com.Real left = _GetRandomReal();
                Com.Real right = left;

                Action method = () =>
                {
                    _ = left.Equals(right);
                };

                ExecuteTest(method, "Com.Real.Equals(Com.Real)");
            }

            // CompareTo

            {
                Com.Real left = _GetRandomReal();
                object right = left;

                Action method = () =>
                {
                    _ = left.CompareTo(right);
                };

                ExecuteTest(method, "Com.Real.CompareTo(object)");
            }

            {
                Com.Real left = _GetRandomReal();
                Com.Real right = left;

                Action method = () =>
                {
                    _ = left.CompareTo(right);
                };

                ExecuteTest(method, "Com.Real.CompareTo(Com.Real)");
            }
        }

        protected override void StaticMethod()
        {
            // Equals

            {
                Com.Real left = _GetRandomReal();
                Com.Real right = left;

                Action method = () =>
                {
                    _ = Com.Real.Equals(left, right);
                };

                ExecuteTest(method, "Com.Real.Equals(Com.Real, Com.Real)");
            }

            // Compare

            {
                Com.Real left = _GetRandomReal();
                Com.Real right = left;

                Action method = () =>
                {
                    _ = Com.Real.Compare(left, right);
                };

                ExecuteTest(method, "Com.Real.CompareTo(Com.Real, Com.Real)");
            }

            // 幂函数，指数函数，对数函数

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Sqr(real);
                };

                ExecuteTest(method, "Com.Real.Sqr(Com.Real)");
            }

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Sqrt(real);
                };

                ExecuteTest(method, "Com.Real.Sqrt(Com.Real)");
            }

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Exp10(real);
                };

                ExecuteTest(method, "Com.Real.Exp10(Com.Real)");
            }

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Exp(real);
                };

                ExecuteTest(method, "Com.Real.Exp(Com.Real)");
            }

            {
                Com.Real left = _GetRandomReal();
                Com.Real right = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Pow(left, right);
                };

                ExecuteTest(method, "Com.Real.Pow(Com.Real, Com.Real)");
            }

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Log10(real);
                };

                ExecuteTest(method, "Com.Real.Log10(Com.Real)");
            }

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Log(real);
                };

                ExecuteTest(method, "Com.Real.Log(Com.Real)");
            }

            {
                Com.Real left = _GetRandomReal();
                Com.Real right = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Log(left, right);
                };

                ExecuteTest(method, "Com.Real.Log(Com.Real, Com.Real)");
            }

            // 三角函数

            {
                Com.Real real = new Com.Real(Com.Statistics.RandomDouble(1, 10), 4096);

                Action method = () =>
                {
                    _ = Com.Real.Sin(real);
                };

                ExecuteTest(method, "Com.Real.Sin(Com.Real)", "real at magnitude of 4096");
            }

            {
                Com.Real real = new Com.Real(Com.Statistics.RandomDouble(1, 10), 4096);

                Action method = () =>
                {
                    _ = Com.Real.Cos(real);
                };

                ExecuteTest(method, "Com.Real.Cos(Com.Real)", "real at magnitude of 4096");
            }

            {
                Com.Real real = new Com.Real(Com.Statistics.RandomDouble(1, 10), 4096);

                Action method = () =>
                {
                    _ = Com.Real.Tan(real);
                };

                ExecuteTest(method, "Com.Real.Tan(Com.Real)", "real at magnitude of 4096");
            }

            {
                Com.Real real = Com.Statistics.RandomDouble(-1, 1);

                Action method = () =>
                {
                    _ = Com.Real.Asin(real);
                };

                ExecuteTest(method, "Com.Real.Asin(Com.Real)");
            }

            {
                Com.Real real = Com.Statistics.RandomDouble(-1, 1);

                Action method = () =>
                {
                    _ = Com.Real.Acos(real);
                };

                ExecuteTest(method, "Com.Real.Acos(Com.Real)");
            }

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Atan(real);
                };

                ExecuteTest(method, "Com.Real.Atan(Com.Real)");
            }

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Sinh(real);
                };

                ExecuteTest(method, "Com.Real.Sinh(Com.Real)");
            }

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Cosh(real);
                };

                ExecuteTest(method, "Com.Real.Cosh(Com.Real)");
            }

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Tanh(real);
                };

                ExecuteTest(method, "Com.Real.Tanh(Com.Real)");
            }

            {
                Com.Real real = Com.Real.Abs(_GetRandomReal());

                Action method = () =>
                {
                    _ = Com.Real.Asinh(real);
                };

                ExecuteTest(method, "Com.Real.Asinh(Com.Real)");
            }

            {
                Com.Real real = Com.Real.Abs(_GetRandomReal());

                Action method = () =>
                {
                    _ = Com.Real.Acosh(real);
                };

                ExecuteTest(method, "Com.Real.Acosh(Com.Real)");
            }

            {
                Com.Real real = Com.Statistics.RandomDouble(-1, 1);

                Action method = () =>
                {
                    _ = Com.Real.Atanh(real);
                };

                ExecuteTest(method, "Com.Real.Atanh(Com.Real)");
            }

            // 初等函数

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Abs(real);
                };

                ExecuteTest(method, "Com.Real.Abs(Com.Real)");
            }

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Sign(real);
                };

                ExecuteTest(method, "Com.Real.Sign(Com.Real)");
            }

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Ceiling(real);
                };

                ExecuteTest(method, "Com.Real.Ceiling(Com.Real)");
            }

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Floor(real);
                };

                ExecuteTest(method, "Com.Real.Floor(Com.Real)");
            }

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Round(real);
                };

                ExecuteTest(method, "Com.Real.Round(Com.Real)");
            }

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Truncate(real);
                };

                ExecuteTest(method, "Com.Real.Truncate(Com.Real)");
            }

            {
                Com.Real left = _GetRandomReal();
                Com.Real right = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Max(left, right);
                };

                ExecuteTest(method, "Com.Real.Max(Com.Real, Com.Real)");
            }

            {
                Com.Real left = _GetRandomReal();
                Com.Real right = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Min(left, right);
                };

                ExecuteTest(method, "Com.Real.Min(Com.Real, Com.Real)");
            }
        }

        protected override void Operator()
        {
            // 比较

            {
                Com.Real left = _GetRandomReal();
                Com.Real right = left;

                Action method = () =>
                {
                    _ = (left == right);
                };

                ExecuteTest(method, "Com.Real.operator ==(Com.Real, Com.Real)");
            }

            {
                Com.Real left = _GetRandomReal();
                Com.Real right = left;

                Action method = () =>
                {
                    _ = (left != right);
                };

                ExecuteTest(method, "Com.Real.operator !=(Com.Real, Com.Real)");
            }

            {
                Com.Real left = _GetRandomReal();
                Com.Real right = _GetRandomReal();

                Action method = () =>
                {
                    _ = (left < right);
                };

                ExecuteTest(method, "Com.Real.operator <(Com.Real, Com.Real)");
            }

            {
                Com.Real left = _GetRandomReal();
                Com.Real right = _GetRandomReal();

                Action method = () =>
                {
                    _ = (left > right);
                };

                ExecuteTest(method, "Com.Real.operator >(Com.Real, Com.Real)");
            }

            {
                Com.Real left = _GetRandomReal();
                Com.Real right = _GetRandomReal();

                Action method = () =>
                {
                    _ = (left <= right);
                };

                ExecuteTest(method, "Com.Real.operator <=(Com.Real, Com.Real)");
            }

            {
                Com.Real left = _GetRandomReal();
                Com.Real right = _GetRandomReal();

                Action method = () =>
                {
                    _ = (left >= right);
                };

                ExecuteTest(method, "Com.Real.operator >=(Com.Real, Com.Real)");
            }

            // 运算

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = +real;
                };

                ExecuteTest(method, "Com.Real.operator +(Com.Real)");
            }

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = -real;
                };

                ExecuteTest(method, "Com.Real.operator -(Com.Real)");
            }

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    real++;
                };

                ExecuteTest(method, "Com.Real.operator ++(Com.Real)");
            }

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    real--;
                };

                ExecuteTest(method, "Com.Real.operator --(Com.Real)");
            }

            {
                Com.Real left = _GetRandomReal();
                Com.Real right = _GetRandomReal();

                Action method = () =>
                {
                    _ = left + right;
                };

                ExecuteTest(method, "Com.Real.operator +(Com.Real, Com.Real)");
            }

            {
                Com.Real left = _GetRandomReal();
                Com.Real right = _GetRandomReal();

                Action method = () =>
                {
                    _ = left - right;
                };

                ExecuteTest(method, "Com.Real.operator -(Com.Real, Com.Real)");
            }

            {
                Com.Real left = _GetRandomReal();
                Com.Real right = _GetRandomReal();

                Action method = () =>
                {
                    _ = left * right;
                };

                ExecuteTest(method, "Com.Real.operator *(Com.Real, Com.Real)");
            }

            {
                Com.Real left = _GetRandomReal();
                Com.Real right = _GetRandomReal();

                Action method = () =>
                {
                    _ = left / right;
                };

                ExecuteTest(method, "Com.Real.operator /(Com.Real, Com.Real)");
            }

            {
                Com.Real left = new Com.Real(Com.Statistics.RandomDouble(1, 10), 4096);
                Com.Real right = new Com.Real(Com.Statistics.RandomDouble(1, 10), 256);

                Action method = () =>
                {
                    _ = left % right;
                };

                ExecuteTest(method, "Com.Real.operator %(Com.Real, Com.Real)", "left at magnitude of 4096, right at magnitude of 256");
            }

            {
                Com.Real left = _GetRandomReal();
                Com.Real right = _GetRandomReal();

                Action method = () =>
                {
                    _ = left ^ right;
                };

                ExecuteTest(method, "Com.Real.operator ^(Com.Real, Com.Real)");
            }

            // 类型转换

            {
                Com.Real real = new Com.Real(Com.Statistics.RandomDouble(-1E150, 1E150));

                Action method = () =>
                {
                    _ = (double)real;
                };

                ExecuteTest(method, "Com.Real.explicit operator double(Com.Real)");
            }

            {
                Com.Real real = new Com.Real(Com.Statistics.RandomDouble(-1E30, 1E30));

                Action method = () =>
                {
                    _ = (float)real;
                };

                ExecuteTest(method, "Com.Real.explicit operator float(Com.Real)");
            }

            {
                Com.Real real = new Com.Real(Com.Statistics.RandomDouble(-1E20, 1E20));

                Action method = () =>
                {
                    _ = (decimal)real;
                };

                ExecuteTest(method, "Com.Real.explicit operator decimal(Com.Real)");
            }

            {
                Com.Real real = new Com.Real(unchecked((ulong)Com.Statistics.RandomInteger()));

                Action method = () =>
                {
                    _ = (ulong)real;
                };

                ExecuteTest(method, "Com.Real.explicit operator ulong(Com.Real)");
            }

            {
                Com.Real real = new Com.Real(Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    _ = (long)real;
                };

                ExecuteTest(method, "Com.Real.explicit operator long(Com.Real)");
            }

            {
                Com.Real real = new Com.Real(unchecked((uint)Com.Statistics.RandomInteger()));

                Action method = () =>
                {
                    _ = (uint)real;
                };

                ExecuteTest(method, "Com.Real.explicit operator uint(Com.Real)");
            }

            {
                Com.Real real = new Com.Real(Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    _ = (int)real;
                };

                ExecuteTest(method, "Com.Real.explicit operator int(Com.Real)");
            }

            {
                Com.Real real = new Com.Real(unchecked((ushort)Com.Statistics.RandomInteger()));

                Action method = () =>
                {
                    _ = (ushort)real;
                };

                ExecuteTest(method, "Com.Real.explicit operator ushort(Com.Real)");
            }

            {
                Com.Real real = new Com.Real(unchecked((short)Com.Statistics.RandomInteger()));

                Action method = () =>
                {
                    _ = (short)real;
                };

                ExecuteTest(method, "Com.Real.explicit operator short(Com.Real)");
            }

            {
                Com.Real real = new Com.Real(unchecked((byte)Com.Statistics.RandomInteger()));

                Action method = () =>
                {
                    _ = (byte)real;
                };

                ExecuteTest(method, "Com.Real.explicit operator byte(Com.Real)");
            }

            {
                Com.Real real = new Com.Real(unchecked((sbyte)Com.Statistics.RandomInteger()));

                Action method = () =>
                {
                    _ = (sbyte)real;
                };

                ExecuteTest(method, "Com.Real.explicit operator sbyte(Com.Real)");
            }

            {
                double value = Com.Statistics.RandomDouble(-1E150, 1E150);

                Action method = () =>
                {
                    _ = (Com.Real)value;
                };

                ExecuteTest(method, "Com.Real.implicit operator Real(double)");
            }

            {
                float value = (float)Com.Statistics.RandomDouble(-1E30, 1E30);

                Action method = () =>
                {
                    _ = (Com.Real)value;
                };

                ExecuteTest(method, "Com.Real.implicit operator Real(float)");
            }

            {
                decimal value = (decimal)Com.Statistics.RandomDouble(-1E20, 1E20);

                Action method = () =>
                {
                    _ = (Com.Real)value;
                };

                ExecuteTest(method, "Com.Real.explicit operator Real(decimal)");
            }

            {
                ulong value = unchecked((ulong)Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    _ = (Com.Real)value;
                };

                ExecuteTest(method, "Com.Real.implicit operator Real(ulong)");
            }

            {
                long value = Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = (Com.Real)value;
                };

                ExecuteTest(method, "Com.Real.implicit operator Real(long)");
            }

            {
                uint value = unchecked((uint)Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    _ = (Com.Real)value;
                };

                ExecuteTest(method, "Com.Real.implicit operator Real(uint)");
            }

            {
                int value = Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = (Com.Real)value;
                };

                ExecuteTest(method, "Com.Real.implicit operator Real(int)");
            }

            {
                ushort value = unchecked((ushort)Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    _ = (Com.Real)value;
                };

                ExecuteTest(method, "Com.Real.implicit operator Real(ushort)");
            }

            {
                short value = unchecked((short)Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    _ = (Com.Real)value;
                };

                ExecuteTest(method, "Com.Real.implicit operator Real(short)");
            }

            {
                byte value = unchecked((byte)Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    _ = (Com.Real)value;
                };

                ExecuteTest(method, "Com.Real.implicit operator Real(byte)");
            }

            {
                sbyte value = unchecked((sbyte)Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    _ = (Com.Real)value;
                };

                ExecuteTest(method, "Com.Real.implicit operator Real(sbyte)");
            }
        }
    }

    sealed class StatisticsTest : ClassPerformanceTestBase
    {
        private static sbyte[] _GetRandomSbyteArray(int size)
        {
            if (size > 0)
            {
                sbyte[] array = new sbyte[size];

                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = unchecked((sbyte)Com.Statistics.RandomInteger());
                }

                return array;
            }
            else
            {
                return new sbyte[0];
            }
        }

        private static byte[] _GetRandomByteArray(int size)
        {
            if (size > 0)
            {
                byte[] array = new byte[size];

                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = unchecked((byte)Com.Statistics.RandomInteger());
                }

                return array;
            }
            else
            {
                return new byte[0];
            }
        }

        private static short[] _GetRandomShortArray(int size)
        {
            if (size > 0)
            {
                short[] array = new short[size];

                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = unchecked((short)Com.Statistics.RandomInteger());
                }

                return array;
            }
            else
            {
                return new short[0];
            }
        }

        private static ushort[] _GetRandomUshortArray(int size)
        {
            if (size > 0)
            {
                ushort[] array = new ushort[size];

                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = unchecked((ushort)Com.Statistics.RandomInteger());
                }

                return array;
            }
            else
            {
                return new ushort[0];
            }
        }

        private static int[] _GetRandomIntArray(int size)
        {
            if (size > 0)
            {
                int[] array = new int[size];

                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = Com.Statistics.RandomInteger();
                }

                return array;
            }
            else
            {
                return new int[0];
            }
        }

        private static uint[] _GetRandomUintArray(int size)
        {
            if (size > 0)
            {
                uint[] array = new uint[size];

                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = unchecked((uint)Com.Statistics.RandomInteger());
                }

                return array;
            }
            else
            {
                return new uint[0];
            }
        }

        private static long[] _GetRandomLongArray(int size)
        {
            if (size > 0)
            {
                long[] array = new long[size];

                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = Com.Statistics.RandomInteger();
                }

                return array;
            }
            else
            {
                return new long[0];
            }
        }

        private static ulong[] _GetRandomUlongArray(int size)
        {
            if (size > 0)
            {
                ulong[] array = new ulong[size];

                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = unchecked((ulong)Com.Statistics.RandomInteger());
                }

                return array;
            }
            else
            {
                return new ulong[0];
            }
        }

        private static decimal[] _GetRandomDecimalArray(int size)
        {
            if (size > 0)
            {
                decimal[] array = new decimal[size];

                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = Com.Statistics.RandomInteger();
                }

                return array;
            }
            else
            {
                return new decimal[0];
            }
        }

        private static float[] _GetRandomFloatArray(int size)
        {
            if (size > 0)
            {
                float[] array = new float[size];

                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = Com.Statistics.RandomInteger();
                }

                return array;
            }
            else
            {
                return new float[0];
            }
        }

        private static double[] _GetRandomDoubleArray(int size)
        {
            if (size > 0)
            {
                double[] array = new double[size];

                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = Com.Statistics.RandomInteger();
                }

                return array;
            }
            else
            {
                return new double[0];
            }
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
            // 随机数

            {
                Action method = () =>
                {
                    _ = Com.Statistics.RandomInteger();
                };

                ExecuteTest(method, "Com.Statistics.RandomInteger()");
            }

            {
                int right = Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = Com.Statistics.RandomInteger(right);
                };

                ExecuteTest(method, "Com.Statistics.RandomInteger(int)");
            }

            {
                int right = Com.Statistics.RandomInteger();
                int left = -Com.Statistics.RandomInteger(right / 2);

                Action method = () =>
                {
                    _ = Com.Statistics.RandomInteger(left, right);
                };

                ExecuteTest(method, "Com.Statistics.RandomInteger(int, int)");
            }

            {
                Action method = () =>
                {
                    _ = Com.Statistics.RandomDouble();
                };

                ExecuteTest(method, "Com.Statistics.RandomDouble()");
            }

            {
                double right = Com.Statistics.RandomDouble(1E18);

                Action method = () =>
                {
                    _ = Com.Statistics.RandomDouble(right);
                };

                ExecuteTest(method, "Com.Statistics.RandomDouble(double)");
            }

            {
                double right = Com.Statistics.RandomDouble(1E18);
                double left = -Com.Statistics.RandomDouble(right / 2);

                Action method = () =>
                {
                    _ = Com.Statistics.RandomDouble(left, right);
                };

                ExecuteTest(method, "Com.Statistics.RandomDouble(double, double)");
            }

            {
                Action method = () =>
                {
                    _ = Com.Statistics.NormalDistributionRandomInteger();
                };

                ExecuteTest(method, "Com.Statistics.NormalDistributionRandomInteger()");
            }

            {
                double ev = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sd = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.Statistics.NormalDistributionRandomInteger(ev, sd);
                };

                ExecuteTest(method, "Com.Statistics.NormalDistributionRandomInteger(double, double)");
            }

            {
                Action method = () =>
                {
                    _ = Com.Statistics.NormalDistributionRandomDouble();
                };

                ExecuteTest(method, "Com.Statistics.NormalDistributionRandomDouble()");
            }

            {
                double ev = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sd = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.Statistics.NormalDistributionRandomDouble(ev, sd);
                };

                ExecuteTest(method, "Com.Statistics.NormalDistributionRandomDouble(double, double)");
            }

            // 排列组合

            {
                double total = 2097152;
                double selection = 1048576;

                Action method = () =>
                {
                    _ = Com.Statistics.Arrangement(total, selection);
                };

                ExecuteTest(method, "Com.Statistics.Arrangement(double, double)", "total at 2097152, selection at 1048576");
            }

            {
                double total = 2097152;
                double selection = 1048576;

                Action method = () =>
                {
                    _ = Com.Statistics.Combination(total, selection);
                };

                ExecuteTest(method, "Com.Statistics.Combination(double, double)", "total at 2097152, selection at 1048576");
            }

            // 分布

            {
                int value = 1048576;
                double p = 0.5;

                Action method = () =>
                {
                    _ = Com.Statistics.GeometricDistributionProbability(value, p);
                };

                ExecuteTest(method, "Com.Statistics.GeometricDistributionProbability(int, double)", "value at 1048576, p at 0.5");
            }

            {
                int value = 1048576;
                int N = 8388608;
                int M = 4194304;
                int n = 2097152;

                Action method = () =>
                {
                    _ = Com.Statistics.HypergeometricDistributionProbability(value, N, M, n);
                };

                ExecuteTest(method, "Com.Statistics.HypergeometricDistributionProbability(int, int, int, int)", "value at 1048576, N at 8388608, M at 4194304, n at 2097152");
            }

            {
                int value = 1048576;
                int n = 2097152;
                double p = 0.5;

                Action method = () =>
                {
                    _ = Com.Statistics.BinomialDistributionProbability(value, n, p);
                };

                ExecuteTest(method, "Com.Statistics.BinomialDistributionProbability(int, int, double)", "value at 1048576, N at 2097152, p at 0.5");
            }

            {
                int value = 1048576;
                double lambda = 1048576;

                Action method = () =>
                {
                    _ = Com.Statistics.PoissonDistributionProbability(value, lambda);
                };

                ExecuteTest(method, "Com.Statistics.PoissonDistributionProbability(int, double)", "value at 1048576, lambda at 1048576");
            }

            {
                double value = 0.5;
                double lambda = 0.5;

                Action method = () =>
                {
                    _ = Com.Statistics.ExponentialDistributionProbabilityDensity(value, lambda);
                };

                ExecuteTest(method, "Com.Statistics.ExponentialDistributionProbabilityDensity(double, double)", "value at 0.5, lambda at 0.5");
            }

            {
                double lambda = 0.5;
                double left = 0.5;
                double right = 1;

                Action method = () =>
                {
                    _ = Com.Statistics.ExponentialDistributionProbability(lambda, left, right);
                };

                ExecuteTest(method, "Com.Statistics.ExponentialDistributionProbabilityDensity(double, double, double)", "lambda at 0.5, left at 0.5, right at 1");
            }

            {
                double value = 0.5;

                Action method = () =>
                {
                    _ = Com.Statistics.NormalDistributionProbabilityDensity(value);
                };

                ExecuteTest(method, "Com.Statistics.NormalDistributionProbabilityDensity(double)", "value at 0.5");
            }

            {
                double value = 0.5;
                double ev = 0;
                double sd = 1;

                Action method = () =>
                {
                    _ = Com.Statistics.NormalDistributionProbabilityDensity(value, ev, sd);
                };

                ExecuteTest(method, "Com.Statistics.NormalDistributionProbabilityDensity(double, double, double)", "value at 0.5, ev at 0, sd at 1");
            }

            {
                double value = 0.5;
                int k = 1;

                Action method = () =>
                {
                    _ = Com.Statistics.ChiSquaredDistributionProbabilityDensity(value, k);
                };

                ExecuteTest(method, "Com.Statistics.ChiSquaredDistributionProbabilityDensity(double, int)", "value at 0.5, k at 1");
            }

            // 极值，极差，求和，平均

            {
                sbyte[] values = _GetRandomSbyteArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Max(values);
                };

                ExecuteTest(method, "Com.Statistics.Max(params sbyte[])", "array size at 1024");
            }

            {
                byte[] values = _GetRandomByteArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Max(values);
                };

                ExecuteTest(method, "Com.Statistics.Max(params byte[])", "array size at 1024");
            }

            {
                short[] values = _GetRandomShortArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Max(values);
                };

                ExecuteTest(method, "Com.Statistics.Max(params short[])", "array size at 1024");
            }

            {
                ushort[] values = _GetRandomUshortArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Max(values);
                };

                ExecuteTest(method, "Com.Statistics.Max(params ushort[])", "array size at 1024");
            }

            {
                int[] values = _GetRandomIntArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Max(values);
                };

                ExecuteTest(method, "Com.Statistics.Max(params int[])", "array size at 1024");
            }

            {
                uint[] values = _GetRandomUintArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Max(values);
                };

                ExecuteTest(method, "Com.Statistics.Max(params uint[])", "array size at 1024");
            }

            {
                long[] values = _GetRandomLongArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Max(values);
                };

                ExecuteTest(method, "Com.Statistics.Max(params long[])", "array size at 1024");
            }

            {
                ulong[] values = _GetRandomUlongArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Max(values);
                };

                ExecuteTest(method, "Com.Statistics.Max(params ulong[])", "array size at 1024");
            }

            {
                decimal[] values = _GetRandomDecimalArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Max(values);
                };

                ExecuteTest(method, "Com.Statistics.Max(params decimal[])", "array size at 1024");
            }

            {
                float[] values = _GetRandomFloatArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Max(values);
                };

                ExecuteTest(method, "Com.Statistics.Max(params float[])", "array size at 1024");
            }

            {
                double[] values = _GetRandomDoubleArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Max(values);
                };

                ExecuteTest(method, "Com.Statistics.Max(params double[])", "array size at 1024");
            }

            ExecuteTest(null, "Com.Statistics.Max(params IComparable[])");

            {
                sbyte[] values = _GetRandomSbyteArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Min(values);
                };

                ExecuteTest(method, "Com.Statistics.Min(params sbyte[])", "array size at 1024");
            }

            {
                byte[] values = _GetRandomByteArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Min(values);
                };

                ExecuteTest(method, "Com.Statistics.Min(params byte[])", "array size at 1024");
            }

            {
                short[] values = _GetRandomShortArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Min(values);
                };

                ExecuteTest(method, "Com.Statistics.Min(params short[])", "array size at 1024");
            }

            {
                ushort[] values = _GetRandomUshortArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Min(values);
                };

                ExecuteTest(method, "Com.Statistics.Min(params ushort[])", "array size at 1024");
            }

            {
                int[] values = _GetRandomIntArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Min(values);
                };

                ExecuteTest(method, "Com.Statistics.Min(params int[])", "array size at 1024");
            }

            {
                uint[] values = _GetRandomUintArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Min(values);
                };

                ExecuteTest(method, "Com.Statistics.Min(params uint[])", "array size at 1024");
            }

            {
                long[] values = _GetRandomLongArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Min(values);
                };

                ExecuteTest(method, "Com.Statistics.Min(params long[])", "array size at 1024");
            }

            {
                ulong[] values = _GetRandomUlongArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Min(values);
                };

                ExecuteTest(method, "Com.Statistics.Min(params ulong[])", "array size at 1024");
            }

            {
                decimal[] values = _GetRandomDecimalArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Min(values);
                };

                ExecuteTest(method, "Com.Statistics.Min(params decimal[])", "array size at 1024");
            }

            {
                float[] values = _GetRandomFloatArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Min(values);
                };

                ExecuteTest(method, "Com.Statistics.Min(params float[])", "array size at 1024");
            }

            {
                double[] values = _GetRandomDoubleArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Min(values);
                };

                ExecuteTest(method, "Com.Statistics.Min(params double[])", "array size at 1024");
            }

            ExecuteTest(null, "Com.Statistics.Min(params IComparable[])");

            {
                sbyte[] values = _GetRandomSbyteArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMax(values);
                };

                ExecuteTest(method, "Com.Statistics.MinMax(params sbyte[])", "array size at 1024");
            }

            {
                byte[] values = _GetRandomByteArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMax(values);
                };

                ExecuteTest(method, "Com.Statistics.MinMax(params byte[])", "array size at 1024");
            }

            {
                short[] values = _GetRandomShortArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMax(values);
                };

                ExecuteTest(method, "Com.Statistics.MinMax(params short[])", "array size at 1024");
            }

            {
                ushort[] values = _GetRandomUshortArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMax(values);
                };

                ExecuteTest(method, "Com.Statistics.MinMax(params ushort[])", "array size at 1024");
            }

            {
                int[] values = _GetRandomIntArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMax(values);
                };

                ExecuteTest(method, "Com.Statistics.MinMax(params int[])", "array size at 1024");
            }

            {
                uint[] values = _GetRandomUintArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMax(values);
                };

                ExecuteTest(method, "Com.Statistics.MinMax(params uint[])", "array size at 1024");
            }

            {
                long[] values = _GetRandomLongArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMax(values);
                };

                ExecuteTest(method, "Com.Statistics.MinMax(params long[])", "array size at 1024");
            }

            {
                ulong[] values = _GetRandomUlongArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMax(values);
                };

                ExecuteTest(method, "Com.Statistics.MinMax(params ulong[])", "array size at 1024");
            }

            {
                decimal[] values = _GetRandomDecimalArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMax(values);
                };

                ExecuteTest(method, "Com.Statistics.MinMax(params decimal[])", "array size at 1024");
            }

            {
                float[] values = _GetRandomFloatArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMax(values);
                };

                ExecuteTest(method, "Com.Statistics.MinMax(params float[])", "array size at 1024");
            }

            {
                double[] values = _GetRandomDoubleArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMax(values);
                };

                ExecuteTest(method, "Com.Statistics.MinMax(params double[])", "array size at 1024");
            }

            ExecuteTest(null, "Com.Statistics.MinMax(params IComparable[])");

            {
                sbyte[] values = _GetRandomSbyteArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Range(values);
                };

                ExecuteTest(method, "Com.Statistics.Range(params sbyte[])", "array size at 1024");
            }

            {
                byte[] values = _GetRandomByteArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Range(values);
                };

                ExecuteTest(method, "Com.Statistics.Range(params byte[])", "array size at 1024");
            }

            {
                short[] values = _GetRandomShortArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Range(values);
                };

                ExecuteTest(method, "Com.Statistics.Range(params short[])", "array size at 1024");
            }

            {
                ushort[] values = _GetRandomUshortArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Range(values);
                };

                ExecuteTest(method, "Com.Statistics.Range(params ushort[])", "array size at 1024");
            }

            {
                int[] values = _GetRandomIntArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Range(values);
                };

                ExecuteTest(method, "Com.Statistics.Range(params int[])", "array size at 1024");
            }

            {
                uint[] values = _GetRandomUintArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Range(values);
                };

                ExecuteTest(method, "Com.Statistics.Range(params uint[])", "array size at 1024");
            }

            {
                long[] values = _GetRandomLongArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Range(values);
                };

                ExecuteTest(method, "Com.Statistics.Range(params long[])", "array size at 1024");
            }

            {
                ulong[] values = _GetRandomUlongArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Range(values);
                };

                ExecuteTest(method, "Com.Statistics.Range(params ulong[])", "array size at 1024");
            }

            {
                decimal[] values = _GetRandomDecimalArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Range(values);
                };

                ExecuteTest(method, "Com.Statistics.Range(params decimal[])", "array size at 1024");
            }

            {
                float[] values = _GetRandomFloatArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Range(values);
                };

                ExecuteTest(method, "Com.Statistics.Range(params float[])", "array size at 1024");
            }

            {
                double[] values = _GetRandomDoubleArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Range(values);
                };

                ExecuteTest(method, "Com.Statistics.Range(params double[])", "array size at 1024");
            }

            ExecuteTest(null, "Com.Statistics.Range(params IComparable[])");

            {
                sbyte[] values = _GetRandomSbyteArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Sum(values);
                };

                ExecuteTest(method, "Com.Statistics.Sum(params sbyte[])", "array size at 1024");
            }

            {
                byte[] values = _GetRandomByteArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Sum(values);
                };

                ExecuteTest(method, "Com.Statistics.Sum(params byte[])", "array size at 1024");
            }

            {
                short[] values = _GetRandomShortArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Sum(values);
                };

                ExecuteTest(method, "Com.Statistics.Sum(params short[])", "array size at 1024");
            }

            {
                ushort[] values = _GetRandomUshortArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Sum(values);
                };

                ExecuteTest(method, "Com.Statistics.Sum(params ushort[])", "array size at 1024");
            }

            {
                int[] values = new int[1024];

                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = Com.Statistics.RandomInteger(1048576);
                }

                Action method = () =>
                {
                    _ = Com.Statistics.Sum(values);
                };

                ExecuteTest(method, "Com.Statistics.Sum(params int[])", "array size at 1024");
            }

            {
                uint[] values = _GetRandomUintArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Sum(values);
                };

                ExecuteTest(method, "Com.Statistics.Sum(params uint[])", "array size at 1024");
            }

            {
                long[] values = _GetRandomLongArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Sum(values);
                };

                ExecuteTest(method, "Com.Statistics.Sum(params long[])", "array size at 1024");
            }

            {
                ulong[] values = _GetRandomUlongArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Sum(values);
                };

                ExecuteTest(method, "Com.Statistics.Sum(params ulong[])", "array size at 1024");
            }

            {
                decimal[] values = _GetRandomDecimalArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Sum(values);
                };

                ExecuteTest(method, "Com.Statistics.Sum(params decimal[])", "array size at 1024");
            }

            {
                float[] values = _GetRandomFloatArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Sum(values);
                };

                ExecuteTest(method, "Com.Statistics.Sum(params float[])", "array size at 1024");
            }

            {
                double[] values = _GetRandomDoubleArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Sum(values);
                };

                ExecuteTest(method, "Com.Statistics.Sum(params double[])", "array size at 1024");
            }

            ExecuteTest(null, "Com.Statistics.Sum(params IComparable[])");

            {
                sbyte[] values = _GetRandomSbyteArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Average(values);
                };

                ExecuteTest(method, "Com.Statistics.Average(params sbyte[])", "array size at 1024");
            }

            {
                byte[] values = _GetRandomByteArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Average(values);
                };

                ExecuteTest(method, "Com.Statistics.Average(params byte[])", "array size at 1024");
            }

            {
                short[] values = _GetRandomShortArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Average(values);
                };

                ExecuteTest(method, "Com.Statistics.Average(params short[])", "array size at 1024");
            }

            {
                ushort[] values = _GetRandomUshortArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Average(values);
                };

                ExecuteTest(method, "Com.Statistics.Average(params ushort[])", "array size at 1024");
            }

            {
                int[] values = _GetRandomIntArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Average(values);
                };

                ExecuteTest(method, "Com.Statistics.Average(params int[])", "array size at 1024");
            }

            {
                uint[] values = _GetRandomUintArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Average(values);
                };

                ExecuteTest(method, "Com.Statistics.Average(params uint[])", "array size at 1024");
            }

            {
                long[] values = _GetRandomLongArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Average(values);
                };

                ExecuteTest(method, "Com.Statistics.Average(params long[])", "array size at 1024");
            }

            {
                ulong[] values = _GetRandomUlongArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Average(values);
                };

                ExecuteTest(method, "Com.Statistics.Average(params ulong[])", "array size at 1024");
            }

            {
                decimal[] values = _GetRandomDecimalArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Average(values);
                };

                ExecuteTest(method, "Com.Statistics.Average(params decimal[])", "array size at 1024");
            }

            {
                float[] values = _GetRandomFloatArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Average(values);
                };

                ExecuteTest(method, "Com.Statistics.Average(params float[])", "array size at 1024");
            }

            {
                double[] values = _GetRandomDoubleArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Average(values);
                };

                ExecuteTest(method, "Com.Statistics.Average(params double[])", "array size at 1024");
            }

            ExecuteTest(null, "Com.Statistics.Average(params IComparable[])");

            {
                sbyte[] values = _GetRandomSbyteArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMaxAverage(values);
                };

                ExecuteTest(method, "Com.Statistics.MinMaxAverage(params sbyte[])", "array size at 1024");
            }

            {
                byte[] values = _GetRandomByteArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMaxAverage(values);
                };

                ExecuteTest(method, "Com.Statistics.MinMaxAverage(params byte[])", "array size at 1024");
            }

            {
                short[] values = _GetRandomShortArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMaxAverage(values);
                };

                ExecuteTest(method, "Com.Statistics.MinMaxAverage(params short[])", "array size at 1024");
            }

            {
                ushort[] values = _GetRandomUshortArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMaxAverage(values);
                };

                ExecuteTest(method, "Com.Statistics.MinMaxAverage(params ushort[])", "array size at 1024");
            }

            {
                int[] values = _GetRandomIntArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMaxAverage(values);
                };

                ExecuteTest(method, "Com.Statistics.MinMaxAverage(params int[])", "array size at 1024");
            }

            {
                uint[] values = _GetRandomUintArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMaxAverage(values);
                };

                ExecuteTest(method, "Com.Statistics.MinMaxAverage(params uint[])", "array size at 1024");
            }

            {
                long[] values = _GetRandomLongArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMaxAverage(values);
                };

                ExecuteTest(method, "Com.Statistics.MinMaxAverage(params long[])", "array size at 1024");
            }

            {
                ulong[] values = _GetRandomUlongArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMaxAverage(values);
                };

                ExecuteTest(method, "Com.Statistics.MinMaxAverage(params ulong[])", "array size at 1024");
            }

            {
                decimal[] values = _GetRandomDecimalArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMaxAverage(values);
                };

                ExecuteTest(method, "Com.Statistics.MinMaxAverage(params decimal[])", "array size at 1024");
            }

            {
                float[] values = _GetRandomFloatArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMaxAverage(values);
                };

                ExecuteTest(method, "Com.Statistics.MinMaxAverage(params float[])", "array size at 1024");
            }

            {
                double[] values = _GetRandomDoubleArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMaxAverage(values);
                };

                ExecuteTest(method, "Com.Statistics.MinMaxAverage(params double[])", "array size at 1024");
            }

            ExecuteTest(null, "Com.Statistics.MinMaxAverage(params IComparable[])");

            // 方差与标准差

            {
                double[] values = _GetRandomDoubleArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Deviation(values);
                };

                ExecuteTest(method, "Com.Statistics.Deviation(params double[])", "array size at 1024");
            }

            {
                double[] values = _GetRandomDoubleArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.SampleDeviation(values);
                };

                ExecuteTest(method, "Com.Statistics.SampleDeviation(params double[])", "array size at 1024");
            }

            {
                double[] values = _GetRandomDoubleArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.StandardDeviation(values);
                };

                ExecuteTest(method, "Com.Statistics.StandardDeviation(params double[])", "array size at 1024");
            }

            {
                double[] values = _GetRandomDoubleArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.SampleStandardDeviation(values);
                };

                ExecuteTest(method, "Com.Statistics.SampleStandardDeviation(params double[])", "array size at 1024");
            }
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
            // 科学计数法

            {
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);
                int significance = 0;
                bool useNaturalExpression = true;
                bool useMagnitudeOrderCode = true;
                string unit = "U";

                Action method = () =>
                {
                    _ = Com.Text.GetScientificNotationString(value, significance, useNaturalExpression, useMagnitudeOrderCode, unit);
                };

                ExecuteTest(method, "Com.Text.GetScientificNotationString(double, int, bool, bool, string)");
            }

            {
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);
                int significance = 0;
                bool useNaturalExpression = true;
                bool useMagnitudeOrderCode = true;

                Action method = () =>
                {
                    _ = Com.Text.GetScientificNotationString(value, significance, useNaturalExpression, useMagnitudeOrderCode);
                };

                ExecuteTest(method, "Com.Text.GetScientificNotationString(double, int, bool, bool)");
            }

            {
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);
                int significance = 0;
                bool useNaturalExpression = true;
                string unit = "U";

                Action method = () =>
                {
                    _ = Com.Text.GetScientificNotationString(value, significance, useNaturalExpression, unit);
                };

                ExecuteTest(method, "Com.Text.GetScientificNotationString(double, int, bool, string)");
            }

            {
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);
                int significance = 0;
                bool useNaturalExpression = true;

                Action method = () =>
                {
                    _ = Com.Text.GetScientificNotationString(value, significance, useNaturalExpression);
                };

                ExecuteTest(method, "Com.Text.GetScientificNotationString(double, int, bool)");
            }

            {
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);
                int significance = 0;
                string unit = "U";

                Action method = () =>
                {
                    _ = Com.Text.GetScientificNotationString(value, significance, unit);
                };

                ExecuteTest(method, "Com.Text.GetScientificNotationString(double, int, string)");
            }

            {
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);
                int significance = 0;

                Action method = () =>
                {
                    _ = Com.Text.GetScientificNotationString(value, significance);
                };

                ExecuteTest(method, "Com.Text.GetScientificNotationString(double, int)");
            }

            {
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);
                bool useNaturalExpression = true;
                bool useMagnitudeOrderCode = true;
                string unit = "U";

                Action method = () =>
                {
                    _ = Com.Text.GetScientificNotationString(value, useNaturalExpression, useMagnitudeOrderCode, unit);
                };

                ExecuteTest(method, "Com.Text.GetScientificNotationString(double, bool, bool, string)");
            }

            {
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);
                bool useNaturalExpression = true;
                bool useMagnitudeOrderCode = true;

                Action method = () =>
                {
                    _ = Com.Text.GetScientificNotationString(value, useNaturalExpression, useMagnitudeOrderCode);
                };

                ExecuteTest(method, "Com.Text.GetScientificNotationString(double, bool, bool)");
            }

            {
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);
                bool useNaturalExpression = true;
                string unit = "U";

                Action method = () =>
                {
                    _ = Com.Text.GetScientificNotationString(value, useNaturalExpression, unit);
                };

                ExecuteTest(method, "Com.Text.GetScientificNotationString(double, bool, string)");
            }

            {
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);
                bool useNaturalExpression = true;

                Action method = () =>
                {
                    _ = Com.Text.GetScientificNotationString(value, useNaturalExpression);
                };

                ExecuteTest(method, "Com.Text.GetScientificNotationString(double, bool)");
            }

            {
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);
                string unit = "U";

                Action method = () =>
                {
                    _ = Com.Text.GetScientificNotationString(value, unit);
                };

                ExecuteTest(method, "Com.Text.GetScientificNotationString(double, string)");
            }

            {
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.Text.GetScientificNotationString(value);
                };

                ExecuteTest(method, "Com.Text.GetScientificNotationString(double)");
            }

            // 字符串处理

            {
                string sourceString = "!\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
                string startString = sourceString.Substring(5, 5);
                string endString = sourceString.Substring(sourceString.Length - 10, 5);
                bool includeStartString = true;
                bool includeEndString = true;

                Action method = () =>
                {
                    _ = Com.Text.GetIntervalString(sourceString, startString, endString, includeStartString, includeEndString);
                };

                ExecuteTest(method, "Com.Text.GetIntervalString(string, string, string, bool, bool)");
            }

            {
                string str = "!\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
                Font font = new Font("微软雅黑", 42F, FontStyle.Regular, GraphicsUnit.Point, 134);
                int width = 1024;

                Action method = () =>
                {
                    _ = Com.Text.StringIntercept(str, font, width);
                };

                ExecuteTest(method, "Com.Text.StringIntercept(string, System.Drawing.Font, int)");
            }

            {
                string text = "!\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
                Font font = new Font("微软雅黑", 42F, FontStyle.Regular, GraphicsUnit.Point, 134);
                SizeF size = new SizeF(1024, 1024);

                Action method = () =>
                {
                    _ = Com.Text.GetSuitableFont(text, font, size);
                };

                ExecuteTest(method, "Com.Text.GetSuitableFont(string, System.Drawing.Font, System.Drawing.SizeF)");
            }

            // 转换为字符串

            {
                TimeSpan timeSpan = TimeSpan.FromDays(1) + DateTime.Now.TimeOfDay;

                Action method = () =>
                {
                    _ = Com.Text.GetLongTimeStringFromTimeSpan(timeSpan);
                };

                ExecuteTest(method, "Com.Text.GetLongTimeStringFromTimeSpan(System.TimeSpan)");
            }

            {
                TimeSpan timeSpan = TimeSpan.FromDays(1) + DateTime.Now.TimeOfDay;

                Action method = () =>
                {
                    _ = Com.Text.GetTimeStringFromTimeSpan(timeSpan);
                };

                ExecuteTest(method, "Com.Text.GetTimeStringFromTimeSpan(System.TimeSpan)");
            }

            {
                double second = (TimeSpan.FromDays(1) + DateTime.Now.TimeOfDay).TotalSeconds;
                int significance = 6;

                Action method = () =>
                {
                    _ = Com.Text.GetStandardizationTimespanOfSecond(second, significance);
                };

                ExecuteTest(method, "Com.Text.GetStandardizationTimespanOfSecond(double, int)");
            }

            {
                double second = (TimeSpan.FromDays(1) + DateTime.Now.TimeOfDay).TotalSeconds;

                Action method = () =>
                {
                    _ = Com.Text.GetLargeTimespanStringOfSecond(second);
                };

                ExecuteTest(method, "Com.Text.GetLargeTimespanStringOfSecond(double)");
            }

            {
                double meter = Com.Statistics.RandomDouble(1E12);
                int significance = 6;

                Action method = () =>
                {
                    _ = Com.Text.GetStandardizationDistanceOfMeter(meter, significance);
                };

                ExecuteTest(method, "Com.Text.GetStandardizationDistanceOfMeter(double, int)");
            }

            {
                double meter = Com.Statistics.RandomDouble(1E12);

                Action method = () =>
                {
                    _ = Com.Text.GetLargeDistanceStringOfMeter(meter);
                };

                ExecuteTest(method, "Com.Text.GetLargeDistanceStringOfMeter(double)");
            }

            {
                double degree = Com.Statistics.RandomDouble(360);
                int decimalDigits = 3;
                bool cutdownIdleZeros = true;

                Action method = () =>
                {
                    _ = Com.Text.GetAngleStringOfDegree(degree, decimalDigits, cutdownIdleZeros);
                };

                ExecuteTest(method, "Com.Text.GetAngleStringOfDegree(double, int, bool)");
            }

            {
                long b = (long)Com.Statistics.RandomDouble(1E18);

                Action method = () =>
                {
                    _ = Com.Text.GetSize64StringFromByte(b);
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
                return Com.Vector.Empty;
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
                return Com.Vector.Empty;
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
                return Com.Matrix.Empty;
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
                    _ = new Com.Vector(type, values);
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
                    _ = new Com.Vector(values);
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
                    _ = vector[index];
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

            // Dimension

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.Dimension;
                };

                ExecuteTest(method, "Com.Vector.Dimension.get()", "dimension at 32");
            }

            // Is

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.IsEmpty;
                };

                ExecuteTest(method, "Com.Vector.IsEmpty.get()", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.IsZero;
                };

                ExecuteTest(method, "Com.Vector.IsZero.get()", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.IsColumnVector;
                };

                ExecuteTest(method, "Com.Vector.IsColumnVector.get()", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.IsRowVector;
                };

                ExecuteTest(method, "Com.Vector.IsRowVector.get()", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.IsReadOnly;
                };

                ExecuteTest(method, "Com.Vector.IsReadOnly.get()", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.IsFixedSize;
                };

                ExecuteTest(method, "Com.Vector.IsFixedSize.get()", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.IsNaN;
                };

                ExecuteTest(method, "Com.Vector.IsNaN.get()", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.IsInfinity;
                };

                ExecuteTest(method, "Com.Vector.IsInfinity.get()", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.IsNaNOrInfinity;
                };

                ExecuteTest(method, "Com.Vector.IsNaNOrInfinity.get()", "dimension at 32");
            }

            // 模

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.Module;
                };

                ExecuteTest(method, "Com.Vector.Module.get()", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.ModuleSquared;
                };

                ExecuteTest(method, "Com.Vector.ModuleSquared.get()", "dimension at 32");
            }

            // 向量

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.Opposite;
                };

                ExecuteTest(method, "Com.Vector.Opposite.get()", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.Normalize;
                };

                ExecuteTest(method, "Com.Vector.Normalize.get()", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.Transport;
                };

                ExecuteTest(method, "Com.Vector.Transport.get()", "dimension at 32");
            }
        }

        protected override void StaticProperty()
        {
            // Empty

            {
                Action method = () =>
                {
                    _ = Com.Vector.Empty;
                };

                ExecuteTest(method, "Com.Vector.Empty.get()");
            }
        }

        protected override void Method()
        {
            // object

            {
                Com.Vector vector = _GetRandomVector(32);
                object obj = vector.Copy();

                Action method = () =>
                {
                    _ = vector.Equals(obj);
                };

                ExecuteTest(method, "Com.Vector.Equals(object)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.GetHashCode();
                };

                ExecuteTest(method, "Com.Vector.GetHashCode()", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.ToString();
                };

                ExecuteTest(method, "Com.Vector.ToString()", "dimension at 32");
            }

            // Equals

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = left.Copy();

                Action method = () =>
                {
                    _ = left.Equals(right);
                };

                ExecuteTest(method, "Com.Vector.Equals(Com.Vector)", "dimension at 32");
            }

            // CompareTo

            {
                Com.Vector vector = _GetRandomVector(32);
                object obj = vector;

                Action method = () =>
                {
                    _ = vector.CompareTo(obj);
                };

                ExecuteTest(method, "Com.Vector.CompareTo(object)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = left;

                Action method = () =>
                {
                    _ = left.CompareTo(right);
                };

                ExecuteTest(method, "Com.Vector.CompareTo(Com.Vector)", "dimension at 32");
            }

            // Copy

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.Copy();
                };

                ExecuteTest(method, "Com.Vector.Copy()", "dimension at 32");
            }

            // 检索

            {
                Com.Vector vector = _GetRandomVector(32);
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = vector.IndexOf(item);
                };

                ExecuteTest(method, "Com.Vector.IndexOf(double)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 0;

                Action method = () =>
                {
                    _ = vector.IndexOf(item, startIndex);
                };

                ExecuteTest(method, "Com.Vector.IndexOf(double, int)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 0;
                int count = 32;

                Action method = () =>
                {
                    _ = vector.IndexOf(item, startIndex, count);
                };

                ExecuteTest(method, "Com.Vector.IndexOf(double, int, int)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = vector.LastIndexOf(item);
                };

                ExecuteTest(method, "Com.Vector.LastIndexOf(double)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 31;

                Action method = () =>
                {
                    _ = vector.LastIndexOf(item, startIndex);
                };

                ExecuteTest(method, "Com.Vector.LastIndexOf(double, int)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 31;
                int count = 32;

                Action method = () =>
                {
                    _ = vector.LastIndexOf(item, startIndex, count);
                };

                ExecuteTest(method, "Com.Vector.LastIndexOf(double, int, int)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = vector.Contains(item);
                };

                ExecuteTest(method, "Com.Vector.Contains(double)", "dimension at 32");
            }

            // ToArray，ToList，ToMatrix

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.ToArray();
                };

                ExecuteTest(method, "Com.Vector.ToArray()", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.ToList();
                };

                ExecuteTest(method, "Com.Vector.ToList()", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.ToMatrix();
                };

                ExecuteTest(method, "Com.Vector.ToMatrix()", "dimension at 32");
            }

            // 坐标系转换

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.ToSpherical();
                };

                ExecuteTest(method, "Com.Vector.ToSpherical()", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.ToCartesian();
                };

                ExecuteTest(method, "Com.Vector.ToCartesian()", "dimension at 32");
            }

            // 距离与夹角

            {
                Com.Vector vector = _GetRandomVector(32);
                Com.Vector vector_d = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.DistanceFrom(vector_d);
                };

                ExecuteTest(method, "Com.Vector.DistanceFrom(Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);
                Com.Vector vector_a = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.AngleFrom(vector_a);
                };

                ExecuteTest(method, "Com.Vector.AngleFrom(Com.Vector)", "dimension at 32");
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
                    _ = vector.OffsetCopy(d);
                };

                ExecuteTest(method, "Com.Vector.OffsetCopy(double)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);
                Com.Vector vector_d = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.OffsetCopy(vector_d);
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
                    _ = vector.ScaleCopy(s);
                };

                ExecuteTest(method, "Com.Vector.ScaleCopy(double)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);
                Com.Vector vector_s = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.ScaleCopy(vector_s);
                };

                ExecuteTest(method, "Com.Vector.ScaleCopy(Com.Vector)", "dimension at 32");
            }

            // Reflect

            {
                Com.Vector vector = _GetRandomVector(32);
                int index = Com.Statistics.RandomInteger(32);

                Action method = () =>
                {
                    vector.Reflect(index);
                };

                ExecuteTest(method, "Com.Vector.Reflect(int)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);
                int index = Com.Statistics.RandomInteger(32);

                Action method = () =>
                {
                    _ = vector.ReflectCopy(index);
                };

                ExecuteTest(method, "Com.Vector.ReflectCopy(int)", "dimension at 32");
            }

            // Shear

            {
                Com.Vector vector = _GetRandomVector(32);
                int index1 = Com.Statistics.RandomInteger(16);
                int index2 = Com.Statistics.RandomInteger(16, 32);
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    vector.Shear(index1, index2, angle);
                };

                ExecuteTest(method, "Com.Vector.Shear(int, int, double)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);
                int index1 = Com.Statistics.RandomInteger(16);
                int index2 = Com.Statistics.RandomInteger(16, 32);
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = vector.ShearCopy(index1, index2, angle);
                };

                ExecuteTest(method, "Com.Vector.ShearCopy(int, int, double)", "dimension at 32");
            }

            // Rotate

            {
                Com.Vector vector = _GetRandomVector(32);
                int index1 = Com.Statistics.RandomInteger(16);
                int index2 = Com.Statistics.RandomInteger(16, 32);
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    vector.Rotate(index1, index2, angle);
                };

                ExecuteTest(method, "Com.Vector.Rotate(int, int, double)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);
                int index1 = Com.Statistics.RandomInteger(16);
                int index2 = Com.Statistics.RandomInteger(16, 32);
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = vector.RotateCopy(index1, index2, angle);
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
                    _ = vector.AffineTransformCopy(matrixLeft);
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
                    _ = vector.AffineTransformCopy(matrixLeftList);
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
                    _ = vector.InverseAffineTransformCopy(matrixLeft);
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
                    _ = vector.InverseAffineTransformCopy(matrixLeftList);
                };

                ExecuteTest(method, "Com.Vector.InverseAffineTransformCopy(System.Collections.Generic.List<Com.Matrix>)", "dimension at 8, total 8 matrices");
            }

            // AngleFromBase，AngleFromSpace

            {
                Com.Vector vector = _GetRandomVector(32);
                int index = Com.Statistics.RandomInteger(32);

                Action method = () =>
                {
                    _ = vector.AngleFromBase(index);
                };

                ExecuteTest(method, "Com.Vector.AngleFromBase(int)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);
                int index = Com.Statistics.RandomInteger(32);

                Action method = () =>
                {
                    _ = vector.AngleFromSpace(index);
                };

                ExecuteTest(method, "Com.Vector.AngleFromSpace(int)", "dimension at 32");
            }
        }

        protected override void StaticMethod()
        {
            // IsNullOrEmpty

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = Com.Vector.IsNullOrEmpty(vector);
                };

                ExecuteTest(method, "Com.Vector.IsNullOrEmpty(Com.Vector)", "dimension at 32");
            }

            // Equals

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = left.Copy();

                Action method = () =>
                {
                    _ = Com.Vector.Equals(left, right);
                };

                ExecuteTest(method, "Com.Vector.Equals(Com.Vector, Com.Vector)", "dimension at 32");
            }

            // Compare

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = left.Copy();

                Action method = () =>
                {
                    _ = Com.Vector.Compare(left, right);
                };

                ExecuteTest(method, "Com.Vector.Compare(Com.Vector, Com.Vector)", "dimension at 32");
            }

            // Zero，Base

            {
                Com.Vector.Type type = Com.Vector.Type.ColumnVector;
                int dimension = 32;

                Action method = () =>
                {
                    _ = Com.Vector.Zero(type, dimension);
                };

                ExecuteTest(method, "Com.Vector.Zero(Com.Vector.Type, int)", "dimension at 32");
            }

            {
                int dimension = 32;

                Action method = () =>
                {
                    _ = Com.Vector.Zero(dimension);
                };

                ExecuteTest(method, "Com.Vector.Zero(int)", "dimension at 32");
            }

            {
                Com.Vector.Type type = Com.Vector.Type.ColumnVector;
                int dimension = 32;
                int index = Com.Statistics.RandomInteger(32);

                Action method = () =>
                {
                    _ = Com.Vector.Base(type, dimension, index);
                };

                ExecuteTest(method, "Com.Vector.Base(Com.Vector.Type, int, int)", "dimension at 32");
            }

            {
                int dimension = 32;
                int index = Com.Statistics.RandomInteger(32);

                Action method = () =>
                {
                    _ = Com.Vector.Base(dimension, index);
                };

                ExecuteTest(method, "Com.Vector.Base(int, int)", "dimension at 32");
            }

            // Matrix

            {
                Com.Vector.Type type = Com.Vector.Type.ColumnVector;
                int dimension = 32;
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.Vector.OffsetMatrix(type, dimension, d);
                };

                ExecuteTest(method, "Com.Vector.OffsetMatrix(Com.Vector.Type, int, double)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = Com.Vector.OffsetMatrix(vector);
                };

                ExecuteTest(method, "Com.Vector.OffsetMatrix(Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector.Type type = Com.Vector.Type.ColumnVector;
                int dimension = 32;
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.Vector.ScaleMatrix(type, dimension, s);
                };

                ExecuteTest(method, "Com.Vector.ScaleMatrix(Com.Vector.Type, int, double)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = Com.Vector.ScaleMatrix(vector);
                };

                ExecuteTest(method, "Com.Vector.ScaleMatrix(Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector.Type type = Com.Vector.Type.ColumnVector;
                int dimension = 32;
                int index = Com.Statistics.RandomInteger(16);

                Action method = () =>
                {
                    _ = Com.Vector.ReflectMatrix(type, dimension, index);
                };

                ExecuteTest(method, "Com.Vector.ReflectMatrix(Com.Vector.Type, int, int)", "dimension at 32");
            }

            {
                Com.Vector.Type type = Com.Vector.Type.ColumnVector;
                int dimension = 32;
                int index1 = Com.Statistics.RandomInteger(16);
                int index2 = Com.Statistics.RandomInteger(16, 32);
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.Vector.ShearMatrix(type, dimension, index1, index2, angle);
                };

                ExecuteTest(method, "Com.Vector.ShearMatrix(Com.Vector.Type, int, int, int, double)", "dimension at 32");
            }

            {
                Com.Vector.Type type = Com.Vector.Type.ColumnVector;
                int dimension = 32;
                int index1 = Com.Statistics.RandomInteger(16);
                int index2 = Com.Statistics.RandomInteger(16, 32);
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.Vector.RotateMatrix(type, dimension, index1, index2, angle);
                };

                ExecuteTest(method, "Com.Vector.RotateMatrix(Com.Vector.Type, int, int, int, double)", "dimension at 32");
            }

            // 距离与夹角

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = Com.Vector.DistanceBetween(left, right);
                };

                ExecuteTest(method, "Com.Vector.DistanceBetween(Com.Vector, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = Com.Vector.AngleBetween(left, right);
                };

                ExecuteTest(method, "Com.Vector.AngleBetween(Com.Vector, Com.Vector)", "dimension at 32");
            }

            // 向量乘积

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = Com.Vector.DotProduct(left, right);
                };

                ExecuteTest(method, "Com.Vector.DotProduct(Com.Vector, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = Com.Vector.CrossProduct(left, right);
                };

                ExecuteTest(method, "Com.Vector.CrossProduct(Com.Vector, Com.Vector)", "dimension at 32");
            }

            // 初等函数

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = Com.Vector.Abs(vector);
                };

                ExecuteTest(method, "Com.Vector.Abs(Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = Com.Vector.Sign(vector);
                };

                ExecuteTest(method, "Com.Vector.Sign(Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = Com.Vector.Ceiling(vector);
                };

                ExecuteTest(method, "Com.Vector.Ceiling(Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = Com.Vector.Floor(vector);
                };

                ExecuteTest(method, "Com.Vector.Floor(Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = Com.Vector.Round(vector);
                };

                ExecuteTest(method, "Com.Vector.Round(Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = Com.Vector.Truncate(vector);
                };

                ExecuteTest(method, "Com.Vector.Truncate(Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = Com.Vector.Max(left, right);
                };

                ExecuteTest(method, "Com.Vector.Max(Com.Vector, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = Com.Vector.Min(left, right);
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
                    _ = (left == right);
                };

                ExecuteTest(method, "Com.Vector.operator ==(Com.Vector, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = (left != right);
                };

                ExecuteTest(method, "Com.Vector.operator !=(Com.Vector, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = (left < right);
                };

                ExecuteTest(method, "Com.Vector.operator <(Com.Vector, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = (left > right);
                };

                ExecuteTest(method, "Com.Vector.operator >(Com.Vector, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = (left <= right);
                };

                ExecuteTest(method, "Com.Vector.operator <=(Com.Vector, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = (left >= right);
                };

                ExecuteTest(method, "Com.Vector.operator >=(Com.Vector, Com.Vector)", "dimension at 32");
            }

            // 运算

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = +vector;
                };

                ExecuteTest(method, "Com.Vector.operator +(Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = -vector;
                };

                ExecuteTest(method, "Com.Vector.operator -(Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector pt = _GetRandomVector(32);
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt + n;
                };

                ExecuteTest(method, "Com.Vector.operator +(Com.Vector, double)", "dimension at 32");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.Vector pt = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = n + pt;
                };

                ExecuteTest(method, "Com.Vector.operator +(double, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = left + right;
                };

                ExecuteTest(method, "Com.Vector.operator +(Com.Vector, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector pt = _GetRandomVector(32);
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt - n;
                };

                ExecuteTest(method, "Com.Vector.operator -(Com.Vector, double)", "dimension at 32");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.Vector pt = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = n - pt;
                };

                ExecuteTest(method, "Com.Vector.operator -(double, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = left - right;
                };

                ExecuteTest(method, "Com.Vector.operator -(Com.Vector, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector pt = _GetRandomVector(32);
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt * n;
                };

                ExecuteTest(method, "Com.Vector.operator *(Com.Vector, double)", "dimension at 32");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.Vector pt = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = n * pt;
                };

                ExecuteTest(method, "Com.Vector.operator *(double, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = left * right;
                };

                ExecuteTest(method, "Com.Vector.operator *(Com.Vector, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector pt = _GetRandomVector(32);
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt / n;
                };

                ExecuteTest(method, "Com.Vector.operator /(Com.Vector, double)", "dimension at 32");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.Vector pt = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = n / pt;
                };

                ExecuteTest(method, "Com.Vector.operator /(double, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = left / right;
                };

                ExecuteTest(method, "Com.Vector.operator /(Com.Vector, Com.Vector)", "dimension at 32");
            }
        }
    }
}