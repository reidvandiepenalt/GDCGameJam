using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class TutorialScreen : MonoBehaviour
{
    [SerializeField] List<GameObject> textOrder;
    int index = 0;

    public void LoadGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void OnAdvance()
    {
        if(index < textOrder.Count - 1)
        {
            textOrder[index].SetActive(false);
            index++;
            textOrder[index].SetActive(true);
        }
    }
}
