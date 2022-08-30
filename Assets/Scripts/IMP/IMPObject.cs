using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace IMP
{
    public class IMPObject : MonoBehaviour
    {
        /////////
        // data
        /////////
        private int _web_id = -1;
        private Task<int> _setup_task = null;

        /////////
        // properties
        /////////
        public Vector3 InitialPosition { get; set; }
        public Vector3 LocalScale { get; set; }
        public Quaternion InitialRotation { get; set; }
        public int WebId { get { return _web_id; } }

        public string Name { get; set; }

        /////////
        // methods
        /////////
        protected void CreateRemote(bool movable)
        {
            if (IMPConfiguration.InitializedRemoteCollision &&
                _setup_task == null)
            {
                var mesh_filter = GetComponent<MeshFilter>();
                var mesh = mesh_filter.mesh;
                _setup_task = Web.Requests.CreateWebObject(
                    movable,
                    mesh.vertices,
                    mesh.triangles,
                    gameObject.transform.localScale,
                    new Configuration
                    {
                        Position = gameObject.transform.position,
                        Rotation = gameObject.transform.rotation
                    },
                    Name);
            }
        }

        /////////
        // unity interface
        /////////
        public void BaseStart()
        {
            LocalScale = gameObject.transform.localScale;
            InitialPosition = gameObject.transform.position;
            InitialRotation = gameObject.transform.rotation;
            Name = gameObject.name;
        }

        public void BaseUpdate()
        {
            if (_setup_task != null && _setup_task.IsCompleted && _web_id == -1)
            {
                _web_id = _setup_task.Result;
            }
        }
    }
}
