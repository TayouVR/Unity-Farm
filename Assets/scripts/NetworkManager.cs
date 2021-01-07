using System;
using System.Collections.Generic;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using UnityEngine;

public class NetworkManager : MonoBehaviour {
	
	[Header("Player Controller Values")]
	public float jumpStrength = 100;
	public float speed = 1;
	public AnimatorOverrideController animator;
	public GameObject modelPrefab;
	public Transform spawnpoint;
	public Vector3 cameraOffset;
	[SerializeField] float sensitivity = 100;

	public Character character;
	
	
	[Header("Network Values")]
	public UnityClient client;
	public string playerId;

	public int networkTickrate = 10;
	public int inputTickrate;
    
	public List<PlayerEntity> Players = new List<PlayerEntity>();

	public string serverIp = "gsi01.eu2.alphablend.cloud";
	public int port = 4558;

	private void Start() {
		DontDestroyOnLoad(gameObject);
	}

	public void Connect() {
		playerId = Guid.NewGuid().ToString();
		//client = gameObject.AddComponent<UnityClient>();
		client.MessageReceived += ReceiveMessage;
		client.ConnectInBackground(serverIp, port, true, ClientConnected);
	}

	private void ClientConnected(Exception e) {
		character = new Character(playerId, inputTickrate, jumpStrength, speed, animator, modelPrefab, spawnpoint, cameraOffset, sensitivity);
		
		SchedulerSystem.AddJob(SendLocalMovement, 0, networkTickrate, -1);
		SchedulerSystem.AddJob(UpdatePlayerModels, 0, networkTickrate, -1);
		
		using (DarkRiftWriter w = DarkRiftWriter.Create()) {
			w.Write(playerId);
			using (Message m = Message.Create((ushort)Tag.OnPlayerJoined, w)) {
				client.SendMessage(m, SendMode.Reliable);
			}
		}
	}


	private void ReceiveMessage(object s, MessageReceivedEventArgs e) {
		Debug.Log(e.Tag);
		switch ((Tag)e.Tag) {
			case Tag.OnPlayerJoined:
				CreatePlayer(e.GetMessage());
				break;
			case Tag.OnPlayerLeft:
				break;
			case Tag.OnPlayerMove:
				MovePlayer(e.GetMessage());
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	private void SendLocalMovement() {
		SendMovement(character.entity);
	}

	private void SendMovement(PlayerEntity player) {
		if (client.ConnectionState != ConnectionState.Connected) return;
		using (DarkRiftWriter w = DarkRiftWriter.Create())
		{
			w.Write(player.playerId); //Player ID 
			w.Write(player.rootPositionX); //Root Pos X
			w.Write(player.rootPositionY); //Root Pos Y
			w.Write(player.rootPositionZ); //Root Pos Z
			w.Write(player.rootRotationX); //Root Rot X
			w.Write(player.rootRotationY); //Root Rot Y
			w.Write(player.rootRotationZ); //Root Rot Z
			w.Write(player.rightSpeed); //Right Speed
			w.Write(player.frontSpeed); //Front Speed
			w.Write(player.jump); //Jump
			w.Write(player.isGrounded); //Grounded
			w.Write(player.isCrouching); //Crouching
		}
	}
	
	private void CreatePlayer(Message message) {
		using (DarkRiftReader r = message.GetReader())
		{
			string Player = r.ReadString();

			GameObject go = Instantiate(modelPrefab);
            
			Players.Add(new PlayerEntity()
			{
				playerId = Player,
				playerRoot = go
			});
		}
	}

	private void UpdatePlayerModels() {
		foreach (var e in Players.FindAll(match => match.dataReceived)) {
			e.playerRoot.transform.position = new Vector3(e.rootPositionX, e.rootPositionY, e.rootPositionZ);
			e.playerRoot.transform.rotation = Quaternion.Euler(e.rootRotationX, e.rootRotationY, e.rootRotationZ);
		}
	}

	private void MovePlayer(Message message) {
		string player;
		using (DarkRiftReader r = message.GetReader())
		{
			player = r.ReadString();
			PlayerEntity e = Players.Find(match => match.playerId == player);
			if (e.playerRoot == null) return;
			e.SetNetworkValues(r);
		}
	}
    
}

public class PlayerEntity
{
	public string playerId;

	public bool dataReceived;

	public GameObject playerRoot;
    
	public float rootPositionX = 0f;
	public float rootPositionY = 0f;
	public float rootPositionZ = 0f;
	public float rootRotationX = 0f;
	public float rootRotationY = 0f;
	public float rootRotationZ = 0f;

	public float rightSpeed = 0f;
	public float frontSpeed = 0f;
	public float jump = 0f;

	public bool isGrounded;
	public bool isCrouching;

	public void SetNetworkValues(DarkRiftReader r) {
		dataReceived = true;
		rootPositionX = r.ReadSingle();
		rootPositionY = r.ReadSingle();
		rootPositionZ = r.ReadSingle();
	            
		rootRotationX = r.ReadSingle();
		rootRotationY = r.ReadSingle();
		rootRotationZ = r.ReadSingle();
	            
		rightSpeed = r.ReadSingle();
		frontSpeed = r.ReadSingle();
		jump = r.ReadSingle();

		isGrounded = r.ReadBoolean();
		isCrouching = r.ReadBoolean();
	}
}