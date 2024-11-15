using System.Threading.Tasks;
using Godot;
using Google.Protobuf;
using IoGame.Gen;
using IoGame.Sdk;
using IoGame.Sdk.Godot;
using Pb.Common;

namespace My.Game
{
    public abstract class MyNetConfig
    {
        private static IoGameGodotWebSocket _socket;
        public static long CurrentTimeMillis { get; set; }

        public static void StartNet()
        {
            // biz code init
            GameCode.Init();
            Index.Listen();

            // --------- IoGameSetting ---------
            IoGameSetting.EnableDevMode = true;
            // China or Us
            IoGameSetting.SetLanguage(IoGameLanguage.China);
            // message callback. 回调监听
            IoGameSetting.ListenMessageCallback = new MyListenMessageCallback();
            IoGameSetting.GameGameConsole = new MyGameConsole();

            // socket
            SocketInit();

            IoGameSetting.StartNet();
        }

        public static void Poll()
        {
            // Receiving server messages
            _socket.Poll();
        }

        private static void SocketInit()
        {
            IoGameSetting.Url = "ws://127.0.0.1:10100/websocket";
            _socket = new IoGameGodotWebSocket();
            IoGameSetting.NetChannel = _socket;

            // login
            _socket.OnOpen += () =>
            {
                var loginVerifyMessage = new LoginVerifyMessage { Jwt = "10" };
                SdkAction.OfLoginVerify(loginVerifyMessage, result =>
                {
                    var userMessage = result.GetValue<UserMessage>();
                    result.Log($"userMessage {userMessage}");
                });

                // heartbeat
                IdleTimer();
            };

            _socket.OnConnecting += () => { GD.Print("My OnConnecting"); };
            _socket.OnConnectError += error => { GD.PrintErr($"My OnConnectError --- {error}"); };
            _socket.OnClosing += () => { GD.Print("My OnClosing"); };
            _socket.OnClosed += () => { GD.Print("My OnClosed"); };
        }

        private static async void IdleTimer()
        {
            var heartbeatMessage = new ExternalMessage().ToByteArray();

            var counter = 0;

            while (true)
            {
                await Task.Delay(8000);
                GD.Print($"-------- ..HeartbeatMessage {counter++}");
                // Send heartbeat to server. 发送心跳给服务器
                IoGameSetting.NetChannel.WriteAndFlush(heartbeatMessage);
            }
        }
    }

    internal class MyListenMessageCallback : SimpleListenMessageCallback
    {
        public override void OnIdleCallback(ExternalMessage message)
        {
            var dataBytes = message.Data.ToByteArray();
            var longValue = new LongValue();
            longValue.MergeFrom(new CodedInputStream(dataBytes));
            /*
             * Synchronize the time of each heartbeat with that of the server.
             * 每次心跳与服务器的时间同步
             */
            MyNetConfig.CurrentTimeMillis = longValue.Value;
        }
    }

    internal class MyGameConsole : IGameConsole
    {
        public void Log(object value)
        {
            GD.Print(value);
        }
    }
}