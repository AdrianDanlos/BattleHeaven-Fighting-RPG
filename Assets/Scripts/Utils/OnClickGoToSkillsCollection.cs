using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnClickGoToSkillsCollection : MonoBehaviour
{
    public void GoToSkillsCollection()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.Cards.ToString());
    }
}



