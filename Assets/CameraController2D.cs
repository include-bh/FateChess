using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController2D : MonoBehaviour
{
    [Header("缩放设置")]
    public bool enableZoom = true;
    public float zoomSpeed = 5f;
    public float minZoom = 1f;
    public float maxZoom = 15f;

    [Header("拖动设置")]
    public bool enableDrag = true;
    public float dragSpeed = 1f;

    [Header("边界限制（可选）")]
    public bool useBounds = false;
    public Vector2 minPosition = new Vector2(-10f, -10f);
    public Vector2 maxPosition = new Vector2(10f, 10f);

    private Camera cam;
    private Vector2 lastInputPosition;
    private bool isDragging = false;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null || !cam.orthographic)
        {
            Debug.LogError("CameraController2D 需要挂载在正交（Orthographic）摄像机上！");
        }
    }

    void Update()
    {
        HandleInput();
        ApplyBounds();
    }

    void HandleInput()
    {
        // PC端处理
        if (Application.platform == RuntimePlatform.WindowsPlayer || 
            Application.platform == RuntimePlatform.OSXPlayer || 
            Application.platform == RuntimePlatform.LinuxPlayer ||
            Application.platform == RuntimePlatform.WindowsEditor ||
            Application.platform == RuntimePlatform.OSXEditor ||
            Application.platform == RuntimePlatform.LinuxEditor)
        {
            HandleMouseInput();
        }
        // 移动端处理
        else
        {
            HandleTouchInput();
        }
    }

    void HandleMouseInput()
    {
        // 滚轮缩放
        if (enableZoom)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0f)
            {
                if (!IsPointerOverUI())
                {
                    float newSize = cam.orthographicSize - scroll * zoomSpeed;
                    cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
                }
            }
        }
        
        // 鼠标拖动
        if (enableDrag)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!IsPointerOverUI())
                {
                    isDragging = true;
                    lastInputPosition = Input.mousePosition;
                }
            }

            if (Input.GetMouseButton(0) && isDragging)
            {
                Vector2 currentPos = Input.mousePosition;
                Vector2 delta = currentPos - lastInputPosition;
                // 转换为世界坐标偏移
                Vector3 worldDelta = cam.ScreenToWorldPoint(new Vector3(delta.x, delta.y, cam.nearClipPlane)) 
                                   - cam.ScreenToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
                transform.position -= worldDelta * dragSpeed;
                lastInputPosition = currentPos;
            }

            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }
        }
    }

    void HandleTouchInput()
    {
        if (!enableDrag) return;

        int touchCount = Input.touchCount;
        
        // 单指拖动
        if (touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            
            if (touch.phase == TouchPhase.Began)
            {
                if (!IsPointerOverUI(touch.position))
                {
                    isDragging = true;
                    lastInputPosition = touch.position; // ✅ Vector2
                }
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                Vector2 currentPos = touch.position;
                Vector2 delta = currentPos - lastInputPosition; // ✅ 正确：Vector2 - Vector2
                Vector3 worldDelta = cam.ScreenToWorldPoint(new Vector3(delta.x, delta.y, cam.nearClipPlane)) 
                                   - cam.ScreenToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
                transform.position -= worldDelta * dragSpeed;
                lastInputPosition = currentPos;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }
        // 双指缩放
        else if (touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
            {
                Vector2 currentTouch0 = touch0.position;
                Vector2 currentTouch1 = touch1.position;
                float currentDistance = Vector2.Distance(currentTouch0, currentTouch1);

                Vector2 prevTouch0 = touch0.position - touch0.deltaPosition;
                Vector2 prevTouch1 = touch1.position - touch1.deltaPosition;
                float prevDistance = Vector2.Distance(prevTouch0, prevTouch1);

                float deltaDistance = currentDistance - prevDistance;
                if (Mathf.Abs(deltaDistance) > 0.1f)
                {
                    // 注意：移动端缩放通常更敏感，所以系数要小一些
                    float newSize = cam.orthographicSize - (deltaDistance * zoomSpeed * 0.005f);
                    cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
                }
            }
        }
    }

    bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        if (EventSystem.current.IsPointerOverGameObject()) return true;

        Vector3 mouseScreenPos = Input.mousePosition;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        Vector2 rayOrigin = new Vector2(mouseWorldPos.x, mouseWorldPos.y);
        RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, Vector2.zero);

        foreach (RaycastHit2D hit in hits)
        {
            GameObject go = hit.collider.gameObject;
            CardRenderer rend = go.GetComponent<CardRenderer>();
            if (rend != null) return true;
        }
        return false;
    }

    bool IsPointerOverUI(Vector2 position)
    {
        // 注意：旧输入系统的 IsPointerOverGameObject() 只支持鼠标
        // 对于触摸，我们需要手动检测
        if (EventSystem.current == null) return false;

        // 创建一个指针事件用于检测
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = position
        };

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }

    void ApplyBounds()
    {
        if (!useBounds) return;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minPosition.x, maxPosition.x);
        pos.y = Mathf.Clamp(pos.y, minPosition.y, maxPosition.y);
        transform.position = pos;
    }
}
