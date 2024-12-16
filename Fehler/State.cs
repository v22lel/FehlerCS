using System.Reflection;

namespace Fehler;

internal class RegisteredException
{
    private string _format;
    private string _type;
    private ConstructorInfo _constructor;

    public RegisteredException(string format, string type, int line)
    {
        _format = format;
        _type = type;

        var found = Type.GetType(_type);
        if (found == null)
        {
            FehlerParseException.TypeNotFound(type, line);
        }
        if (!found.IsClass)
        {
            FehlerParseException.TypeNotClass(type, line);
        }
        if (found.IsAbstract)
        {
            FehlerParseException.TypeAbstract(type, line);
        }
        if (!found.IsAssignableTo(typeof(FehlerException)))
        {
            FehlerParseException.TypeNotException(type, line);
        }

        var constructor = found.GetConstructor([typeof(string)]);
        if (constructor == null)
        {
            FehlerParseException.TypeNoConstructor(type, line);
        }

        _constructor = constructor;
    }

    internal void Throw(params object?[] args)
    {
        var message = string.Format(_format, args);
        
        var e = (Exception)_constructor.Invoke([message]);
        Fehler.OnThrow(e);
    }
}