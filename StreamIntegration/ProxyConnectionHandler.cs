using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace StreamIntegration
{
    public static class ProxyConnectionHandler
    {
        private static readonly CancellationTokenSource Source = new CancellationTokenSource();

        //Tells the connection to shut down
        public static void Shutdown() {
            Source.Cancel();
        }
        public static void StartConnection() {
            
            Main.Logger.Log("Starting Integration Connection");
            var token = Source.Token;
            
            if (!token.IsCancellationRequested)
            {
                //Starts the connection task on a new thread
                Task.Factory.StartNew(() =>
                {
                    //Catch any errors
                    try
                    {
                        using (var ws = new WebSocket("ws://localhost:56214/"))
                        {
                            ws.OnMessage += (sender, e) =>
                            {
                                var line = e.Data;
                                if (line != null)
                                {
                                    if (line.StartsWith("Action: "))
                                    {
                                        var action = line.Substring(8);
                                        ActionManager.ActionQueue.Enqueue(action);

                                    }
                                    else if (line.StartsWith("Message: "))
                                    {
                                        var message = line.Substring(9);
                                        ActionManager.MessageQueue.Enqueue(message);
                                    }
                                }
                            };
                            ws.Connect();
                            while (!token.IsCancellationRequested)
                            {
                                //Keep trying to read
                                while (!token.IsCancellationRequested && ws.IsAlive)
                                {
                                    //Only read every 50ms
                                    Thread.Sleep(50);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Main.Logger.LogException(e);
                    }
                }, token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
            }
        }
    }
}