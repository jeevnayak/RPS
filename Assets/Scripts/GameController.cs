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
	public GameObject spaceship;
	public GameObject player;

	private PlayerController localPlayer = null;
	private GameObject localPlayerShip = null;
	private PlayerController remotePlayer = null;
	private GameObject remotePlayerShip = null;
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
			Instantiate(player, new Vector3 (0, 0, 0), Quaternion.identity);
		}
	}

	public int GetQueueSize () {
		return queueSize;
	}

	public void SetLocalPlayer (PlayerController player) {
		localPlayer = player;

		if (localPlayerShip == null) {
			localPlayerShip = Instantiate(spaceship, new Vector3 (0, 0, 0), Quaternion.identity) as GameObject;
		}
	}
	
	public void SetRemotePlayer (PlayerController player) {
		remotePlayer = player;
		if (singlePlayer) {
			remotePlayer.MakeAutomated();
		}

		if (remotePlayerShip == null) {
			Vector3 spawnPosition = new Vector3 (0, 1, 3);
			remotePlayerShip = Instantiate(spaceship, spawnPosition, Quaternion.Euler(0, 180, 0)) as GameObject;
		}
	}

	public void OnWaitingForOthersChanged () {
		if (localPlayer == null || remotePlayer == null) {
			return;
		}

		if (localPlayer.GetWaitingForOthers() &&
		    (remotePlayer.GetWaitingForOthers() || remotePlayer.IsAutomated())) {
			queueSize = steadyStateQueueSize;
			localPlayer.CmdStartNextMove();
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

	void ResolveMove (int moveIndex) {
		localPlayer.ExecuteQueuedMove(moveIndex, remotePlayer);
		remotePlayer.ExecuteQueuedMove(moveIndex, localPlayer);
		UpdateHpText();
	}

	void UpdateHpText () {
		hpText.text = "Your HP: " + localPlayer.hp + "\nOpponent's HP: " + remotePlayer.hp;
	}
}
