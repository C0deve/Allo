using System.Collections.Concurrent;

namespace Allo;

public class Executeur
{
    public int ActionsAFaireCount => _actions.Count;
    private readonly ConcurrentDictionary<Type, object> _actions = new();
    public Executeur AjouterUneActionAFaire<T>(Action<T> action)
    {
        _actions.TryAdd(typeof(T), action);
        return this;
    }

    public Executeur Execute<T>(T parametre)
    {
        if (_actions.TryGetValue(typeof(T), out var action))
            ((Action<T>)action)(parametre);
        
        return this;
    }
}