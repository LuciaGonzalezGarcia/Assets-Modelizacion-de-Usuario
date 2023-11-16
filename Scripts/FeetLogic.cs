using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeetLogic : MonoBehaviour
{

    public CharacterMovement movement;

    private void OnTriggerStay2D(Collider2D other)
    {
        movement.onFloor = true;    
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        movement.onFloor = false;
    }
}
