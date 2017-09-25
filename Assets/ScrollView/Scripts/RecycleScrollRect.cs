using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RecycleScrollRect : ScrollRect
{
    public delegate RectTransform GetItem(int index);
    public delegate void RefreshItem(RectTransform item, int index);
    public delegate bool IsValidIndex(int index);
    [System.NonSerialized] public GetItem _delegateGetItem = null;
    [System.NonSerialized] public RefreshItem _delegateRefreshItem = null;
    [System.NonSerialized] public IsValidIndex _delegateIsValidIndex = null;
    [System.NonSerialized] public GameObject _pool;
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

    public void Init(int index)
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

        // refresh content
        _itemStartIndex = index - (index % _contentConstraintCount);
        _itemEndIndex = index - (index % _contentConstraintCount);
        float size = 0f;
        // add item at end
        while (true)
        {
            if (horizontal)
            {
                if (size < GetComponent<RectTransform>().rect.size.x)
                {
                    if (!AddItemAtEnd())
                    {
                        break;
                    }
                    size += (content.GetChild(0).GetComponent<LayoutElement>().preferredWidth + GetSpacing());
                }
                else
                {
                    break;
                }
            }
            else
            {
                if (size < GetComponent<RectTransform>().rect.size.y)
                {
                    if (!AddItemAtEnd())
                    {
                        break;
                    }
                    size += (content.GetChild(0).GetComponent<LayoutElement>().preferredHeight + GetSpacing());
                }
                else
                {
                    break;
                }
            }
        }
        // check enough to item at first
        float size2 = size;
        while (true)
        {
            if (horizontal)
            {
                if (size < GetComponent<RectTransform>().rect.size.x)
                {
                    if (!AddItemAtStart())
                    {
                        break;
                    }
                    size += (content.GetChild(0).GetComponent<LayoutElement>().preferredWidth + GetSpacing());
                }
                else
                {
                    break;
                }
            }
            else
            {
                if (size < GetComponent<RectTransform>().rect.size.y)
                {
                    if (!AddItemAtStart())
                    {
                        break;
                    }
                    size += (content.GetChild(0).GetComponent<LayoutElement>().preferredHeight + GetSpacing());
                }
                else
                {
                    break;
                }
            }
        }

        // init position
        content.localPosition = Vector3.zero;
        if (index != -1)
        {
            float itemSize = horizontal ? content.GetChild(index - _itemStartIndex).GetComponent<LayoutElement>().preferredWidth : content.GetChild(index - _itemStartIndex).GetComponent<LayoutElement>().preferredHeight;
            float viewSize = GetSize(GetComponent<RectTransform>().rect.size) - (horizontal ? content.GetComponent<LayoutGroup>().padding.left : content.GetComponent<LayoutGroup>().padding.top);
            if (size - size2 + itemSize >= viewSize)
            {
                var localPosition = content.localPosition;
                localPosition += (Vector3)GetOffset(size - size2 + itemSize - viewSize);
                content.localPosition = localPosition;
            }
        }
    }

    public void Refresh()
    {
        int count = 0;
        for (int i = _itemStartIndex; i < _itemEndIndex; i++)
        {
            if (!_delegateIsValidIndex(i))
            {
                _itemEndIndex = i;
                for (int j = content.childCount - 1; j >= count; j++)
                {
                    var child = content.GetChild(j);
                    child.gameObject.SetActive(false);
                    child.SetParent(_freeItem);
                }
            }
            var item = content.GetChild(count).GetComponent<RectTransform>();
            _delegateRefreshItem(item, i);
            count++;
        }
    }

    private void CreatePool()
    {
        if (_freeItem != null)
        {
            return;
        }

        // create pool
        var pool = new GameObject(gameObject.name + "Pool");
        pool.transform.SetParent(_pool.transform);
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
            if (content.GetComponent<HorizontalLayoutGroup>() == null)
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
            if (content.GetComponent<VerticalLayoutGroup>() == null)
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
        int removeCount = (_itemEndIndex - _itemStartIndex) % _contentConstraintCount;
        if (removeCount == 0)
        {
            removeCount = _contentConstraintCount;
        }

        if (!_delegateIsValidIndex(_itemStartIndex - removeCount))
        {
            return false;
        }

        for (int i = 0; i < removeCount; i++)
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