using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour
{
   public void Main() {
      SceneManager.LoadScene(0);
   }
   public void Practice() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
   }
   public void Local() {
      SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);
   }
      public void CPU() {
      SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 3);
   }
}
