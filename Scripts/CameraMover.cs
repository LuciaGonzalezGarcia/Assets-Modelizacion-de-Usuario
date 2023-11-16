using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMover : MonoBehaviour
{
    public float speed = 6;
    public GameObject character;

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(character.transform.position.x, 0, -10);
    }
}
