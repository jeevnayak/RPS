using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public enum Move {Load, Shoot, Sidestep};

public class GameController : MonoBehaviour {

	public Button loadButton;
	public Button shootButton;
	public Button sidestepButton;
	public Button singlePlayerButton;
	public Button restartButton;
	public Text consoleText;
	public Text hpText;
	public int initQueueSize;
	public int steadyStateQueueSize;
	public GameObject player;

	private PlayerController localPlayer = null;
	private PlayerController remotePlayer = null;
	private int queueSize;
	private bool singlePlayer;

	public GameObject winloseLighting;
	public GameObject winModel;
	public GameObject loseModel;

	void Start () {
		queueSize = initQueueSize;
		singlePlayer = false;

		consoleText.text = "";
		hpText.text = "";
	}

	public void StartSinglePlayer () {
		singlePlayer = true;
		GameObject.Find("Network Manager").GetComponent<NetworkManager>().StartHost();
		Instantiate(player, new Vector3 (0, 0, 3), Quaternion.Euler(0, 180, 0));
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
		Destroy(singlePlayerButton.gameObject);
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
			shootButton.interactable = true;
			sidestepButton.interactable = true;
		}
	}
	
	public void QueueShoot () {
		if (localPlayer != null && !localPlayer.IsQueueFull()) {
			localPlayer.AddQueuedMove (Move.Shoot);
			shootButton.interactable = false;
			sidestepButton.interactable = true;
		}
	}
	
	public void QueueSidestep () {
		if (localPlayer != null && !localPlayer.IsQueueFull()) {
			localPlayer.AddQueuedMove (Move.Sidestep);
			sidestepButton.interactable = false;
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

	public void GameOver () {
		winloseLighting.SetActive(true);
		if (localPlayer.hp <= 0) {
			// lose
			loseModel.SetActive(true);
			winModel.SetActive(false);
		} else if (remotePlayer.hp <= 0) {
			// win
			loseModel.SetActive(false);
			winModel.SetActive(true);
		}

		loadButton.gameObject.SetActive(false);
		shootButton.gameObject.SetActive(false);
		sidestepButton.gameObject.SetActive(false);
		restartButton.gameObject.SetActive(true);
	}

	public void Restart () {
		Application.LoadLevel(Application.loadedLevel);
	}
}
