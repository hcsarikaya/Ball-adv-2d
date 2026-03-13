using UnityEngine;

public class BallController : MonoBehaviour
{
    [Header("Spawner")]
    public BallSpawner spawner;

    [Header("Components")]
    private Rigidbody2D rb;
    private AudioSource audioSource;
    private Collider2D col;

    [Header("Ball Stats")]
    public float xRange = 10f;
    public GameObject hitEffect;
    public float minCollisionVelocity = 1.5f;
    public float collisionCooldown = 0.15f;
    private float lastCollisionTime;
    private bool isDragging = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        HandleInput();
        PhysicsLimits();
    }

    void HandleInput()
    {
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            Collider2D hit = Physics2D.OverlapPoint(worldPoint);
            if (hit != null && hit == col)
            {
                isDragging = true;
                rb.gravityScale = 0;
                rb.linearVelocity = Vector2.zero;
            }
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            float zDistance = Camera.main.WorldToScreenPoint(transform.position).z;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, zDistance));

            float clampedX = Mathf.Clamp(worldPos.x, -xRange, xRange);
            transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            rb.gravityScale = 1;

            if (spawner != null)
            {
                spawner.SpawnBalls(transform.position.x);
            }
        }
    }

    void PhysicsLimits()
    {
        if (rb.linearVelocity.y > 6f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 6f);
        }

        if (rb.linearVelocity.y < -15f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -15f);
        }

        if (transform.position.y < -5.5f)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (rb.linearVelocity.magnitude < minCollisionVelocity)
            return;

        if (Time.time - lastCollisionTime < collisionCooldown)
            return;

        lastCollisionTime = Time.time;

        if (audioSource != null)
            audioSource.Play();

        if (hitEffect != null)
        {
            GameObject effect = Instantiate(hitEffect, collision.contacts[0].point, Quaternion.identity);
            Destroy(effect, 1f);
        }
    }
}