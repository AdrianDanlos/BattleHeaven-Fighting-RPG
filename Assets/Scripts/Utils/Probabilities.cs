using UnityEngine;

public static class Probabilities
{
    public static bool IsHappening(int probabilityInPercentage)
    {
        int randomNumber = Random.Range(0, 100) + 1;
        return randomNumber <= probabilityInPercentage ? true : false;
    }
}

