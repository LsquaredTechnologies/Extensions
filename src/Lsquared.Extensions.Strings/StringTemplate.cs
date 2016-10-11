using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace Lsquared
{
    [Pure]
    public sealed class StringTemplate
    {
        private StringTemplate(string template, string format, Parameter[] tokens)
        {
            _template = template;
            _format = format;
            _parameters = tokens;
        }

        public static StringTemplate Create(string template)
        {
            var tokens = new List<Parameter>();
            var format = ParseTemplate(template, tokens);
            return new StringTemplate(template, format, tokens.ToArray());
        }

        public string Format(object source)
        {
            object[] args;
            if ((args = source as object[]) != null)
            {
                return string.Format(null, _format, args);
            }
            else
            {
                return Format(null, ObjectToDictionary(source));
            }
        }

        public string Format(IFormatProvider provider, object source)
        {
            object[] args;
            if ((args = source as object[]) != null)
            {
                return string.Format(provider, _format, args);
            }
            else
            {
                return Format(provider, ObjectToDictionary(source));
            }
        }

        public string Format(IDictionary<string, object> args)
        {
            if (args == null) { throw new ArgumentNullException(nameof(args)); }
            return Format(null, args.TryGetValue);
        }

        public string Format(IFormatProvider provider, IDictionary<string, object> args)
        {
            if (args == null) { throw new ArgumentNullException(nameof(args)); }
            return Format(provider, args.TryGetValue);
        }

        public string Format(IFormatProvider provider, GetValue getValue)
        {
            if (getValue == null) { throw new ArgumentNullException(nameof(getValue)); }

            ICustomFormatter formatter = null;
            if (provider != null)
            {
                formatter = (ICustomFormatter)provider.GetFormat(typeof(ICustomFormatter));
            }

            var index = 0;
            var b = new StringBuilder(_template.Length * 2);
            foreach (var token in _parameters)
            {
                b.Append(_template.Substring(index, token.Index - index));
                object arg;
                if (!getValue(token.Name, out arg))
                {
                    throw new FormatException(/*SR.StringTemplate_ArgumentNotFound*/);
                }
                string argumentValue = null;
                if (formatter != null)
                {
                    argumentValue = formatter.Format(token.Format, arg, provider);
                }
                if (argumentValue == null)
                {
                    var formattableArg = arg as IFormattable;
                    if (formattableArg != null)
                    {
                        argumentValue = formattableArg.ToString(token.Format, provider);
                    }
                    else if (arg != null)
                    {
                        argumentValue = arg.ToString();
                    }
                }
                if (argumentValue == null)
                {
                    argumentValue = string.Empty;
                }
                int paddingCount = token.Width - argumentValue.Length;
                if (!token.LeftAlign && (paddingCount > 0))
                {
                    b.Append(' ', paddingCount);
                }
                b.Append(argumentValue);
                if (token.LeftAlign && (paddingCount > 0))
                {
                    b.Append(' ', paddingCount);
                }

                index = token.Index + token.Length;
            }

            b.Append(_template.Substring(index));

            return b.ToString();
        }

        public override string ToString()
        {
            return _template;
        }

        private static string ParseTemplate(string template, IList<Parameter> tokens)
        {
            var chArray = template.ToCharArray();
            var length = chArray.Length;
            var index = 0;
            var b = new StringBuilder();
            var argumentIndex = 0;
            var nameToIndex = new Dictionary<string, int>();

            char ch;
            while (index < length)
            {
                ch = chArray[index];
                index++;

                if (ch == '}')
                {
                    if ((index < length) && (chArray[index] == '}'))
                    {
                        // Literal close curly brace
                        b.Append('}');
                        index++;
                    }
                    else
                    {
                        throw new FormatException(/*SR.StringTemplate_InvalidString*/);
                    }
                }
                else if (ch == '{')
                {
                    if ((index < length) && (chArray[index] == '{'))
                    {
                        // Literal open curly brace
                        b.Append('{');
                        index++;
                    }
                    else
                    {
                        // Template token:
                        if (index == length)
                        {
                            throw new FormatException(/*SR.StringTemplate_InvalidString*/);
                        }

                        // Argument name:
                        var nameStart = index;
                        ch = chArray[index];
                        index++;
                        // Check start character.
                        if (!(ch == '_' ||
                            ((ch >= 'a') && (ch <= 'z')) ||
                            ((ch >= 'A') && (ch <= 'Z'))))
                        {
                            throw new FormatException(/*SR.StringTemplate_InvalidString*/);
                        }
                        // Check all other characters.
                        while ((index < length) &&
                                (ch == '.' || ch == '-' || ch == '_' ||
                                ((ch >= '0') && (ch <= '9')) ||
                                ((ch >= 'a') && (ch <= 'z')) ||
                                ((ch >= 'A') && (ch <= 'Z'))))
                        {
                            ch = chArray[index];
                            index++;
                        }

                        var nameEnd = index - 1;
                        if (nameEnd == nameStart)
                        {
                            throw new FormatException(/*SR.StringTemplate_InvalidString*/);
                        }

                        var argumentName = new string(chArray, nameStart, nameEnd - nameStart);
                        b.Append('{');
                        if (nameToIndex.TryGetValue(argumentName, out argumentIndex))
                        {
                            b.Append(argumentIndex);
                        }
                        else
                        {
                            b.Append(argumentIndex++);
                            nameToIndex[argumentName] = argumentIndex;
                        }

                        // Skip blanks
                        while ((index < length) && (ch == ' '))
                        {
                            ch = chArray[index];
                            index++;
                        }

                        // Argument alignment
                        var width = 0;
                        var leftAlign = false;
                        if (ch == ',')
                        {
                            if (index == length)
                            {
                                throw new FormatException(/*SR.StringTemplate_InvalidString*/);
                            }
                            ch = chArray[index];
                            index++;
                            while ((index < length) && (ch == ' '))
                            {
                                ch = chArray[index];
                                index++;
                            }
                            if (index == length)
                            {
                                throw new FormatException(/*SR.StringTemplate_InvalidString*/);
                            }
                            if (ch == '-')
                            {
                                leftAlign = true;
                                if (index == length)
                                {
                                    throw new FormatException(/*SR.StringTemplate_InvalidString*/);
                                }
                                ch = chArray[index];
                                index++;
                            }
                            if ((ch < '0') || (ch > '9'))
                            {
                                throw new FormatException(/*SR.StringTemplate_InvalidString*/);
                            }
                            while ((index < length) && (ch >= '0') && (ch <= '9'))
                            {
                                // TODO: What if number too large for Int32, i.e. throw exception
                                width = (width * 10) + (ch - 0x30);
                                ch = chArray[index];
                                index++;
                            }
                        }

                        // Skip blanks
                        while ((index < length) && (ch == ' '))
                        {
                            ch = chArray[index];
                            index++;
                        }

                        // Format string
                        int formatEnd = nameEnd;
                        var formatString = (string)null;
                        if (ch == ':')
                        {
                            if (index == length)
                            {
                                throw new FormatException(/*SR.StringTemplate_InvalidString*/);
                            }
                            var formatStart = index;
                            ch = chArray[index];
                            index++;
                            while ((index < length) && (ch != '{') && (ch != '}'))
                            {
                                ch = chArray[index];
                                index++;
                            }
                            formatEnd = index - 1;
                            if (formatEnd >= formatStart)
                            {
                                formatString = new string(chArray, formatStart, formatEnd - formatStart);
                            }
                            b.Append(formatString);
                        }

                        // Insert formatted argument
                        if (ch != '}')
                        {
                            throw new FormatException(/*SR.StringTemplate_InvalidString*/);
                        }

                        b.Append('}');

                        var token = new Parameter { Index = nameStart - 1, Name = argumentName, Format = formatString, Length = formatEnd - nameStart + 2, Width = width, LeftAlign = leftAlign };
                        tokens.Add(token);
                    }
                }
                else
                {
                    // Literal -- scan up until next curly brace
                    var literalStart = index - 1;
                    while (index < length)
                    {
                        ch = chArray[index];
                        if (ch == '{' || ch == '}')
                        {
                            break;
                        }
                        index++;
                    }
                    b.Append(chArray, literalStart, index - literalStart);
                }
            }

            return b.ToString();
        }

        #region Fields

        private readonly string _template;
        private readonly string _format;
        private readonly Parameter[] _parameters;

        #endregion

        #region Nested

        private struct Parameter
        {
            public int Index;
            public int Length;
            public int Width;
            public bool LeftAlign;
            public string Name;
            public string Format;
        }

        #endregion
    }
}
