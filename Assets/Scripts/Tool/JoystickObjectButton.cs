using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;

namespace Tool
{
    public class JoystickObjectButton : MonoBehaviour
    {
        public TextMeshPro TMPro;

        public IMP.IMPMovableObject Object {
           get { return _object; }
        }

        IMP.IMPMovableObject _object = null;
        JoystickMenu _menu = null;
        int _id = -1;

        //public IMP.IMPMovableObject Object { get { return _object; } }

        public void Start()
        {
            GetComponent<PressableButtonHoloLens2>().ButtonPressed.AddListener(Interact);
        }

        // Update is called once per frame
        public void Update()
        {
        }

        public void Init(IMP.IMPMovableObject obj, JoystickMenu menu, int id)
        {
            TMPro.text = obj.Name;
            _object = obj;
            _menu = menu;
            _id = id;
        }

        public void Select()
        {
            GetComponent<Interactable>().IsToggled = true;
        }

        public void Deselect()
        {
            GetComponent<Interactable>().IsToggled = false;
        }

        public void Interact()
        {
            _menu.Select(_id);
        }
    }

}
