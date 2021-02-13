using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Globalization;
using Flow.Launcher.Plugin;

namespace Flow.Launcher.Plugin.BaseConverter
{
    public class BaseConverter : IPlugin
    {
        private PluginInitContext _context;

        public void Init(PluginInitContext context)
        {
            _context = context;
        }

        public List<Result> Query(Query query)
        {
            List<Result> results = new List<Result>();

            Dictionary<String, Tuple<int, String>> lut = new Dictionary<String, Tuple<int, String>>(){
                {"c~", new Tuple<int, string>(-3, "{0:CHAR}")},
                {"f~", new Tuple<int, string>(-2, "{0:DFP}")},
                {"s~", new Tuple<int, string>(-1, "{0:SFP}")},
                {"e~", new Tuple<int, string>(0,  "{0:CALC}")},
                {"b~", new Tuple<int, string>(2,  "{0:BIN}")},
                {"o~", new Tuple<int, string>(8,  "{0:OCT}")},
                {"d~", new Tuple<int, string>(10, "{0:DEC}")},
                {"x~", new Tuple<int, string>(16, "{0:HEX}")},
            };

            Tuple<int, String> fromPattern;
            string prefix = query.Search.Substring(0, 2);
            string input = query.Search.Substring(2);

            /* try match prefix pattern with fromBase */
            if (false == lut.TryGetValue(prefix, out fromPattern))
            {
                return results;
            }

            /* calc convert results */
            foreach (var toPattern in lut.Values)
            {
                if (toPattern.Item1 <= 0)
                    continue;

                string num = Converter(input, fromPattern, toPattern);
                if (num == null)
                    continue;

                results.Add(new Result()
                {
                    Title = " " + fromPattern.Item1 + " -> " + toPattern.Item1 + ":\t" + num,
                    SubTitle = "Enter to copy to the clipboard",
                    Action = e =>
                    {
                        Clipboard.SetText(num);
                        return true;
                    }
                });
            }

            return results;
        }

        public struct Pattern
        {
            public int numBase;
            public String format;
        }

        private double Evaluate(string expr)
        {
            DataTable table = new DataTable();

            table.Columns.Add("expression", string.Empty.GetType(), expr);
            table.Rows.Add(table.NewRow());

            return double.Parse((string)table.Rows[0]["expression"]);
        }

        private string Converter(string input, Tuple<int, String> fromPattern, Tuple<int, String> toPattern)
        {
            try
            {
                Int64 value = Convert.ToInt64(input, fromPattern.Item1);
                return String.Format(new BaseFormatter(), toPattern.Item2, value);
            }
            catch (ArgumentException)
            {
                return null;
            }
            catch (FormatException)
            {
                return null;
            }
            catch (OverflowException)
            {
                return null;
            }
        }

        private class BaseFormatter : IFormatProvider, ICustomFormatter
        {
            public object GetFormat(Type format)
            {
                if (typeof(ICustomFormatter) == format)
                {
                    return this;
                }

                return null;
            }

            public string Format(string format, object arg, IFormatProvider provider)
            {
                if (null == format)
                {
                    if (arg is IFormattable)
                        return ((IFormattable)arg).ToString(format, provider);
                    else
                        return arg.ToString();
                }
                else
                {
                    Int32 bitWidth = Convert.ToString((Int64)arg, 2).Length;

                    switch (format)
                    {
                        case "BIN":
                        {
                            String result = Convert.ToString(((Int64)arg), 2);

                            Int32 alignWidth = (Int32)Math.Ceiling(bitWidth / 8.0);
                            Int32 padWidth = (Int32)(8 * alignWidth);
                            result = Regex.Replace(result.PadLeft(padWidth, '0'),
                                                   string.Format(@".{{{0}}}", 8), "$0" + "_",
                                                   RegexOptions.RightToLeft);

                            return bitWidth + "'b " + result.TrimEnd('_');
                        }
                        case "OCT":
                        {
                            return bitWidth + "'o " + Convert.ToString(((Int64)arg), 8);
                        }
                        case "DEC":
                        {
                            NumberFormatInfo nfi = (NumberFormatInfo)Thread.CurrentThread.CurrentCulture.NumberFormat.Clone();

                            /* format no digits */
                            nfi.NumberDecimalDigits = 0;

                            return bitWidth + "'d " + ((Int64)arg).ToString("N", nfi);
                        }
                        case "HEX":
                        {
                            String result = ((Int64)arg).ToString("X");

                            Int32 alignWidth = (Int32)Math.Ceiling(bitWidth / 16.0);
                            Int32 padWidth = (Int32)(4 *alignWidth);
                            result = Regex.Replace(result.PadLeft(padWidth, '0'),
                                                   string.Format(@".{{{0}}}", 4), "$0" + "_",
                                                   RegexOptions.RightToLeft);

                            return bitWidth + "'h " + result.TrimEnd('_');
                        }
                        default:
                        {
                            if (arg is IFormattable)
                                return ((IFormattable)arg).ToString(format, provider);
                            else
                                return null;
                        }
                    }
                }
            }
        }
    }
}