using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using System;

public class Combat : MonoBehaviour
{
    // Data Objects
    public static Fighter player;
    public static Fighter bot;
    public int botElo;

    // GameObjects
    public static GameObject playerGameObject;
    public GameObject playerWrapper;
    public static GameObject botGameObject;
    public Canvas results;
    public SpriteRenderer arena;

    // Script references
    public static Movement movementScript;
    Attack attacktScript;
    public static FightersUIData fightersUIDataScript;

    // Positions data
    static Vector3 playerStartingPosition = new Vector3(-6, -0.7f, 0);
    static Vector3 botStartingPosition = new Vector3(6, -0.7f, 0);
    public const float DefaultDistanceFromEachotherOnAttack = 2.3f;

    // Game status data
    public static bool isGameOver;
    List<Fighter> fightersOrderOfAttack = new List<Fighter> { };
    public static float playerMaxHp;
    public static float botMaxHp;

    private void Awake()
    {
        // From the current gameobject (this) access the movement component which is a script.
        movementScript = this.GetComponent<Movement>();
        attacktScript = this.GetComponent<Attack>();
        fightersUIDataScript = this.GetComponent<FightersUIData>();
        isGameOver = false;
    }

    void Start()
    {
        FindGameObjects();
        SetVisibilityOfGameObjects();
        GetFighterScriptComponents();
        GenerateBotData();
        SetFighterPositions();
        SetOrderOfAttacks();
        GetRandomArena();
        FighterSkin.SetFightersSkin(player, bot);
        FighterAnimations.ResetToDefaultAnimation(player);
        fightersUIDataScript.SetFightersUIInfo(player, bot, botElo);
        SetMaxHpValues();
        
        StartCoroutine(InitiateCombat());
    }

    private void SetMaxHpValues()
    {
        playerMaxHp = player.hp;
        botMaxHp = bot.hp;
    }

    private void GetFighterScriptComponents()
    {
        player = playerGameObject.GetComponent<Fighter>();
        bot = botGameObject.GetComponent<Fighter>();
    }
    private void GetRandomArena()
    {
        Sprite[] arenas = Resources.LoadAll<Sprite>("Arenas/");
        int chosenArena = UnityEngine.Random.Range(0, arenas.Length);
        arena.sprite = arenas[chosenArena];
    }

    private void SetVisibilityOfGameObjects()
    {
        playerGameObject.SetActive(true);
    }
    private void FindGameObjects()
    {
        playerWrapper = GameObject.Find("FighterWrapper");
        playerGameObject = playerWrapper.transform.Find("Fighter").gameObject;
        botGameObject = GameObject.Find("Bot");
        results = GameObject.FindGameObjectWithTag("Results").GetComponent<Canvas>();
        arena = GameObject.FindGameObjectWithTag("Arena").GetComponent<SpriteRenderer>();
    }

    private void SetFighterPositions()
    {
        //Set GameObjects
        playerGameObject.transform.position = playerStartingPosition;
        botGameObject.transform.position = botStartingPosition;

        //Set Objects
        player.initialPosition = playerStartingPosition;
        bot.initialPosition = botStartingPosition;

        SetFightersDestinationPositions(DefaultDistanceFromEachotherOnAttack);
    }

    private void SetFightersDestinationPositions(float distanceAwayFromEachOtherOnAttack)
    {
        Vector3 playerDestinationPosition = botStartingPosition;
        Vector3 botDestinationPosition = playerStartingPosition;

        playerDestinationPosition.x -= distanceAwayFromEachOtherOnAttack;
        player.destinationPosition = playerDestinationPosition;

        botDestinationPosition.x += distanceAwayFromEachOtherOnAttack;
        bot.destinationPosition = botDestinationPosition;
    }

    IEnumerator InitiateCombat()
    {
        Fighter firstAttacker = fightersOrderOfAttack[0];
        Fighter secondAttacker = fightersOrderOfAttack[1];

        //1 loop = 1 turn (both players attacking)
        while (!isGameOver)
        {
            // The StartTurn method should handle all the actions of a player for that turn. E.G. Move, Attack, Throw skill....
            yield return StartCoroutine(StartTurn(firstAttacker, secondAttacker));
            if (isGameOver) break;
            yield return StartCoroutine(StartTurn(secondAttacker, firstAttacker));
        }
        StartPostGameActions();
    }

    private void GenerateBotData()
    {
        string botName = MatchMaking.FetchBotRandomName();
        botElo = MatchMaking.GenerateBotElo(User.Instance.elo);

        List<Skill> botSkills = new List<Skill>();

        //ADD ALL SKILLS
        foreach (OrderedDictionary skill in SkillCollection.skills)
        {
            Skill skillInstance = new Skill(skill["name"].ToString(), skill["description"].ToString(),
                skill["rarity"].ToString(), skill["category"].ToString(), skill["icon"].ToString());

            botSkills.Add(skillInstance);
        }

        SpeciesNames randomSpecies = GetRandomSpecies();

        Dictionary<string, float> botStats = GenerateBotRandomStats(randomSpecies);

        bot.FighterConstructor(botName, botStats["hp"], botStats["damage"], botStats["speed"],
            randomSpecies.ToString(), randomSpecies.ToString(), 1, 0, botSkills);

        //FIXME: We should remove the skin concept from the fighters and use the species name for the skin.
    }

    private Dictionary<string, float> GenerateBotRandomStats(SpeciesNames randomSpecies)
    {
        float hp = Species.defaultStats[randomSpecies]["hp"] + (Species.statsPerLevel[randomSpecies]["hp"] * player.level);
        float damage = Species.defaultStats[randomSpecies]["damage"] + (Species.statsPerLevel[randomSpecies]["damage"] * player.level);
        float speed = Species.defaultStats[randomSpecies]["speed"] + (Species.statsPerLevel[randomSpecies]["speed"] * player.level);

        return new Dictionary<string, float>
        {
            {"hp", hp},
            {"damage", damage},
            {"speed", speed},
        };
    }
    private SpeciesNames GetRandomSpecies()
    {
        System.Random random = new System.Random();
        Array species = Enum.GetValues(typeof(SpeciesNames));
        return (SpeciesNames)species.GetValue(random.Next(species.Length));
    }

    IEnumerator StartTurn(Fighter attacker, Fighter defender)
    {
        yield return JumpStrike(attacker, defender);
        // if (WillUseSkillThisTurn())
        // {
        //     yield return JumpStrike(attacker, defender);
        //     yield return CosmicKicks(attacker, defender);
        //     yield return ShurikenFury(attacker, defender);
        //     yield return LowBlow(attacker, defender);
        //     yield break;
        // }
        // yield return AttackWithoutSkills(attacker, defender);
    }

    private bool WillUseSkillThisTurn()
    {
        int probabilityOfUsingSkillEachTurn = 70;
        return Probabilities.IsHappening(probabilityOfUsingSkillEachTurn);
    }

    //TODO: Move this functions to a different file that handles skills logic
    IEnumerator LowBlow(Fighter attacker, Fighter defender)
    {
        SetFightersDestinationPositions(0.8f);
        FighterAnimations.ChangeAnimation(attacker, FighterAnimations.AnimationNames.RUN);
        yield return movementScript.MoveSlide(attacker);
        yield return StartCoroutine(attacktScript.PerformLowBlow(attacker, defender));
        yield return MoveBackHandler(attacker);
        //FIXME: Create a helper called reset destination positions
        SetFightersDestinationPositions(DefaultDistanceFromEachotherOnAttack);
    }
    IEnumerator JumpStrike(Fighter attacker, Fighter defender)
    {
        SetFightersDestinationPositions(1f);
        FighterAnimations.ChangeAnimation(attacker, FighterAnimations.AnimationNames.RUN);

        yield return movementScript.MoveJumpStrike(attacker);

        float rotationDegrees = attacker == player ? -35f : 35f;
        movementScript.Rotate(attacker, rotationDegrees);

        int nStrikes = UnityEngine.Random.Range(4, 9); // 4-8 attacks

        for (int i = 0; i < nStrikes && !isGameOver; i++)
        {
            yield return StartCoroutine(attacktScript.PerformJumpStrike(attacker, defender));
        }

        if (!isGameOver) FighterAnimations.ChangeAnimation(defender, FighterAnimations.AnimationNames.IDLE);

        //Go back to the ground
        yield return StartCoroutine(movementScript.Move(attacker, attacker.transform.position, attacker.destinationPosition, 0.1f));
        movementScript.ResetRotation(attacker);

        yield return MoveBackHandler(attacker);
        SetFightersDestinationPositions(DefaultDistanceFromEachotherOnAttack);
    }
    IEnumerator ShurikenFury(Fighter attacker, Fighter defender)
    {
        int nShurikens = UnityEngine.Random.Range(4, 9); // 4-8 shurikens

        for (int i = 0; i < nShurikens && !isGameOver; i++)
        {
            yield return StartCoroutine(attacktScript.PerformShurikenFury(attacker, defender));
        }

        if (!isGameOver) FighterAnimations.ChangeAnimation(defender, FighterAnimations.AnimationNames.IDLE);
    }

    IEnumerator CosmicKicks(Fighter attacker, Fighter defender)
    {
        SetFightersDestinationPositions(1.5f);
        yield return MoveForwardHandler(attacker);

        int nKicks = UnityEngine.Random.Range(4, 9); // 4-8 kicks

        for (int i = 0; i < nKicks && !isGameOver; i++)
        {
            yield return StartCoroutine(attacktScript.PerformCosmicKicks(attacker, defender));
        }

        if (!isGameOver) FighterAnimations.ChangeAnimation(defender, FighterAnimations.AnimationNames.IDLE);

        yield return MoveBackHandler(attacker);
        SetFightersDestinationPositions(DefaultDistanceFromEachotherOnAttack);
    }

    IEnumerator AttackWithoutSkills(Fighter attacker, Fighter defender)
    {
        yield return MoveForwardHandler(attacker);

        // Attack
        int attackCounter = 0;

        while (!isGameOver && (attackCounter == 0 || attacktScript.IsAttackRepeated(attacker)))
        {
            yield return StartCoroutine(attacktScript.PerformAttack(attacker, defender));
            attackCounter++;
        };

        if (!isGameOver) FighterAnimations.ChangeAnimation(defender, FighterAnimations.AnimationNames.IDLE);

        yield return MoveBackHandler(attacker);
    }

    IEnumerator MoveForwardHandler(Fighter attacker)
    {
        FighterAnimations.ChangeAnimation(attacker, FighterAnimations.AnimationNames.RUN);
        yield return StartCoroutine(movementScript.MoveForward(attacker, attacker.destinationPosition));
    }

    IEnumerator MoveBackHandler(Fighter attacker)
    {
        FighterAnimations.ChangeAnimation(attacker, FighterAnimations.AnimationNames.RUN);
        FighterSkin.SwitchFighterOrientation(attacker.GetComponent<SpriteRenderer>());
        yield return StartCoroutine(movementScript.MoveBack(attacker, attacker.initialPosition));
        FighterSkin.SwitchFighterOrientation(attacker.GetComponent<SpriteRenderer>());
        FighterAnimations.ChangeAnimation(attacker, FighterAnimations.AnimationNames.IDLE);
    }

    // This method creates a dictionary with the Fighter class objects sorted by their speeds to get the order of attack.
    // Higher speeds will get sorted first
    private void SetOrderOfAttacks()
    {
        OrderedDictionary fighterDictWithSpeed = new OrderedDictionary
        {
            {player, player.speed},
            {bot, bot.speed},
        };

        var fighterDictSortedBySpeed = fighterDictWithSpeed.Cast<DictionaryEntry>()
                       .OrderByDescending(r => r.Value)
                       .ToDictionary(c => c.Key, d => d.Value);

        foreach (var fighter in fighterDictSortedBySpeed)
        {
            fightersOrderOfAttack.Add((Fighter)fighter.Key);
        }
    }

    private void StartPostGameActions()
    {
        bool isPlayerWinner = PostGameActions.HasPlayerWon(player);
        int eloChange = MatchMaking.CalculateEloChange(User.Instance.elo, botElo, isPlayerWinner);
        int playerUpdatedExperience = player.experiencePoints + Levels.GetXpGain(isPlayerWinner);
        bool isLevelUp = Levels.IsLevelUp(player.level, playerUpdatedExperience);
        int goldReward = PostGameActions.GoldReward(isPlayerWinner);
        int gemsReward = PostGameActions.GemsReward();

        //PlayerData
        PostGameActions.SetElo(eloChange);
        PostGameActions.SetWinLoseCounter(isPlayerWinner);
        PostGameActions.SetExperience(player, isPlayerWinner);
        if (isLevelUp) PostGameActions.SetLevelUpSideEffects(player);
        EnergyManager.SubtractOneEnergyPoint();

        //Rewards
        PostGameActions.SetCurrencies(goldReward, gemsReward);

        //UI
        fightersUIDataScript.SetResultsBanner(isPlayerWinner);
        fightersUIDataScript.SetResultsEloChange(eloChange);
        fightersUIDataScript.SetResultsLevel(player.level, player.experiencePoints);
        fightersUIDataScript.SetResultsExpGainText(isPlayerWinner);
        fightersUIDataScript.ShowLevelUpIcon(isLevelUp);
        fightersUIDataScript.ShowRewards(goldReward, gemsReward, isLevelUp);
        fightersUIDataScript.EnableResults(results);

        //Save
        PostGameActions.ResetPlayerHp(playerMaxHp);
        PostGameActions.Save(player);
    }
}
