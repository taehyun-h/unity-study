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

        // free content item
        var content = GetComponent<ScrollRect>().content;
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            content.GetChild(i).SetParent(_freeItem);
        }

        // refresh scroll view
        _itemStartIndex = 0;
        _itemEndIndex = 0;
        float size = 0f;
        float spaceSize = GetSpacing();
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

            // add size
            size += (item.GetComponent<LayoutElement>().preferredWidth + spaceSize);

            // check view boundary
            if (size >= GetComponent<RectTransform>().sizeDelta.x)
            {
                break;
            }
        }

        // init position
        content.localPosition = Vector3.zero;
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
        if (true)
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

    private void UpdateBoundary()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        bool changed = false;
        var viewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
        var contentBounds = GetBounds();

        if (viewBounds.max.x > contentBounds.max.x)
        {
            // add item at end
            var item = GetFreeItem(_itemEndIndex);
            if (item != null)
            {
                item.gameObject.SetActive(true);
                item.SetParent(content);
                item.localPosition = Vector3.zero;
                item.localScale = Vector3.one;
                _itemEndIndex++;
                changed = true;
            }
        }
        else
        {
            // remove item at end
            var last = content.GetChild(content.childCount - 1).GetComponent<RectTransform>();
            if (viewBounds.max.x < contentBounds.max.x - (last.rect.size.x + GetSpacing()) * 1.1f && _delegateIsValidIndex(_itemStartIndex - 1))
            {
                last.gameObject.SetActive(false);
                last.SetParent(_freeItem);
                _itemEndIndex--;
                changed = true;
            }
        }

        if (viewBounds.min.x < contentBounds.min.x)
        {
            // add item at start
            var item = GetFreeItem(_itemStartIndex - 1);
            if (item != null)
            {
                item.gameObject.SetActive(true);
                item.SetParent(content);
                item.SetAsFirstSibling();
                item.localPosition = Vector3.zero;
                item.localScale = Vector3.one;
                _itemStartIndex--;

                // refresh position
                Vector2 offset = new Vector2(-(200 + GetSpacing()), 0f);
                content.anchoredPosition += offset;
                m_ContentStartPosition += offset;
                _prevPosition += offset;
                changed = true;
            }
        }
        else
        {
            // remove item at start
            var first = content.GetChild(0).GetComponent<RectTransform>();
            if (viewBounds.min.x > contentBounds.min.x + (first.rect.size.x + GetSpacing()) * 1.1f && _delegateIsValidIndex(_itemEndIndex + 1))
            {
                first.gameObject.SetActive(false);
                first.SetParent(_freeItem);
                _itemStartIndex++;

                // refresh position
                Vector2 offset = new Vector2(-(first.rect.size.x + GetSpacing()), 0f);
                content.anchoredPosition -= offset;
                m_ContentStartPosition -= offset;
                _prevPosition -= offset;
                changed = true;
            }
        }

        if (changed)
        {
            Canvas.ForceUpdateCanvases();
        }
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