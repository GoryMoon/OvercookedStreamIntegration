using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using LitJson;
using StreamIntegration.Actions;

namespace StreamIntegration
{
    public class ActionManager
    {
        public static readonly ConcurrentQueue<string> MessageQueue = new ConcurrentQueue<string>();
        public static readonly ConcurrentQueue<string> ActionQueue = new ConcurrentQueue<string>();
        
        private readonly Dictionary<string, IAction> _actions = new Dictionary<string, IAction>();

        public ActionManager()
        {
            AddAction(new DropAction());
            AddAction(new ThrowItem());
            AddAction(new MoveAction());
            AddAction(new SwitchLocation());
            AddAction(new SwitchItems());
            
            JsonMapper.RegisterExporter<float>((o, writer) => writer.Write(Convert.ToDouble(o)));
            JsonMapper.RegisterImporter<double, float>(Convert.ToSingle);
        }

        private void AddAction(IAction action)
        {
            _actions.Add(action.Name, action);
        }
        
        public void Update()
        {
            if (!Main.Enabled)
            {
                return;
            }

            if (!AccessUtils.InGame())
            {
                return;
            }
            
            string message;
            if (MessageQueue.TryDequeue(out message))
            {
                Main.Logger.Log($"Message: {message}");
                MessageDisplay.Messages.Enqueue(Message.Create(message));
            }

            string ac;
            if (ActionQueue.TryDequeue(out ac))
            {
                Main.Logger.Log($"Action: {ac}");

                var data = JsonMapper.ToObject(ac);
                IAction action;
                if (_actions.TryGetValue(data["type"].ToString(), out action))
                {
                    if (action.CanRun(AccessUtils.GameState))
                    {
                        action.Handle(data);
                    }
                    else
                    {
                        ActionQueue.Enqueue(ac);
                    }
                }
            }
        }

        public static void GameStateChange(GameState state)
        {
            if (state == GameState.InMap || state == GameState.InLevel)
            {
                MessageDisplay.Load();
            }
        }
    }
}