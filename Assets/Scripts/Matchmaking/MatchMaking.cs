using System.Collections.Generic;
using System.Collections.Specialized;
using System;
using UnityEngine;

public static class MatchMaking
{
    private static readonly int baseEloGain = 15;
    public static CupFighter bot; // use for cup mode

    public static void GenerateCupBotData(Fighter player, Fighter bot)
    {
        CupFighter cupBot = GetCupBotData();
        string botName = cupBot.fighterName;
        SpeciesNames botSpecies = (SpeciesNames)Enum.Parse(typeof(SpeciesNames), cupBot.species);

        GenerateBotData(player, bot, botName, botSpecies);
    }

    public static void GenerateSoloQBotData(Fighter player, Fighter bot)
    {
        string botName = FetchBotRandomName();
        SpeciesNames randomSpecies = GetRandomSpecies();

        GenerateBotData(player, bot, botName, randomSpecies);
    }

    public static void GenerateBotData(Fighter player, Fighter bot, string botName, SpeciesNames botSpecies)
    {
        int botLevel = GenerateBotLevel(player.level);
        Combat.botElo = GenerateBotElo(User.Instance.elo);

        List<Skill> botSkills = new List<Skill>();

        //ADD ALL SKILLS
        //FIXME: Remove this in production
        // foreach (OrderedDictionary skill in SkillCollection.skills)
        // {
        //     Skill skillInstance = new Skill(skill["name"].ToString(), skill["description"].ToString(),
        //         skill["skillRarity"].ToString(), skill["category"].ToString(), skill["icon"].ToString());

        //     botSkills.Add(skillInstance);
        // }

        //Add random skills for the bot
        int skillCountBottomRange = player.skills.Count == 0 ? 0 : player.skills.Count - 1;
        int skillCountTopRange = player.skills.Count + 1 >= SkillCollection.skills.Count ? player.skills.Count : player.skills.Count + 2;

        int botSkillsCount = UnityEngine.Random.Range(skillCountBottomRange, skillCountTopRange);
        int randomSkillIndex = UnityEngine.Random.Range(0, SkillCollection.skills.Count);

        for (int i = 0; i < botSkillsCount; i++)
        {
            OrderedDictionary skill = SkillCollection.skills[randomSkillIndex];
            Skill skillInstance = new Skill(skill["name"].ToString(), skill["description"].ToString(),
                skill["skillRarity"].ToString(), skill["category"].ToString(), skill["icon"].ToString());

            botSkills.Add(skillInstance);
        }

        Dictionary<string, float> botStats = GenerateBotRandomStats(botSpecies);

        //weightedHealth to give the player a little advantadge
        float weightedHealth = UnityEngine.Random.Range(botStats["hp"] * 0.95f, botStats["hp"] * 1.03f);
        Debug.Log(weightedHealth);

        bot.FighterConstructor(botName, weightedHealth, botStats["damage"], botStats["speed"],
            botSpecies.ToString(), botSpecies.ToString(), botLevel, 0, botSkills);

        Debug.Log("BOT STATS -> hp: " + botStats["hp"] + " damage: " + botStats["damage"] + " speed: " + botStats["speed"]);
    }

    private static CupFighter GetCupBotData()
    {
        string cupBotId = "";
        int counter = 0;

        // player enemies will be on seed2, seed10, seed14
        if (Cup.Instance.round == CupDB.CupRounds.QUARTERS.ToString())
            cupBotId = Cup.Instance.cupInfo[CupDB.CupRounds.QUARTERS.ToString()]["1"]["2"];
        if (Cup.Instance.round == CupDB.CupRounds.SEMIS.ToString())
            cupBotId = Cup.Instance.cupInfo[CupDB.CupRounds.SEMIS.ToString()]["5"]["10"];
        if (Cup.Instance.round == CupDB.CupRounds.FINALS.ToString())
            cupBotId = Cup.Instance.cupInfo[CupDB.CupRounds.FINALS.ToString()]["7"]["14"];

        for (int i = 0; i < Cup.Instance.participants.Count; i++)
        {
            if (cupBotId == Cup.Instance.participants[counter].id)
                return Cup.Instance.participants[counter];
            counter++;
        }

        Debug.Log("Couldn't get fighter!");
        return new CupFighter("", "", "");
    }

    private static Dictionary<string, float> GenerateBotRandomStats(SpeciesNames randomSpecies)
    {
        float hp = Species.defaultStats[randomSpecies]["hp"] + (Species.statsPerLevel[randomSpecies]["hp"] * (Combat.player.level - 1));
        float damage = Species.defaultStats[randomSpecies]["damage"] + (Species.statsPerLevel[randomSpecies]["damage"] * (Combat.player.level - 1));
        float speed = Species.defaultStats[randomSpecies]["speed"] + (Species.statsPerLevel[randomSpecies]["speed"] * (Combat.player.level - 1));

        return new Dictionary<string, float>
        {
            {"hp", hp},
            {"damage", damage},
            {"speed", speed},
        };
    }
    private static SpeciesNames GetRandomSpecies()
    {
        System.Random random = new System.Random();
        Array species = Enum.GetValues(typeof(SpeciesNames));
        return (SpeciesNames)species.GetValue(random.Next(species.Length));
    }
    private static string FetchBotRandomName()
    {
        return RandomNameGenerator.GenerateRandomName();
    }

    private static int GenerateBotElo(int playerElo)
    {
        int botElo = UnityEngine.Random.Range(playerElo - 50, playerElo + 50);
        return botElo >= 0 ? botElo : 0;
    }
    private static int GenerateBotLevel(int playerLevel)
    {
        return playerLevel > 1 ? UnityEngine.Random.Range(playerLevel - 1, playerLevel + 2) : playerLevel;
    }

    public static int CalculateEloChange(int playerElo, int botElo, bool isPlayerWinner)
    {
        int eloDifference = botElo - playerElo;
        int eloPonderance = eloDifference / 10;
        int absoluteEloChange = baseEloGain + eloPonderance;
        int modifierToRankUpOverTime = 2;
        int eloChange = isPlayerWinner ? absoluteEloChange : -absoluteEloChange + modifierToRankUpOverTime;
        if (playerElo + eloChange < 0) return -playerElo;
        return eloChange;
    }
}