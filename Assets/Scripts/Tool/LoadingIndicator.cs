using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;

namespace Tool
{

class LoadingIndicator : MonoBehaviour
{
    private static bool _show = true;
    private static LoadingIndicator _indicator = null;

    private static GameObject gobj;


    public void Start()
    {
        _indicator = this;
        gobj = gameObject;
        Hide();
    }

    public static void Show()
    {
        gobj.SetActive(true);
        gobj.GetComponent<IProgressIndicator>().OpenAsync();
    }

    public static void Hide()
    {
        gobj.GetComponent<IProgressIndicator>().CloseAsync();
        gobj.SetActive(false);
    }
}

}