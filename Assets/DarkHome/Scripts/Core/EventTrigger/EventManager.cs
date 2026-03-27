/*
* Observer Pattern (chỉ cho hàm void)
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace DarkHome
{
    public class EventManager
    {
        private static Dictionary<string, Delegate> _listeners = new();

        public static void AddObserver(string name, Action callback)
        {
            if (!_listeners.ContainsKey(name))
            {
                _listeners.Add(name, callback);
            }
            else
            {
                _listeners[name] = Delegate.Combine(_listeners[name], callback);
            }
        }
        public static void AddObserver<T>(string name, Action<T> callback)
        {
            if (!_listeners.ContainsKey(name))
            {
                _listeners.Add(name, callback);
            }
            else
            {
                _listeners[name] = Delegate.Combine(_listeners[name], callback);
            }
        }

        public static void RemoveListener(string name, Action callback)
        {
            if (_listeners.ContainsKey(name))
            {
                _listeners[name] = Delegate.Remove(_listeners[name], callback);
                if (_listeners[name] == null)
                {
                    _listeners.Remove(name);
                }
            }
        }
        public static void RemoveListener<T>(string name, Action<T> callback)
        {
            if (_listeners.ContainsKey(name))
            {
                _listeners[name] = Delegate.Remove(_listeners[name], callback);
                if (_listeners[name] == null)
                {
                    _listeners.Remove(name);
                }
            }
        }

        public static void Notify(string name)
        {
            if (_listeners.TryGetValue(name, out var del))
            {
                (del as Action)?.Invoke();
            }
        }
        public static void Notify<T>(string name, T data)
        {
            if (_listeners.TryGetValue(name, out var del))
            {
                (del as Action<T>)?.Invoke(data);
            }
        }

    }
}