using Boom.UI;
using UnityEngine;

public class OpenWindowBehaviour : MonoBehaviour
{
    [SerializeField] string windowName;
    [SerializeField] int sortingOrder = 0;
    [SerializeField] bool noParent;
    [SerializeField] bool openOnStart;
    [SerializeField, ShowOnly] Window window;

    private void Start()
    {
        if (openOnStart)
        {
            Open();
        }
    }
    private void OnDestroy()
    {
        if (openOnStart)
        {
            Close();
        }
    }

    public void Open()
    {
        if (window) return;
        window = WindowManager.Instance.OpenWindow(windowName, null, sortingOrder, noParent);
    }
    public void Close()
    {
        if (window == null) return;
        window.Close();
    }
}
