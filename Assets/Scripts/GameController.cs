using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public enum Move {Load, Shoot, Sidestep};

public class GameController : MonoBehaviour {

	public Text consoleText;
	public Text hpText;
	public int initQueueSize;
	public int steadyStateQueueSize;
	public GameObject player;

	private PlayerController localPlayer = null;
	private PlayerController remotePlayer = null;
	private int queueSize;
	private bool singlePlayer;

	void Start () {
		queueSize = initQueueSize;
		singlePlayer = false;

		consoleText.text = "";
		hpText.text = "";
	}

	void Update () {
		if (Input.GetKeyDown(KeyCode.P) && remotePlayer == null) {
			singlePlayer = true;
			GameObject.Find("Network Manager").GetComponent<NetworkManager>().StartHost();
			Instantiate(player, new Vector3 (0, 0, 3), Quaternion.Euler(0, 180, 0));
		}
	}

	public int GetQueueSize () {
		return queueSize;
	}

	public void SetLocalPlayer (PlayerController player) {
		localPlayer = player;
	}
	
	public void SetRemotePlayer (PlayerController player) {
		remotePlayer = player;
		if (singlePlayer) {
			remotePlayer.MakeAutomated();
		}
	}

	public void OnWaitingForOthersChanged () {
		if (localPlayer == null || remotePlayer == null) {
			return;
		}

		if (localPlayer.GetWaitingForOthers() &&
		    (remotePlayer.GetWaitingForOthers() || remotePlayer.IsAutomated())) {
			queueSize = steadyStateQueueSize;
			localPlayer.StartNextMove();
			if (remotePlayer.IsAutomated()) {
				remotePlayer.AutomatedStartNextMove();
			}
		}
	}

	public void OnQueuedMovesChanged () {
		if (localPlayer == null || remotePlayer == null) {
			return;
		}

		if (!localPlayer.IsQueueFull() || !remotePlayer.IsQueueFull()) {
			return;
		}

		for (int i = 0; i < queueSize; i++) {
			ResolveMove(i);
		}

		localPlayer.CmdWaitForOthers();
	}

	public void QueueLoad () {
		if (localPlayer != null && !localPlayer.IsQueueFull()) {
			localPlayer.AddQueuedMove (Move.Load);
		}
	}
	
	public void QueueShoot () {
		if (localPlayer != null && !localPlayer.IsQueueFull()) {
			localPlayer.AddQueuedMove (Move.Shoot);
		}
	}
	
	public void QueueSidestep () {
		if (localPlayer != null && !localPlayer.IsQueueFull()) {
			localPlayer.AddQueuedMove (Move.Sidestep);
		}
	}

	void ResolveMove (int moveIndex) {
		localPlayer.ExecuteQueuedMove(moveIndex, remotePlayer);
		remotePlayer.ExecuteQueuedMove(moveIndex, localPlayer);
		UpdateHpText();
	}

	void UpdateHpText () {
		hpText.text = "Your HP: " + localPlayer.hp + "\nOpponent's HP: " + remotePlayer.hp;
	}
}
