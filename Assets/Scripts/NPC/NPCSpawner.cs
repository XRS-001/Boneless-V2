using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    public GameObject[] NPCS;
    public void InstantiateNPC()
    {
        Instantiate(NPCS[Random.Range(0, NPCS.Length)], transform.position, transform.rotation);
    }
}
