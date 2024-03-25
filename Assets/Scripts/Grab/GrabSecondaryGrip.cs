using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabSecondaryGrip : GrabTwoAttach
{
    public LeftAttach secondaryGripLeft;
    [HideInInspector]
    public LeftAttach primaryGripLeft;
    public RightAttach secondaryGripRight;
    [HideInInspector]
    public RightAttach primaryGripRight;
    public bool isPrimaryGrabbing;
    public bool disconnectSecondaryOnUnGrab;
    // Start is called before the first frame update
    void Start()
    {
        primaryGripLeft = leftAttach;
        primaryGripRight = rightAttach;
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        if (disconnectSecondaryOnUnGrab)
        {
            if (isGrabbing)
            {
                leftAttach = secondaryGripLeft;
                rightAttach = secondaryGripRight;
            }
            else
            {
                leftAttach = primaryGripLeft;
                rightAttach = primaryGripRight;
            }
        }
        else
        {
            if (isPrimaryGrabbing)
            {
                leftAttach = secondaryGripLeft;
                rightAttach = secondaryGripRight;
            }
            else
            {
                leftAttach = primaryGripLeft;
                rightAttach = primaryGripRight;
            }
        }
    }
}
