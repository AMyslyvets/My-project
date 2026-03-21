using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_3IfElse : MonoBehaviour
{
    [ContextMenu("Check")]
    
    //void Start()
    void CheckNumbers()
    {
        int A = 7;
        int B = 8;
        //bool IsAMoreB = A > B;
       // bool IsSumMoreLess20 = A + B <= 20;

       if (A > B)
       {
           Debug.Log($"a is more b");
       }
       else
       {
           Debug.Log($"b is more a");
       }
    }

    

}
