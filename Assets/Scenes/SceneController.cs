using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    [SerializeField] KeyCode ExitGameKey = KeyCode.Escape;
    [SerializeField] TMP_Text TitleText;
    [SerializeField] GameObject StartButton;
    [SerializeField] GameObject ExitButton;
    [SerializeField] TMP_Text CreditButtonText;
    [SerializeField] Button CreditButton;
    [SerializeField] TMP_Text CreditsTextLeft;
    [SerializeField] TMP_Text CreditsTextRight;

    [Tooltip("What to change the main title to when clicking the credits button")]
    [SerializeField] string CreditTitle = "Credits";

    private string Title;
    private bool IsInMainMenu = true;
    Vector2 TitlePosition;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        CreditsTextLeft.gameObject.SetActive(false);
        CreditsTextRight.gameObject.SetActive(false);
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
        Application.Quit();
    }

    public void OnCreditButtonPress() 
    {
        if (IsInMainMenu) { DisplayCredits(); }
        else { DisplayMainMenu(); }
    }   

    public void DisplayCredits() 
    {
        Title = TitleText.text;
        TitlePosition = TitleText.transform.position;

        TitleText.SetText("Credits");
        TitleText.transform.position = new Vector2(TitlePosition.x, TitlePosition.y + 2);

        StartButton.SetActive(false);
        ExitButton.SetActive(false);

        CreditsTextLeft.gameObject .SetActive(true);
        CreditsTextRight.gameObject .SetActive(true);
        
        IsInMainMenu = false;
        CreditButtonText.SetText("Go Back");
    }

    public void DisplayMainMenu() 
    {
        TitleText.SetText(Title);
        TitleText.transform.position = TitlePosition;

        StartButton.SetActive(true);
        ExitButton.SetActive(true);

        CreditsTextLeft.gameObject.SetActive(false);
        CreditsTextRight.gameObject.SetActive(false);

        IsInMainMenu = true;
        CreditButtonText.SetText("Credits");
    }
}
