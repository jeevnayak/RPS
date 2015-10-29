﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.Match;
using System.Collections.Generic;

public class AutoNetwork : MonoBehaviour {

	List<MatchDesc> matchList = new List<MatchDesc>();
	bool matchCreated;
	NetworkMatch networkMatch;
	NetworkManager nm;
	
	void Awake()
	{
		nm = GameObject.Find ("Network Manager").GetComponent<NetworkManager> ();
		nm.StartMatchMaker ();
		networkMatch = nm.matchMaker;
	}
	
	void OnGUI()
	{
		// You would normally not join a match you created yourself but this is possible here for demonstration purposes.
		if(GUILayout.Button("Create Room"))
		{
			CreateMatchRequest create = new CreateMatchRequest();
			create.name = "NewRoom";
			create.size = 4;
			create.advertise = true;
			create.password = "";
			
			networkMatch.CreateMatch(create, nm.OnMatchCreate);
		}
		
		if (GUILayout.Button("List rooms"))
		{
			networkMatch.ListMatches(0, 20, "", OnMatchList);
		}
		
		if (matchList.Count > 0)
		{
			GUILayout.Label("Current rooms");
		}
		foreach (var match in matchList)
		{
			if (GUILayout.Button(match.name))
			{
				networkMatch.JoinMatch(match.networkId, "", nm.OnMatchJoined);
			}
		}
	}
	
	public void OnMatchCreate(CreateMatchResponse matchResponse)
	{
		if (matchResponse.success)
		{
			Debug.Log("Create match succeeded");
			matchCreated = true;
			Utility.SetAccessTokenForNetwork(matchResponse.networkId, new NetworkAccessToken(matchResponse.accessTokenString));
			NetworkServer.Listen(new MatchInfo(matchResponse), 9000);
		}
		else
		{
			Debug.LogError ("Create match failed");
		}
	}
	
	public void OnMatchList(ListMatchResponse matchListResponse)
	{
		if (matchListResponse.success && matchListResponse.matches != null)
		{

			foreach (var match in matchListResponse.matches)
			{
				Debug.Log (match.name + ";" + match.networkId);
			}

			networkMatch.JoinMatch(matchListResponse.matches[0].networkId, "", nm.OnMatchJoined);
		}
	}

	/*
	public void OnMatchJoined(JoinMatchResponse matchJoin)
	{
		if (matchJoin.success)
		{
			Debug.Log("Join match succeeded");
			if (matchCreated)
			{
				Debug.LogWarning("Match already set up, aborting...");
				return;
			}
			Utility.SetAccessTokenForNetwork(matchJoin.networkId, new NetworkAccessToken(matchJoin.accessTokenString));
			NetworkClient myClient = new NetworkClient();
			myClient.RegisterHandler(MsgType.Connect, OnConnected);
			myClient.Connect(new MatchInfo(matchJoin));
		}
		else
		{
			Debug.LogError("Join match failed");
		}
	}
	*/
	
	public void OnConnected(NetworkMessage msg)
	{
		Debug.Log("Connected!");
	}
}
