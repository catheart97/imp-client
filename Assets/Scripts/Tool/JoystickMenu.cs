using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.UI;

namespace Tool
{
    public class JoystickMenu : MonoBehaviour
    {

        public GridObjectCollection GridCollection;
        public GameObject ButtonPrefab;

        public Interactable PathSmoothingInteractable = null;
        public Interactable PathAnimationInteractable = null;
        public Interactable InteractionInteractable = null;
        public Interactable PlanningInteractable = null;

        public JoystickObjectButton Selected
        {
            get
            {
                return _game_objects[_selected].GetComponent<JoystickObjectButton>();
            }
        }

        List<GameObject> _game_objects;
        int _selected = -1;

        bool _initialized = false;

        // Start is called before the first frame update
        public void Start()
        {
            _game_objects = new List<GameObject>();
        }

        // Update is called once per frame
        public void Update()
        {
            if (!_initialized)
            {
                Initialize();
            }
        }

        public void Initialize()
        {
            _initialized = true;
            IMP.IMPMovableObject[] objects = FindObjectsOfType<IMP.IMPMovableObject>();
            int id = 0;
            foreach (var obj in objects)
            {
                GameObject button = Instantiate(ButtonPrefab, GridCollection.gameObject.transform);
                button.GetComponent<JoystickObjectButton>().Init(obj, this, id++);
                _game_objects.Add(button);
                GridCollection.UpdateCollection();
            }
            Select(0);
        }

        public void Select(int id)
        {
            _selected = id;
            for (int i = 0; i < _game_objects.Count; i++)
            {
                if (id == i)
                {
                    var button = _game_objects[i].GetComponent<JoystickObjectButton>();
                    button.Select();
                    if (PathAnimationInteractable != null)
                        PathAnimationInteractable.IsToggled = button.Object.EnablePathAnimation;
                    if (PathSmoothingInteractable != null)
                        PathSmoothingInteractable.IsToggled = button.Object.EnablePathSmoothing;
                    if (InteractionInteractable != null)
                        InteractionInteractable.IsToggled = button.Object.InteractionAllowed;
                    if (PlanningInteractable != null)
                        PlanningInteractable.IsToggled = button.Object.EnablePlanning;
                }
                else
                    _game_objects[i].GetComponent<JoystickObjectButton>().Deselect();
            }
        }

        public void TogglePathSmoothing()
        {
            var button = _game_objects[_selected].GetComponent<JoystickObjectButton>();
            button.Object.EnablePathSmoothing = !button.Object.EnablePathSmoothing;
            if (PathSmoothingInteractable != null)
                PathSmoothingInteractable.IsToggled = button.Object.EnablePathSmoothing;
        }

        public void TogglePathAnimation()
        {
            var button = _game_objects[_selected].GetComponent<JoystickObjectButton>();
            button.Object.EnablePathAnimation = !button.Object.EnablePathAnimation;
            if (PathAnimationInteractable != null)
                PathAnimationInteractable.IsToggled = button.Object.EnablePathAnimation;
        }

        public void ToggleInteraction()
        {
            var button = _game_objects[_selected].GetComponent<JoystickObjectButton>();
            button.Object.InteractionAllowed = !button.Object.InteractionAllowed;
            if (InteractionInteractable != null)
                InteractionInteractable.IsToggled = button.Object.InteractionAllowed;
        }

        public void TogglePlanning()
        {
            var button = _game_objects[_selected].GetComponent<JoystickObjectButton>();
            button.Object.EnablePlanning = !button.Object.EnablePlanning;
            if (PlanningInteractable != null)
                PlanningInteractable.IsToggled = button.Object.EnablePlanning;
        }
    }

}