using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_MoreRavno_3_1 : MonoBehaviour
 {
     [ContextMenu("Check")]
     
     //void Start()
     void CheckNumbers()
     {
         int A = 10;
         int B = 5;
 
         if (A > 5 || B > 3) 
         {
             Debug.Log("YES");
         }
         
     }
     
 }