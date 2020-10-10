﻿using Data;
using EventController;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hotfix
{
    public class HotfixRoot : SingletonEvent<HotfixRoot>, IRoot
    {
        private HotfixWindow hotfixWin;
        public HotfixRoot()
        {
            AddEvent();

            string prefab_HotfixPath = "App/Hotfix/Windows/HotfixWindow";
            hotfixWin = (HotfixWindow)UIRoot.Instance.LoadLocalWindow(prefab_HotfixPath, GetType().Namespace, true);

            StartHotfix();
        }
        private void AddEvent()
        {
            AddEventMsg("UpdateNow", () => { UpdateNow(); }, true);
        }
        private void StartHotfix()
        {
            hotfixWin.SetTipsText("检查更新中...");
            HotfixManager.Instance.StartHotfix((bool isUpdate, string des) =>
            {
                if (isUpdate)
                {
                    hotfixWin.SetContentText(des);
                    hotfixWin.SetUpdateTipsActive(true);
                }
                else
                {
                    LoadAssetBundle();
                }
            });
        }
        private void UpdateNow()
        {
            hotfixWin.SetUpdateTipsActive(false);
            hotfixWin.SetTipsText("下载中...");
            HotfixManager.Instance.DownLoadAssetBundle();
            App.app.StartCoroutine(DownLoading());

        }
        private IEnumerator DownLoading()
        {
            float time = 0;
            float previousSize = 0;
            float speed = 0;
            while (HotfixManager.Instance.GetProgress() != 1)
            {
                yield return new WaitForEndOfFrame();
                if (time == 0)
                {
                    previousSize = HotfixManager.Instance.GetLoadedSize();
                }
                if (time > 0.1f)
                {
                    time = 0;
                    speed = (HotfixManager.Instance.GetLoadedSize() - previousSize) / 0.1f;
                }
                time += Time.deltaTime;
                hotfixWin.SetSpeedText(speed);
                hotfixWin.SetProgressText(HotfixManager.Instance.GetLoadedSize(), HotfixManager.Instance.LoadTotalSize);
                hotfixWin.SetProgressValue(HotfixManager.Instance.GetProgress());
            }
            //更新下载完成，加载AB包
            LoadAssetBundle();
        }
        private void LoadAssetBundle()
        {
            hotfixWin.SetTipsText("正在加载资源...");
            hotfixWin.SetProgressBarActive(true);
            HotfixManager.Instance.LoadAssetBundle((bool isEnd, string bundleName, float bundleProgress) =>
            {
                hotfixWin.SetTipsText(string.Format("正在加载资源:{0}资源包...", bundleName));
                if (isEnd && bundleProgress == 1)
                {
                    hotfixWin.SetProgressBarActive(false);
                    Debug.Log("加载完成");
                    //AB包加载完成
                    Root.InitRootScripts();
                }
            });
        }

        public void End()
        {

        }
    }
}