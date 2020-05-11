using Harmony12;
using Team17.Online;
using Team17.Online.Multiplayer.Messaging;
using UnityEngine;

namespace StreamIntegration
{
    public static class AccessUtils
    {
        private static FastInvokeHandler _isHost;
        private static FastInvokeHandler _getTextStyle;
        private static FastInvokeHandler _chefEventMessage;

        private static GameState _gameState;

        public static GameState GameState => _gameState;

        public static void Init()
        {
            _isHost = MethodInvoker.GetHandler(AccessTools.Method("ConnectionStatus:IsHost"));
            _getTextStyle = MethodInvoker.GetHandler(AccessTools.Method("GUIUtils:GetTextStyle", new []{ typeof(Rect), typeof(TextAnchor), typeof(Font), typeof(string)}));
            
            var mailbox = AccessTools.TypeByName("Mailbox");
            Traverse
                .Create(mailbox)
                .Field("Client")
                .Method("RegisterForMessageType", new [] {typeof(MessageType), typeof(OrderedMessageReceivedCallback)})
                .GetValue(MessageType.GameState, new OrderedMessageReceivedCallback(OnGameStateChanged));
            
            _chefEventMessage = MethodInvoker.GetHandler(AccessTools.Method("ClientMessenger:ChefEventMessage", new []{ typeof(ChefEventMessage.ChefEventType), typeof(GameObject), typeof(GameObject)}));
        }

        private static void OnGameStateChanged(IOnlineMultiplayerSessionUserId sessionUserId, Serialisable message)
        {
            _gameState = ((GameStateMessage) message).m_State;
            Main.Logger.Log($"GameState: {_gameState}");
            ActionManager.GameStateChange(_gameState);
        }

        public static bool InGame()
        {
            return InLevel() || _gameState == GameState.InMap;
        }
        
        public static bool InLevel()
        {
            return _gameState == GameState.InLevel;
        }

        public static bool InMap()
        {
            return _gameState == GameState.InMap;
        }
        
        public static bool IsHost()
        {
            return (bool) _isHost.Invoke(null, null);
        }

        public static GUIStyle GetTextStyle(Rect size, TextAnchor alignment, Font font, string message)
        {
            return (GUIStyle) _getTextStyle?.Invoke(null, new object[] {size, alignment, font, message});
        }
        
        public static void ChefEventMessage(ChefEventMessage.ChefEventType type, GameObject chef, GameObject @object)
        {
            _chefEventMessage.Invoke(null, new object[] {type, chef, @object});
        }
    }
}