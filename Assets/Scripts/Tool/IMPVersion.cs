using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;

namespace Tool
{
    public class IMPVersion : MonoBehaviour
    {
        IMPVersion()
        {

        }

        // Start is called before the first frame update
        public void Awake()
        {
            GetComponent<TextMeshPro>().text = Application.version + "a";
        }
    }
}
