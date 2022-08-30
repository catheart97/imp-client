using UnityEngine;
using TMPro;

namespace Tool
{
    public class ConnectionDialog : MonoBehaviour
    {
        public GameObject DescriptionText;

        public static GameObject HDialog;

        private TouchScreenKeyboard _keyboard = null;

        public void Retry()
        {
            FindObjectOfType<IMP.IMPConfiguration>().Connect();
            gameObject.SetActive(false);
        }

        public void RetryNew()
        {
            HDialog.SetActive(true);
            gameObject.SetActive(false);
        }

        public void Start()
        {
            IMP.IMPConfiguration.Dialog = gameObject;
            UpdateDescription();
            gameObject.SetActive(false);
        }

        private void UpdateDescription()
        {
            DescriptionText.GetComponent<TextMeshPro>().text = "The host: \"" + Web.Requests.Host + "\" is not responding. Maybe it is not running. Check your network configuration or retry.";
        }

        public void Update()
        {
            UpdateDescription();
        }
    }}