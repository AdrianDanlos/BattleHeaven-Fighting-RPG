using System.Collections;
using System;
using UnityEngine;

public class SkillsLogicInCombat : MonoBehaviour
{
    private Combat combatScript;
    private Movement movementScript;
    private Attack attackScript;
    private void Awake()
    {
        combatScript = this.GetComponent<Combat>();
        movementScript = this.GetComponent<Movement>();
        attackScript = this.GetComponent<Attack>();
    }
    public IEnumerator AttackWithoutSkills(Fighter attacker, Fighter defender)
    {
        yield return combatScript.MoveForwardHandler(attacker);

        // Attack
        int attackCounter = 0;

        while (!Combat.isGameOver && (attackCounter == 0 || attackScript.IsAttackRepeated(attacker)))
        {
            yield return StartCoroutine(attackScript.PerformAttack(attacker, defender));
            attackCounter++;
        };

        if (!Combat.isGameOver) FighterAnimations.ChangeAnimation(defender, FighterAnimations.AnimationNames.IDLE);

        yield return combatScript.MoveBackHandler(attacker);
    }
    public IEnumerator LowBlow(Fighter attacker, Fighter defender)
    {
        combatScript.SetFightersDestinationPositions(0.8f);
        FighterAnimations.ChangeAnimation(attacker, FighterAnimations.AnimationNames.RUN);
        yield return movementScript.MoveSlide(attacker);
        yield return StartCoroutine(attackScript.PerformLowBlow(attacker, defender));
        yield return combatScript.MoveBackHandler(attacker);
        combatScript.ResetFightersDestinationPosition();
    }

    public IEnumerator JumpStrike(Fighter attacker, Fighter defender)
    {
        combatScript.SetFightersDestinationPositions(1f);
        FighterAnimations.ChangeAnimation(attacker, FighterAnimations.AnimationNames.RUN);

        yield return movementScript.MoveJumpStrike(attacker);

        float rotationDegrees = attacker == Combat.player ? -35f : 35f;
        movementScript.Rotate(attacker, rotationDegrees);

        int nStrikes = UnityEngine.Random.Range(4, 9); // 4-8 attacks

        for (int i = 0; i < nStrikes && !Combat.isGameOver; i++)
        {
            yield return StartCoroutine(attackScript.PerformJumpStrike(attacker, defender));
        }

        if (!Combat.isGameOver) FighterAnimations.ChangeAnimation(defender, FighterAnimations.AnimationNames.IDLE);

        //Go back to the ground
        yield return StartCoroutine(movementScript.Move(attacker, attacker.transform.position, attacker.destinationPosition, 0.1f));
        movementScript.ResetRotation(attacker);

        yield return combatScript.MoveBackHandler(attacker);
        combatScript.ResetFightersDestinationPosition();
    }

    public IEnumerator ShurikenFury(Fighter attacker, Fighter defender)
    {
        int nShurikens = UnityEngine.Random.Range(4, 9); // 4-8 shurikens

        for (int i = 0; i < nShurikens && !Combat.isGameOver; i++)
        {
            yield return StartCoroutine(attackScript.PerformShurikenFury(attacker, defender));
        }

        if (!Combat.isGameOver) FighterAnimations.ChangeAnimation(defender, FighterAnimations.AnimationNames.IDLE);
    }

    public IEnumerator CosmicKicks(Fighter attacker, Fighter defender)
    {
        combatScript.SetFightersDestinationPositions(1.5f);
        yield return combatScript.MoveForwardHandler(attacker);

        int nKicks = UnityEngine.Random.Range(4, 9); // 4-8 kicks

        for (int i = 0; i < nKicks && !Combat.isGameOver; i++)
        {
            yield return StartCoroutine(attackScript.PerformCosmicKicks(attacker, defender));
        }

        if (!Combat.isGameOver) FighterAnimations.ChangeAnimation(defender, FighterAnimations.AnimationNames.IDLE);

        yield return combatScript.MoveBackHandler(attacker);
        combatScript.ResetFightersDestinationPosition();
    }
    public IEnumerator ExplosiveBomb(Fighter attacker, Fighter defender)
    {
        yield return StartCoroutine(attackScript.PerformExplosiveBomb(attacker, defender));
        if (!Combat.isGameOver) FighterAnimations.ChangeAnimation(defender, FighterAnimations.AnimationNames.IDLE);
    }
    public IEnumerator InterdimensionalTravel(Fighter attacker, Fighter defender)
    {
        FighterAnimations.ChangeAnimation(attacker, FighterAnimations.AnimationNames.IDLE_BLINKING);
        //Wait for blinking animation to finish
        yield return new WaitForSeconds(1.2f);
        attacker.GetComponent<Renderer>().material.color = GetFighterColorWithCustomOpacity(attacker, 0);
        yield return combatScript.MoveForwardHandler(attacker);
        attacker.GetComponent<Renderer>().material.color = GetFighterColorWithCustomOpacity(attacker, 1);
        yield return StartCoroutine(attackScript.PerformAttack(attacker, defender));
        if (!Combat.isGameOver) FighterAnimations.ChangeAnimation(defender, FighterAnimations.AnimationNames.IDLE);
        yield return combatScript.MoveBackHandler(attacker);
    }
    private Color GetFighterColorWithCustomOpacity(Fighter fighter, float opacity)
    {
        Color fighterColor = fighter.GetComponent<Renderer>().material.color;
        fighterColor.a = opacity;
        return fighterColor;
    }
}