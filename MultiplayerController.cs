using Godot;
using System;
using System.Linq;

public partial class MultiplayerController : Control
{
	[Export]
	private int port = 8910;
	[Export]
	private string ip = "127.0.0.1";

	private ENetMultiplayerPeer peer;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Multiplayer.PeerConnected += PlayerConnected;
		Multiplayer.PeerDisconnected += PlayerDisconnected;

		if (OS.GetCmdlineArgs().Contains("--server"))
		{
			HostGame();
		}
	}

	private void PlayerConnected(long id)
	{
		GD.Print(id);
	}

	private void PlayerDisconnected(long id)
	{
		GD.Print("Player Disconnected: " + id.ToString());
		GameManager.Players.Remove(GameManager.Players.Where(i => i.Id == id).First<PlayerInfo>());
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void HostGame()
	{
		peer = new ENetMultiplayerPeer();
		var status = peer.CreateServer(port, 2);
		if (status != Error.Ok)
		{
			GD.Print("create server failed");
			return;
		}

		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("waiting for players");
	}

	public void _on_host_pressed()
	{
		HostGame();
	}

	public void _on_start_game_pressed()
	{
		Rpc("StartGame");
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void StartGame()
	{
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	public void SendPlayerInformation(string name, int id)
	{
		GD.Print("SendPlayerInformation");

		PlayerInfo playerInfo = new PlayerInfo()
		{
			Name = name,
			Id = id
		};

		if (!GameManager.Players.Contains(playerInfo))
		{
			GameManager.Players.Add(playerInfo);
		}

		foreach (PlayerInfo player in GameManager.Players)
		{
			Rpc("SendPlayerInformation", player.Name, player.Id);

			GD.Print(player);
		}
	}
}
