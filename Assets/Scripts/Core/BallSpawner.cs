using System.Collections;
using UnityEngine;
using TMPro;

public class BallSpawner : MonoBehaviour
{
    public GameObject ballPrefab;
    public TextMeshProUGUI ballCountText;

    public int ballCount = 10;

    //public float spawnHeight = 8f;
    public float spawnDelay = 0.25f;

    private int ballsLeft;

    void Start()
    {
        ballsLeft = ballCount;
        UpdateUI();
    }

    public void SpawnBalls(float xPosition, float yPosition)
    {
        
        StartCoroutine(SpawnRoutine(xPosition, yPosition));
    }

    public void SpawnBalls(float xPosition, float yPosition, int num)
    {
        StartCoroutine(SpawnRoutine(xPosition, yPosition, num));
    }

    private IEnumerator SpawnRoutine(float xPos, float yPos)
    {
        yield return new WaitForSeconds(spawnDelay);
        for (int i = 0; i < ballCount; i++)
        {
            Vector3 spawnPos = new Vector3(xPos, yPos, 0);
            GameObject ball = Instantiate(ballPrefab, spawnPos, Quaternion.identity);

            Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.gravityScale = 1;

            ballsLeft--;
            UpdateUI();

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private IEnumerator SpawnRoutine(float xPos, float yPos, int num)
    {
        yield return new WaitForSeconds(spawnDelay);
        for (int i = 0; i < num; i++)
        {
            Vector3 spawnPos = new Vector3(xPos, yPos, 0);
            GameObject ball = Instantiate(ballPrefab, spawnPos, Quaternion.identity);

            Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.gravityScale = 1;

            ballsLeft--;
            UpdateUI();

            yield return new WaitForSeconds(spawnDelay);
        }
    }



    void UpdateUI()
    {
        if (ballCountText != null)
            ballCountText.text = "Ball Left: " + ballsLeft.ToString();
    }
}