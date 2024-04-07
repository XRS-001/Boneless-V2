using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RayInteract : MonoBehaviour
{
    public InputActionProperty UIClickInput;
    public Transform reticle;
    public Transform point1;
    public Transform point2;
    public Transform point3;
    public LineRenderer lineRenderer;
    public int vertexCount = 6;

    private Graphic image;
    private GameObject element;
    [HideInInspector]
    public bool hasChangedOpacity;
    private bool hasTouched;
    private void Start()
    {
        point1.transform.parent = null;
        lineRenderer.enabled = true;
    }
    // Update is called once per frame
    void Update()
    {
        var pointList = new List<Vector3>();
        for (float ratio = 0; ratio <= 1; ratio += 1.0f / vertexCount)
        {
            var tangentLineVertex1 = Vector3.Lerp(point1.position, point2.position, ratio);
            var tangentLineVertex2 = Vector3.Lerp(point2.position, point3.position, ratio);
            var bezierpoint = Vector3.Lerp(tangentLineVertex1, tangentLineVertex2, ratio);
            pointList.Add(bezierpoint);
        }
        if (lineRenderer.enabled)
        {
            lineRenderer.positionCount = pointList.Count;
            lineRenderer.SetPositions(pointList.ToArray());
        }

        Physics.Raycast(point3.position, point3.forward, out RaycastHit hitInfo, float.PositiveInfinity);
        if (hitInfo.collider)
        {
            if (hitInfo.collider.gameObject != element)
            {
                ChangeOpacity();
            }
            if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("UI"))
            {
                reticle.gameObject.SetActive(true);
                reticle.rotation = Quaternion.LookRotation(hitInfo.normal, Vector3.up);
                lineRenderer.enabled = true;
                point1.position = Vector3.Lerp(point1.position, hitInfo.point, 0.1f);
                Vector3 centerpos = (hitInfo.point + point3.position) / 2;
                point2.position = centerpos;
                HandleUIInteraction(hitInfo.collider.gameObject);
            }
            else
            {
                reticle.gameObject.SetActive(false);
                lineRenderer.enabled = false;
                ChangeOpacity();
            }
        }
        else
        {
            reticle.gameObject.SetActive(false);
            lineRenderer.enabled = false;
            ChangeOpacity();
        }
        if (UIClickInput.action.ReadValue<float>() < 0.25f)
        {
            hasTouched = false;
        }
    }
    void HandleUIInteraction(GameObject uiElement)
    {
        element = uiElement;
        PointerEventData eventData = new PointerEventData(EventSystem.current);

        eventData.position = new Vector2(Screen.width / 2, Screen.height / 2);
        EventSystem.current.RaycastAll(eventData, new List<RaycastResult>());
        ExecuteEvents.ExecuteHierarchy(uiElement, eventData, ExecuteEvents.pointerEnterHandler);
        if (uiElement.GetComponent<Button>() || uiElement.GetComponentInParent<Button>())
        {
            if (uiElement.GetComponent<Button>())
            {
                image = uiElement.GetComponent<Button>().targetGraphic;
            }
            else if (uiElement.GetComponentInParent<Button>())
            {
                image = uiElement.GetComponentInParent<Button>().targetGraphic;
            }
        }
        else
        {
            image = null;
        }
        if(image != null && !hasChangedOpacity)
        {
            if (image.GetComponent<Button>() || image.GetComponentInParent<Button>())
            {
                if (image.GetComponent<Image>())
                {
                    if (image.GetComponent<Image>().sprite)
                    {
                        Color color = image.color;
                        color.a /= 2;
                        image.color = color;
                        hasChangedOpacity = true;
                    }
                    else
                    {
                        Color color = image.color;
                        color.a /= 1.1f;
                        image.color = color;
                        hasChangedOpacity = true;
                    }
                }
                else
                {
                    Color color = image.color;
                    color.a /= 2;
                    image.color = color;
                    hasChangedOpacity = true;
                }
            }
        }
        if (UIClickInput.action.ReadValue<float>() > 0.25f && !hasTouched)
        {
            ChangeOpacity();
            ExecuteEvents.ExecuteHierarchy(uiElement, eventData, ExecuteEvents.pointerClickHandler);
            hasTouched = true;
        }
    }
    void ChangeOpacity()
    {
        if (image != null && hasChangedOpacity)
        {
            if (image.GetComponent<Button>() || image.GetComponentInParent<Button>())
            {
                if (image.GetComponent<Image>())
                {
                    if (image.GetComponent<Image>().sprite)
                    {
                        Color color = image.color;
                        color.a *= 2;
                        image.color = color;
                        hasChangedOpacity = false;
                    }
                    else
                    {
                        Color color = image.color;
                        color.a *= 1.1f;
                        image.color = color;
                        hasChangedOpacity = false;
                    }
                }
                else
                {
                    Color color = image.color;
                    color.a *= 2;
                    image.color = color;
                    hasChangedOpacity = false;
                }
            }
        }
    }
}
