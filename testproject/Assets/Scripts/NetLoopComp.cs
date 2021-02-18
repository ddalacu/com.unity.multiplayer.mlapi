using System;
using MLAPI;
using UnityEngine;

public class NetLoopComp : MonoBehaviour, INetworkUpdateSystem
{
    private NetworkUpdateLoop.UpdateHandles _updateHandles;

    private void Awake()
    {
        _updateHandles = this.CreateUpdateHandles();
    }

    public void UnregisterUpdates()
    {
        _updateHandles.UnregisterAll();
    }

    public void RegisterUpdates(int id)
    {
        switch (id % 5)
        {
            case 0:
                _updateHandles.RegisterAll();
                break;
            case 1:
                _updateHandles.Register(NetworkUpdateStage.FixedUpdate);
                _updateHandles.Register(NetworkUpdateStage.Update);
                _updateHandles.Register(NetworkUpdateStage.PreLateUpdate);
                break;
            case 2:
                _updateHandles.Register(NetworkUpdateStage.EarlyUpdate);
                _updateHandles.Register(NetworkUpdateStage.PreUpdate);
                _updateHandles.Register(NetworkUpdateStage.PostLateUpdate);
                break;
            case 3:
                _updateHandles.Register(NetworkUpdateStage.Initialization);
                _updateHandles.Register(NetworkUpdateStage.FixedUpdate);
                break;
            case 4:
                _updateHandles.Register();
                break;
        }
    }

    private void OnDestroy()
    {
        UnregisterUpdates();
    }

    private readonly int[] m_NetUpdates = new int[7];

    public void NetworkUpdate(NetworkUpdateStage updateStage)
    {
        switch (updateStage)
        {
            case NetworkUpdateStage.Initialization:
                m_NetUpdates[0] += Time.frameCount;
                break;
            case NetworkUpdateStage.EarlyUpdate:
                m_NetUpdates[1] += Time.frameCount;
                break;
            case NetworkUpdateStage.FixedUpdate:
                m_NetUpdates[2] += Time.frameCount;
                break;
            case NetworkUpdateStage.PreUpdate:
                m_NetUpdates[3] += Time.frameCount;
                break;
            case NetworkUpdateStage.Update:
                m_NetUpdates[4] += Time.frameCount;
                break;
            case NetworkUpdateStage.PreLateUpdate:
                m_NetUpdates[5] += Time.frameCount;
                break;
            case NetworkUpdateStage.PostLateUpdate:
                m_NetUpdates[6] += Time.frameCount;
                break;
        }
    }
}
