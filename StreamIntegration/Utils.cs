using System;
using Object = UnityEngine.Object;

namespace StreamIntegration
{
    public static class Utils
    {
        public static void ForEachPlayer(Action<PlayerControls> host, Action<PlayerControls> remote)
        {
            var controls = Object.FindObjectsOfType<PlayerControls>();
            if (controls != null && controls.Length > 0)
            {
                foreach (var control in controls)
                {
                    if (AccessUtils.IsHost())
                    {
                        host?.Invoke(control);
                    }
                    else
                    {
                        remote?.Invoke(control);
                    }
                }
            }
        }

        public static bool CheckItem(PlayerControls controls, out IPlayerCarrier carrier)
        {
            carrier = GetCarrier(controls);
            return carrier.InspectCarriedItem() != null;
        }

        public static IPlayerCarrier GetCarrier(PlayerControls controls)
        {
            return controls.gameObject.RequireInterface<IPlayerCarrier>();;
        }
    }
}