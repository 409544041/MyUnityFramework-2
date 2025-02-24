﻿using System;
using System.Collections.Generic;
using UnityEngine;
using XLua;
/// <summary>
/// 在Xlua中使用DoTween
/// </summary>
public static class DOTweenCustomExport
{
    [LuaCallCSharp]
    [ReflectionUse]
    public static List<Type> luaCallCsharpList = new List<Type>(){
        typeof(DG.Tweening.AutoPlay),
        typeof(DG.Tweening.AxisConstraint),
        typeof(DG.Tweening.Ease),
        typeof(DG.Tweening.LogBehaviour),
        typeof(DG.Tweening.LoopType),
        typeof(DG.Tweening.PathMode),
        typeof(DG.Tweening.PathType),
        typeof(DG.Tweening.RotateMode),
        typeof(DG.Tweening.ScrambleMode),
        typeof(DG.Tweening.TweenType),
        typeof(DG.Tweening.UpdateType),

        typeof(DG.Tweening.DOTween),
        typeof(DG.Tweening.DOVirtual),
        typeof(DG.Tweening.EaseFactory),
        typeof(DG.Tweening.Tweener),
        typeof(DG.Tweening.Tween),
        typeof(DG.Tweening.Sequence),
        typeof(DG.Tweening.TweenParams),
        typeof(DG.Tweening.Core.ABSSequentiable),

        typeof(DG.Tweening.Core.TweenerCore<Vector3, Vector3, DG.Tweening.Plugins.Options.VectorOptions>),

        typeof(DG.Tweening.TweenCallback),
        typeof(DG.Tweening.TweenExtensions),
        typeof(DG.Tweening.TweenSettingsExtensions),
        typeof(DG.Tweening.ShortcutExtensions),
        typeof(DG.Tweening.DOTweenModuleUI),
        typeof(DG.Tweening.DOTweenCYInstruction),
        typeof(DG.Tweening.DOTweenModuleUnityVersion),
        typeof(DG.Tweening.DOTweenModuleSprite),
        typeof(DG.Tweening.DOTweenModuleUtils),
        typeof(DG.Tweening.DOTweenModulePhysics2D),
        typeof(DG.Tweening.DOTweenModulePhysics),
        typeof(DG.Tweening.DOTweenModuleAudio)
    };
}