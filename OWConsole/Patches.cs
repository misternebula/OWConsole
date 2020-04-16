using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Diagnostics;

namespace OWConsole
{
    public static class Patches
    {
        static bool OWMLLogPrefix(ref string s)
        {
            StackTrace st = new StackTrace();
            GameObject.FindObjectOfType<ChatHandler>().HandleOWMLLog(s, st.GetFrames());
            return true;
        }
    }
}
