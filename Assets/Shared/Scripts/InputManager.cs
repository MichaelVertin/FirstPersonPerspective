using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;


// TODO: identify active camera
public class InputManager : MonoBehaviour
{
    public static PlayerInput playerInput;

    private InputAction _mousePositionAction;
    private InputAction _mouseAction;

    public static Vector2 MousePosition;
    public static bool WasLeftMouseButtonPressed;
    public static bool WasLeftMouseButtonReleased;
    public static bool IsLeftMouseButtonPressed;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        _mousePositionAction = playerInput.actions["MousePosition"];
        _mouseAction = playerInput.actions["Mouse"];
    }

    private void Update()
    {
        MousePosition = _mousePositionAction.ReadValue<Vector2>();

        WasLeftMouseButtonPressed = _mouseAction.WasPressedThisFrame();
        WasLeftMouseButtonReleased = _mouseAction.WasReleasedThisFrame();
        IsLeftMouseButtonPressed = _mouseAction.IsPressed();
    }

    #region raycasts

    public static bool GetObjectUnderMouse<SearchType>(out Vector3 result)
    {
        if (GetRaycastHitUnderMouse<SearchType>(out RaycastHit hit))
        {
            result = hit.point;
            return true;
        }

        result = Vector3.zero;

        return false;
    }

    // returns all objects under the mouse of type SearchType
    public static List<SearchType> GetObjectsUnderMouse<SearchType>()
    {
        List<SearchType> results = new List<SearchType>();
        List<RaycastHit> hits = GetRaycastHitsUnderMouse<SearchType>();

        foreach (RaycastHit raycastHit in hits)
        {
            results.Add(GetComponentBySearch<SearchType>(raycastHit.collider));
        }

        return results;
    }


    public static bool GetObjectUnderMouse<SearchType>(out SearchType result)
    {
        if (GetRaycastHitUnderMouse<SearchType>(out RaycastHit rayHit))
        {
            result = GetComponentBySearch<SearchType>(rayHit.collider);
            return true;
        }

        result = default(SearchType);
        return false;
    }

    public static bool GetObjectUnderMouse<SearchType>(out SearchType outObject, out Vector3 outPoint)
    {
        if (GetRaycastHitUnderMouse<SearchType>(out RaycastHit rayHit))
        {
            outObject = GetComponentBySearch<SearchType>(rayHit.collider);
            outPoint = rayHit.point;
            return true;
        }

        outObject = default(SearchType);
        outPoint = Vector3.zero;
        return false;
    }

    public static List<RaycastHit> GetRaycastHitsUnderMouse<SearchType>()
    {
        List<RaycastHit> results = new List<RaycastHit>();

        Ray rayOrigin = Camera.main.ScreenPointToRay(MousePosition);
        RaycastHit[] rayHits = Physics.RaycastAll(rayOrigin);

        foreach (RaycastHit hit in rayHits)
        {
            SearchType searchAttempt = GetComponentBySearch<SearchType>(hit.collider);

            if (searchAttempt != null)
            {
                results.Add(hit);
            }
        }

        return results;
    }

    public static bool GetRaycastHitUnderMouse<SearchType>(out RaycastHit resultHit)
    {
        List<RaycastHit> raycastHits = GetRaycastHitsUnderMouse<SearchType>();

        if (raycastHits.Count > 0)
        {
            resultHit = raycastHits[0];
            return true;
        }

        resultHit = default(RaycastHit);

        return false;
    }
    #endregion raycasts


    public static SearchType GetComponentBySearch<SearchType>(Collider objectToSearch)
    {
        SearchType result = default(SearchType);

        result = objectToSearch.GetComponent<SearchType>();

        if (result == null)
        {
            result = objectToSearch.GetComponentInParent<SearchType>();
        }

        return result;
    }

    public static bool WasKeyPressedThisFrame(string value)
    {
        return Keyboard.current.FindKeyOnCurrentKeyboardLayout(value).wasPressedThisFrame;
    }

    public static bool IsKeyPressed(string value)
    {
        return Keyboard.current.FindKeyOnCurrentKeyboardLayout(value).isPressed;
    }

    public static bool WasKeyPressedThisFrame(Key key)
    {
        return Keyboard.current[key].wasPressedThisFrame;
    }

    public static bool IsKeyPressed(Key key)
    {
        // Key.Space
        return Keyboard.current[key].isPressed;
    }
}
