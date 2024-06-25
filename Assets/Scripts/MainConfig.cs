using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainConfig : MonoBehaviour
{
    public static int[] BET_MULTIPLES = {1, 2, 5, 10, 20, 50, 100};

    public static Dictionary<string, int> SYMBOL_TYPE_MAPPING = new Dictionary<string, int>()
    {
        {"bear", 0},
        {"beetle", 1},
        {"bunny", 2},
        {"vulture", 3},
        {"eagle", 4},
        {"dog", 5},
        {"frog", 6},
        {"pig", 7}
    };

    public static Dictionary<string, List<int>> cheats = new Dictionary<string, List<int>>()
    {
        {"preset1", new List<int> { 0, 1, 2, 3, 4 }},
        {"preset2", new List<int> { 0, 2, 2, 4, 2 }},
    };

}
