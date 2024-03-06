using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabWithSecondaryGrip : GrabTwoAttach
{
    public LeftAttach secondaryGripLeft;
    [HideInInspector]
    public LeftAttach primaryGripLeft;
    public RightAttach secondaryGripRight;
    [HideInInspector]
    public RightAttach primaryGripRight;
    // Start is called before the first frame update
    void Start()
    {
        primaryGripLeft = leftAttach;
        primaryGripRight = rightAttach;
    }

    // Update is called once per frame
    void Update()
    {
        if(isGrabbing)
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
