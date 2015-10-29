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
	private bool automatedCanSidestep;

	public GameObject chargerLeft;
	public GameObject chargerRight;
	private GameObject morningStar;
	private Animator msAnimator;
	public GameObject explosion;

	// audio sources
	public AudioSource audioSidestep;
	public AudioSource audioShoot;
	public AudioSource audioLoad;

	[SyncVar(hook="OnWaitingForOthersChanged")]
	private bool waitingForOthers;

	private SyncListInt queuedMoves = new SyncListInt();

	void Start () {
		gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();
		consoleText = GameObject.Find("Console Text").GetComponent<Text>();
		loaded = false;
		automated = false;
		automatedCanSidestep = true;

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

	public void AddQueuedMove (Move move) {
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
			audioSidestep.Play();
			
		} else if (myMove.Equals (Move.Shoot)) {
			msAnimator.SetTrigger ("triggerLasers");
			audioShoot.Play();
		}

		chargerLeft.SetActive (loaded);
		chargerRight.SetActive (loaded);
		if (loaded) {
			audioLoad.Play ();
		} else {
			audioLoad.Pause ();
		}
	}

	public void TakeHit () {
		hp--;

		if (hp <= 0) {
			explosion.GetComponent<ParticleSystem> ().loop = false;
			msAnimator.SetTrigger ("triggerDeath");
			Destroy(gameObject, 1.5f);
			gameController.GameOver();
		}

		explosion.SetActive (true);
		explosion.GetComponent<ParticleSystem> ().maxParticles = explosion.GetComponent<ParticleSystem> ().maxParticles * 5;
	}

	public void AutomatedStartNextMove () {
		queuedMoves.Clear();
		AutomatedFillQueue();
	}

	void AutomatedFillQueue () {
		while (!IsQueueFull()) {
			Move move = GenerateRandomMove();

			queuedMoves.Add((int)move);

			if (move.Equals (Move.Sidestep)) {
				automatedCanSidestep = false;
			} else {
				automatedCanSidestep = true;
			}
		}
		gameController.OnQueuedMovesChanged();
	}

	Move GenerateRandomMove () {
		Array moves = Enum.GetValues(typeof(Move));
		List<Move> availableMoves = new List<Move>();
		foreach (Move move in moves) {
			if (move.Equals(Move.Shoot) && !CanShoot()) {
				continue;
			}

			if (move.Equals(Move.Sidestep) && !automatedCanSidestep) {
				continue;
			}

			availableMoves.Add(move);
		}
		return (Move)availableMoves[UnityEngine.Random.Range(0, availableMoves.Count)];
	}

	bool CanShoot () {
		bool canShoot = loaded;
		foreach (Move move in queuedMoves) {
			if (move.Equals(Move.Load)) {
				canShoot = true;
			} else if (move.Equals(Move.Shoot)) {
				canShoot = false;
			}
		}
		return canShoot;
	}
}
