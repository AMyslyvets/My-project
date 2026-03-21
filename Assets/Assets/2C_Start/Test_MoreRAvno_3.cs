using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_MoreRAvno_3 : MonoBehaviour

{
    [ContextMenu("Check")]
    
    //void Start()
    void CheckNumbers()
    {
        int A = 9;
        int B = 9;
        //bool IsAMoreB = A > B;
        //bool IsSumMoreLess20 = A + B <= 20;                     A>B A=B A<B              Debug.Log(A >= B);

        if (A > B)
        {
            Debug.Log("a is more b");
        } 
        else if (A < B)
        {
            Debug.Log("b is more a");
        }
        else 

        {
            Debug.Log("A ravno B");
        }
    }

    

}
