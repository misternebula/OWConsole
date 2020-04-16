using OWML.Common;
using OWML.ModHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OWConsole
{
    public class Main : ModBehaviour
    {
        bool _loaded;
        bool _collapse;
        bool _show;

        ChatHandler instance;
        private bool doFinalSetup;
        private static bool inputEnabled;

        void Start()
        {
            foreach (var gameObj in GameObject.FindObjectsOfType(typeof(ModBehaviour)))
            {

            }
        }

        public override void Configure(IModConfig config)
        {
            _collapse = config.GetSettingsValue<bool>("collapsed");
            _show = config.GetSettingsValue<bool>("shown");
            if (instance != null)
            {
                instance.ResetLog();
                instance.enabled = _show;
                instance.gameObject.SetActive(_show);
            }
        }

        void Update()
        {
            if (doFinalSetup)
            {
                base.ModHelper.HarmonyHelper.AddPrefix(typeof(ModConsole).GetMethods().Where(m => m.Name == "WriteLine").ElementAt(0), typeof(Patches), "OWMLLogPrefix");
                ModHelper.Console.WriteLine("[OWConsole] - OWML patch done.");
                doFinalSetup = false;
            }

            if (!_loaded)
            {
                GameObject mainCanvas = new GameObject("MessageCanvas");
                mainCanvas.SetActive(false);

                var rt = mainCanvas.AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(1920, 1080);
                rt.localScale = Vector3.one;
                rt.localPosition = new Vector3(0, 0, 0);

                var canvas = mainCanvas.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 9999;

                mainCanvas.AddComponent<CanvasRenderer>();

                var cs = mainCanvas.AddComponent<CanvasScaler>();
                cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                cs.referenceResolution = new Vector2(1920, 1080);
                cs.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                cs.matchWidthOrHeight = 1.0f;
                cs.referencePixelsPerUnit = 100;

                var ch = mainCanvas.AddComponent<ChatHandler>();
                ch.helper = ModHelper;

                mainCanvas.SetActive(true);

                _loaded = true;

                DontDestroyOnLoad(mainCanvas);
                instance = GameObject.FindObjectOfType<ChatHandler>();

                doFinalSetup = true;
            }
            if (_collapse && !instance._collapsed)
            {
                instance._collapsed = true;
            }
            else if (!_collapse && instance._collapsed)
            {
                instance._collapsed = false;
            }
        }
        public static void MNActivateInput()
        {
            inputEnabled = true;
        }

        public static void MNDeactivateInput()
        {
            inputEnabled = false;
        }
    }
}
