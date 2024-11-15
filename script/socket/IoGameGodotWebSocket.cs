using System;
using Godot;

namespace IoGame.Sdk.Godot
{
    public delegate void OpenEventHandler();

    public delegate void ConnectingEventHandler();

    public delegate void ConnectErrorEventHandler(Error error);

    public delegate void ClosedEventHandler();

    public delegate void ClosingEventHandler();

    public class IoGameGodotWebSocket : SimpleNetChannel
    {
        private WebSocketPeer.State _lastState = WebSocketPeer.State.Closed;
        private WebSocketPeer Socket { get; } = new();
        public string Url { set; get; }

        // 使用委托定义一个事件
        public event OpenEventHandler OnOpen = () => { GD.Print("OnOpen"); };
        public event ConnectingEventHandler OnConnecting = () => { GD.Print("OnConnecting"); };

        public event ConnectErrorEventHandler OnConnectError = error => { GD.PrintErr($"OnConnectError --- {error}"); };

        public event ClosingEventHandler OnClosing = () => { GD.Print("OnClosing"); };
        public event ClosedEventHandler OnClosed = () => { GD.Print("OnClosed"); };

        public override void Prepare()
        {
            Url ??= IoGameSetting.Url;

            var error = Socket.ConnectToUrl(Url);
            if (error != 0)
            {
                OnConnectError(error);
                return;
            }

            _lastState = Socket.GetReadyState();
        }

        public override void WriteAndFlush(byte[] bytes)
        {
            Socket.Send(bytes);
        }

        public void Poll()
        {
            if (Socket.GetReadyState() != WebSocketPeer.State.Closed) Socket.Poll();

            while (Socket.GetAvailablePacketCount() > 0)
            {
                var packet = Socket.GetPacket();
                var message = ExternalMessage.Parser.ParseFrom(packet);
                AcceptMessage(message);
            }

            var state = Socket.GetReadyState();
            if (_lastState == state) return;

            _lastState = state;
            switch (state)
            {
                case WebSocketPeer.State.Open:
                    OnOpen();
                    break;
                case WebSocketPeer.State.Connecting:
                    OnConnecting();
                    break;
                case WebSocketPeer.State.Closing:
                    OnClosing();
                    break;
                case WebSocketPeer.State.Closed:
                    OnClosed();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}