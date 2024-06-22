using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainConfig : MonoBehaviour
{
    public static int[] BET_MULTIPLES = {1, 2, 5, 10, 20, 50, 100};

    public enum SymbolType
    {
        Bear = 0,
        Beetle = 1,
        Bunny = 2,
        Vulture = 3,
        Eagle = 4,
        Dog = 5,
        Frog = 6,
        Pig = 7,
    }
}
