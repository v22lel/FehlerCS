using System.Reflection;

namespace Fehler;

public class Fehler
{
    private static ExceptionList? list;
    private static Action<Exception> action;

    public static void Initialize()
    {
        Initialize(e => throw e);
    }
    
    public static void Initialize(Action<Exception> exceptionHandler)
    {
        list = new ExceptionList();
        try
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Exceptions.ini");
            var stream = File.Open(path, FileMode.Open);
            list.Initialize(new StreamReader(stream));
        }
        catch (FehlerException e)
        {
            throw new FehlerParseException("Exceptions.ini not be found!");
        }

        action = exceptionHandler;
    }
    
    public static void Werfen(string ex, params object?[] args)
    {
        if (list == null)
        {
            throw new FehlerParseException("You haven't called Initialize() yet!");
        }

        RegisteredException? reg;
        list.Registry.TryGetValue(ex, out reg);
        if (reg == null)
        {
            throw new FehlerParseException($"Exception '{ex}' not found in Exceptions.ini!");
        }
        reg.Throw(args);
    }

    internal static void OnThrow(Exception incoming)
    {
        action(incoming);
    }
}