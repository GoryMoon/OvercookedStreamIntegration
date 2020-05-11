using LitJson;

namespace StreamIntegration.Actions
{
    public class DropAction: IAction
    {
        public string Name => "drop_item";
        
        public void Handle(JsonData data)
        {
            Utils.ForEachPlayer(controls =>
            {
                IPlayerCarrier carrier;
                if (Utils.CheckItem(controls, out carrier))
                {
                    PlayerControlsHelper.DropHeldItem(controls, controls.transform.forward.XZ());
                }
            }, controls =>
            {
                IPlayerCarrier carrier;
                if (Utils.CheckItem(controls, out carrier))
                {
                    PlayerControlsHelper.PlaceHeldItem_Client(controls);
                }
            });
        }

        public bool CanRun(GameState state)
        {
            return state == GameState.InLevel;
        }
    }
}