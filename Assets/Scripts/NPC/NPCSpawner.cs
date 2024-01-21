using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    public GameObject NPC;
    public void InstantiateNPC()
    {
        Instantiate(NPC, transform.position, transform.rotation);
    }
}
