using System;
using System.Collections.Generic;
using BestHTTP.Core;
using UnityEngine;
using UnityEngine.UI;

namespace BestHTTP.Examples.Helpers.Components
{
    public class Cache : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private Text _count;

        [SerializeField]
        private Text _size;

        [SerializeField]
        private Button _clear;
#pragma warning restore

        private void Start()
        {
            PluginEventHelper.OnEvent += OnPluginEvent;
            UpdateLabels();
        }

        private void OnDestroy()
        {
            PluginEventHelper.OnEvent -= OnPluginEvent;
        }

        private void OnPluginEvent(PluginEventInfo @event)
        {
            if (@event.Event == PluginEvents.SaveCacheLibrary)
                UpdateLabels();
        }

        private void UpdateLabels()
        {
            this._count.text = BestHTTP.Caching.HTTPCacheService.GetCacheEntityCount().ToString("N0");
            this._size.text = BestHTTP.Caching.HTTPCacheService.GetCacheSize().ToString("N0");
        }

        public void OnClearButtonClicked()
        {
            BestHTTP.Caching.HTTPCacheService.BeginClear();
        }
    }
}