using UnityEngine;
using System.Collections;

public class PlayerController : PaddleController
{
    override protected void Start()
    {
        base.Start();
        OnMoveUp += MoveUp;
        OnMoveDown += MoveDown;
    }

    private void Update ()
    {
	    if (Input.GetAxis("Vertical") > 0f)
        {
            OnMoveUp();
        }
        else if (Input.GetAxis("Vertical") < 0f)
        {
            OnMoveDown();
        }
        else if (Input.GetButtonDown("Jump"))
        {
            OnLaunchBall();
        }
    }
}
