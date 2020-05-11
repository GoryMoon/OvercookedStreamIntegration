using System;
using System.Linq;
using LitJson;
using Object = UnityEngine.Object;

namespace StreamIntegration.Actions
{
    public class SwitchLocation: IAction
    {
        public string Name => "switch_location";
        public void Handle(JsonData data)
        {
            var players = Object.FindObjectsOfType<PlayerControls>().ToList();
            var switches = (int) Math.Ceiling(players.Count / 2d);
            Main.Logger.Log($"Host: {AccessUtils.IsHost()}");
            Action<PlayerControls> action = controls =>
            {
                if (players.Count > switches)
                {
                    var first = players.First(c => c != controls);
                    players.Remove(controls);
                    if (switches >= 2)
                    {
                        players.Remove(first);
                    }

                    var playerTransform = controls.transform;
                    var switchTransform = first.transform;
                    
                    var tmp = playerTransform.position;
                    playerTransform.position = switchTransform.position;
                    switchTransform.position = tmp;
                }
            };
            Utils.ForEachPlayer(action, action);
        }
        
        public bool CanRun(GameState state)
        {
            return state == GameState.InLevel;
        }
    }
}