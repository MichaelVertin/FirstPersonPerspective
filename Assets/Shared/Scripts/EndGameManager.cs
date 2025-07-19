using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameManager : MonoBehaviour
{
    [SerializeField] TMP_Text endGameMessage;
    [SerializeField] GameObject endGameScreen;

    private bool __gameOver = false;

    public bool GameOver
    {
        get { return __gameOver; }
    }

    public void OnGameEnd(bool won)
    {
        if (GameOver) return;
        string message = "";

        if (won)
        {
            message = "You Win!";
        }
        else
        {
            message = "You Lost!";
        }

        endGameScreen.SetActive(true);
        endGameMessage.text = message;
    }

    public void RestartGame()
    {
        __gameOver = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
