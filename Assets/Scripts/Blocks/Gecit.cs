using UnityEngine;
using TMPro;

public class Gecit : MonoBehaviour
{
    [SerializeField] private TextMeshPro label;

    [Header("Spawner")]
    public BallSpawner spawner;

    bool _isMultiplier;
    int _value;
    bool _consumedThisTurn;

    void Start()
    {
        ParseLabel(label.text.Trim());
    }

    void ParseLabel(string text)
    {
        if (text.StartsWith("+"))
        {
            _isMultiplier = false;
            int.TryParse(text.Substring(1), out _value);
        }
        else if (text.StartsWith("x") || text.StartsWith("X"))
        {
            _isMultiplier = true;
            int.TryParse(text.Substring(1), out _value);
        }
        
    }

    void OnTriggerEnter2D(Collider2D col)
    {

        if (!col.TryGetComponent<BallController>(out var ball)) return;
        if (IsTravellingThrough(ball)) return;
        Debug.Log("Gecit Triggered by: " + col.name);
        //if (!col.TryGetComponent<BallController>(out var ball)) return;

        if (_isMultiplier)
        {
            spawner.SpawnBalls(transform.position.x , (float)(transform.position.y - 0.5), _value-1);
            
            
        }
        else
        {
            Debug.Log("Gecit: Attempting to spawn " + _value + " balls");
            if (_consumedThisTurn) return;
            _consumedThisTurn = true;
            spawner.SpawnBalls(transform.position.x, (float)(transform.position.y - 0.5), _value);
        }
    }

    bool IsTravellingThrough(BallController ball)
    {
        Vector2 gateNormal = transform.up;
        Vector2 ballVelocity = ball.GetVelocity();
        return Vector2.Dot(ballVelocity.normalized, gateNormal) > 0f;
    }



    public void ResetGate() => _consumedThisTurn = false;

}
