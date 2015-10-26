using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public enum Weapon {Rock, Paper, Scissors};

public class Move {

	public enum MoveType {Load, Shoot, Sidestep};

	public MoveType type;
	public Weapon? weapon;

	public Move (MoveType type, Weapon? weapon) {
		this.type = type;
		this.weapon = weapon;
	}

	public override string ToString ()
	{
		string ret = type.ToString();
		if (weapon != null) {
			ret += " " + weapon.ToString();
		}
		return ret;
	}
}

public class GameController : MonoBehaviour {

	public Fighter player;
	public Fighter opponent;
	public Text consoleText;
	public Text hpText;

	private int queueSize;
	private List<Move> queuedMoves;

	void Start () {
		consoleText.text = "";
		UpdateHpText ();

		queueSize = 1;
		queuedMoves = new List<Move>();
	}

	void Update () {
		if (Input.GetKeyDown(KeyCode.A)) {
			queuedMoves.Add(new Move(Move.MoveType.Load, Weapon.Rock));
		} else if (Input.GetKeyDown(KeyCode.S)) {
			queuedMoves.Add(new Move(Move.MoveType.Load, Weapon.Paper));
		} else if (Input.GetKeyDown(KeyCode.D)) {
			queuedMoves.Add(new Move(Move.MoveType.Load, Weapon.Scissors));
		} else if (Input.GetKeyDown(KeyCode.F)) {
			queuedMoves.Add(new Move(Move.MoveType.Shoot, null));
		} else if (Input.GetKeyDown(KeyCode.G)) {
			queuedMoves.Add(new Move(Move.MoveType.Sidestep, null));
		}

		if (queuedMoves.Count == queueSize) {
			foreach (Move move in queuedMoves) {
				if (move.type.Equals(Move.MoveType.Shoot)) {
					move.weapon = player.GetLoadedWeapon();
				}
				Move playerMove = player.ExecuteMove(move);
				consoleText.text += "You: " + playerMove.ToString() + "\n";

				Move opponentMove = opponent.ExecuteRandomMove();
				consoleText.text += "Opponent: " + opponentMove.ToString() + "\n";

				ResolveMoves(playerMove, opponentMove);
			}
			queueSize = 2;
			queuedMoves.Clear();
		}
	}

	void ResolveMoves (Move playerMove, Move opponentMove) {
		if (playerMove.type.Equals (Move.MoveType.Shoot)) {
			if (opponentMove.type.Equals(Move.MoveType.Load)) {
				opponent.TakeHit();
			} else if (opponentMove.type.Equals(Move.MoveType.Shoot)) {
				if (WeaponBeatsOther(playerMove.weapon, opponentMove.weapon)) {
					opponent.TakeHit();
				}
			}
		}

		if (opponentMove.type.Equals (Move.MoveType.Shoot)) {
			if (playerMove.type.Equals(Move.MoveType.Load)) {
				player.TakeHit();
			} else if (playerMove.type.Equals(Move.MoveType.Shoot)) {
				if (WeaponBeatsOther(opponentMove.weapon, playerMove.weapon)) {
					player.TakeHit();
				}
			}
		}

		UpdateHpText ();
	}

	void UpdateHpText () {
		hpText.text = "Your HP: " + player.hp + "\nOpponent's HP: " + opponent.hp;
	}

	bool WeaponBeatsOther (Weapon? weapon, Weapon? other) {
		if (weapon.Equals (Weapon.Rock)) {
			return other.Equals(Weapon.Scissors);
		} else if (weapon.Equals (Weapon.Scissors)) {
			return other.Equals(Weapon.Paper);
		} else if (weapon.Equals (Weapon.Paper)) {
			return other.Equals(Weapon.Rock);
		}

		return false;
	}
}
