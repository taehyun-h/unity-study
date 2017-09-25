using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class DefaultScrollRect : ScrollRect
{
    public delegate RectTransform GetItem(int index);
    public delegate int GetItemSize();
    [HideInInspector]
    public GetItem _delegateGetItem = null;
    [HideInInspector]
    public GetItemSize _delegateGetItemSize = null;
    private int _contentConstraintCount;
    private float _originalSize = -1f;

    public void Refresh()
    {
        if (_originalSize < 0)
        {
            _originalSize = GetSize(GetComponent<RectTransform>().sizeDelta);
        }
        if (!gameObject.activeSelf)
        {
            return;
        }
        if (_delegateGetItem == null)
        {
            Debug.LogError("GetItem method is null!");
        }
        if (_delegateGetItemSize == null)
        {
            Debug.LogError("GetItemSize method is null!");
        }

        // get constraint count
        _contentConstraintCount = 1;
        var gridLayoutGroup = content.GetComponent<GridLayoutGroup>();
        if (gridLayoutGroup != null)
        {
            if (gridLayoutGroup.constraint == GridLayoutGroup.Constraint.Flexible)
            {
                Debug.LogError("Flexible GridLayoutGroup is not support");
                return;
            }
            _contentConstraintCount = gridLayoutGroup.constraintCount;
        }

        // clear content
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }

        // add item
        int itemSize = _delegateGetItemSize();
        float size = 0f;
        for (int i = 0; i < itemSize; i++)
        {
            // get item
            var item = _delegateGetItem(i);
            if (item == null)
            {
                Debug.LogWarning("item index - " + i.ToString() + " - is null");
                continue;
            }

            // set item to content's child
            item.SetParent(content);
            item.localPosition = Vector3.zero;
            item.localScale = Vector3.one;

            // get size
            if (horizontal)
            {
                size += (item.GetComponent<LayoutElement>().preferredWidth + GetSpacing());
            }
            else
            {
                size += (item.GetComponent<LayoutElement>().preferredHeight + GetSpacing());
            }
        }

        // fit content size
        if (horizontal)
        {
            if (size < _originalSize)
            {
                var sizeDelta = GetComponent<RectTransform>().sizeDelta;
                sizeDelta.x = size;
                GetComponent<RectTransform>().sizeDelta = sizeDelta;
            }
        }
        else
        {
            if (size < _originalSize)
            {
                var sizeDelta = GetComponent<RectTransform>().sizeDelta;
                sizeDelta.y = size;
                GetComponent<RectTransform>().sizeDelta = sizeDelta;
            }
        }
    }

    public void SetIndex(int index)
    {
        if (index < 0 || index >= content.childCount)
        {
            Debug.LogWarning("item index - " + index.ToString() + " - is out of index");
            return;
        }

        // set position
        var localPosition = content.localPosition;
        var item = content.GetChild(index).GetComponent<RectTransform>();
        if (horizontal)
        {
            localPosition.x = (item.sizeDelta.x + GetSpacing()) * (index / _contentConstraintCount) * -1;
            float size = GetComponent<RectTransform>().rect.size.x + GetSpacing();
            if (localPosition.x < size * -1)
            {
                localPosition.x = size * -1;
            }
        }
        else
        {
            localPosition.y = (item.sizeDelta.y + GetSpacing()) * (index / _contentConstraintCount) * -1;
            float size = GetComponent<RectTransform>().rect.size.y + GetSpacing();
            if (localPosition.y < size * -1)
            {
                localPosition.y = size * -1;
            }
        }
        content.localPosition = localPosition;
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
}