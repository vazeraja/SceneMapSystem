using System;
using System.Collections;
using TNS.SceneSystem;
using UnityEngine;


public class SceneManagerCallbacks : ComponentSingleton<SceneManagerCallbacks>
{
    private Action<float> m_OnUpdateDelegate;
    private Action<float> m_OnLateUpdateDelegate;

    public event Action<float> onUpdate
    {
        add => m_OnUpdateDelegate += value;
        remove => m_OnUpdateDelegate -= value;
    }

    internal event Action<float> onLateUpdate
    {
        add => m_OnLateUpdateDelegate += value;
        remove => m_OnLateUpdateDelegate -= value;
    }

    public int UpdateInvocationCount => m_OnUpdateDelegate.GetInvocationList().Length;
    protected override string GetGameObjectName() => "SceneManagerCallbacks";

    public CoroutineHandle RunCoroutine( IEnumerator enumerator )
    {
        return CoroutineHelpers.RunCoroutine( this, enumerator );
    }

    internal void Update()
    {
        m_OnUpdateDelegate?.Invoke( Time.unscaledDeltaTime );
    }

    internal void LateUpdate()
    {
        m_OnLateUpdateDelegate?.Invoke( Time.unscaledDeltaTime );
    }
}