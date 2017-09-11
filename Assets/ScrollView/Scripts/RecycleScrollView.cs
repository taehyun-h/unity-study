using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RecycleScrollView : ScrollRect
{
    public delegate RectTransform GetItem(int index);
    public delegate void RefreshItem(RectTransform item, int index);
    public delegate bool IsValidIndex(int index);
    [System.NonSerialized] public GetItem _delegateGetItem = null;
    [System.NonSerialized] public RefreshItem _delegateRefreshItem = null;
    [System.NonSerialized] public IsValidIndex _delegateIsValidIndex = null;
    private Transform _freeItem = null;
    private int _itemStartIndex;
    private int _itemEndIndex;
    private bool _isDragging = false;
    private Vector2 _prevPosition;
    private int _contentConstraintCount;

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (Application.isPlaying && _freeItem != null)
        {
            Destroy(_freeItem.gameObject);
        }
    }

    protected override void LateUpdate()
    {
        UpdateBoundary();
        base.LateUpdate();
        if (!Application.isPlaying)
        {
            return;
        }
        if (_isDragging && inertia)
        {
            float deltaTime = Time.unscaledDeltaTime;
            Vector3 newVelocity = (content.anchoredPosition - _prevPosition) / deltaTime;
            velocity = Vector3.Lerp(velocity, newVelocity, deltaTime * 10);
        }
        _prevPosition = content.anchoredPosition;
    }

    public override void OnInitializePotentialDrag(PointerEventData eventData)
    {
        base.OnInitializePotentialDrag(eventData);
        UpdateBoundary();
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
        _isDragging = true;
        UpdateBoundary();
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        _isDragging = false;
        UpdateBoundary();
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);
        UpdateBoundary();
    }

    public override void OnScroll(PointerEventData eventData)
    {
        base.OnScroll(eventData);
        UpdateBoundary();
    }

    public void Refresh()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }
        if (_delegateGetItem == null)
        {
            Debug.LogError("GetItem method is null!");
        }
        if (_delegateRefreshItem == null)
        {
            Debug.LogError("RefreshItem method is null!");
        }

        // get constraint count
        var content = GetComponent<ScrollRect>().content;
        var gridLayoutGroup = content.GetComponent<GridLayoutGroup>();
        _contentConstraintCount = 1;
        if (gridLayoutGroup != null)
        {
            if (gridLayoutGroup.constraint == GridLayoutGroup.Constraint.Flexible)
            {
                Debug.LogError("Flexible GridLayoutGroup is not support");
                return;
            }
            _contentConstraintCount = gridLayoutGroup.constraintCount;
        }

        // free content item
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            content.GetChild(i).SetParent(_freeItem);
        }

        // refresh scroll view
        _itemStartIndex = 0;
        _itemEndIndex = 0;
        float size = 0f;
        int count = 0;
        while (true)
        {
            // get new item
            var item = GetFreeItem(_itemEndIndex++);
            if (item == null)
            {
                break;
            }

            // add item to scroll view
            item.gameObject.SetActive(true);
            item.SetParent(content);
            item.localPosition = Vector3.zero;
            item.localScale = Vector3.one;
            count++;

            // add size and check view bounds
            if (horizontal)
            {
                size += (item.GetComponent<LayoutElement>().preferredWidth + GetSpacing());
                if (size >= GetComponent<RectTransform>().sizeDelta.x && count % _contentConstraintCount == 0)
                {
                    break;
                }
            }
            else
            {
                size += (item.GetComponent<LayoutElement>().preferredHeight + GetSpacing());
                if (size >= GetComponent<RectTransform>().sizeDelta.y && count % _contentConstraintCount == 0)
                {
                    break;
                }
            }
        }

        // init position
        content.localPosition = Vector3.zero;
    }

    private void CreatePool()
    {
        if (_freeItem != null)
        {
            return;
        }

        // create pool
        var itemPool = GameObject.Find("ItemPool");
        var pool = new GameObject(gameObject.name + "Pool");
        pool.transform.SetParent(itemPool.transform);
        _freeItem = pool.transform;
    }

    private RectTransform GetFreeItem(int index)
    {
        if (_freeItem == null)
        {
            CreatePool();
        }
        if (!_delegateIsValidIndex(index))
        {
            return null;
        }

        if (_freeItem.childCount > 0)
        {
            var item = _freeItem.GetChild(0).GetComponent<RectTransform>();
            _delegateRefreshItem(item, index);
            return item;
        }
        else
        {
            return _delegateGetItem(index);
        }
    }

    private float GetSpacing()
    {
        if (horizontal)
        {
            if (GetComponentInChildren<HorizontalLayoutGroup>() == null)
            {
                return GetComponentInChildren<GridLayoutGroup>().spacing.x;
            }
            else
            {
                return GetComponentInChildren<HorizontalLayoutGroup>().spacing;
            }
        }
        else
        {
            if (GetComponentInChildren<VerticalLayoutGroup>() == null)
            {
                return GetComponentInChildren<GridLayoutGroup>().spacing.y;
            }
            else
            {
                return GetComponentInChildren<VerticalLayoutGroup>().spacing;
            }
        }
    }

    private Vector2 GetOffset(float offset)
    {
        if (horizontal)
        {
            return new Vector2(-offset, 0f);
        }
        else
        {
            return new Vector2(0f, offset);
        }
    }

    private float GetSize(Vector3 size)
    {
        if (horizontal)
        {
            return size.x;
        }
        else
        {
            return size.y;
        }
    }

    private void UpdateBoundary()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        float size = 0;
        if (content.childCount > 0)
        {
            size = (GetSize(content.GetChild(0).GetComponent<RectTransform>().rect.size) + GetSpacing()) * 1.1f;
        }

        bool changed = false;
        var contentBounds = GetBounds();
        var viewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
        if ((horizontal && GetSize(viewBounds.max) > GetSize(contentBounds.max)) || (!horizontal && GetSize(viewBounds.min) < GetSize(contentBounds.min)))
        {
            changed |= AddItemAtEnd();
        }
        else if ((horizontal && GetSize(viewBounds.max) < GetSize(contentBounds.max) - size) || (!horizontal && GetSize(viewBounds.min) > GetSize(contentBounds.min) + size))
        {
            changed |= RemoveItemAtEnd();
        }
        if ((horizontal && GetSize(viewBounds.min) < GetSize(contentBounds.min)) || (!horizontal && GetSize(viewBounds.max) > GetSize(contentBounds.max)))
        {
            changed |= AddItemAtStart();
        }
        else if ((horizontal && GetSize(viewBounds.min) > GetSize(contentBounds.min) + size) || (!horizontal && GetSize(viewBounds.max) < GetSize(contentBounds.max) - size))
        {
            changed |= RemoveItemAtStart();
        }

        if (changed)
        {
            Canvas.ForceUpdateCanvases();
        }
    }

    private bool AddItemAtStart()
    {
        if (!_delegateIsValidIndex(_itemStartIndex - _contentConstraintCount))
        {
            return false;
        }

        float size = 0f;
        for (int i = 0; i < _contentConstraintCount; i++)
        {
            var item = GetFreeItem(_itemStartIndex - 1);
            item.gameObject.SetActive(true);
            item.SetParent(content);
            item.SetAsFirstSibling();
            item.localPosition = Vector3.zero;
            item.localScale = Vector3.one;
            size = Mathf.Max(size, GetSize(item.GetComponent<RectTransform>().rect.size));
            _itemStartIndex--;
        }

        // refresh position
        Vector2 offset = GetOffset(size + GetSpacing());
        content.anchoredPosition += offset;
        m_ContentStartPosition += offset;
        _prevPosition += offset;

        return true;
    }

    private bool AddItemAtEnd()
    {
        if (!_delegateIsValidIndex(_itemEndIndex))
        {
            return false;
        }

        for (int i = 0; i < _contentConstraintCount; i++)
        {
            var item = GetFreeItem(_itemEndIndex);
            if (item == null)
            {
                break;
            }
            item.gameObject.SetActive(true);
            item.SetParent(content);
            item.localPosition = Vector3.zero;
            item.localScale = Vector3.one;
            _itemEndIndex++;
        }

        return true;
    }

    private bool RemoveItemAtStart()
    {
        if (!_delegateIsValidIndex(_itemEndIndex + _contentConstraintCount))
        {
            return false;
        }

        float size = 0f;
        for (int i = 0; i < _contentConstraintCount; i++)
        {
            var start = content.GetChild(0);
            start.gameObject.SetActive(false);
            start.SetParent(_freeItem);
            size = Mathf.Max(size, GetSize(start.GetComponent<RectTransform>().rect.size));
            _itemStartIndex++;
        }

        // refresh position
        Vector2 offset = GetOffset(size + GetSpacing());
        content.anchoredPosition -= offset;
        m_ContentStartPosition -= offset;
        _prevPosition -= offset;

        return true;
    }

    private bool RemoveItemAtEnd()
    {
        if (!_delegateIsValidIndex(_itemStartIndex - _contentConstraintCount))
        {
            return false;
        }

        for (int i = 0; i < _contentConstraintCount; i++)
        {
            var end = content.GetChild(content.childCount - 1).GetComponent<RectTransform>();
            end.gameObject.SetActive(false);
            end.SetParent(_freeItem);
            _itemEndIndex--;
        }

        return true;
    }

    private Bounds GetBounds()
    {
        if (content == null)
        {
            return new Bounds();
        }

        Vector3[] corners = new Vector3[4];
        content.GetWorldCorners(corners);

        var viewWorldToLocalMatrix = viewRect.worldToLocalMatrix;
        var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        for (int j = 0; j < 4; j++)
        {
            Vector3 v = viewWorldToLocalMatrix.MultiplyPoint3x4(corners[j]);
            vMin = Vector3.Min(v, vMin);
            vMax = Vector3.Max(v, vMax);
        }

        var bounds = new Bounds(vMin, Vector3.zero);
        bounds.Encapsulate(vMax);
        return bounds;
    }
}