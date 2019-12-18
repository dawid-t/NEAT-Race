using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CarMovement))]
public class CarPlayerInput : MonoBehaviour
{
	private CarMovement carMovement;


	private void Start()
	{
		carMovement = GetComponent<CarMovement>();
	}

	private void Update()
	{
		MovementInput();
	}

	private void MovementInput()
	{
		carMovement.Speed = Input.GetAxis("Vertical");
		carMovement.Direction = Input.GetAxis("Horizontal");
	}
}
