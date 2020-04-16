using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OWConsole
{
    public static class Patches
    {
        static bool OWMLLogPrefix(ref string s)
        {
            //GameObject.FindObjectOfType<ChatHandler>().LogOWMLMod(s);
            return true;
        }
    }
}