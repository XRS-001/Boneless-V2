using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 startPosition;
    public Vector3 moveToOffset;
    public float moveTime;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
        StartCoroutine(MovePlatformStart());
    }

    IEnumerator MovePlatformStart()
    {
        float timer = 0;
        while (timer < moveTime)
        {
            rb.MovePosition(Vector3.Lerp(startPosition, startPosition + moveToOffset, timer / moveTime));
            timer += Time.deltaTime;
            yield return null;
        }
        StartCoroutine(MovePlatform());
    }
    IEnumerator MovePlatform()
    {
        float timer = 0;
        while (timer < moveTime)
        {
            rb.MovePosition(Vector3.Lerp(startPosition + moveToOffset, startPosition, timer / moveTime));
            timer += Time.deltaTime;
            yield return null;
        }
        StartCoroutine(MovePlatformStart());
    }
}
