﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Platform
{
    public class EditorPlayer : PlatformManager
    {
        public override bool IsEditor { get; } = true;
        public override string Name { get; } = "Editor";
        public override string GetDataPath(string folder)
        {
            return string.Format("{0}/{1}", Application.persistentDataPath, folder);
        }
        public override void SavePhoto(string folder, string fileName)
        {
            Debug.Log("SavePhoto");
        }
        public override void QuitUnityPlayer(bool isStay = false)
        {
            Debug.Log("Quit Editor");
        }
        public override string GetAppData(string key)
        {
            return "";
        }
    }
}
