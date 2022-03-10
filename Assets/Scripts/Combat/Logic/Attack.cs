using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Attack : MonoBehaviour
{
    public GameObject shuriken;
    public IEnumerator PerformAttack(Fighter attacker, Fighter defender)
    {
        if (Combat.movementScript.FighterShouldAdvanceToAttack(attacker)) yield return StartCoroutine(Combat.movementScript.MoveToMeleeRangeAgain(attacker, defender));

        FighterAnimations.ChangeAnimation(attacker, FighterAnimations.AnimationNames.ATTACK);

        if (IsAttackDodged(defender))
        {
            yield return DefenderDodgesAttack(defender);
            yield break;
        }

        yield return DefenderReceivesAttack(attacker, defender, 0.25f);
    }

    public IEnumerator PerformCosmicKicks(Fighter attacker, Fighter defender)
    {
        FighterAnimations.ChangeAnimation(attacker, FighterAnimations.AnimationNames.KICK);
        yield return DefenderReceivesAttack(attacker, defender, 0.1f);
    }
    public IEnumerator PerformShurikenFury(Fighter attacker, Fighter defender)
    {
        bool dodged = IsAttackDodged(defender);

        Vector3 shurikenStartPos = attacker.transform.position;
        Vector3 shurikenEndPos = defender.transform.position;
        shurikenStartPos.y -= 0.7f;
        shurikenEndPos.y -= 0.7f;
        //If the attack is dodged throw the shuriken much further away
        shurikenEndPos.x = dodged ? Combat.player == attacker ? shurikenEndPos.x + 10 : shurikenEndPos.x - 10 : shurikenEndPos.x;

        FighterAnimations.ChangeAnimation(attacker, FighterAnimations.AnimationNames.THROW);
        //Throw the shuriken when the fighter arm is already up
        yield return new WaitForSeconds(.1f);

        GameObject shurikenInstance = Instantiate(shuriken, shurikenStartPos, Quaternion.identity);

        if (dodged)
        {
            StartCoroutine(Combat.movementScript.MoveShuriken(shurikenInstance, shurikenStartPos, shurikenEndPos, 0.25f));
            //Wait for the shuriken to approach before jumping
            yield return new WaitForSeconds(.1f);
            yield return DefenderDodgesAttack(defender);
            Destroy(shurikenInstance);
            yield break;
        }

        yield return StartCoroutine(Combat.movementScript.MoveShuriken(shurikenInstance, shurikenStartPos, shurikenEndPos, 0.25f));
        Destroy(shurikenInstance);
        yield return DefenderReceivesAttack(attacker, defender, 0.25f);
    }

    IEnumerator DefenderDodgesAttack(Fighter defender)
    {
        StartCoroutine(Combat.movementScript.DodgeMovement(defender));
        FighterAnimations.ChangeAnimation(defender, FighterAnimations.AnimationNames.JUMP);
        yield return new WaitForSeconds(.3f);
        FighterAnimations.ChangeAnimation(defender, FighterAnimations.AnimationNames.IDLE);
    }

    IEnumerator DefenderReceivesAttack(Fighter attacker, Fighter defender, float secondsToWaitForHurtAnim)
    {
        DealDamage(attacker, defender);

        Combat.isGameOver = defender.hp <= 0 ? true : false;

        if (Combat.isGameOver)
        {
            FighterAnimations.ChangeAnimation(defender, FighterAnimations.AnimationNames.DEATH);
            yield return StartCoroutine(ReceiveDamageAnimation(defender));
        }
        else
        {
            FighterAnimations.ChangeAnimation(defender, FighterAnimations.AnimationNames.HURT);
            yield return StartCoroutine(ReceiveDamageAnimation(defender));
            yield return new WaitForSeconds(secondsToWaitForHurtAnim);
            FighterAnimations.ChangeAnimation(defender, FighterAnimations.AnimationNames.IDLE);
        }
    }

    private void DealDamage(Fighter attacker, Fighter defender)
    {
        var attackerDamageForNextHit = IsAttackCritical(attacker) ? attacker.damage * 2 : attacker.damage;
        defender.hp -= attackerDamageForNextHit;
        Combat.fightersUIDataScript.ModifyHealthBar(defender, Combat.player == defender);
    }

    IEnumerator ReceiveDamageAnimation(Fighter defender)
    {
        yield return new WaitForSeconds(.05f);
        Renderer defenderRenderer = defender.GetComponent<Renderer>();
        defenderRenderer.material.color = new Color(255, 1, 1);
        yield return new WaitForSeconds(.08f);
        defenderRenderer.material.color = new Color(1, 1, 1);
    }

    public bool IsAttackRepeated(Fighter attacker)
    {
        return Probabilities.IsHappening(attacker.repeatAttackChance);
    }

    private bool IsAttackDodged(Fighter defender)
    {
        return Probabilities.IsHappening(defender.dodgeChance);
    }

    private bool IsAttackCritical(Fighter attacker)
    {
        return Probabilities.IsHappening(attacker.criticalChance);
    }
}