using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {
	
	//annoying gui code..
	private Vector2 buttonSize = new Vector2(200,100);
	
	public bool spawnedPlayer = false;
	
	// This is rather a terrible GUI
	void OnGUI () {
	
		//if we already spaned a player, our work is done
		if(spawnedPlayer)
		{
			if(GUI.Button(new Rect(30, 40, buttonSize.x/3, buttonSize.y/5), "Restart") || Input.GetKeyDown(KeyCode.Space))
			{
				PoliteDisconnect();
				Restart();
			}
			return;
		}
	
		int numPlayers = 2;
		//check to see if we're already connected
		if (Network.peerType == NetworkPeerType.Disconnected)
		{
			string typeName = "DGDGNetworking";
		
			if(GUI.Button(new Rect(100, 400, buttonSize.x, buttonSize.y), "Initialize Server"))
			{
				//Start the server
				int port = 25000;
				Network.InitializeServer(numPlayers,port,!Network.HavePublicAddress());
				
				//Register the Game on Unity's Master Server
				MasterServer.RegisterHost(typeName, "Part 1");
			}
			if(GUI.Button(new Rect(100, 200, buttonSize.x, buttonSize.y), "Refresh Hosts"))
			{
				//Refresh the list of registered hosts from the Master Server
				MasterServer.RequestHostList(typeName);
			}
			
			//Display all the hosts
			HostData[] hosts = MasterServer.PollHostList();
			int i =0;
			foreach(HostData host in hosts)
			{
				if(GUI.Button(new Rect(350,200+buttonSize.y*1.5f*i,buttonSize.x,buttonSize.y),"Join "+host.gameName))
				{
					//Join as a client
					Network.Connect(host);
				}
			}
		}
		else if(Network.peerType == NetworkPeerType.Server 
					&& Network.connections.Length==numPlayers-1) //Network.connections.Length counts all clients (not server) from server
		{
			SpawnPlayer();
		}
	}
	
	public Transform playerPrefab;
	
	//Called by client
	public void OnConnectedToServer()
	{
		SpawnPlayer();
	}
	
	public void SpawnPlayer()
	{
		spawnedPlayer = true;
		Debug.Log("spawned a player!");
		
		//Set spawn point differently for client and server
		Vector3 spawnPoint = new Vector3(-5,0,0);
		if(Network.peerType == NetworkPeerType.Client)
			spawnPoint *= -1;
			
		//Spawn the player!
		Network.Instantiate(playerPrefab,spawnPoint,Quaternion.identity,0);	
	}
	
	//For when server dies..
	/*private void OnDisconnectedFromServer(NetworkDisconnection info) {
		Debug.Log("ack disconnected!");
        if (Network.isServer)
            MasterServer.UnregisterHost();	
		Restart();
    }*/
	
	public void PoliteDisconnect()
	{
		Debug.Log("sending death message to others");
		networkView.RPC("Restart", RPCMode.OthersBuffered);
	}
	
	//Disconnect even if exit window
	private void OnApplicationQuit()
	{
		PoliteDisconnect();
		Network.Disconnect();
	}
	
	[RPC]
	public void Restart()
	{
		Network.Disconnect();
		
		//kill all players in my game
		foreach(GameObject g in GameObject.FindGameObjectsWithTag("Player"))
		{
			GameObject.Destroy(g);
		}
			
		//get ready to start a new game
		spawnedPlayer = false;
	}
}
