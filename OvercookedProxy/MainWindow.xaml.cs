using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using OvercookedProxy.Annotations;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace OvercookedProxy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public sealed partial class MainWindow: INotifyPropertyChanged
    {
        private static readonly CancellationTokenSource Source = new CancellationTokenSource();
        private readonly WebSocketServer _webSocketServer;
        private readonly ConcurrentQueue<string> _proxyQueue = new ConcurrentQueue<string>();

        private const string Connected = "Connected";
        private const string Disconnected = "Disconnected";
        public string IntegrationStatusText => $"Integration: {(_integrationConnected ? Connected: Disconnected)}";
        private bool _integrationConnected;
        public bool IntegrationConnected
        {
            get => _integrationConnected;
            private set
            {
                _integrationConnected = value;
                OnPropertyChanged(nameof(IntegrationConnected));
                OnPropertyChanged(nameof(IntegrationStatusText));
            }
        }

        public string SocketStatusText => $"Game: {(SocketConnected ? Connected: Disconnected)}";
        public bool SocketConnected => _webSocketServer.WebSocketServices.SessionCount > 0;


        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            _webSocketServer = new WebSocketServer(56214);
            _webSocketServer.Start();
            _webSocketServer.AddWebSocketService("/", () =>
            {
                OnPropertyChanged(nameof(SocketConnected));
                OnPropertyChanged(nameof(SocketStatusText));
                return new RelayBehavior(this);
            });
            StartConnection();
        }

        public void Deconstruct()
        {
            _webSocketServer.Stop();
            Source.Cancel();
        }

        private void StartConnection()
        {
            var token = Source.Token;

            if (!token.IsCancellationRequested)
            {
                //Starts the connection task on a new thread
                Task.Factory.StartNew(() =>
                {
                    //Keep making new pipes
                    while (!token.IsCancellationRequested)
                    {
                        //Catch any errors
                        try
                        {
                            //pipeName is the same as your subfolder name in the Integrations folder of the app
                            using (var client = new NamedPipeClientStream(".", "Overcooked2", PipeDirection.In))
                            {
                                using (var reader = new StreamReader(client))
                                {
                                    //Keep trying to connect
                                    while (!token.IsCancellationRequested && !client.IsConnected)
                                    {
                                        try
                                        {
                                            client.Connect();
                                            IntegrationConnected = true;

                                        }
                                        catch (TimeoutException e)
                                        {
                                            Console.Error.WriteLine(e);
                                            //Ignore
                                        }
                                        catch (Win32Exception e)
                                        {
                                            Console.Error.WriteLine(e);
                                            //Ignore and sleep for a bit, since the connection didn't time out
                                            Thread.Sleep(500);
                                        }
                                        catch (Exception e)
                                        {
                                            Console.Error.WriteLine(e);
                                            //Ignore and sleep for a bit, since the connection didn't time out
                                            Thread.Sleep(500);
                                        }
                                    }

                                    //Keep trying to read
                                    while (!token.IsCancellationRequested && client.IsConnected && !reader.EndOfStream)
                                    {

                                        //Read line from stream
                                        var line = reader.ReadLine();
                                        if (line != null)
                                        {
                                            if (SocketConnected)
                                            {
                                                _webSocketServer.WebSocketServices.BroadcastAsync(line, null);
                                            }
                                            else
                                            {
                                                _proxyQueue.Enqueue(line);
                                            }
                                        }

                                        //Only read every 50ms
                                        Thread.Sleep(50);
                                    }

                                    IntegrationConnected = false;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            //Ignore
                        }
                    }
                }, token);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UpdateSocketInfo()
        {
            OnPropertyChanged(nameof(SocketConnected));
            OnPropertyChanged(nameof(SocketStatusText));
            if (SocketConnected)
            {
                while (_proxyQueue.TryDequeue(out var message))
                {
                    _webSocketServer.WebSocketServices.BroadcastAsync(message, null);
                }
            }
        }
    }

    public class RelayBehavior : WebSocketBehavior
    {
        private readonly MainWindow _mainWindow;

        public RelayBehavior(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        protected override void OnClose(CloseEventArgs e)
        {
            _mainWindow.UpdateSocketInfo();
        }

        protected override void OnOpen()
        {
            _mainWindow.UpdateSocketInfo();
        }
    }

    public class BrushColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && (bool) value)
            {
                return new SolidColorBrush(Colors.Green);
            }
            return new SolidColorBrush(Colors.Red);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
    
}