using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRendererObject : MonoBehaviour
{
    #region Components
    LineRenderer m_LineRenderer = null;
    #endregion

    private void Awake()
    {
        m_LineRenderer = GetComponent<LineRenderer>();
    }

    public void DrawLines(DebugLine[] lines)
    {
        if (!m_LineRenderer)
            return;

        int numLines = lines.Length;
        m_LineRenderer.positionCount = numLines + 1;

        if (numLines > 0)
        {
            m_LineRenderer.SetPosition(0, lines[0].start);

            for (int i = 0; i < numLines; ++i)
            {
                m_LineRenderer.SetPosition(i + 1, lines[i].end);
            }
        }
    }

    public void ClearLines()
    {
        if (m_LineRenderer)
            m_LineRenderer.positionCount = 0;
    }
}
