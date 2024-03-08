using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnumDeclaration;

public class DecalSurface : MonoBehaviour
{
    public surfaceType material;
    public GameManager gameManager;
    public void ImpactEffect(Vector3 position, Quaternion rotation)
    {
        GameObject effect = Instantiate(gameManager.FindDecal(material));

        effect.transform.position = position;
        effect.transform.rotation = rotation;

        Destroy(effect, 5);
    }
}
