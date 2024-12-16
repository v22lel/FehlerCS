using System.Text;

namespace Fehler;

public class FehlerParseException : FehlerException
{
    internal FehlerParseException(string errmsg): base(errmsg) {}
    
    internal static bool Expected(string token, object got, int line)
    {
        if (token.Equals(got.ToString()))
        {
            return true;
        }
        throw new FehlerParseException($"Error when parsing Exceptions.ini: Expected '{token}', got '{got}'. [line:{line}]");
    }

    internal static bool NotEmpty(string toCheck, string stringName, int line)
    {
        if (toCheck.Length != 0)
        {
            return true;
        }

        throw new FehlerParseException($"Error when parsing Exceptions.ini: '{stringName}' cannot be empty! [line:{line}]");
    }

    internal static void TypeNotFound(string type, int line)
    {
        throw new FehlerParseException($"Error when parsing Exceptions.ini: Exception Type '{type}' could not be resolved! [line:{line}]");
    }
    
    internal static void TypeNotClass(string type, int line)
    {
        throw new FehlerParseException($"Error when parsing Exceptions.ini: Exception Type '{type}' has to be a class! [line:{line}]");
    }
    
    internal static void TypeAbstract(string type, int line)
    {
        throw new FehlerParseException($"Error when parsing Exceptions.ini: Exception Type '{type}' cannot be abstract! [line:{line}]");
    }
    
    internal static void TypeNotException(string type, int line)
    {
        throw new FehlerParseException($"Error when parsing Exceptions.ini: Exception Type '{type}' has to somehow inherit Exception! [line:{line}]");
    }
    
    internal static void TypeNoConstructor(string type, int line)
    {
        throw new FehlerParseException($"Error when parsing Exceptions.ini: Exception Type '{type}' has no valid constructor! Expected: public {type}(string message) {{...}} [line:{line}]");
    }
}


file enum ParseState
{
    Name,
    Type,
    Format
}

public class ExceptionList
{
    internal Dictionary<string, RegisteredException> Registry { private set; get; }
    
    internal Dictionary<string, string> _aliases;
    private string _currentHeading;

    internal ExceptionList()
    {
        Registry = new Dictionary<string, RegisteredException>();
        _currentHeading = "Index";
        _aliases = new Dictionary<string, string>();
    }

    internal void Initialize(StreamReader configStream)
    {
        var i = 1;
        while (!configStream.EndOfStream)
        {
            var line = configStream.ReadLine();
            if (line != null)
            {
                ParseLine(line, i++);
            }
        }
    }

    private void ParseLine(string line, int i)
    {
        if (line.Length == 0 || line.StartsWith('#')) return;
        if (line.StartsWith('[')) ParseHeading(line, i);
        else
        {
            if (_currentHeading == "Types")
            {
                var nameBuilder = new StringBuilder();
                var typeBuilder = new StringBuilder();
                var state = ParseState.Name;
                foreach (var c in line)
                {
                    if (c == '=')
                    {
                        state = ParseState.Type;
                        continue;
                    }

                    switch (state)
                    {
                        case ParseState.Name:
                            nameBuilder.Append(c);
                            break;
                        case ParseState.Type:
                            typeBuilder.Append(c);
                            break;
                    }
                }
                var name = nameBuilder.ToString();
                var type = typeBuilder.ToString();
                FehlerParseException.NotEmpty(name, "Name", i);
                if (type.Length == 0)
                {
                    type = "Fehler.FehlerException";
                }
                _aliases.Add(name, type);
            }
            else
            {
                var nameBuilder = new StringBuilder();
                var typeBuilder = new StringBuilder();
                var formatBuilder = new StringBuilder();
                var state = ParseState.Name;
                foreach (var c in line)
                {
                    if (c == '=')
                    {
                        state = ParseState.Format;
                        continue;
                    }

                    if (c == '!')
                    {
                        state = ParseState.Type;
                        continue;
                    }

                    switch (state)
                    {
                        case ParseState.Name:
                            nameBuilder.Append(c);
                            break;
                        case ParseState.Type:
                            typeBuilder.Append(c);
                            break;
                        case ParseState.Format:
                            formatBuilder.Append(c);
                            break;
                    }
                }

                var name = nameBuilder.ToString();
                var type = typeBuilder.ToString();
                var format = formatBuilder.ToString();
            
                FehlerParseException.NotEmpty(name, "Name", i);
                if (type.Length == 0)
                {
                    type = "Fehler.FehlerException";
                }

                if (_aliases.TryGetValue(type, out var alias))
                {
                    type = alias;
                }
                var reg = new RegisteredException(format, type, i);
                Registry.Add(name, reg);
            }
        }
    }

    private void ParseHeading(string line, int i)
    {
        var en = line.GetEnumerator();
        if (!en.MoveNext())
        {
            en.Dispose();
            return;
        }
        if (!FehlerParseException.Expected("[", en.Current, i))
        {
            en.Dispose();
            return;
        };
        var atEnd = false;
        var builder = new StringBuilder();
        while (en.MoveNext())
        {
            var c = en.Current;
            if (c == ']')
            {
                atEnd = true;
                break;
            }
            builder.Append(c);
        }

        if (!atEnd)
        {
            en.Dispose();
            FehlerParseException.Expected("]", en.Current, i);
        }

        var name = builder.ToString();
        _currentHeading = name;
    }
}