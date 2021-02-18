using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;

/// <summary>
/// Used to modify <see cref="PlayerLoopSystem"/>
/// </summary>
public class ConstructedPlayerLoopEdit
{
    public delegate bool ExecuteDelegate(ref PlayerLoopSystem system, List<ExecuteDelegate> commands, int commandIndex);

    protected readonly List<ExecuteDelegate> _commands;

    public ConstructedPlayerLoopEdit(int capacity = 2)
    {
        if (capacity == 0)
            _commands = new List<ExecuteDelegate>();
        else
            _commands = new List<ExecuteDelegate>(capacity);
    }

    /// <summary>
    /// Will try to apply the constructed command
    /// </summary>
    /// <param name="system"></param>
    /// <returns>true if it changed something</returns>
    public bool TryApply(ref PlayerLoopSystem system)
    {
        if (_commands.Count == 0)
        {
            Debug.LogError("Nothing to execute!");
            return false;
        }

        var first = _commands[0];
        return first(ref system, _commands, 1);
    }


    /// <summary>
    /// </summary>
    /// <param name="commands"></param>
    /// <param name="loopSystem"></param>
    /// <returns>true if any changes were made to <paramref name="loopSystem"/></returns>
    public static bool Apply(IEnumerable<ConstructedPlayerLoopEdit> commands, ref PlayerLoopSystem loopSystem)
    {
        var changed = false;
        foreach (var builder in commands)
            changed |= builder.TryApply(ref loopSystem);

        return changed;
    }

}

/// <summary>
/// Used to modify <see cref="PlayerLoopSystem"/>
/// </summary>
public class PlayerLoopEdit : ConstructedPlayerLoopEdit
{
    public PlayerLoopEdit(int capacity = 2) : base(capacity)
    {

    }

    private static bool ExecuteEnter<T>(ref PlayerLoopSystem system, List<ExecuteDelegate> commands, int nextCommandIndex)
    {
        if (nextCommandIndex >= commands.Count)
        {
            Debug.LogError("Nothing to execute!");
            return false;
        }

        var toExecute = commands[nextCommandIndex];

        var array = system.subSystemList;

        for (var i = 0; i < array.Length; i++)
        {
            var child = array[i];

            if (child.type != typeof(T))
                continue;

            if (toExecute(ref child, commands, nextCommandIndex + 1))
            {
                array[i] = child;
                return true;
            }
        }

        Debug.LogError($"Type not found {typeof(T)}");
        return false;
    }

    public PlayerLoopEdit Enter<T>()
    {
        _commands.Add(ExecuteEnter<T>);
        return this;
    }

    public ConstructedPlayerLoopEdit Add<T>(PlayerLoopSystem.UpdateFunction updateFunction)
    {
        bool Execute(ref PlayerLoopSystem system, List<ExecuteDelegate> commands, int nextCommandIndex)
        {
            Debug.Assert(commands.Count == nextCommandIndex);

            var instance = new PlayerLoopSystem
            {
                type = typeof(T),
                updateDelegate = updateFunction
            };

            if (system.subSystemList == null)
            {
                system.subSystemList = new[]
                {
                    instance
                };
            }
            else
                system.subSystemList = ArrayExtensions.Add(system.subSystemList, instance);

            return true;
        }

        _commands.Add(Execute);

        return this;
    }

    public ConstructedPlayerLoopEdit InsertBefore<TFilter, T>(PlayerLoopSystem.UpdateFunction updateFunction)
    {
        bool Execute(ref PlayerLoopSystem system, List<ExecuteDelegate> commands, int nextCommandIndex)
        {
            Debug.Assert(commands.Count == nextCommandIndex);

            if (system.subSystemList == null)
            {
                Debug.LogError($"Cannot insert {typeof(T)} before {typeof(TFilter)} because sub system list is null!");
                return false;
            }

            var index = -1;

            var length = system.subSystemList.Length;
            for (var i = 0; i < length; i++)
            {
                if (system.subSystemList[i].type == typeof(TFilter))
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
            {
                Debug.LogError($"Cannot insert {typeof(T)} before {typeof(TFilter)} because item can't be found!");
                return false;
            }

            var instance = new PlayerLoopSystem
            {
                type = typeof(T),
                updateDelegate = updateFunction
            };

            system.subSystemList = ArrayExtensions.Insert(system.subSystemList, instance, index);

            return true;
        }

        _commands.Add(Execute);

        return this;
    }

    public ConstructedPlayerLoopEdit InsertAfter<TFilter, T>(PlayerLoopSystem.UpdateFunction updateFunction)
    {
        bool Execute(ref PlayerLoopSystem system, List<ExecuteDelegate> commands, int nextCommandIndex)
        {
            Debug.Assert(commands.Count == nextCommandIndex);

            if (system.subSystemList == null)
            {
                Debug.LogError($"Cannot insert {typeof(T)} before {typeof(TFilter)} because sub system list is null!");
                return false;
            }

            var index = -1;

            var length = system.subSystemList.Length;
            for (var i = 0; i < length; i++)
            {
                if (system.subSystemList[i].type == typeof(TFilter))
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
            {
                Debug.LogError($"Cannot insert {typeof(T)} before {typeof(TFilter)} because item can't be found!");
                return false;
            }

            var instance = new PlayerLoopSystem
            {
                type = typeof(T),
                updateDelegate = updateFunction
            };

            system.subSystemList = ArrayExtensions.Insert(system.subSystemList, instance, index + 1);

            return true;
        }

        _commands.Add(Execute);

        return this;
    }


    //maybe add remove commands too
}