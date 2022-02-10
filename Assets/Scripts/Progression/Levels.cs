public static class Levels {
    //This constant is used to calculate the total xp of each level. Formula: Level x LevelMultiplier.
    const int LevelMultiplier = 15;
    public static int MaxXpOfCurrentLevel(int playerLevel){
        return playerLevel * LevelMultiplier;
    }

    public static int MinXpOfCurrentLevel(int playerLevel){
        return playerLevel - 1 * LevelMultiplier;
    }
}