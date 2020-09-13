using System.Collections.Generic;
using UnityEngine;

public class AIController : PaddleController
{
    [SerializeField]
    private EventDispatcher m_EventDispatcher = null;

    [SerializeField]
    private bool drawForesightLines = true;
    [SerializeField]
    private bool isLearning = false;
    [SerializeField]
    private int updateFrenquency = 1;

    private int frameCount = 0;
    private float wantedPosY = 0f;

    private List<float> inputs;
    private AINeuralNetwork ann;

    private Vector2 ballStartPoint;
    private Vector2 ballStartDirection;

    int ForesightDepth = 10;
    int currentForesight = 0;

    List<DebugLine> debugLines;
    List<DebugLine> realLines;

    // Use this for initialization
    protected override void Start ()
    {
        base.Start();

        inputs = new List<float>();
        // BallStartPoint
        inputs.Add(0f);
        inputs.Add(0f);
        // BallStartDirection
        inputs.Add(0f);
        inputs.Add(0f);
        ann = GetComponent<AINeuralNetwork>();

        debugLines = new List<DebugLine>();
        realLines = new List<DebugLine>();

        GameMgr.Instance.Ball.OnBallThrown += OnBallThrown;
        GameMgr.Instance.Ball.OnBallCollidePaddle += OnBallCollidePaddle;
    }

    private void FixedUpdate ()
    {
        if (++frameCount % updateFrenquency != 0)
            return;

        ProcessOutput();
    }

    private void ProcessOutput()
    {
        if (!ann)
            return;

        //inputs[0] = GameMgr.Instance.Ball.transform.position.x;
        //inputs[1] = GameMgr.Instance.Ball.transform.position.y;
        //inputs[2] = GameMgr.Instance.Ball.LaunchDirection.x;
        //inputs[3] = GameMgr.Instance.Ball.LaunchDirection.y;

        //ann.Compute(inputs);

        //List<float> outputs = ann.GetOutputs();
        //wantedPosY = Mathf.Lerp(-5f, 5f, outputs[0]);

        
    }

    private void Update()
    {
        MoveToPos(wantedPosY);
    }

    public void ActivateLearning(bool value)
    {
        isLearning = value;
    }

    private void Foresight(Vector2 startPos, Vector2 direction)
    {
        List<float> localInputs = new List<float>();

        Vector2 normStartPos = startPos.normalized;

        localInputs.Add(normStartPos.x);
        localInputs.Add(normStartPos.y);
        localInputs.Add(direction.x);
        localInputs.Add(direction.y);

        ann.Compute(localInputs);

        DebugLine line = new DebugLine();

        List<float> outputs = ann.GetOutputs();

        int finishedForesight = Mathf.RoundToInt(Mathf.Clamp01(outputs[2]));

        if (finishedForesight == 1 || currentForesight == ForesightDepth)
        {
            wantedPosY = Mathf.Lerp(-GameMgr.Instance.CourtHeight / 2f, GameMgr.Instance.CourtHeight / 2f, outputs[1]);

            line.start = startPos;
            line.end = new Vector2(transform.position.x, wantedPosY);

            debugLines.Add(line);
        }
        else
        {
            ++currentForesight;

            float x = Mathf.Lerp(-10f, 10f, outputs[0]);
            float y = Mathf.Lerp(-GameMgr.Instance.CourtHeight / 2f, GameMgr.Instance.CourtHeight / 2f, outputs[1]);

            Vector2 bouncePos = new Vector2(x, y);
            Vector2 computedDir = (bouncePos - startPos).normalized;
            RaycastHit2D hit = Physics2D.Raycast(startPos, computedDir, Mathf.Infinity, 1 << 9);

            if (hit.collider != null)
            {
                Vector2 bounceDir = Vector2.Reflect(computedDir, hit.normal);

                line.start = startPos;
                line.end = bouncePos;

                debugLines.Add(line);

                Foresight(bouncePos, bounceDir);
            }
        }
    }

    void LearnForesight(Vector2 startPos, Vector2 direction)
    {
        bool finished = true;

        RaycastHit2D hit = Physics2D.Raycast(startPos, direction, Mathf.Infinity, 1 << 9);
        List<float> wantedOutputs = new List<float>();

        if (hit.collider != null)
        {
            wantedOutputs.Add(GetLerpRateFromFloat(hit.point.x, 20f));
            wantedOutputs.Add(GetLerpRateFromFloat(hit.point.y, GameMgr.Instance.CourtHeight));

            if (hit.transform.gameObject.tag == "AIPositionLine")
            {
                wantedOutputs.Add(1f);
            }
            else
            {
                wantedOutputs.Add(0f);
                finished = false;
            }

            DebugLine line;
            line.start = startPos;
            line.end = hit.point;

            realLines.Add(line);

            Vector2 normStartPos = startPos.normalized;

            List<float> realInputs = new List<float>();
            realInputs.Add(normStartPos.x);
            realInputs.Add(normStartPos.y);
            realInputs.Add(direction.x);
            realInputs.Add(direction.y);

            ann.ComputeLearn(realInputs, wantedOutputs);

            if (!finished)
            {
                Vector2 bouncedDir = Vector2.Reflect(direction, hit.normal);
                DebugLine debugLine;
                debugLine.start = hit.point;
                debugLine.end = hit.point + bouncedDir * 20f;

                realLines.Add(debugLine);
                LearnForesight(hit.point, bouncedDir);
            }
        }
    }

    float GetLerpRateFromFloat(float val, float total)
    {
        float min = total / 2f;
        return (val + min) / total;
    }

    public void OnPointLost(Vector3 ballPos)
    {
        if (isLearning)
            ComputeLearning(ballPos);
    }

    public void OnBallCollidePaddle()
    {
        Vector2 direction = GameMgr.Instance.Ball.LaunchDirection;

        if (direction.x > 0f)
            OnBallThrown();
        else if (isLearning && direction.x < 0f)
            LearnForesight(ballStartPoint, ballStartPoint);
    }

    public void OnBallThrown()
    {
        debugLines.Clear();
        realLines.Clear();

        MemorizeBallStartPoint();

        currentForesight = 0;
        Foresight(ballStartPoint, ballStartDirection);

        if (isLearning)
            LearnForesight(ballStartPoint, ballStartDirection);

        if (m_EventDispatcher)
            m_EventDispatcher.ExecuteOnForesightComplete(debugLines.ToArray(), realLines.ToArray());
    }

    private void ComputeLearning(Vector3 ballPos)
    {

    }

    private void MemorizeBallStartPoint()
    {
        ballStartPoint = GameMgr.Instance.Ball.transform.position;
        ballStartDirection = GameMgr.Instance.Ball.LaunchDirection;
    }

    private void OnRenderObject()
    {
        
    }

    private void OnDrawGizmos()
    {
        if (debugLines == null)
            return;

        for (int i = 0; i < debugLines.Count; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(debugLines[i].start, debugLines[i].end);
        }

        for (int i = 0; i < realLines.Count; i++)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(realLines[i].start, realLines[i].end);
        }
    }
}
