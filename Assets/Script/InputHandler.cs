using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputHandler : SingletonBehaviour<InputHandler>
{
    [SerializeField] private float dragSensitivity = 10f;

    private Vector2 startPosition;
    private Vector2 endPosition;

    public event Action<Vector2, Vector2> OnDragEnd;

    private void Update()
    {
        HandleInput();
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
        }
#endif
#if UNITY_ANDROID
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                startPosition = touch.position;
            }

            if (!EventSystem.current.IsPointerOverGameObject() && touch.phase == TouchPhase.Ended)
            {
                endPosition = touch.position;
                if((startPosition - endPosition).sqrMagnitude >= dragSensitivity)
                {
                    OnDragEnd?.Invoke(startPosition, endPosition);
                }
            }
        }
#endif
    }
}
