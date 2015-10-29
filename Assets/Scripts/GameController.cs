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
	public Button multiplayerButton;
	public Button restartButton;
	public Text consoleText;
	public Text hpText;
	public Text waitingForRestartText;
	public Text waitingForMatchText;
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
		Instantiate(player, new Vector3 (0, 0, 0), Quaternion.identity);
	}

	public void StartMultiplayer () {
		singlePlayerButton.gameObject.SetActive (false);
		multiplayerButton.gameObject.SetActive (false);
		waitingForMatchText.gameObject.SetActive (true);
		gameObject.GetComponent<AutoNetwork> ().StartMultiplayer ();
	}

	public int GetQueueSize () {
		return queueSize;
	}

	public void SetLocalPlayer (PlayerController player) {
		localPlayer = player;
		InitButtons ();
		UpdateHpText ();
	}
	
	public void SetRemotePlayer (PlayerController player) {
		remotePlayer = player;
		if (singlePlayer) {
			remotePlayer.MakeAutomated();
		}
		singlePlayerButton.gameObject.SetActive (false);
		multiplayerButton.gameObject.SetActive (false);
		waitingForMatchText.gameObject.SetActive (false);
		InitButtons ();
		UpdateHpText ();
	}

	void InitButtons () {
		if (localPlayer == null || remotePlayer == null) {
			return;
		}

		loadButton.gameObject.SetActive(true);
		shootButton.gameObject.SetActive(true);
		shootButton.interactable = false;
		sidestepButton.gameObject.SetActive(true);
		sidestepButton.interactable = true;
		loseModel.SetActive(false);
		winModel.SetActive(false);
		restartButton.gameObject.SetActive(false);
	}

	public void OnWaitingForOthersChanged () {
		if (localPlayer == null || remotePlayer == null) {
			return;
		}

		if (localPlayer.GetWaitingForOthers() &&
		    (remotePlayer.GetWaitingForOthers() ||
		 	!remotePlayer.IsQueueFull() ||
		 	remotePlayer.IsAutomated())) {
			queueSize = steadyStateQueueSize;
			localPlayer.StartNextMove();
			if (remotePlayer.IsAutomated()) {
				remotePlayer.AutomatedStartNextMove();
			}
		}
	}

	public void OnWaitingForRestartChanged () {
		if (localPlayer == null || remotePlayer == null) {
			return;
		}
		
		if (localPlayer.GetWaitingForRestart()) {
			if (localPlayer.GetHp() == 0 || remotePlayer.GetHp() == 0) {
				waitingForRestartText.gameObject.SetActive(true);
				loseModel.SetActive(false);
				winModel.SetActive(false);
				restartButton.gameObject.SetActive(false);
			}

		    if (remotePlayer.GetWaitingForRestart() || remotePlayer.IsAutomated()) {
				Restart();
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
		if (localPlayer == null || remotePlayer == null) {
			return;
		}

		hpText.text = "Your HP: " + localPlayer.GetHp() + "\nOpponent's HP: " + remotePlayer.GetHp();
	}

	public void GameOver () {
		winloseLighting.SetActive(true);
		if (localPlayer.GetHp() <= 0) {
			// lose
			loseModel.SetActive(true);
			winModel.SetActive(false);
		} else if (remotePlayer.GetHp() <= 0) {
			// win
			loseModel.SetActive(false);
			winModel.SetActive(true);
		}

		loadButton.gameObject.SetActive(false);
		shootButton.gameObject.SetActive(false);
		sidestepButton.gameObject.SetActive(false);
		restartButton.gameObject.SetActive(true);
	}

	public void WaitForRestart () {
		localPlayer.CmdWaitForRestart();
	}

	void Restart () {
		waitingForRestartText.gameObject.SetActive (false);

		localPlayer.Reset();
		remotePlayer.Reset();

		InitButtons ();
		UpdateHpText ();
	}
}
