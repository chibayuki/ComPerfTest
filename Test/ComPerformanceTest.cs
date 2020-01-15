/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
Copyright © 2019 chibayuki@foxmail.com

Com性能测试 (ComPerformanceTest)
Version 19.12.5.0000

This file is part of "Com性能测试" (ComPerformanceTest)

"Com性能测试" (ComPerformanceTest) is released under the GPLv3 license
* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

#define ComVerNext
#define ComVer1910
#define ComVer1905
#define ComVer1809

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.IO;

namespace Test
{
    static class ComInfo // Com 信息
    {
        public const int TotalMemberCount = 1828; // 成员总数量

#if ComVerNext
        public const string ComVersionString = "<master>"; // Com 版本字符串
#elif ComVer1910
        public const string ComVersionString = "19.10.14.2100"; // Com 版本字符串
#elif ComVer1905
        public const string ComVersionString = "19.5.11.1720"; // Com 版本字符串
#elif ComVer1809
        public const string ComVersionString = "18.9.28.2200"; // Com 版本字符串
#else
        public const string ComVersionString = "<unknown>"; // Com 版本字符串
#endif
    }

    static class TestResult // 测试结果
    {
        private static List<string> _ResultList = new List<string>(2048);

        //

        public static void Log(string result) // 记录测试结果
        {
            if (string.IsNullOrWhiteSpace(result))
            {
                _ResultList.Add(string.Empty);
            }
            else
            {
                _ResultList.Add(result);
            }
        }

        public static void LogCsv(params string[] result) // 记录测试结果
        {
            if (result == null || result.Length <= 0)
            {
                _ResultList.Add(string.Empty);
            }
            else
            {
                Log(string.Join(",", result));
            }
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
                string fileName = string.Concat("ComPerfReport,version=", ComInfo.ComVersionString, ",id=", dt.GetHashCode(), ".csv");

                filePath = Path.Combine(fileDir, fileName);

                StreamWriter sw = null;

                try
                {
                    sw = new StreamWriter(filePath, false);

                    for (int i = 0; i < _ResultList.Count; i++)
                    {
                        sw.WriteLine(_ResultList[i]);
                    }
                }
                finally
                {
                    if (sw != null)
                    {
                        sw.Close();
                    }
                }
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
        private static int _CompletedMemberCount = 0; // 已测试成员数量

        private static int _FullWidth => Math.Max(10, Math.Min(Console.WindowWidth * 3 / 4, 100)); // 进度条宽度

        //

        public static void Report(int delta) // 报告测试进度
        {
            _CompletedMemberCount += delta;

            double progress = (double)_CompletedMemberCount / ComInfo.TotalMemberCount;

            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("Executing test for Com " + ComInfo.ComVersionString);
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
            Console.Write(string.Concat(Math.Floor(progress * 1000) / 10, "% (", _CompletedMemberCount, " of ", ComInfo.TotalMemberCount, ") completed"));
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
            Console.Write("Executing test for Com " + ComInfo.ComVersionString);
            Console.ForegroundColor = ConsoleColor.Black;
            Console.SetCursorPosition(2, 2);
            Console.BackgroundColor = ConsoleColor.Gray;
            Console.Write(new string(' ', _FullWidth));
            Console.BackgroundColor = ConsoleColor.White;
            Console.SetCursorPosition(2, 3);
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write(string.Concat("0% (0 of ", ComInfo.TotalMemberCount, ") completed"));
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

    abstract class ClassPerfTestBase // 类性能测试类的基类
    {
#if !DEBUG
        private const int _MinMSOfPerMember = 100; // 被测试类每个成员的最短执行时长的毫秒数
        private const int _MinCycOfPerMember = 10; // 被测试类每个成员的最小执行次数
        private const int _MaxCycOfPerMember = 1000000000; // 被测试类每个成员的最大执行次数
#else
        private const int _MinMSOfPerMember = 1; // 被测试类每个成员的最短执行时长的毫秒数
        private const int _MinCycOfPerMember = 1; // 被测试类每个成员的最小执行次数
        private const int _MaxCycOfPerMember = 10000000; // 被测试类每个成员的最大执行次数
#endif

        //

        private static string _LastResult = string.Empty; // 最近的一条测试结果

        //

#if ComVer1905
        private static string _GetScientificNotationString(double value, int significance, bool useNaturalExpression, bool useMagnitudeOrderCode, string unit)
        {
            return Com.Text.GetScientificNotationString(value, significance, useNaturalExpression, useMagnitudeOrderCode, unit);
        }
#else
        private const string _PositiveMagnitudeOrderCode = "kMGTPEZY"; // 千进制正数量级符号。
        private const string _NegativeMagnitudeOrderCode = "mμnpfazy"; // 千进制负数量级符号。

        private static string _GetScientificNotationString(double value, int significance, bool useNaturalExpression, bool useMagnitudeOrderCode, string unit)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return "N/A";
            }
            else
            {
                string part1 = string.Empty, part2 = string.Empty, part3 = string.Empty, part4 = string.Empty;

                if (value == 0 || (value > -1E-308 && value < 1E-308))
                {
                    part2 = "0";
                    part4 = (string.IsNullOrEmpty(unit) ? string.Empty : " " + unit);
                }
                else
                {
                    int sign = Math.Sign(value);

                    part1 = (sign < 0 ? "-" : string.Empty);

                    value = Math.Abs(value);

                    significance = Math.Max(0, Math.Min(significance, 16));

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
                }

                return string.Concat(part1, part2, part3, part4);
            }
        }
#endif

        //

        protected static readonly Action WillNotTest = null; // 不执行测试

        protected enum UnsupportedReason // 不支持原因
        {
            NeedComVerNext,
            NeedComVer1910,
            NeedComVer1905,
            NeedComVer1809
        }

        protected void ExecuteTest(Action method, string namespaceName, string className, string methodName, string comment) // 执行测试
        {
            string[] resultForLog = null;
            string resultForDisplay = string.Empty;

            if (string.IsNullOrWhiteSpace(namespaceName))
            {
                namespaceName = "<unnamed>";
            }

            if (string.IsNullOrWhiteSpace(className))
            {
                className = "<unnamed>";
            }

            if (string.IsNullOrWhiteSpace(methodName))
            {
                methodName = "<unnamed>";
            }

            string memberName = string.Concat(namespaceName.Replace(',', ';'), ',', className.Replace(',', ';'), ',', methodName.Replace(',', ';'));
            string memberNameOriginal = string.Concat(namespaceName, '.', className, '.', methodName);

            //

            TestProgress.ClearExtra();

            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.Write("Now testing: ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(string.Concat('[', memberNameOriginal, ']'));

            if (!string.IsNullOrWhiteSpace(_LastResult))
            {
                Console.SetCursorPosition(0, 6);
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.Write("Last result: ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write(_LastResult);
            }

            //

            if (comment == null)
            {
                comment = string.Empty;
            }

            if (method == null || method == WillNotTest)
            {
                resultForLog = new string[] { memberName, "N/A", "N/A", "N/A", "N/A", "N/A", "N/A", (comment.Length > 0 ? comment.Replace(',', ';') : string.Empty) };
                resultForDisplay = string.Concat('[', memberNameOriginal, "], N/A, N/A");
            }
            else
            {
                const double tryMS = _MinMSOfPerMember * 0.1;
                int tryCycle = 1;
                int cycle = 0;

                double totalMS = 0;
                DateTime dt = DateTime.Now;

                while (true)
                {
                    method();

                    if (++cycle >= tryCycle)
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

                tryCycle = (int)Math.Max(_MinCycOfPerMember, Math.Min(Math.Ceiling(_MinMSOfPerMember / totalMS * cycle), _MaxCycOfPerMember));

                cycle = 0;
                totalMS = 0;

                while (true)
                {
                    dt = DateTime.Now;

                    while (true)
                    {
                        method();

                        if (++cycle >= tryCycle)
                        {
                            break;
                        }
                    }

                    totalMS += (DateTime.Now - dt).TotalMilliseconds;

                    if (totalMS < _MinMSOfPerMember)
                    {
                        long tryCycleNew = 0;

                        if (totalMS <= _MinMSOfPerMember * 0.5)
                        {
                            if (totalMS <= _MinMSOfPerMember * 0.1)
                            {
                                tryCycleNew = (long)tryCycle * 10;
                            }
                            else
                            {
                                tryCycleNew = (long)Math.Ceiling(_MinMSOfPerMember / totalMS * tryCycle);
                            }

                            if (tryCycleNew >= _MaxCycOfPerMember)
                            {
                                break;
                            }
                            else
                            {
                                tryCycle = (int)tryCycleNew;
                                cycle = 0;
                                totalMS = 0;
                            }
                        }
                        else
                        {
                            tryCycleNew = (long)tryCycle * 2;

                            if (tryCycleNew >= _MaxCycOfPerMember)
                            {
                                break;
                            }
                            else
                            {
                                tryCycle = (int)tryCycleNew;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                double msPerCycle = totalMS / cycle;

                string period = _GetScientificNotationString(msPerCycle / 1000, 4, true, true, "s").Replace('μ', 'u');
                string periodOriginal = (msPerCycle * 1000000).ToString("G");
                string frequency = _GetScientificNotationString(1000 / msPerCycle, 4, true, true, "Hz").Replace('μ', 'u');
                string frequencyOriginal = (1000 / msPerCycle).ToString("G");

                resultForLog = new string[] { memberName, period, frequency, cycle.ToString(), totalMS.ToString("G"), periodOriginal, frequencyOriginal, (comment.Length > 0 ? comment.Replace(',', ';') : string.Empty) };
                resultForDisplay = string.Concat('[', memberNameOriginal, "], ", period, ", ", frequency);
            }

            TestResult.LogCsv(resultForLog);

            TestProgress.Report(1);

            _LastResult = resultForDisplay;
        }

        protected void ExecuteTest(Action method, string namespaceName, string className, string methodName) // 执行测试
        {
            ExecuteTest(method, namespaceName, className, methodName, string.Empty);
        }

        protected void ExecuteTest(string namespaceName, string className, string methodName) // 执行测试
        {
            ExecuteTest(WillNotTest, namespaceName, className, methodName, "<untested member>");
        }

        protected void ExecuteTest(string namespaceName, string className, string methodName, UnsupportedReason unsupportedReason) // 执行测试
        {
            switch (unsupportedReason)
            {
                case UnsupportedReason.NeedComVerNext:
                    ExecuteTest(WillNotTest, namespaceName, className, methodName, "<unsupported member: need Com <master>>");
                    break;

                case UnsupportedReason.NeedComVer1910:
                    ExecuteTest(WillNotTest, namespaceName, className, methodName, "<unsupported member: need Com 19.10.14.2100 or later>");
                    break;

                case UnsupportedReason.NeedComVer1905:
                    ExecuteTest(WillNotTest, namespaceName, className, methodName, "<unsupported member: need Com 19.5.11.1720 or later>");
                    break;

                case UnsupportedReason.NeedComVer1809:
                    ExecuteTest(WillNotTest, namespaceName, className, methodName, "<unsupported member: need Com 18.9.28.2200 or later>");
                    break;

                default:
                    ExecuteTest(WillNotTest, namespaceName, className, methodName, "<unsupported member>");
                    break;
            }
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

    sealed class AnimationTest : ClassPerfTestBase
    {
        private const string _NamespaceName = "Com";
        private const string _ClassName = "Animation";

        private void ExecuteTest(Action method, string methodName, string comment)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName, comment);
        }

        private void ExecuteTest(Action method, string methodName)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(string methodName, UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName, unsupportedReason);
        }

        private void ExecuteTest(string methodName)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, string.Empty, unsupportedReason);
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
            ExecuteTest("Show(Com.Animation.Frame, int, int, int, System.Collections.Generic.List<int>)");

            ExecuteTest("Show(Com.Animation.Frame, int, int, int)");

            ExecuteTest("Show(Com.Animation.Frame, int, int, System.Collections.Generic.List<int>)");

            ExecuteTest("Show(Com.Animation.Frame, int, int)");
        }

        protected override void Operator()
        {

        }
    }

    sealed class BitOperationTest : ClassPerfTestBase
    {
        private const string _NamespaceName = "Com";
        private const string _ClassName = "BitOperation";

        private void ExecuteTest(Action method, string methodName, string comment)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName, comment);
        }

        private void ExecuteTest(Action method, string methodName)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(string methodName, UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName, unsupportedReason);
        }

        private void ExecuteTest(string methodName)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, string.Empty, unsupportedReason);
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
            // 8 位

            {
                int bit = Com.Statistics.RandomInteger() % 8;

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBinary8WithSingleBit1(bit);
                };

                ExecuteTest(method, "GetBinary8WithSingleBit1(int)");
            }

            {
                int bit = Com.Statistics.RandomInteger() % 8;

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBinary8WithSingleBit0(bit);
                };

                ExecuteTest(method, "GetBinary8WithSingleBit0(int)");
            }

            {
                byte bin = (byte)(Com.Statistics.RandomInteger() % 255);
                int bit = Com.Statistics.RandomInteger() % 8;

                Action method = () =>
                {
                    Com.BitOperation.AddBitToBinary(ref bin, bit);
                };

                ExecuteTest(method, "AddBitToBinary(ref byte, int)");
            }

            {
                byte bin = (byte)(Com.Statistics.RandomInteger() % 255);
                int bit = Com.Statistics.RandomInteger() % 8;

                Action method = () =>
                {
                    Com.BitOperation.RemoveBitFromBinary(ref bin, bit);
                };

                ExecuteTest(method, "RemoveBitFromBinary(ref byte, int)");
            }

            {
                byte bin = (byte)(Com.Statistics.RandomInteger() % 255);
                int bit = Com.Statistics.RandomInteger() % 8;

                Action method = () =>
                {
                    Com.BitOperation.InverseBitOfBinary(ref bin, bit);
                };

                ExecuteTest(method, "InverseBitOfBinary(ref byte, int)");
            }

            {
                byte bin = (byte)(Com.Statistics.RandomInteger() % 255);
                int bit = Com.Statistics.RandomInteger() % 8;

                Action method = () =>
                {
                    _ = Com.BitOperation.BinaryHasBit(bin, bit);
                };

                ExecuteTest(method, "BinaryHasBit(byte, int)");
            }

            {
                byte bin = (byte)(Com.Statistics.RandomInteger() % 255);

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBit1CountOfBinary(bin);
                };

                ExecuteTest(method, "GetBit1CountOfBinary(byte)");
            }

            {
                byte bin = (byte)(Com.Statistics.RandomInteger() % 255);

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBit0CountOfBinary(bin);
                };

                ExecuteTest(method, "GetBit0CountOfBinary(byte)");
            }

            {
                byte bin = (byte)(Com.Statistics.RandomInteger() % 255);

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBit1IndexOfBinary(bin);
                };

                ExecuteTest(method, "GetBit1IndexOfBinary(byte)");
            }

            {
                byte bin = (byte)(Com.Statistics.RandomInteger() % 255);

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBit0IndexOfBinary(bin);
                };

                ExecuteTest(method, "GetBit0IndexOfBinary(byte)");
            }

            // 16 位

            {
                int bit = Com.Statistics.RandomInteger() % 16;

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBinary16WithSingleBit1(bit);
                };

                ExecuteTest(method, "GetBinary16WithSingleBit1(int)");
            }

            {
                int bit = Com.Statistics.RandomInteger() % 16;

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBinary16WithSingleBit0(bit);
                };

                ExecuteTest(method, "GetBinary16WithSingleBit0(int)");
            }

            {
                ushort bin = (ushort)Com.Statistics.RandomInteger();
                int bit = Com.Statistics.RandomInteger() % 16;

                Action method = () =>
                {
                    Com.BitOperation.AddBitToBinary(ref bin, bit);
                };

                ExecuteTest(method, "AddBitToBinary(ref ushort, int)");
            }

            {
                ushort bin = (ushort)Com.Statistics.RandomInteger();
                int bit = Com.Statistics.RandomInteger() % 16;

                Action method = () =>
                {
                    Com.BitOperation.RemoveBitFromBinary(ref bin, bit);
                };

                ExecuteTest(method, "RemoveBitFromBinary(ref ushort, int)");
            }

            {
                ushort bin = (ushort)Com.Statistics.RandomInteger();
                int bit = Com.Statistics.RandomInteger() % 16;

                Action method = () =>
                {
                    Com.BitOperation.InverseBitOfBinary(ref bin, bit);
                };

                ExecuteTest(method, "InverseBitOfBinary(ref ushort, int)");
            }

            {
                ushort bin = (ushort)Com.Statistics.RandomInteger();
                int bit = Com.Statistics.RandomInteger() % 16;

                Action method = () =>
                {
                    _ = Com.BitOperation.BinaryHasBit(bin, bit);
                };

                ExecuteTest(method, "BinaryHasBit(ushort, int)");
            }

            {
                ushort bin = (ushort)Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBit1CountOfBinary(bin);
                };

                ExecuteTest(method, "GetBit1CountOfBinary(ushort)");
            }

            {
                ushort bin = (ushort)Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBit0CountOfBinary(bin);
                };

                ExecuteTest(method, "GetBit0CountOfBinary(ushort)");
            }

            {
                ushort bin = (ushort)Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBit1IndexOfBinary(bin);
                };

                ExecuteTest(method, "GetBit1IndexOfBinary(ushort)");
            }

            {
                ushort bin = (ushort)Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBit0IndexOfBinary(bin);
                };

                ExecuteTest(method, "GetBit0IndexOfBinary(ushort)");
            }

            // 32 位

            {
                int bit = Com.Statistics.RandomInteger() % 32;

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBinary32WithSingleBit1(bit);
                };

                ExecuteTest(method, "GetBinary32WithSingleBit1(int)");
            }

            {
                int bit = Com.Statistics.RandomInteger() % 32;

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBinary32WithSingleBit0(bit);
                };

                ExecuteTest(method, "GetBinary32WithSingleBit0(int)");
            }

            {
                uint bin = (uint)Com.Statistics.RandomInteger();
                int bit = Com.Statistics.RandomInteger() % 32;

                Action method = () =>
                {
                    Com.BitOperation.AddBitToBinary(ref bin, bit);
                };

                ExecuteTest(method, "AddBitToBinary(ref uint, int)");
            }

            {
                uint bin = (uint)Com.Statistics.RandomInteger();
                int bit = Com.Statistics.RandomInteger() % 32;

                Action method = () =>
                {
                    Com.BitOperation.RemoveBitFromBinary(ref bin, bit);
                };

                ExecuteTest(method, "RemoveBitFromBinary(ref uint, int)");
            }

            {
                uint bin = (uint)Com.Statistics.RandomInteger();
                int bit = Com.Statistics.RandomInteger() % 32;

                Action method = () =>
                {
                    Com.BitOperation.InverseBitOfBinary(ref bin, bit);
                };

                ExecuteTest(method, "InverseBitOfBinary(ref uint, int)");
            }

            {
                uint bin = (uint)Com.Statistics.RandomInteger();
                int bit = Com.Statistics.RandomInteger() % 32;

                Action method = () =>
                {
                    _ = Com.BitOperation.BinaryHasBit(bin, bit);
                };

                ExecuteTest(method, "BinaryHasBit(uint, int)");
            }

            {
                uint bin = (uint)Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBit1CountOfBinary(bin);
                };

                ExecuteTest(method, "GetBit1CountOfBinary(uint)");
            }

            {
                uint bin = (uint)Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBit0CountOfBinary(bin);
                };

                ExecuteTest(method, "GetBit0CountOfBinary(uint)");
            }

            {
                uint bin = (uint)Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBit1IndexOfBinary(bin);
                };

                ExecuteTest(method, "GetBit1IndexOfBinary(uint)");
            }

            {
                uint bin = (uint)Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBit0IndexOfBinary(bin);
                };

                ExecuteTest(method, "GetBit0IndexOfBinary(uint)");
            }

            // 64 位

            {
                int bit = Com.Statistics.RandomInteger() % 64;

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBinary64WithSingleBit1(bit);
                };

                ExecuteTest(method, "GetBinary64WithSingleBit1(int)");
            }

            {
                int bit = Com.Statistics.RandomInteger() % 64;

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBinary64WithSingleBit0(bit);
                };

                ExecuteTest(method, "GetBinary64WithSingleBit0(int)");
            }

            {
                ulong bin = (ulong)Com.Statistics.RandomInteger();
                int bit = Com.Statistics.RandomInteger() % 64;

                Action method = () =>
                {
                    Com.BitOperation.AddBitToBinary(ref bin, bit);
                };

                ExecuteTest(method, "AddBitToBinary(ref ulong, int)");
            }

            {
                ulong bin = (ulong)Com.Statistics.RandomInteger();
                int bit = Com.Statistics.RandomInteger() % 64;

                Action method = () =>
                {
                    Com.BitOperation.RemoveBitFromBinary(ref bin, bit);
                };

                ExecuteTest(method, "RemoveBitFromBinary(ref ulong, int)");
            }

            {
                ulong bin = (ulong)Com.Statistics.RandomInteger();
                int bit = Com.Statistics.RandomInteger() % 64;

                Action method = () =>
                {
                    Com.BitOperation.InverseBitOfBinary(ref bin, bit);
                };

                ExecuteTest(method, "InverseBitOfBinary(ref ulong, int)");
            }

            {
                ulong bin = (ulong)Com.Statistics.RandomInteger();
                int bit = Com.Statistics.RandomInteger() % 64;

                Action method = () =>
                {
                    _ = Com.BitOperation.BinaryHasBit(bin, bit);
                };

                ExecuteTest(method, "BinaryHasBit(ulong, int)");
            }

            {
                ulong bin = (ulong)Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBit1CountOfBinary(bin);
                };

                ExecuteTest(method, "GetBit1CountOfBinary(ulong)");
            }

            {
                ulong bin = (ulong)Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBit0CountOfBinary(bin);
                };

                ExecuteTest(method, "GetBit0CountOfBinary(ulong)");
            }

            {
                ulong bin = (ulong)Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBit1IndexOfBinary(bin);
                };

                ExecuteTest(method, "GetBit1IndexOfBinary(ulong)");
            }

            {
                ulong bin = (ulong)Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = Com.BitOperation.GetBit0IndexOfBinary(bin);
                };

                ExecuteTest(method, "GetBit0IndexOfBinary(ulong)");
            }
        }

        protected override void Operator()
        {

        }
    }

    sealed class BitSetTest : ClassPerfTestBase
    {
        private const string _NamespaceName = "Com";
        private const string _ClassName = "BitSet";

        private void ExecuteTest(Action method, string methodName, string comment)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName, comment);
        }

        private void ExecuteTest(Action method, string methodName)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(string methodName, UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName, unsupportedReason);
        }

        private void ExecuteTest(string methodName)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, string.Empty, unsupportedReason);
        }

        //

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

                ExecuteTest(method, "BitSet(int)", "size at 1024 bits");
            }

            {
                int length = 1024;
                bool bitValue = true;

                Action method = () =>
                {
                    _ = new Com.BitSet(length, bitValue);
                };

                ExecuteTest(method, "BitSet(int, bool)", "size at 1024 bits");
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

                ExecuteTest(method, "BitSet(params bool[])", "size at 1024 bits");
            }

#if ComVer1905
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

                ExecuteTest(method, "BitSet(params byte[])", "size at 1024 bits");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
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

                ExecuteTest(method, "BitSet(params ushort[])", "size at 1024 bits");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
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

                ExecuteTest(method, "BitSet(params uint[])", "size at 1024 bits");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
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

                ExecuteTest(method, "BitSet(params ulong[])", "size at 1024 bits");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif
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

                ExecuteTest(method, "this[int].get()", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = new Com.BitSet(1024);
                int index = Com.Statistics.RandomInteger(1024);
                bool value = true;

                Action method = () =>
                {
                    bitSet[index] = value;
                };

                ExecuteTest(method, "this[int].set(bool)", "size at 1024 bits");
            }

            // Is

            {
                Com.BitSet bitSet = new Com.BitSet(1024);

                Action method = () =>
                {
                    _ = bitSet.IsEmpty;
                };

                ExecuteTest(method, "IsEmpty.get()", "size at 1024 bits");
            }

#if ComVer1905
            {
                Com.BitSet bitSet = new Com.BitSet(1024);

                Action method = () =>
                {
                    _ = bitSet.IsReadOnly;
                };

                ExecuteTest(method, "IsReadOnly.get()", "size at 1024 bits");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.BitSet bitSet = new Com.BitSet(1024);

                Action method = () =>
                {
                    _ = bitSet.IsFixedSize;
                };

                ExecuteTest(method, "IsFixedSize.get()", "size at 1024 bits");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // Size

            {
                Com.BitSet bitSet = new Com.BitSet(1024);

                Action method = () =>
                {
                    _ = bitSet.Size;
                };

                ExecuteTest(method, "Size.get()", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = new Com.BitSet(1024);
                int value = 8;

                Action method = () =>
                {
                    bitSet.Size += value;
                };

                ExecuteTest(method, "Size.set(int)", "size at 1024 bits, increase by 8 bits");
            }

            {
                Com.BitSet bitSet = new Com.BitSet(1024);

                Action method = () =>
                {
                    _ = bitSet.Count;
                };

                ExecuteTest(method, "Count.get()", "size at 1024 bits");
            }

            // Capacity

            {
                Com.BitSet bitSet = new Com.BitSet(1024);

                Action method = () =>
                {
                    _ = bitSet.Capacity;
                };

                ExecuteTest(method, "Capacity.get()", "size at 1024 bits");
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

                ExecuteTest(method, "Empty.get()");
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

                ExecuteTest(method, "Equals(object)", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    _ = bitSet.GetHashCode();
                };

                ExecuteTest(method, "GetHashCode()", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    _ = bitSet.ToString();
                };

                ExecuteTest(method, "ToString()", "size at 1024 bits");
            }

            // Equals

            {
                Com.BitSet left = _GetRandomBitSet(1024);
                Com.BitSet right = left.Copy();

                Action method = () =>
                {
                    _ = left.Equals(right);
                };

                ExecuteTest(method, "Equals(Com.BitSet)", "size at 1024 bits");
            }

            // CompareTo

#if ComVer1905
            {
                Com.BitSet left = _GetRandomBitSet(1024);
                object right = left.Copy();

                Action method = () =>
                {
                    _ = left.CompareTo(right);
                };

                ExecuteTest(method, "CompareTo(object)", "size at 1024 bits");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.BitSet left = _GetRandomBitSet(1024);
                Com.BitSet right = left.Copy();

                Action method = () =>
                {
                    _ = left.CompareTo(right);
                };

                ExecuteTest(method, "CompareTo(Com.BitSet)", "size at 1024 bits");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // Copy

            {
                Com.BitSet bitSet = new Com.BitSet(1024);

                Action method = () =>
                {
                    _ = bitSet.Copy();
                };

                ExecuteTest(method, "Copy()", "size at 1024 bits");
            }

            // 检索

#if ComVer1905
            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);
                bool item1 = true;
                bool item2 = false;

                Action method = () =>
                {
                    _ = bitSet.IndexOf(item1);
                    _ = bitSet.IndexOf(item2);
                };

                ExecuteTest(method, "IndexOf(bool)", "size at 1024 bits, search for both true and false bit");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);
                bool item1 = true;
                bool item2 = false;
                int startIndex = 0;

                Action method = () =>
                {
                    _ = bitSet.IndexOf(item1, startIndex);
                    _ = bitSet.IndexOf(item2, startIndex);
                };

                ExecuteTest(method, "IndexOf(bool, int)", "size at 1024 bits, search for both true and false bit");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);
                bool item1 = true;
                bool item2 = false;
                int startIndex = 0;
                int count = 1024;

                Action method = () =>
                {
                    _ = bitSet.IndexOf(item1, startIndex, count);
                    _ = bitSet.IndexOf(item2, startIndex, count);
                };

                ExecuteTest(method, "IndexOf(bool, int, int)", "size at 1024 bits, search for both true and false bit");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);
                bool item1 = true;
                bool item2 = false;

                Action method = () =>
                {
                    _ = bitSet.LastIndexOf(item1);
                    _ = bitSet.LastIndexOf(item2);
                };

                ExecuteTest(method, "LastIndexOf(bool)", "size at 1024 bits, search for both true and false bit");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);
                bool item1 = true;
                bool item2 = false;
                int startIndex = 1023;

                Action method = () =>
                {
                    _ = bitSet.LastIndexOf(item1, startIndex);
                    _ = bitSet.LastIndexOf(item2, startIndex);
                };

                ExecuteTest(method, "LastIndexOf(bool, int)", "size at 1024 bits, search for both true and false bit");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);
                bool item1 = true;
                bool item2 = false;
                int startIndex = 1023;
                int count = 1024;

                Action method = () =>
                {
                    _ = bitSet.LastIndexOf(item1, startIndex, count);
                    _ = bitSet.LastIndexOf(item2, startIndex, count);
                };

                ExecuteTest(method, "LastIndexOf(bool, int, int)", "size at 1024 bits, search for both true and false bit");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);
                bool item1 = true;
                bool item2 = false;

                Action method = () =>
                {
                    _ = bitSet.Contains(item1);
                    _ = bitSet.Contains(item2);
                };

                ExecuteTest(method, "Contains(bool)", "size at 1024 bits, search for both true and false bit");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // ToArray，ToList

#if ComVer1905
            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    _ = bitSet.ToArray();
                };

                ExecuteTest(method, "ToArray()", "size at 1024 bits");
            }
#else
            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    _ = bitSet.ToBoolArray();
                };

                ExecuteTest(method, "ToBoolArray()", "size at 1024 bits");
            }
#endif

#if ComVer1905
            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    _ = bitSet.ToList();
                };

                ExecuteTest(method, "ToList()", "size at 1024 bits");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // Trim

            {
                Action method = () =>
                {
                    Com.BitSet bitSet = new Com.BitSet(1280);

                    bitSet.Size = 1024;

                    bitSet.Trim();
                };

                ExecuteTest(method, "Trim()", "new at 1280 bits and set Size to 1024 bits before every Trim");
            }

            // Get，Set

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);
                int index = Com.Statistics.RandomInteger(1024);

                Action method = () =>
                {
                    _ = bitSet.Get(index);
                };

                ExecuteTest(method, "Get(int)", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);
                int index = Com.Statistics.RandomInteger(1024);
                bool bitValue = true;

                Action method = () =>
                {
                    bitSet.Set(index, bitValue);
                };

                ExecuteTest(method, "Set(int, bool)", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);
                bool bitValue = true;

                Action method = () =>
                {
                    bitSet.SetAll(bitValue);
                };

                ExecuteTest(method, "SetAll(bool)", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);
                int index = Com.Statistics.RandomInteger(1024);

                Action method = () =>
                {
                    bitSet.TrueForBit(index);
                };

                ExecuteTest(method, "TrueForBit(int)", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);
                int index = Com.Statistics.RandomInteger(1024);

                Action method = () =>
                {
                    bitSet.FalseForBit(index);
                };

                ExecuteTest(method, "FalseForBit(int)", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);
                int index = Com.Statistics.RandomInteger(1024);

                Action method = () =>
                {
                    bitSet.InverseBit(index);
                };

                ExecuteTest(method, "InverseBit(int)", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    bitSet.TrueForAll();
                };

                ExecuteTest(method, "TrueForAll()", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    bitSet.FalseForAll();
                };

                ExecuteTest(method, "FalseForAll()", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    bitSet.InverseAll();
                };

                ExecuteTest(method, "InverseAll()", "size at 1024 bits");
            }

            // BitCount，BitIndex

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    _ = bitSet.TrueBitCount();
                };

                ExecuteTest(method, "TrueBitCount()", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    _ = bitSet.FalseBitCount();
                };

                ExecuteTest(method, "FalseBitCount()", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    _ = bitSet.TrueBitIndex();
                };

                ExecuteTest(method, "TrueBitIndex()", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    _ = bitSet.FalseBitIndex();
                };

                ExecuteTest(method, "FalseBitIndex()", "size at 1024 bits");
            }

            // 位运算

            {
                Com.BitSet left = _GetRandomBitSet(1024);
                Com.BitSet right = left.Not();

                Action method = () =>
                {
                    _ = left.And(right);
                };

                ExecuteTest(method, "And(Com.BitSet)", "size at 1024 bits");
            }

            {
                Com.BitSet left = _GetRandomBitSet(1024);
                Com.BitSet right = left.Not();

                Action method = () =>
                {
                    _ = left.Or(right);
                };

                ExecuteTest(method, "Or(Com.BitSet)", "size at 1024 bits");
            }

            {
                Com.BitSet left = _GetRandomBitSet(1024);
                Com.BitSet right = left.Not();

                Action method = () =>
                {
                    _ = left.Xor(right);
                };

                ExecuteTest(method, "Xor(Com.BitSet)", "size at 1024 bits");
            }

            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    _ = bitSet.Not();
                };

                ExecuteTest(method, "Not()", "size at 1024 bits");
            }

            // 字符串

#if ComVer1905
            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    _ = bitSet.ToBinaryString();
                };

                ExecuteTest(method, "ToBinaryString()", "size at 1024 bits");
            }
#else
            {
                Com.BitSet bitSet = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    _ = bitSet.ToBitString();
                };

                ExecuteTest(method, "ToBitString()", "size at 1024 bits");
            }
#endif
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

                ExecuteTest(method, "IsNullOrEmpty(Com.BitSet)", "size at 1024 bits");
            }

            // Equals

            {
                Com.BitSet left = _GetRandomBitSet(1024);
                Com.BitSet right = left.Copy();

                Action method = () =>
                {
                    _ = Com.BitSet.Equals(left, right);
                };

                ExecuteTest(method, "Equals(Com.BitSet, Com.BitSet)", "size at 1024 bits");
            }

            // Compare

#if ComVer1905
            {
                Com.BitSet left = _GetRandomBitSet(1024);
                Com.BitSet right = left.Copy();

                Action method = () =>
                {
                    _ = Com.BitSet.Compare(left, right);
                };

                ExecuteTest(method, "Compare(Com.BitSet, Com.BitSet)", "size at 1024 bits");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif
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

                ExecuteTest(method, "operator ==(Com.BitSet, Com.BitSet)", "size at 1024 bits");
            }

            {
                Com.BitSet left = _GetRandomBitSet(1024);
                Com.BitSet right = left.Copy();

                Action method = () =>
                {
                    _ = (left != right);
                };

                ExecuteTest(method, "operator !=(Com.BitSet, Com.BitSet)", "size at 1024 bits");
            }

#if ComVer1905
            {
                Com.BitSet left = _GetRandomBitSet(1024);
                Com.BitSet right = left.Copy();

                Action method = () =>
                {
                    _ = (left < right);
                };

                ExecuteTest(method, "operator <(Com.BitSet, Com.BitSet)", "size at 1024 bits");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.BitSet left = _GetRandomBitSet(1024);
                Com.BitSet right = left.Copy();

                Action method = () =>
                {
                    _ = (left > right);
                };

                ExecuteTest(method, "operator >(Com.BitSet, Com.BitSet)", "size at 1024 bits");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.BitSet left = _GetRandomBitSet(1024);
                Com.BitSet right = left.Copy();

                Action method = () =>
                {
                    _ = (left <= right);
                };

                ExecuteTest(method, "operator <=(Com.BitSet, Com.BitSet)", "size at 1024 bits");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.BitSet left = _GetRandomBitSet(1024);
                Com.BitSet right = left.Copy();

                Action method = () =>
                {
                    _ = (left >= right);
                };

                ExecuteTest(method, "operator >=(Com.BitSet, Com.BitSet)", "size at 1024 bits");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // 运算

#if ComVer1905
            {
                Com.BitSet left = _GetRandomBitSet(1024);
                Com.BitSet right = left.Copy();

                Action method = () =>
                {
                    _ = left & right;
                };

                ExecuteTest(method, "operator &(Com.BitSet, Com.BitSet)", "size at 1024 bits");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.BitSet left = _GetRandomBitSet(1024);
                Com.BitSet right = left.Copy();

                Action method = () =>
                {
                    _ = left | right;
                };

                ExecuteTest(method, "operator |(Com.BitSet, Com.BitSet)", "size at 1024 bits");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.BitSet left = _GetRandomBitSet(1024);
                Com.BitSet right = left.Copy();

                Action method = () =>
                {
                    _ = left ^ right;
                };

                ExecuteTest(method, "operator ^(Com.BitSet, Com.BitSet)", "size at 1024 bits");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.BitSet right = _GetRandomBitSet(1024);

                Action method = () =>
                {
                    _ = ~right;
                };

                ExecuteTest(method, "operator ~(Com.BitSet)", "size at 1024 bits");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif
        }
    }

    sealed class ColorManipulationTest : ClassPerfTestBase
    {
        private const string _NamespaceName = "Com";
        private const string _ClassName = "ColorManipulation";

        private void ExecuteTest(Action method, string methodName, string comment)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName, comment);
        }

        private void ExecuteTest(Action method, string methodName)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(string methodName, UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName, unsupportedReason);
        }

        private void ExecuteTest(string methodName)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, string.Empty, unsupportedReason);
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
            // 颜色名

            {
                Com.ColorX color = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = Com.ColorManipulation.GetColorName(color);
                };

                ExecuteTest(method, "GetColorName(Com.ColorX)");
            }

            {
                Color color = Com.ColorManipulation.GetRandomColor();

                Action method = () =>
                {
                    _ = Com.ColorManipulation.GetColorName(color);
                };

                ExecuteTest(method, "GetColorName(System.Drawing.Color)");
            }

            // 随机色

            {
                Action method = () =>
                {
                    _ = Com.ColorManipulation.GetRandomColorX();
                };

                ExecuteTest(method, "GetRandomColorX()");
            }

            {
                Action method = () =>
                {
                    _ = Com.ColorManipulation.GetRandomColor();
                };

                ExecuteTest(method, "GetRandomColor()");
            }

            // 相反色，互补色，灰度

#if ComVer1910
            {
                Com.ColorX color = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = Com.ColorManipulation.GetInvertColor(color);
                };

                ExecuteTest(method, "GetInvertColor(Com.ColorX)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

#if ComVer1910
            {
                Color color = Com.ColorManipulation.GetRandomColor();

                Action method = () =>
                {
                    _ = Com.ColorManipulation.GetInvertColor(color);
                };

                ExecuteTest(method, "GetInvertColor(System.Drawing.Color)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

            {
                Com.ColorX color = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = Com.ColorManipulation.GetComplementaryColor(color);
                };

                ExecuteTest(method, "GetComplementaryColor(Com.ColorX)");
            }

            {
                Color color = Com.ColorManipulation.GetRandomColor();

                Action method = () =>
                {
                    _ = Com.ColorManipulation.GetComplementaryColor(color);
                };

                ExecuteTest(method, "GetComplementaryColor(System.Drawing.Color)");
            }

            {
                Com.ColorX color = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = Com.ColorManipulation.GetGrayscaleColor(color);
                };

                ExecuteTest(method, "GetGrayscaleColor(Com.ColorX)");
            }

            {
                Color color = Com.ColorManipulation.GetRandomColor();

                Action method = () =>
                {
                    _ = Com.ColorManipulation.GetGrayscaleColor(color);
                };

                ExecuteTest(method, "GetGrayscaleColor(System.Drawing.Color)");
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

                ExecuteTest(method, "BlendByRGB(Com.ColorX, Com.ColorX, double)");
            }

            {
                Color color1 = Com.ColorManipulation.GetRandomColor();
                Color color2 = Com.ColorManipulation.GetRandomColor();
                double proportion = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.BlendByRGB(color1, color2, proportion);
                };

                ExecuteTest(method, "BlendByRGB(System.Drawing.Color, System.Drawing.Color, double)");
            }

            {
                Com.ColorX color1 = Com.ColorManipulation.GetRandomColorX();
                Com.ColorX color2 = Com.ColorManipulation.GetRandomColorX();
                double proportion = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.BlendByHSV(color1, color2, proportion);
                };

                ExecuteTest(method, "BlendByHSV(Com.ColorX, Com.ColorX, double)");
            }

            {
                Color color1 = Com.ColorManipulation.GetRandomColor();
                Color color2 = Com.ColorManipulation.GetRandomColor();
                double proportion = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.BlendByHSV(color1, color2, proportion);
                };

                ExecuteTest(method, "BlendByHSV(System.Drawing.Color, System.Drawing.Color, double)");
            }

            {
                Com.ColorX color1 = Com.ColorManipulation.GetRandomColorX();
                Com.ColorX color2 = Com.ColorManipulation.GetRandomColorX();
                double proportion = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.BlendByHSL(color1, color2, proportion);
                };

                ExecuteTest(method, "BlendByHSL(Com.ColorX, Com.ColorX, double)");
            }

            {
                Color color1 = Com.ColorManipulation.GetRandomColor();
                Color color2 = Com.ColorManipulation.GetRandomColor();
                double proportion = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.BlendByHSL(color1, color2, proportion);
                };

                ExecuteTest(method, "BlendByHSL(System.Drawing.Color, System.Drawing.Color, double)");
            }

            {
                Com.ColorX color1 = Com.ColorManipulation.GetRandomColorX();
                Com.ColorX color2 = Com.ColorManipulation.GetRandomColorX();
                double proportion = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.BlendByCMYK(color1, color2, proportion);
                };

                ExecuteTest(method, "BlendByCMYK(Com.ColorX, Com.ColorX, double)");
            }

            {
                Color color1 = Com.ColorManipulation.GetRandomColor();
                Color color2 = Com.ColorManipulation.GetRandomColor();
                double proportion = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.BlendByCMYK(color1, color2, proportion);
                };

                ExecuteTest(method, "BlendByCMYK(System.Drawing.Color, System.Drawing.Color, double)");
            }

            {
                Com.ColorX color1 = Com.ColorManipulation.GetRandomColorX();
                Com.ColorX color2 = Com.ColorManipulation.GetRandomColorX();
                double proportion = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.BlendByLAB(color1, color2, proportion);
                };

                ExecuteTest(method, "BlendByLAB(Com.ColorX, Com.ColorX, double)");
            }

            {
                Color color1 = Com.ColorManipulation.GetRandomColor();
                Color color2 = Com.ColorManipulation.GetRandomColor();
                double proportion = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.BlendByLAB(color1, color2, proportion);
                };

                ExecuteTest(method, "BlendByLAB(System.Drawing.Color, System.Drawing.Color, double)");
            }

            // Shift

            {
                Com.ColorX color = Com.ColorManipulation.GetRandomColorX();
                double level = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.ShiftLightnessByHSV(color, level);
                };

                ExecuteTest(method, "ShiftLightnessByHSV(Com.ColorX, double)");
            }

            {
                Color color = Com.ColorManipulation.GetRandomColor();
                double level = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.ShiftLightnessByHSV(color, level);
                };

                ExecuteTest(method, "ShiftLightnessByHSV(System.Drawing.Color, double)");
            }

            {
                Com.ColorX color = Com.ColorManipulation.GetRandomColorX();
                double level = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.ShiftLightnessByHSL(color, level);
                };

                ExecuteTest(method, "ShiftLightnessByHSL(Com.ColorX, double)");
            }

            {
                Color color = Com.ColorManipulation.GetRandomColor();
                double level = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.ShiftLightnessByHSL(color, level);
                };

                ExecuteTest(method, "ShiftLightnessByHSL(System.Drawing.Color, double)");
            }

            {
                Com.ColorX color = Com.ColorManipulation.GetRandomColorX();
                double level = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.ShiftLightnessByLAB(color, level);
                };

                ExecuteTest(method, "ShiftLightnessByLAB(Com.ColorX, double)");
            }

            {
                Color color = Com.ColorManipulation.GetRandomColor();
                double level = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.ShiftLightnessByLAB(color, level);
                };

                ExecuteTest(method, "ShiftLightnessByLAB(System.Drawing.Color, double)");
            }

            {
                Com.ColorX color = Com.ColorManipulation.GetRandomColorX();
                double level = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.ShiftSaturationByHSV(color, level);
                };

                ExecuteTest(method, "ShiftSaturationByHSV(Com.ColorX, double)");
            }

            {
                Color color = Com.ColorManipulation.GetRandomColor();
                double level = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.ShiftSaturationByHSV(color, level);
                };

                ExecuteTest(method, "ShiftSaturationByHSV(System.Drawing.Color, double)");
            }

            {
                Com.ColorX color = Com.ColorManipulation.GetRandomColorX();
                double level = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.ShiftSaturationByHSL(color, level);
                };

                ExecuteTest(method, "ShiftSaturationByHSL(Com.ColorX, double)");
            }

            {
                Color color = Com.ColorManipulation.GetRandomColor();
                double level = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorManipulation.ShiftSaturationByHSL(color, level);
                };

                ExecuteTest(method, "ShiftSaturationByHSL(System.Drawing.Color, double)");
            }
        }

        protected override void Operator()
        {

        }
    }

    sealed class ColorXTest : ClassPerfTestBase
    {
        private const string _NamespaceName = "Com";
        private const string _ClassName = "ColorX";

        private void ExecuteTest(Action method, string methodName, string comment)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName, comment);
        }

        private void ExecuteTest(Action method, string methodName)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(string methodName, UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName, unsupportedReason);
        }

        private void ExecuteTest(string methodName)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, string.Empty, unsupportedReason);
        }

        //

        protected override void Constructor()
        {
            {
                Color color = Com.ColorManipulation.GetRandomColor();

                Action method = () =>
                {
                    _ = new Com.ColorX(color);
                };

                ExecuteTest(method, "ColorX(System.Drawing.Color)");
            }

            {
                int argb = Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = new Com.ColorX(argb);
                };

                ExecuteTest(method, "ColorX(int)");
            }

#if ComVer1905
            {
                string hexCode = Com.ColorManipulation.GetRandomColorX().ARGBHexCode;

                Action method = () =>
                {
                    _ = new Com.ColorX(hexCode);
                };

                ExecuteTest(method, "ColorX(string)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif
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

                ExecuteTest(method, "IsEmpty.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.IsTransparent;
                };

                ExecuteTest(method, "IsTransparent.get()");
            }

            {
#if ComVer1910
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.IsTrueColor;
                };

                ExecuteTest(method, "IsTrueColor.get()");
#else
                ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif
            }

            // Opacity

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.Opacity;
                };

                ExecuteTest(method, "Opacity.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    colorX.Opacity = value;
                };

                ExecuteTest(method, "Opacity.set(double)");
            }

            // RGB

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.Alpha;
                };

                ExecuteTest(method, "Alpha.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(255);

                Action method = () =>
                {
                    colorX.Alpha = value;
                };

                ExecuteTest(method, "Alpha.set(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.Red;
                };

                ExecuteTest(method, "Red.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(255);

                Action method = () =>
                {
                    colorX.Red = value;
                };

                ExecuteTest(method, "Red.set(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.Green;
                };

                ExecuteTest(method, "Green.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(255);

                Action method = () =>
                {
                    colorX.Green = value;
                };

                ExecuteTest(method, "Green.set(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.Blue;
                };

                ExecuteTest(method, "Blue.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(255);

                Action method = () =>
                {
                    colorX.Blue = value;
                };

                ExecuteTest(method, "Blue.set(double)");
            }

            // HSV

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.Hue_HSV;
                };

                ExecuteTest(method, "Hue_HSV.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(360);

                Action method = () =>
                {
                    colorX.Hue_HSV = value;
                };

                ExecuteTest(method, "Hue_HSV.set(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.Saturation_HSV;
                };

                ExecuteTest(method, "Saturation_HSV.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    colorX.Saturation_HSV = value;
                };

                ExecuteTest(method, "Saturation_HSV.set(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.Brightness;
                };

                ExecuteTest(method, "Brightness.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    colorX.Brightness = value;
                };

                ExecuteTest(method, "Brightness.set(double)");
            }

            // HSL

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.Hue_HSL;
                };

                ExecuteTest(method, "Hue_HSL.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(360);

                Action method = () =>
                {
                    colorX.Hue_HSL = value;
                };

                ExecuteTest(method, "Hue_HSL.set(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.Saturation_HSL;
                };

                ExecuteTest(method, "Saturation_HSL.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    colorX.Saturation_HSL = value;
                };

                ExecuteTest(method, "Saturation_HSL.set(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.Lightness_HSL;
                };

                ExecuteTest(method, "Lightness_HSL.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    colorX.Lightness_HSL = value;
                };

                ExecuteTest(method, "Lightness_HSL.set(double)");
            }

            // CMYK

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.Cyan;
                };

                ExecuteTest(method, "Cyan.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    colorX.Cyan = value;
                };

                ExecuteTest(method, "Cyan.set(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.Magenta;
                };

                ExecuteTest(method, "Magenta.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    colorX.Magenta = value;
                };

                ExecuteTest(method, "Magenta.set(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.Yellow;
                };

                ExecuteTest(method, "Yellow.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    colorX.Yellow = value;
                };

                ExecuteTest(method, "Yellow.set(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.Black;
                };

                ExecuteTest(method, "Black.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    colorX.Black = value;
                };

                ExecuteTest(method, "Black.set(double)");
            }

            // LAB

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.Lightness_LAB;
                };

                ExecuteTest(method, "Lightness_LAB.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    colorX.Lightness_LAB = value;
                };

                ExecuteTest(method, "Lightness_LAB.set(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.GreenRed;
                };

                ExecuteTest(method, "GreenRed.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(-128, 128);

                Action method = () =>
                {
                    colorX.GreenRed = value;
                };

                ExecuteTest(method, "GreenRed.set(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.BlueYellow;
                };

                ExecuteTest(method, "BlueYellow.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(-128, 128);

                Action method = () =>
                {
                    colorX.BlueYellow = value;
                };

                ExecuteTest(method, "BlueYellow.set(double)");
            }

            // YUV

#if ComVer1910
            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.Luminance;
                };

                ExecuteTest(method, "Luminance.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

#if ComVer1910
            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(1);

                Action method = () =>
                {
                    colorX.Luminance = value;
                };

                ExecuteTest(method, "Luminance.set(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

#if ComVer1910
            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.ChrominanceBlue;
                };

                ExecuteTest(method, "ChrominanceBlue.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

#if ComVer1910
            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(-0.5, 0.5);

                Action method = () =>
                {
                    colorX.ChrominanceBlue = value;
                };

                ExecuteTest(method, "ChrominanceBlue.set(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

#if ComVer1910
            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.ChrominanceRed;
                };

                ExecuteTest(method, "ChrominanceRed.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

#if ComVer1910
            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(-0.5, 0.5);

                Action method = () =>
                {
                    colorX.ChrominanceRed = value;
                };

                ExecuteTest(method, "ChrominanceRed.set(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

            // 向量

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.RGB;
                };

                ExecuteTest(method, "RGB.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                Com.PointD3D value = Com.ColorManipulation.GetRandomColorX().RGB;

                Action method = () =>
                {
                    colorX.RGB = value;
                };

                ExecuteTest(method, "RGB.set(Com.PointD3D)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.HSV;
                };

                ExecuteTest(method, "HSV.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                Com.PointD3D value = Com.ColorManipulation.GetRandomColorX().HSV;

                Action method = () =>
                {
                    colorX.HSV = value;
                };

                ExecuteTest(method, "HSV.set(Com.PointD3D)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.HSL;
                };

                ExecuteTest(method, "HSL.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                Com.PointD3D value = Com.ColorManipulation.GetRandomColorX().HSL;

                Action method = () =>
                {
                    colorX.HSL = value;
                };

                ExecuteTest(method, "HSL.set(Com.PointD3D)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.CMYK;
                };

                ExecuteTest(method, "CMYK.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                Com.PointD4D value = Com.ColorManipulation.GetRandomColorX().CMYK;

                Action method = () =>
                {
                    colorX.CMYK = value;
                };

                ExecuteTest(method, "CMYK.set(Com.PointD4D)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.LAB;
                };

                ExecuteTest(method, "LAB.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                Com.PointD3D value = Com.ColorManipulation.GetRandomColorX().LAB;

                Action method = () =>
                {
                    colorX.LAB = value;
                };

                ExecuteTest(method, "LAB.set(Com.PointD3D)");
            }

#if ComVer1910
            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.YUV;
                };

                ExecuteTest(method, "YUV.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

#if ComVer1910
            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                Com.PointD3D value = Com.ColorManipulation.GetRandomColorX().YUV;

                Action method = () =>
                {
                    colorX.YUV = value;
                };

                ExecuteTest(method, "YUV.set(Com.PointD3D)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

            // 相反色，互补色，灰度

#if ComVer1910
            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.Invert;
                };

                ExecuteTest(method, "Invert.get()");
            }
#else
            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.ComplementaryColor;
                };

                ExecuteTest(method, "ComplementaryColor.get()");
            }
#endif

#if ComVer1910
            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.Complementary;
                };

                ExecuteTest(method, "Complementary.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

#if ComVer1910
            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.ComplementaryColor;
                };

                ExecuteTest(method, "ComplementaryColor.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

#if ComVer1910
            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.Grayscale;
                };

                ExecuteTest(method, "Grayscale.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.GrayscaleColor;
                };

                ExecuteTest(method, "GrayscaleColor.get()");
            }

            // HexCode

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.ARGBHexCode;
                };

                ExecuteTest(method, "ARGBHexCode.get()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.RGBHexCode;
                };

                ExecuteTest(method, "RGBHexCode.get()");
            }

            // Name

#if ComVer1910
            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.Name;
                };

                ExecuteTest(method, "Name.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif
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

                ExecuteTest(method, "Equals(object)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.GetHashCode();
                };

                ExecuteTest(method, "GetHashCode()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.ToString();
                };

                ExecuteTest(method, "ToString()");
            }

            // Equals

            {
                Com.ColorX left = Com.ColorManipulation.GetRandomColorX();
                Com.ColorX right = left;

                Action method = () =>
                {
                    _ = left.Equals(right);
                };

                ExecuteTest(method, "Equals(Com.ColorX)");
            }

            // To

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.ToColor();
                };

                ExecuteTest(method, "ToColor()");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = colorX.ToARGB();
                };

                ExecuteTest(method, "ToARGB()");
            }

            // AtOpacity

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = colorX.AtOpacity(value);
                };

                ExecuteTest(method, "AtOpacity(double)");
            }

            // AtRGB

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(255);

                Action method = () =>
                {
                    _ = colorX.AtAlpha(value);
                };

                ExecuteTest(method, "AtAlpha(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(255);

                Action method = () =>
                {
                    _ = colorX.AtRed(value);
                };

                ExecuteTest(method, "AtRed(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(255);

                Action method = () =>
                {
                    _ = colorX.AtGreen(value);
                };

                ExecuteTest(method, "AtGreen(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(255);

                Action method = () =>
                {
                    _ = colorX.AtBlue(value);
                };

                ExecuteTest(method, "AtBlue(double)");
            }

            // AtHSV

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(360);

                Action method = () =>
                {
                    _ = colorX.AtHue_HSV(value);
                };

                ExecuteTest(method, "AtHue_HSV(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = colorX.AtSaturation_HSV(value);
                };

                ExecuteTest(method, "AtSaturation_HSV(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = colorX.AtBrightness(value);
                };

                ExecuteTest(method, "AtBrightness(double)");
            }

            // AtHSL

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(360);

                Action method = () =>
                {
                    _ = colorX.AtHue_HSL(value);
                };

                ExecuteTest(method, "AtHue_HSL(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = colorX.AtSaturation_HSL(value);
                };

                ExecuteTest(method, "AtSaturation_HSL(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = colorX.AtLightness_HSL(value);
                };

                ExecuteTest(method, "AtLightness_HSL(double)");
            }

            // AtCMYK

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = colorX.AtCyan(value);
                };

                ExecuteTest(method, "AtCyan(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = colorX.AtMagenta(value);
                };

                ExecuteTest(method, "AtMagenta(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = colorX.AtYellow(value);
                };

                ExecuteTest(method, "AtYellow(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = colorX.AtBlack(value);
                };

                ExecuteTest(method, "AtBlack(double)");
            }

            // AtLAB

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = colorX.AtLightness_LAB(value);
                };

                ExecuteTest(method, "AtLightness_LAB(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(-128, 128);

                Action method = () =>
                {
                    _ = colorX.AtGreenRed(value);
                };

                ExecuteTest(method, "AtGreenRed(double)");
            }

            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(-128, 128);

                Action method = () =>
                {
                    _ = colorX.AtBlueYellow(value);
                };

                ExecuteTest(method, "AtBlueYellow(double)");
            }

            // AtYUV

#if ComVer1910
            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(1);

                Action method = () =>
                {
                    _ = colorX.AtLuminance(value);
                };

                ExecuteTest(method, "AtLuminance(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

#if ComVer1910
            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(-0.5, 0.5);

                Action method = () =>
                {
                    _ = colorX.AtChrominanceBlue(value);
                };

                ExecuteTest(method, "AtChrominanceBlue(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

#if ComVer1910
            {
                Com.ColorX colorX = Com.ColorManipulation.GetRandomColorX();
                double value = Com.Statistics.RandomDouble(-0.5, 0.5);

                Action method = () =>
                {
                    _ = colorX.AtChrominanceRed(value);
                };

                ExecuteTest(method, "AtChrominanceRed(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif
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

                ExecuteTest(method, "Equals(Com.ColorX, Com.ColorX)");
            }

            // FromColor

            {
                int alpha = Com.Statistics.RandomInteger(255);
                Color color = Com.ColorManipulation.GetRandomColor();

                Action method = () =>
                {
                    _ = Com.ColorX.FromColor(alpha, color);
                };

                ExecuteTest(method, "FromColor(int, System.Drawing.Color)");
            }

            {
                Color color = Com.ColorManipulation.GetRandomColor();

                Action method = () =>
                {
                    _ = Com.ColorX.FromColor(color);
                };

                ExecuteTest(method, "FromColor(System.Drawing.Color)");
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

                ExecuteTest(method, "FromRGB(double, double, double, double)");
            }

            {
                double red = Com.Statistics.RandomDouble(255);
                double green = Com.Statistics.RandomDouble(255);
                double blue = Com.Statistics.RandomDouble(255);

                Action method = () =>
                {
                    _ = Com.ColorX.FromRGB(red, green, blue);
                };

                ExecuteTest(method, "FromRGB(double, double, double)");
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

                ExecuteTest(method, "FromRGB(double, Com.PointD3D)");
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

                ExecuteTest(method, "FromRGB(Com.PointD3D)");
            }

            {
                int argb = Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = Com.ColorX.FromRGB(argb);
                };

                ExecuteTest(method, "FromRGB(int)");
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

                ExecuteTest(method, "FromHSV(double, double, double, double)");
            }

            {
                double hue = Com.Statistics.RandomDouble(360);
                double saturation = Com.Statistics.RandomDouble(100);
                double brightness = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorX.FromHSV(hue, saturation, brightness);
                };

                ExecuteTest(method, "FromHSV(double, double, double)");
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

                ExecuteTest(method, "FromHSV(Com.PointD3D, double)");
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

                ExecuteTest(method, "FromHSV(Com.PointD3D)");
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

                ExecuteTest(method, "FromHSL(double, double, double, double)");
            }

            {
                double hue = Com.Statistics.RandomDouble(360);
                double saturation = Com.Statistics.RandomDouble(100);
                double lightness = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorX.FromHSL(hue, saturation, lightness);
                };

                ExecuteTest(method, "FromHSL(double, double, double)");
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

                ExecuteTest(method, "FromHSL(Com.PointD3D, double)");
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

                ExecuteTest(method, "FromHSL(Com.PointD3D)");
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

                ExecuteTest(method, "FromCMYK(double, double, double, double, double)");
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

                ExecuteTest(method, "FromCMYK(double, double, double, double)");
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

                ExecuteTest(method, "FromCMYK(Com.PointD4D, double)");
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

                ExecuteTest(method, "FromCMYK(Com.PointD4D)");
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

                ExecuteTest(method, "FromLAB(double, double, double, double)");
            }

            {
                double lightness = Com.Statistics.RandomDouble(100);
                double greenRed = Com.Statistics.RandomDouble(-128, 128);
                double blueYellow = Com.Statistics.RandomDouble(-128, 128);

                Action method = () =>
                {
                    _ = Com.ColorX.FromLAB(lightness, greenRed, blueYellow);
                };

                ExecuteTest(method, "FromLAB(double, double, double)");
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

                ExecuteTest(method, "FromLAB(Com.PointD3D, double)");
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

                ExecuteTest(method, "FromLAB(Com.PointD3D)");
            }

            // FromYUV

#if ComVer1910
            {
                double luminance = Com.Statistics.RandomDouble(1);
                double chrominanceBlue = Com.Statistics.RandomDouble(-0.5, 0.5);
                double chrominanceRed = Com.Statistics.RandomDouble(-0.5, 0.5);
                double opacity = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorX.FromYUV(luminance, chrominanceBlue, chrominanceRed, opacity);
                };

                ExecuteTest(method, "FromYUV(double, double, double, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

#if ComVer1910
            {
                double luminance = Com.Statistics.RandomDouble(1);
                double chrominanceBlue = Com.Statistics.RandomDouble(-0.5, 0.5);
                double chrominanceRed = Com.Statistics.RandomDouble(-0.5, 0.5);

                Action method = () =>
                {
                    _ = Com.ColorX.FromYUV(luminance, chrominanceBlue, chrominanceRed);
                };

                ExecuteTest(method, "FromYUV(double, double, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

#if ComVer1910
            {
                double luminance = Com.Statistics.RandomDouble(1);
                double chrominanceBlue = Com.Statistics.RandomDouble(-0.5, 0.5);
                double chrominanceRed = Com.Statistics.RandomDouble(-0.5, 0.5);
                Com.PointD3D yuv = new Com.PointD3D(luminance, chrominanceBlue, chrominanceRed);
                double opacity = Com.Statistics.RandomDouble(100);

                Action method = () =>
                {
                    _ = Com.ColorX.FromYUV(yuv, opacity);
                };

                ExecuteTest(method, "FromYUV(Com.PointD3D, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

#if ComVer1910
            {
                double luminance = Com.Statistics.RandomDouble(1);
                double chrominanceBlue = Com.Statistics.RandomDouble(-0.5, 0.5);
                double chrominanceRed = Com.Statistics.RandomDouble(-0.5, 0.5);
                Com.PointD3D yuv = new Com.PointD3D(luminance, chrominanceBlue, chrominanceRed);

                Action method = () =>
                {
                    _ = Com.ColorX.FromYUV(yuv);
                };

                ExecuteTest(method, "FromYUV(Com.PointD3D)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

            // FromHexCode

            {
                string hexCode = Com.ColorManipulation.GetRandomColorX().ARGBHexCode;

                Action method = () =>
                {
                    _ = Com.ColorX.FromHexCode(hexCode);
                };

                ExecuteTest(method, "FromHexCode(string)");
            }

            // FromName

#if ComVer1910
            {
                string name = Com.ColorManipulation.GetRandomColorX().Name;

                Action method = () =>
                {
                    _ = Com.ColorX.FromName(name);
                };

                ExecuteTest(method, "FromName(string)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

            // RandomColor

            {
                Action method = () =>
                {
                    _ = Com.ColorX.RandomColor();
                };

                ExecuteTest(method, "RandomColor()");
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

                ExecuteTest(method, "operator ==(Com.ColorX, Com.ColorX)");
            }

            {
                Com.ColorX left = Com.ColorManipulation.GetRandomColorX();
                Com.ColorX right = left;

                Action method = () =>
                {
                    _ = (left != right);
                };

                ExecuteTest(method, "operator !=(Com.ColorX, Com.ColorX)");
            }

            // 类型转换

#if ComVer1905
            {
                Com.ColorX color = Com.ColorManipulation.GetRandomColorX();

                Action method = () =>
                {
                    _ = (Color)color;
                };

                ExecuteTest(method, "explicit operator System.Drawing.Color(Com.ColorX)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Color color = Com.ColorManipulation.GetRandomColor();

                Action method = () =>
                {
                    _ = (Com.ColorX)color;
                };

                ExecuteTest(method, "implicit operator Com.ColorX(System.Drawing.Color)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif
        }
    }

    sealed class ComplexTest : ClassPerfTestBase
    {
        private const string _NamespaceName = "Com";
        private const string _ClassName = "Complex";

        private void ExecuteTest(Action method, string methodName, string comment)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName, comment);
        }

        private void ExecuteTest(Action method, string methodName)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(string methodName, UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName, unsupportedReason);
        }

        private void ExecuteTest(string methodName)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, string.Empty, unsupportedReason);
        }

        //

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

                ExecuteTest(method, "Complex(double, double)");
            }

            {
                double real = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = new Com.Complex(real);
                };

                ExecuteTest(method, "Complex(double)");
            }

            {
                Com.PointD pt = new Com.PointD(Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = new Com.Complex(pt);
                };

                ExecuteTest(method, "Complex(Com.PointD)");
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

                ExecuteTest(method, "IsNaN.get()");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = comp.IsInfinity;
                };

                ExecuteTest(method, "IsInfinity.get()");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = comp.IsNaNOrInfinity;
                };

                ExecuteTest(method, "IsNaNOrInfinity.get()");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = comp.IsZero;
                };

                ExecuteTest(method, "IsZero.get()");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = comp.IsOne;
                };

                ExecuteTest(method, "IsOne.get()");
            }

#if ComVer1905
            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = comp.IsImaginaryOne;
                };

                ExecuteTest(method, "IsImaginaryOne.get()");
            }
#else
            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = comp.IsI;
                };

                ExecuteTest(method, "IsI.get()");
            }
#endif

            // 分量

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = comp.Real;
                };

                ExecuteTest(method, "Real.get()");
            }

            {
                Com.Complex comp = _GetRandomComplex();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    comp.Real = value;
                };

                ExecuteTest(method, "Real.set(double)");
            }

#if ComVer1905
            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = comp.Imaginary;
                };

                ExecuteTest(method, "Imaginary.get()");
            }
#else
            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = comp.Image;
                };

                ExecuteTest(method, "Image.get()");
            }
#endif

#if ComVer1905
            {
                Com.Complex comp = _GetRandomComplex();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    comp.Imaginary = value;
                };

                ExecuteTest(method, "Imaginary.set(double)");
            }
#else
            {
                Com.Complex comp = _GetRandomComplex();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    comp.Image = value;
                };

                ExecuteTest(method, "Image.set(double)");
            }
#endif

            // 模与辐角

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = comp.Module;
                };

                ExecuteTest(method, "Module.get()");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = comp.ModuleSquared;
                };

                ExecuteTest(method, "ModuleSquared.get()");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = comp.Argument;
                };

                ExecuteTest(method, "Argument.get()");
            }

            // 相反数、倒数、共轭

#if ComVer1910
            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = comp.Opposite;
                };

                ExecuteTest(method, "Opposite.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

#if ComVer1905
            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = comp.Reciprocal;
                };

                ExecuteTest(method, "Reciprocal.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = comp.Conjugate;
                };

                ExecuteTest(method, "Conjugate.get()");
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

                ExecuteTest(method, "Equals(object)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = comp.GetHashCode();
                };

                ExecuteTest(method, "GetHashCode()");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = comp.ToString();
                };

                ExecuteTest(method, "ToString()");
            }

            // Equals

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = left;

                Action method = () =>
                {
                    _ = left.Equals(right);
                };

                ExecuteTest(method, "Equals(Com.Complex)");
            }

            // CompareTo

#if ComVer1905
            {
                Com.Complex left = _GetRandomComplex();
                object right = left;

                Action method = () =>
                {
                    _ = left.CompareTo(right);
                };

                ExecuteTest(method, "CompareTo(object)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = left;

                Action method = () =>
                {
                    _ = left.CompareTo(right);
                };

                ExecuteTest(method, "CompareTo(Com.Complex)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // To

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = comp.ToPointD();
                };

                ExecuteTest(method, "ToPointD()");
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

                ExecuteTest(method, "Equals(Com.Complex, Com.Complex)");
            }

            // Compare

#if ComVer1905
            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = left;

                Action method = () =>
                {
                    _ = Com.Complex.Compare(left, right);
                };

                ExecuteTest(method, "CompareTo(Com.Complex, Com.Complex)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // From

            {
                Com.PointD pt = new Com.PointD(Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = Com.Complex.FromPointD(pt);
                };

                ExecuteTest(method, "FromPointD(Com.PointD)");
            }

#if ComVer1905
            {
                double module = Com.Statistics.RandomDouble(-1E18, 1E18);
                double argument = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    Com.Complex complex = Com.Complex.FromPolarCoordinates(module, argument);
                };

                ExecuteTest(method, "FromPolarCoordinates(double, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // 幂函数，指数函数，对数函数

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Sqr(comp);
                };

                ExecuteTest(method, "Sqr(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Sqrt(comp);
                };

                ExecuteTest(method, "Sqrt(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Exp(comp);
                };

                ExecuteTest(method, "Exp(Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Pow(left, right);
                };

                ExecuteTest(method, "Pow(Com.Complex, Com.Complex)");
            }

#if ComVer1905
            {
                Com.Complex left = _GetRandomComplex();
                double right = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.Complex.Pow(left, right);
                };

                ExecuteTest(method, "Pow(Com.Complex, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double left = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Pow(left, right);
                };

                ExecuteTest(method, "Pow(double, Com.Complex)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Log(comp);
                };

                ExecuteTest(method, "Log(Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Log(left, right);
                };

                ExecuteTest(method, "Log(Com.Complex, Com.Complex)");
            }

#if ComVer1905
            {
                Com.Complex left = _GetRandomComplex();
                double right = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.Complex.Log(left, right);
                };

                ExecuteTest(method, "Log(Com.Complex, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double left = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Log(left, right);
                };

                ExecuteTest(method, "Log(double, Com.Complex)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // 三角函数

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Sin(comp);
                };

                ExecuteTest(method, "Sin(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Cos(comp);
                };

                ExecuteTest(method, "Cos(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Tan(comp);
                };

                ExecuteTest(method, "Tan(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Asin(comp);
                };

                ExecuteTest(method, "Asin(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Acos(comp);
                };

                ExecuteTest(method, "Acos(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Atan(comp);
                };

                ExecuteTest(method, "Atan(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Sinh(comp);
                };

                ExecuteTest(method, "Sinh(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Cosh(comp);
                };

                ExecuteTest(method, "Cosh(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Tanh(comp);
                };

                ExecuteTest(method, "Tanh(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Asinh(comp);
                };

                ExecuteTest(method, "Asinh(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Acosh(comp);
                };

                ExecuteTest(method, "Acosh(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Atanh(comp);
                };

                ExecuteTest(method, "Atanh(Com.Complex)");
            }

            // 初等函数

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Abs(comp);
                };

                ExecuteTest(method, "Abs(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Sign(comp);
                };

                ExecuteTest(method, "Sign(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Ceiling(comp);
                };

                ExecuteTest(method, "Ceiling(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Floor(comp);
                };

                ExecuteTest(method, "Floor(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Round(comp);
                };

                ExecuteTest(method, "Round(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Truncate(comp);
                };

                ExecuteTest(method, "Truncate(Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Max(left, right);
                };

                ExecuteTest(method, "Max(Com.Complex, Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    _ = Com.Complex.Min(left, right);
                };

                ExecuteTest(method, "Min(Com.Complex, Com.Complex)");
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

                ExecuteTest(method, "operator ==(Com.Complex, Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = left;

                Action method = () =>
                {
                    _ = (left != right);
                };

                ExecuteTest(method, "operator !=(Com.Complex, Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    _ = (left < right);
                };

                ExecuteTest(method, "operator <(Com.Complex, Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    _ = (left > right);
                };

                ExecuteTest(method, "operator >(Com.Complex, Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    _ = (left <= right);
                };

                ExecuteTest(method, "operator <=(Com.Complex, Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    _ = (left >= right);
                };

                ExecuteTest(method, "operator >=(Com.Complex, Com.Complex)");
            }

            // 运算

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = +comp;
                };

                ExecuteTest(method, "operator +(Com.Complex)");
            }

            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = -comp;
                };

                ExecuteTest(method, "operator -(Com.Complex)");
            }

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    _ = left + right;
                };

                ExecuteTest(method, "operator +(Com.Complex, Com.Complex)");
            }

#if ComVer1905
            {
                Com.Complex left = _GetRandomComplex();
                double right = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = left + right;
                };

                ExecuteTest(method, "operator +(Com.Complex, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double left = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    _ = left + right;
                };

                ExecuteTest(method, "operator +(double, Com.Complex)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    _ = left - right;
                };

                ExecuteTest(method, "operator -(Com.Complex, Com.Complex)");
            }

#if ComVer1905
            {
                Com.Complex left = _GetRandomComplex();
                double right = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = left - right;
                };

                ExecuteTest(method, "operator -(Com.Complex, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double left = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    _ = left - right;
                };

                ExecuteTest(method, "operator -(double, Com.Complex)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    _ = left * right;
                };

                ExecuteTest(method, "operator *(Com.Complex, Com.Complex)");
            }

#if ComVer1905
            {
                Com.Complex left = _GetRandomComplex();
                double right = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = left * right;
                };

                ExecuteTest(method, "operator *(Com.Complex, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double left = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    _ = left * right;
                };

                ExecuteTest(method, "operator *(double, Com.Complex)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            {
                Com.Complex left = _GetRandomComplex();
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    _ = left / right;
                };

                ExecuteTest(method, "operator /(Com.Complex, Com.Complex)");
            }

#if ComVer1905
            {
                Com.Complex left = _GetRandomComplex();
                double right = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = left / right;
                };

                ExecuteTest(method, "operator /(Com.Complex, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double left = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.Complex right = _GetRandomComplex();

                Action method = () =>
                {
                    _ = left / right;
                };

                ExecuteTest(method, "operator /(double, Com.Complex)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // 类型转换

#if ComVer1905
            {
                Com.Complex comp = _GetRandomComplex();

                Action method = () =>
                {
                    _ = (Com.PointD)comp;
                };

                ExecuteTest(method, "explicit operator Com.PointD(Com.Complex)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double real = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = (Com.Complex)real;
                };

                ExecuteTest(method, "explicit operator Com.Complex(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif
        }
    }

    sealed class DateTimeXTest : ClassPerfTestBase
    {
        private const string _NamespaceName = "Com";
        private const string _ClassName = "DateTimeX";

        private void ExecuteTest(Action method, string methodName, string comment)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName, comment);
        }

        private void ExecuteTest(Action method, string methodName)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(string methodName, UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName, unsupportedReason);
        }

        private void ExecuteTest(string methodName)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, string.Empty, unsupportedReason);
        }

        //

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

                ExecuteTest(method, "DateTimeX(decimal, double)");
            }

            {
                decimal totalMilliseconds = (decimal)Com.Statistics.RandomDouble(-1E16, 1E16);

                Action method = () =>
                {
                    _ = new Com.DateTimeX(totalMilliseconds);
                };

                ExecuteTest(method, "DateTimeX(decimal)");
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

                ExecuteTest(method, "DateTimeX(long, int, int, int, int, int, int, double)");
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

                ExecuteTest(method, "DateTimeX(long, int, int, int, int, int, int)");
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

                ExecuteTest(method, "DateTimeX(long, int, int, int, int, int, double)");
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

                ExecuteTest(method, "DateTimeX(long, int, int, int, int, int)");
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

                ExecuteTest(method, "DateTimeX(long, int, int, double)");
            }

            {
                long year = Com.Statistics.RandomInteger(1, 10000) * ((Com.Statistics.RandomInteger() % 2) * 2 - 1);
                int month = Com.Statistics.RandomInteger(1, 13);
                int day = Com.Statistics.RandomInteger(1, Com.DateTimeX.DaysInMonth(year, month) + 1);

                Action method = () =>
                {
                    _ = new Com.DateTimeX(year, month, day);
                };

                ExecuteTest(method, "DateTimeX(long, int, int)");
            }

            // UtcOffset，DateTimeX，DateTime

            {
                Com.DateTimeX dateTime = _GetRandomDateTimeX();
                double utcOffset = Com.Statistics.RandomDouble(-12, 12);

                Action method = () =>
                {
                    _ = new Com.DateTimeX(dateTime, utcOffset);
                };

                ExecuteTest(method, "DateTimeX(Com.DateTimeX, double)");
            }

            {
                Com.DateTimeX dateTime = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = new Com.DateTimeX(dateTime);
                };

                ExecuteTest(method, "DateTimeX(Com.DateTimeX)");
            }

            {
                DateTime dateTime = _GetRandomDateTime();
                double utcOffset = Com.Statistics.RandomDouble(-12, 12);

                Action method = () =>
                {
                    _ = new Com.DateTimeX(dateTime, utcOffset);
                };

                ExecuteTest(method, "DateTimeX(System.DateTime, double)");
            }

            {
                DateTime dateTime = _GetRandomDateTime();

                Action method = () =>
                {
                    _ = new Com.DateTimeX(dateTime);
                };

                ExecuteTest(method, "DateTimeX(System.DateTime)");
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

                ExecuteTest(method, "IsEmpty.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.IsChristianEra;
                };

                ExecuteTest(method, "IsChristianEra.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.IsMinValue;
                };

                ExecuteTest(method, "IsMinValue.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.IsMaxValue;
                };

                ExecuteTest(method, "IsMaxValue.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.IsAnnoDomini;
                };

                ExecuteTest(method, "IsAnnoDomini.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.IsBeforeChrist;
                };

                ExecuteTest(method, "IsBeforeChrist.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.IsLeap;
                };

                ExecuteTest(method, "IsLeap.get()");
            }

            // UtcOffset，TotalMS，YMD-hms

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.UtcOffset;
                };

                ExecuteTest(method, "UtcOffset.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                double value = Com.Statistics.RandomDouble(-12, 12);

                Action method = () =>
                {
                    dateTimeX.UtcOffset = value;
                };

                ExecuteTest(method, "UtcOffset.set(double)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.TotalMilliseconds;
                };

                ExecuteTest(method, "TotalMilliseconds.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                decimal value = (decimal)Com.Statistics.RandomDouble(-1E16, 1E16);

                Action method = () =>
                {
                    dateTimeX.TotalMilliseconds = value;
                };

                ExecuteTest(method, "TotalMilliseconds.set(decimal)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.Year;
                };

                ExecuteTest(method, "Year.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                dateTimeX.Day = Com.Statistics.RandomInteger(1, 29);
                long value = Com.Statistics.RandomInteger(1, 10000) * ((Com.Statistics.RandomInteger() % 2) * 2 - 1);

                Action method = () =>
                {
                    dateTimeX.Year = value;
                };

                ExecuteTest(method, "Year.set(long)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.Month;
                };

                ExecuteTest(method, "Month.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                dateTimeX.Day = Com.Statistics.RandomInteger(1, 29);
                int value = Com.Statistics.RandomInteger(1, 13);

                Action method = () =>
                {
                    dateTimeX.Month = value;
                };

                ExecuteTest(method, "Month.set(int)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.Day;
                };

                ExecuteTest(method, "Day.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                int value = Com.Statistics.RandomInteger(1, Com.DateTimeX.DaysInMonth(dateTimeX.Year, dateTimeX.Month) + 1);

                Action method = () =>
                {
                    dateTimeX.Day = value;
                };

                ExecuteTest(method, "Day.set(int)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.Hour;
                };

                ExecuteTest(method, "Hour.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                int value = Com.Statistics.RandomInteger(0, 24);

                Action method = () =>
                {
                    dateTimeX.Hour = value;
                };

                ExecuteTest(method, "Hour.set(int)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.Minute;
                };

                ExecuteTest(method, "Minute.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                int value = Com.Statistics.RandomInteger(0, 60);

                Action method = () =>
                {
                    dateTimeX.Minute = value;
                };

                ExecuteTest(method, "Minute.set(int)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.Second;
                };

                ExecuteTest(method, "Second.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                int value = Com.Statistics.RandomInteger(0, 60);

                Action method = () =>
                {
                    dateTimeX.Second = value;
                };

                ExecuteTest(method, "Second.set(int)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.Millisecond;
                };

                ExecuteTest(method, "Millisecond.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                int value = Com.Statistics.RandomInteger(0, 1000);

                Action method = () =>
                {
                    dateTimeX.Millisecond = value;
                };

                ExecuteTest(method, "Millisecond.set(int)");
            }

            // Date，TimeOfDay

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.Date;
                };

                ExecuteTest(method, "Date.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.TimeOfDay;
                };

                ExecuteTest(method, "TimeOfDay.get()");
            }

            // Of

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.WeekOfYear;
                };

                ExecuteTest(method, "WeekOfYear.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.DayOfYear;
                };

                ExecuteTest(method, "DayOfYear.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.HourOfYear;
                };

                ExecuteTest(method, "HourOfYear.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.MinuteOfYear;
                };

                ExecuteTest(method, "MinuteOfYear.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.SecondOfYear;
                };

                ExecuteTest(method, "SecondOfYear.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.MillisecondOfYear;
                };

                ExecuteTest(method, "MillisecondOfYear.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.DayOfWeek;
                };

                ExecuteTest(method, "DayOfWeek.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.HourOfDay;
                };

                ExecuteTest(method, "HourOfDay.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.MinuteOfDay;
                };

                ExecuteTest(method, "MinuteOfDay.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.SecondOfDay;
                };

                ExecuteTest(method, "SecondOfDay.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.MillisecondOfDay;
                };

                ExecuteTest(method, "MillisecondOfDay.get()");
            }

            // MonthString

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.MonthStringInChinese;
                };

                ExecuteTest(method, "MonthStringInChinese.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.MonthLongStringInEnglish;
                };

                ExecuteTest(method, "MonthLongStringInEnglish.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.MonthShortStringInEnglish;
                };

                ExecuteTest(method, "MonthShortStringInEnglish.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.MonthStringInJapaneseKanji;
                };

                ExecuteTest(method, "MonthStringInJapaneseKanji.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.MonthStringInJapaneseHiragana;
                };

                ExecuteTest(method, "MonthStringInJapaneseHiragana.get()");
            }

            // WeekdayString

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.WeekdayLongStringInChinese;
                };

                ExecuteTest(method, "WeekdayLongStringInChinese.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.WeekdayShortStringInChinese;
                };

                ExecuteTest(method, "WeekdayShortStringInChinese.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.WeekdayLongStringInEnglish;
                };

                ExecuteTest(method, "WeekdayLongStringInEnglish.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.WeekdayShortStringInEnglish;
                };

                ExecuteTest(method, "WeekdayShortStringInEnglish.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.WeekdayLongStringInJapanese;
                };

                ExecuteTest(method, "WeekdayLongStringInJapanese.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.WeekdayShortStringInJapanese;
                };

                ExecuteTest(method, "WeekdayShortStringInJapanese.get()");
            }

            // DateTimeString

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.DateLongString;
                };

                ExecuteTest(method, "DateLongString.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.DateShortString;
                };

                ExecuteTest(method, "DateShortString.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.TimeLongString;
                };

                ExecuteTest(method, "TimeLongString.get()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.TimeShortString;
                };

                ExecuteTest(method, "TimeShortString.get()");
            }
        }

        protected override void StaticProperty()
        {
            {
                Action method = () =>
                {
                    _ = Com.DateTimeX.Now;
                };

                ExecuteTest(method, "Now.get()");
            }

            {
                Action method = () =>
                {
                    _ = Com.DateTimeX.UtcNow;
                };

                ExecuteTest(method, "UtcNow.get()");
            }

            {
                Action method = () =>
                {
                    _ = Com.DateTimeX.Today;
                };

                ExecuteTest(method, "Today.get()");
            }

            {
                Action method = () =>
                {
                    _ = Com.DateTimeX.UtcToday;
                };

                ExecuteTest(method, "UtcToday.get()");
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

                ExecuteTest(method, "Equals(object)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.GetHashCode();
                };

                ExecuteTest(method, "GetHashCode()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.ToString();
                };

                ExecuteTest(method, "ToString()");
            }

            // Equals

            {
                Com.DateTimeX left = _GetRandomDateTimeX();
                Com.DateTimeX right = left;

                Action method = () =>
                {
                    _ = left.Equals(right);
                };

                ExecuteTest(method, "Equals(Com.DateTimeX)");
            }

            // CompareTo

#if ComVer1905
            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                object obj = dateTimeX;

                Action method = () =>
                {
                    _ = dateTimeX.CompareTo(obj);
                };

                ExecuteTest(method, "CompareTo(object)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.DateTimeX left = _GetRandomDateTimeX();
                Com.DateTimeX right = left;

                Action method = () =>
                {
                    _ = left.CompareTo(right);
                };

                ExecuteTest(method, "CompareTo(Com.DateTimeX)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // Add

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                TimeSpan timeSpan = TimeSpan.FromMilliseconds(Com.Statistics.RandomInteger(-999, 1000) * 365.25 * 86400000);

                Action method = () =>
                {
                    _ = dateTimeX.Add(timeSpan);
                };

                ExecuteTest(method, "Add(TimeSpan)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                long years = Com.Statistics.RandomInteger(-999, 1000);

                Action method = () =>
                {
                    _ = dateTimeX.AddYears(years);
                };

                ExecuteTest(method, "AddYears(long)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                long months = Com.Statistics.RandomInteger(-999, 1000) * 12;

                Action method = () =>
                {
                    _ = dateTimeX.AddMonths(months);
                };

                ExecuteTest(method, "AddMonths(long)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                double weeks = Com.Statistics.RandomInteger(-999, 1000) * 52.18;

                Action method = () =>
                {
                    _ = dateTimeX.AddWeeks(weeks);
                };

                ExecuteTest(method, "AddWeeks(double)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                double days = Com.Statistics.RandomInteger(-999, 1000) * 365.25;

                Action method = () =>
                {
                    _ = dateTimeX.AddDays(days);
                };

                ExecuteTest(method, "AddDays(double)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                double hours = Com.Statistics.RandomInteger(-999, 1000) * 365.25 * 24;

                Action method = () =>
                {
                    _ = dateTimeX.AddHours(hours);
                };

                ExecuteTest(method, "AddHours(double)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                double minutes = Com.Statistics.RandomInteger(-999, 1000) * 365.25 * 1440;

                Action method = () =>
                {
                    _ = dateTimeX.AddMinutes(minutes);
                };

                ExecuteTest(method, "AddMinutes(double)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                double seconds = Com.Statistics.RandomInteger(-999, 1000) * 365.25 * 86400;

                Action method = () =>
                {
                    _ = dateTimeX.AddSeconds(seconds);
                };

                ExecuteTest(method, "AddSeconds(double)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                decimal milliseconds = (decimal)(Com.Statistics.RandomInteger(-999, 1000) * 365.25 * 86400000);

                Action method = () =>
                {
                    _ = dateTimeX.AddMilliseconds(milliseconds);
                };

                ExecuteTest(method, "AddMilliseconds(decimal)");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();
                double milliseconds = Com.Statistics.RandomInteger(-999, 1000) * 365.25 * 86400000;

                Action method = () =>
                {
                    _ = dateTimeX.AddMilliseconds(milliseconds);
                };

                ExecuteTest(method, "AddMilliseconds(double)");
            }

            // To

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.ToLocalTime();
                };

                ExecuteTest(method, "ToLocalTime()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.ToUniversalTime();
                };

                ExecuteTest(method, "ToUniversalTime()");
            }

            // ToString

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.ToLongDateString();
                };

                ExecuteTest(method, "ToLongDateString()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.ToShortDateString();
                };

                ExecuteTest(method, "ToShortDateString()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.ToLongTimeString();
                };

                ExecuteTest(method, "ToLongTimeString()");
            }

            {
                Com.DateTimeX dateTimeX = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = dateTimeX.ToShortTimeString();
                };

                ExecuteTest(method, "ToShortTimeString()");
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

                ExecuteTest(method, "Equals(Com.DateTimeX, Com.DateTimeX)");
            }

            // Compare

#if ComVer1905
            {
                Com.DateTimeX left = _GetRandomDateTimeX();
                Com.DateTimeX right = left;

                Action method = () =>
                {
                    _ = Com.DateTimeX.Compare(left, right);
                };

                ExecuteTest(method, "Compare(Com.DateTimeX, Com.DateTimeX)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // IsLeapYear

            {
                long year = Com.Statistics.RandomInteger(-999999, 1000000);

                Action method = () =>
                {
                    _ = Com.DateTimeX.IsLeapYear(year);
                };

                ExecuteTest(method, "IsLeapYear(long)");
            }

            // Days

            {
                long year = Com.Statistics.RandomInteger(-999999, 1000000);

                Action method = () =>
                {
                    _ = Com.DateTimeX.DaysInYear(year);
                };

                ExecuteTest(method, "DaysInYear(long)");
            }

            {
                long year = Com.Statistics.RandomInteger(-999999, 1000000);
                int month = Com.Statistics.RandomInteger(1, 13);

                Action method = () =>
                {
                    _ = Com.DateTimeX.DaysInMonth(year, month);
                };

                ExecuteTest(method, "DaysInMonth(long, int)");
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

                ExecuteTest(method, "operator ==(Com.DateTimeX, Com.DateTimeX)");
            }

            {
                Com.DateTimeX left = _GetRandomDateTimeX();
                Com.DateTimeX right = left;

                Action method = () =>
                {
                    _ = (left != right);
                };

                ExecuteTest(method, "operator !=(Com.DateTimeX, Com.DateTimeX)");
            }

            {
                Com.DateTimeX left = _GetRandomDateTimeX();
                Com.DateTimeX right = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = (left < right);
                };

                ExecuteTest(method, "operator <(Com.DateTimeX, Com.DateTimeX)");
            }

            {
                Com.DateTimeX left = _GetRandomDateTimeX();
                Com.DateTimeX right = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = (left > right);
                };

                ExecuteTest(method, "operator >(Com.DateTimeX, Com.DateTimeX)");
            }

            {
                Com.DateTimeX left = _GetRandomDateTimeX();
                Com.DateTimeX right = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = (left <= right);
                };

                ExecuteTest(method, "operator <=(Com.DateTimeX, Com.DateTimeX)");
            }

            {
                Com.DateTimeX left = _GetRandomDateTimeX();
                Com.DateTimeX right = _GetRandomDateTimeX();

                Action method = () =>
                {
                    _ = (left >= right);
                };

                ExecuteTest(method, "operator >=(Com.DateTimeX, Com.DateTimeX)");
            }

            // 运算

            {
                Com.DateTimeX dateTime = _GetRandomDateTimeX();
                TimeSpan timeSpan = TimeSpan.FromMilliseconds(Com.Statistics.RandomInteger(-999, 1000) * 365.25 * 86400000);

                Action method = () =>
                {
                    _ = dateTime - timeSpan;
                };

                ExecuteTest(method, "operator +(Com.DateTimeX, TimeSpan)");
            }

            {
                Com.DateTimeX dateTime = _GetRandomDateTimeX();
                TimeSpan timeSpan = TimeSpan.FromMilliseconds(Com.Statistics.RandomInteger(-999, 1000) * 365.25 * 86400000);

                Action method = () =>
                {
                    _ = dateTime - timeSpan;
                };

                ExecuteTest(method, "operator -(Com.DateTimeX, TimeSpan)");
            }

            // 类型转换

#if ComVer1905
            {
                DateTime dateTime = _GetRandomDateTime();

                Action method = () =>
                {
                    _ = (Com.DateTimeX)dateTime;
                };

                ExecuteTest(method, "implicit operator Com.DateTimeX(System.DateTime)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif
        }
    }

    sealed class GeometryTest : ClassPerfTestBase
    {
        private const string _NamespaceName = "Com";
        private const string _ClassName = "Geometry";

        private void ExecuteTest(Action method, string methodName, string comment)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName, comment);
        }

        private void ExecuteTest(Action method, string methodName)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(string methodName, UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName, unsupportedReason);
        }

        private void ExecuteTest(string methodName)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, string.Empty, unsupportedReason);
        }

        //

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

#if ComVer1905
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

                ExecuteTest(method, "CalcLineGeneralFunction(Com.PointD, Com.PointD, out double, out double, out double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD pt = _GetRandomPointD();
                double A = Com.Statistics.RandomDouble(-1E9, 1E9);
                double B = Com.Statistics.RandomDouble(-1E9, 1E9);
                double C = Com.Statistics.RandomDouble(-1E9, 1E9);

                Action method = () =>
                {
                    _ = Com.Geometry.GetDistanceBetweenPointAndLine(pt, A, B, C);
                };

                ExecuteTest(method, "GetDistanceBetweenPointAndLine(Com.PointD, double, double, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD pt = _GetRandomPointD();
                Com.PointD pt1 = _GetRandomPointD();
                Com.PointD pt2 = _GetRandomPointD();

                Action method = () =>
                {
                    _ = Com.Geometry.GetDistanceBetweenPointAndLine(pt, pt1, pt2);
                };

                ExecuteTest(method, "GetDistanceBetweenPointAndLine(Com.PointD, Com.PointD, Com.PointD)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            {
                Com.PointD pt = _GetRandomPointD();
                Com.PointD pt1 = _GetRandomPointD();
                Com.PointD pt2 = _GetRandomPointD();

                Action method = () =>
                {
                    _ = Com.Geometry.GetFootPoint(pt, pt1, pt2);
                };

                ExecuteTest(method, "GetFootPoint(Com.PointD, Com.PointD, Com.PointD)");
            }

            // 角度

            {
                Com.PointD pt1 = _GetRandomPointD();
                Com.PointD pt2 = _GetRandomPointD();

                Action method = () =>
                {
                    _ = Com.Geometry.GetAngleOfTwoPoints(pt1, pt2);
                };

                ExecuteTest(method, "GetAngleOfTwoPoints(Com.PointD, Com.PointD)");
            }

#if ComVer1910
            {
                double angle = Com.Statistics.RandomDouble(-1E9, 1E9);

                Action method = () =>
                {
                    _ = Com.Geometry.AngleMapping(angle, true, true);
                };

                ExecuteTest(method, "AngleMapping(double, bool, bool)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

#if ComVer1910
            {
                double angle = Com.Statistics.RandomDouble(-1E9, 1E9);

                Action method = () =>
                {
                    _ = Com.Geometry.AngleMapping(angle, true);
                };

                ExecuteTest(method, "AngleMapping(double, bool)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

            {
                double angle = Com.Statistics.RandomDouble(-1E9, 1E9);

                Action method = () =>
                {
                    _ = Com.Geometry.AngleMapping(angle);
                };

                ExecuteTest(method, "AngleMapping(double)");
            }

#if ComVer1910
            {
                double angle = Com.Statistics.RandomDouble(-1E9, 1E9);

                Action method = () =>
                {
                    _ = Com.Geometry.RadianToDegree(angle);
                };

                ExecuteTest(method, "RadianToDegree(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

#if ComVer1910
            {
                double angle = Com.Statistics.RandomDouble(-1E9, 1E9);

                Action method = () =>
                {
                    _ = Com.Geometry.DegreeToRadian(angle);
                };

                ExecuteTest(method, "DegreeToRadian(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

            // 控件

            ExecuteTest("GetCursorPositionOfControl(System.Windows.Forms.Control)");

            ExecuteTest("CursorIsInControl(System.Windows.Forms.Control)");

            ExecuteTest("PointIsInControl(System.Drawing.Point, System.Windows.Forms.Control)");

            ExecuteTest("ScreenPointIsInControl(System.Drawing.Point, System.Windows.Forms.Control)");

            ExecuteTest("GetMinimumBoundingRectangleOfControls(System.Windows.Forms.Control[], int)");

            ExecuteTest("GetMinimumBoundingRectangleOfControls(System.Windows.Forms.Control[])");

            // 图形可见性

            {
                Com.PointD pt = _GetRandomPointD();
                RectangleF rect = _GetRandomRectangleF();

                Action method = () =>
                {
                    _ = Com.Geometry.PointIsVisibleInRectangle(pt, rect);
                };

                ExecuteTest(method, "PointIsVisibleInRectangle(Com.PointD, System.Drawing.RectangleF)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                Com.PointD offset = _GetRandomPointD();
                double radius = Com.Statistics.RandomDouble(1E9);

                Action method = () =>
                {
                    _ = Com.Geometry.PointIsVisibleInCircle(pt, offset, radius);
                };

                ExecuteTest(method, "PointIsVisibleInCircle(Com.PointD, Com.PointD, double)");
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

                ExecuteTest(method, "PointIsVisibleInEllipse(Com.PointD, Com.PointD, double, double, double)");
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

                ExecuteTest(method, "PointIsVisibleInRhombus(Com.PointD, Com.PointD, double, double, double)");
            }

            {
                Com.PointD pt1 = _GetRandomPointD();
                Com.PointD pt2 = _GetRandomPointD();
                RectangleF rect = _GetRandomRectangleF();

                Action method = () =>
                {
                    _ = Com.Geometry.LineIsVisibleInRectangle(pt1, pt2, rect);
                };

                ExecuteTest(method, "LineIsVisibleInRectangle(Com.PointD, Com.PointD, System.Drawing.RectangleF)");
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

                ExecuteTest(method, "LineIsVisibleInCircle(Com.PointD, Com.PointD, Com.PointD, double)");
            }

            {
                Com.PointD offset = _GetRandomPointD();
                double radius = Com.Statistics.RandomDouble(1E9);
                RectangleF rect = _GetRandomRectangleF();

                Action method = () =>
                {
                    _ = Com.Geometry.CircleInnerIsVisibleInRectangle(offset, radius, rect);
                };

                ExecuteTest(method, "CircleInnerIsVisibleInRectangle(Com.PointD, double, System.Drawing.RectangleF)");
            }

            {
                Com.PointD offset = _GetRandomPointD();
                double radius = Com.Statistics.RandomDouble(1E9);
                RectangleF rect = _GetRandomRectangleF();

                Action method = () =>
                {
                    _ = Com.Geometry.CircumferenceIsVisibleInRectangle(offset, radius, rect);
                };

                ExecuteTest(method, "CircumferenceIsVisibleInRectangle(Com.PointD, double, System.Drawing.RectangleF)");
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

                ExecuteTest(method, "GetRadiusOfEllipse(double, double, double)");
            }

            {
                double semiMajorAxis = Com.Statistics.RandomDouble(1E9);
                double eccentricity = Com.Statistics.RandomDouble();
                double phase = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.Geometry.GetFocalRadiusOfEllipse(semiMajorAxis, eccentricity, phase);
                };

                ExecuteTest(method, "GetFocalRadiusOfEllipse(double, double, double)");
            }

            {
                double centralAngle = Com.Statistics.RandomDouble(2 * Math.PI);
                double eccentricity = Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    _ = Com.Geometry.EllipseCentralAngleToPhase(centralAngle, eccentricity);
                };

                ExecuteTest(method, "EllipseCentralAngleToPhase(double, double, double)");
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

                ExecuteTest(method, "RotateBitmap(System.Drawing.Bitmap, double, bool)", "bmp at 1024x1024 pixels, enable antiAlias");
            }

            // 路径

            {
                Rectangle rect = _GetRandomRectangle();
                int cornerRadius = Com.Statistics.RandomInteger(32768);

                Action method = () =>
                {
                    _ = Com.Geometry.CreateRoundedRectanglePath(rect, cornerRadius);
                };

                ExecuteTest(method, "CreateRoundedRectanglePath(System.Drawing.Rectangle, int)");
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

                ExecuteTest(method, "CreateRoundedRectanglePath(System.Drawing.Rectangle, int, int, int, int)");
            }

            {
                Rectangle rect = _GetRandomRectangle();
                int cornerRadius = Com.Statistics.RandomInteger(32768);

                Action method = () =>
                {
                    _ = Com.Geometry.CreateRoundedRectangleOuterPaths(rect, cornerRadius);
                };

                ExecuteTest(method, "CreateRoundedRectangleOuterPaths(System.Drawing.Rectangle, int)");
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

                ExecuteTest(method, "CreateRoundedRectangleOuterPaths(System.Drawing.Rectangle, int, int, int, int)");
            }
        }

        protected override void Operator()
        {

        }
    }

    sealed class IOTest : ClassPerfTestBase
    {
        private const string _NamespaceName = "Com";
        private const string _ClassName = "IO";

        private void ExecuteTest(Action method, string methodName, string comment)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName, comment);
        }

        private void ExecuteTest(Action method, string methodName)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(string methodName, UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName, unsupportedReason);
        }

        private void ExecuteTest(string methodName)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, string.Empty, unsupportedReason);
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
            ExecuteTest("CopyFolder(string, string, bool, bool, bool)");

            ExecuteTest("CopyFolder(string, string, bool, bool)");

            ExecuteTest("CopyFolder(string, string, bool)");

            ExecuteTest("CopyFolder(string, string)");
        }

        protected override void Operator()
        {

        }
    }

    sealed class MatrixTest : ClassPerfTestBase
    {
        private const string _NamespaceName = "Com";
        private const string _ClassName = "Matrix";

        private void ExecuteTest(Action method, string methodName, string comment)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName, comment);
        }

        private void ExecuteTest(Action method, string methodName)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(string methodName, UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName, unsupportedReason);
        }

        private void ExecuteTest(string methodName)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, string.Empty, unsupportedReason);
        }

        //

        private static Com.Matrix _GetRandomMatrix(int width, int height)
        {
            if (width > 0 && height > 0)
            {
                Com.Matrix matrix = new Com.Matrix(width, height);

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        matrix[x, y] = Com.Statistics.RandomDouble(-10, 10);
                    }
                }

                return matrix;
            }
            else
            {
#if ComVer1905
                return Com.Matrix.Empty;
#else
                return Com.Matrix.NonMatrix;
#endif
            }
        }

        private static Com.Vector _GetRandomVector(Com.Vector.Type type, int dimension)
        {
            if (dimension > 0)
            {
                Com.Vector vector = Com.Vector.Zero(type, dimension);

                for (int i = 0; i < dimension; i++)
                {
                    vector[i] = Com.Statistics.RandomDouble(-1000, 1000);
                }

                return vector;
            }
            else
            {
#if ComVer1905
                return Com.Vector.Empty;
#else
                return Com.Vector.NonVector;
#endif
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

                ExecuteTest(method, "Matrix(System.Drawing.Size)", "size at 32x32");
            }

            {
                Size size = new Size(32, 32);
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = new Com.Matrix(size, value);
                };

                ExecuteTest(method, "Matrix(System.Drawing.Size, double)", "size at 32x32");
            }

            {
                int width = 32;
                int height = 32;

                Action method = () =>
                {
                    _ = new Com.Matrix(width, height);
                };

                ExecuteTest(method, "Matrix(int, int)", "size at 32x32");
            }

            {
                int width = 32;
                int height = 32;
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = new Com.Matrix(width, height, value);
                };

                ExecuteTest(method, "Matrix(int, int, double)", "size at 32x32");
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

                ExecuteTest(method, "Matrix(double[,])", "size at 32x32");
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

                ExecuteTest(method, "this[int, int].get()", "size at 32x32");
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

                ExecuteTest(method, "this[int, int].set(double)", "size at 32x32");
            }

            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);
                Point index = new Point(Com.Statistics.RandomInteger(32), Com.Statistics.RandomInteger(32));

                Action method = () =>
                {
                    _ = matrix[index];
                };

                ExecuteTest(method, "this[System.Drawing.Point].get()", "size at 32x32");
            }

            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);
                Point index = new Point(Com.Statistics.RandomInteger(32), Com.Statistics.RandomInteger(32));
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    matrix[index] = value;
                };

                ExecuteTest(method, "this[System.Drawing.Point].set(double)", "size at 32x32");
            }

            // Is

#if ComVer1905
            {
                Com.Matrix matrix = new Com.Matrix(32, 32);

                Action method = () =>
                {
                    _ = matrix.IsEmpty;
                };

                ExecuteTest(method, "IsEmpty.get()", "size at 32x32");
            }
#else
            {
                Com.Matrix matrix = new Com.Matrix(32, 32);

                Action method = () =>
                {
                    _ = matrix.IsNonMatrix;
                };

                ExecuteTest(method, "IsNonMatrix.get()", "size at 32x32");
            }
#endif

            // Size

            {
                Com.Matrix matrix = new Com.Matrix(32, 32);

                Action method = () =>
                {
                    _ = matrix.Size;
                };

                ExecuteTest(method, "Size.get()", "size at 32x32");
            }

            {
                Com.Matrix matrix = new Com.Matrix(32, 32);

                Action method = () =>
                {
                    _ = matrix.Width;
                };

                ExecuteTest(method, "Width.get()", "size at 32x32");
            }

            {
                Com.Matrix matrix = new Com.Matrix(32, 32);

                Action method = () =>
                {
                    _ = matrix.Column;
                };

                ExecuteTest(method, "Column.get()", "size at 32x32");
            }

            {
                Com.Matrix matrix = new Com.Matrix(32, 32);

                Action method = () =>
                {
                    _ = matrix.Height;
                };

                ExecuteTest(method, "Height.get()", "size at 32x32");
            }

            {
                Com.Matrix matrix = new Com.Matrix(32, 32);

                Action method = () =>
                {
                    _ = matrix.Row;
                };

                ExecuteTest(method, "Row.get()", "size at 32x32");
            }

            {
                Com.Matrix matrix = new Com.Matrix(32, 32);

                Action method = () =>
                {
                    _ = matrix.Count;
                };

                ExecuteTest(method, "Count.get()", "size at 32x32");
            }

            // 线性代数属性

            {
                Com.Matrix matrix = _GetRandomMatrix(8, 8);

                Action method = () =>
                {
                    _ = matrix.Determinant;
                };

                ExecuteTest(method, "Determinant.get()", "size at 8x8");
            }

            {
                Com.Matrix matrix = _GetRandomMatrix(8, 8);

                Action method = () =>
                {
                    _ = matrix.Rank;
                };

                ExecuteTest(method, "Rank.get()", "size at 8x8");
            }

            {
                Com.Matrix matrix = _GetRandomMatrix(8, 8);

                Action method = () =>
                {
                    _ = matrix.Transport;
                };

                ExecuteTest(method, "Transport.get()", "size at 8x8");
            }

            {
                Com.Matrix matrix = _GetRandomMatrix(8, 8);

                Action method = () =>
                {
                    _ = matrix.Adjoint;
                };

                ExecuteTest(method, "Adjoint.get()", "size at 8x8");
            }

            {
                Com.Matrix matrix = _GetRandomMatrix(8, 8);

                Action method = () =>
                {
                    _ = matrix.Invert;
                };

                ExecuteTest(method, "Invert.get()", "size at 8x8");
            }
        }

        protected override void StaticProperty()
        {
            // Empty

#if ComVer1905
            {
                Action method = () =>
                {
                    _ = Com.Matrix.Empty;
                };

                ExecuteTest(method, "Empty.get()");
            }
#else
            {
                Action method = () =>
                {
                    _ = Com.Matrix.NonMatrix;
                };

                ExecuteTest(method, "NonMatrix.get()");
            }
#endif
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

                ExecuteTest(method, "Equals(object)", "size at 32x32");
            }

            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);

                Action method = () =>
                {
                    _ = matrix.GetHashCode();
                };

                ExecuteTest(method, "GetHashCode()", "size at 32x32");
            }

            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);

                Action method = () =>
                {
                    _ = matrix.ToString();
                };

                ExecuteTest(method, "ToString()", "size at 32x32");
            }

            // Equals

            {
                Com.Matrix left = _GetRandomMatrix(32, 32);
                Com.Matrix right = left.Copy();

                Action method = () =>
                {
                    _ = left.Equals(right);
                };

                ExecuteTest(method, "Equals(Com.Matrix)", "size at 32x32");
            }

            // Copy

            {
                Com.Matrix matrix = new Com.Matrix(32, 32);

                Action method = () =>
                {
                    _ = matrix.Copy();
                };

                ExecuteTest(method, "Copy()", "size at 32x32");
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

                ExecuteTest(method, "SubMatrix(System.Drawing.Point, System.Drawing.Size)", "size at 32x32, SubMatrix is 16x16 at (8,8)");
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

                ExecuteTest(method, "SubMatrix(int, int, int, int)", "size at 32x32, SubMatrix is 16x16 at (8,8)");
            }

            // get/set行列

            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);
                int x = Com.Statistics.RandomInteger(32);

                Action method = () =>
                {
                    _ = matrix.GetColumn(x);
                };

                ExecuteTest(method, "GetColumn(int)", "size at 32x32");
            }

#if ComVerNext
            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);
                int x = Com.Statistics.RandomInteger(32);
                Com.Vector vector = _GetRandomVector(Com.Vector.Type.ColumnVector, 32);

                Action method = () =>
                {
                    matrix.SetColumn(x, vector);
                };

                ExecuteTest(method, "SetColumn(int, Com.Vector)", "size at 32x32");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);
                int y = Com.Statistics.RandomInteger(32);

                Action method = () =>
                {
                    _ = matrix.GetRow(y);
                };

                ExecuteTest(method, "GetRow(int)", "size at 32x32");
            }

#if ComVerNext
            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);
                int x = Com.Statistics.RandomInteger(32);
                Com.Vector vector = _GetRandomVector(Com.Vector.Type.RowVector, 32);

                Action method = () =>
                {
                    matrix.SetRow(x, vector);
                };

                ExecuteTest(method, "SetRow(int, Com.Vector)", "size at 32x32");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            // ToArray

            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);

                Action method = () =>
                {
                    _ = matrix.ToArray();
                };

                ExecuteTest(method, "ToArray()", "size at 32x32");
            }
        }

        protected override void StaticMethod()
        {
            // IsNullOrEmpty

#if ComVer1905
            {
                Com.Matrix matrix = new Com.Matrix(32, 32);

                Action method = () =>
                {
                    _ = Com.Matrix.IsNullOrEmpty(matrix);
                };

                ExecuteTest(method, "IsNullOrEmpty(Com.Matrix)", "size at 32x32");
            }
#else
            {
                Com.Matrix matrix = new Com.Matrix(32, 32);

                Action method = () =>
                {
                    _ = Com.Matrix.IsNullOrNonMatrix(matrix);
                };

                ExecuteTest(method, "IsNullOrNonMatrix(Com.Matrix)", "size at 32x32");
            }
#endif

            // Equals

            {
                Com.Matrix left = _GetRandomMatrix(32, 32);
                Com.Matrix right = left.Copy();

                Action method = () =>
                {
                    _ = Com.Matrix.Equals(left, right);
                };

                ExecuteTest(method, "Equals(Com.Matrix, Com.Matrix)", "size at 32x32");
            }

            // 矩阵生成

            {
                int order = 32;

                Action method = () =>
                {
                    _ = Com.Matrix.Identity(order);
                };

                ExecuteTest(method, "Identity(int)", "size at 32x32");
            }

            {
                Size size = new Size(32, 32);

                Action method = () =>
                {
                    _ = Com.Matrix.Zeros(size);
                };

                ExecuteTest(method, "Zeros(System.Drawing.Size)", "size at 32x32");
            }

            {
                int width = 32;
                int height = 32;

                Action method = () =>
                {
                    _ = Com.Matrix.Zeros(width, height);
                };

                ExecuteTest(method, "Zeros(int, int)", "size at 32x32");
            }

            {
                Size size = new Size(32, 32);

                Action method = () =>
                {
                    _ = Com.Matrix.Ones(size);
                };

                ExecuteTest(method, "Ones(System.Drawing.Size)", "size at 32x32");
            }

            {
                int width = 32;
                int height = 32;

                Action method = () =>
                {
                    _ = Com.Matrix.Ones(width, height);
                };

                ExecuteTest(method, "Ones(int, int)", "size at 32x32");
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

                ExecuteTest(method, "Diagonal(double[], int)", "size at 32x32");
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

                ExecuteTest(method, "Diagonal(double[])", "size at 32x32");
            }

            // 增广矩阵

            {
                Com.Matrix left = _GetRandomMatrix(32, 32);
                Com.Matrix right = _GetRandomMatrix(32, 32);

                Action method = () =>
                {
                    _ = Com.Matrix.Augment(left, right);
                };

                ExecuteTest(method, "Augment(Com.Matrix, Com.Matrix)", "size at 32x32");
            }

            // 线性代数运算

            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.Matrix.Add(matrix, n);
                };

                ExecuteTest(method, "Add(Com.Matrix, double)", "size at 32x32");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.Matrix matrix = _GetRandomMatrix(32, 32);

                Action method = () =>
                {
                    _ = Com.Matrix.Add(n, matrix);
                };

                ExecuteTest(method, "Add(double, Com.Matrix)", "size at 32x32");
            }

            {
                Com.Matrix left = _GetRandomMatrix(32, 32);
                Com.Matrix right = _GetRandomMatrix(32, 32);

                Action method = () =>
                {
                    _ = Com.Matrix.Add(left, right);
                };

                ExecuteTest(method, "Add(Com.Matrix, Com.Matrix)", "size at 32x32");
            }

            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.Matrix.Subtract(matrix, n);
                };

                ExecuteTest(method, "Subtract(Com.Matrix, double)", "size at 32x32");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.Matrix matrix = _GetRandomMatrix(32, 32);

                Action method = () =>
                {
                    _ = Com.Matrix.Subtract(n, matrix);
                };

                ExecuteTest(method, "Subtract(double, Com.Matrix)", "size at 32x32");
            }

            {
                Com.Matrix left = _GetRandomMatrix(32, 32);
                Com.Matrix right = _GetRandomMatrix(32, 32);

                Action method = () =>
                {
                    _ = Com.Matrix.Subtract(left, right);
                };

                ExecuteTest(method, "Subtract(Com.Matrix, Com.Matrix)", "size at 32x32");
            }

            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.Matrix.Multiply(matrix, n);
                };

                ExecuteTest(method, "Multiply(Com.Matrix, double)", "size at 32x32");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.Matrix matrix = _GetRandomMatrix(32, 32);

                Action method = () =>
                {
                    _ = Com.Matrix.Multiply(n, matrix);
                };

                ExecuteTest(method, "Multiply(double, Com.Matrix)", "size at 32x32");
            }

            {
                Com.Matrix left = _GetRandomMatrix(32, 32);
                Com.Matrix right = _GetRandomMatrix(32, 32);

                Action method = () =>
                {
                    _ = Com.Matrix.Multiply(left, right);
                };

                ExecuteTest(method, "Multiply(Com.Matrix, Com.Matrix)", "size at 32x32");
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

                ExecuteTest(method, "MultiplyLeft(System.Collections.Generic.List<Com.Matrix>)", "size at 32x32, total 8 matrices");
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

                ExecuteTest(method, "MultiplyRight(System.Collections.Generic.List<Com.Matrix>)", "size at 32x32, total 8 matrices");
            }

            {
                Com.Matrix matrix = _GetRandomMatrix(32, 32);
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.Matrix.Divide(matrix, n);
                };

                ExecuteTest(method, "Divide(Com.Matrix, double)", "size at 32x32");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.Matrix matrix = _GetRandomMatrix(32, 32);

                Action method = () =>
                {
                    _ = Com.Matrix.Divide(n, matrix);
                };

                ExecuteTest(method, "Divide(double, Com.Matrix)", "size at 32x32");
            }

            {
                Com.Matrix left = _GetRandomMatrix(8, 8);
                Com.Matrix right = _GetRandomMatrix(8, 8);

                Action method = () =>
                {
                    _ = Com.Matrix.DivideLeft(left, right);
                };

                ExecuteTest(method, "DivideLeft(Com.Matrix, Com.Matrix)", "size at 8x8");
            }

            {
                Com.Matrix left = _GetRandomMatrix(8, 8);
                Com.Matrix right = _GetRandomMatrix(8, 8);

                Action method = () =>
                {
                    _ = Com.Matrix.DivideRight(left, right);
                };

                ExecuteTest(method, "DivideRight(Com.Matrix, Com.Matrix)", "size at 8x8");
            }

            // 求解线性方程组

            {
                Com.Matrix matrix = _GetRandomMatrix(8, 8);
                Com.Vector vector = _GetRandomVector(Com.Vector.Type.ColumnVector, 8);

                Action method = () =>
                {
                    _ = Com.Matrix.SolveLinearEquation(matrix, vector);
                };

                ExecuteTest(method, "SolveLinearEquation(Com.Matrix, Com.Vector)", "size at 8x8");
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

                ExecuteTest(method, "operator ==(Com.Matrix, Com.Matrix)", "size at 32x32");
            }

            {
                Com.Matrix left = _GetRandomMatrix(32, 32);
                Com.Matrix right = left.Copy();

                Action method = () =>
                {
                    _ = (left != right);
                };

                ExecuteTest(method, "operator !=(Com.Matrix, Com.Matrix)", "size at 32x32");
            }
        }
    }

    sealed class Painting2DTest : ClassPerfTestBase
    {
        private const string _NamespaceName = "Com";
        private const string _ClassName = "Painting2D";

        private void ExecuteTest(Action method, string methodName, string comment)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName, comment);
        }

        private void ExecuteTest(Action method, string methodName)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(string methodName, UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName, unsupportedReason);
        }

        private void ExecuteTest(string methodName)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, string.Empty, unsupportedReason);
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

                ExecuteTest(method, "PaintLine(System.Drawing.Bitmap, Com.PointD, Com.PointD, System.Drawing.Color, float, bool)", "bmp at 1024x1024 pixels, width at 1.0F, enable antiAlias");
            }

            {
                Bitmap bmp = new Bitmap(1024, 1024);
                Com.PointD offset = new Com.PointD(bmp.Size) / 2;
#if ComVer1905
                double radius = new Com.PointD(bmp.Size).Module / 2;
#else
                double radius = new Com.PointD(bmp.Size).VectorModule / 2;
#endif
                double deltaRadius = radius / 9;
                int normalIncreasePeriod = 3;
                Color color = Com.ColorManipulation.GetRandomColor();
                bool antiAlias = true;

                Action method = () =>
                {
                    _ = Com.Painting2D.PaintPolarGrid(bmp, offset, radius, deltaRadius, normalIncreasePeriod, color, antiAlias);
                };

                ExecuteTest(method, "PaintPolarGrid(System.Drawing.Bitmap, Com.PointD, double, double, int, System.Drawing.Color, bool)", "bmp at 1024x1024 pixels, radius at half diagonal of bmp, deltaRadius at 1/9 of radius, normalIncreasePeriod at 3, enable antiAlias");
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

                ExecuteTest(method, "PaintCircle(System.Drawing.Bitmap, Com.PointD, double, System.Drawing.Color, float, bool)", "bmp at 1024x1024 pixels, radius at half width of bmp, width at 1.0F, enable antiAlias");
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

                ExecuteTest(method, "PaintLargeCircle(System.Drawing.Bitmap, Com.PointD, double, double, System.Drawing.Color, float, bool, int, int, double)", "bmp at 1024x1024 pixels, radius at sqrt(5) width of bmp, width at 1.0F, enable antiAlias, minDiv at 32, maxDiv at 256, divArc at 4");
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

                ExecuteTest(method, "PaintLargeCircle(System.Drawing.Bitmap, Com.PointD, double, double, System.Drawing.Color, float, bool, int, int, double)", "bmp at 1024x1024 pixels, radius at sqrt(5) width of bmp, width at 1.0F, enable antiAlias");
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

                ExecuteTest(method, "PaintLargeEllipse(System.Drawing.Bitmap, Com.PointD, double, double, double, double, System.Drawing.Color, float, bool, int, int, double)", "bmp at 1024x1024 pixels, semiMajorAxis at 5 width of bmp, eccentricity at 0.8, width at 1.0F, enable antiAlias, minDiv at 32, maxDiv at 256, divArc at 4");
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

                ExecuteTest(method, "PaintLargeEllipse(System.Drawing.Bitmap, Com.PointD, double, double, double, double, System.Drawing.Color, float, bool)", "bmp at 1024x1024 pixels, semiMajorAxis at 5 width of bmp, eccentricity at 0.8, width at 1.0F, enable antiAlias");
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

                ExecuteTest(method, "PaintTextWithShadow(System.Drawing.Bitmap, string, System.Drawing.Font, System.Drawing.Color, System.Drawing.Color, System.Drawing.PointF, float, bool)", "bmp at 1024x1024 pixels, font at 42 pt, enable antiAlias");
            }

            ExecuteTest("PaintImageOnTransparentForm(System.Windows.Forms.Form, System.Drawing.Bitmap, double)");
        }

        protected override void Operator()
        {

        }
    }

    sealed class Painting3DTest : ClassPerfTestBase
    {
        private const string _NamespaceName = "Com";
        private const string _ClassName = "Painting3D";

        private void ExecuteTest(Action method, string methodName, string comment)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName, comment);
        }

        private void ExecuteTest(Action method, string methodName)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(string methodName, UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName, unsupportedReason);
        }

        private void ExecuteTest(string methodName)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, string.Empty, unsupportedReason);
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
            {
                Bitmap bmp = new Bitmap(1024, 1024);
                Com.PointD3D center = new Com.PointD3D(bmp.Width / 2, bmp.Height / 2, 2048);
                Com.PointD3D size = new Com.PointD3D(512, 512, 512);
                Color color = Com.ColorManipulation.GetRandomColorX().AtAlpha(128).ToColor();
                float edgeWidth = 1.0F;
                List<Com.Matrix> affineMatrixList = new List<Com.Matrix>(4) { Com.PointD3D.IdentityMatrix(), Com.PointD3D.IdentityMatrix(), Com.PointD3D.IdentityMatrix(), Com.PointD3D.IdentityMatrix() };
                double focalLength = 512;
                Com.PointD3D illuminationDirection = new Com.PointD3D(1, 1, 1);
                bool illuminationDirectionIsAfterAffineTransform = false;
                double exposure = 0;
                bool antiAlias = true;

                Action method = () =>
                {
                    _ = Com.Painting3D.PaintCuboid(bmp, center, size, color, edgeWidth, affineMatrixList, focalLength, illuminationDirection, illuminationDirectionIsAfterAffineTransform, exposure, antiAlias);
                };

                ExecuteTest(method, "PaintCuboid(System.Drawing.Bitmap, Com.PointD3D, Com.PointD3D, System.Drawing.Color, float, System.Collections.Generic.List<Com.Matrix>, double, Com.PointD3D, bool, double, bool)", "bmp at 1024x1024 pixels, cuboid size at 512x512x512, edgeWidth at 1.0F, total 4 matrices, focalLength at 1024, enable antiAlias");
            }

            {
                Bitmap bmp = new Bitmap(1024, 1024);
                Com.PointD3D center = new Com.PointD3D(bmp.Width / 2, bmp.Height / 2, 2048);
                Com.PointD3D size = new Com.PointD3D(512, 512, 512);
                Color color = Com.ColorManipulation.GetRandomColorX().AtAlpha(128).ToColor();
                float edgeWidth = 1.0F;
                Com.Matrix affineMatrix = Com.PointD3D.IdentityMatrix();
                double focalLength = 512;
                Com.PointD3D illuminationDirection = new Com.PointD3D(1, 1, 1);
                bool illuminationDirectionIsAfterAffineTransform = false;
                double exposure = 0;
                bool antiAlias = true;

                Action method = () =>
                {
                    _ = Com.Painting3D.PaintCuboid(bmp, center, size, color, edgeWidth, affineMatrix, focalLength, illuminationDirection, illuminationDirectionIsAfterAffineTransform, exposure, antiAlias);
                };

                ExecuteTest(method, "PaintCuboid(System.Drawing.Bitmap, Com.PointD3D, Com.PointD3D, System.Drawing.Color, float, Com.Matrix, double, Com.PointD3D, bool, double, bool)", "bmp at 1024x1024 pixels, cuboid size at 512x512x512, edgeWidth at 1.0F, focalLength at 1024, enable antiAlias");
            }

            {
                Bitmap bmp = new Bitmap(1024, 1024);
                Com.PointD3D center = new Com.PointD3D(bmp.Width / 2, bmp.Height / 2, 2048);
                Com.PointD3D size = new Com.PointD3D(512, 512, 512);
                Color color = Com.ColorManipulation.GetRandomColorX().AtAlpha(128).ToColor();
                float edgeWidth = 1.0F;
                List<Com.Matrix> affineMatrixList = new List<Com.Matrix>(4) { Com.PointD3D.IdentityMatrix(), Com.PointD3D.IdentityMatrix(), Com.PointD3D.IdentityMatrix(), Com.PointD3D.IdentityMatrix() };
                double focalLength = 512;
                bool antiAlias = true;

                Action method = () =>
                {
                    _ = Com.Painting3D.PaintCuboid(bmp, center, size, color, edgeWidth, affineMatrixList, focalLength, antiAlias);
                };

                ExecuteTest(method, "PaintCuboid(System.Drawing.Bitmap, Com.PointD3D, Com.PointD3D, System.Drawing.Color, float, System.Collections.Generic.List<Com.Matrix>, double, bool)", "bmp at 1024x1024 pixels, cuboid size at 512x512x512, edgeWidth at 1.0F, total 4 matrices, focalLength at 1024, enable antiAlias");
            }

            {
                Bitmap bmp = new Bitmap(1024, 1024);
                Com.PointD3D center = new Com.PointD3D(bmp.Width / 2, bmp.Height / 2, 2048);
                Com.PointD3D size = new Com.PointD3D(512, 512, 512);
                Color color = Com.ColorManipulation.GetRandomColorX().AtAlpha(128).ToColor();
                float edgeWidth = 1.0F;
                Com.Matrix affineMatrix = Com.PointD3D.IdentityMatrix();
                double focalLength = 512;
                bool antiAlias = true;

                Action method = () =>
                {
                    _ = Com.Painting3D.PaintCuboid(bmp, center, size, color, edgeWidth, affineMatrix, focalLength, antiAlias);
                };

                ExecuteTest(method, "PaintCuboid(System.Drawing.Bitmap, Com.PointD3D, Com.PointD3D, System.Drawing.Color, float, Com.Matrix, double, bool)", "bmp at 1024x1024 pixels, cuboid size at 512x512x512, edgeWidth at 1.0F, focalLength at 1024, enable antiAlias");
            }
        }

        protected override void Operator()
        {

        }
    }

    sealed class PointDTest : ClassPerfTestBase
    {
        private const string _NamespaceName = "Com";
        private const string _ClassName = "PointD";

        private void ExecuteTest(Action method, string methodName, string comment)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName, comment);
        }

        private void ExecuteTest(Action method, string methodName)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(string methodName, UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName, unsupportedReason);
        }

        private void ExecuteTest(string methodName)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, string.Empty, unsupportedReason);
        }

        //

        private static Com.PointD _GetRandomPointD()
        {
            return new Com.PointD(Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18));
        }

        private static Com.PointD _GetRandomNormalPointD()
        {
#if ComVer1905
            return _GetRandomPointD().Normalize;
#else
            return _GetRandomPointD().VectorNormalize;
#endif
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
                        matrix[x, y] = Com.Statistics.RandomDouble(-10, 10);
                    }
                }

                return matrix;
            }
            else
            {
#if ComVer1905
                return Com.Matrix.Empty;
#else
                return Com.Matrix.NonMatrix;
#endif
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

                ExecuteTest(method, "PointD(double, double)");
            }

            {
                Point pt = new Point(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = new Com.PointD(pt);
                };

                ExecuteTest(method, "PointD(System.Drawing.Point)");
            }

            {
                PointF pt = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = new Com.PointD(pt);
                };

                ExecuteTest(method, "PointD(System.Drawing.PointF)");
            }

            {
                Size sz = new Size(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = new Com.PointD(sz);
                };

                ExecuteTest(method, "PointD(System.Drawing.Size)");
            }

            {
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = new Com.PointD(sz);
                };

                ExecuteTest(method, "PointD(System.Drawing.SizeF)");
            }

            {
                Com.Complex comp = new Com.Complex(Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = new Com.PointD(comp);
                };

                ExecuteTest(method, "PointD(Com.Complex)");
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

                ExecuteTest(method, "this[int].get()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                int index = Com.Statistics.RandomInteger(2);
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD[index] = value;
                };

                ExecuteTest(method, "this[int].set(double)");
            }

            // 分量

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.X;
                };

                ExecuteTest(method, "X.get()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD.X = value;
                };

                ExecuteTest(method, "X.set(double)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.Y;
                };

                ExecuteTest(method, "Y.get()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD.Y = value;
                };

                ExecuteTest(method, "Y.set(double)");
            }

            // Dimension

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.Dimension;
                };

                ExecuteTest(method, "Dimension.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // Is

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.IsEmpty;
                };

                ExecuteTest(method, "IsEmpty.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.IsZero;
                };

                ExecuteTest(method, "IsZero.get()");
            }
#else
            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.IsEmpty;
                };

                ExecuteTest(method, "IsEmpty.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.IsReadOnly;
                };

                ExecuteTest(method, "IsReadOnly.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.IsFixedSize;
                };

                ExecuteTest(method, "IsFixedSize.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.IsNaN;
                };

                ExecuteTest(method, "IsNaN.get()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.IsInfinity;
                };

                ExecuteTest(method, "IsInfinity.get()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.IsNaNOrInfinity;
                };

                ExecuteTest(method, "IsNaNOrInfinity.get()");
            }

            // 模

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.Module;
                };

                ExecuteTest(method, "Module.get()");
            }
#else
            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.VectorModule;
                };

                ExecuteTest(method, "VectorModule.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.ModuleSquared;
                };

                ExecuteTest(method, "ModuleSquared.get()");
            }
#else
            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.VectorModuleSquared;
                };

                ExecuteTest(method, "VectorModuleSquared.get()");
            }
#endif

            // 向量

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.Opposite;
                };

                ExecuteTest(method, "Opposite.get()");
            }
#else
            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.VectorNegate;
                };

                ExecuteTest(method, "VectorNegate.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.Normalize;
                };

                ExecuteTest(method, "Normalize.get()");
            }
#else
            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.VectorNormalize;
                };

                ExecuteTest(method, "VectorNormalize.get()");
            }
#endif

            // 角度

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.AngleFromX;
                };

                ExecuteTest(method, "AngleFromX.get()");
            }
#else
            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.AngleX;
                };

                ExecuteTest(method, "AngleX.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.AngleFromY;
                };

                ExecuteTest(method, "AngleFromY.get()");
            }
#else
            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.AngleY;
                };

                ExecuteTest(method, "AngleY.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.Azimuth;
                };

                ExecuteTest(method, "Azimuth.get()");
            }
#else
            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.VectorAngle;
                };

                ExecuteTest(method, "VectorAngle.get()");
            }
#endif
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

                ExecuteTest(method, "Equals(object)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.GetHashCode();
                };

                ExecuteTest(method, "GetHashCode()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.ToString();
                };

                ExecuteTest(method, "ToString()");
            }

            // Equals

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = left;

                Action method = () =>
                {
                    _ = left.Equals(right);
                };

                ExecuteTest(method, "Equals(Com.PointD)");
            }

            // CompareTo

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();
                object obj = pointD;

                Action method = () =>
                {
                    _ = pointD.CompareTo(obj);
                };

                ExecuteTest(method, "CompareTo(object)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = left;

                Action method = () =>
                {
                    _ = left.CompareTo(right);
                };

                ExecuteTest(method, "CompareTo(Com.PointD)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // 检索

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD.IndexOf(item);
                };

                ExecuteTest(method, "IndexOf(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 0;

                Action method = () =>
                {
                    _ = pointD.IndexOf(item, startIndex);
                };

                ExecuteTest(method, "IndexOf(double, int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 0;
                int count = 2;

                Action method = () =>
                {
                    _ = pointD.IndexOf(item, startIndex, count);
                };

                ExecuteTest(method, "IndexOf(double, int, int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD.LastIndexOf(item);
                };

                ExecuteTest(method, "LastIndexOf(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 1;

                Action method = () =>
                {
                    _ = pointD.LastIndexOf(item, startIndex);
                };

                ExecuteTest(method, "LastIndexOf(double, int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 1;
                int count = 2;

                Action method = () =>
                {
                    _ = pointD.LastIndexOf(item, startIndex, count);
                };

                ExecuteTest(method, "LastIndexOf(double, int, int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD.Contains(item);
                };

                ExecuteTest(method, "Contains(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // ToArray，ToList

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.ToArray();
                };

                ExecuteTest(method, "ToArray()");
            }

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.ToList();
                };

                ExecuteTest(method, "ToList()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // 坐标系转换

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.ToSpherical();
                };

                ExecuteTest(method, "ToSpherical()");
            }
#else
            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.ToPolar();
                };

                ExecuteTest(method, "ToPolar()");
            }
#endif

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.ToCartesian();
                };

                ExecuteTest(method, "ToCartesian()");
            }

            // 距离与夹角

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.DistanceFrom(pt);
                };

                ExecuteTest(method, "DistanceFrom(Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.AngleFrom(pt);
                };

                ExecuteTest(method, "AngleFrom(Com.PointD)");
            }

            // Offset

#if ComVerNext
            {
                Com.PointD pointD = _GetRandomPointD();
                int index = Com.Statistics.RandomInteger(2);
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD.Offset(index, d);
                };

                ExecuteTest(method, "Offset(int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                Com.PointD pointD = _GetRandomPointD();
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD.Offset(d);
                };

                ExecuteTest(method, "Offset(double)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    pointD.Offset(pt);
                };

                ExecuteTest(method, "Offset(Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                double dx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dy = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD.Offset(dx, dy);
                };

                ExecuteTest(method, "Offset(double, double)");
            }

#if ComVerNext
            {
                Com.PointD pointD = _GetRandomPointD();
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD.OffsetX(d);
                };

                ExecuteTest(method, "OffsetX(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

#if ComVerNext
            {
                Com.PointD pointD = _GetRandomPointD();
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD.OffsetY(d);
                };

                ExecuteTest(method, "OffsetY(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                Com.PointD pointD = _GetRandomPointD();
                Point pt = new Point(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    pointD.Offset(pt);
                };

                ExecuteTest(method, "Offset(System.Drawing.Point)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                PointF pt = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    pointD.Offset(pt);
                };

                ExecuteTest(method, "Offset(System.Drawing.PointF)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Size sz = new Size(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    pointD.Offset(sz);
                };

                ExecuteTest(method, "Offset(System.Drawing.Size)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    pointD.Offset(sz);
                };

                ExecuteTest(method, "Offset(System.Drawing.SizeF)");
            }

#if ComVerNext
            {
                Com.PointD pointD = _GetRandomPointD();
                int index = Com.Statistics.RandomInteger(2);
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD.OffsetCopy(index, d);
                };

                ExecuteTest(method, "OffsetCopy(int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                Com.PointD pointD = _GetRandomPointD();
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD.OffsetCopy(d);
                };

                ExecuteTest(method, "OffsetCopy(double)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.OffsetCopy(pt);
                };

                ExecuteTest(method, "OffsetCopy(Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                double dx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dy = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD.OffsetCopy(dx, dy);
                };

                ExecuteTest(method, "OffsetCopy(double, double)");
            }

#if ComVerNext
            {
                Com.PointD pointD = _GetRandomPointD();
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD.OffsetXCopy(d);
                };

                ExecuteTest(method, "OffsetXCopy(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

#if ComVerNext
            {
                Com.PointD pointD = _GetRandomPointD();
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD.OffsetYCopy(d);
                };

                ExecuteTest(method, "OffsetYCopy(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                Com.PointD pointD = _GetRandomPointD();
                Point pt = new Point(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = pointD.OffsetCopy(pt);
                };

                ExecuteTest(method, "OffsetCopy(System.Drawing.Point)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                PointF pt = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = pointD.OffsetCopy(pt);
                };

                ExecuteTest(method, "OffsetCopy(System.Drawing.PointF)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Size sz = new Size(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = pointD.OffsetCopy(sz);
                };

                ExecuteTest(method, "OffsetCopy(System.Drawing.Size)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = pointD.OffsetCopy(sz);
                };

                ExecuteTest(method, "OffsetCopy(System.Drawing.SizeF)");
            }

            // Scale

#if ComVerNext
            {
                Com.PointD pointD = _GetRandomPointD();
                int index = Com.Statistics.RandomInteger(2);
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD.Scale(index, s);
                };

                ExecuteTest(method, "Scale(int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                Com.PointD pointD = _GetRandomPointD();
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD.Scale(s);
                };

                ExecuteTest(method, "Scale(double)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    pointD.Scale(pt);
                };

                ExecuteTest(method, "Scale(Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                double sx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sy = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD.Scale(sx, sy);
                };

                ExecuteTest(method, "Scale(double, double)");
            }

#if ComVerNext
            {
                Com.PointD pointD = _GetRandomPointD();
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD.ScaleX(s);
                };

                ExecuteTest(method, "ScaleX(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

#if ComVerNext
            {
                Com.PointD pointD = _GetRandomPointD();
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD.ScaleY(s);
                };

                ExecuteTest(method, "ScaleY(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                Com.PointD pointD = _GetRandomPointD();
                Point pt = new Point(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    pointD.Scale(pt);
                };

                ExecuteTest(method, "Scale(System.Drawing.Point)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                PointF pt = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    pointD.Scale(pt);
                };

                ExecuteTest(method, "Scale(System.Drawing.PointF)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Size sz = new Size(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    pointD.Scale(sz);
                };

                ExecuteTest(method, "Scale(System.Drawing.Size)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    pointD.Scale(sz);
                };

                ExecuteTest(method, "Scale(System.Drawing.SizeF)");
            }

#if ComVerNext
            {
                Com.PointD pointD = _GetRandomPointD();
                int index = Com.Statistics.RandomInteger(2);
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD.ScaleCopy(index, s);
                };

                ExecuteTest(method, "ScaleCopy(int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                Com.PointD pointD = _GetRandomPointD();
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD.ScaleCopy(s);
                };

                ExecuteTest(method, "ScaleCopy(double)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.ScaleCopy(pt);
                };

                ExecuteTest(method, "ScaleCopy(Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                double sx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sy = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD.ScaleCopy(sx, sy);
                };

                ExecuteTest(method, "ScaleCopy(double, double)");
            }

#if ComVerNext
            {
                Com.PointD pointD = _GetRandomPointD();
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD.ScaleXCopy(s);
                };

                ExecuteTest(method, "ScaleXCopy(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

#if ComVerNext
            {
                Com.PointD pointD = _GetRandomPointD();
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD.ScaleYCopy(s);
                };

                ExecuteTest(method, "ScaleYCopy(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                Com.PointD pointD = _GetRandomPointD();
                Point pt = new Point(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = pointD.ScaleCopy(pt);
                };

                ExecuteTest(method, "ScaleCopy(System.Drawing.Point)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                PointF pt = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = pointD.ScaleCopy(pt);
                };

                ExecuteTest(method, "ScaleCopy(System.Drawing.PointF)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Size sz = new Size(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = pointD.ScaleCopy(sz);
                };

                ExecuteTest(method, "ScaleCopy(System.Drawing.Size)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = pointD.ScaleCopy(sz);
                };

                ExecuteTest(method, "ScaleCopy(System.Drawing.SizeF)");
            }

            // Reflect

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();
                int index = Com.Statistics.RandomInteger(2);

                Action method = () =>
                {
                    pointD.Reflect(index);
                };

                ExecuteTest(method, "Reflect(int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    pointD.ReflectX();
                };

                ExecuteTest(method, "ReflectX()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    pointD.ReflectY();
                };

                ExecuteTest(method, "ReflectY()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();
                int index = Com.Statistics.RandomInteger(2);

                Action method = () =>
                {
                    _ = pointD.ReflectCopy(index);
                };

                ExecuteTest(method, "ReflectCopy(int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.ReflectXCopy();
                };

                ExecuteTest(method, "ReflectXCopy()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.ReflectYCopy();
                };

                ExecuteTest(method, "ReflectYCopy()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // Shear

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();
                int index1 = Com.Statistics.RandomInteger(2);
                int index2 = 1 - index1;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD.Shear(index1, index2, angle);
                };

                ExecuteTest(method, "Shear(int, int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD.ShearX(angle);
                };

                ExecuteTest(method, "ShearX(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD.ShearY(angle);
                };

                ExecuteTest(method, "ShearY(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();
                int index1 = Com.Statistics.RandomInteger(2);
                int index2 = 1 - index1;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD.ShearCopy(index1, index2, angle);
                };

                ExecuteTest(method, "ShearCopy(int, int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD.ShearXCopy(angle);
                };

                ExecuteTest(method, "ShearXCopy(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD.ShearYCopy(angle);
                };

                ExecuteTest(method, "ShearYCopy(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // Rotate

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();
                int index1 = Com.Statistics.RandomInteger(2);
                int index2 = 1 - index1;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD.Rotate(index1, index2, angle);
                };

                ExecuteTest(method, "Rotate(int, int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            {
                Com.PointD pointD = _GetRandomPointD();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD.Rotate(angle);
                };

                ExecuteTest(method, "Rotate(double)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.PointD pt = _GetRandomPointD();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD.Rotate(angle, pt);
                };

                ExecuteTest(method, "Rotate(double, Com.PointD)");
            }

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();
                int index1 = Com.Statistics.RandomInteger(2);
                int index2 = 1 - index1;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD.RotateCopy(index1, index2, angle);
                };

                ExecuteTest(method, "RotateCopy(int, int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            {
                Com.PointD pointD = _GetRandomPointD();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD.RotateCopy(angle);
                };

                ExecuteTest(method, "RotateCopy(double)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.PointD pt = _GetRandomPointD();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD.RotateCopy(angle, pt);
                };

                ExecuteTest(method, "RotateCopy(double, Com.PointD)");
            }

            // Affine

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.PointD ex = _GetRandomNormalPointD();
                Com.PointD ey = _GetRandomNormalPointD();
                Com.PointD offset = _GetRandomPointD();

                Action method = () =>
                {
                    pointD.AffineTransform(ex, ey, offset);
                };

                ExecuteTest(method, "AffineTransform(Com.PointD, Com.PointD, Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.Matrix matrixLeft = _GetRandomMatrix(3, 3);

                Action method = () =>
                {
                    pointD.AffineTransform(matrixLeft);
                };

                ExecuteTest(method, "AffineTransform(Com.Matrix)");
            }

#if ComVer1910
            {
                Com.PointD pointD = _GetRandomPointD();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(3, 3));
                }

                Com.Matrix[] matricesLeft = matrixLeftList.ToArray();

                Action method = () =>
                {
                    pointD.AffineTransform(matricesLeft);
                };

                ExecuteTest(method, "AffineTransform(param Com.Matrix[])", "total 8 matrices");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

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

                ExecuteTest(method, "AffineTransform(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.PointD ex = _GetRandomNormalPointD();
                Com.PointD ey = _GetRandomNormalPointD();
                Com.PointD offset = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.AffineTransformCopy(ex, ey, offset);
                };

                ExecuteTest(method, "AffineTransformCopy(Com.PointD, Com.PointD, Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.Matrix matrixLeft = _GetRandomMatrix(3, 3);

                Action method = () =>
                {
                    _ = pointD.AffineTransformCopy(matrixLeft);
                };

                ExecuteTest(method, "AffineTransformCopy(Com.Matrix)");
            }

#if ComVer1910
            {
                Com.PointD pointD = _GetRandomPointD();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(3, 3));
                }

                Com.Matrix[] matricesLeft = matrixLeftList.ToArray();

                Action method = () =>
                {
                    _ = pointD.AffineTransformCopy(matricesLeft);
                };

                ExecuteTest(method, "AffineTransformCopy(param Com.Matrix[])", "total 8 matrices");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

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

                ExecuteTest(method, "AffineTransformCopy(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.PointD ex = _GetRandomNormalPointD();
                Com.PointD ey = _GetRandomNormalPointD();
                Com.PointD offset = _GetRandomPointD();

                Action method = () =>
                {
                    pointD.InverseAffineTransform(ex, ey, offset);
                };

                ExecuteTest(method, "InverseAffineTransform(Com.PointD, Com.PointD, Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.Matrix matrixLeft = _GetRandomMatrix(3, 3);

                Action method = () =>
                {
                    pointD.InverseAffineTransform(matrixLeft);
                };

                ExecuteTest(method, "InverseAffineTransform(Com.Matrix)");
            }

#if ComVer1910
            {
                Com.PointD pointD = _GetRandomPointD();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(3, 3));
                }

                Com.Matrix[] matricesLeft = matrixLeftList.ToArray();

                Action method = () =>
                {
                    pointD.InverseAffineTransform(matricesLeft);
                };

                ExecuteTest(method, "InverseAffineTransform(param Com.Matrix[])", "total 8 matrices");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

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

                ExecuteTest(method, "InverseAffineTransform(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.PointD ex = _GetRandomNormalPointD();
                Com.PointD ey = _GetRandomNormalPointD();
                Com.PointD offset = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.InverseAffineTransformCopy(ex, ey, offset);
                };

                ExecuteTest(method, "InverseAffineTransformCopy(Com.PointD, Com.PointD, Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();
                Com.Matrix matrixLeft = _GetRandomMatrix(3, 3);

                Action method = () =>
                {
                    _ = pointD.InverseAffineTransformCopy(matrixLeft);
                };

                ExecuteTest(method, "InverseAffineTransformCopy(Com.Matrix)");
            }

#if ComVer1910
            {
                Com.PointD pointD = _GetRandomPointD();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(3, 3));
                }

                Com.Matrix[] matricesLeft = matrixLeftList.ToArray();

                Action method = () =>
                {
                    _ = pointD.InverseAffineTransformCopy(matricesLeft);
                };

                ExecuteTest(method, "InverseAffineTransformCopy(param Com.Matrix[])", "total 8 matrices");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

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

                ExecuteTest(method, "InverseAffineTransformCopy(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            // ToVector

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.ToColumnVector();
                };

                ExecuteTest(method, "ToColumnVector()");
            }
#else
            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.ToVectorColumn();
                };

                ExecuteTest(method, "ToVectorColumn()");
            }
#endif

#if ComVer1905
            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.ToRowVector();
                };

                ExecuteTest(method, "ToRowVector()");
            }
#else
            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.ToVectorRow();
                };

                ExecuteTest(method, "ToVectorRow()");
            }
#endif

            // To

            {
                Com.PointD pointD = new Com.PointD(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = pointD.ToPoint();
                };

                ExecuteTest(method, "ToPoint()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.ToPointF();
                };

                ExecuteTest(method, "ToPointF()");
            }

            {
                Com.PointD pointD = new Com.PointD(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = pointD.ToSize();
                };

                ExecuteTest(method, "ToSize()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.ToSizeF();
                };

                ExecuteTest(method, "ToSizeF()");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = pointD.ToComplex();
                };

                ExecuteTest(method, "ToComplex()");
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

                ExecuteTest(method, "Equals(Com.PointD, Com.PointD)");
            }

            // Compare

#if ComVer1905
            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = left;

                Action method = () =>
                {
                    _ = Com.PointD.Compare(left, right);
                };

                ExecuteTest(method, "Compare(Com.PointD, Com.PointD)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // From

#if ComVer1910
            {
                Com.Vector vector = _GetRandomPointD().ToColumnVector();

                Action method = () =>
                {
                    _ = Com.PointD.FromVector(vector);
                };

                ExecuteTest(method, "FromVector(Com.Vector)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

            {
                Point pt = new Point(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = Com.PointD.FromPoint(pt);
                };

                ExecuteTest(method, "FromPoint(System.Drawing.Point)");
            }

            {
                PointF pt = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = Com.PointD.FromPointF(pt);
                };

                ExecuteTest(method, "FromPointF(System.Drawing.PointF)");
            }

            {
                Size sz = new Size(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = Com.PointD.FromSize(sz);
                };

                ExecuteTest(method, "FromSize(System.Drawing.Size)");
            }

            {
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = Com.PointD.FromSizeF(sz);
                };

                ExecuteTest(method, "FromSizeF(System.Drawing.SizeF)");
            }

            {
                Com.Complex comp = new Com.Complex(Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = Com.PointD.FromComplex(comp);
                };

                ExecuteTest(method, "FromComplex(Com.Complex)");
            }

            // Matrix

            {
                Action method = () =>
                {
                    _ = Com.PointD.IdentityMatrix();
                };

                ExecuteTest(method, "IdentityMatrix()");
            }

#if ComVerNext
            {
                int index = Com.Statistics.RandomInteger(2);
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD.OffsetMatrix(index, d);
                };

                ExecuteTest(method, "OffsetMatrix(int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD.OffsetMatrix(d);
                };

                ExecuteTest(method, "OffsetMatrix(double)");
            }

            {
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = Com.PointD.OffsetMatrix(pt);
                };

                ExecuteTest(method, "OffsetMatrix(Com.PointD)");
            }

            {
                double dx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dy = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD.OffsetMatrix(dx, dy);
                };

                ExecuteTest(method, "OffsetMatrix(double, double)");
            }

#if ComVerNext
            {
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD.OffsetXMatrix(d);
                };

                ExecuteTest(method, "OffsetXMatrix(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

#if ComVerNext
            {
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD.OffsetYMatrix(d);
                };

                ExecuteTest(method, "OffsetYMatrix(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                Point pt = new Point(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = Com.PointD.OffsetMatrix(pt);
                };

                ExecuteTest(method, "OffsetMatrix(System.Drawing.Point)");
            }

            {
                PointF pt = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = Com.PointD.OffsetMatrix(pt);
                };

                ExecuteTest(method, "OffsetMatrix(System.Drawing.PointF)");
            }

            {
                Size sz = new Size(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = Com.PointD.OffsetMatrix(sz);
                };

                ExecuteTest(method, "OffsetMatrix(System.Drawing.Size)");
            }

            {
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = Com.PointD.OffsetMatrix(sz);
                };

                ExecuteTest(method, "OffsetMatrix(System.Drawing.SizeF)");
            }

#if ComVerNext
            {
                int index = Com.Statistics.RandomInteger(2);
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD.ScaleMatrix(index, s);
                };

                ExecuteTest(method, "ScaleMatrix(int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD.ScaleMatrix(s);
                };

                ExecuteTest(method, "ScaleMatrix(double)");
            }

            {
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = Com.PointD.ScaleMatrix(pt);
                };

                ExecuteTest(method, "ScaleMatrix(Com.PointD)");
            }

            {
                double sx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sy = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD.ScaleMatrix(sx, sy);
                };

                ExecuteTest(method, "ScaleMatrix(double, double)");
            }

#if ComVerNext
            {
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD.ScaleXMatrix(s);
                };

                ExecuteTest(method, "ScaleXMatrix(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

#if ComVerNext
            {
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD.ScaleYMatrix(s);
                };

                ExecuteTest(method, "ScaleYMatrix(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                Point pt = new Point(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = Com.PointD.ScaleMatrix(pt);
                };

                ExecuteTest(method, "ScaleMatrix(System.Drawing.Point)");
            }

            {
                PointF pt = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = Com.PointD.ScaleMatrix(pt);
                };

                ExecuteTest(method, "ScaleMatrix(System.Drawing.PointF)");
            }

            {
                Size sz = new Size(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = Com.PointD.ScaleMatrix(sz);
                };

                ExecuteTest(method, "ScaleMatrix(System.Drawing.Size)");
            }

            {
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = Com.PointD.ScaleMatrix(sz);
                };

                ExecuteTest(method, "ScaleMatrix(System.Drawing.SizeF)");
            }

#if ComVer1905
            {
                int index = Com.Statistics.RandomInteger(2);

                Action method = () =>
                {
                    _ = Com.PointD.ReflectMatrix(index);
                };

                ExecuteTest(method, "ReflectMatrix(int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Action method = () =>
                {
                    _ = Com.PointD.ReflectXMatrix();
                };

                ExecuteTest(method, "ReflectXMatrix()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Action method = () =>
                {
                    _ = Com.PointD.ReflectYMatrix();
                };

                ExecuteTest(method, "ReflectYMatrix()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                int index1 = Com.Statistics.RandomInteger(2);
                int index2 = 1 - index1;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD.ShearMatrix(index1, index2, angle);
                };

                ExecuteTest(method, "ShearMatrix(int, int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD.ShearXMatrix(angle);
                };

                ExecuteTest(method, "ShearXMatrix(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD.ShearYMatrix(angle);
                };

                ExecuteTest(method, "ShearYMatrix(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                int index1 = Com.Statistics.RandomInteger(2);
                int index2 = 1 - index1;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD.RotateMatrix(index1, index2, angle);
                };

                ExecuteTest(method, "RotateMatrix(int, int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD.RotateMatrix(angle);
                };

                ExecuteTest(method, "RotateMatrix(double)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD.RotateMatrix(angle, pt);
                };

                ExecuteTest(method, "RotateMatrix(double, Com.PointD)");
            }

            // 距离与夹角

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = Com.PointD.DistanceBetween(left, right);
                };

                ExecuteTest(method, "DistanceBetween(Com.PointD, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = Com.PointD.AngleBetween(left, right);
                };

                ExecuteTest(method, "AngleBetween(Com.PointD, Com.PointD)");
            }

            // 向量乘积

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = Com.PointD.DotProduct(left, right);
                };

                ExecuteTest(method, "DotProduct(Com.PointD, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = Com.PointD.CrossProduct(left, right);
                };

                ExecuteTest(method, "CrossProduct(Com.PointD, Com.PointD)");
            }

            // 初等函数

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = Com.PointD.Abs(pointD);
                };

                ExecuteTest(method, "Abs(Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = Com.PointD.Sign(pointD);
                };

                ExecuteTest(method, "Sign(Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = Com.PointD.Ceiling(pointD);
                };

                ExecuteTest(method, "Ceiling(Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = Com.PointD.Floor(pointD);
                };

                ExecuteTest(method, "Floor(Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = Com.PointD.Round(pointD);
                };

                ExecuteTest(method, "Round(Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = Com.PointD.Truncate(pointD);
                };

                ExecuteTest(method, "Truncate(Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = Com.PointD.Max(left, right);
                };

                ExecuteTest(method, "Max(Com.PointD, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = Com.PointD.Min(left, right);
                };

                ExecuteTest(method, "Min(Com.PointD, Com.PointD)");
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

                ExecuteTest(method, "operator ==(Com.PointD, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = left;

                Action method = () =>
                {
                    _ = (left != right);
                };

                ExecuteTest(method, "operator !=(Com.PointD, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = (left < right);
                };

                ExecuteTest(method, "operator <(Com.PointD, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = (left > right);
                };

                ExecuteTest(method, "operator >(Com.PointD, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = (left <= right);
                };

                ExecuteTest(method, "operator <=(Com.PointD, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = (left >= right);
                };

                ExecuteTest(method, "operator >=(Com.PointD, Com.PointD)");
            }

            // 运算

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = +pointD;
                };

                ExecuteTest(method, "operator +(Com.PointD)");
            }

            {
                Com.PointD pointD = _GetRandomPointD();

                Action method = () =>
                {
                    _ = -pointD;
                };

                ExecuteTest(method, "operator -(Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt + n;
                };

                ExecuteTest(method, "operator +(Com.PointD, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = n + pt;
                };

                ExecuteTest(method, "operator +(double, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = left + right;
                };

                ExecuteTest(method, "operator +(Com.PointD, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Point right = new Point(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = left + right;
                };

                ExecuteTest(method, "operator +(Com.PointD, System.Drawing.Point)");
            }

            {
                Point left = new Point(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = left + right;
                };

                ExecuteTest(method, "operator +(System.Drawing.Point, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                PointF right = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = left + right;
                };

                ExecuteTest(method, "operator +(Com.PointD, System.Drawing.PointF)");
            }

            {
                PointF left = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = left + right;
                };

                ExecuteTest(method, "operator +(System.Drawing.PointF, Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                Size sz = new Size(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = pt + sz;
                };

                ExecuteTest(method, "operator +(Com.PointD, System.Drawing.Size)");
            }

            {
                Size sz = new Size(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = sz + pt;
                };

                ExecuteTest(method, "operator +(System.Drawing.Size, Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = pt + sz;
                };

                ExecuteTest(method, "operator +(Com.PointD, System.Drawing.SizeF)");
            }

            {
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = sz + pt;
                };

                ExecuteTest(method, "operator +(System.Drawing.SizeF, Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt - n;
                };

                ExecuteTest(method, "operator -(Com.PointD, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = n - pt;
                };

                ExecuteTest(method, "operator -(double, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = left - right;
                };

                ExecuteTest(method, "operator -(Com.PointD, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Point right = new Point(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = left - right;
                };

                ExecuteTest(method, "operator -(Com.PointD, System.Drawing.Point)");
            }

            {
                Point left = new Point(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = left - right;
                };

                ExecuteTest(method, "operator -(System.Drawing.Point, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                PointF right = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = left - right;
                };

                ExecuteTest(method, "operator -(Com.PointD, System.Drawing.PointF)");
            }

            {
                PointF left = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = left - right;
                };

                ExecuteTest(method, "operator -(System.Drawing.PointF, Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                Size sz = new Size(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = pt - sz;
                };

                ExecuteTest(method, "operator -(Com.PointD, System.Drawing.Size)");
            }

            {
                Size sz = new Size(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = sz - pt;
                };

                ExecuteTest(method, "operator -(System.Drawing.Size, Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = pt - sz;
                };

                ExecuteTest(method, "operator -(Com.PointD, System.Drawing.SizeF)");
            }

            {
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = sz - pt;
                };

                ExecuteTest(method, "operator -(System.Drawing.SizeF, Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt * n;
                };

                ExecuteTest(method, "operator *(Com.PointD, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = n * pt;
                };

                ExecuteTest(method, "operator *(double, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = left * right;
                };

                ExecuteTest(method, "operator *(Com.PointD, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Point right = new Point(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = left * right;
                };

                ExecuteTest(method, "operator *(Com.PointD, System.Drawing.Point)");
            }

            {
                Point left = new Point(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = left * right;
                };

                ExecuteTest(method, "operator *(System.Drawing.Point, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                PointF right = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = left * right;
                };

                ExecuteTest(method, "operator *(Com.PointD, System.Drawing.PointF)");
            }

            {
                PointF left = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = left * right;
                };

                ExecuteTest(method, "operator *(System.Drawing.PointF, Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                Size sz = new Size(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = pt * sz;
                };

                ExecuteTest(method, "operator *(Com.PointD, System.Drawing.Size)");
            }

            {
                Size sz = new Size(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = sz * pt;
                };

                ExecuteTest(method, "operator *(System.Drawing.Size, Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = pt * sz;
                };

                ExecuteTest(method, "operator *(Com.PointD, System.Drawing.SizeF)");
            }

            {
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = sz * pt;
                };

                ExecuteTest(method, "operator *(System.Drawing.SizeF, Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt / n;
                };

                ExecuteTest(method, "operator /(Com.PointD, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = n / pt;
                };

                ExecuteTest(method, "operator /(double, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = left / right;
                };

                ExecuteTest(method, "operator /(Com.PointD, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                Point right = new Point(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = left / right;
                };

                ExecuteTest(method, "operator /(Com.PointD, System.Drawing.Point)");
            }

            {
                Point left = new Point(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = left / right;
                };

                ExecuteTest(method, "operator /(System.Drawing.Point, Com.PointD)");
            }

            {
                Com.PointD left = _GetRandomPointD();
                PointF right = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = left / right;
                };

                ExecuteTest(method, "operator /(Com.PointD, System.Drawing.PointF)");
            }

            {
                PointF left = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));
                Com.PointD right = _GetRandomPointD();

                Action method = () =>
                {
                    _ = left / right;
                };

                ExecuteTest(method, "operator /(System.Drawing.PointF, Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                Size sz = new Size(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = pt / sz;
                };

                ExecuteTest(method, "operator /(Com.PointD, System.Drawing.Size)");
            }

            {
                Size sz = new Size(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = sz / pt;
                };

                ExecuteTest(method, "operator /(System.Drawing.Size, Com.PointD)");
            }

            {
                Com.PointD pt = _GetRandomPointD();
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = pt / sz;
                };

                ExecuteTest(method, "operator /(Com.PointD, System.Drawing.SizeF)");
            }

            {
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = sz / pt;
                };

                ExecuteTest(method, "operator /(System.Drawing.SizeF, Com.PointD)");
            }

            // 类型转换

#if ComVer1905
            {
                Com.PointD pt = new Com.PointD(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = (Point)pt;
                };

                ExecuteTest(method, "explicit operator System.Drawing.Point(Com.PointD)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = (PointF)pt;
                };

                ExecuteTest(method, "explicit operator System.Drawing.PointF(Com.PointD)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD pt = new Com.PointD(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = (Size)pt;
                };

                ExecuteTest(method, "explicit operator System.Drawing.Size(Com.PointD)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = (SizeF)pt;
                };

                ExecuteTest(method, "explicit operator System.Drawing.SizeF(Com.PointD)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD pt = _GetRandomPointD();

                Action method = () =>
                {
                    _ = (Com.Complex)pt;
                };

                ExecuteTest(method, "explicit operator Com.Complex(Com.PointD)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Point pt = new Point(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = (Com.PointD)pt;
                };

                ExecuteTest(method, "implicit operator Com.PointD(System.Drawing.Point)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                PointF pt = new PointF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = (Com.PointD)pt;
                };

                ExecuteTest(method, "implicit operator Com.PointD(System.Drawing.PointF)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Size sz = new Size(Com.Statistics.RandomInteger() - int.MaxValue / 2, Com.Statistics.RandomInteger() - int.MaxValue / 2);

                Action method = () =>
                {
                    _ = (Com.PointD)sz;
                };

                ExecuteTest(method, "explicit operator Com.PointD(System.Drawing.Size)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                SizeF sz = new SizeF((float)Com.Statistics.RandomDouble(-1E18, 1E18), (float)Com.Statistics.RandomDouble(-1E18, 1E18));

                Action method = () =>
                {
                    _ = (Com.PointD)sz;
                };

                ExecuteTest(method, "explicit operator Com.PointD(System.Drawing.SizeF)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVerNext
            {
                Com.PointD pt = _GetRandomPointD();
                (double, double) tuple = (pt.X, pt.Y);

                Action method = () =>
                {
                    _ = (Com.PointD)tuple;
                };

                ExecuteTest(method, "implicit operator Com.PointD(System.ValueTuple<double, double>)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif
        }
    }

    sealed class PointD3DTest : ClassPerfTestBase
    {
        private const string _NamespaceName = "Com";
        private const string _ClassName = "PointD3D";

        private void ExecuteTest(Action method, string methodName, string comment)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName, comment);
        }

        private void ExecuteTest(Action method, string methodName)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(string methodName, UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName, unsupportedReason);
        }

        private void ExecuteTest(string methodName)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, string.Empty, unsupportedReason);
        }

        //

        private static Com.PointD3D _GetRandomPointD3D()
        {
            return new Com.PointD3D(Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18));
        }

        private static Com.PointD3D _GetRandomNormalPointD3D()
        {
#if ComVer1905
            return _GetRandomPointD3D().Normalize;
#else
            return _GetRandomPointD3D().VectorNormalize;
#endif
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
                        matrix[x, y] = Com.Statistics.RandomDouble(-10, 10);
                    }
                }

                return matrix;
            }
            else
            {
#if ComVer1905
                return Com.Matrix.Empty;
#else
                return Com.Matrix.NonMatrix;
#endif
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

                ExecuteTest(method, "PointD3D(double, double, double)");
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

                ExecuteTest(method, "this[int].get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                int index = Com.Statistics.RandomInteger(3);
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD3D[index] = value;
                };

                ExecuteTest(method, "this[int].set(double)");
            }

            // 分量

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.X;
                };

                ExecuteTest(method, "X.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD3D.X = value;
                };

                ExecuteTest(method, "X.set(double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.Y;
                };

                ExecuteTest(method, "Y.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD3D.Y = value;
                };

                ExecuteTest(method, "Y.set(double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.Z;
                };

                ExecuteTest(method, "Z.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD3D.Z = value;
                };

                ExecuteTest(method, "Z.set(double)");
            }

            // Dimension

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.Dimension;
                };

                ExecuteTest(method, "Dimension.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // Is

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.IsEmpty;
                };

                ExecuteTest(method, "IsEmpty.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.IsZero;
                };

                ExecuteTest(method, "IsZero.get()");
            }
#else
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.IsEmpty;
                };

                ExecuteTest(method, "IsEmpty.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.IsReadOnly;
                };

                ExecuteTest(method, "IsReadOnly.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.IsFixedSize;
                };

                ExecuteTest(method, "IsFixedSize.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.IsNaN;
                };

                ExecuteTest(method, "IsNaN.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.IsInfinity;
                };

                ExecuteTest(method, "IsInfinity.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.IsNaNOrInfinity;
                };

                ExecuteTest(method, "IsNaNOrInfinity.get()");
            }

            // 模

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.Module;
                };

                ExecuteTest(method, "Module.get()");
            }
#else
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.VectorModule;
                };

                ExecuteTest(method, "VectorModule.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.ModuleSquared;
                };

                ExecuteTest(method, "ModuleSquared.get()");
            }
#else
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.VectorModuleSquared;
                };

                ExecuteTest(method, "VectorModuleSquared.get()");
            }
#endif

            // 向量

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.Opposite;
                };

                ExecuteTest(method, "Opposite.get()");
            }
#else
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.VectorNegate;
                };

                ExecuteTest(method, "VectorNegate.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.Normalize;
                };

                ExecuteTest(method, "Normalize.get()");
            }
#else
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.VectorNormalize;
                };

                ExecuteTest(method, "VectorNormalize.get()");
            }
#endif

            // 子空间分量

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.XY;
                };

                ExecuteTest(method, "XY.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD value = _GetRandomPointD();

                Action method = () =>
                {
                    pointD3D.XY = value;
                };

                ExecuteTest(method, "XY.set(Com.PointD)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.YZ;
                };

                ExecuteTest(method, "YZ.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD value = _GetRandomPointD();

                Action method = () =>
                {
                    pointD3D.YZ = value;
                };

                ExecuteTest(method, "YZ.set(Com.PointD)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.ZX;
                };

                ExecuteTest(method, "ZX.get()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD value = _GetRandomPointD();

                Action method = () =>
                {
                    pointD3D.ZX = value;
                };

                ExecuteTest(method, "ZX.set(Com.PointD)");
            }

            // 角度

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.AngleFromX;
                };

                ExecuteTest(method, "AngleFromX.get()");
            }
#else
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.AngleX;
                };

                ExecuteTest(method, "AngleX.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.AngleFromY;
                };

                ExecuteTest(method, "AngleFromY.get()");
            }
#else
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.AngleY;
                };

                ExecuteTest(method, "AngleY.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.AngleFromZ;
                };

                ExecuteTest(method, "AngleFromZ.get()");
            }
#else
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.AngleZ;
                };

                ExecuteTest(method, "AngleZ.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.AngleFromXY;
                };

                ExecuteTest(method, "AngleFromXY.get()");
            }
#else
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.AngleXY;
                };

                ExecuteTest(method, "AngleXY.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.AngleFromYZ;
                };

                ExecuteTest(method, "AngleFromYZ.get()");
            }
#else
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.AngleYZ;
                };

                ExecuteTest(method, "AngleYZ.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.AngleFromZX;
                };

                ExecuteTest(method, "AngleFromZX.get()");
            }
#else
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.AngleZX;
                };

                ExecuteTest(method, "AngleZX.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.Zenith;
                };

                ExecuteTest(method, "Zenith.get()");
            }
#else
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.VectorAngleZ;
                };

                ExecuteTest(method, "VectorAngleZ.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.Azimuth;
                };

                ExecuteTest(method, "Azimuth.get()");
            }
#else
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.VectorAngleXY;
                };

                ExecuteTest(method, "VectorAngleXY.get()");
            }
#endif
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

                ExecuteTest(method, "Equals(object)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.GetHashCode();
                };

                ExecuteTest(method, "GetHashCode()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.ToString();
                };

                ExecuteTest(method, "ToString()");
            }

            // Equals

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = left;

                Action method = () =>
                {
                    _ = left.Equals(right);
                };

                ExecuteTest(method, "Equals(Com.PointD3D)");
            }

            // CompareTo

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                object obj = pointD3D;

                Action method = () =>
                {
                    _ = pointD3D.CompareTo(obj);
                };

                ExecuteTest(method, "CompareTo(object)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = left;

                Action method = () =>
                {
                    _ = left.CompareTo(right);
                };

                ExecuteTest(method, "CompareTo(Com.PointD3D)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // 检索

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD3D.IndexOf(item);
                };

                ExecuteTest(method, "IndexOf(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 0;

                Action method = () =>
                {
                    _ = pointD3D.IndexOf(item, startIndex);
                };

                ExecuteTest(method, "IndexOf(double, int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 0;
                int count = 3;

                Action method = () =>
                {
                    _ = pointD3D.IndexOf(item, startIndex, count);
                };

                ExecuteTest(method, "IndexOf(double, int, int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD3D.LastIndexOf(item);
                };

                ExecuteTest(method, "LastIndexOf(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 2;

                Action method = () =>
                {
                    _ = pointD3D.LastIndexOf(item, startIndex);
                };

                ExecuteTest(method, "LastIndexOf(double, int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 2;
                int count = 3;

                Action method = () =>
                {
                    _ = pointD3D.LastIndexOf(item, startIndex, count);
                };

                ExecuteTest(method, "LastIndexOf(double, int, int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD3D.Contains(item);
                };

                ExecuteTest(method, "Contains(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // ToArray，ToList

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.ToArray();
                };

                ExecuteTest(method, "ToArray()");
            }

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.ToList();
                };

                ExecuteTest(method, "ToList()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // 坐标系转换

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.ToSpherical();
                };

                ExecuteTest(method, "ToSpherical()");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.ToCartesian();
                };

                ExecuteTest(method, "ToCartesian()");
            }

            // 距离与夹角

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD3D pt = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.DistanceFrom(pt);
                };

                ExecuteTest(method, "DistanceFrom(Com.PointD3D)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD3D pt = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.AngleFrom(pt);
                };

                ExecuteTest(method, "AngleFrom(Com.PointD3D)");
            }

            // Offset

#if ComVerNext
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                int index = Com.Statistics.RandomInteger(3);
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD3D.Offset(index, d);
                };

                ExecuteTest(method, "Offset(int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD3D.Offset(d);
                };

                ExecuteTest(method, "Offset(double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD3D pt = _GetRandomPointD3D();

                Action method = () =>
                {
                    pointD3D.Offset(pt);
                };

                ExecuteTest(method, "Offset(Com.PointD3D)");
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

                ExecuteTest(method, "Offset(double, double, double)");
            }

#if ComVerNext
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD3D.OffsetX(d);
                };

                ExecuteTest(method, "OffsetX(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

#if ComVerNext
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD3D.OffsetY(d);
                };

                ExecuteTest(method, "OffsetY(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

#if ComVerNext
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD3D.OffsetZ(d);
                };

                ExecuteTest(method, "OffsetZ(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

#if ComVerNext
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                int index = Com.Statistics.RandomInteger(3);
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD3D.OffsetCopy(index, d);
                };

                ExecuteTest(method, "OffsetCopy(int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD3D.OffsetCopy(d);
                };

                ExecuteTest(method, "OffsetCopy(double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD3D pt = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.OffsetCopy(pt);
                };

                ExecuteTest(method, "OffsetCopy(Com.PointD3D)");
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

                ExecuteTest(method, "OffsetCopy(double, double, double)");
            }

#if ComVerNext
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD3D.OffsetXCopy(d);
                };

                ExecuteTest(method, "OffsetXCopy(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

#if ComVerNext
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD3D.OffsetYCopy(d);
                };

                ExecuteTest(method, "OffsetYCopy(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

#if ComVerNext
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD3D.OffsetZCopy(d);
                };

                ExecuteTest(method, "OffsetZCopy(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            // Scale

#if ComVerNext
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                int index = Com.Statistics.RandomInteger(3);
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD3D.Scale(index, s);
                };

                ExecuteTest(method, "Scale(int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD3D.Scale(s);
                };

                ExecuteTest(method, "Scale(double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD3D pt = _GetRandomPointD3D();

                Action method = () =>
                {
                    pointD3D.Scale(pt);
                };

                ExecuteTest(method, "Scale(Com.PointD3D)");
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

                ExecuteTest(method, "Scale(double, double, double)");
            }

#if ComVerNext
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD3D.ScaleX(s);
                };

                ExecuteTest(method, "ScaleX(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

#if ComVerNext
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD3D.ScaleY(s);
                };

                ExecuteTest(method, "ScaleY(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

#if ComVerNext
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD3D.ScaleZ(s);
                };

                ExecuteTest(method, "ScaleZ(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD3D.ScaleCopy(s);
                };

                ExecuteTest(method, "ScaleCopy(double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD3D pt = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.ScaleCopy(pt);
                };

                ExecuteTest(method, "ScaleCopy(Com.PointD3D)");
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

                ExecuteTest(method, "ScaleCopy(double, double, double)");
            }

            // Reflect

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                int index = Com.Statistics.RandomInteger(3);

                Action method = () =>
                {
                    pointD3D.Reflect(index);
                };

                ExecuteTest(method, "Reflect(int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    pointD3D.ReflectX();
                };

                ExecuteTest(method, "ReflectX()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    pointD3D.ReflectY();
                };

                ExecuteTest(method, "ReflectY()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    pointD3D.ReflectZ();
                };

                ExecuteTest(method, "ReflectZ()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                int index = Com.Statistics.RandomInteger(3);

                Action method = () =>
                {
                    _ = pointD3D.ReflectCopy(index);
                };

                ExecuteTest(method, "ReflectCopy(int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.ReflectXCopy();
                };

                ExecuteTest(method, "ReflectXCopy()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.ReflectYCopy();
                };

                ExecuteTest(method, "ReflectYCopy()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.ReflectZCopy();
                };

                ExecuteTest(method, "ReflectZCopy()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // Shear

#if ComVer1905
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

                ExecuteTest(method, "Shear(int, int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD3D.ShearXY(angle);
                };

                ExecuteTest(method, "ShearXY(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD3D.ShearYX(angle);
                };

                ExecuteTest(method, "ShearYX(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD3D.ShearYZ(angle);
                };

                ExecuteTest(method, "ShearYZ(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD3D.ShearZY(angle);
                };

                ExecuteTest(method, "ShearZY(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD3D.ShearZX(angle);
                };

                ExecuteTest(method, "ShearZX(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD3D.ShearXZ(angle);
                };

                ExecuteTest(method, "ShearXZ(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
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

                ExecuteTest(method, "ShearCopy(int, int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD3D.ShearXYCopy(angle);
                };

                ExecuteTest(method, "ShearXYCopy(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD3D.ShearYXCopy(angle);
                };

                ExecuteTest(method, "ShearYXCopy(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD3D.ShearYZCopy(angle);
                };

                ExecuteTest(method, "ShearYZCopy(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD3D.ShearZYCopy(angle);
                };

                ExecuteTest(method, "ShearZYCopy(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD3D.ShearZXCopy(angle);
                };

                ExecuteTest(method, "ShearZXCopy(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD3D.ShearXZCopy(angle);
                };

                ExecuteTest(method, "ShearXZCopy(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // Rotate

#if ComVer1905
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

                ExecuteTest(method, "Rotate(int, int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD3D.RotateX(angle);
                };

                ExecuteTest(method, "RotateX(double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD3D.RotateY(angle);
                };

                ExecuteTest(method, "RotateY(double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    pointD3D.RotateZ(angle);
                };

                ExecuteTest(method, "RotateZ(double)");
            }

#if ComVer1905
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

                ExecuteTest(method, "RotateCopy(int, int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD3D.RotateXCopy(angle);
                };

                ExecuteTest(method, "RotateXCopy(double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD3D.RotateYCopy(angle);
                };

                ExecuteTest(method, "RotateYCopy(double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = pointD3D.RotateZCopy(angle);
                };

                ExecuteTest(method, "RotateZCopy(double)");
            }

            // Affine

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD3D ex = _GetRandomNormalPointD3D();
                Com.PointD3D ey = _GetRandomNormalPointD3D();
                Com.PointD3D ez = _GetRandomNormalPointD3D();
                Com.PointD3D offset = _GetRandomPointD3D();

                Action method = () =>
                {
                    pointD3D.AffineTransform(ex, ey, ez, offset);
                };

                ExecuteTest(method, "AffineTransform(Com.PointD3D, Com.PointD3D, Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.Matrix matrixLeft = _GetRandomMatrix(4, 4);

                Action method = () =>
                {
                    pointD3D.AffineTransform(matrixLeft);
                };

                ExecuteTest(method, "AffineTransform(Com.Matrix)");
            }

#if ComVer1910
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(4, 4));
                }

                Com.Matrix[] matricesLeft = matrixLeftList.ToArray();

                Action method = () =>
                {
                    pointD3D.AffineTransform(matricesLeft);
                };

                ExecuteTest(method, "AffineTransform(param Com.Matrix[])", "total 8 matrices");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

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

                ExecuteTest(method, "AffineTransform(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD3D ex = _GetRandomNormalPointD3D();
                Com.PointD3D ey = _GetRandomNormalPointD3D();
                Com.PointD3D ez = _GetRandomNormalPointD3D();
                Com.PointD3D offset = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.AffineTransformCopy(ex, ey, ez, offset);
                };

                ExecuteTest(method, "AffineTransformCopy(Com.PointD3D, Com.PointD3D, Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.Matrix matrixLeft = _GetRandomMatrix(4, 4);

                Action method = () =>
                {
                    _ = pointD3D.AffineTransformCopy(matrixLeft);
                };

                ExecuteTest(method, "AffineTransformCopy(Com.Matrix)");
            }

#if ComVer1910
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(4, 4));
                }

                Com.Matrix[] matricesLeft = matrixLeftList.ToArray();

                Action method = () =>
                {
                    _ = pointD3D.AffineTransformCopy(matricesLeft);
                };

                ExecuteTest(method, "AffineTransformCopy(param Com.Matrix[])", "total 8 matrices");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

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

                ExecuteTest(method, "AffineTransformCopy(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD3D ex = _GetRandomNormalPointD3D();
                Com.PointD3D ey = _GetRandomNormalPointD3D();
                Com.PointD3D ez = _GetRandomNormalPointD3D();
                Com.PointD3D offset = _GetRandomPointD3D();

                Action method = () =>
                {
                    pointD3D.InverseAffineTransform(ex, ey, ez, offset);
                };

                ExecuteTest(method, "InverseAffineTransform(Com.PointD3D, Com.PointD3D, Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.Matrix matrixLeft = _GetRandomMatrix(4, 4);

                Action method = () =>
                {
                    pointD3D.InverseAffineTransform(matrixLeft);
                };

                ExecuteTest(method, "InverseAffineTransform(Com.Matrix)");
            }

#if ComVer1910
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(4, 4));
                }

                Com.Matrix[] matricesLeft = matrixLeftList.ToArray();

                Action method = () =>
                {
                    pointD3D.InverseAffineTransform(matricesLeft);
                };

                ExecuteTest(method, "InverseAffineTransform(param Com.Matrix[])", "total 8 matrices");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

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

                ExecuteTest(method, "InverseAffineTransform(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD3D ex = _GetRandomNormalPointD3D();
                Com.PointD3D ey = _GetRandomNormalPointD3D();
                Com.PointD3D ez = _GetRandomNormalPointD3D();
                Com.PointD3D offset = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.InverseAffineTransformCopy(ex, ey, ez, offset);
                };

                ExecuteTest(method, "InverseAffineTransformCopy(Com.PointD3D, Com.PointD3D, Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.Matrix matrixLeft = _GetRandomMatrix(4, 4);

                Action method = () =>
                {
                    _ = pointD3D.InverseAffineTransformCopy(matrixLeft);
                };

                ExecuteTest(method, "InverseAffineTransformCopy(Com.Matrix)");
            }

#if ComVer1910
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(4, 4));
                }

                Com.Matrix[] matricesLeft = matrixLeftList.ToArray();

                Action method = () =>
                {
                    _ = pointD3D.InverseAffineTransformCopy(matricesLeft);
                };

                ExecuteTest(method, "InverseAffineTransformCopy(param Com.Matrix[])", "total 8 matrices");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

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

                ExecuteTest(method, "InverseAffineTransformCopy(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            // Project

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD3D prjCenter = _GetRandomPointD3D();
                double focalLength
                    = (pointD3D.Z - prjCenter.Z) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    _ = pointD3D.ProjectToXY(prjCenter, focalLength);
                };

                ExecuteTest(method, "ProjectToXY(Com.PointD3D, double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD3D prjCenter = _GetRandomPointD3D();
                double focalLength = (pointD3D.X - prjCenter.X) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    _ = pointD3D.ProjectToYZ(prjCenter, focalLength);
                };

                ExecuteTest(method, "ProjectToYZ(Com.PointD3D, double)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();
                Com.PointD3D prjCenter = _GetRandomPointD3D();
                double focalLength = (pointD3D.Y - prjCenter.Y) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    _ = pointD3D.ProjectToZX(prjCenter, focalLength);
                };

                ExecuteTest(method, "ProjectToZX(Com.PointD3D, double)");
            }

            // ToVector

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.ToColumnVector();
                };

                ExecuteTest(method, "ToColumnVector()");
            }
#else
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.ToVectorColumn();
                };

                ExecuteTest(method, "ToVectorColumn()");
            }
#endif

#if ComVer1905
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.ToRowVector();
                };

                ExecuteTest(method, "ToRowVector()");
            }
#else
            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = pointD3D.ToVectorRow();
                };

                ExecuteTest(method, "ToVectorRow()");
            }
#endif
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

                ExecuteTest(method, "Equals(Com.PointD3D, Com.PointD3D)");
            }

            // Compare

#if ComVer1905
            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = left;

                Action method = () =>
                {
                    _ = Com.PointD3D.Compare(left, right);
                };

                ExecuteTest(method, "Compare(Com.PointD3D, Com.PointD3D)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // From

#if ComVer1910
            {
                Com.Vector vector = _GetRandomPointD3D().ToColumnVector();

                Action method = () =>
                {
                    _ = Com.PointD3D.FromVector(vector);
                };

                ExecuteTest(method, "FromVector(Com.Vector)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

            // Matrix

            {
                Action method = () =>
                {
                    _ = Com.PointD3D.IdentityMatrix();
                };

                ExecuteTest(method, "IdentityMatrix()");
            }

#if ComVerNext
            {
                int index = Com.Statistics.RandomInteger(3);
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD3D.OffsetMatrix(index, d);
                };

                ExecuteTest(method, "OffsetMatrix(int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD3D.OffsetMatrix(d);
                };

                ExecuteTest(method, "OffsetMatrix(double)");
            }

            {
                Com.PointD3D pt = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = Com.PointD3D.OffsetMatrix(pt);
                };

                ExecuteTest(method, "OffsetMatrix(Com.PointD3D)");
            }

            {
                double dx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dy = Com.Statistics.RandomDouble(-1E18, 1E18);
                double dz = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD3D.OffsetMatrix(dx, dy, dz);
                };

                ExecuteTest(method, "OffsetMatrix(double, double, double)");
            }

#if ComVerNext
            {
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD3D.OffsetXMatrix(d);
                };

                ExecuteTest(method, "OffsetXMatrix(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

#if ComVerNext
            {
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD3D.OffsetYMatrix(d);
                };

                ExecuteTest(method, "OffsetYMatrix(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

#if ComVerNext
            {
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD3D.OffsetZMatrix(d);
                };

                ExecuteTest(method, "OffsetZMatrix(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

#if ComVerNext
            {
                int index = Com.Statistics.RandomInteger(3);
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD3D.ScaleMatrix(index, s);
                };

                ExecuteTest(method, "ScaleMatrix(int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD3D.ScaleMatrix(s);
                };

                ExecuteTest(method, "ScaleMatrix(double)");
            }

            {
                Com.PointD3D pt = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = Com.PointD3D.ScaleMatrix(pt);
                };

                ExecuteTest(method, "ScaleMatrix(Com.PointD3D)");
            }

            {
                double sx = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sy = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sz = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD3D.ScaleMatrix(sx, sy, sz);
                };

                ExecuteTest(method, "ScaleMatrix(double, double, double)");
            }

#if ComVerNext
            {
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD3D.ScaleXMatrix(s);
                };

                ExecuteTest(method, "ScaleXMatrix(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

#if ComVerNext
            {
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD3D.ScaleYMatrix(s);
                };

                ExecuteTest(method, "ScaleYMatrix(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

#if ComVerNext
            {
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD3D.ScaleZMatrix(s);
                };

                ExecuteTest(method, "ScaleZMatrix(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

#if ComVer1905
            {
                int index = Com.Statistics.RandomInteger(3);

                Action method = () =>
                {
                    _ = Com.PointD3D.ReflectMatrix(index);
                };

                ExecuteTest(method, "ReflectMatrix(int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Action method = () =>
                {
                    _ = Com.PointD3D.ReflectXMatrix();
                };

                ExecuteTest(method, "ReflectXMatrix()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Action method = () =>
                {
                    _ = Com.PointD3D.ReflectYMatrix();
                };

                ExecuteTest(method, "ReflectYMatrix()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Action method = () =>
                {
                    _ = Com.PointD3D.ReflectZMatrix();
                };

                ExecuteTest(method, "ReflectZMatrix()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                int index1 = Com.Statistics.RandomInteger(3);
                int index2 = Com.Statistics.RandomInteger(2);
                if (index2 == index1) index2++;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD3D.ShearMatrix(index1, index2, angle);
                };

                ExecuteTest(method, "ShearMatrix(int, int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD3D.ShearXYMatrix(angle);
                };

                ExecuteTest(method, "ShearXYMatrix(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD3D.ShearYXMatrix(angle);
                };

                ExecuteTest(method, "ShearYXMatrix(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD3D.ShearYZMatrix(angle);
                };

                ExecuteTest(method, "ShearYZMatrix(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD3D.ShearZYMatrix(angle);
                };

                ExecuteTest(method, "ShearZYMatrix(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD3D.ShearZXMatrix(angle);
                };

                ExecuteTest(method, "ShearZXMatrix(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD3D.ShearXZMatrix(angle);
                };

                ExecuteTest(method, "ShearXZMatrix(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                int index1 = Com.Statistics.RandomInteger(3);
                int index2 = Com.Statistics.RandomInteger(2);
                if (index2 == index1) index2++;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD3D.RotateMatrix(index1, index2, angle);
                };

                ExecuteTest(method, "RotateMatrix(int, int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD3D.RotateXMatrix(angle);
                };

                ExecuteTest(method, "RotateXMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD3D.RotateYMatrix(angle);
                };

                ExecuteTest(method, "RotateYMatrix(double)");
            }

            {
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD3D.RotateZMatrix(angle);
                };

                ExecuteTest(method, "RotateZMatrix(double)");
            }

            // 距离与夹角

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = Com.PointD3D.DistanceBetween(left, right);
                };

                ExecuteTest(method, "DistanceBetween(Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = Com.PointD3D.AngleBetween(left, right);
                };

                ExecuteTest(method, "AngleBetween(Com.PointD3D, Com.PointD3D)");
            }

            // 向量乘积

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = Com.PointD3D.DotProduct(left, right);
                };

                ExecuteTest(method, "DotProduct(Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = Com.PointD3D.CrossProduct(left, right);
                };

                ExecuteTest(method, "CrossProduct(Com.PointD3D, Com.PointD3D)");
            }

            // 初等函数

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = Com.PointD3D.Abs(pointD3D);
                };

                ExecuteTest(method, "Abs(Com.PointD3D)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = Com.PointD3D.Sign(pointD3D);
                };

                ExecuteTest(method, "Sign(Com.PointD3D)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = Com.PointD3D.Ceiling(pointD3D);
                };

                ExecuteTest(method, "Ceiling(Com.PointD3D)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = Com.PointD3D.Floor(pointD3D);
                };

                ExecuteTest(method, "Floor(Com.PointD3D)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = Com.PointD3D.Round(pointD3D);
                };

                ExecuteTest(method, "Round(Com.PointD3D)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = Com.PointD3D.Truncate(pointD3D);
                };

                ExecuteTest(method, "Truncate(Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = Com.PointD3D.Max(left, right);
                };

                ExecuteTest(method, "Max(Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = Com.PointD3D.Min(left, right);
                };

                ExecuteTest(method, "Min(Com.PointD3D, Com.PointD3D)");
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

                ExecuteTest(method, "operator ==(Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = left;

                Action method = () =>
                {
                    _ = (left != right);
                };

                ExecuteTest(method, "operator !=(Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = (left < right);
                };

                ExecuteTest(method, "operator <(Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = (left > right);
                };

                ExecuteTest(method, "operator >(Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = (left <= right);
                };

                ExecuteTest(method, "operator <=(Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = (left >= right);
                };

                ExecuteTest(method, "operator >=(Com.PointD3D, Com.PointD3D)");
            }

            // 运算

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = +pointD3D;
                };

                ExecuteTest(method, "operator +(Com.PointD3D)");
            }

            {
                Com.PointD3D pointD3D = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = -pointD3D;
                };

                ExecuteTest(method, "operator -(Com.PointD3D)");
            }

            {
                Com.PointD3D pt = _GetRandomPointD3D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt + n;
                };

                ExecuteTest(method, "operator +(Com.PointD3D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD3D pt = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = n + pt;
                };

                ExecuteTest(method, "operator +(double, Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = left + right;
                };

                ExecuteTest(method, "operator +(Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D pt = _GetRandomPointD3D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt - n;
                };

                ExecuteTest(method, "operator -(Com.PointD3D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD3D pt = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = n - pt;
                };

                ExecuteTest(method, "operator -(double, Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = left - right;
                };

                ExecuteTest(method, "operator -(Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D pt = _GetRandomPointD3D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt * n;
                };

                ExecuteTest(method, "operator *(Com.PointD3D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD3D pt = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = n * pt;
                };

                ExecuteTest(method, "operator *(double, Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = left * right;
                };

                ExecuteTest(method, "operator *(Com.PointD3D, Com.PointD3D)");
            }

            {
                Com.PointD3D pt = _GetRandomPointD3D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt / n;
                };

                ExecuteTest(method, "operator /(Com.PointD3D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD3D pt = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = n / pt;
                };

                ExecuteTest(method, "operator /(double, Com.PointD3D)");
            }

            {
                Com.PointD3D left = _GetRandomPointD3D();
                Com.PointD3D right = _GetRandomPointD3D();

                Action method = () =>
                {
                    _ = left / right;
                };

                ExecuteTest(method, "operator /(Com.PointD3D, Com.PointD3D)");
            }

            // 类型转换

#if ComVerNext
            {
                Com.PointD3D pt = _GetRandomPointD3D();
                (double, double, double) tuple = (pt.X, pt.Y, pt.Z);

                Action method = () =>
                {
                    _ = (Com.PointD3D)tuple;
                };

                ExecuteTest(method, "implicit operator Com.PointD3D(System.ValueTuple<double, double, double>)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif
        }
    }

    sealed class PointD4DTest : ClassPerfTestBase
    {
        private const string _NamespaceName = "Com";
        private const string _ClassName = "PointD4D";

        private void ExecuteTest(Action method, string methodName, string comment)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName, comment);
        }

        private void ExecuteTest(Action method, string methodName)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(string methodName, UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName, unsupportedReason);
        }

        private void ExecuteTest(string methodName)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, string.Empty, unsupportedReason);
        }

        //

        private static Com.PointD4D _GetRandomPointD4D()
        {
            return new Com.PointD4D(Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18));
        }

        private static Com.PointD4D _GetRandomNormalPointD4D()
        {
#if ComVer1905
            return _GetRandomPointD4D().Normalize;
#else
            return _GetRandomPointD4D().VectorNormalize;
#endif
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
                        matrix[x, y] = Com.Statistics.RandomDouble(-10, 10);
                    }
                }

                return matrix;
            }
            else
            {
#if ComVer1905
                return Com.Matrix.Empty;
#else
                return Com.Matrix.NonMatrix;
#endif
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

                ExecuteTest(method, "PointD4D(double, double, double, double)");
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

                ExecuteTest(method, "this[int].get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                int index = Com.Statistics.RandomInteger(4);
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD4D[index] = value;
                };

                ExecuteTest(method, "this[int].set(double)");
            }

            // 分量

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.X;
                };

                ExecuteTest(method, "X.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD4D.X = value;
                };

                ExecuteTest(method, "X.set(double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.Y;
                };

                ExecuteTest(method, "Y.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD4D.Y = value;
                };

                ExecuteTest(method, "Y.set(double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.Z;
                };

                ExecuteTest(method, "Z.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD4D.Z = value;
                };

                ExecuteTest(method, "Z.set(double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.U;
                };

                ExecuteTest(method, "U.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD4D.U = value;
                };

                ExecuteTest(method, "U.set(double)");
            }

            // Dimension

#if ComVer1905
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.Dimension;
                };

                ExecuteTest(method, "Dimension.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // Is

#if ComVer1905
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.IsEmpty;
                };

                ExecuteTest(method, "IsEmpty.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.IsZero;
                };

                ExecuteTest(method, "IsZero.get()");
            }
#else
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.IsEmpty;
                };

                ExecuteTest(method, "IsEmpty.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.IsReadOnly;
                };

                ExecuteTest(method, "IsReadOnly.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.IsFixedSize;
                };

                ExecuteTest(method, "IsFixedSize.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.IsNaN;
                };

                ExecuteTest(method, "IsNaN.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.IsInfinity;
                };

                ExecuteTest(method, "IsInfinity.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.IsNaNOrInfinity;
                };

                ExecuteTest(method, "IsNaNOrInfinity.get()");
            }

            // 模

#if ComVer1905
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.Module;
                };

                ExecuteTest(method, "Module.get()");
            }
#else
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.VectorModule;
                };

                ExecuteTest(method, "VectorModule.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.ModuleSquared;
                };

                ExecuteTest(method, "ModuleSquared.get()");
            }
#else
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.VectorModuleSquared;
                };

                ExecuteTest(method, "VectorModuleSquared.get()");
            }
#endif

            // 向量

#if ComVer1905
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.Opposite;
                };

                ExecuteTest(method, "Opposite.get()");
            }
#else
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.VectorNegate;
                };

                ExecuteTest(method, "VectorNegate.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.Normalize;
                };

                ExecuteTest(method, "Normalize.get()");
            }
#else
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.VectorNormalize;
                };

                ExecuteTest(method, "VectorNormalize.get()");
            }
#endif

            // 子空间分量

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.XYZ;
                };

                ExecuteTest(method, "XYZ.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD3D value = _GetRandomPointD3D();

                Action method = () =>
                {
                    pointD4D.XYZ = value;
                };

                ExecuteTest(method, "XYZ.set(Com.PointD3D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.YZU;
                };

                ExecuteTest(method, "YZU.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD3D value = _GetRandomPointD3D();

                Action method = () =>
                {
                    pointD4D.YZU = value;
                };

                ExecuteTest(method, "YZU.set(Com.PointD3D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.ZUX;
                };

                ExecuteTest(method, "ZUX.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD3D value = _GetRandomPointD3D();

                Action method = () =>
                {
                    pointD4D.ZUX = value;
                };

                ExecuteTest(method, "ZUX.set(Com.PointD3D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.UXY;
                };

                ExecuteTest(method, "UXY.get()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD3D value = _GetRandomPointD3D();

                Action method = () =>
                {
                    pointD4D.UXY = value;
                };

                ExecuteTest(method, "UXY.set(Com.PointD3D)");
            }

            // 角度

#if ComVer1905
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.AngleFromX;
                };

                ExecuteTest(method, "AngleFromX.get()");
            }
#else
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.AngleX;
                };

                ExecuteTest(method, "AngleX.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.AngleFromY;
                };

                ExecuteTest(method, "AngleFromY.get()");
            }
#else
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.AngleY;
                };

                ExecuteTest(method, "AngleY.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.AngleFromZ;
                };

                ExecuteTest(method, "AngleFromZ.get()");
            }
#else
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.AngleZ;
                };

                ExecuteTest(method, "AngleZ.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.AngleFromU;
                };

                ExecuteTest(method, "AngleFromU.get()");
            }
#else
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.AngleU;
                };

                ExecuteTest(method, "AngleU.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.AngleFromXYZ;
                };

                ExecuteTest(method, "AngleFromXYZ.get()");
            }
#else
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.AngleXYZ;
                };

                ExecuteTest(method, "AngleXYZ.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.AngleFromYZU;
                };

                ExecuteTest(method, "AngleFromYZU.get()");
            }
#else
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.AngleYZU;
                };

                ExecuteTest(method, "AngleYZU.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.AngleFromZUX;
                };

                ExecuteTest(method, "AngleFromZUX.get()");
            }
#else
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.AngleZUX;
                };

                ExecuteTest(method, "AngleZUX.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.AngleFromUXY;
                };

                ExecuteTest(method, "AngleFromUXY.get()");
            }
#else
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.AngleUXY;
                };

                ExecuteTest(method, "AngleUXY.get()");
            }
#endif
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

                ExecuteTest(method, "Equals(object)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.GetHashCode();
                };

                ExecuteTest(method, "GetHashCode()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.ToString();
                };

                ExecuteTest(method, "ToString()");
            }

            // Equals

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = left;

                Action method = () =>
                {
                    _ = left.Equals(right);
                };

                ExecuteTest(method, "Equals(Com.PointD4D)");
            }

            // CompareTo

#if ComVer1905
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                object obj = pointD4D;

                Action method = () =>
                {
                    _ = pointD4D.CompareTo(obj);
                };

                ExecuteTest(method, "CompareTo(object)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = left;

                Action method = () =>
                {
                    _ = left.CompareTo(right);
                };

                ExecuteTest(method, "CompareTo(Com.PointD4D)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // 检索

#if ComVer1905
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD4D.IndexOf(item);
                };

                ExecuteTest(method, "IndexOf(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 0;

                Action method = () =>
                {
                    _ = pointD4D.IndexOf(item, startIndex);
                };

                ExecuteTest(method, "IndexOf(double, int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 0;
                int count = 4;

                Action method = () =>
                {
                    _ = pointD4D.IndexOf(item, startIndex, count);
                };

                ExecuteTest(method, "IndexOf(double, int, int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD4D.LastIndexOf(item);
                };

                ExecuteTest(method, "LastIndexOf(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 3;

                Action method = () =>
                {
                    _ = pointD4D.LastIndexOf(item, startIndex);
                };

                ExecuteTest(method, "LastIndexOf(double, int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 3;
                int count = 4;

                Action method = () =>
                {
                    _ = pointD4D.LastIndexOf(item, startIndex, count);
                };

                ExecuteTest(method, "LastIndexOf(double, int, int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD4D.Contains(item);
                };

                ExecuteTest(method, "Contains(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // ToArray，ToList

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.ToArray();
                };

                ExecuteTest(method, "ToArray()");
            }

#if ComVer1905
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.ToList();
                };

                ExecuteTest(method, "ToList()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // 坐标系转换

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.ToSpherical();
                };

                ExecuteTest(method, "ToSpherical()");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.ToCartesian();
                };

                ExecuteTest(method, "ToCartesian()");
            }

            // 距离与夹角

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD4D pt = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.DistanceFrom(pt);
                };

                ExecuteTest(method, "DistanceFrom(Com.PointD4D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD4D pt = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.AngleFrom(pt);
                };

                ExecuteTest(method, "AngleFrom(Com.PointD4D)");
            }

            // Offset

#if ComVerNext
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                int index = Com.Statistics.RandomInteger(4);
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD4D.Offset(index, d);
                };

                ExecuteTest(method, "Offset(int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD4D.Offset(d);
                };

                ExecuteTest(method, "Offset(double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD4D pt = _GetRandomPointD4D();

                Action method = () =>
                {
                    pointD4D.Offset(pt);
                };

                ExecuteTest(method, "Offset(Com.PointD4D)");
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

                ExecuteTest(method, "Offset(double, double, double, double)");
            }

#if ComVerNext
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                int index = Com.Statistics.RandomInteger(4);
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD4D.OffsetCopy(index, d);
                };

                ExecuteTest(method, "OffsetCopy(int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD4D.OffsetCopy(d);
                };

                ExecuteTest(method, "OffsetCopy(double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD4D pt = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.OffsetCopy(pt);
                };

                ExecuteTest(method, "OffsetCopy(Com.PointD4D)");
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

                ExecuteTest(method, "OffsetCopy(double, double, double, double)");
            }

            // Scale

#if ComVerNext
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                int index = Com.Statistics.RandomInteger(4);
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD4D.Scale(index, s);
                };

                ExecuteTest(method, "Scale(int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD4D.Scale(s);
                };

                ExecuteTest(method, "Scale(double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD4D pt = _GetRandomPointD4D();

                Action method = () =>
                {
                    pointD4D.Scale(pt);
                };

                ExecuteTest(method, "Scale(Com.PointD4D)");
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

                ExecuteTest(method, "Scale(double, double, double, double)");
            }

#if ComVerNext
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                int index = Com.Statistics.RandomInteger(4);
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD4D.ScaleCopy(index, s);
                };

                ExecuteTest(method, "ScaleCopy(int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD4D.ScaleCopy(s);
                };

                ExecuteTest(method, "ScaleCopy(double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD4D pt = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.ScaleCopy(pt);
                };

                ExecuteTest(method, "ScaleCopy(Com.PointD4D)");
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

                ExecuteTest(method, "ScaleCopy(double, double, double, double)");
            }

            // Reflect

#if ComVer1905
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                int index = Com.Statistics.RandomInteger(4);

                Action method = () =>
                {
                    pointD4D.Reflect(index);
                };

                ExecuteTest(method, "Reflect(int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                int index = Com.Statistics.RandomInteger(4);

                Action method = () =>
                {
                    _ = pointD4D.ReflectCopy(index);
                };

                ExecuteTest(method, "ReflectCopy(int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // Shear

#if ComVer1905
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

                ExecuteTest(method, "Shear(int, int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
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

                ExecuteTest(method, "ShearCopy(int, int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // Rotate

#if ComVer1905
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

                ExecuteTest(method, "Rotate(int, int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
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

                ExecuteTest(method, "RotateCopy(int, int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // Affine

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD4D ex = _GetRandomNormalPointD4D();
                Com.PointD4D ey = _GetRandomNormalPointD4D();
                Com.PointD4D ez = _GetRandomNormalPointD4D();
                Com.PointD4D eu = _GetRandomNormalPointD4D();
                Com.PointD4D offset = _GetRandomPointD4D();

                Action method = () =>
                {
                    pointD4D.AffineTransform(ex, ey, ez, eu, offset);
                };

                ExecuteTest(method, "AffineTransform(Com.PointD4D, Com.PointD4D, Com.PointD4D, Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.Matrix matrixLeft = _GetRandomMatrix(5, 5);

                Action method = () =>
                {
                    pointD4D.AffineTransform(matrixLeft);
                };

                ExecuteTest(method, "AffineTransform(Com.Matrix)");
            }

#if ComVer1910
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(5, 5));
                }

                Com.Matrix[] matricesLeft = matrixLeftList.ToArray();

                Action method = () =>
                {
                    pointD4D.AffineTransform(matricesLeft);
                };

                ExecuteTest(method, "AffineTransform(param Com.Matrix[])", "total 8 matrices");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

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

                ExecuteTest(method, "AffineTransform(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD4D ex = _GetRandomNormalPointD4D();
                Com.PointD4D ey = _GetRandomNormalPointD4D();
                Com.PointD4D ez = _GetRandomNormalPointD4D();
                Com.PointD4D eu = _GetRandomNormalPointD4D();
                Com.PointD4D offset = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.AffineTransformCopy(ex, ey, ez, eu, offset);
                };

                ExecuteTest(method, "AffineTransformCopy(Com.PointD4D, Com.PointD4D, Com.PointD4D, Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.Matrix matrixLeft = _GetRandomMatrix(5, 5);

                Action method = () =>
                {
                    _ = pointD4D.AffineTransformCopy(matrixLeft);
                };

                ExecuteTest(method, "AffineTransformCopy(Com.Matrix)");
            }

#if ComVer1910
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(5, 5));
                }

                Com.Matrix[] matricesLeft = matrixLeftList.ToArray();

                Action method = () =>
                {
                    _ = pointD4D.AffineTransformCopy(matricesLeft);
                };

                ExecuteTest(method, "AffineTransformCopy(param Com.Matrix[])", "total 8 matrices");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

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

                ExecuteTest(method, "AffineTransformCopy(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD4D ex = _GetRandomNormalPointD4D();
                Com.PointD4D ey = _GetRandomNormalPointD4D();
                Com.PointD4D ez = _GetRandomNormalPointD4D();
                Com.PointD4D eu = _GetRandomNormalPointD4D();
                Com.PointD4D offset = _GetRandomPointD4D();

                Action method = () =>
                {
                    pointD4D.InverseAffineTransform(ex, ey, ez, eu, offset);
                };

                ExecuteTest(method, "InverseAffineTransform(Com.PointD4D, Com.PointD4D, Com.PointD4D, Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.Matrix matrixLeft = _GetRandomMatrix(5, 5);

                Action method = () =>
                {
                    pointD4D.InverseAffineTransform(matrixLeft);
                };

                ExecuteTest(method, "InverseAffineTransform(Com.Matrix)");
            }

#if ComVer1910
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(5, 5));
                }

                Com.Matrix[] matricesLeft = matrixLeftList.ToArray();

                Action method = () =>
                {
                    pointD4D.InverseAffineTransform(matricesLeft);
                };

                ExecuteTest(method, "InverseAffineTransform(param Com.Matrix[])", "total 8 matrices");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

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

                ExecuteTest(method, "InverseAffineTransform(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD4D ex = _GetRandomNormalPointD4D();
                Com.PointD4D ey = _GetRandomNormalPointD4D();
                Com.PointD4D ez = _GetRandomNormalPointD4D();
                Com.PointD4D eu = _GetRandomNormalPointD4D();
                Com.PointD4D offset = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.InverseAffineTransformCopy(ex, ey, ez, eu, offset);
                };

                ExecuteTest(method, "InverseAffineTransformCopy(Com.PointD4D, Com.PointD4D, Com.PointD4D, Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.Matrix matrixLeft = _GetRandomMatrix(5, 5);

                Action method = () =>
                {
                    _ = pointD4D.InverseAffineTransformCopy(matrixLeft);
                };

                ExecuteTest(method, "InverseAffineTransformCopy(Com.Matrix)");
            }

#if ComVer1910
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(5, 5));
                }

                Com.Matrix[] matricesLeft = matrixLeftList.ToArray();

                Action method = () =>
                {
                    _ = pointD4D.InverseAffineTransformCopy(matricesLeft);
                };

                ExecuteTest(method, "InverseAffineTransformCopy(param Com.Matrix[])", "total 8 matrices");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

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

                ExecuteTest(method, "InverseAffineTransformCopy(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            // Project

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD4D prjCenter = _GetRandomPointD4D();
                double focalLength = (pointD4D.U - prjCenter.U) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    _ = pointD4D.ProjectToXYZ(prjCenter, focalLength);
                };

                ExecuteTest(method, "ProjectToXYZ(Com.PointD4D, double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD4D prjCenter = _GetRandomPointD4D();
                double focalLength = (pointD4D.X - prjCenter.X) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    _ = pointD4D.ProjectToYZU(prjCenter, focalLength);
                };

                ExecuteTest(method, "ProjectToYZU(Com.PointD4D, double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD4D prjCenter = _GetRandomPointD4D();
                double focalLength = (pointD4D.Y - prjCenter.Y) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    _ = pointD4D.ProjectToZUX(prjCenter, focalLength);
                };

                ExecuteTest(method, "ProjectToZUX(Com.PointD4D, double)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();
                Com.PointD4D prjCenter = _GetRandomPointD4D();
                double focalLength = (pointD4D.Z - prjCenter.Z) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    _ = pointD4D.ProjectToUXY(prjCenter, focalLength);
                };

                ExecuteTest(method, "ProjectToUXY(Com.PointD4D, double)");
            }

            // ToVector

#if ComVer1905
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.ToColumnVector();
                };

                ExecuteTest(method, "ToColumnVector()");
            }
#else
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.ToVectorColumn();
                };

                ExecuteTest(method, "ToVectorColumn()");
            }
#endif

#if ComVer1905
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.ToRowVector();
                };

                ExecuteTest(method, "ToRowVector()");
            }
#else
            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = pointD4D.ToVectorRow();
                };

                ExecuteTest(method, "ToVectorRow()");
            }
#endif
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

                ExecuteTest(method, "Equals(Com.PointD4D, Com.PointD4D)");
            }

            // Compare

#if ComVer1905
            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = left;

                Action method = () =>
                {
                    _ = Com.PointD4D.Compare(left, right);
                };

                ExecuteTest(method, "Compare(Com.PointD4D, Com.PointD4D)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // From

#if ComVer1910
            {
                Com.Vector vector = _GetRandomPointD4D().ToColumnVector();

                Action method = () =>
                {
                    _ = Com.PointD4D.FromVector(vector);
                };

                ExecuteTest(method, "FromVector(Com.Vector)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

            // Matrix

            {
                Action method = () =>
                {
                    _ = Com.PointD4D.IdentityMatrix();
                };

                ExecuteTest(method, "IdentityMatrix()");
            }

#if ComVerNext
            {
                int index = Com.Statistics.RandomInteger(4);
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD4D.OffsetMatrix(index, d);
                };

                ExecuteTest(method, "OffsetMatrix(int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD4D.OffsetMatrix(d);
                };

                ExecuteTest(method, "OffsetMatrix(double)");
            }

            {
                Com.PointD4D pt = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = Com.PointD4D.OffsetMatrix(pt);
                };

                ExecuteTest(method, "OffsetMatrix(Com.PointD4D)");
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

                ExecuteTest(method, "OffsetMatrix(double, double, double, double)");
            }

#if ComVerNext
            {
                int index = Com.Statistics.RandomInteger(4);
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD4D.ScaleMatrix(index, s);
                };

                ExecuteTest(method, "ScaleMatrix(int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD4D.ScaleMatrix(s);
                };

                ExecuteTest(method, "ScaleMatrix(double)");
            }

            {
                Com.PointD4D pt = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = Com.PointD4D.ScaleMatrix(pt);
                };

                ExecuteTest(method, "ScaleMatrix(Com.PointD4D)");
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

                ExecuteTest(method, "ScaleMatrix(double, double, double, double)");
            }

#if ComVer1905
            {
                int index = Com.Statistics.RandomInteger(3);

                Action method = () =>
                {
                    _ = Com.PointD4D.ReflectMatrix(index);
                };

                ExecuteTest(method, "ReflectMatrix(int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                int index1 = Com.Statistics.RandomInteger(4);
                int index2 = Com.Statistics.RandomInteger(3);
                if (index2 == index1) index2++;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD4D.ShearMatrix(index1, index2, angle);
                };

                ExecuteTest(method, "ShearMatrix(int, int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                int index1 = Com.Statistics.RandomInteger(4);
                int index2 = Com.Statistics.RandomInteger(3);
                if (index2 == index1) index2++;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD4D.RotateMatrix(index1, index2, angle);
                };

                ExecuteTest(method, "RotateMatrix(int, int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // 距离与夹角

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = Com.PointD4D.DistanceBetween(left, right);
                };

                ExecuteTest(method, "DistanceBetween(Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = Com.PointD4D.AngleBetween(left, right);
                };

                ExecuteTest(method, "AngleBetween(Com.PointD4D, Com.PointD4D)");
            }

            // 向量乘积

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = Com.PointD4D.DotProduct(left, right);
                };

                ExecuteTest(method, "DotProduct(Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = Com.PointD4D.CrossProduct(left, right);
                };

                ExecuteTest(method, "CrossProduct(Com.PointD4D, Com.PointD4D)");
            }

            // 初等函数

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = Com.PointD4D.Abs(pointD4D);
                };

                ExecuteTest(method, "Abs(Com.PointD4D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = Com.PointD4D.Sign(pointD4D);
                };

                ExecuteTest(method, "Sign(Com.PointD4D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = Com.PointD4D.Ceiling(pointD4D);
                };

                ExecuteTest(method, "Ceiling(Com.PointD4D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = Com.PointD4D.Floor(pointD4D);
                };

                ExecuteTest(method, "Floor(Com.PointD4D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = Com.PointD4D.Round(pointD4D);
                };

                ExecuteTest(method, "Round(Com.PointD4D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = Com.PointD4D.Truncate(pointD4D);
                };

                ExecuteTest(method, "Truncate(Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = Com.PointD4D.Max(left, right);
                };

                ExecuteTest(method, "Max(Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = Com.PointD4D.Min(left, right);
                };

                ExecuteTest(method, "Min(Com.PointD4D, Com.PointD4D)");
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

                ExecuteTest(method, "operator ==(Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = left;

                Action method = () =>
                {
                    _ = (left != right);
                };

                ExecuteTest(method, "operator !=(Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = (left < right);
                };

                ExecuteTest(method, "operator <(Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = (left > right);
                };

                ExecuteTest(method, "operator >(Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = (left <= right);
                };

                ExecuteTest(method, "operator <=(Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = (left >= right);
                };

                ExecuteTest(method, "operator >=(Com.PointD4D, Com.PointD4D)");
            }

            // 运算

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = +pointD4D;
                };

                ExecuteTest(method, "operator +(Com.PointD4D)");
            }

            {
                Com.PointD4D pointD4D = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = -pointD4D;
                };

                ExecuteTest(method, "operator -(Com.PointD4D)");
            }

            {
                Com.PointD4D pt = _GetRandomPointD4D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt + n;
                };

                ExecuteTest(method, "operator +(Com.PointD4D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD4D pt = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = n + pt;
                };

                ExecuteTest(method, "operator +(double, Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = left + right;
                };

                ExecuteTest(method, "operator +(Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D pt = _GetRandomPointD4D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt - n;
                };

                ExecuteTest(method, "operator -(Com.PointD4D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD4D pt = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = n - pt;
                };

                ExecuteTest(method, "operator -(double, Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = left - right;
                };

                ExecuteTest(method, "operator -(Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D pt = _GetRandomPointD4D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt * n;
                };

                ExecuteTest(method, "operator *(Com.PointD4D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD4D pt = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = n * pt;
                };

                ExecuteTest(method, "operator *(double, Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = left * right;
                };

                ExecuteTest(method, "operator *(Com.PointD4D, Com.PointD4D)");
            }

            {
                Com.PointD4D pt = _GetRandomPointD4D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt / n;
                };

                ExecuteTest(method, "operator /(Com.PointD4D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD4D pt = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = n / pt;
                };

                ExecuteTest(method, "operator /(double, Com.PointD4D)");
            }

            {
                Com.PointD4D left = _GetRandomPointD4D();
                Com.PointD4D right = _GetRandomPointD4D();

                Action method = () =>
                {
                    _ = left / right;
                };

                ExecuteTest(method, "operator /(Com.PointD4D, Com.PointD4D)");
            }

            // 类型转换

#if ComVerNext
            {
                Com.PointD4D pt = _GetRandomPointD4D();
                (double, double, double, double) tuple = (pt.X, pt.Y, pt.Z, pt.U);

                Action method = () =>
                {
                    _ = (Com.PointD4D)tuple;
                };

                ExecuteTest(method, "implicit operator Com.PointD4D(System.ValueTuple<double, double, double, double>)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif
        }
    }

    sealed class PointD5DTest : ClassPerfTestBase
    {
        private const string _NamespaceName = "Com";
        private const string _ClassName = "PointD5D";

        private void ExecuteTest(Action method, string methodName, string comment)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName, comment);
        }

        private void ExecuteTest(Action method, string methodName)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(string methodName, UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName, unsupportedReason);
        }

        private void ExecuteTest(string methodName)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, string.Empty, unsupportedReason);
        }

        //

        private static Com.PointD5D _GetRandomPointD5D()
        {
            return new Com.PointD5D(Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18));
        }

        private static Com.PointD5D _GetRandomNormalPointD5D()
        {
#if ComVer1905
            return _GetRandomPointD5D().Normalize;
#else
            return _GetRandomPointD5D().VectorNormalize;
#endif
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
                        matrix[x, y] = Com.Statistics.RandomDouble(-10, 10);
                    }
                }

                return matrix;
            }
            else
            {
#if ComVer1905
                return Com.Matrix.Empty;
#else
                return Com.Matrix.NonMatrix;
#endif
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

                ExecuteTest(method, "PointD5D(double, double, double, double, double)");
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

                ExecuteTest(method, "this[int].get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                int index = Com.Statistics.RandomInteger(5);
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD5D[index] = value;
                };

                ExecuteTest(method, "this[int].set(double)");
            }

            // 分量

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.X;
                };

                ExecuteTest(method, "X.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD5D.X = value;
                };

                ExecuteTest(method, "X.set(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.Y;
                };

                ExecuteTest(method, "Y.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD5D.Y = value;
                };

                ExecuteTest(method, "Y.set(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.Z;
                };

                ExecuteTest(method, "Z.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD5D.Z = value;
                };

                ExecuteTest(method, "Z.set(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.U;
                };

                ExecuteTest(method, "U.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD5D.U = value;
                };

                ExecuteTest(method, "U.set(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.V;
                };

                ExecuteTest(method, "V.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD5D.V = value;
                };

                ExecuteTest(method, "V.set(double)");
            }

            // Dimension

#if ComVer1905
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.Dimension;
                };

                ExecuteTest(method, "Dimension.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // Is

#if ComVer1905
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.IsEmpty;
                };

                ExecuteTest(method, "IsEmpty.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.IsZero;
                };

                ExecuteTest(method, "IsZero.get()");
            }
#else
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.IsEmpty;
                };

                ExecuteTest(method, "IsEmpty.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.IsReadOnly;
                };

                ExecuteTest(method, "IsReadOnly.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.IsFixedSize;
                };

                ExecuteTest(method, "IsFixedSize.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.IsNaN;
                };

                ExecuteTest(method, "IsNaN.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.IsInfinity;
                };

                ExecuteTest(method, "IsInfinity.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.IsNaNOrInfinity;
                };

                ExecuteTest(method, "IsNaNOrInfinity.get()");
            }

            // 模

#if ComVer1905
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.Module;
                };

                ExecuteTest(method, "Module.get()");
            }
#else
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.VectorModule;
                };

                ExecuteTest(method, "VectorModule.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.ModuleSquared;
                };

                ExecuteTest(method, "ModuleSquared.get()");
            }
#else
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.VectorModuleSquared;
                };

                ExecuteTest(method, "VectorModuleSquared.get()");
            }
#endif

            // 向量

#if ComVer1905
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.Opposite;
                };

                ExecuteTest(method, "Opposite.get()");
            }
#else
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.VectorNegate;
                };

                ExecuteTest(method, "VectorNegate.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.Normalize;
                };

                ExecuteTest(method, "Normalize.get()");
            }
#else
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.VectorNormalize;
                };

                ExecuteTest(method, "VectorNormalize.get()");
            }
#endif

            // 子空间分量

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.XYZU;
                };

                ExecuteTest(method, "XYZU.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD4D value = _GetRandomPointD4D();

                Action method = () =>
                {
                    pointD5D.XYZU = value;
                };

                ExecuteTest(method, "XYZU.set(Com.PointD4D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.YZUV;
                };

                ExecuteTest(method, "YZUV.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD4D value = _GetRandomPointD4D();

                Action method = () =>
                {
                    pointD5D.YZUV = value;
                };

                ExecuteTest(method, "YZUV.set(Com.PointD4D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.ZUVX;
                };

                ExecuteTest(method, "ZUVX.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD4D value = _GetRandomPointD4D();

                Action method = () =>
                {
                    pointD5D.ZUVX = value;
                };

                ExecuteTest(method, "ZUVX.set(Com.PointD4D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.UVXY;
                };

                ExecuteTest(method, "UVXY.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD4D value = _GetRandomPointD4D();

                Action method = () =>
                {
                    pointD5D.UVXY = value;
                };

                ExecuteTest(method, "UVXY.set(Com.PointD4D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.VXYZ;
                };

                ExecuteTest(method, "VXYZ.get()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD4D value = _GetRandomPointD4D();

                Action method = () =>
                {
                    pointD5D.VXYZ = value;
                };

                ExecuteTest(method, "VXYZ.set(Com.PointD4D)");
            }

            // 角度

#if ComVer1905
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.AngleFromX;
                };

                ExecuteTest(method, "AngleFromX.get()");
            }
#else
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.AngleX;
                };

                ExecuteTest(method, "AngleX.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.AngleFromY;
                };

                ExecuteTest(method, "AngleFromY.get()");
            }
#else
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.AngleY;
                };

                ExecuteTest(method, "AngleY.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.AngleFromZ;
                };

                ExecuteTest(method, "AngleFromZ.get()");
            }
#else
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.AngleZ;
                };

                ExecuteTest(method, "AngleZ.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.AngleFromU;
                };

                ExecuteTest(method, "AngleFromU.get()");
            }
#else
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.AngleU;
                };

                ExecuteTest(method, "AngleU.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.AngleFromV;
                };

                ExecuteTest(method, "AngleFromV.get()");
            }
#else
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.AngleV;
                };

                ExecuteTest(method, "AngleV.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.AngleFromXYZU;
                };

                ExecuteTest(method, "AngleFromXYZU.get()");
            }
#else
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.AngleXYZU;
                };

                ExecuteTest(method, "AngleXYZU.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.AngleFromYZUV;
                };

                ExecuteTest(method, "AngleFromYZUV.get()");
            }
#else
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.AngleYZUV;
                };

                ExecuteTest(method, "AngleYZUV.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.AngleFromZUVX;
                };

                ExecuteTest(method, "AngleFromZUVX.get()");
            }
#else
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.AngleZUVX;
                };

                ExecuteTest(method, "AngleZUVX.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.AngleFromUVXY;
                };

                ExecuteTest(method, "AngleFromUVXY.get()");
            }
#else
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.AngleUVXY;
                };

                ExecuteTest(method, "AngleUVXY.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.AngleFromVXYZ;
                };

                ExecuteTest(method, "AngleFromVXYZ.get()");
            }
#else
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.AngleVXYZ;
                };

                ExecuteTest(method, "AngleVXYZ.get()");
            }
#endif
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

                ExecuteTest(method, "Equals(object)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.GetHashCode();
                };

                ExecuteTest(method, "GetHashCode()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.ToString();
                };

                ExecuteTest(method, "ToString()");
            }

            // Equals

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = left;

                Action method = () =>
                {
                    _ = left.Equals(right);
                };

                ExecuteTest(method, "Equals(Com.PointD5D)");
            }

            // CompareTo

#if ComVer1905
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                object obj = pointD5D;

                Action method = () =>
                {
                    _ = pointD5D.CompareTo(obj);
                };

                ExecuteTest(method, "CompareTo(object)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = left;

                Action method = () =>
                {
                    _ = left.CompareTo(right);
                };

                ExecuteTest(method, "CompareTo(Com.PointD5D)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // 检索

#if ComVer1905
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD5D.IndexOf(item);
                };

                ExecuteTest(method, "IndexOf(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 0;

                Action method = () =>
                {
                    _ = pointD5D.IndexOf(item, startIndex);
                };

                ExecuteTest(method, "IndexOf(double, int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 0;
                int count = 5;

                Action method = () =>
                {
                    _ = pointD5D.IndexOf(item, startIndex, count);
                };

                ExecuteTest(method, "IndexOf(double, int, int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD5D.LastIndexOf(item);
                };

                ExecuteTest(method, "LastIndexOf(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 4;

                Action method = () =>
                {
                    _ = pointD5D.LastIndexOf(item, startIndex);
                };

                ExecuteTest(method, "LastIndexOf(double, int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 4;
                int count = 5;

                Action method = () =>
                {
                    _ = pointD5D.LastIndexOf(item, startIndex, count);
                };

                ExecuteTest(method, "LastIndexOf(double, int, int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD5D.Contains(item);
                };

                ExecuteTest(method, "Contains(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // ToArray，ToList

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.ToArray();
                };

                ExecuteTest(method, "ToArray()");
            }

#if ComVer1905
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.ToList();
                };

                ExecuteTest(method, "ToList()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // 坐标系转换

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.ToSpherical();
                };

                ExecuteTest(method, "ToSpherical()");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.ToCartesian();
                };

                ExecuteTest(method, "ToCartesian()");
            }

            // 距离与夹角

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D pt = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.DistanceFrom(pt);
                };

                ExecuteTest(method, "DistanceFrom(Com.PointD5D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D pt = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.AngleFrom(pt);
                };

                ExecuteTest(method, "AngleFrom(Com.PointD5D)");
            }

            // Offset

#if ComVerNext
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                int index = Com.Statistics.RandomInteger(5);
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD5D.Offset(index, d);
                };

                ExecuteTest(method, "Offset(int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD5D.Offset(d);
                };

                ExecuteTest(method, "Offset(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D pt = _GetRandomPointD5D();

                Action method = () =>
                {
                    pointD5D.Offset(pt);
                };

                ExecuteTest(method, "Offset(Com.PointD5D)");
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

                ExecuteTest(method, "Offset(double, double, double, double, double)");
            }

#if ComVerNext
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                int index = Com.Statistics.RandomInteger(5);
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD5D.OffsetCopy(index, d);
                };

                ExecuteTest(method, "OffsetCopy(int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD5D.OffsetCopy(d);
                };

                ExecuteTest(method, "OffsetCopy(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D pt = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.OffsetCopy(pt);
                };

                ExecuteTest(method, "OffsetCopy(Com.PointD5D)");
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

                ExecuteTest(method, "OffsetCopy(double, double, double, double, double)");
            }

            // Scale

#if ComVerNext
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                int index = Com.Statistics.RandomInteger(5);
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD5D.Scale(index, s);
                };

                ExecuteTest(method, "Scale(int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD5D.Scale(s);
                };

                ExecuteTest(method, "Scale(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D pt = _GetRandomPointD5D();

                Action method = () =>
                {
                    pointD5D.Scale(pt);
                };

                ExecuteTest(method, "Scale(Com.PointD5D)");
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

                ExecuteTest(method, "Scale(double, double, double, double, double)");
            }

#if ComVerNext
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                int index = Com.Statistics.RandomInteger(5);
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD5D.ScaleCopy(index, s);
                };

                ExecuteTest(method, "ScaleCopy(int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD5D.ScaleCopy(s);
                };

                ExecuteTest(method, "ScaleCopy(double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D pt = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.ScaleCopy(pt);
                };

                ExecuteTest(method, "ScaleCopy(Com.PointD5D)");
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

                ExecuteTest(method, "ScaleCopy(double, double, double, double, double)");
            }

            // Reflect

#if ComVer1905
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                int index = Com.Statistics.RandomInteger(5);

                Action method = () =>
                {
                    pointD5D.Reflect(index);
                };

                ExecuteTest(method, "Reflect(int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                int index = Com.Statistics.RandomInteger(5);

                Action method = () =>
                {
                    _ = pointD5D.ReflectCopy(index);
                };

                ExecuteTest(method, "ReflectCopy(int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // Shear

#if ComVer1905
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

                ExecuteTest(method, "Shear(int, int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
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

                ExecuteTest(method, "ShearCopy(int, int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // Rotate

#if ComVer1905
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

                ExecuteTest(method, "Rotate(int, int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
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

                ExecuteTest(method, "RotateCopy(int, int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // Affine

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D ex = _GetRandomNormalPointD5D();
                Com.PointD5D ey = _GetRandomNormalPointD5D();
                Com.PointD5D ez = _GetRandomNormalPointD5D();
                Com.PointD5D eu = _GetRandomNormalPointD5D();
                Com.PointD5D ev = _GetRandomNormalPointD5D();
                Com.PointD5D offset = _GetRandomPointD5D();

                Action method = () =>
                {
                    pointD5D.AffineTransform(ex, ey, ez, eu, ev, offset);
                };

                ExecuteTest(method, "AffineTransform(Com.PointD5D, Com.PointD5D, Com.PointD5D, Com.PointD5D, Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.Matrix matrixLeft = _GetRandomMatrix(6, 6);

                Action method = () =>
                {
                    pointD5D.AffineTransform(matrixLeft);
                };

                ExecuteTest(method, "AffineTransform(Com.Matrix)");
            }

#if ComVer1910
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(6, 6));
                }

                Com.Matrix[] matricesLeft = matrixLeftList.ToArray();

                Action method = () =>
                {
                    pointD5D.AffineTransform(matricesLeft);
                };

                ExecuteTest(method, "AffineTransform(param Com.Matrix[])", "total 8 matrices");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

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

                ExecuteTest(method, "AffineTransform(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D ex = _GetRandomNormalPointD5D();
                Com.PointD5D ey = _GetRandomNormalPointD5D();
                Com.PointD5D ez = _GetRandomNormalPointD5D();
                Com.PointD5D eu = _GetRandomNormalPointD5D();
                Com.PointD5D ev = _GetRandomNormalPointD5D();
                Com.PointD5D offset = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.AffineTransformCopy(ex, ey, ez, eu, ev, offset);
                };

                ExecuteTest(method, "AffineTransformCopy(Com.PointD5D, Com.PointD5D, Com.PointD5D, Com.PointD5D, Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.Matrix matrixLeft = _GetRandomMatrix(6, 6);

                Action method = () =>
                {
                    _ = pointD5D.AffineTransformCopy(matrixLeft);
                };

                ExecuteTest(method, "AffineTransformCopy(Com.Matrix)");
            }

#if ComVer1910
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(6, 6));
                }

                Com.Matrix[] matricesLeft = matrixLeftList.ToArray();

                Action method = () =>
                {
                    _ = pointD5D.AffineTransformCopy(matricesLeft);
                };

                ExecuteTest(method, "AffineTransformCopy(param Com.Matrix[])", "total 8 matrices");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

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

                ExecuteTest(method, "AffineTransformCopy(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D ex = _GetRandomNormalPointD5D();
                Com.PointD5D ey = _GetRandomNormalPointD5D();
                Com.PointD5D ez = _GetRandomNormalPointD5D();
                Com.PointD5D eu = _GetRandomNormalPointD5D();
                Com.PointD5D ev = _GetRandomNormalPointD5D();
                Com.PointD5D offset = _GetRandomPointD5D();

                Action method = () =>
                {
                    pointD5D.InverseAffineTransform(ex, ey, ez, eu, ev, offset);
                };

                ExecuteTest(method, "InverseAffineTransform(Com.PointD5D, Com.PointD5D, Com.PointD5D, Com.PointD5D, Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.Matrix matrixLeft = _GetRandomMatrix(6, 6);

                Action method = () =>
                {
                    pointD5D.InverseAffineTransform(matrixLeft);
                };

                ExecuteTest(method, "InverseAffineTransform(Com.Matrix)");
            }

#if ComVer1910
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(6, 6));
                }

                Com.Matrix[] matricesLeft = matrixLeftList.ToArray();

                Action method = () =>
                {
                    pointD5D.InverseAffineTransform(matricesLeft);
                };

                ExecuteTest(method, "InverseAffineTransform(param Com.Matrix[])", "total 8 matrices");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

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

                ExecuteTest(method, "InverseAffineTransform(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D ex = _GetRandomNormalPointD5D();
                Com.PointD5D ey = _GetRandomNormalPointD5D();
                Com.PointD5D ez = _GetRandomNormalPointD5D();
                Com.PointD5D eu = _GetRandomNormalPointD5D();
                Com.PointD5D ev = _GetRandomNormalPointD5D();
                Com.PointD5D offset = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.InverseAffineTransformCopy(ex, ey, ez, eu, ev, offset);
                };

                ExecuteTest(method, "InverseAffineTransformCopy(Com.PointD5D, Com.PointD5D, Com.PointD5D, Com.PointD5D, Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.Matrix matrixLeft = _GetRandomMatrix(6, 6);

                Action method = () =>
                {
                    _ = pointD5D.InverseAffineTransformCopy(matrixLeft);
                };

                ExecuteTest(method, "InverseAffineTransformCopy(Com.Matrix)");
            }

#if ComVer1910
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(6, 6));
                }

                Com.Matrix[] matricesLeft = matrixLeftList.ToArray();

                Action method = () =>
                {
                    _ = pointD5D.InverseAffineTransformCopy(matricesLeft);
                };

                ExecuteTest(method, "InverseAffineTransformCopy(param Com.Matrix[])", "total 8 matrices");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

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

                ExecuteTest(method, "InverseAffineTransformCopy(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            // Project

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D prjCenter = _GetRandomPointD5D();
                double focalLength = (pointD5D.V - prjCenter.V) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    _ = pointD5D.ProjectToXYZU(prjCenter, focalLength);
                };

                ExecuteTest(method, "ProjectToXYZU(Com.PointD5D, double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D prjCenter = _GetRandomPointD5D();
                double focalLength = (pointD5D.X - prjCenter.X) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    _ = pointD5D.ProjectToYZUV(prjCenter, focalLength);
                };

                ExecuteTest(method, "ProjectToYZUV(Com.PointD5D, double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D prjCenter = _GetRandomPointD5D();
                double focalLength = (pointD5D.Y - prjCenter.Y) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    _ = pointD5D.ProjectToZUVX(prjCenter, focalLength);
                };

                ExecuteTest(method, "ProjectToZUVX(Com.PointD5D, double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D prjCenter = _GetRandomPointD5D();
                double focalLength = (pointD5D.Z - prjCenter.Z) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    _ = pointD5D.ProjectToUVXY(prjCenter, focalLength);
                };

                ExecuteTest(method, "ProjectToUVXY(Com.PointD5D, double)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();
                Com.PointD5D prjCenter = _GetRandomPointD5D();
                double focalLength = (pointD5D.U - prjCenter.U) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    _ = pointD5D.ProjectToVXYZ(prjCenter, focalLength);
                };

                ExecuteTest(method, "ProjectToVXYZ(Com.PointD5D, double)");
            }

            // ToVector

#if ComVer1905
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.ToColumnVector();
                };

                ExecuteTest(method, "ToColumnVector()");
            }
#else
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.ToVectorColumn();
                };

                ExecuteTest(method, "ToVectorColumn()");
            }
#endif

#if ComVer1905
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.ToRowVector();
                };

                ExecuteTest(method, "ToRowVector()");
            }
#else
            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = pointD5D.ToVectorRow();
                };

                ExecuteTest(method, "ToVectorRow()");
            }
#endif
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

                ExecuteTest(method, "Equals(Com.PointD5D, Com.PointD5D)");
            }

            // Compare

#if ComVer1905
            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = left;

                Action method = () =>
                {
                    _ = Com.PointD5D.Compare(left, right);
                };

                ExecuteTest(method, "Compare(Com.PointD5D, Com.PointD5D)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // From

#if ComVer1910
            {
                Com.Vector vector = _GetRandomPointD5D().ToColumnVector();

                Action method = () =>
                {
                    _ = Com.PointD5D.FromVector(vector);
                };

                ExecuteTest(method, "FromVector(Com.Vector)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

            // Matrix

            {
                Action method = () =>
                {
                    _ = Com.PointD5D.IdentityMatrix();
                };

                ExecuteTest(method, "IdentityMatrix()");
            }

#if ComVerNext
            {
                int index = Com.Statistics.RandomInteger(5);
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD5D.OffsetMatrix(index, d);
                };

                ExecuteTest(method, "OffsetMatrix(int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD5D.OffsetMatrix(d);
                };

                ExecuteTest(method, "OffsetMatrix(double)");
            }

            {
                Com.PointD5D pt = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = Com.PointD5D.OffsetMatrix(pt);
                };

                ExecuteTest(method, "OffsetMatrix(Com.PointD5D)");
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

                ExecuteTest(method, "OffsetMatrix(double, double, double, double, double)");
            }

#if ComVerNext
            {
                int index = Com.Statistics.RandomInteger(5);
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD5D.ScaleMatrix(index, s);
                };

                ExecuteTest(method, "ScaleMatrix(int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD5D.ScaleMatrix(s);
                };

                ExecuteTest(method, "ScaleMatrix(double)");
            }

            {
                Com.PointD5D pt = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = Com.PointD5D.ScaleMatrix(pt);
                };

                ExecuteTest(method, "ScaleMatrix(Com.PointD5D)");
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

                ExecuteTest(method, "ScaleMatrix(double, double, double, double, double)");
            }

#if ComVer1905
            {
                int index = Com.Statistics.RandomInteger(3);

                Action method = () =>
                {
                    _ = Com.PointD5D.ReflectMatrix(index);
                };

                ExecuteTest(method, "ReflectMatrix(int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                int index1 = Com.Statistics.RandomInteger(5);
                int index2 = Com.Statistics.RandomInteger(4);
                if (index2 == index1) index2++;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD5D.ShearMatrix(index1, index2, angle);
                };

                ExecuteTest(method, "ShearMatrix(int, int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                int index1 = Com.Statistics.RandomInteger(5);
                int index2 = Com.Statistics.RandomInteger(4);
                if (index2 == index1) index2++;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD5D.RotateMatrix(index1, index2, angle);
                };

                ExecuteTest(method, "RotateMatrix(int, int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // 距离与夹角

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = Com.PointD5D.DistanceBetween(left, right);
                };

                ExecuteTest(method, "DistanceBetween(Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = Com.PointD5D.AngleBetween(left, right);
                };

                ExecuteTest(method, "AngleBetween(Com.PointD5D, Com.PointD5D)");
            }

            // 向量乘积

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = Com.PointD5D.DotProduct(left, right);
                };

                ExecuteTest(method, "DotProduct(Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = Com.PointD5D.CrossProduct(left, right);
                };

                ExecuteTest(method, "CrossProduct(Com.PointD5D, Com.PointD5D)");
            }

            // 初等函数

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = Com.PointD5D.Abs(pointD5D);
                };

                ExecuteTest(method, "Abs(Com.PointD5D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = Com.PointD5D.Sign(pointD5D);
                };

                ExecuteTest(method, "Sign(Com.PointD5D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = Com.PointD5D.Ceiling(pointD5D);
                };

                ExecuteTest(method, "Ceiling(Com.PointD5D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = Com.PointD5D.Floor(pointD5D);
                };

                ExecuteTest(method, "Floor(Com.PointD5D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = Com.PointD5D.Round(pointD5D);
                };

                ExecuteTest(method, "Round(Com.PointD5D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = Com.PointD5D.Truncate(pointD5D);
                };

                ExecuteTest(method, "Truncate(Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = Com.PointD5D.Max(left, right);
                };

                ExecuteTest(method, "Max(Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = Com.PointD5D.Min(left, right);
                };

                ExecuteTest(method, "Min(Com.PointD5D, Com.PointD5D)");
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

                ExecuteTest(method, "operator ==(Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = left;

                Action method = () =>
                {
                    _ = (left != right);
                };

                ExecuteTest(method, "operator !=(Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = (left < right);
                };

                ExecuteTest(method, "operator <(Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = (left > right);
                };

                ExecuteTest(method, "operator >(Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = (left <= right);
                };

                ExecuteTest(method, "operator <=(Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = (left >= right);
                };

                ExecuteTest(method, "operator >=(Com.PointD5D, Com.PointD5D)");
            }

            // 运算

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = +pointD5D;
                };

                ExecuteTest(method, "operator +(Com.PointD5D)");
            }

            {
                Com.PointD5D pointD5D = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = -pointD5D;
                };

                ExecuteTest(method, "operator -(Com.PointD5D)");
            }

            {
                Com.PointD5D pt = _GetRandomPointD5D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt + n;
                };

                ExecuteTest(method, "operator +(Com.PointD5D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD5D pt = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = n + pt;
                };

                ExecuteTest(method, "operator +(double, Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = left + right;
                };

                ExecuteTest(method, "operator +(Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D pt = _GetRandomPointD5D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt - n;
                };

                ExecuteTest(method, "operator -(Com.PointD5D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD5D pt = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = n - pt;
                };

                ExecuteTest(method, "operator -(double, Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = left - right;
                };

                ExecuteTest(method, "operator -(Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D pt = _GetRandomPointD5D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt * n;
                };

                ExecuteTest(method, "operator *(Com.PointD5D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD5D pt = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = n * pt;
                };

                ExecuteTest(method, "operator *(double, Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = left * right;
                };

                ExecuteTest(method, "operator *(Com.PointD5D, Com.PointD5D)");
            }

            {
                Com.PointD5D pt = _GetRandomPointD5D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt / n;
                };

                ExecuteTest(method, "operator /(Com.PointD5D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD5D pt = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = n / pt;
                };

                ExecuteTest(method, "operator /(double, Com.PointD5D)");
            }

            {
                Com.PointD5D left = _GetRandomPointD5D();
                Com.PointD5D right = _GetRandomPointD5D();

                Action method = () =>
                {
                    _ = left / right;
                };

                ExecuteTest(method, "operator /(Com.PointD5D, Com.PointD5D)");
            }

            // 类型转换

#if ComVerNext
            {
                Com.PointD5D pt = _GetRandomPointD5D();
                (double, double, double, double, double) tuple = (pt.X, pt.Y, pt.Z, pt.U, pt.V);

                Action method = () =>
                {
                    _ = (Com.PointD5D)tuple;
                };

                ExecuteTest(method, "implicit operator Com.PointD5D(System.ValueTuple<double, double, double, double, double>)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif
        }
    }

    sealed class PointD6DTest : ClassPerfTestBase
    {
        private const string _NamespaceName = "Com";
        private const string _ClassName = "PointD6D";

        private void ExecuteTest(Action method, string methodName, string comment)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName, comment);
        }

        private void ExecuteTest(Action method, string methodName)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(string methodName, UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName, unsupportedReason);
        }

        private void ExecuteTest(string methodName)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, string.Empty, unsupportedReason);
        }

        //

        private static Com.PointD6D _GetRandomPointD6D()
        {
            return new Com.PointD6D(Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18), Com.Statistics.RandomDouble(-1E18, 1E18));
        }

        private static Com.PointD6D _GetRandomNormalPointD6D()
        {
#if ComVer1905
            return _GetRandomPointD6D().Normalize;
#else
            return _GetRandomPointD6D().VectorNormalize;
#endif
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
                        matrix[x, y] = Com.Statistics.RandomDouble(-10, 10);
                    }
                }

                return matrix;
            }
            else
            {
#if ComVer1905
                return Com.Matrix.Empty;
#else
                return Com.Matrix.NonMatrix;
#endif
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

                ExecuteTest(method, "PointD6D(double, double, double, double, double, double)");
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

                ExecuteTest(method, "this[int].get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                int index = Com.Statistics.RandomInteger(6);
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD6D[index] = value;
                };

                ExecuteTest(method, "this[int].set(double)");
            }

            // 分量

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.X;
                };

                ExecuteTest(method, "X.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD6D.X = value;
                };

                ExecuteTest(method, "X.set(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.Y;
                };

                ExecuteTest(method, "Y.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD6D.Y = value;
                };

                ExecuteTest(method, "Y.set(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.Z;
                };

                ExecuteTest(method, "Z.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD6D.Z = value;
                };

                ExecuteTest(method, "Z.set(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.U;
                };

                ExecuteTest(method, "U.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD6D.U = value;
                };

                ExecuteTest(method, "U.set(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.V;
                };

                ExecuteTest(method, "V.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD6D.V = value;
                };

                ExecuteTest(method, "V.set(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.W;
                };

                ExecuteTest(method, "W.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD6D.W = value;
                };

                ExecuteTest(method, "W.set(double)");
            }

            // Dimension

#if ComVer1905
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.Dimension;
                };

                ExecuteTest(method, "Dimension.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // Is

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.IsEmpty;
                };

                ExecuteTest(method, "IsEmpty.get()");
            }

#if ComVer1905
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.IsZero;
                };

                ExecuteTest(method, "IsZero.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.IsReadOnly;
                };

                ExecuteTest(method, "IsReadOnly.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.IsFixedSize;
                };

                ExecuteTest(method, "IsFixedSize.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.IsNaN;
                };

                ExecuteTest(method, "IsNaN.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.IsInfinity;
                };

                ExecuteTest(method, "IsInfinity.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.IsNaNOrInfinity;
                };

                ExecuteTest(method, "IsNaNOrInfinity.get()");
            }

            // 模

#if ComVer1905
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.Module;
                };

                ExecuteTest(method, "Module.get()");
            }
#else
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.VectorModule;
                };

                ExecuteTest(method, "VectorModule.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.ModuleSquared;
                };

                ExecuteTest(method, "ModuleSquared.get()");
            }
#else
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.VectorModuleSquared;
                };

                ExecuteTest(method, "VectorModuleSquared.get()");
            }
#endif

            // 向量

#if ComVer1905
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.Opposite;
                };

                ExecuteTest(method, "Opposite.get()");
            }
#else
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.VectorNegate;
                };

                ExecuteTest(method, "VectorNegate.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.Normalize;
                };

                ExecuteTest(method, "Normalize.get()");
            }
#else
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.VectorNormalize;
                };

                ExecuteTest(method, "VectorNormalize.get()");
            }
#endif

            // 子空间分量

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.XYZUV;
                };

                ExecuteTest(method, "XYZUV.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD5D value = _GetRandomPointD5D();

                Action method = () =>
                {
                    pointD6D.XYZUV = value;
                };

                ExecuteTest(method, "XYZUV.set(Com.PointD5D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.YZUVW;
                };

                ExecuteTest(method, "YZUVW.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD5D value = _GetRandomPointD5D();

                Action method = () =>
                {
                    pointD6D.YZUVW = value;
                };

                ExecuteTest(method, "YZUVW.set(Com.PointD5D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.ZUVWX;
                };

                ExecuteTest(method, "ZUVWX.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD5D value = _GetRandomPointD5D();

                Action method = () =>
                {
                    pointD6D.ZUVWX = value;
                };

                ExecuteTest(method, "ZUVWX.set(Com.PointD5D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.UVWXY;
                };

                ExecuteTest(method, "UVWXY.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD5D value = _GetRandomPointD5D();

                Action method = () =>
                {
                    pointD6D.UVWXY = value;
                };

                ExecuteTest(method, "UVWXY.set(Com.PointD5D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.VWXYZ;
                };

                ExecuteTest(method, "VWXYZ.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD5D value = _GetRandomPointD5D();

                Action method = () =>
                {
                    pointD6D.VWXYZ = value;
                };

                ExecuteTest(method, "VWXYZ.set(Com.PointD5D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.WXYZU;
                };

                ExecuteTest(method, "WXYZU.get()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD5D value = _GetRandomPointD5D();

                Action method = () =>
                {
                    pointD6D.WXYZU = value;
                };

                ExecuteTest(method, "WXYZU.set(Com.PointD5D)");
            }

            // 角度

#if ComVer1905
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AngleFromX;
                };

                ExecuteTest(method, "AngleFromX.get()");
            }
#else
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AngleX;
                };

                ExecuteTest(method, "AngleX.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AngleFromY;
                };

                ExecuteTest(method, "AngleFromY.get()");
            }
#else
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AngleY;
                };

                ExecuteTest(method, "AngleY.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AngleFromZ;
                };

                ExecuteTest(method, "AngleFromZ.get()");
            }
#else
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AngleZ;
                };

                ExecuteTest(method, "AngleZ.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AngleFromU;
                };

                ExecuteTest(method, "AngleFromU.get()");
            }
#else
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AngleU;
                };

                ExecuteTest(method, "AngleU.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AngleFromV;
                };

                ExecuteTest(method, "AngleFromV.get()");
            }
#else
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AngleV;
                };

                ExecuteTest(method, "AngleV.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AngleFromW;
                };

                ExecuteTest(method, "AngleFromW.get()");
            }
#else
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AngleW;
                };

                ExecuteTest(method, "AngleW.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AngleFromXYZUV;
                };

                ExecuteTest(method, "AngleFromXYZUV.get()");
            }
#else
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AngleXYZUV;
                };

                ExecuteTest(method, "AngleXYZUV.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AngleFromYZUVW;
                };

                ExecuteTest(method, "AngleFromYZUVW.get()");
            }
#else
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AngleYZUVW;
                };

                ExecuteTest(method, "AngleYZUVW.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AngleFromZUVWX;
                };

                ExecuteTest(method, "AngleFromZUVWX.get()");
            }
#else
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AngleZUVWX;
                };

                ExecuteTest(method, "AngleZUVWX.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AngleFromUVWXY;
                };

                ExecuteTest(method, "AngleFromUVWXY.get()");
            }
#else
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AngleUVWXY;
                };

                ExecuteTest(method, "AngleUVWXY.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AngleFromVWXYZ;
                };

                ExecuteTest(method, "AngleFromVWXYZ.get()");
            }
#else
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AngleVWXYZ;
                };

                ExecuteTest(method, "AngleVWXYZ.get()");
            }
#endif

#if ComVer1905
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AngleFromWXYZU;
                };

                ExecuteTest(method, "AngleFromWXYZU.get()");
            }
#else
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AngleWXYZU;
                };

                ExecuteTest(method, "AngleWXYZU.get()");
            }
#endif
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

                ExecuteTest(method, "Equals(object)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.GetHashCode();
                };

                ExecuteTest(method, "GetHashCode()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.ToString();
                };

                ExecuteTest(method, "ToString()");
            }

            // Equals

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = left;

                Action method = () =>
                {
                    _ = left.Equals(right);
                };

                ExecuteTest(method, "Equals(Com.PointD6D)");
            }

            // CompareTo

#if ComVer1905
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                object obj = pointD6D;

                Action method = () =>
                {
                    _ = pointD6D.CompareTo(obj);
                };

                ExecuteTest(method, "CompareTo(object)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = left;

                Action method = () =>
                {
                    _ = left.CompareTo(right);
                };

                ExecuteTest(method, "CompareTo(Com.PointD6D)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // 检索

#if ComVer1905
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD6D.IndexOf(item);
                };

                ExecuteTest(method, "IndexOf(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 0;

                Action method = () =>
                {
                    _ = pointD6D.IndexOf(item, startIndex);
                };

                ExecuteTest(method, "IndexOf(double, int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 0;
                int count = 6;

                Action method = () =>
                {
                    _ = pointD6D.IndexOf(item, startIndex, count);
                };

                ExecuteTest(method, "IndexOf(double, int, int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD6D.LastIndexOf(item);
                };

                ExecuteTest(method, "LastIndexOf(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 5;

                Action method = () =>
                {
                    _ = pointD6D.LastIndexOf(item, startIndex);
                };

                ExecuteTest(method, "LastIndexOf(double, int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 5;
                int count = 6;

                Action method = () =>
                {
                    _ = pointD6D.LastIndexOf(item, startIndex, count);
                };

                ExecuteTest(method, "LastIndexOf(double, int, int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD6D.Contains(item);
                };

                ExecuteTest(method, "Contains(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // ToArray，ToList

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.ToArray();
                };

                ExecuteTest(method, "ToArray()");
            }

#if ComVer1905
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.ToList();
                };

                ExecuteTest(method, "ToList()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // 坐标系转换

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.ToSpherical();
                };

                ExecuteTest(method, "ToSpherical()");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.ToCartesian();
                };

                ExecuteTest(method, "ToCartesian()");
            }

            // 距离与夹角

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D pt = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.DistanceFrom(pt);
                };

                ExecuteTest(method, "DistanceFrom(Com.PointD6D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D pt = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AngleFrom(pt);
                };

                ExecuteTest(method, "AngleFrom(Com.PointD6D)");
            }

            // Offset

#if ComVerNext
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                int index = Com.Statistics.RandomInteger(6);
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD6D.Offset(index, d);
                };

                ExecuteTest(method, "Offset(int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD6D.Offset(d);
                };

                ExecuteTest(method, "Offset(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D pt = _GetRandomPointD6D();

                Action method = () =>
                {
                    pointD6D.Offset(pt);
                };

                ExecuteTest(method, "Offset(Com.PointD6D)");
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

                ExecuteTest(method, "Offset(double, double, double, double, double, double)");
            }

#if ComVerNext
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                int index = Com.Statistics.RandomInteger(6);
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD6D.OffsetCopy(index, d);
                };

                ExecuteTest(method, "OffsetCopy(int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD6D.OffsetCopy(d);
                };

                ExecuteTest(method, "OffsetCopy(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D pt = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.OffsetCopy(pt);
                };

                ExecuteTest(method, "OffsetCopy(Com.PointD6D)");
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

                ExecuteTest(method, "OffsetCopy(double, double, double, double, double, double)");
            }

            // Scale

#if ComVerNext
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                int index = Com.Statistics.RandomInteger(6);
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD6D.Scale(index, s);
                };

                ExecuteTest(method, "Scale(int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    pointD6D.Scale(s);
                };

                ExecuteTest(method, "Scale(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D pt = _GetRandomPointD6D();

                Action method = () =>
                {
                    pointD6D.Scale(pt);
                };

                ExecuteTest(method, "Scale(Com.PointD6D)");
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

                ExecuteTest(method, "Scale(double, double, double, double, double, double)");
            }

#if ComVerNext
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                int index = Com.Statistics.RandomInteger(6);
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD6D.ScaleCopy(index, s);
                };

                ExecuteTest(method, "ScaleCopy(int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pointD6D.ScaleCopy(s);
                };

                ExecuteTest(method, "ScaleCopy(double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D pt = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.ScaleCopy(pt);
                };

                ExecuteTest(method, "ScaleCopy(Com.PointD6D)");
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

                ExecuteTest(method, "ScaleCopy(double, double, double, double, double, double)");
            }

            // Reflect

#if ComVer1905
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                int index = Com.Statistics.RandomInteger(6);

                Action method = () =>
                {
                    pointD6D.Reflect(index);
                };

                ExecuteTest(method, "Reflect(int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                int index = Com.Statistics.RandomInteger(6);

                Action method = () =>
                {
                    _ = pointD6D.ReflectCopy(index);
                };

                ExecuteTest(method, "ReflectCopy(int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // Shear

#if ComVer1905
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

                ExecuteTest(method, "Shear(int, int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
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

                ExecuteTest(method, "ShearCopy(int, int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // Rotate

#if ComVer1905
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

                ExecuteTest(method, "Rotate(int, int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
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

                ExecuteTest(method, "RotateCopy(int, int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // Affine

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D ex = _GetRandomNormalPointD6D();
                Com.PointD6D ey = _GetRandomNormalPointD6D();
                Com.PointD6D ez = _GetRandomNormalPointD6D();
                Com.PointD6D eu = _GetRandomNormalPointD6D();
                Com.PointD6D ev = _GetRandomNormalPointD6D();
                Com.PointD6D ew = _GetRandomNormalPointD6D();
                Com.PointD6D offset = _GetRandomPointD6D();

                Action method = () =>
                {
                    pointD6D.AffineTransform(ex, ey, ez, eu, ev, ew, offset);
                };

                ExecuteTest(method, "AffineTransform(Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.Matrix matrixLeft = _GetRandomMatrix(7, 7);

                Action method = () =>
                {
                    pointD6D.AffineTransform(matrixLeft);
                };

                ExecuteTest(method, "AffineTransform(Com.Matrix)");
            }

#if ComVer1910
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(7, 7));
                }

                Com.Matrix[] matricesLeft = matrixLeftList.ToArray();

                Action method = () =>
                {
                    pointD6D.AffineTransform(matricesLeft);
                };

                ExecuteTest(method, "AffineTransform(param Com.Matrix[])", "total 8 matrices");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

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

                ExecuteTest(method, "AffineTransform(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D ex = _GetRandomNormalPointD6D();
                Com.PointD6D ey = _GetRandomNormalPointD6D();
                Com.PointD6D ez = _GetRandomNormalPointD6D();
                Com.PointD6D eu = _GetRandomNormalPointD6D();
                Com.PointD6D ev = _GetRandomNormalPointD6D();
                Com.PointD6D ew = _GetRandomNormalPointD6D();
                Com.PointD6D offset = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.AffineTransformCopy(ex, ey, ez, eu, ev, ew, offset);
                };

                ExecuteTest(method, "AffineTransformCopy(Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.Matrix matrixLeft = _GetRandomMatrix(7, 7);

                Action method = () =>
                {
                    _ = pointD6D.AffineTransformCopy(matrixLeft);
                };

                ExecuteTest(method, "AffineTransformCopy(Com.Matrix)");
            }

#if ComVer1910
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(7, 7));
                }

                Com.Matrix[] matricesLeft = matrixLeftList.ToArray();

                Action method = () =>
                {
                    _ = pointD6D.AffineTransformCopy(matricesLeft);
                };

                ExecuteTest(method, "AffineTransformCopy(param Com.Matrix[])", "total 8 matrices");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

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

                ExecuteTest(method, "AffineTransformCopy(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D ex = _GetRandomNormalPointD6D();
                Com.PointD6D ey = _GetRandomNormalPointD6D();
                Com.PointD6D ez = _GetRandomNormalPointD6D();
                Com.PointD6D eu = _GetRandomNormalPointD6D();
                Com.PointD6D ev = _GetRandomNormalPointD6D();
                Com.PointD6D ew = _GetRandomNormalPointD6D();
                Com.PointD6D offset = _GetRandomPointD6D();

                Action method = () =>
                {
                    pointD6D.InverseAffineTransform(ex, ey, ez, eu, ev, ew, offset);
                };

                ExecuteTest(method, "InverseAffineTransform(Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.Matrix matrixLeft = _GetRandomMatrix(7, 7);

                Action method = () =>
                {
                    pointD6D.InverseAffineTransform(matrixLeft);
                };

                ExecuteTest(method, "InverseAffineTransform(Com.Matrix)");
            }

#if ComVer1910
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(7, 7));
                }

                Com.Matrix[] matricesLeft = matrixLeftList.ToArray();

                Action method = () =>
                {
                    pointD6D.InverseAffineTransform(matricesLeft);
                };

                ExecuteTest(method, "InverseAffineTransform(param Com.Matrix[])", "total 8 matrices");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

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

                ExecuteTest(method, "InverseAffineTransform(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D ex = _GetRandomNormalPointD6D();
                Com.PointD6D ey = _GetRandomNormalPointD6D();
                Com.PointD6D ez = _GetRandomNormalPointD6D();
                Com.PointD6D eu = _GetRandomNormalPointD6D();
                Com.PointD6D ev = _GetRandomNormalPointD6D();
                Com.PointD6D ew = _GetRandomNormalPointD6D();
                Com.PointD6D offset = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.InverseAffineTransformCopy(ex, ey, ez, eu, ev, ew, offset);
                };

                ExecuteTest(method, "InverseAffineTransformCopy(Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.Matrix matrixLeft = _GetRandomMatrix(7, 7);

                Action method = () =>
                {
                    _ = pointD6D.InverseAffineTransformCopy(matrixLeft);
                };

                ExecuteTest(method, "InverseAffineTransformCopy(Com.Matrix)");
            }

#if ComVer1910
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                List<Com.Matrix> matrixLeftList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixLeftList.Add(_GetRandomMatrix(7, 7));
                }

                Com.Matrix[] matricesLeft = matrixLeftList.ToArray();

                Action method = () =>
                {
                    _ = pointD6D.InverseAffineTransformCopy(matricesLeft);
                };

                ExecuteTest(method, "InverseAffineTransformCopy(param Com.Matrix[])", "total 8 matrices");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

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

                ExecuteTest(method, "InverseAffineTransformCopy(System.Collections.Generic.List<Com.Matrix>)", "total 8 matrices");
            }

            // Project

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D prjCenter = _GetRandomPointD6D();
                double focalLength = (pointD6D.W - prjCenter.W) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    _ = pointD6D.ProjectToXYZUV(prjCenter, focalLength);
                };

                ExecuteTest(method, "ProjectToXYZUV(Com.PointD6D, double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D prjCenter = _GetRandomPointD6D();
                double focalLength = (pointD6D.X - prjCenter.X) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    _ = pointD6D.ProjectToYZUVW(prjCenter, focalLength);
                };

                ExecuteTest(method, "ProjectToYZUVW(Com.PointD6D, double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D prjCenter = _GetRandomPointD6D();
                double focalLength = (pointD6D.Y - prjCenter.Y) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    _ = pointD6D.ProjectToZUVWX(prjCenter, focalLength);
                };

                ExecuteTest(method, "ProjectToZUVWX(Com.PointD6D, double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D prjCenter = _GetRandomPointD6D();
                double focalLength = (pointD6D.Z - prjCenter.Z) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    _ = pointD6D.ProjectToUVWXY(prjCenter, focalLength);
                };

                ExecuteTest(method, "ProjectToUVWXY(Com.PointD6D, double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D prjCenter = _GetRandomPointD6D();
                double focalLength = (pointD6D.U - prjCenter.U) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    _ = pointD6D.ProjectToVWXYZ(prjCenter, focalLength);
                };

                ExecuteTest(method, "ProjectToVWXYZ(Com.PointD6D, double)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();
                Com.PointD6D prjCenter = _GetRandomPointD6D();
                double focalLength = (pointD6D.V - prjCenter.V) * Com.Statistics.RandomDouble();

                Action method = () =>
                {
                    _ = pointD6D.ProjectToWXYZU(prjCenter, focalLength);
                };

                ExecuteTest(method, "ProjectToWXYZU(Com.PointD6D, double)");
            }

            // ToVector

#if ComVer1905
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.ToColumnVector();
                };

                ExecuteTest(method, "ToColumnVector()");
            }
#else
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.ToVectorColumn();
                };

                ExecuteTest(method, "ToVectorColumn()");
            }
#endif

#if ComVer1905
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.ToRowVector();
                };

                ExecuteTest(method, "ToRowVector()");
            }
#else
            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = pointD6D.ToVectorRow();
                };

                ExecuteTest(method, "ToVectorRow()");
            }
#endif
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

                ExecuteTest(method, "Equals(Com.PointD6D, Com.PointD6D)");
            }

            // Compare

#if ComVer1905
            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = left;

                Action method = () =>
                {
                    _ = Com.PointD6D.Compare(left, right);
                };

                ExecuteTest(method, "Compare(Com.PointD6D, Com.PointD6D)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // From

#if ComVer1910
            {
                Com.Vector vector = _GetRandomPointD6D().ToColumnVector();

                Action method = () =>
                {
                    _ = Com.PointD6D.FromVector(vector);
                };

                ExecuteTest(method, "FromVector(Com.Vector)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

            // Matrix

            {
                Action method = () =>
                {
                    _ = Com.PointD6D.IdentityMatrix();
                };

                ExecuteTest(method, "IdentityMatrix()");
            }

#if ComVerNext
            {
                int index = Com.Statistics.RandomInteger(6);
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD6D.OffsetMatrix(index, d);
                };

                ExecuteTest(method, "OffsetMatrix(int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD6D.OffsetMatrix(d);
                };

                ExecuteTest(method, "OffsetMatrix(double)");
            }

            {
                Com.PointD6D pt = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = Com.PointD6D.OffsetMatrix(pt);
                };

                ExecuteTest(method, "OffsetMatrix(Com.PointD6D)");
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

                ExecuteTest(method, "OffsetMatrix(double, double, double, double, double, double)");
            }

#if ComVerNext
            {
                int index = Com.Statistics.RandomInteger(6);
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD6D.ScaleMatrix(index, s);
                };

                ExecuteTest(method, "ScaleMatrix(int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.PointD6D.ScaleMatrix(s);
                };

                ExecuteTest(method, "ScaleMatrix(double)");
            }

            {
                Com.PointD6D pt = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = Com.PointD6D.ScaleMatrix(pt);
                };

                ExecuteTest(method, "ScaleMatrix(Com.PointD6D)");
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

                ExecuteTest(method, "ScaleMatrix(double, double, double, double, double, double)");
            }

#if ComVer1905
            {
                int index = Com.Statistics.RandomInteger(3);

                Action method = () =>
                {
                    _ = Com.PointD6D.ReflectMatrix(index);
                };

                ExecuteTest(method, "ReflectMatrix(int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                int index1 = Com.Statistics.RandomInteger(6);
                int index2 = Com.Statistics.RandomInteger(5);
                if (index2 == index1) index2++;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD6D.ShearMatrix(index1, index2, angle);
                };

                ExecuteTest(method, "ShearMatrix(int, int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                int index1 = Com.Statistics.RandomInteger(6);
                int index2 = Com.Statistics.RandomInteger(5);
                if (index2 == index1) index2++;
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = Com.PointD6D.RotateMatrix(index1, index2, angle);
                };

                ExecuteTest(method, "RotateMatrix(int, int, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // 距离与夹角

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = Com.PointD6D.DistanceBetween(left, right);
                };

                ExecuteTest(method, "DistanceBetween(Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = Com.PointD6D.AngleBetween(left, right);
                };

                ExecuteTest(method, "AngleBetween(Com.PointD6D, Com.PointD6D)");
            }

            // 向量乘积

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = Com.PointD6D.DotProduct(left, right);
                };

                ExecuteTest(method, "DotProduct(Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = Com.PointD6D.CrossProduct(left, right);
                };

                ExecuteTest(method, "CrossProduct(Com.PointD6D, Com.PointD6D)");
            }

            // 初等函数

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = Com.PointD6D.Abs(pointD6D);
                };

                ExecuteTest(method, "Abs(Com.PointD6D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = Com.PointD6D.Sign(pointD6D);
                };

                ExecuteTest(method, "Sign(Com.PointD6D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = Com.PointD6D.Ceiling(pointD6D);
                };

                ExecuteTest(method, "Ceiling(Com.PointD6D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = Com.PointD6D.Floor(pointD6D);
                };

                ExecuteTest(method, "Floor(Com.PointD6D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = Com.PointD6D.Round(pointD6D);
                };

                ExecuteTest(method, "Round(Com.PointD6D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = Com.PointD6D.Truncate(pointD6D);
                };

                ExecuteTest(method, "Truncate(Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = Com.PointD6D.Max(left, right);
                };

                ExecuteTest(method, "Max(Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = Com.PointD6D.Min(left, right);
                };

                ExecuteTest(method, "Min(Com.PointD6D, Com.PointD6D)");
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

                ExecuteTest(method, "operator ==(Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = left;

                Action method = () =>
                {
                    _ = (left != right);
                };

                ExecuteTest(method, "operator !=(Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = (left < right);
                };

                ExecuteTest(method, "operator <(Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = (left > right);
                };

                ExecuteTest(method, "operator >(Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = (left <= right);
                };

                ExecuteTest(method, "operator <=(Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = (left >= right);
                };

                ExecuteTest(method, "operator >=(Com.PointD6D, Com.PointD6D)");
            }

            // 运算

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = +pointD6D;
                };

                ExecuteTest(method, "operator +(Com.PointD6D)");
            }

            {
                Com.PointD6D pointD6D = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = -pointD6D;
                };

                ExecuteTest(method, "operator -(Com.PointD6D)");
            }

            {
                Com.PointD6D pt = _GetRandomPointD6D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt + n;
                };

                ExecuteTest(method, "operator +(Com.PointD6D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD6D pt = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = n + pt;
                };

                ExecuteTest(method, "operator +(double, Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = left + right;
                };

                ExecuteTest(method, "operator +(Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D pt = _GetRandomPointD6D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt - n;
                };

                ExecuteTest(method, "operator -(Com.PointD6D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD6D pt = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = n - pt;
                };

                ExecuteTest(method, "operator -(double, Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = left - right;
                };

                ExecuteTest(method, "operator -(Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D pt = _GetRandomPointD6D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt * n;
                };

                ExecuteTest(method, "operator *(Com.PointD6D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD6D pt = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = n * pt;
                };

                ExecuteTest(method, "operator *(double, Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = left * right;
                };

                ExecuteTest(method, "operator *(Com.PointD6D, Com.PointD6D)");
            }

            {
                Com.PointD6D pt = _GetRandomPointD6D();
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt / n;
                };

                ExecuteTest(method, "operator /(Com.PointD6D, double)");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.PointD6D pt = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = n / pt;
                };

                ExecuteTest(method, "operator /(double, Com.PointD6D)");
            }

            {
                Com.PointD6D left = _GetRandomPointD6D();
                Com.PointD6D right = _GetRandomPointD6D();

                Action method = () =>
                {
                    _ = left / right;
                };

                ExecuteTest(method, "operator /(Com.PointD6D, Com.PointD6D)");
            }

            // 类型转换

#if ComVerNext
            {
                Com.PointD6D pt = _GetRandomPointD6D();
                (double, double, double, double, double, double) tuple = (pt.X, pt.Y, pt.Z, pt.U, pt.V, pt.W);

                Action method = () =>
                {
                    _ = (Com.PointD6D)tuple;
                };

                ExecuteTest(method, "implicit operator Com.PointD6D(System.ValueTuple<double, double, double, double, double, double>)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif
        }
    }

    sealed class RealTest : ClassPerfTestBase
    {
        private const string _NamespaceName = "Com";
        private const string _ClassName = "Real";

        private void ExecuteTest(Action method, string methodName, string comment)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName, comment);
        }

        private void ExecuteTest(Action method, string methodName)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(string methodName, UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName, unsupportedReason);
        }

        private void ExecuteTest(string methodName)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, string.Empty, unsupportedReason);
        }

        //

#if ComVer1905
        private static Com.Real _GetRandomReal()
        {
            return new Com.Real(Com.Statistics.RandomDouble(1, 10), Com.Statistics.RandomInteger());
        }
#endif

        //

        protected override void Constructor()
        {
#if ComVer1905
            {
                double value = Com.Statistics.RandomDouble(-10, 10);
                long magnitude = Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = new Com.Real(value, magnitude);
                };

                ExecuteTest(method, "Real(double, long)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double value = Com.Statistics.RandomDouble(-1E150, 1E150);

                Action method = () =>
                {
                    _ = new Com.Real(value);
                };

                ExecuteTest(method, "Real(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                float value = (float)Com.Statistics.RandomDouble(-1E30, 1E30);

                Action method = () =>
                {
                    _ = new Com.Real(value);
                };

                ExecuteTest(method, "Real(float)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                decimal value = (decimal)Com.Statistics.RandomDouble(-1E20, 1E20);

                Action method = () =>
                {
                    _ = new Com.Real(value);
                };

                ExecuteTest(method, "Real(decimal)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                ulong value = unchecked((ulong)Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    _ = new Com.Real(value);
                };

                ExecuteTest(method, "Real(ulong)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                long value = Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = new Com.Real(value);
                };

                ExecuteTest(method, "Real(long)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                uint value = unchecked((uint)Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    _ = new Com.Real(value);
                };

                ExecuteTest(method, "Real(uint)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                int value = Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = new Com.Real(value);
                };

                ExecuteTest(method, "Real(int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                ushort value = unchecked((ushort)Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    _ = new Com.Real(value);
                };

                ExecuteTest(method, "Real(ushort)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                short value = unchecked((short)Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    _ = new Com.Real(value);
                };

                ExecuteTest(method, "Real(short)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                byte value = unchecked((byte)Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    _ = new Com.Real(value);
                };

                ExecuteTest(method, "Real(byte)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                sbyte value = unchecked((sbyte)Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    _ = new Com.Real(value);
                };

                ExecuteTest(method, "Real(sbyte)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif
        }

        protected override void Property()
        {
            // Is

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.IsNaN;
                };

                ExecuteTest(method, "IsNaN.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.IsPositiveInfinity;
                };

                ExecuteTest(method, "IsPositiveInfinity.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.IsNegativeInfinity;
                };

                ExecuteTest(method, "IsNegativeInfinity.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.IsInfinity;
                };

                ExecuteTest(method, "IsInfinity.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.IsNaNOrInfinity;
                };

                ExecuteTest(method, "IsNaNOrInfinity.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.IsZero;
                };

                ExecuteTest(method, "IsZero.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.IsOne;
                };

                ExecuteTest(method, "IsOne.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.IsMinusOne;
                };

                ExecuteTest(method, "IsMinusOne.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.IsPositive;
                };

                ExecuteTest(method, "IsPositive.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.IsNegative;
                };

                ExecuteTest(method, "IsNegative.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.IsInteger;
                };

                ExecuteTest(method, "IsInteger.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.IsDecimal;
                };

                ExecuteTest(method, "IsDecimal.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.IsEven;
                };

                ExecuteTest(method, "IsEven.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.IsOdd;
                };

                ExecuteTest(method, "IsOdd.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // 分量

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.Value;
                };

                ExecuteTest(method, "Value.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();
                double value = Com.Statistics.RandomDouble(1, 10);

                Action method = () =>
                {
                    real.Value = value;
                };

                ExecuteTest(method, "Value.set(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.Magnitude;
                };

                ExecuteTest(method, "Magnitude.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();
                long value = Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    real.Magnitude = value;
                };

                ExecuteTest(method, "Magnitude.set(long)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // 相反数、倒数

#if ComVer1910
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.Opposite;
                };

                ExecuteTest(method, "Opposite.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

#if ComVer1910
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.Reciprocal;
                };

                ExecuteTest(method, "Reciprocal.get()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif
        }

        protected override void StaticProperty()
        {

        }

        protected override void Method()
        {
            // object

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();
                object obj = real;

                Action method = () =>
                {
                    _ = real.Equals(obj);
                };

                ExecuteTest(method, "Equals(object)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.GetHashCode();
                };

                ExecuteTest(method, "GetHashCode()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905

            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = real.ToString();
                };

                ExecuteTest(method, "ToString()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // Equals

#if ComVer1905
            {
                Com.Real left = _GetRandomReal();
                Com.Real right = left;

                Action method = () =>
                {
                    _ = left.Equals(right);
                };

                ExecuteTest(method, "Equals(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // CompareTo

#if ComVer1905
            {
                Com.Real left = _GetRandomReal();
                object right = left;

                Action method = () =>
                {
                    _ = left.CompareTo(right);
                };

                ExecuteTest(method, "CompareTo(object)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real left = _GetRandomReal();
                Com.Real right = left;

                Action method = () =>
                {
                    _ = left.CompareTo(right);
                };

                ExecuteTest(method, "CompareTo(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif
        }

        protected override void StaticMethod()
        {
            // Equals

#if ComVer1905
            {
                Com.Real left = _GetRandomReal();
                Com.Real right = left;

                Action method = () =>
                {
                    _ = Com.Real.Equals(left, right);
                };

                ExecuteTest(method, "Equals(Com.Real, Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // Compare

#if ComVer1905
            {
                Com.Real left = _GetRandomReal();
                Com.Real right = left;

                Action method = () =>
                {
                    _ = Com.Real.Compare(left, right);
                };

                ExecuteTest(method, "CompareTo(Com.Real, Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // 幂函数，指数函数，对数函数

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Sqr(real);
                };

                ExecuteTest(method, "Sqr(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Sqrt(real);
                };

                ExecuteTest(method, "Sqrt(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Exp10(real);
                };

                ExecuteTest(method, "Exp10(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Exp(real);
                };

                ExecuteTest(method, "Exp(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real left = _GetRandomReal();
                Com.Real right = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Pow(left, right);
                };

                ExecuteTest(method, "Pow(Com.Real, Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Log10(real);
                };

                ExecuteTest(method, "Log10(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Log(real);
                };

                ExecuteTest(method, "Log(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real left = _GetRandomReal();
                Com.Real right = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Log(left, right);
                };

                ExecuteTest(method, "Log(Com.Real, Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // 三角函数

#if ComVer1905
            {
                Com.Real real = new Com.Real(Com.Statistics.RandomDouble(1, 10), 4096);

                Action method = () =>
                {
                    _ = Com.Real.Sin(real);
                };

                ExecuteTest(method, "Sin(Com.Real)", "real at magnitude of 4096");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = new Com.Real(Com.Statistics.RandomDouble(1, 10), 4096);

                Action method = () =>
                {
                    _ = Com.Real.Cos(real);
                };

                ExecuteTest(method, "Cos(Com.Real)", "real at magnitude of 4096");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = new Com.Real(Com.Statistics.RandomDouble(1, 10), 4096);

                Action method = () =>
                {
                    _ = Com.Real.Tan(real);
                };

                ExecuteTest(method, "Tan(Com.Real)", "real at magnitude of 4096");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = Com.Statistics.RandomDouble(-1, 1);

                Action method = () =>
                {
                    _ = Com.Real.Asin(real);
                };

                ExecuteTest(method, "Asin(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = Com.Statistics.RandomDouble(-1, 1);

                Action method = () =>
                {
                    _ = Com.Real.Acos(real);
                };

                ExecuteTest(method, "Acos(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Atan(real);
                };

                ExecuteTest(method, "Atan(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Sinh(real);
                };

                ExecuteTest(method, "Sinh(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Cosh(real);
                };

                ExecuteTest(method, "Cosh(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Tanh(real);
                };

                ExecuteTest(method, "Tanh(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = Com.Real.Abs(_GetRandomReal());

                Action method = () =>
                {
                    _ = Com.Real.Asinh(real);
                };

                ExecuteTest(method, "Asinh(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = Com.Real.Abs(_GetRandomReal());

                Action method = () =>
                {
                    _ = Com.Real.Acosh(real);
                };

                ExecuteTest(method, "Acosh(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = Com.Statistics.RandomDouble(-1, 1);

                Action method = () =>
                {
                    _ = Com.Real.Atanh(real);
                };

                ExecuteTest(method, "Atanh(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // 初等函数

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Abs(real);
                };

                ExecuteTest(method, "Abs(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Sign(real);
                };

                ExecuteTest(method, "Sign(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Ceiling(real);
                };

                ExecuteTest(method, "Ceiling(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Floor(real);
                };

                ExecuteTest(method, "Floor(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Round(real);
                };

                ExecuteTest(method, "Round(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Truncate(real);
                };

                ExecuteTest(method, "Truncate(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real left = _GetRandomReal();
                Com.Real right = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Max(left, right);
                };

                ExecuteTest(method, "Max(Com.Real, Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real left = _GetRandomReal();
                Com.Real right = _GetRandomReal();

                Action method = () =>
                {
                    _ = Com.Real.Min(left, right);
                };

                ExecuteTest(method, "Min(Com.Real, Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif
        }

        protected override void Operator()
        {
            // 比较

#if ComVer1905
            {
                Com.Real left = _GetRandomReal();
                Com.Real right = left;

                Action method = () =>
                {
                    _ = (left == right);
                };

                ExecuteTest(method, "operator ==(Com.Real, Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real left = _GetRandomReal();
                Com.Real right = left;

                Action method = () =>
                {
                    _ = (left != right);
                };

                ExecuteTest(method, "operator !=(Com.Real, Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real left = _GetRandomReal();
                Com.Real right = _GetRandomReal();

                Action method = () =>
                {
                    _ = (left < right);
                };

                ExecuteTest(method, "operator <(Com.Real, Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real left = _GetRandomReal();
                Com.Real right = _GetRandomReal();

                Action method = () =>
                {
                    _ = (left > right);
                };

                ExecuteTest(method, "operator >(Com.Real, Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real left = _GetRandomReal();
                Com.Real right = _GetRandomReal();

                Action method = () =>
                {
                    _ = (left <= right);
                };

                ExecuteTest(method, "operator <=(Com.Real, Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real left = _GetRandomReal();
                Com.Real right = _GetRandomReal();

                Action method = () =>
                {
                    _ = (left >= right);
                };

                ExecuteTest(method, "operator >=(Com.Real, Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // 运算

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = +real;
                };

                ExecuteTest(method, "operator +(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    _ = -real;
                };

                ExecuteTest(method, "operator -(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    real++;
                };

                ExecuteTest(method, "operator ++(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = _GetRandomReal();

                Action method = () =>
                {
                    real--;
                };

                ExecuteTest(method, "operator --(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real left = _GetRandomReal();
                Com.Real right = _GetRandomReal();

                Action method = () =>
                {
                    _ = left + right;
                };

                ExecuteTest(method, "operator +(Com.Real, Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real left = _GetRandomReal();
                Com.Real right = _GetRandomReal();

                Action method = () =>
                {
                    _ = left - right;
                };

                ExecuteTest(method, "operator -(Com.Real, Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real left = _GetRandomReal();
                Com.Real right = _GetRandomReal();

                Action method = () =>
                {
                    _ = left * right;
                };

                ExecuteTest(method, "operator *(Com.Real, Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real left = _GetRandomReal();
                Com.Real right = _GetRandomReal();

                Action method = () =>
                {
                    _ = left / right;
                };

                ExecuteTest(method, "operator /(Com.Real, Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real left = new Com.Real(Com.Statistics.RandomDouble(1, 10), 4096);
                Com.Real right = new Com.Real(Com.Statistics.RandomDouble(1, 10), 256);

                Action method = () =>
                {
                    _ = left % right;
                };

                ExecuteTest(method, "operator %(Com.Real, Com.Real)", "left at magnitude of 4096, right at magnitude of 256");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real left = _GetRandomReal();
                Com.Real right = _GetRandomReal();

                Action method = () =>
                {
                    _ = left ^ right;
                };

                ExecuteTest(method, "operator ^(Com.Real, Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // 类型转换

#if ComVer1905
            {
                Com.Real real = new Com.Real(Com.Statistics.RandomDouble(-1E150, 1E150));

                Action method = () =>
                {
                    _ = (double)real;
                };

                ExecuteTest(method, "explicit operator double(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = new Com.Real(Com.Statistics.RandomDouble(-1E30, 1E30));

                Action method = () =>
                {
                    _ = (float)real;
                };

                ExecuteTest(method, "explicit operator float(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = new Com.Real(Com.Statistics.RandomDouble(-1E20, 1E20));

                Action method = () =>
                {
                    _ = (decimal)real;
                };

                ExecuteTest(method, "explicit operator decimal(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = new Com.Real(unchecked((ulong)Com.Statistics.RandomInteger()));

                Action method = () =>
                {
                    _ = (ulong)real;
                };

                ExecuteTest(method, "explicit operator ulong(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = new Com.Real(Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    _ = (long)real;
                };

                ExecuteTest(method, "explicit operator long(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = new Com.Real(unchecked((uint)Com.Statistics.RandomInteger()));

                Action method = () =>
                {
                    _ = (uint)real;
                };

                ExecuteTest(method, "explicit operator uint(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = new Com.Real(Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    _ = (int)real;
                };

                ExecuteTest(method, "explicit operator int(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = new Com.Real(unchecked((ushort)Com.Statistics.RandomInteger()));

                Action method = () =>
                {
                    _ = (ushort)real;
                };

                ExecuteTest(method, "explicit operator ushort(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = new Com.Real(unchecked((short)Com.Statistics.RandomInteger()));

                Action method = () =>
                {
                    _ = (short)real;
                };

                ExecuteTest(method, "explicit operator short(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = new Com.Real(unchecked((byte)Com.Statistics.RandomInteger()));

                Action method = () =>
                {
                    _ = (byte)real;
                };

                ExecuteTest(method, "explicit operator byte(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Real real = new Com.Real(unchecked((sbyte)Com.Statistics.RandomInteger()));

                Action method = () =>
                {
                    _ = (sbyte)real;
                };

                ExecuteTest(method, "explicit operator sbyte(Com.Real)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double value = Com.Statistics.RandomDouble(-1E150, 1E150);

                Action method = () =>
                {
                    _ = (Com.Real)value;
                };

                ExecuteTest(method, "implicit operator Real(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                float value = (float)Com.Statistics.RandomDouble(-1E30, 1E30);

                Action method = () =>
                {
                    _ = (Com.Real)value;
                };

                ExecuteTest(method, "implicit operator Real(float)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                decimal value = (decimal)Com.Statistics.RandomDouble(-1E20, 1E20);

                Action method = () =>
                {
                    _ = (Com.Real)value;
                };

                ExecuteTest(method, "explicit operator Real(decimal)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                ulong value = unchecked((ulong)Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    _ = (Com.Real)value;
                };

                ExecuteTest(method, "implicit operator Real(ulong)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                long value = Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = (Com.Real)value;
                };

                ExecuteTest(method, "implicit operator Real(long)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                uint value = unchecked((uint)Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    _ = (Com.Real)value;
                };

                ExecuteTest(method, "implicit operator Real(uint)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                int value = Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = (Com.Real)value;
                };

                ExecuteTest(method, "implicit operator Real(int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                ushort value = unchecked((ushort)Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    _ = (Com.Real)value;
                };

                ExecuteTest(method, "implicit operator Real(ushort)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                short value = unchecked((short)Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    _ = (Com.Real)value;
                };

                ExecuteTest(method, "implicit operator Real(short)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                byte value = unchecked((byte)Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    _ = (Com.Real)value;
                };

                ExecuteTest(method, "implicit operator Real(byte)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                sbyte value = unchecked((sbyte)Com.Statistics.RandomInteger());

                Action method = () =>
                {
                    _ = (Com.Real)value;
                };

                ExecuteTest(method, "implicit operator Real(sbyte)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif
        }
    }

    sealed class StatisticsTest : ClassPerfTestBase
    {
        private const string _NamespaceName = "Com";
        private const string _ClassName = "Statistics";

        private void ExecuteTest(Action method, string methodName, string comment)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName, comment);
        }

        private void ExecuteTest(Action method, string methodName)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(string methodName, UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName, unsupportedReason);
        }

        private void ExecuteTest(string methodName)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, string.Empty, unsupportedReason);
        }

        //

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
                    array[i] = unchecked((short)Com.Statistics.RandomInteger());
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
                    array[i] = (decimal)(Com.Statistics.RandomDouble() * Com.Statistics.RandomInteger());
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
                    array[i] = (float)(Com.Statistics.RandomDouble() * Com.Statistics.RandomInteger());
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
                    array[i] = Com.Statistics.RandomDouble() * Com.Statistics.RandomInteger();
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

                ExecuteTest(method, "RandomInteger()");
            }

            {
                int right = Com.Statistics.RandomInteger();

                Action method = () =>
                {
                    _ = Com.Statistics.RandomInteger(right);
                };

                ExecuteTest(method, "RandomInteger(int)");
            }

            {
                int right = Com.Statistics.RandomInteger();
                int left = -Com.Statistics.RandomInteger(right / 2);

                Action method = () =>
                {
                    _ = Com.Statistics.RandomInteger(left, right);
                };

                ExecuteTest(method, "RandomInteger(int, int)");
            }

            {
                Action method = () =>
                {
                    _ = Com.Statistics.RandomDouble();
                };

                ExecuteTest(method, "RandomDouble()");
            }

            {
                double right = Com.Statistics.RandomDouble(1E18);

                Action method = () =>
                {
                    _ = Com.Statistics.RandomDouble(right);
                };

                ExecuteTest(method, "RandomDouble(double)");
            }

            {
                double right = Com.Statistics.RandomDouble(1E18);
                double left = -Com.Statistics.RandomDouble(right / 2);

                Action method = () =>
                {
                    _ = Com.Statistics.RandomDouble(left, right);
                };

                ExecuteTest(method, "RandomDouble(double, double)");
            }

#if ComVer1905
            {
                Action method = () =>
                {
                    _ = Com.Statistics.NormalDistributionRandomInteger();
                };

                ExecuteTest(method, "NormalDistributionRandomInteger()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double ev = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sd = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.Statistics.NormalDistributionRandomInteger(ev, sd);
                };

                ExecuteTest(method, "NormalDistributionRandomInteger(double, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Action method = () =>
                {
                    _ = Com.Statistics.NormalDistributionRandomDouble();
                };

                ExecuteTest(method, "NormalDistributionRandomDouble()");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double ev = Com.Statistics.RandomDouble(-1E18, 1E18);
                double sd = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.Statistics.NormalDistributionRandomDouble(ev, sd);
                };

                ExecuteTest(method, "NormalDistributionRandomDouble(double, double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // 排列组合

#if ComVer1905
            {
                double total = 2097152;
                double selection = 1048576;

                Action method = () =>
                {
                    _ = Com.Statistics.Arrangement(total, selection);
                };

                ExecuteTest(method, "Arrangement(double, double)", "total at 2097152, selection at 1048576");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double total = 2097152;
                double selection = 1048576;

                Action method = () =>
                {
                    _ = Com.Statistics.Combination(total, selection);
                };

                ExecuteTest(method, "Combination(double, double)", "total at 2097152, selection at 1048576");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // 分布

#if ComVer1905
            {
                int value = 1048576;
                double p = 0.5;

                Action method = () =>
                {
                    _ = Com.Statistics.GeometricDistributionProbability(value, p);
                };

                ExecuteTest(method, "GeometricDistributionProbability(int, double)", "value at 1048576, p at 0.5");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                int value = 1048576;
                int N = 8388608;
                int M = 4194304;
                int n = 2097152;

                Action method = () =>
                {
                    _ = Com.Statistics.HypergeometricDistributionProbability(value, N, M, n);
                };

                ExecuteTest(method, "HypergeometricDistributionProbability(int, int, int, int)", "value at 1048576, N at 8388608, M at 4194304, n at 2097152");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                int value = 1048576;
                int n = 2097152;
                double p = 0.5;

                Action method = () =>
                {
                    _ = Com.Statistics.BinomialDistributionProbability(value, n, p);
                };

                ExecuteTest(method, "BinomialDistributionProbability(int, int, double)", "value at 1048576, N at 2097152, p at 0.5");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                int value = 1048576;
                double lambda = 1048576;

                Action method = () =>
                {
                    _ = Com.Statistics.PoissonDistributionProbability(value, lambda);
                };

                ExecuteTest(method, "PoissonDistributionProbability(int, double)", "value at 1048576, lambda at 1048576");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double value = 0.5;
                double lambda = 0.5;

                Action method = () =>
                {
                    _ = Com.Statistics.ExponentialDistributionProbabilityDensity(value, lambda);
                };

                ExecuteTest(method, "ExponentialDistributionProbabilityDensity(double, double)", "value at 0.5, lambda at 0.5");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double lambda = 0.5;
                double left = 0.5;
                double right = 1;

                Action method = () =>
                {
                    _ = Com.Statistics.ExponentialDistributionProbability(lambda, left, right);
                };

                ExecuteTest(method, "ExponentialDistributionProbabilityDensity(double, double, double)", "lambda at 0.5, left at 0.5, right at 1");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double value = 0.5;

                Action method = () =>
                {
                    _ = Com.Statistics.NormalDistributionProbabilityDensity(value);
                };

                ExecuteTest(method, "NormalDistributionProbabilityDensity(double)", "value at 0.5");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double value = 0.5;
                double ev = 0;
                double sd = 1;

                Action method = () =>
                {
                    _ = Com.Statistics.NormalDistributionProbabilityDensity(value, ev, sd);
                };

                ExecuteTest(method, "NormalDistributionProbabilityDensity(double, double, double)", "value at 0.5, ev at 0, sd at 1");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double value = 0.5;
                int k = 1;

                Action method = () =>
                {
                    _ = Com.Statistics.ChiSquaredDistributionProbabilityDensity(value, k);
                };

                ExecuteTest(method, "ChiSquaredDistributionProbabilityDensity(double, int)", "value at 0.5, k at 1");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // 极值，极差，求和，平均

#if ComVer1905
            {
                sbyte[] values = _GetRandomSbyteArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Max(values);
                };

                ExecuteTest(method, "Max(params sbyte[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                byte[] values = _GetRandomByteArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Max(values);
                };

                ExecuteTest(method, "Max(params byte[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                short[] values = _GetRandomShortArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Max(values);
                };

                ExecuteTest(method, "Max(params short[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                ushort[] values = _GetRandomUshortArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Max(values);
                };

                ExecuteTest(method, "Max(params ushort[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                int[] values = _GetRandomIntArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Max(values);
                };

                ExecuteTest(method, "Max(params int[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                uint[] values = _GetRandomUintArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Max(values);
                };

                ExecuteTest(method, "Max(params uint[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                long[] values = _GetRandomLongArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Max(values);
                };

                ExecuteTest(method, "Max(params long[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                ulong[] values = _GetRandomUlongArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Max(values);
                };

                ExecuteTest(method, "Max(params ulong[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                decimal[] values = _GetRandomDecimalArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Max(values);
                };

                ExecuteTest(method, "Max(params decimal[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                float[] values = _GetRandomFloatArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Max(values);
                };

                ExecuteTest(method, "Max(params float[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double[] values = _GetRandomDoubleArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Max(values);
                };

                ExecuteTest(method, "Max(params double[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            ExecuteTest("Max<T>(params T[]) where T : System.IComparable");
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            ExecuteTest("Max(params System.IComparable[])");
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                sbyte[] values = _GetRandomSbyteArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Min(values);
                };

                ExecuteTest(method, "Min(params sbyte[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                byte[] values = _GetRandomByteArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Min(values);
                };

                ExecuteTest(method, "Min(params byte[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                short[] values = _GetRandomShortArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Min(values);
                };

                ExecuteTest(method, "Min(params short[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                ushort[] values = _GetRandomUshortArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Min(values);
                };

                ExecuteTest(method, "Min(params ushort[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                int[] values = _GetRandomIntArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Min(values);
                };

                ExecuteTest(method, "Min(params int[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                uint[] values = _GetRandomUintArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Min(values);
                };

                ExecuteTest(method, "Min(params uint[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                long[] values = _GetRandomLongArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Min(values);
                };

                ExecuteTest(method, "Min(params long[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                ulong[] values = _GetRandomUlongArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Min(values);
                };

                ExecuteTest(method, "Min(params ulong[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                decimal[] values = _GetRandomDecimalArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Min(values);
                };

                ExecuteTest(method, "Min(params decimal[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                float[] values = _GetRandomFloatArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Min(values);
                };

                ExecuteTest(method, "Min(params float[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double[] values = _GetRandomDoubleArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Min(values);
                };

                ExecuteTest(method, "Min(params double[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            ExecuteTest("Min<T>(params T[]) where T : System.IComparable");
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            ExecuteTest("Min(params System.IComparable[])");
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                sbyte[] values = _GetRandomSbyteArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMax(values);
                };

                ExecuteTest(method, "MinMax(params sbyte[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                byte[] values = _GetRandomByteArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMax(values);
                };

                ExecuteTest(method, "MinMax(params byte[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                short[] values = _GetRandomShortArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMax(values);
                };

                ExecuteTest(method, "MinMax(params short[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                ushort[] values = _GetRandomUshortArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMax(values);
                };

                ExecuteTest(method, "MinMax(params ushort[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                int[] values = _GetRandomIntArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMax(values);
                };

                ExecuteTest(method, "MinMax(params int[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                uint[] values = _GetRandomUintArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMax(values);
                };

                ExecuteTest(method, "MinMax(params uint[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                long[] values = _GetRandomLongArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMax(values);
                };

                ExecuteTest(method, "MinMax(params long[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                ulong[] values = _GetRandomUlongArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMax(values);
                };

                ExecuteTest(method, "MinMax(params ulong[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                decimal[] values = _GetRandomDecimalArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMax(values);
                };

                ExecuteTest(method, "MinMax(params decimal[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                float[] values = _GetRandomFloatArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMax(values);
                };

                ExecuteTest(method, "MinMax(params float[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double[] values = _GetRandomDoubleArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMax(values);
                };

                ExecuteTest(method, "MinMax(params double[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            ExecuteTest("MinMax<T>(params T[]) where T : System.IComparable");
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            ExecuteTest("MinMax(params System.IComparable[])");
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                sbyte[] values = _GetRandomSbyteArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Range(values);
                };

                ExecuteTest(method, "Range(params sbyte[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                byte[] values = _GetRandomByteArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Range(values);
                };

                ExecuteTest(method, "Range(params byte[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                short[] values = _GetRandomShortArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Range(values);
                };

                ExecuteTest(method, "Range(params short[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                ushort[] values = _GetRandomUshortArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Range(values);
                };

                ExecuteTest(method, "Range(params ushort[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                int[] values = _GetRandomIntArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Range(values);
                };

                ExecuteTest(method, "Range(params int[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                uint[] values = _GetRandomUintArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Range(values);
                };

                ExecuteTest(method, "Range(params uint[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                long[] values = _GetRandomLongArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Range(values);
                };

                ExecuteTest(method, "Range(params long[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                ulong[] values = _GetRandomUlongArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Range(values);
                };

                ExecuteTest(method, "Range(params ulong[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                decimal[] values = _GetRandomDecimalArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Range(values);
                };

                ExecuteTest(method, "Range(params decimal[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                float[] values = _GetRandomFloatArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Range(values);
                };

                ExecuteTest(method, "Range(params float[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double[] values = _GetRandomDoubleArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Range(values);
                };

                ExecuteTest(method, "Range(params double[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                sbyte[] values = _GetRandomSbyteArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Sum(values);
                };

                ExecuteTest(method, "Sum(params sbyte[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                byte[] values = _GetRandomByteArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Sum(values);
                };

                ExecuteTest(method, "Sum(params byte[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                short[] values = _GetRandomShortArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Sum(values);
                };

                ExecuteTest(method, "Sum(params short[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                ushort[] values = _GetRandomUshortArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Sum(values);
                };

                ExecuteTest(method, "Sum(params ushort[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
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

                ExecuteTest(method, "Sum(params int[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                uint[] values = _GetRandomUintArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Sum(values);
                };

                ExecuteTest(method, "Sum(params uint[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                long[] values = _GetRandomLongArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Sum(values);
                };

                ExecuteTest(method, "Sum(params long[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                ulong[] values = _GetRandomUlongArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Sum(values);
                };

                ExecuteTest(method, "Sum(params ulong[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                decimal[] values = _GetRandomDecimalArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Sum(values);
                };

                ExecuteTest(method, "Sum(params decimal[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                float[] values = _GetRandomFloatArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Sum(values);
                };

                ExecuteTest(method, "Sum(params float[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double[] values = _GetRandomDoubleArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Sum(values);
                };

                ExecuteTest(method, "Sum(params double[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                sbyte[] values = _GetRandomSbyteArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Average(values);
                };

                ExecuteTest(method, "Average(params sbyte[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                byte[] values = _GetRandomByteArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Average(values);
                };

                ExecuteTest(method, "Average(params byte[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                short[] values = _GetRandomShortArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Average(values);
                };

                ExecuteTest(method, "Average(params short[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                ushort[] values = _GetRandomUshortArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Average(values);
                };

                ExecuteTest(method, "Average(params ushort[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                int[] values = _GetRandomIntArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Average(values);
                };

                ExecuteTest(method, "Average(params int[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                uint[] values = _GetRandomUintArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Average(values);
                };

                ExecuteTest(method, "Average(params uint[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                long[] values = _GetRandomLongArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Average(values);
                };

                ExecuteTest(method, "Average(params long[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                ulong[] values = _GetRandomUlongArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Average(values);
                };

                ExecuteTest(method, "Average(params ulong[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                decimal[] values = _GetRandomDecimalArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Average(values);
                };

                ExecuteTest(method, "Average(params decimal[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                float[] values = _GetRandomFloatArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Average(values);
                };

                ExecuteTest(method, "Average(params float[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double[] values = _GetRandomDoubleArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Average(values);
                };

                ExecuteTest(method, "Average(params double[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                sbyte[] values = _GetRandomSbyteArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMaxAverage(values);
                };

                ExecuteTest(method, "MinMaxAverage(params sbyte[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                byte[] values = _GetRandomByteArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMaxAverage(values);
                };

                ExecuteTest(method, "MinMaxAverage(params byte[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                short[] values = _GetRandomShortArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMaxAverage(values);
                };

                ExecuteTest(method, "MinMaxAverage(params short[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                ushort[] values = _GetRandomUshortArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMaxAverage(values);
                };

                ExecuteTest(method, "MinMaxAverage(params ushort[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                int[] values = _GetRandomIntArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMaxAverage(values);
                };

                ExecuteTest(method, "MinMaxAverage(params int[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                uint[] values = _GetRandomUintArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMaxAverage(values);
                };

                ExecuteTest(method, "MinMaxAverage(params uint[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                long[] values = _GetRandomLongArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMaxAverage(values);
                };

                ExecuteTest(method, "MinMaxAverage(params long[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                ulong[] values = _GetRandomUlongArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMaxAverage(values);
                };

                ExecuteTest(method, "MinMaxAverage(params ulong[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                decimal[] values = _GetRandomDecimalArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMaxAverage(values);
                };

                ExecuteTest(method, "MinMaxAverage(params decimal[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                float[] values = _GetRandomFloatArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMaxAverage(values);
                };

                ExecuteTest(method, "MinMaxAverage(params float[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double[] values = _GetRandomDoubleArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.MinMaxAverage(values);
                };

                ExecuteTest(method, "MinMaxAverage(params double[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // 方差与标准差

#if ComVer1905
            {
                double[] values = _GetRandomDoubleArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.Deviation(values);
                };

                ExecuteTest(method, "Deviation(params double[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double[] values = _GetRandomDoubleArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.SampleDeviation(values);
                };

                ExecuteTest(method, "SampleDeviation(params double[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double[] values = _GetRandomDoubleArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.StandardDeviation(values);
                };

                ExecuteTest(method, "StandardDeviation(params double[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double[] values = _GetRandomDoubleArray(1024);

                Action method = () =>
                {
                    _ = Com.Statistics.SampleStandardDeviation(values);
                };

                ExecuteTest(method, "SampleStandardDeviation(params double[])", "array size at 1024");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif
        }

        protected override void Operator()
        {

        }
    }

    sealed class TextTest : ClassPerfTestBase
    {
        private const string _NamespaceName = "Com";
        private const string _ClassName = "Text";

        private void ExecuteTest(Action method, string methodName, string comment)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName, comment);
        }

        private void ExecuteTest(Action method, string methodName)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(string methodName, UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName, unsupportedReason);
        }

        private void ExecuteTest(string methodName)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, string.Empty, unsupportedReason);
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
            // 科学计数法

#if ComVer1905
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

                ExecuteTest(method, "GetScientificNotationString(double, int, bool, bool, string)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);
                int significance = 0;
                bool useNaturalExpression = true;
                bool useMagnitudeOrderCode = true;

                Action method = () =>
                {
                    _ = Com.Text.GetScientificNotationString(value, significance, useNaturalExpression, useMagnitudeOrderCode);
                };

                ExecuteTest(method, "GetScientificNotationString(double, int, bool, bool)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);
                int significance = 0;
                bool useNaturalExpression = true;
                string unit = "U";

                Action method = () =>
                {
                    _ = Com.Text.GetScientificNotationString(value, significance, useNaturalExpression, unit);
                };

                ExecuteTest(method, "GetScientificNotationString(double, int, bool, string)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);
                int significance = 0;
                bool useNaturalExpression = true;

                Action method = () =>
                {
                    _ = Com.Text.GetScientificNotationString(value, significance, useNaturalExpression);
                };

                ExecuteTest(method, "GetScientificNotationString(double, int, bool)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);
                int significance = 0;
                string unit = "U";

                Action method = () =>
                {
                    _ = Com.Text.GetScientificNotationString(value, significance, unit);
                };

                ExecuteTest(method, "GetScientificNotationString(double, int, string)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);
                int significance = 0;

                Action method = () =>
                {
                    _ = Com.Text.GetScientificNotationString(value, significance);
                };

                ExecuteTest(method, "GetScientificNotationString(double, int)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);
                bool useNaturalExpression = true;
                bool useMagnitudeOrderCode = true;
                string unit = "U";

                Action method = () =>
                {
                    _ = Com.Text.GetScientificNotationString(value, useNaturalExpression, useMagnitudeOrderCode, unit);
                };

                ExecuteTest(method, "GetScientificNotationString(double, bool, bool, string)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);
                bool useNaturalExpression = true;
                bool useMagnitudeOrderCode = true;

                Action method = () =>
                {
                    _ = Com.Text.GetScientificNotationString(value, useNaturalExpression, useMagnitudeOrderCode);
                };

                ExecuteTest(method, "GetScientificNotationString(double, bool, bool)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);
                bool useNaturalExpression = true;
                string unit = "U";

                Action method = () =>
                {
                    _ = Com.Text.GetScientificNotationString(value, useNaturalExpression, unit);
                };

                ExecuteTest(method, "GetScientificNotationString(double, bool, string)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);
                bool useNaturalExpression = true;

                Action method = () =>
                {
                    _ = Com.Text.GetScientificNotationString(value, useNaturalExpression);
                };

                ExecuteTest(method, "GetScientificNotationString(double, bool)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);
                string unit = "U";

                Action method = () =>
                {
                    _ = Com.Text.GetScientificNotationString(value, unit);
                };

                ExecuteTest(method, "GetScientificNotationString(double, string)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.Text.GetScientificNotationString(value);
                };

                ExecuteTest(method, "GetScientificNotationString(double)");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

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

                ExecuteTest(method, "GetIntervalString(string, string, string, bool, bool)");
            }

            {
                string str = "!\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
                Font font = new Font("微软雅黑", 42F, FontStyle.Regular, GraphicsUnit.Point, 134);
                int width = 1024;

                Action method = () =>
                {
                    _ = Com.Text.StringIntercept(str, font, width);
                };

                ExecuteTest(method, "StringIntercept(string, System.Drawing.Font, int)");
            }

            {
                string text = "!\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
                Font font = new Font("微软雅黑", 42F, FontStyle.Regular, GraphicsUnit.Point, 134);
                SizeF size = new SizeF(1024, 1024);

                Action method = () =>
                {
                    _ = Com.Text.GetSuitableFont(text, font, size);
                };

                ExecuteTest(method, "GetSuitableFont(string, System.Drawing.Font, System.Drawing.SizeF)");
            }

            // 转换为字符串

            {
                TimeSpan timeSpan = TimeSpan.FromDays(1) + DateTime.Now.TimeOfDay;

                Action method = () =>
                {
                    _ = Com.Text.GetLongTimeStringFromTimeSpan(timeSpan);
                };

                ExecuteTest(method, "GetLongTimeStringFromTimeSpan(System.TimeSpan)");
            }

            {
                TimeSpan timeSpan = TimeSpan.FromDays(1) + DateTime.Now.TimeOfDay;

                Action method = () =>
                {
                    _ = Com.Text.GetTimeStringFromTimeSpan(timeSpan);
                };

                ExecuteTest(method, "GetTimeStringFromTimeSpan(System.TimeSpan)");
            }

            {
                double second = (TimeSpan.FromDays(1) + DateTime.Now.TimeOfDay).TotalSeconds;
                int significance = 6;

                Action method = () =>
                {
                    _ = Com.Text.GetStandardizationTimespanOfSecond(second, significance);
                };

                ExecuteTest(method, "GetStandardizationTimespanOfSecond(double, int)");
            }

            {
                double second = (TimeSpan.FromDays(1) + DateTime.Now.TimeOfDay).TotalSeconds;

                Action method = () =>
                {
                    _ = Com.Text.GetLargeTimespanStringOfSecond(second);
                };

                ExecuteTest(method, "GetLargeTimespanStringOfSecond(double)");
            }

            {
                double meter = Com.Statistics.RandomDouble(1E12);
                int significance = 6;

                Action method = () =>
                {
                    _ = Com.Text.GetStandardizationDistanceOfMeter(meter, significance);
                };

                ExecuteTest(method, "GetStandardizationDistanceOfMeter(double, int)");
            }

            {
                double meter = Com.Statistics.RandomDouble(1E12);

                Action method = () =>
                {
                    _ = Com.Text.GetLargeDistanceStringOfMeter(meter);
                };

                ExecuteTest(method, "GetLargeDistanceStringOfMeter(double)");
            }

            {
                double degree = Com.Statistics.RandomDouble(360);
                int decimalDigits = 3;
                bool cutdownIdleZeros = true;

                Action method = () =>
                {
                    _ = Com.Text.GetAngleStringOfDegree(degree, decimalDigits, cutdownIdleZeros);
                };

                ExecuteTest(method, "GetAngleStringOfDegree(double, int, bool)");
            }

            {
                long b = (long)Com.Statistics.RandomDouble(1E18);

                Action method = () =>
                {
                    _ = Com.Text.GetSize64StringFromByte(b);
                };

                ExecuteTest(method, "GetSize64StringFromByte(long)");
            }
        }

        protected override void Operator()
        {

        }
    }

    sealed class VectorTest : ClassPerfTestBase
    {
        private const string _NamespaceName = "Com";
        private const string _ClassName = "Vector";

        private void ExecuteTest(Action method, string methodName, string comment)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName, comment);
        }

        private void ExecuteTest(Action method, string methodName)
        {
            base.ExecuteTest(method, _NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(string methodName, UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName, unsupportedReason);
        }

        private void ExecuteTest(string methodName)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, methodName);
        }

        private void ExecuteTest(UnsupportedReason unsupportedReason)
        {
            base.ExecuteTest(_NamespaceName, _ClassName, string.Empty, unsupportedReason);
        }

        //

        private static Com.Vector _GetRandomVector(Com.Vector.Type type, int dimension)
        {
            if (dimension > 0)
            {
                Com.Vector vector = Com.Vector.Zero(type, dimension);

                for (int i = 0; i < dimension; i++)
                {
                    vector[i] = Com.Statistics.RandomDouble(-1000, 1000);
                }

                return vector;
            }
            else
            {
#if ComVer1905
                return Com.Vector.Empty;
#else
                return Com.Vector.NonVector;
#endif
            }
        }

        private static Com.Vector _GetRandomVector(int dimension)
        {
            if (dimension > 0)
            {
                Com.Vector vector = Com.Vector.Zero(dimension);

                for (int i = 0; i < dimension; i++)
                {
                    vector[i] = Com.Statistics.RandomDouble(-1000, 1000);
                }

                return vector;
            }
            else
            {
#if ComVer1905
                return Com.Vector.Empty;
#else
                return Com.Vector.NonVector;
#endif
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
                        matrix[x, y] = Com.Statistics.RandomDouble(-10, 10);
                    }
                }

                return matrix;
            }
            else
            {
#if ComVer1905
                return Com.Matrix.Empty;
#else
                return Com.Matrix.NonMatrix;
#endif
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

                ExecuteTest(method, "Vector(Com.Vector.Type, params double[])", "dimension at 32");
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

                ExecuteTest(method, "Vector(params double[])", "dimension at 32");
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

                ExecuteTest(method, "this[int].get()", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);
                int index = Com.Statistics.RandomInteger(32);
                double value = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    vector[index] = value;
                };

                ExecuteTest(method, "this[int].set(double)", "dimension at 32");
            }

            // Dimension

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.Dimension;
                };

                ExecuteTest(method, "Dimension.get()", "dimension at 32");
            }

            // Is

#if ComVer1905
            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.IsEmpty;
                };

                ExecuteTest(method, "IsEmpty.get()", "dimension at 32");
            }
#else
            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.IsNonVector;
                };

                ExecuteTest(method, "IsNonVector.get()", "dimension at 32");
            }
#endif

#if ComVer1905
            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.IsZero;
                };

                ExecuteTest(method, "IsZero.get()", "dimension at 32");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.IsColumnVector;
                };

                ExecuteTest(method, "IsColumnVector.get()", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.IsRowVector;
                };

                ExecuteTest(method, "IsRowVector.get()", "dimension at 32");
            }

#if ComVer1905
            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.IsReadOnly;
                };

                ExecuteTest(method, "IsReadOnly.get()", "dimension at 32");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.IsFixedSize;
                };

                ExecuteTest(method, "IsFixedSize.get()", "dimension at 32");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.IsNaN;
                };

                ExecuteTest(method, "IsNaN.get()", "dimension at 32");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.IsInfinity;
                };

                ExecuteTest(method, "IsInfinity.get()", "dimension at 32");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.IsNaNOrInfinity;
                };

                ExecuteTest(method, "IsNaNOrInfinity.get()", "dimension at 32");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // 模

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.Module;
                };

                ExecuteTest(method, "Module.get()", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.ModuleSquared;
                };

                ExecuteTest(method, "ModuleSquared.get()", "dimension at 32");
            }

            // 向量

#if ComVer1905
            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.Opposite;
                };

                ExecuteTest(method, "Opposite.get()", "dimension at 32");
            }
#else
            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.Negate;
                };

                ExecuteTest(method, "Negate.get()", "dimension at 32");
            }
#endif

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.Normalize;
                };

                ExecuteTest(method, "Normalize.get()", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.Transport;
                };

                ExecuteTest(method, "Transport.get()", "dimension at 32");
            }
        }

        protected override void StaticProperty()
        {
            // Empty

#if ComVer1905
            {
                Action method = () =>
                {
                    _ = Com.Vector.Empty;
                };

                ExecuteTest(method, "Empty.get()");
            }
#else
            {
                Action method = () =>
                {
                    _ = Com.Vector.NonVector;
                };

                ExecuteTest(method, "NonVector.get()");
            }
#endif
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

                ExecuteTest(method, "Equals(object)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.GetHashCode();
                };

                ExecuteTest(method, "GetHashCode()", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.ToString();
                };

                ExecuteTest(method, "ToString()", "dimension at 32");
            }

            // Equals

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = left.Copy();

                Action method = () =>
                {
                    _ = left.Equals(right);
                };

                ExecuteTest(method, "Equals(Com.Vector)", "dimension at 32");
            }

            // CompareTo

#if ComVer1905
            {
                Com.Vector vector = _GetRandomVector(32);
                object obj = vector;

                Action method = () =>
                {
                    _ = vector.CompareTo(obj);
                };

                ExecuteTest(method, "CompareTo(object)", "dimension at 32");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = left;

                Action method = () =>
                {
                    _ = left.CompareTo(right);
                };

                ExecuteTest(method, "CompareTo(Com.Vector)", "dimension at 32");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // Copy

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.Copy();
                };

                ExecuteTest(method, "Copy()", "dimension at 32");
            }

            // 检索

#if ComVer1905
            {
                Com.Vector vector = _GetRandomVector(32);
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = vector.IndexOf(item);
                };

                ExecuteTest(method, "IndexOf(double)", "dimension at 32");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Vector vector = _GetRandomVector(32);
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 0;

                Action method = () =>
                {
                    _ = vector.IndexOf(item, startIndex);
                };

                ExecuteTest(method, "IndexOf(double, int)", "dimension at 32");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Vector vector = _GetRandomVector(32);
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 0;
                int count = 32;

                Action method = () =>
                {
                    _ = vector.IndexOf(item, startIndex, count);
                };

                ExecuteTest(method, "IndexOf(double, int, int)", "dimension at 32");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Vector vector = _GetRandomVector(32);
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = vector.LastIndexOf(item);
                };

                ExecuteTest(method, "LastIndexOf(double)", "dimension at 32");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Vector vector = _GetRandomVector(32);
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 31;

                Action method = () =>
                {
                    _ = vector.LastIndexOf(item, startIndex);
                };

                ExecuteTest(method, "LastIndexOf(double, int)", "dimension at 32");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Vector vector = _GetRandomVector(32);
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);
                int startIndex = 31;
                int count = 32;

                Action method = () =>
                {
                    _ = vector.LastIndexOf(item, startIndex, count);
                };

                ExecuteTest(method, "LastIndexOf(double, int, int)", "dimension at 32");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Vector vector = _GetRandomVector(32);
                double item = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = vector.Contains(item);
                };

                ExecuteTest(method, "Contains(double)", "dimension at 32");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // ToArray，ToList，ToMatrix

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.ToArray();
                };

                ExecuteTest(method, "ToArray()", "dimension at 32");
            }

#if ComVer1905
            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.ToList();
                };

                ExecuteTest(method, "ToList()", "dimension at 32");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.ToMatrix();
                };

                ExecuteTest(method, "ToMatrix()", "dimension at 32");
            }

            // 坐标系转换

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.ToSpherical();
                };

                ExecuteTest(method, "ToSpherical()", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.ToCartesian();
                };

                ExecuteTest(method, "ToCartesian()", "dimension at 32");
            }

            // 距离与夹角

            {
                Com.Vector vector = _GetRandomVector(32);
                Com.Vector vector_d = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.DistanceFrom(vector_d);
                };

                ExecuteTest(method, "DistanceFrom(Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);
                Com.Vector vector_a = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.AngleFrom(vector_a);
                };

                ExecuteTest(method, "AngleFrom(Com.Vector)", "dimension at 32");
            }

            // Offset

#if ComVerNext
            {
                Com.Vector vector = _GetRandomVector(32);
                int index = Com.Statistics.RandomInteger(32);
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    vector.Offset(index, d);
                };

                ExecuteTest(method, "Offset(int, double)", "dimension at 32");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                Com.Vector vector = _GetRandomVector(32);
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    vector.Offset(d);
                };

                ExecuteTest(method, "Offset(double)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);
                Com.Vector vector_d = _GetRandomVector(32);

                Action method = () =>
                {
                    vector.Offset(vector_d);
                };

                ExecuteTest(method, "Offset(Com.Vector)", "dimension at 32");
            }

#if ComVerNext
            {
                Com.Vector vector = _GetRandomVector(32);
                int index = Com.Statistics.RandomInteger(32);
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = vector.OffsetCopy(index, d);
                };

                ExecuteTest(method, "OffsetCopy(int, double)", "dimension at 32");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                Com.Vector vector = _GetRandomVector(32);
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = vector.OffsetCopy(d);
                };

                ExecuteTest(method, "OffsetCopy(double)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);
                Com.Vector vector_d = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.OffsetCopy(vector_d);
                };

                ExecuteTest(method, "OffsetCopy(Com.Vector)", "dimension at 32");
            }

            // Scale

#if ComVerNext
            {
                Com.Vector vector = _GetRandomVector(32);
                int index = Com.Statistics.RandomInteger(32);
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    vector.Scale(index, d);
                };

                ExecuteTest(method, "Scale(int, double)", "dimension at 32");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                Com.Vector vector = _GetRandomVector(32);
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    vector.Scale(d);
                };

                ExecuteTest(method, "Scale(double)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);
                Com.Vector vector_d = _GetRandomVector(32);

                Action method = () =>
                {
                    vector.Scale(vector_d);
                };

                ExecuteTest(method, "Scale(Com.Vector)", "dimension at 32");
            }

#if ComVerNext
            {
                Com.Vector vector = _GetRandomVector(32);
                int index = Com.Statistics.RandomInteger(32);
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = vector.ScaleCopy(index, s);
                };

                ExecuteTest(method, "ScaleCopy(int, double)", "dimension at 32");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                Com.Vector vector = _GetRandomVector(32);
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = vector.ScaleCopy(s);
                };

                ExecuteTest(method, "ScaleCopy(double)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);
                Com.Vector vector_s = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = vector.ScaleCopy(vector_s);
                };

                ExecuteTest(method, "ScaleCopy(Com.Vector)", "dimension at 32");
            }

            // Reflect

#if ComVer1905
            {
                Com.Vector vector = _GetRandomVector(32);
                int index = Com.Statistics.RandomInteger(32);

                Action method = () =>
                {
                    vector.Reflect(index);
                };

                ExecuteTest(method, "Reflect(int)", "dimension at 32");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Vector vector = _GetRandomVector(32);
                int index = Com.Statistics.RandomInteger(32);

                Action method = () =>
                {
                    _ = vector.ReflectCopy(index);
                };

                ExecuteTest(method, "ReflectCopy(int)", "dimension at 32");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // Shear

#if ComVer1905
            {
                Com.Vector vector = _GetRandomVector(32);
                int index1 = Com.Statistics.RandomInteger(16);
                int index2 = Com.Statistics.RandomInteger(16, 32);
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    vector.Shear(index1, index2, angle);
                };

                ExecuteTest(method, "Shear(int, int, double)", "dimension at 32");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
            {
                Com.Vector vector = _GetRandomVector(32);
                int index1 = Com.Statistics.RandomInteger(16);
                int index2 = Com.Statistics.RandomInteger(16, 32);
                double angle = Com.Statistics.RandomDouble(2 * Math.PI);

                Action method = () =>
                {
                    _ = vector.ShearCopy(index1, index2, angle);
                };

                ExecuteTest(method, "ShearCopy(int, int, double)", "dimension at 32");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

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

                ExecuteTest(method, "Rotate(int, int, double)", "dimension at 32");
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

                ExecuteTest(method, "RotateCopy(int, int, double)", "dimension at 32");
            }

            // Affine

            {
                Com.Vector vector = _GetRandomVector(8);
                Com.Matrix matrix = _GetRandomMatrix(9, 9);

                Action method = () =>
                {
                    vector.AffineTransform(matrix);
                };

                ExecuteTest(method, "AffineTransform(Com.Matrix)", "dimension at 8");
            }

#if ComVer1910
            {
                Com.Vector vector = _GetRandomVector(8);
                List<Com.Matrix> matrixList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixList.Add(_GetRandomMatrix(9, 9));
                }

                Com.Matrix[] matrices = matrixList.ToArray();

                Action method = () =>
                {
                    vector.AffineTransform(matrices);
                };

                ExecuteTest(method, "AffineTransform(param Com.Matrix[])", "dimension at 8, total 8 matrices");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

            {
                Com.Vector vector = _GetRandomVector(8);
                List<Com.Matrix> matrixList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixList.Add(_GetRandomMatrix(9, 9));
                }

                Action method = () =>
                {
                    vector.AffineTransform(matrixList);
                };

                ExecuteTest(method, "AffineTransform(System.Collections.Generic.List<Com.Matrix>)", "dimension at 8, total 8 matrices");
            }

            {
                Com.Vector vector = _GetRandomVector(8);
                Com.Matrix matrix = _GetRandomMatrix(9, 9);

                Action method = () =>
                {
                    _ = vector.AffineTransformCopy(matrix);
                };

                ExecuteTest(method, "AffineTransformCopy(Com.Matrix)", "dimension at 8");
            }

#if ComVer1910
            {
                Com.Vector vector = _GetRandomVector(8);
                List<Com.Matrix> matrixList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixList.Add(_GetRandomMatrix(9, 9));
                }

                Com.Matrix[] matrices = matrixList.ToArray();

                Action method = () =>
                {
                    _ = vector.AffineTransformCopy(matrices);
                };

                ExecuteTest(method, "AffineTransformCopy(param Com.Matrix[])", "dimension at 8, total 8 matrices");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

            {
                Com.Vector vector = _GetRandomVector(8);
                List<Com.Matrix> matrixList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixList.Add(_GetRandomMatrix(9, 9));
                }

                Action method = () =>
                {
                    _ = vector.AffineTransformCopy(matrixList);
                };

                ExecuteTest(method, "AffineTransformCopy(System.Collections.Generic.List<Com.Matrix>)", "dimension at 8, total 8 matrices");
            }

            {
                Com.Vector vector = _GetRandomVector(8);
                Com.Matrix matrix = _GetRandomMatrix(9, 9);

                Action method = () =>
                {
                    vector.InverseAffineTransform(matrix);
                };

                ExecuteTest(method, "InverseAffineTransform(Com.Matrix)", "dimension at 8");
            }

#if ComVer1910
            {
                Com.Vector vector = _GetRandomVector(8);
                List<Com.Matrix> matrixList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixList.Add(_GetRandomMatrix(9, 9));
                }

                Com.Matrix[] matrices = matrixList.ToArray();

                Action method = () =>
                {
                    vector.InverseAffineTransform(matrices);
                };

                ExecuteTest(method, "InverseAffineTransform(param Com.Matrix[])", "dimension at 8, total 8 matrices");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

            {
                Com.Vector vector = _GetRandomVector(8);
                List<Com.Matrix> matrixList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixList.Add(_GetRandomMatrix(9, 9));
                }

                Action method = () =>
                {
                    vector.InverseAffineTransform(matrixList);
                };

                ExecuteTest(method, "InverseAffineTransform(System.Collections.Generic.List<Com.Matrix>)", "dimension at 8, total 8 matrices");
            }

            {
                Com.Vector vector = _GetRandomVector(8);
                Com.Matrix matrix = _GetRandomMatrix(9, 9);

                Action method = () =>
                {
                    _ = vector.InverseAffineTransformCopy(matrix);
                };

                ExecuteTest(method, "InverseAffineTransformCopy(Com.Matrix)", "dimension at 8");
            }

#if ComVer1910
            {
                Com.Vector vector = _GetRandomVector(8);
                List<Com.Matrix> matrixList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixList.Add(_GetRandomMatrix(9, 9));
                }

                Com.Matrix[] matrices = matrixList.ToArray();

                Action method = () =>
                {
                    _ = vector.InverseAffineTransformCopy(matrices);
                };

                ExecuteTest(method, "InverseAffineTransformCopy(param Com.Matrix[])", "dimension at 8, total 8 matrices");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1910);
#endif

            {
                Com.Vector vector = _GetRandomVector(8);
                List<Com.Matrix> matrixList = new List<Com.Matrix>(8);

                for (int i = 0; i < 8; i++)
                {
                    matrixList.Add(_GetRandomMatrix(9, 9));
                }

                Action method = () =>
                {
                    _ = vector.InverseAffineTransformCopy(matrixList);
                };

                ExecuteTest(method, "InverseAffineTransformCopy(System.Collections.Generic.List<Com.Matrix>)", "dimension at 8, total 8 matrices");
            }

            // AngleFromBase，AngleFromSpace

#if ComVer1905
            {
                Com.Vector vector = _GetRandomVector(32);
                int index = Com.Statistics.RandomInteger(32);

                Action method = () =>
                {
                    _ = vector.AngleFromBase(index);
                };

                ExecuteTest(method, "AngleFromBase(int)", "dimension at 32");
            }
#else
            {
                Com.Vector vector = _GetRandomVector(32);
                int index = Com.Statistics.RandomInteger(32);

                Action method = () =>
                {
                    _ = vector.AngleOfBasis(index);
                };

                ExecuteTest(method, "AngleOfBasis(int)", "dimension at 32");
            }
#endif

#if ComVer1905
            {
                Com.Vector vector = _GetRandomVector(32);
                int index = Com.Statistics.RandomInteger(32);

                Action method = () =>
                {
                    _ = vector.AngleFromSpace(index);
                };

                ExecuteTest(method, "AngleFromSpace(int)", "dimension at 32");
            }
#else
            {
                Com.Vector vector = _GetRandomVector(32);
                int index = Com.Statistics.RandomInteger(32);

                Action method = () =>
                {
                    _ = vector.AngleOfSpace(index);
                };

                ExecuteTest(method, "AngleOfSpace(int)", "dimension at 32");
            }
#endif
        }

        protected override void StaticMethod()
        {
            // IsNullOrEmpty

#if ComVer1905
            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = Com.Vector.IsNullOrEmpty(vector);
                };

                ExecuteTest(method, "IsNullOrEmpty(Com.Vector)", "dimension at 32");
            }
#else
            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = Com.Vector.IsNullOrNonVector(vector);
                };

                ExecuteTest(method, "IsNullOrNonVector(Com.Vector)", "dimension at 32");
            }
#endif

            // Equals

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = left.Copy();

                Action method = () =>
                {
                    _ = Com.Vector.Equals(left, right);
                };

                ExecuteTest(method, "Equals(Com.Vector, Com.Vector)", "dimension at 32");
            }

            // Compare

#if ComVer1905
            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = left.Copy();

                Action method = () =>
                {
                    _ = Com.Vector.Compare(left, right);
                };

                ExecuteTest(method, "Compare(Com.Vector, Com.Vector)", "dimension at 32");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

            // Zero，Base

            {
                Com.Vector.Type type = Com.Vector.Type.ColumnVector;
                int dimension = 32;

                Action method = () =>
                {
                    _ = Com.Vector.Zero(type, dimension);
                };

                ExecuteTest(method, "Zero(Com.Vector.Type, int)", "dimension at 32");
            }

            {
                int dimension = 32;

                Action method = () =>
                {
                    _ = Com.Vector.Zero(dimension);
                };

                ExecuteTest(method, "Zero(int)", "dimension at 32");
            }

#if ComVer1905
            {
                Com.Vector.Type type = Com.Vector.Type.ColumnVector;
                int dimension = 32;
                int index = Com.Statistics.RandomInteger(32);

                Action method = () =>
                {
                    _ = Com.Vector.Base(type, dimension, index);
                };

                ExecuteTest(method, "Base(Com.Vector.Type, int, int)", "dimension at 32");
            }
#else
            {
                Com.Vector.Type type = Com.Vector.Type.ColumnVector;
                int dimension = 32;
                int index = Com.Statistics.RandomInteger(32);

                Action method = () =>
                {
                    _ = Com.Vector.Basis(type, dimension, index);
                };

                ExecuteTest(method, "Basis(Com.Vector.Type, int, int)", "dimension at 32");
            }
#endif

#if ComVer1905
            {
                int dimension = 32;
                int index = Com.Statistics.RandomInteger(32);

                Action method = () =>
                {
                    _ = Com.Vector.Base(dimension, index);
                };

                ExecuteTest(method, "Base(int, int)", "dimension at 32");
            }
#else
            {
                int dimension = 32;
                int index = Com.Statistics.RandomInteger(32);

                Action method = () =>
                {
                    _ = Com.Vector.Basis(dimension, index);
                };

                ExecuteTest(method, "Basis(int, int)", "dimension at 32");
            }
#endif

            // Matrix

#if ComVerNext
            {
                Com.Vector.Type type = Com.Vector.Type.ColumnVector;
                int dimension = 32;
                int index = Com.Statistics.RandomInteger(32);
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.Vector.OffsetMatrix(type, dimension, index, d);
                };

                ExecuteTest(method, "OffsetMatrix(Com.Vector.Type, int, int, double)", "dimension at 32");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                Com.Vector.Type type = Com.Vector.Type.ColumnVector;
                int dimension = 32;
                double d = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.Vector.OffsetMatrix(type, dimension, d);
                };

                ExecuteTest(method, "OffsetMatrix(Com.Vector.Type, int, double)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = Com.Vector.OffsetMatrix(vector);
                };

                ExecuteTest(method, "OffsetMatrix(Com.Vector)", "dimension at 32");
            }

#if ComVerNext
            {
                Com.Vector.Type type = Com.Vector.Type.ColumnVector;
                int dimension = 32;
                int index = Com.Statistics.RandomInteger(32);
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.Vector.ScaleMatrix(type, dimension, index, s);
                };

                ExecuteTest(method, "ScaleMatrix(Com.Vector.Type, int, int, double)", "dimension at 32");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVerNext);
#endif

            {
                Com.Vector.Type type = Com.Vector.Type.ColumnVector;
                int dimension = 32;
                double s = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = Com.Vector.ScaleMatrix(type, dimension, s);
                };

                ExecuteTest(method, "ScaleMatrix(Com.Vector.Type, int, double)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = Com.Vector.ScaleMatrix(vector);
                };

                ExecuteTest(method, "ScaleMatrix(Com.Vector)", "dimension at 32");
            }

#if ComVer1905
            {
                Com.Vector.Type type = Com.Vector.Type.ColumnVector;
                int dimension = 32;
                int index = Com.Statistics.RandomInteger(32);

                Action method = () =>
                {
                    _ = Com.Vector.ReflectMatrix(type, dimension, index);
                };

                ExecuteTest(method, "ReflectMatrix(Com.Vector.Type, int, int)", "dimension at 32");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

#if ComVer1905
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

                ExecuteTest(method, "ShearMatrix(Com.Vector.Type, int, int, int, double)", "dimension at 32");
            }
#else
            ExecuteTest(UnsupportedReason.NeedComVer1905);
#endif

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

                ExecuteTest(method, "RotateMatrix(Com.Vector.Type, int, int, int, double)", "dimension at 32");
            }

            // 距离与夹角

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = Com.Vector.DistanceBetween(left, right);
                };

                ExecuteTest(method, "DistanceBetween(Com.Vector, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = Com.Vector.AngleBetween(left, right);
                };

                ExecuteTest(method, "AngleBetween(Com.Vector, Com.Vector)", "dimension at 32");
            }

            // 向量乘积

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = Com.Vector.DotProduct(left, right);
                };

                ExecuteTest(method, "DotProduct(Com.Vector, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = Com.Vector.CrossProduct(left, right);
                };

                ExecuteTest(method, "CrossProduct(Com.Vector, Com.Vector)", "dimension at 32");
            }

            // 初等函数

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = Com.Vector.Abs(vector);
                };

                ExecuteTest(method, "Abs(Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = Com.Vector.Sign(vector);
                };

                ExecuteTest(method, "Sign(Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = Com.Vector.Ceiling(vector);
                };

                ExecuteTest(method, "Ceiling(Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = Com.Vector.Floor(vector);
                };

                ExecuteTest(method, "Floor(Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = Com.Vector.Round(vector);
                };

                ExecuteTest(method, "Round(Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = Com.Vector.Truncate(vector);
                };

                ExecuteTest(method, "Truncate(Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = Com.Vector.Max(left, right);
                };

                ExecuteTest(method, "Max(Com.Vector, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = Com.Vector.Min(left, right);
                };

                ExecuteTest(method, "Min(Com.Vector, Com.Vector)", "dimension at 32");
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

                ExecuteTest(method, "operator ==(Com.Vector, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = (left != right);
                };

                ExecuteTest(method, "operator !=(Com.Vector, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = (left < right);
                };

                ExecuteTest(method, "operator <(Com.Vector, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = (left > right);
                };

                ExecuteTest(method, "operator >(Com.Vector, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = (left <= right);
                };

                ExecuteTest(method, "operator <=(Com.Vector, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = (left >= right);
                };

                ExecuteTest(method, "operator >=(Com.Vector, Com.Vector)", "dimension at 32");
            }

            // 运算

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = +vector;
                };

                ExecuteTest(method, "operator +(Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector vector = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = -vector;
                };

                ExecuteTest(method, "operator -(Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector pt = _GetRandomVector(32);
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt + n;
                };

                ExecuteTest(method, "operator +(Com.Vector, double)", "dimension at 32");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.Vector pt = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = n + pt;
                };

                ExecuteTest(method, "operator +(double, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = left + right;
                };

                ExecuteTest(method, "operator +(Com.Vector, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector pt = _GetRandomVector(32);
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt - n;
                };

                ExecuteTest(method, "operator -(Com.Vector, double)", "dimension at 32");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.Vector pt = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = n - pt;
                };

                ExecuteTest(method, "operator -(double, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = left - right;
                };

                ExecuteTest(method, "operator -(Com.Vector, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector pt = _GetRandomVector(32);
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt * n;
                };

                ExecuteTest(method, "operator *(Com.Vector, double)", "dimension at 32");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.Vector pt = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = n * pt;
                };

                ExecuteTest(method, "operator *(double, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = left * right;
                };

                ExecuteTest(method, "operator *(Com.Vector, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector pt = _GetRandomVector(32);
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);

                Action method = () =>
                {
                    _ = pt / n;
                };

                ExecuteTest(method, "operator /(Com.Vector, double)", "dimension at 32");
            }

            {
                double n = Com.Statistics.RandomDouble(-1E18, 1E18);
                Com.Vector pt = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = n / pt;
                };

                ExecuteTest(method, "operator /(double, Com.Vector)", "dimension at 32");
            }

            {
                Com.Vector left = _GetRandomVector(32);
                Com.Vector right = _GetRandomVector(32);

                Action method = () =>
                {
                    _ = left / right;
                };

                ExecuteTest(method, "operator /(Com.Vector, Com.Vector)", "dimension at 32");
            }
        }
    }
}