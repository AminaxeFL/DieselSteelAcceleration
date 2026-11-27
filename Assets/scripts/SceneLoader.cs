using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private float _changeSceneCooldown;
    private GameObject _player;

    private bool _isNextScene = false;
    private bool _isPreviousScene = false;
    private int currentSceneIndex;

    private Color backgroundColor;
    private Scene currentScene;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        backgroundColor = _backgroundImage.color;
        currentScene = SceneManager.GetActiveScene();
        StartCoroutine(StartChangeLoadScreen());
    }

    private void Update()
    {
        float changeSpeed = 1f / _changeSceneCooldown;

        if (_isNextScene)
        {

            _backgroundImage.enabled = true;
            backgroundColor.a += changeSpeed * Time.deltaTime;
            backgroundColor.a = Mathf.Clamp01(backgroundColor.a);
            _backgroundImage.color = backgroundColor;

        }
        if (_isPreviousScene)
        {

            _backgroundImage.enabled = true;
            backgroundColor.a -= changeSpeed * Time.deltaTime;
            backgroundColor.a = Mathf.Clamp01(backgroundColor.a);
            _backgroundImage.color = backgroundColor;

        }
    }
    public void NextScene()
    {
        currentSceneIndex = currentScene.buildIndex;
        StartCoroutine(ChangeBackground(true));
    }

    public void PerviousScene()
    {
        currentSceneIndex = currentScene.buildIndex;
        StartCoroutine(ChangeBackground(false));
    }

    public void LeaveGame()
    {
        Application.Quit();
    }

    IEnumerator ChangeBackground(bool isChange)
    {
        if (isChange)
        {
            _isNextScene = true;
            yield return new WaitForSeconds(_changeSceneCooldown);
            _isNextScene = false;
            backgroundColor.a = 1f;
            _backgroundImage.color = backgroundColor;
            yield return new WaitForSeconds(0.1f);
            SceneManager.LoadScene(currentSceneIndex + 1);

        }
        if (!isChange)
        {
            _isNextScene = true;
            yield return new WaitForSeconds(_changeSceneCooldown);
            _isNextScene = false;
            _backgroundImage.color = backgroundColor;
            yield return new WaitForSeconds(0.1f);
            if (currentSceneIndex != 0)
            {
                SceneManager.LoadScene(currentSceneIndex - 1);
            }
        }
    }
    IEnumerator StartChangeLoadScreen()
    {
        _isPreviousScene = true;
        if (currentScene.buildIndex == 0)
        {
            if (_player != null)
            {
                Destroy(_player);
            }
        }
        
        yield return new WaitForSeconds(_changeSceneCooldown);
        _isPreviousScene = false;
        _backgroundImage.enabled = false;
    }
}