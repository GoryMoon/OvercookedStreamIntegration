using LitJson;
using Team17.Online.Multiplayer.Messaging;

namespace StreamIntegration.Actions
{
    public class ThrowItem: IAction
    {
        public string Name => "throw_item";
        public void Handle(JsonData data)
        {
            Utils.ForEachPlayer(Throw, Throw);
        }

        private static void Throw(PlayerControls controls)
        {
            IPlayerCarrier carrier;
            if (Utils.CheckItem(controls, out carrier))
            {
                AccessUtils.ChefEventMessage(ChefEventMessage.ChefEventType.Throw, controls.gameObject, carrier.InspectCarriedItem());   
            }
        } 
        
        public bool CanRun(GameState state)
        {
            return state == GameState.InLevel;
        }
    }
}