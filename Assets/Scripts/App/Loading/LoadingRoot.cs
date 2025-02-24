using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Loading
{
    public class LoadingRoot : SingletonEvent<LoadingRoot>, IRoot
    {
        private LoadingWindow window;
        public LoadingRoot()
        {

        }
        public void Begin()
        {
            if (window == null)
            {
                //加载窗体
                window = AssetsManager.Instance.LoadUIWindow<LoadingWindow>(AssetsPathConfig.LoadingWindow);
            }
        }

        public void Loading(float progress, Action callback)
        {
            if (!window.GetWindowActive())
            {
                window.SetWindowActive(true);
            }
            window.SetLoadingSliderValue(progress);
            if (progress >= 1)
            {
                window.SetWindowActive(false);
                callback?.Invoke();
            }
        }
        public void End()
        {
            window.SetWindowActive(false);
        }
        public void AppPause(bool pause)
        {
            
        }
        public void AppFocus(bool focus)
        {
            
        }
        public void AppQuit()
        {

        }
    }
}