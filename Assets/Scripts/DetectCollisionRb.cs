using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectCollisionRb : MonoBehaviour
{
    public bool isColliding;
    private void Start()
    {
        StartCoroutine(UnCollide());
    }
    public void OnCollisionStay(Collision collision)
    {
        //Check if it's not colliding with hand layers
        if (collision.gameObject.layer != 6 && collision.gameObject.layer != 7)
        {
            isColliding = true;
        }
    }
    IEnumerator UnCollide()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.25f);
            isColliding = false;
        }
    }
}
