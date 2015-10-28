using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : NetworkBehaviour {

	public int hp;
	public Text queuedMoveText;

	private GameController gameController;
	private Text consoleText;
	public bool loaded;
	private bool automated;

	public GameObject chargerLeft;
	public GameObject chargerRight;
	private GameObject morningStar;
	private Animator msAnimator;

	[SyncVar(hook="OnWaitingForOthersChanged")]
	private bool waitingForOthers;

	private SyncListInt queuedMoves = new SyncListInt();

	void Start () {
		gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();
		consoleText = GameObject.Find("Console Text").GetComponent<Text>();
		loaded = false;
		automated = false;

		queuedMoves.Callback += OnQueuedMovesChanged;

		if (isLocalPlayer) {
			gameController.SetLocalPlayer(this);
		} else {
			gameController.SetRemotePlayer(this);
		}

		chargerLeft.SetActive (loaded);
		chargerRight.SetActive (loaded);

		morningStar = gameObject.transform.Find ("MorningStar").gameObject;
		msAnimator = morningStar.GetComponent<Animator> ();
	}

	void Update () {
		if (!isLocalPlayer) {
			return;
		}

		if (IsQueueFull()) {
			return;
		}

		if (Input.GetKeyDown(KeyCode.A)) {
			AddQueuedMove(Move.Load);
		} else if (Input.GetKeyDown(KeyCode.S)) {
			AddQueuedMove(Move.Shoot);
		} else if (Input.GetKeyDown(KeyCode.D)) {
			AddQueuedMove(Move.Sidestep);
		}
	}

	public bool IsAutomated () {
		return automated;
	}

	public void MakeAutomated () {
		automated = true;
		AutomatedFillQueue();
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

	void AddQueuedMove (Move move) {
		if (isLocalPlayer) {
			Text queuedMoveDisplay = Instantiate (queuedMoveText) as Text;
			queuedMoveDisplay.text = move.ToString ();
			queuedMoveDisplay.transform.SetParent (GameObject.Find ("Canvas").transform);
			Vector3 pos = queuedMoveDisplay.transform.position;
			pos.x += 80 * queuedMoves.Count;
			queuedMoveDisplay.transform.position = pos;
		}

		CmdAddQueuedMove(move);
	}

	[Command]
	void CmdAddQueuedMove (Move move) {
		queuedMoves.Add((int)move);
	}

	public void StartNextMove () {
		if (isLocalPlayer) {
			GameObject[] displayedQueuedMoves = GameObject.FindGameObjectsWithTag("QueuedMoveText");
			foreach (GameObject queuedMoveDisplay in displayedQueuedMoves) {
				Destroy(queuedMoveDisplay);
			}
		}

		CmdStartNextMove();
	}

	[Command]
	void CmdStartNextMove () {
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

		// animations
		animateMove (myMove);
	}

	public void animateMove(Move myMove) {

		if (myMove.Equals (Move.Sidestep)) {
			msAnimator.SetTrigger ("triggerRoll");
			
		} else if (myMove.Equals (Move.Shoot)) {
			msAnimator.SetTrigger ("triggerLasers");
		}

		chargerLeft.SetActive (loaded);
		chargerRight.SetActive (loaded);
	}

	public void TakeHit () {
		hp--;
	}

	public void AutomatedStartNextMove () {
		queuedMoves.Clear();
		AutomatedFillQueue();
	}

	void AutomatedFillQueue () {
		while (!IsQueueFull()) {
			queuedMoves.Add((int)GenerateRandomMove());
		}
		gameController.OnQueuedMovesChanged();
	}

	Move GenerateRandomMove () {
		Array moves = Enum.GetValues(typeof(Move));
		return (Move)moves.GetValue(UnityEngine.Random.Range(0, moves.Length));
	}
}
