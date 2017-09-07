using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollViewSceneManager : MonoBehaviour
{
    public GameObject _scrollRect;
    public GameObject _scrollRect2;
    public GameObject _scrollRect3;
    public GameObject _item;
    public int _size = 10;

    private void Awake()
    {
        // horizontal view
        _scrollRect.GetComponent<DefaultScrollView>()._size = _size;
        _scrollRect.GetComponent<DefaultScrollView>()._delegateGetItem = GetItem;
        _scrollRect.GetComponent<DefaultScrollView>().Refresh();

        // vertical view
        _scrollRect2.GetComponent<DefaultScrollView>()._size = _size;
        _scrollRect2.GetComponent<DefaultScrollView>()._delegateGetItem = GetItem;
        _scrollRect2.GetComponent<DefaultScrollView>().Refresh();

        // grid view
        _scrollRect3.GetComponent<DefaultScrollView>()._size = _size;
        _scrollRect3.GetComponent<DefaultScrollView>()._delegateGetItem = GetItem;
        _scrollRect3.GetComponent<DefaultScrollView>().Refresh();
    }

    public RectTransform GetItem(int index)
    {
        if (index < 0 || index >= _size)
        {
            return null;
        }
        var item = Instantiate(_item) as GameObject;
        item.GetComponentInChildren<Text>().text = (index + 1).ToString();
        return item.GetComponent<RectTransform>();
    }
}