using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [SerializeField] KeyCode ExitGameKey = KeyCode.Escape;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyUp(ExitGameKey)) 
        {
            ExitGame();
        }
    }

    public void StartGame() 
    {
        SceneManager.LoadScene(1);
    }

    public void ExitGame() 
    {
        if (!Application.isEditor)
        {
            Application.Quit();
        }
        else
        {
            EditorApplication.ExitPlaymode();
        }
    }
}
