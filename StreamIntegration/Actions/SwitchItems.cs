using System;
using System.Linq;
using LitJson;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StreamIntegration.Actions
{
    public class SwitchItems: IAction
    {
        public string Name => "switch_items";
        public void Handle(JsonData data)
        {
            var players = Object.FindObjectsOfType<PlayerControls>().ToList();
            var switches = (int) Math.Ceiling(players.Count / 2d);
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

                    var playerCarrier = Utils.GetCarrier(controls);
                    var switchCarrier = Utils.GetCarrier(first);

                    var playerItem = playerCarrier.InspectCarriedItem() != null ? playerCarrier.TakeItem(): null;
                    var switchItem = switchCarrier.InspectCarriedItem() != null ? switchCarrier.TakeItem(): null;

                    if (playerItem != null)
                    {
                        switchCarrier.CarryItem(playerItem);
                    }
                    if (switchItem != null)
                    {
                        playerCarrier.CarryItem(switchItem);
                    }
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