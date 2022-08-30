using UnityEngine;
using System.Threading.Tasks;
using System.Threading;

namespace IMP
{
    /// <summary>
    ///     Initalization Script for the IMP application.
    /// </summary>
    public class IMPConfiguration : MonoBehaviour
    {
        /////////
        // unity properties
        /////////
        [Header("Remote Collision System")]
        public static string CollisionIP = "192.168.188.99";
        public static uint CollisionPort = 8000;

        [Header("Visualization")]
        public Material GhostMaterial = null;
        public GameObject GrabHand;

        /////////
        // data
        /////////
        private Task _clear_task = null;
        private static bool _connected = false;
        private bool _show_dialog = false;

        public static GameObject Dialog = null;

        IMPMovableObject[] _movables;
        IMPStaticObject[] _statics;

        /////////
        // properties
        /////////
        public static bool InitializedRemoteCollision
        {
            get
            {
                return _connected;
            }
        }

        public async Task ClearRemote()
        {
            string uri = Web.Requests.Address + "clear";
            bool success = false;
            try
            {
                var result = await Web.Requests.HttpClient.GetAsync(uri);
                success = result.IsSuccessStatusCode;
            }
            catch (System.Exception)
            {
            }

            if (!success)
            {
                _show_dialog = true;
            }
            else
            {
                _connected = true;
            }
        }

        /////////
        // methods
        /////////

        public void Connect()
        {
            Tool.LoadingIndicator.Show();
            _clear_task = ClearRemote();
        }

        public void UpdateObjects()
        {
            foreach (var item in _movables)
            {
                item.gameObject.SetActive(_connected);
            }
            foreach (var item in _statics)
            {
                item.gameObject.SetActive(_connected);
            }
        }

        public void Start()
        {
            _movables = FindObjectsOfType<IMPMovableObject>();
            _statics = FindObjectsOfType<IMPStaticObject>();
            UpdateObjects();
            Connect();
        }

        public void Update()
        {
            if (_connected)
            {
                Tool.LoadingIndicator.Hide();
                UpdateObjects();
            }

            if (_show_dialog)
            {
                Tool.LoadingIndicator.Hide();
                Dialog.SetActive(true);
                _show_dialog = false;
            }
        }
    }
}

