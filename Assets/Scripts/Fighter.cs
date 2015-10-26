using UnityEngine;
using System;
using System.Collections;

public class Fighter : MonoBehaviour {

	public int hp;

	private Weapon? loadedWeapon;

	void Start () {
		loadedWeapon = null;
	}

	public Weapon? GetLoadedWeapon () {
		return loadedWeapon;
	}
	
	public Move ExecuteMove (Move move) {
		if (move.type.Equals(Move.MoveType.Load)) {
			loadedWeapon = move.weapon;
		} else if (move.type.Equals(Move.MoveType.Shoot)) {
			loadedWeapon = null;
		}
		return move;
	}

	public Move ExecuteRandomMove () {
		return ExecuteMove (GenerateRandomValidMove ());
	}

	public void TakeHit () {
		hp--;
	}

	Move GenerateRandomValidMove () {
		Move move = GetRandomMove();
		while (move.type.Equals(Move.MoveType.Shoot) && loadedWeapon == null) {
			move = GetRandomMove();
		}
		if (move.type.Equals (Move.MoveType.Shoot)) {
			move.weapon = loadedWeapon;
		}
		return move;
	}
	
	Move GetRandomMove () {
		Array moveTypes = Enum.GetValues(typeof(Move.MoveType));
		Move.MoveType moveType = (Move.MoveType)moveTypes.GetValue(UnityEngine.Random.Range(0, moveTypes.Length));
		Weapon? weapon = null;
		if (moveType.Equals(Move.MoveType.Load)) {
			Array weapons = Enum.GetValues(typeof(Weapon));
			weapon = (Weapon)weapons.GetValue(UnityEngine.Random.Range(0, weapons.Length));
		}
		return new Move(moveType, weapon);
	}
}
