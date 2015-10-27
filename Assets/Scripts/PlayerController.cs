using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : NetworkBehaviour {

	public int hp;

	private GameController gameController;
	private Text consoleText;
	private bool loaded;

	[SyncVar(hook="OnWaitingForOthersChanged")]
	private bool waitingForOthers;

	private SyncListInt queuedMoves = new SyncListInt();

	void Start () {
		gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();
		consoleText = GameObject.Find("Console Text").GetComponent<Text>();
		loaded = false;

		queuedMoves.Callback += OnQueuedMovesChanged;

		if (isLocalPlayer) {
			gameController.SetLocalPlayer(this);
		} else {
			gameController.SetRemotePlayer(this);
		}
	}

	void Update () {
		if (!isLocalPlayer) {
			return;
		}

		if (queuedMoves.Count == gameController.GetQueueSize()) {
			return;
		}

		if (Input.GetKeyDown(KeyCode.A)) {
			CmdAddQueuedMove(Move.Load);
		} else if (Input.GetKeyDown(KeyCode.S)) {
			CmdAddQueuedMove(Move.Shoot);
		} else if (Input.GetKeyDown(KeyCode.D)) {
			CmdAddQueuedMove(Move.Sidestep);
		}
	}

	public bool GetWaitingForOthers () {
		return waitingForOthers;
	}

	public bool IsQueueFull () {
		return queuedMoves.Count == gameController.GetQueueSize();
	}

	[Command]
	public void CmdWaitForOthers () {
		waitingForOthers = true;
	}
	
	void OnWaitingForOthersChanged (bool newWaitingForOthers) {
		waitingForOthers = newWaitingForOthers;
		gameController.OnWaitingForOthersChanged();
	}

	[Command]
	void CmdAddQueuedMove (Move move) {
		queuedMoves.Add((int)move);
	}

	[Command]
	public void CmdStartNextMove () {
		waitingForOthers = false;
		queuedMoves.Clear();
	}

	void OnQueuedMovesChanged (SyncListInt.Operation op, int index) {
		gameController.OnQueuedMovesChanged();
	}

	public void ExecuteQueuedMove (int moveIndex, PlayerController otherPlayerController) {
		Move myMove = (Move)queuedMoves[moveIndex];
		Move otherPlayerMove = (Move)otherPlayerController.queuedMoves[moveIndex];

		string playerStr = isLocalPlayer ? "Your" : "Opponent's";
		consoleText.text += playerStr + " Move: " + myMove.ToString() + "\n";
		
		if (myMove.Equals(Move.Load)) {
			loaded = true;
		} else if (myMove.Equals(Move.Shoot) && loaded) {
			if (otherPlayerMove.Equals(Move.Load)) {
				otherPlayerController.TakeHit();
			}
			loaded = false;
		}
	}

	public void TakeHit () {
		hp--;
	}
}
