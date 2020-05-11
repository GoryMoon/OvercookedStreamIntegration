using LitJson;

namespace StreamIntegration.Actions
{
    public interface IAction
    {
        string Name { get; }
        
        void Handle(JsonData data);

        bool CanRun(GameState state);
    }
}