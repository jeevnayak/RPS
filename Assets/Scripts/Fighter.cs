using UnityEngine;
using System;
using System.Collections;

public class Fighter : MonoBehaviour {

	private enum WeaponState {Empty, Rock, Paper, Scissors};

	private WeaponState weaponState;

	void Start () {
		weaponState = WeaponState.Empty;
	}
	
	public Move ExecuteMove (Move move) {
		if (move.Equals(Move.Rock)) {
			weaponState = WeaponState.Rock;
		} else if (move.Equals(Move.Paper)) {
			weaponState = WeaponState.Paper;
		} else if (move.Equals(Move.Scissors)) {
			weaponState = WeaponState.Scissors;
		} else if (move.Equals (Move.Shoot)) {
			weaponState = WeaponState.Empty;
		}
		return move;
	}

	public Move ExecuteRandomMove () {
		return ExecuteMove (GenerateRandomValidMove ());
	}

	Move GenerateRandomValidMove () {
		Move move = GetRandomMove();
		while (move.Equals(Move.Shoot) && weaponState.Equals(WeaponState.Empty)) {
			move = GetRandomMove();
		}
		return move;
	}
	
	Move GetRandomMove () {
		Array moves = Enum.GetValues(typeof(Move));
		return (Move)moves.GetValue(UnityEngine.Random.Range(0, moves.Length));
	}
}
