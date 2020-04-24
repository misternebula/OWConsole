using OWML.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static ChatHandler;

public class ChatHandler : MonoBehaviour
{
    List<GameObject> _boxList = new List<GameObject>();

    List<Message> _messages = new List<Message>();

    GameObject _board;

    GameObject _input;

    Vector2 _boxSize = new Vector2(790, 30);
    const int _fontSize = 12;

    private Text _searchBox;

    private string prevText;

    void Awake()
    {
        _board = new GameObject("Board");
        _board.transform.parent = gameObject.transform;

        var rt = _board.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(800, -480);
        rt.localPosition = new Vector3(410, -232, 0);
        rt.anchorMax = new Vector2(0, 1);
        rt.anchorMin = new Vector2(0, 0);
        rt.pivot = new Vector2(.5f, .5f);
        rt.localScale = Vector3.one;

        Application.logMessageReceived += this.OnLogMessageReceived;

        _board.AddComponent<CanvasRenderer>();
        var ri = _board.AddComponent<RawImage>();
        ri.color = new Color32(0, 0, 0, 100);
        ri.raycastTarget = false;

        var eventSys = new GameObject("EventSystem");
        eventSys.transform.parent = _board.transform;

        var sys = eventSys.AddComponent<EventSystem>();
        sys.sendNavigationEvents = false;
        sys.pixelDragThreshold = 10;

        var input = eventSys.AddComponent<StandaloneInputModule>();
        input.horizontalAxis = "Mouse_X";
        input.verticalAxis = "Mouse_Y";
        input.submitButton = "Joystick4Axis9";
        input.cancelButton = "Joystick4Axis10";
        input.inputActionsPerSecond = 10;
        input.repeatDelay = 0.5f;
        input.forceModuleActive = false;

        _input = new GameObject("InputField");
        _input.transform.parent = _board.transform;
        var irt = _input.AddComponent<RectTransform>();
        irt.sizeDelta = new Vector2(500, 30);
        irt.localPosition = new Vector3(0, 320, 0);

        irt.localScale = Vector3.one;

        _input.AddComponent<CanvasRenderer>();

        _input.AddComponent<Image>();

        var placeholder = new GameObject("Placeholder");
        placeholder.transform.parent = _input.transform;

        var prt = placeholder.AddComponent<RectTransform>();
        prt.anchorMax = new Vector2(1, 1);
        prt.anchorMin = new Vector2(0, 0);
        prt.pivot = new Vector2(.5f, .5f);
        prt.sizeDelta = new Vector2(0, 0);
        prt.transform.localPosition = Vector3.zero;
        prt.transform.localScale = Vector3.one;

        var text = placeholder.AddComponent<Text>();
        text.text = "Enter text...";
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.color = Color.black;

        var textGO = new GameObject("Text");
        textGO.transform.parent = _input.transform;

        var trt = textGO.AddComponent<RectTransform>();
        trt.anchorMax = new Vector2(1, 1);
        trt.anchorMin = new Vector2(0, 0);
        trt.pivot = new Vector2(.5f, .5f);
        trt.sizeDelta = new Vector2(0, 0);
        trt.transform.localPosition = Vector3.zero;
        trt.transform.localScale = Vector3.one;

        _searchBox = textGO.AddComponent<Text>();
        _searchBox.text = "";
        _searchBox.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        _searchBox.color = Color.black;

        var inf = _input.AddComponent<InputField>();
        inf.interactable = true;
        inf.placeholder = text;
        inf.textComponent = _searchBox;

        SceneManager.sceneLoaded += (Scene scene, LoadSceneMode mod) => ResetLog();
    }

    private void OnLogMessageReceived(string message, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception)
        {
            PostMessage(message + ". Stack trace: " + ((stackTrace != null) ? stackTrace.Trim() : null), "Unity", MsgType.ERROR);
        }
        else if (type == LogType.Warning)
        {
            PostMessage(message, "Unity", MsgType.WARNING);
        }
        else
        {
            PostMessage(message, "Unity", MsgType.LOG);
        }
    }

    void Update()
    {
        if (_searchBox.text != prevText)
        {
            prevText = _searchBox.text;
            DisplayMessages(prevText);
        }
    }

    public void ResetLog()
    {
        
        foreach (var item in _boxList)
        {
            GameObject.Destroy(item);
        }
        _boxList = new List<GameObject>();

        _messages = new List<Message>();

        foreach (var item in GameObject.FindObjectsOfType<RawImage>())
        {
            if (item.gameObject.name == "OWConsoleMessageBox")
            {
                GameObject.Destroy(item.gameObject);
            }
        }
    }

    private void DisplayMessages(string searchTerm = "")
    {
        foreach (var item in _boxList)
        {
            Destroy(item);
        }
        _boxList = new List<GameObject>();

        foreach (var item in GameObject.FindObjectsOfType<RawImage>())
        {
            if (item.gameObject.name == "OWConsoleMessageBox")
            {
                GameObject.Destroy(item.gameObject);
            }
        }

        List<Message> displayList = new List<Message>();

        if (searchTerm == "")
        {
            displayList = _messages;
        }
        else
        {
            displayList = _messages.Where(x => x.Text.ToLower().Contains(searchTerm.ToLower())).ToList();
        }

        float lastYPos = 300;
        foreach (var item in displayList.Skip(displayList.Count - 15).Take(15))
        {
            var box = CreateBox(item.Text, item.ModName, item.Type, item.DupCount);
            box.GetComponent<RectTransform>().localPosition = new Vector3(0, lastYPos - 35, 0);
            lastYPos -= 35;
            _boxList.Add(box);
        }
    }

    public void PostMessage(string message, string modName, MsgType type)
    {
        bool dup = false;
        foreach (Message msg in _messages)
        {
            if (msg.Text == message)
            {
                msg.DupCount += 1;
                dup = true;
            }
        }

        if (!dup)
        {
            _messages.Add(new Message { Text = message, ModName = modName, Type = type, DupCount = 0 });
        }

        DisplayMessages(_searchBox.text);
    }

    GameObject CreateBox(string text, string mod, MsgType type, int dupCount)
    {
        // Create dark message box
        GameObject box = new GameObject("OWConsoleMessageBox");
        box.transform.parent = _board.transform;

        var rt = box.AddComponent<RectTransform>();
        rt.sizeDelta = _boxSize;
        rt.localScale = Vector3.one;
        rt.localPosition = Vector3.zero;

        box.AddComponent<CanvasRenderer>();

        var ri = box.AddComponent<RawImage>();
        ri.color = new Color32(0, 0, 0, 90);
        ri.raycastTarget = false;

        // Create gameobject with text component
        GameObject words = new GameObject("Words");
        words.transform.parent = box.transform;

        var rt2 = words.AddComponent<RectTransform>();
        rt2.sizeDelta = new Vector2(_boxSize.x - 10, _boxSize.y);
        rt2.localScale = Vector3.one;
        rt2.localPosition = Vector3.zero;

        words.AddComponent<CanvasRenderer>();

        var textC = words.AddComponent<Text>();
        textC.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        textC.fontSize = _fontSize;
        textC.text = "[" + mod + "] : " + text;
        textC.alignment = TextAnchor.UpperLeft;

        if (type == MsgType.ERROR)
        {
            textC.color = Color.red;
        }

        if (type == MsgType.WARNING)
        {
            textC.color = Color.yellow;
        }

        return box;
    }

    public enum MsgType
    {
        ERROR,
        LOG,
        WARNING,
        CHAT
    }
}

class Message
{
    public string Text { get; set; }
    public string ModName { get; set; }
    public MsgType Type { get; set; }
    public int DupCount { get; set; }
}