using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollViewSceneManager : MonoBehaviour
{
    // default scroll rect
    public GameObject _horizontalScrollRect;
    public GameObject _verticalScrollRect;
    public GameObject _gridScrollRect;
    public GameObject _item;
    public int _size;

    // recycle scroll rect
    public GameObject _horizontalRecycleScrollRect;
    public GameObject _verticalRecycleScrollRect;
    public GameObject _gridRecycleScrollRect;

    private void Awake()
    {
        // default scroll view
        // horizontal view
        _horizontalScrollRect.GetComponent<DefaultScrollRect>()._size = _size;
        _horizontalScrollRect.GetComponent<DefaultScrollRect>()._delegateGetItem = GetDefaultItem;
        _horizontalScrollRect.GetComponent<DefaultScrollRect>().Refresh();

        // vertical view
        _verticalScrollRect.GetComponent<DefaultScrollRect>()._size = _size;
        _verticalScrollRect.GetComponent<DefaultScrollRect>()._delegateGetItem = GetDefaultItem;
        _verticalScrollRect.GetComponent<DefaultScrollRect>().Refresh();

        // grid view
        _gridScrollRect.GetComponent<DefaultScrollRect>()._size = _size;
        _gridScrollRect.GetComponent<DefaultScrollRect>()._delegateGetItem = GetDefaultItem;
        _gridScrollRect.GetComponent<DefaultScrollRect>().Refresh();

        // recycle scroll rect
        // horizontal rect
        _horizontalRecycleScrollRect.GetComponent<RecycleScrollRect>()._delegateGetItem = GetRecycleItem;
        _horizontalRecycleScrollRect.GetComponent<RecycleScrollRect>()._delegateRefreshItem = RefreshRecycleItem;
        _horizontalRecycleScrollRect.GetComponent<RecycleScrollRect>()._delegateIsValidIndex = IsValidIndex;
        _horizontalRecycleScrollRect.GetComponent<RecycleScrollRect>().Refresh();

        // veritcal rect
        _verticalRecycleScrollRect.GetComponent<RecycleScrollRect>()._delegateGetItem = GetRecycleItem;
        _verticalRecycleScrollRect.GetComponent<RecycleScrollRect>()._delegateRefreshItem = RefreshRecycleItem;
        _verticalRecycleScrollRect.GetComponent<RecycleScrollRect>()._delegateIsValidIndex = IsValidIndex;
        _verticalRecycleScrollRect.GetComponent<RecycleScrollRect>().Refresh();

        // grid rect
        _gridRecycleScrollRect.GetComponent<RecycleScrollRect>()._delegateGetItem = GetRecycleItem;
        _gridRecycleScrollRect.GetComponent<RecycleScrollRect>()._delegateRefreshItem = RefreshRecycleItem;
        _gridRecycleScrollRect.GetComponent<RecycleScrollRect>()._delegateIsValidIndex = IsValidIndex;
        _gridRecycleScrollRect.GetComponent<RecycleScrollRect>().Refresh();
    }

    public RectTransform GetDefaultItem(int index)
    {
        if (index < 0 || index >= _size)
        {
            return null;
        }
        var item = Instantiate(_item) as GameObject;
        item.GetComponentInChildren<Text>().text = (index + 1).ToString();
        return item.GetComponent<RectTransform>();
    }

    public RectTransform GetRecycleItem(int index)
    {
        if (!IsValidIndex(index))
        {
            return null;
        }
        var item = Instantiate(_item) as GameObject;
        item.GetComponentInChildren<Text>().text = (index + 1).ToString();
        return item.GetComponent<RectTransform>();
    }

    public void RefreshRecycleItem(RectTransform item, int index)
    {
        item.GetComponentInChildren<Text>().text = (index + 1).ToString();
    }

    public bool IsValidIndex(int index)
    {
        return index >= 0 && index < _size;
    }
}