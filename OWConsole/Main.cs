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

        ChatHandler instance;

        public override void Configure(IModConfig config)
        {
            if (instance != null)
            {
                instance.ResetLog();
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Keypad0))
            {
                instance.PostMessage("this is a test", "OWConsole", ChatHandler.MsgType.ERROR);
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
                canvas.pixelPerfect = true;

                mainCanvas.AddComponent<CanvasRenderer>();

                var cs = mainCanvas.AddComponent<CanvasScaler>();
                cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                cs.referenceResolution = new Vector2(1920, 1080);
                cs.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                cs.matchWidthOrHeight = 1.0f;
                cs.referencePixelsPerUnit = 100;

                var ch = mainCanvas.AddComponent<ChatHandler>();

                var gr = mainCanvas.AddComponent<GraphicRaycaster>();

                mainCanvas.SetActive(true);

                _loaded = true;

                ModConsole.OnConsole += OnOWMLLog;

                DontDestroyOnLoad(mainCanvas);
                instance = GameObject.FindObjectOfType<ChatHandler>();
            }
        }

        public void OnOWMLLog(IModManifest man, string message)
        {
            if (!message.Contains("Unity log message:"))
            {
                instance.PostMessage(message, man.Name, ChatHandler.MsgType.LOG);
            }
        }
    }
}
