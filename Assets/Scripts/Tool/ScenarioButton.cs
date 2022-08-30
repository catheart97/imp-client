using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;

public class ScenarioButton : MonoBehaviour
{
    private int _index = -1;

    public ObjectDialog Dialog {get; set;}

    public TextMeshPro Title;

    private string _name;

    // Start is called before the first frame update
    public void Start()
    {
        GetComponent<PressableButtonHoloLens2>().ButtonPressed.AddListener(Interact);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize(string name, ObjectDialog dialog, int index)
    {
        Dialog = dialog;
        _name = name;
        _index = index;
        Title.text = name;
    }

    public void Interact()
    {
        Dialog.Pressed(_index, GetComponent<Interactable>().IsToggled);
    }
}
