using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_4_1forfor : MonoBehaviour
{
    [ContextMenu("Check")]
     
    
    void CheckNumbers()
    {
        

       for (int i = 0; i < 5; i++)
       {
           for (int j = 0; j < 3; j++)
           {
               Debug.Log($"i: {i}, j: {j}");
           }
       }
       Debug.Log("The End!");

         
    }
     
}