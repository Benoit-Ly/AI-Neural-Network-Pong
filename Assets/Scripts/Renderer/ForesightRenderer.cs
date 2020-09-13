using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForesightRenderer : MonoBehaviour
{
    #region Dependencies
    [SerializeField]
    private EventDispatcher m_EventDispatcher = null;
    [SerializeField]
    private LineRendererObject m_AILineRenderer = null;
    [SerializeField]
    private LineRendererObject m_RealLineRenderer = null;
    #endregion

    #region Params
    private bool m_DisplayGizmos = true;
    #endregion

    private void Awake()
    {
        if (m_EventDispatcher)
            m_EventDispatcher.OnForesightComplete += OnForesightComplete;
    }

    private void OnForesightComplete(DebugLine[] foresightLines, DebugLine[] realLines)
    {
        if (m_DisplayGizmos)
        {
            if (m_AILineRenderer)
                m_AILineRenderer.DrawLines(foresightLines);

            if (m_RealLineRenderer)
                m_RealLineRenderer.DrawLines(realLines);
        }
    }

    public void ActivateGizmos(bool active)
    {
        m_DisplayGizmos = active;

        if (!m_DisplayGizmos)
            ClearLines();
    }

    private void ClearLines()
    {
        if (m_AILineRenderer)
            m_AILineRenderer.ClearLines();

        if (m_RealLineRenderer)
            m_RealLineRenderer.ClearLines();
    }

    private void OnDestroy()
    {
        if (m_EventDispatcher)
            m_EventDispatcher.OnForesightComplete -= OnForesightComplete;
    }
}
