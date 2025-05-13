using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // Movement speed

    private LayerMask solicObjects; 

    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        movement.y = Input.GetAxisRaw("Vertical");   // W/S or Up/Down
        movement.Normalize(); // Optional: prevent diagonal speed boost
    }

    void FixedUpdate()
    {
        // Move the player
        rb.velocity = movement * moveSpeed;
    }

}
