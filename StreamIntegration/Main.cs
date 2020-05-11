using System.Reflection;
using Harmony12;
using UnityModManagerNet;

namespace StreamIntegration
{
    public static class Main
    {
        public static bool Enabled;
        public static UnityModManager.ModEntry.ModLogger Logger;
        private static ActionManager _actionManager;

        static void Load(UnityModManager.ModEntry entry)
        {
            var harmony = HarmonyInstance.Create(entry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            entry.OnToggle += (modEntry, b) => Enabled = b;
            entry.OnUpdate += Update;
            Enabled = entry.Enabled;
            Logger = entry.Logger;
            AccessUtils.Init();
            
            _actionManager = new ActionManager();
            ProxyConnectionHandler.StartConnection();
        }

        private static void Update(UnityModManager.ModEntry entry, float deltaTime)
        {
            _actionManager.Update();
        }
    }
}