using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    float startTimer;
    int currentScene;
    bool danielStarted = false;
    bool runningStarted = false;
    bool doOnce = false;

    private void Awake()
    {
        startTimer = 0;
        danielStarted = false;
        currentScene = 0;
        SoundManager.Instance.PlayPlayerSound("GlassShatter");

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        startTimer += Time.deltaTime;

        if (runningStarted == false && startTimer >= 0.5)
        {
            Debug.Log("Running started");
            SoundManager.Instance.PlayPlayerSound("Running");
            runningStarted = true;
        }

        if (danielStarted == false && startTimer >= 2)
        {
            Debug.Log("Daniel started");
            SoundManager.Instance.PlayEnemySound("starting");
            danielStarted = true;
        }

        if (!doOnce && startTimer >= 5)
        {
            SceneManager.LoadScene("SampleScene");
            startTimer = 0;
            Debug.Log("Loading sample scene");
            doOnce = true;
        }
        else
        {
            return;
        }

    }

    public void LoadScene(string sceneName)
    {
        currentScene = SceneManager.GetSceneByName(sceneName).rootCount;
        SceneManager.LoadScene(sceneName);
    }
}
