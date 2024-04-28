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
      SceneManager.LoadScene(1);
   }
   public void MultiLocal() {
      SceneManager.LoadScene(2);
   }
   public void MultiCPU() {
      SceneManager.LoadScene(3);
   }
}
