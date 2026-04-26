using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitioner : MonoBehaviour
{
    private static SceneTransitioner instance;

    public bool loadingScene { get; private set; } = false;

    public static SceneTransitioner Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameObject("SceneTransitioner").AddComponent<SceneTransitioner>();
                DontDestroyOnLoad(instance.gameObject);
            }
            return instance;
        }
    }

    public static bool ReloadScene (float speedIn = 1f, float speedOut = 1f, float yieldTime = 0f) =>
        LoadScene(SceneManager.GetActiveScene().name, speedIn, speedOut, yieldTime);
    public static bool LoadScene(string name, Color transitionColor, float speedIn = 1f, float speedOut = 1f, float yieldTime = 0f)
    {
        if (Instance.loadingScene) return false;

        Instance.StartCoroutine(Instance.LoadSceneSequence(name, speedIn, speedOut, yieldTime, transitionColor));
        return true;
    }
    public static bool LoadScene (string name, float speedIn = 1f, float speedOut = 1f, float yieldTime = 0f)
    {
        if (Instance.loadingScene) return false;

        Instance.StartCoroutine(Instance.LoadSceneSequence(name, speedIn, speedOut, yieldTime, Color.black));
        return true;
    }

    protected IEnumerator LoadSceneSequence 
        (string nextScene, float speedIn, float speedOut, float yieldTime, Color transitionColor, float setupTime = 0.5f)
    {
        loadingScene = true;

        var transitionScene = "SceneTransition";
        var currentScene = SceneManager.GetActiveScene();
        string lastScene = currentScene.name;
        SceneManager.LoadScene(transitionScene, LoadSceneMode.Additive);

        while (UISceneTransitionerController.Instance == null) yield return null;

        yield return UISceneTransitionerController.Instance.TransitionIn(speedIn, transitionColor);

        AsyncOperation ao = SceneManager.UnloadSceneAsync(currentScene);
        while (!ao.isDone) yield return null;

        yield return new WaitForSecondsRealtime(yieldTime);

        ao = SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Additive);
        while (!ao.isDone) yield return null;
        Scene newMainScene = SceneManager.GetSceneByName(nextScene);
        while (!SceneManager.SetActiveScene(newMainScene));

        var sceneController = FindFirstObjectByType<SceneController>();
        sceneController?.OnSceneStart(fromScene: lastScene);

        yield return null; //Give one frame to set everything up on the new scene
        yield return new WaitForSecondsRealtime(setupTime);

        yield return UISceneTransitionerController.Instance.TransitionOut(speedOut, transitionColor);

        ao = SceneManager.UnloadSceneAsync(transitionScene);
        while (!ao.isDone) yield return null;

        loadingScene = false;
    }
}
