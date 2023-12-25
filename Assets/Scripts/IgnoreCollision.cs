using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreCollision : MonoBehaviour
{
    public Collider[] collidersToIgnore;
    // Start is called before the first frame update
    void Start()
    {
        foreach(Collider collider in collidersToIgnore)
        {
            Physics.IgnoreCollision(collider, GetComponent<Collider>());
        }
    }
}
