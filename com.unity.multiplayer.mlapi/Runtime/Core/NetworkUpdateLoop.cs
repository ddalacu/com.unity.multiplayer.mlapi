using System;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using System.Collections.Generic;


public interface INetworkUpdateSystem
{
    void NetworkUpdate(NetworkUpdateStage updateStage);
}

public enum NetworkUpdateStage : byte
{
    Initialization = 1,
    EarlyUpdate = 2,
    FixedUpdate = 3,
    PreUpdate = 4,
    Update = 0,
    PreLateUpdate = 5,
    PostLateUpdate = 6,
    Unknown = 255//used when not inside <see cref="INetworkUpdateSystem.NetworkUpdate"/>
}


public static class NetworkUpdateLoop
{
    private static List<INetworkUpdateSystem>[] Stages;

    private static List<uint>[] IDs;

    private static List<UpdateHandles>[] Handles;


    public static uint[] IDCounters;


    public const int StageCount = 7;

    /// <summary>
    /// Current active stage, have a valid value only when accessed from <see cref="INetworkUpdateSystem.NetworkUpdate"/>
    /// </summary>
    public static NetworkUpdateStage UpdateStage { get; private set; }

    [RuntimeInitializeOnLoadMethod]
    private static void Initialize()
    {
        IDCounters = new uint[StageCount];
        IDs = new List<uint>[StageCount];
        Stages = new List<INetworkUpdateSystem>[StageCount];
        Handles = new List<UpdateHandles>[StageCount];

        for (var i = 0; i < StageCount; i++)
        {
            Stages[i] = new List<INetworkUpdateSystem>(64);
            IDs[i] = new List<uint>(64);
            Handles[i] = new List<UpdateHandles>(64);
        }

        var customPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
        if (ConstructedPlayerLoopEdit.Apply(GetDefaultEdits(), ref customPlayerLoop))
            PlayerLoop.SetPlayerLoop(customPlayerLoop);
    }

    public class UpdateHandles : IDisposable
    {
        private readonly INetworkUpdateSystem _system;

        private readonly uint[] _identifiers;

        public UpdateHandles(INetworkUpdateSystem system)
        {
            _system = system;
            _identifiers = new uint[StageCount];
        }


        public void RegisterAll()
        {
            for (var index = 0; index < StageCount; index++)
                Register((NetworkUpdateStage)index);
        }

        public void UnregisterAll()
        {
            for (var index = 0; index < StageCount; index++)
                Unregister((NetworkUpdateStage)index);
        }

        public bool Register(NetworkUpdateStage updateStage = NetworkUpdateStage.Update)
        {
            if (_identifiers[(int)updateStage] != 0)
                return false;

            var index = (int)updateStage;

            IDCounters[index]++;
            _identifiers[(int)updateStage] = IDCounters[(int)updateStage];

            Stages[index].Add(_system);
            IDs[index].Add(_identifiers[(int)updateStage]);
            Handles[index].Add(this);

            return true;
        }

        public static int BinarySearch(List<uint> list, uint key)
        {
            int min = 0;
            int max = list.Count - 1;
            while (min <= max)
            {
                int mid = (min + max) / 2;
                if (key == list[mid])
                {
                    return mid;
                }

                if (key < list[mid])
                    max = mid - 1;
                else
                    min = mid + 1;
            }

            throw new Exception("Id not found");
        }

        public bool Unregister(NetworkUpdateStage updateStage = NetworkUpdateStage.Update)
        {
            var identifier = _identifiers[(int)updateStage];
            if (identifier == 0)
                return false;

            var index = (int)updateStage;

            var idsList = IDs[index];
            var handleList = Handles[index];
            var stageList = Stages[index];

            var mid = BinarySearch(idsList, identifier);

            var lastHandle = handleList[handleList.Count - 1];

            lastHandle._identifiers[index] = _identifiers[index];

            handleList[mid] = lastHandle;
            stageList[mid] = stageList[stageList.Count - 1];

            handleList.RemoveAt(handleList.Count - 1);
            idsList.RemoveAt(idsList.Count - 1);
            stageList.RemoveAt(stageList.Count - 1);

            _identifiers[index] = 0;

            return true;
        }

        public void Dispose()
        {
            UnregisterAll();
        }
    }

    public static UpdateHandles CreateUpdateHandles(this INetworkUpdateSystem system)
    {
        return new UpdateHandles(system);
    }

    private static IEnumerable<ConstructedPlayerLoopEdit> GetDefaultEdits()
    {
        var builders = new[]
        {
            NetworkInitialization.CreateEdit(),
            NetworkEarlyUpdate.CreateEdit(),
            NetworkFixedUpdate.CreateEdit(),
            NetworkPreUpdate.CreateEdit(),
            NetworkUpdate.CreateEdit(),
            NetworkPreLateUpdate.CreateEdit(),
            NetworkPostLateUpdate.CreateEdit()
        };

        return builders;
    }

    public struct NetworkInitialization
    {
        public static ConstructedPlayerLoopEdit CreateEdit()
        {
            return new PlayerLoopEdit()
                .Enter<Initialization>()
                .Add<NetworkInitialization>(() =>
                {
                    RunStage(NetworkUpdateStage.Initialization);
                });
        }
    }

    public struct NetworkEarlyUpdate
    {
        public static ConstructedPlayerLoopEdit CreateEdit()
        {
            return new PlayerLoopEdit()
                .Enter<EarlyUpdate>()
                .InsertBefore<EarlyUpdate.ScriptRunDelayedStartupFrame, NetworkEarlyUpdate>(() =>
                {
                    RunStage(NetworkUpdateStage.EarlyUpdate);
                });
        }
    }

    public struct NetworkFixedUpdate
    {
        public static ConstructedPlayerLoopEdit CreateEdit()
        {
            return new PlayerLoopEdit()
                .Enter<FixedUpdate>()
                .InsertBefore<FixedUpdate.ScriptRunBehaviourFixedUpdate, NetworkFixedUpdate>(() =>
                {
                    RunStage(NetworkUpdateStage.FixedUpdate);
                });
        }
    }

    public struct NetworkPreUpdate
    {
        public static ConstructedPlayerLoopEdit CreateEdit()
        {
            return new PlayerLoopEdit()
                .Enter<PreUpdate>()
                .InsertBefore<PreUpdate.PhysicsUpdate, NetworkPreUpdate>(() =>
                {
                    RunStage(NetworkUpdateStage.PreUpdate);
                });
        }
    }

    public struct NetworkUpdate
    {
        public static ConstructedPlayerLoopEdit CreateEdit()
        {
            return new PlayerLoopEdit()
                .Enter<Update>()
                .InsertBefore<Update.ScriptRunBehaviourUpdate, NetworkUpdate>(() =>
                {
                    RunStage(NetworkUpdateStage.Update);
                });
        }
    }

    public struct NetworkPreLateUpdate
    {
        public static ConstructedPlayerLoopEdit CreateEdit()
        {
            return new PlayerLoopEdit()
                .Enter<PreLateUpdate>()
                .InsertBefore<PreLateUpdate.ScriptRunBehaviourLateUpdate, NetworkPreLateUpdate>(() =>
                {
                    RunStage(NetworkUpdateStage.PreLateUpdate);
                });
        }
    }

    public struct NetworkPostLateUpdate
    {
        public static ConstructedPlayerLoopEdit CreateEdit()
        {
            return new PlayerLoopEdit()
                .Enter<PostLateUpdate>()
                .InsertAfter<PostLateUpdate.PlayerSendFrameComplete, NetworkPostLateUpdate>(() =>
                {
                    RunStage(NetworkUpdateStage.PostLateUpdate);
                });
        }
    }

    private static void RunStage(NetworkUpdateStage stage)
    {
        UpdateStage = stage;

        var array = Stages[(byte)stage];
        var arrayLength = array.Count;
        for (var i = 0; i < arrayLength; i++)
            array[i].NetworkUpdate(stage);

        UpdateStage = NetworkUpdateStage.Unknown;
    }

}
