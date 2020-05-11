using System;
using LitJson;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace StreamIntegration.Actions
{
    public class MoveAction: IAction
    {
        public string Name => "move";
        public void Handle(JsonData data)
        {
            if (AccessUtils.InMap())
            {
                var min = Convert.ToSingle((double) data["vanMin"]);
                var max = Convert.ToSingle((double) data["vanMax"]);
                var van = Object.FindObjectOfType<MapAvatarControls>();
                Move(van, min, max);
            }
            else
            {
                var min = Convert.ToSingle((double) data["playerMin"]);
                var max = Convert.ToSingle((double) data["playerMax"]);
                Action<PlayerControls> action = controls => Move(controls, min, max);
                Utils.ForEachPlayer(action, action);
            }
        }

        public bool CanRun(GameState state)
        {
            return state == GameState.InLevel || state == GameState.InMap;
        }

        private static void Move(Component controls, float min, float max)
        {
            controls.transform.position += GetBoundedRandVector(min, max);
        }
        
        private static Vector3 GetBoundedRandVector(float min, float max)
        {
            var x = Random.value > 0.5 ? Random.Range(-max, -min) : Random.Range(min, max);
            var z = Random.value > 0.5 ? Random.Range(-max, -min) : Random.Range(min, max);
            return new Vector3(x, 0, z);
        }
    }
}