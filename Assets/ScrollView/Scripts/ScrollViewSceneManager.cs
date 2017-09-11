using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollViewSceneManager : MonoBehaviour
{
    // default scroll view
    public GameObject _horizontalScrollRect;
    public GameObject _verticalScrollRect;
    public GameObject _gridScrollRect;
    public GameObject _item;
    public int _size;

    // recycle scroll view
    public GameObject _horizontalRecycleScrollRect;
    public GameObject _verticalRecycleScrollRect;
    public GameObject _gridRecycleScrollRect;

    private void Awake()
    {
        // default scroll view
        // horizontal view
        _horizontalScrollRect.GetComponent<DefaultScrollView>()._size = _size;
        _horizontalScrollRect.GetComponent<DefaultScrollView>()._delegateGetItem = GetDefaultItem;
        _horizontalScrollRect.GetComponent<DefaultScrollView>().Refresh();

        // vertical view
        _verticalScrollRect.GetComponent<DefaultScrollView>()._size = _size;
        _verticalScrollRect.GetComponent<DefaultScrollView>()._delegateGetItem = GetDefaultItem;
        _verticalScrollRect.GetComponent<DefaultScrollView>().Refresh();

        // grid view
        _gridScrollRect.GetComponent<DefaultScrollView>()._size = _size;
        _gridScrollRect.GetComponent<DefaultScrollView>()._delegateGetItem = GetDefaultItem;
        _gridScrollRect.GetComponent<DefaultScrollView>().Refresh();

        // recycle scroll view
        // horizontal view
        _horizontalRecycleScrollRect.GetComponent<RecycleScrollView>()._delegateGetItem = GetRecycleItem;
        _horizontalRecycleScrollRect.GetComponent<RecycleScrollView>()._delegateRefreshItem = RefreshRecycleItem;
        _horizontalRecycleScrollRect.GetComponent<RecycleScrollView>()._delegateIsValidIndex = IsValidIndex;
        _horizontalRecycleScrollRect.GetComponent<RecycleScrollView>().Refresh();

        // veritcal view
        _verticalRecycleScrollRect.GetComponent<RecycleScrollView>()._delegateGetItem = GetRecycleItem;
        _verticalRecycleScrollRect.GetComponent<RecycleScrollView>()._delegateRefreshItem = RefreshRecycleItem;
        _verticalRecycleScrollRect.GetComponent<RecycleScrollView>()._delegateIsValidIndex = IsValidIndex;
        _verticalRecycleScrollRect.GetComponent<RecycleScrollView>().Refresh();

        // grid view
        _gridRecycleScrollRect.GetComponent<RecycleScrollView>()._delegateGetItem = GetRecycleItem;
        _gridRecycleScrollRect.GetComponent<RecycleScrollView>()._delegateRefreshItem = RefreshRecycleItem;
        _gridRecycleScrollRect.GetComponent<RecycleScrollView>()._delegateIsValidIndex = IsValidIndex;
        _gridRecycleScrollRect.GetComponent<RecycleScrollView>().Refresh();
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