using Godot;
using System;
using System.Linq;

public partial class MultiplayerController : Control
{
	[Export]
	private int _port = 8910;
	[Export]
	private string _ip = "127.0.0.1";

	[Export]
	private int _maxPlayerCount = 2;

	private ENetMultiplayerPeer _peer;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GDPrint.Print("<<< START ELDRALITH SERVER >>>");
		Multiplayer.PeerConnected += PlayerConnected;
		Multiplayer.PeerDisconnected += PlayerDisconnected;


		HostGame();
		this.Hide();
		// if (OS.GetCmdlineArgs().Contains("--server"))
		// {
		// 	GDPrint.Print("Starting server in headless mode.");
		// 	HostGame();
		// }
	}

	private void PlayerConnected(long id)
	{
		GDPrint.Print($"Player <{id}> connected.");
	}

	private void PlayerDisconnected(long id)
	{
		GDPrint.Print($"Player <{id}> disconected.");
		bool success = GameManager.Players.Remove(GameManager.Players.FirstOrDefault(i => i.Id == id));
		if (!success){
			GDPrint.PrintErr($"Player <{id}> was not in the list of current players");
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void HostGame()
	{
		_peer = new ENetMultiplayerPeer();
		var status = _peer.CreateServer(_port, _maxPlayerCount);
		if (status != Error.Ok)
		{
			GDPrint.PrintErr("Server could not be created:");
			GDPrint.PrintErr($"Port: {_port}");
			GDPrint.PrintErr($"PlayerCount: {_maxPlayerCount}");
			return;
		}

		_peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
		Multiplayer.MultiplayerPeer = _peer;
		GDPrint.Print("Server started SUCCESSFULLY.");
		GDPrint.Print("Waiting for players to connect ...");
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal =false)]
	public void SendPlayerInformation(string name, int id)
	{
		PlayerInfo playerInfo = new PlayerInfo()
		{
			Name = name,
			Id = id
		};

		GDPrint.Print(playerInfo.ToString());

		GameManager.Players.Add(playerInfo);
		GameManager.Players.Each(player => Rpc("SendPlayerInformation", player.Name, player.Id));
	}
}
