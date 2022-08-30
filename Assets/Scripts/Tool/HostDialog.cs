using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.Experimental.UI;

namespace Tool
{

    class HostDialog : MonoBehaviour
    {
        public GameObject TextField;

        private TextMeshPro _text_field;
        private TouchScreenKeyboard _keyboard = null;

        public void Update()
        {
            if (_keyboard != null)
            {
                _text_field.text = _keyboard.text;
            }
        }

        public void Start()
        {
            ConnectionDialog.HDialog = gameObject;
            _text_field = TextField.GetComponent<TextMeshPro>();
            gameObject.SetActive(false);
        }

        public void Confirm()
        {
            try 
            {
                var res = _text_field.text.Split(":");
                uint port = uint.Parse(res[1], System.Globalization.NumberStyles.Number);
                string host = res[0];
                IMP.IMPConfiguration.CollisionIP = host;
                IMP.IMPConfiguration.CollisionPort = port;
            }
            catch (System.Exception)
            {}
            _keyboard = null;
            FindObjectOfType<IMP.IMPConfiguration>().Connect();
            gameObject.SetActive(false);
        }

        public void OpenKeyboard()
        {
            _text_field.text = Web.Requests.Host;
            _keyboard = TouchScreenKeyboard.Open(_text_field.text, TouchScreenKeyboardType.URL, false, false, false, false);
        }
    }

}