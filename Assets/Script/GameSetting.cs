using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameSetting : MonoBehaviour
{
    public GameObject[] Prefabs;
    public int score = 0;
    public TMP_Text scoreText;
    public GameObject playAgainButton;
    public bool gameOver = false;

    public bool CanInstantiateAnimal(int id)
    {
        return id >= 0 && id < Prefabs.Length;
    }

    public GameObject InstantiateAnimal(Vector2 position, int id)
    {
        if (!CanInstantiateAnimal(id))
        {
            return null;
        }

        Vector3 spawnPosition = new Vector3(position.x, position.y, Prefabs[id].transform.position.z);
        GameObject combinedAnimal = Instantiate(Prefabs[id], spawnPosition, Quaternion.identity);
        combinedAnimal.GetComponent<Prefabs>().id = id;
        return combinedAnimal;

    }

    public void addScore(int id)
    {
        switch (id)
        {
            case 0:
                score += 1;
                break;
            case 1:
                score += 3;
                break;
            case 2:
                score += 6;
                break;
            case 3:
                score += 10;
                break;
            case 4:
                score += 15;
                break;
            case 5:
                score += 21;
                break;
            case 6:
                score += 28;
                break;
            case 7:
                score += 36;
                break;
            case 8:
                score += 45;
                break;
            case 9:
                score += 55;
                break;
            case 10:
                score += 66;
                break;
        }
        scoreText.text = "Score: " + score.ToString();
    }

    public void endGame()
    {
        gameOver = true;
        playAgainButton.SetActive(true);
    }

    public void playAgain()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }
}
