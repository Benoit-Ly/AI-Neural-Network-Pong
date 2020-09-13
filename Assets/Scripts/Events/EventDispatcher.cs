using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EventDispatcher", menuName = "Event Dispatcher", order = 1)]
public class EventDispatcher : ScriptableObject
{
    #region Events
    public event Action<DebugLine[], DebugLine[]> OnForesightComplete;
    #endregion

    #region Executions
    public void ExecuteOnForesightComplete(DebugLine[] foresightLines, DebugLine[] realLines)
    {
        if (OnForesightComplete != null)
            OnForesightComplete(foresightLines, realLines);
    }
    #endregion
}
