using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
[RequireComponent(typeof(ScrollRect))]
public class DefaultScrollView : MonoBehaviour
{
    public delegate RectTransform GetItem(int index);
    [HideInInspector]
    public GetItem _delegateGetItem = null;
    [HideInInspector]
    public int _size = 0;

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

        // clear content
        var content = GetComponent<ScrollRect>().content;
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }

        // add item
        for (int i = 0; i < _size; i++)
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
        }

        // init position
        content.localPosition = Vector3.zero;
    }
}