using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    public float speed = 5;
    public GameObject m_Character;
    private Vector3 initialPosition;
    public static int numJumps = 0;
    Rigidbody2D m_Rigidbody;
    public bool onFloor;

    void Start()
    {
        onFloor = true;
        m_Rigidbody = GetComponent<Rigidbody2D>();

        initialPosition = new Vector3(LevelGenerator.startingPosition.x + 1, 0, 0);
        transform.position = initialPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.position += Vector3.right * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.position += Vector3.left * speed * Time.deltaTime;
        }
        
        if (onFloor)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                numJumps += 1;
                m_Rigidbody.AddForce(new Vector2(0, 6.5f), ForceMode2D.Impulse);
                onFloor = false;

            }

        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.name == "End")
        {
            LevelGenerator.inEnd = true;
        }

        if (collision.gameObject.name == "Dead")
        {
            transform.position = initialPosition;
        }
    }
}
