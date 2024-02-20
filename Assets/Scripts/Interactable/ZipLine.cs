using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZipLine : MonoBehaviour
{
    public HexaBody player;
    public bool isZiplining;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.GetComponent<GrabTwoAttach>())
        {
            if (collision.transform.GetComponent<GrabTwoAttach>().isGrabbing == true)
            {
                isZiplining = true;
            }
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.transform.GetComponent<GrabTwoAttach>())
        {
            if (collision.transform.GetComponent<GrabTwoAttach>().isGrabbing == true)
            {
                isZiplining = false;
            }
        }
    }
    private void FixedUpdate()
    {
        if(isZiplining)
        {
            player.Zipline();
        }
        else
        {
            player.zipLining = false;
        }
    }
}
