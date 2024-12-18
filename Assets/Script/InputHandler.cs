using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputHandler : SingletonBehaviour<InputHandler>
{
    [SerializeField] private float dragSensitivity = 50f;

    private Vector2 startPosition;
    private Vector2 endPosition;

    public event Action<Vector2, Vector2> OnDragEnd;
    public event Action<Vector2> OnTouchEnd;

    private bool isTouchStartFromUI = false;

    private void Update()
    {
        HandleInput();
    }

    public void ResetHandler()
    {
        OnDragEnd = null;
        OnTouchEnd = null;
    }

    private void HandleInput()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            startPosition = Input.mousePosition;
        }

        if(!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonUp(0))
        {
            endPosition = Input.mousePosition;

            if((startPosition - endPosition).sqrMagnitude >= dragSensitivity)
            {
                OnDragEnd?.Invoke(startPosition, endPosition);
            }
            else
            {
                OnTouchEnd?.Invoke(endPosition);
            }
        }
#endif
#if UNITY_ANDROID
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if (IsTouchOverUI(touch)) isTouchStartFromUI = true;
                startPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                endPosition = touch.position;
                if(!isTouchStartFromUI)
                {
                    if ((startPosition - endPosition).sqrMagnitude >= dragSensitivity)
                    {
                        OnDragEnd?.Invoke(startPosition, endPosition);
                    }
                    else
                    {
                        OnTouchEnd?.Invoke(endPosition);
                    }
                }
                isTouchStartFromUI = false;
            }
        }
#endif
    }
        
    private bool IsTouchOverUI(Touch touch)
    {
        if (EventSystem.current == null)
            return false;

        return EventSystem.current.IsPointerOverGameObject(touch.fingerId);
    }

}
