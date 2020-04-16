using OWML.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChatHandler : MonoBehaviour
{
    List<GameObject> _boxList = new List<GameObject>();

    public GameObject _board;

    public IModHelper helper;

    public bool _shown;
    public bool _collapsed;

    public Vector2 _boxSize = new Vector2(790, 30);
    public int _fontSize = 12;

    void Awake()
    {
        _board = new GameObject("Board");
        _board.transform.parent = gameObject.transform;

        var rt = _board.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(800, -680);
        rt.localPosition = new Vector3(410, -332, 0);
        rt.anchorMax = new Vector2(0, 1);
        rt.anchorMin = new Vector2(0, 0);
        rt.pivot = new Vector2(.5f, .5f);
        rt.localScale = Vector3.one;

        Application.logMessageReceived += this.OnLogMessageReceived;

        _board.AddComponent<CanvasRenderer>();
        var ri = _board.AddComponent<RawImage>();
        ri.color = new Color32(0, 0, 0, 100);

        SceneManager.sceneLoaded += SceneLoad;
    }

    void SceneLoad(Scene scene, LoadSceneMode mode)
    {
        ResetLog();
    }

    //public void LogOWMLMod(string s)
    //{
    //    PostMessage(s, "OWML", MsgType.Log);
    //}

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

    public void ResetLog()
    {
        foreach (var item in _boxList)
        {
            GameObject.Destroy(item);
        }
        _boxList = new List<GameObject>();
        foreach (var item in GameObject.FindObjectsOfType<RawImage>())
        {
            if (item.gameObject.name == "OWConsoleMessageBox")
            {
                GameObject.Destroy(item.gameObject);
            }
        }
    }

    public void PostMessage(string message, string modName, MsgType type)
    {
        var box = CreateBox(message, modName, type);

        if (_boxList.Count != 0)
        {
            GameObject latestBox = _boxList.Last(); // Get latest message
            bool duplicate = false;
            GameObject duplicatedObject = null;

            foreach (var item in _boxList)
            {
                if (box.GetComponentInChildren<Text>().text == item.GetComponentInChildren<Text>().text)
                {
                    duplicate = true;
                    duplicatedObject = item;
                }
            }

            if (_collapsed && duplicate)
            {
                // Collapsed and a duplicate message.
                var wordsObject = duplicatedObject.GetComponentInChildren<Text>().gameObject; // Gets the "Words" object.

                if (wordsObject.transform.childCount == 0)
                {
                    GameObject dupCount = new GameObject("DuplicateCount");
                    dupCount.transform.parent = wordsObject.transform;

                    var textC = dupCount.AddComponent<Text>();
                    textC.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                    textC.fontSize = _fontSize + 5;
                    textC.text = "2";
                    textC.alignment = TextAnchor.MiddleRight;
                    dupCount.transform.localPosition = new Vector3(300, 0, 0);
                    dupCount.transform.localScale = Vector3.one;
                }
                else
                {
                    wordsObject.transform.Find("DuplicateCount").GetComponent<Text>().text = (Int32.Parse(wordsObject.transform.Find("DuplicateCount").GetComponent<Text>().text) + 1).ToString();
                }

                GameObject.Destroy(box);
            }
            else if (!_collapsed || !duplicate)
            {
                // Not collapsed or not a duplicate message.
                if ((latestBox.GetComponent<RectTransform>().localPosition.y - 35) < -180) // If message goes off bottom of board...
                {
                    _boxList.RemoveAt(0); // Delete topmost message.
                    GameObject.Destroy(_boxList[0]);
                    foreach (var item in _boxList) // Shift every message up.
                    {
                        item.GetComponent<RectTransform>().localPosition = new Vector3(0, item.GetComponent<RectTransform>().localPosition.y + 35, 0);
                    }
                }

                box.GetComponent<RectTransform>().localPosition = new Vector3(0, latestBox.GetComponent<RectTransform>().localPosition.y - 35, 0); // Add message below last message.
                _boxList.Add(box);
            }
        }
        else
        {
            box.GetComponent<RectTransform>().localPosition = new Vector3(0, 180, 0);
            _boxList.Add(box);
        }
    }

    GameObject CreateBox(string text, string mod, MsgType type)
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
        textC.text = "[" + mod + "] " + text;
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
