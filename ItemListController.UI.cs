using System;

namespace ApproximatelyUpMod
{
    public partial class ItemListController
    {
        private UnityReflection.RectData _windowRect = new UnityReflection.RectData(20f, 20f, 480f, 620f);
        private object _itemScroll = UnityReflection.CreateVector2(0f, 0f);
        private bool _itemsExpanded = true;
        private bool _guiFailed;

        private void ToggleVisibility()
        {
            SetVisibility(!_isVisible, true);
        }

        private void SetVisibility(bool visible, bool preserveCursorState)
        {
            if (visible)
            {
                if (preserveCursorState)
                {
                    _prevLockMode = UnityReflection.GetCursorLockState();
                }

                _isVisible = true;
                UnityReflection.SetCursorVisible(true);
                UnityReflection.SetCursorLockStateNone();
                return;
            }

            _isVisible = false;
            UnityReflection.SetCursorVisible(false);
            UnityReflection.SetCursorLockState(_prevLockMode);
        }

        internal void DrawGui()
        {
            if (!_isVisible || _guiFailed)
            {
                return;
            }

            try
            {
                UnityReflection.GuiBox(_windowRect, "Approximately Up Items");
                UnityReflection.RectData contentRect = new UnityReflection.RectData(_windowRect.X + 10f, _windowRect.Y + 24f, _windowRect.Width - 20f, _windowRect.Height - 34f);
                UnityReflection.BeginArea(contentRect);
                DrawPanelContent();
                UnityReflection.EndArea();
            }
            catch (Exception ex)
            {
                _guiFailed = true;
                ModLog.Error("GUI disabled after draw failure: " + ex);
            }
        }

        private void DrawPanelContent()
        {
            UnityReflection.Label("Items available: " + _allItems.Count);
            if (!_cacheReady)
            {
                UnityReflection.Label("Loading item list...");
            }

            UnityReflection.Space(6f);
            UnityReflection.BeginHorizontal();
            if (UnityReflection.Button("Set 999", 110f))
            {
                ApplyMaterialsAmount(DefaultMaterialsAmount);
            }

            if (UnityReflection.Button("Set 99999", 110f))
            {
                ApplyMaterialsAmount(MaxMaterialsAmount);
            }
            UnityReflection.EndHorizontal();

            if (UnityReflection.Button("Max item amounts + fill hotbar"))
            {
                ApplyMaterialsAmount(MaxMaterialsAmount);
                UnlockAllItems();
            }

            UnityReflection.BeginHorizontal();
            if (UnityReflection.Button("Refresh list"))
            {
                TryRefreshItems(true);
            }

            if (UnityReflection.Button(_itemsExpanded ? "Hide items" : "Show items"))
            {
                _itemsExpanded = !_itemsExpanded;
            }
            UnityReflection.EndHorizontal();

            if (_itemsExpanded)
            {
                _itemScroll = UnityReflection.BeginScrollView(_itemScroll, 390f);
                if (_allItems.Count == 0)
                {
                    UnityReflection.Label("No items available yet.");
                }
                else
                {
                    for (int i = 0; i < _allItems.Count; i++)
                    {
                        ItemEntry entry = _allItems[i];
                        if (UnityReflection.Button(entry.Name))
                        {
                            AssignToFirstHotbar(entry);
                        }
                    }
                }
                UnityReflection.EndScrollView();
            }

            UnityReflection.Space(6f);
            if (UnityReflection.Button("Close"))
            {
                SetVisibility(false, false);
            }

            UnityReflection.Space(6f);
        }
    }
}
