using Godot;
using My.Game;

namespace ioGameSdkCsharpExampleGodot;

public partial class Client : Node2D
{
    public override void _Ready()
    {
        GD.Print("----- ioGame Sdk Client -----");
        MyNetConfig.StartNet();

        GetBtn("single/OnIntValue").Pressed += () => { _ = Index.OnIntValue(); };
        GetBtn("single/OnLongValue").Pressed += () => { _ = Index.OnLongValue(); };
        GetBtn("single/OnBoolValue").Pressed += () => { _ = Index.OnBoolValue(); };
        GetBtn("single/OnStringValue").Pressed += () => { _ = Index.OnStringValue(); };
        GetBtn("single/OnValueObject").Pressed += () => { _ = Index.OnValueObject(); };

        GetBtn("list/OnListInt").Pressed += () => { _ = Index.OnListInt(); };
        GetBtn("list/OnListLong").Pressed += () => { _ = Index.OnListLong(); };
        GetBtn("list/OnListBool").Pressed += () => { _ = Index.OnListBool(); };
        GetBtn("list/OnListString").Pressed += () => { _ = Index.OnListString(); };
        GetBtn("list/OnListValue").Pressed += () => { _ = Index.OnListValue(); };

        GetBtn("OnTestError").Pressed += () => { _ = Index.OnTestError(); };
        GetBtn("OnTriggerBroadcast").Pressed += Index.OnTriggerBroadcast;
    }

    public override void _Process(double delta)
    {
        // Receiving server messages
        MyNetConfig.Poll();
    }

    private Button GetBtn(string path)
    {
        return GetNode<Button>(path);
    }
}