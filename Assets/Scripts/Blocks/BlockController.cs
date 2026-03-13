using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class BlockController : MonoBehaviour
{
    public int health = 10;
    public AudioClip destroyClip;
    public GameObject destroyEffectPrefab;

    private SpriteRenderer spriteRenderer;
    public TextMeshProUGUI blockLeftText;

    private static int blockCount = 16;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateUI();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        health--;
        UpdateColor();

        if (health <= 0)
        {
            GameObject effect = Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
            AudioSource effectAudio = effect.AddComponent<AudioSource>();
            effectAudio.clip = destroyClip;
            effectAudio.Play();
            Destroy(effect, destroyClip.length + 0.1f);

            Destroy(gameObject);
            blockCount--;
            UpdateUI();

            if (blockCount <= 0)
            {
                GoToNextLevel();
            }
        }
    }

    private void UpdateColor()
    {
        if (health >= 5)
            spriteRenderer.color = Color.green;
        else if (health >= 3)
            spriteRenderer.color = Color.yellow;
        else
            spriteRenderer.color = Color.red;
    }

    private void UpdateUI()
    {
        blockLeftText.text = "Block Left: " + blockCount.ToString();
    }

    private void GoToNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            nextSceneIndex = 0;
        }
        SceneManager.LoadScene(nextSceneIndex);
    }
}