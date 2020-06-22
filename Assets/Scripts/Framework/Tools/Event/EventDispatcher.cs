﻿using System;
namespace EventController {
    public class EventDispatcher {
        private static EventController m_eventController = new EventController();


        //判断是否有该监听
        public static bool HasEventListener(string eventType,Delegate handler) {
            return m_eventController.HasEventListener(eventType,handler);
        }


        #region 注册消息
        //注册参数类型不同的消息调用;  
        public static void AddEventListener(string eventType,Action handler,bool isDesignateRemove) {
            m_eventController.AddEventListener(eventType,handler,isDesignateRemove);
        }

        public static void AddEventListener<T>(string eventType,Action<T> handler,bool isDesignateRemove) {
            m_eventController.AddEventListener(eventType,handler,isDesignateRemove);
        }

        public static void AddEventListener<T, U>(string eventType,Action<T,U> handler,bool isDesignateRemove) {
            m_eventController.AddEventListener(eventType,handler,isDesignateRemove);
        }

        public static void AddEventListener<T, U, V>(string eventType,Action<T,U,V> handler,bool isDesignateRemove) {
            m_eventController.AddEventListener(eventType,handler,isDesignateRemove);
        }

        public static void AddEventListener<T, U, V, W>(string eventType,Action<T,U,V,W> handler,bool isDesignateRemove) {
            m_eventController.AddEventListener(eventType,handler,isDesignateRemove);
        }
        #endregion

        #region 触发事件
        //触发某个事件
        public static void TriggerEvent(string eventType) {
            m_eventController.DispatchEvent(eventType);
        }

        public static void TriggerEvent<T>(string eventType,T arg1) {
            m_eventController.DispatchEvent<T>(eventType,arg1);
        }

        public static void TriggerEvent<T, U>(string eventType,T arg1,U arg2) {
            m_eventController.DispatchEvent<T,U>(eventType,arg1,arg2);
        }

        public static void TriggerEvent<T, U, V>(string eventType,T arg1,U arg2,V arg3) {
            m_eventController.DispatchEvent<T,U,V>(eventType,arg1,arg2,arg3);
        }

        public static void TriggerEvent<T, U, V, W>(string eventType,T arg1,U arg2,V arg3,W arg4) {
            m_eventController.DispatchEvent<T,U,V,W>(eventType,arg1,arg2,arg3,arg4);
        }
        #endregion

        #region 移除事件
        //删除所有事件的所有监听
        public static void RemoveAllEventListeners() {
            m_eventController.RemoveAllEventListeners();
        }
        //移除指定的事件
        public static void RemoveDesignateEvent() {
            m_eventController.RemoveDesignateEvent();
        }
        //删除某个事件的一个监听
         public static void RemoveEventListener(string eventType) {
            m_eventController.RemoveEventListener(eventType);
        }

        public static bool RemoveEventListener(string eventType,Action handler) {
            return m_eventController.RemoveEventListener(eventType,handler);
        }

        public static bool RemoveEventListener<T>(string eventType,Action<T> handler) {
            return m_eventController.RemoveEventListener(eventType,handler);
        }

        public static bool RemoveEventListener<T, U>(string eventType,Action<T,U> handler) {
            return m_eventController.RemoveEventListener(eventType,handler);
        }

        public static bool RemoveEventListener<T, U, V>(string eventType,Action<T,U,V> handler) {
            return m_eventController.RemoveEventListener(eventType,handler);
        }

        public static bool RemoveEventListener<T, U, V, W>(string eventType,Action<T,U,V,W> handler) {
            return m_eventController.RemoveEventListener(eventType,handler);
        }
        #endregion
    }
}

