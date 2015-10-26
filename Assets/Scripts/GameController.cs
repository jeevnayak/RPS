using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public enum Move {Rock, Paper, Scissors, Shoot, Sidestep};

public class GameController : MonoBehaviour {

	public Fighter player;
	public Fighter opponent;
	public Text consoleText;

	private int queueSize;
	private List<Move> queuedMoves;

	void Start () {
		consoleText.text = "";

		queueSize = 1;
		queuedMoves = new List<Move>();
	}

	void Update () {
		if (Input.GetKeyDown(KeyCode.A)) {
			queuedMoves.Add(Move.Rock);
		} else if (Input.GetKeyDown(KeyCode.S)) {
			queuedMoves.Add(Move.Paper);
		} else if (Input.GetKeyDown(KeyCode.D)) {
			queuedMoves.Add(Move.Scissors);
		} else if (Input.GetKeyDown(KeyCode.F)) {
			queuedMoves.Add(Move.Shoot);
		} else if (Input.GetKeyDown(KeyCode.G)) {
			queuedMoves.Add(Move.Sidestep);
		}

		if (queuedMoves.Count == queueSize) {
			foreach (Move move in queuedMoves) {
				Move playerMove = player.ExecuteMove(move);
				consoleText.text += "You: " + playerMove.ToString() + "\n";

				Move opponentMove = opponent.ExecuteRandomMove();
				consoleText.text += "Opponent: " + opponentMove.ToString() + "\n";
			}
			queueSize = 2;
			queuedMoves.Clear();
		}
	}
}
