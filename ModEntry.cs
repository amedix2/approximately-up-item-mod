using MelonLoader;
using System;

[assembly: MelonInfo(typeof(ApproximatelyUpMod.ModEntry), "ApproximatelyUpMod", "2.5.0-items-only", "discord: dmtftf / local fix")]
[assembly: MelonGame(null, null)]

namespace ApproximatelyUpMod
{
    internal static class ModLog
    {
        internal const string Prefix = "[ApproximatelyUpMod]";

        internal static void Info(string message)
        {
            MelonLogger.Msg(Prefix + " " + message);
        }

        internal static void Warn(string message)
        {
            MelonLogger.Warning(Prefix + " " + message);
        }

        internal static void Error(string message)
        {
            MelonLogger.Error(Prefix + " " + message);
        }
    }

    public class ModEntry : MelonMod
    {
        private ItemListController _controller;

        public override void OnInitializeMelon()
        {
            try
            {
                EnsureController();
                ModLog.Info("Items-only mod initialized.");
            }
            catch (Exception ex)
            {
                ModLog.Error("Critical initialization error: " + ex);
            }
        }

        public override void OnUpdate()
        {
            ItemListController controller = EnsureController();
            if (controller != null)
            {
                controller.Tick();
            }
        }

        public override void OnGUI()
        {
            ItemListController controller = EnsureController();
            if (controller != null)
            {
                controller.DrawGui();
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            try
            {
                ModLog.Info("Scene loaded: " + sceneName + " (build " + buildIndex + ").");
                ItemListController controller = EnsureController();
                if (controller != null)
                {
                    controller.NotifySceneLoaded(sceneName);
                }
            }
            catch (Exception ex)
            {
                ModLog.Error("OnSceneWasLoaded failed: " + ex);
            }
        }

        private ItemListController EnsureController()
        {
            if (_controller == null)
            {
                _controller = new ItemListController();
                _controller.Initialize();
            }

            return _controller;
        }
    }
}
