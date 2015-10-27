using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public enum Move {Load, Shoot, Sidestep};

public class GameController : MonoBehaviour {

	public Text consoleText;
	public Text hpText;
	public int initQueueSize;
	public int steadyStateQueueSize;

	private PlayerController localPlayer = null;
	private PlayerController remotePlayer = null;
	private int queueSize;

	void Start () {
		queueSize = initQueueSize;

		consoleText.text = "";
		hpText.text = "";
	}

	public int GetQueueSize () {
		return queueSize;
	}

	public void SetLocalPlayer (PlayerController player) {
		localPlayer = player;
	}
	
	public void SetRemotePlayer (PlayerController player) {
		remotePlayer = player;
	}

	public void OnWaitingForOthersChanged () {
		if (localPlayer == null || remotePlayer == null) {
			return;
		}

		if (localPlayer.GetWaitingForOthers() && remotePlayer.GetWaitingForOthers()) {
			localPlayer.CmdStartNextMove();
			queueSize = steadyStateQueueSize;
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
