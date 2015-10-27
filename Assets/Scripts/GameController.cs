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

	private int queueSize;

	void Start () {
		queueSize = initQueueSize;

		consoleText.text = "";
		hpText.text = "";
	}

	public int GetQueueSize () {
		return queueSize;
	}

	public void OnWaitingForOthersChanged () {
		PlayerController localPlayerController = null;
		PlayerController remotePlayerController = null;
		
		GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
		foreach (GameObject player in players) {
			PlayerController playerController = player.GetComponent<PlayerController>();
			if (playerController.isLocalPlayer) {
				localPlayerController = playerController;
			} else {
				remotePlayerController = playerController;
			}
		}
		
		if (localPlayerController == null || remotePlayerController == null) {
			return;
		}

		if (localPlayerController.GetWaitingForOthers() &&
		    remotePlayerController.GetWaitingForOthers()) {
			localPlayerController.CmdStartNextMove();
			queueSize = steadyStateQueueSize;
		}
	}

	public void OnQueuedMovesChanged () {
		PlayerController localPlayerController = null;
		PlayerController remotePlayerController = null;

		GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
		foreach (GameObject player in players) {
			PlayerController playerController = player.GetComponent<PlayerController>();
			if (playerController.isLocalPlayer) {
				localPlayerController = playerController;
			} else {
				remotePlayerController = playerController;
			}
		}

		if (localPlayerController == null || remotePlayerController == null) {
			return;
		}

		if (!localPlayerController.IsQueueFull() || !remotePlayerController.IsQueueFull()) {
			return;
		}

		for (int i = 0; i < queueSize; i++) {
			ResolveMove(localPlayerController, remotePlayerController, i);
		}

		localPlayerController.CmdWaitForOthers();
	}

	void ResolveMove (PlayerController localPlayerController,
	                  PlayerController remotePlayerController,
	                  int moveIndex) {
		localPlayerController.ExecuteQueuedMove(moveIndex, remotePlayerController);
		remotePlayerController.ExecuteQueuedMove(moveIndex, localPlayerController);
		UpdateHpText (localPlayerController, remotePlayerController);
	}

	void UpdateHpText (PlayerController localPlayerController,
	                   PlayerController remotePlayerController) {
		hpText.text = "Your HP: " + localPlayerController.hp + "\nOpponent's HP: " + remotePlayerController.hp;
	}
}
