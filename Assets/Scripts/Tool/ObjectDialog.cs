using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.UI;

public class ObjectDialog : MonoBehaviour
{
    public List<GameObject> Prefabs;

    private List<ScenarioButton> _buttons = new List<ScenarioButton>();

    public GameObject ButtonPrefab;

    public GameObject Container;
    public GameObject Content;

    private Tool.JoystickMenu _menu;
    private Tool.HandMenu _hand_menu;

    private GameObject _scenario = null;

    // Start is called before the first frame update
    public void Start()
    {
        _menu = FindObjectOfType<Tool.JoystickMenu>();
        _hand_menu = GetComponent<Tool.HandMenu>();
        int index = 0;
        foreach (var prefab in Prefabs)
        {
            var button = Instantiate(ButtonPrefab, Container.transform);
            _buttons.Add(button.GetComponent<ScenarioButton>());
            _buttons[^1].Initialize(prefab.name, this, index++);
        }
        Container.GetComponent<GridObjectCollection>().UpdateCollection();
        Content.GetComponent<ScrollingObjectCollection>().UpdateContent();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void RemoveInstatiation()
    {
        if (_scenario != null)
        {
            Destroy(_scenario);
            foreach (var o in IMP.IMPMovableObject.Companions) Destroy(o);
            IMP.IMPMovableObject.Companions.Clear();
            _scenario = null;
        }
    }

    public void Pressed(int index, bool pressed)
    {
        if (index >= 0)
        {
            RemoveInstatiation();
            for (int i = 0; i < _buttons.Count; ++i)
            {
                _buttons[i].GetComponent<Interactable>().IsToggled = index == i && pressed;
            }

            var config = FindObjectOfType<IMP.IMPConfiguration>();
            config.ClearRemote();

            if (pressed)
            {
                _scenario = Instantiate(Prefabs[index], Vector3.zero, Quaternion.identity);
            }

            _menu?.Initialize();
            _hand_menu?.Reset();
        }
        Destroy(gameObject);
    }
}
