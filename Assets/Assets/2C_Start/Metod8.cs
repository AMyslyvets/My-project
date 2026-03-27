using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Metod8 : MonoBehaviour

{
    [ContextMenu("Check")]
    void Start()
    {
        Debug.Log("$Before");
        PrintPrettyInfo();
        Debug.Log("$After");

    }

    void PrintPrettyInfo()
    {
         Debug.Log($"Hello World! ");
    }


}
