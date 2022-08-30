using UnityEngine;
using TMPro;

namespace Tool
{
    /// <summary>
    ///     MonoBehaviour used for in-game logging hand menu console.
    ///     Handles string formatting. Needs to be attached to the LogObject, which reference
    ///     need to be mapped.
    /// </summary>
    public class Console : MonoBehaviour
    {
        /////////
        // data
        /////////
        public GameObject LogObject;
        private static GameObject _object = null;
        private static string _content = null;

        /////////
        // methods
        /////////
        public void Start()
        {
            if (_object == null)
                _object = LogObject;
            _content = _object.GetComponent<TextMeshPro>().text;
        }

        public void Update()
        { 
            lock(_object)
            {
                lock (_content)
                {
                    _object.GetComponent<TextMeshPro>().text = _content;
                }
            }
        }

        /// <summary>
        ///     Truncates the given string log to maximum num_lines by removing lines in the
        ///     beginning.
        /// </summary>
        /// <param name="num_lines">Maximum number of lines</param>
        /// <param name="log">The to be shortened string.</param>
        /// <returns>The truncated string</returns>
        public static string Truncate(int num_lines, string log)
        {
            log = InsertNewLines(log, 33);

            int num_newlines = 0;
            foreach(char c in log)
            {
                if (c == '\n')
                {
                    num_newlines++;
                }
            }

            if (num_newlines > num_lines)
            {
                string buffer = "";
                int remaining = num_newlines - num_lines;
                foreach(char c in log)
                {
                    if (remaining == 0)
                        buffer += c;
                    else if (c == '\n')
                        remaining--;
                }
                return buffer;
            }
            else 
            {
                return log;
            }
        }

        public static string InsertNewLines(string text, int max_per_line)
        {
            string res = "";
            int num_per_line = 0;
            foreach (char c in text)
            {
                if (c == '\n')
                {
                    num_per_line = 0;
                    res += c;
                }
                else
                {
                    num_per_line++;
                    if (num_per_line >= max_per_line)
                    {
                        res = res + "\n    " + c;
                        num_per_line = 5;
                    }
                    else
                    {
                        res += c;
                    }
                }
            }
            return res;
        }

        /// <summary>
        ///     Log a message to the hand menu and the unity debug log.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Log(string message)
        {
            var final_message = "> " + message;
            if (_object != null)
            {
                lock (_content)
                {
                    _content = Truncate(18, _content + "\n" + final_message);
                }
            }
            Debug.Log(final_message);
        }
    }
}