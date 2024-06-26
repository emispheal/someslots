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

    public static Dictionary<string, List<int>> CHEATS = new Dictionary<string, List<int>>()
    {
        {"small win", new List<int> { 0, 1, 2, 3, 4 }},
        {"other win", new List<int> { 0, 2, 2, 4, 2 }},
        {"long cascade", new List<int> { 17, 23, 11, 13, 23 }},
        {"respin trigger", new List<int> { 26, 12, 33, 30, 1 }},
        {"respin trigger2", new List<int> { 34, 30, 13, 19, 20 }},
        {"wild win", new List<int> { 15, 8, 7, 0, 0 }}
    };

    public static string SERVER_URL = "https://ec2.spheal.xyz/api/spin";

    public static int ROWS = 5;
    public static int COLS = 5;

    public static string SYMBOL_OBJECT = "SymbolObject";

    public static string CUR_PAY_STR = "Current Pay: $";

}
