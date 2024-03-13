using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnumDeclaration;

public class DecalSurface : MonoBehaviour
{
    public surfaceType material;
    public GameManager gameManager;
    public void ImpactEffect(Vector3 position, Quaternion rotation, Transform hitObject)
    {
        GameObject effect = Instantiate(gameManager.FindEffect(material));

        effect.transform.position = position;
        effect.transform.rotation = rotation;

        Destroy(effect, 5);
        if (gameManager.FindDecal(material))
        {
            GameObject decal = Instantiate(gameManager.FindDecal(material));
            decal.transform.parent = hitObject;

            decal.transform.position = position;
            decal.transform.rotation = rotation;

            if (isActiveAndEnabled)
            {
                StartCoroutine(DelayDecalAlpha(decal.GetComponentInChildren<MeshRenderer>()));
            }
        }
    }
    IEnumerator DelayDecalAlpha(MeshRenderer decal)
    {
        yield return new WaitForSeconds(25);
        float timer = 0;
        Color newColor = decal.material.color;
        while (timer < 5)
        {
            newColor.a = Mathf.Lerp(1, 0, timer / 5);
            decal.material.color = newColor;
            timer += Time.deltaTime;
            yield return null;
        }
    }
}
