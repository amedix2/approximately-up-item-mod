using System;
using System.Collections.Generic;

namespace ApproximatelyUpMod
{
    public partial class ItemListController
    {
        internal struct ItemEntry
        {
            internal object Component;
            internal string Name;
        }

        private const int ToggleKey = 291; // UnityEngine.KeyCode.F10
        private const int FillHotbarKey = 290; // UnityEngine.KeyCode.F9
        private const bool ShowGuiOnStartup = true;
        private const double StartupShowRetryDelaySeconds = 0.5;
        internal const int MaxMaterialsAmount = 99999;
        internal const int DefaultMaterialsAmount = 999;

        private static readonly List<ItemEntry> _allItems = new List<ItemEntry>(512);

        private static ItemListController _activeInstance;
        private static bool _cacheReady;

        public static int MaterialsAmountOverride = DefaultMaterialsAmount;
        public static bool EnforceMaterialsAmount;

        private bool _isVisible;
        private bool _startupShowPending;
        private string _lastSceneName = string.Empty;

        private double _nextRefreshAt;
        private double _nextStartupShowAttemptAt;

        private int _itemsRevision;
        private int _hotbarPage;

        private object _prevLockMode;

        internal static ItemListController ActiveInstance
        {
            get { return _activeInstance; }
        }

        internal void Initialize()
        {
            _activeInstance = this;
            _startupShowPending = ShowGuiOnStartup;

            ModLog.Info("ItemListController started.");
            TryRefreshItems(true);
        }

        internal void Tick()
        {
            if (UnityReflection.GetKeyDown(ToggleKey))
            {
                ModLog.Info("Toggle key pressed (F10).");
                ToggleVisibility();
            }

            if (UnityReflection.GetKeyDown(FillHotbarKey))
            {
                ModLog.Info("Fill hotbar key pressed (F9).");
                ApplyMaterialsAmount(MaxMaterialsAmount);
                UnlockAllItems();
            }

            if (UnityReflection.RealtimeSinceStartup >= _nextRefreshAt)
            {
                TryRefreshItems(false);
            }

            if (_startupShowPending && UnityReflection.RealtimeSinceStartup >= _nextStartupShowAttemptAt)
            {
                TryShowGuiOnStartup();
            }

            if (_isVisible)
            {
                UnityReflection.SetCursorVisible(true);
                UnityReflection.SetCursorLockStateNone();
            }
        }

        internal void NotifySceneLoaded(string sceneName)
        {
            _lastSceneName = sceneName ?? string.Empty;
            _startupShowPending = ShowGuiOnStartup;
            _nextStartupShowAttemptAt = UnityReflection.RealtimeSinceStartup + StartupShowRetryDelaySeconds;

            ModLog.Info("NotifySceneLoaded -> scheduling GUI activation for scene: " + _lastSceneName);
        }

        private void TryShowGuiOnStartup()
        {
            if (!_isVisible)
            {
                SetVisibility(true, true);
                ModLog.Info("GUI activated automatically in scene '" + _lastSceneName + "'.");
            }

            _startupShowPending = false;
        }
    }
}
