using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Annotate : MonoBehaviour
{
    [SerializeField]
    private GameManager gameManager;

    [SerializeField]
    private LayerMask annotationsLayer;
    [SerializeField]
    private LayerMask objectsLayer;

    [SerializeField]
    private float pickupRange = 20f;
    private GameObject currentLookedAtObject = null;
    private GameObject originalParent = null;
    private Camera playerCamera = null;

    [Header("Debug")]
    public bool logTargetName = true;

    private void Awake()
    {
        playerCamera = Camera.main;

        if (playerCamera == null)
        {
            Debug.LogError("No Main Camera found! Assign a camera reference.");
        }
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        CastRay();
    }

    private void CastRay()
    {
        Debug.Log("CastRay called!");
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

        RaycastHit[] allHits = Physics.RaycastAll(ray, pickupRange);
        Debug.Log($"Total hits: {allHits.Length}");
        foreach (var h in allHits)
        {
            Debug.Log($"Hit: {h.collider.gameObject.name} layer: {h.collider.gameObject.layer}");
            Debug.Log($"Hit: {h.collider.gameObject.name} parent: {h.collider.gameObject.transform.parent?.name} layer: {h.collider.gameObject.layer} distance: {h.distance}");
        }

        Debug.Log($"Camera: {playerCamera.name}, position: {playerCamera.transform.position}");
        Debug.Log($"Ray origin: {ray.origin}, direction: {ray.direction}");



        if (Physics.Raycast(ray, out RaycastHit anyHit, pickupRange))
        {
            Debug.Log($"Ray hit: {anyHit.collider.gameObject.name} on layer: {anyHit.collider.gameObject.layer}");
        }
        else
        {
            Debug.Log("Ray hit nothing!");
        }

        if (currentLookedAtObject == null)
        {
            if (Physics.Raycast(ray, out RaycastHit hitInfo, pickupRange))
            {
                if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Annotations"))
                {
                    currentLookedAtObject = hitInfo.collider.gameObject;
                    originalParent = currentLookedAtObject.transform.parent.gameObject;
                    currentLookedAtObject.transform.SetParent(playerCamera.transform);
                    currentLookedAtObject.transform.localPosition = new Vector3(0.3f, -0.15f, 0.4f);
                    currentLookedAtObject.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                    currentLookedAtObject.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                    if (logTargetName)
                    {
                        Debug.Log($"Looking at: {currentLookedAtObject.name}");
                        Debug.Log($"With parent: {originalParent.name}");
                    }
                }
            }
        }
        else
        {
            if (Physics.Raycast(ray, out RaycastHit hitInfo, pickupRange, objectsLayer))
            {
                gameManager.AddAttempt();
                if (hitInfo.collider.gameObject != originalParent)
                {
                    Debug.Log("Wrong object!");
                    return;
                }
                Vector3 surfaceNormal = hitInfo.normal;
                Vector3 forward = Vector3.ProjectOnPlane(playerCamera.transform.position - hitInfo.point, surfaceNormal).normalized;
                currentLookedAtObject.transform.rotation = Quaternion.LookRotation(forward, surfaceNormal);
                currentLookedAtObject.transform.Rotate(-90f, 0f, 0f, Space.Self);
                if (hitInfo.point.y < playerCamera.transform.position.y)
                {
                    currentLookedAtObject.transform.Rotate(0f, 180f, 0f, Space.Self);
                }
                currentLookedAtObject.transform.position = hitInfo.point + surfaceNormal * 0.01f;
                currentLookedAtObject.transform.SetParent(hitInfo.collider.gameObject.transform);
                currentLookedAtObject.GetComponent<BoxCollider>().enabled = false;
                currentLookedAtObject = null;
                originalParent = null;
                gameManager.AddPoint();
            }
        }
    }

    private void PickupAnnotation()
    {

    }
}
