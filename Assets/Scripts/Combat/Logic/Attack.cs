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

        if (IsAttackShielded())
        {
            yield return StartCoroutine(ShieldAttack(defender));
            yield break;
        }

        if (IsAttackDodged(defender))
        {
            yield return DefenderDodgesAttack(defender);
            yield break;
        }

        yield return DefenderReceivesAttack(attacker, defender, attacker.damage, 0.25f, 0.05f);
    }

    IEnumerator ShieldAttack(Fighter defender, float secondsToWaitForAttackAnim = 0.35f)
    {
        FighterAnimations.ChangeAnimation(defender, FighterAnimations.AnimationNames.JUMP);
        Transform shield = defender.transform.Find("Shield");
        SpriteRenderer shieldSprite = shield.GetComponent<SpriteRenderer>();
        float xShieldDisplacement = Combat.player == defender ? 0.8f : -0.8f;
        Vector3 shieldDisplacement = new Vector3(xShieldDisplacement, -0.7f, 0);

        shield.transform.position = defender.transform.position;
        shield.transform.position += shieldDisplacement;
        shieldSprite.enabled = true;
        yield return new WaitForSeconds(secondsToWaitForAttackAnim);
        shieldSprite.enabled = false;
        FighterAnimations.ChangeAnimation(defender, FighterAnimations.AnimationNames.IDLE);
    }

    public IEnumerator PerformCosmicKicks(Fighter attacker, Fighter defender)
    {
        FighterAnimations.ChangeAnimation(attacker, FighterAnimations.AnimationNames.KICK);
        yield return DefenderReceivesAttack(attacker, defender, attacker.damage, 0.1f, 0.05f);
    }
    public IEnumerator PerformLowBlow(Fighter attacker, Fighter defender)
    {
        if (IsAttackShielded())
        {
            yield return StartCoroutine(ShieldAttack(defender, 0.22f));
            yield break;
        }

        if (IsAttackDodged(defender))
        {
            yield return DefenderDodgesAttack(defender);
            yield break;
        }

        yield return DefenderReceivesAttack(attacker, defender, attacker.damage, 0, 0);
    }
    public IEnumerator PerformJumpStrike(Fighter attacker, Fighter defender)
    {
        FighterAnimations.ChangeAnimation(attacker, FighterAnimations.AnimationNames.AIR_ATTACK);

        if (IsAttackShielded())
        {
            yield return StartCoroutine(ShieldAttack(defender));
            yield break;
        }

        yield return DefenderReceivesAttack(attacker, defender, attacker.damage, 0.15f, 0.05f);
        LifeSteal(attacker, 3);
        Combat.fightersUIDataScript.ModifyHealthBar(attacker, Combat.player == attacker);
    }
    public IEnumerator PerformShurikenFury(Fighter attacker, Fighter defender)
    {
        bool dodged = IsAttackDodged(defender);

        Vector3 shurikenStartPos = attacker.transform.position;
        Vector3 shurikenEndPos = defender.transform.position;
        shurikenStartPos.y -= 0.7f;
        shurikenEndPos.y -= 0.7f;
        shurikenEndPos.x = GetShurikenEndPositionX(dodged, attacker, shurikenEndPos);

        FighterAnimations.ChangeAnimation(attacker, FighterAnimations.AnimationNames.THROW);
        yield return new WaitForSeconds(.1f); //Throw the shuriken when the fighter arm is already up

        GameObject shurikenInstance = Instantiate(shuriken, shurikenStartPos, Quaternion.identity);

        if (dodged)
        {
            StartCoroutine(Combat.movementScript.RotateObjectOverTime(shurikenInstance, new Vector3(0, 0, 3000), 0.5f));
            StartCoroutine(Combat.movementScript.MoveShuriken(shurikenInstance, shurikenStartPos, shurikenEndPos, 0.5f)); //We dont yield here so we can jump mid animation
            yield return new WaitForSeconds(.2f); //Wait for the shuriken to approach before jumping
            yield return DefenderDodgesAttack(defender);
            yield return new WaitForSeconds(.2f); //Wait for the shuriken to be in its final position before destroying it (This could be avoided with colliders)
            Destroy(shurikenInstance);
            yield break;
        }

        StartCoroutine(Combat.movementScript.RotateObjectOverTime(shurikenInstance, new Vector3(0, 0, 2000), 0.35f));
        yield return StartCoroutine(Combat.movementScript.MoveShuriken(shurikenInstance, shurikenStartPos, shurikenEndPos, 0.35f));
        Destroy(shurikenInstance);

        if (IsAttackShielded())
        {
            yield return StartCoroutine(ShieldAttack(defender));
            yield break;
        }

        yield return DefenderReceivesAttack(attacker, defender, attacker.damage, 0.25f, 0);
    }

    private float GetShurikenEndPositionX(bool dodged, Fighter attacker, Vector3 shurikenEndPos)
    {
        if (dodged) return Combat.player == attacker ? shurikenEndPos.x + 10 : shurikenEndPos.x - 10;
        return Combat.player == attacker ? shurikenEndPos.x - 1f : shurikenEndPos.x + 1f; //To move the hitbox a bit upfront
    }


    IEnumerator DefenderDodgesAttack(Fighter defender)
    {
        StartCoroutine(Combat.movementScript.DodgeMovement(defender));
        FighterAnimations.ChangeAnimation(defender, FighterAnimations.AnimationNames.JUMP);
        yield return new WaitForSeconds(.3f);
        FighterAnimations.ChangeAnimation(defender, FighterAnimations.AnimationNames.IDLE);
    }

    IEnumerator DefenderReceivesAttack(Fighter attacker, Fighter defender, float damagePerHit, float secondsToWaitForHurtAnim, float secondsUntilHitMarker)
    {

        DealDamage(attacker, defender, damagePerHit);

        Combat.isGameOver = defender.hp <= 0 ? true : false;

        if (Combat.isGameOver)
        {
            FighterAnimations.ChangeAnimation(defender, FighterAnimations.AnimationNames.DEATH);
            yield return StartCoroutine(ReceiveDamageAnimation(defender, secondsUntilHitMarker));
            yield return new WaitForSeconds(.15f); //Wait for attack animation to finish
        }
        else
        {
            FighterAnimations.ChangeAnimation(defender, FighterAnimations.AnimationNames.HURT);
            yield return StartCoroutine(ReceiveDamageAnimation(defender, secondsUntilHitMarker));
            yield return new WaitForSeconds(secondsToWaitForHurtAnim);
        }
    }

    private void DealDamage(Fighter attacker, Fighter defender, float damagePerHit)
    {
        var attackerDamageForNextHit = IsAttackCritical(attacker) ? damagePerHit * 2 : damagePerHit;
        defender.hp -= attackerDamageForNextHit;
        Combat.fightersUIDataScript.ModifyHealthBar(defender, Combat.player == defender);
    }

    //Restores x % of missing health
    private void LifeSteal(Fighter attacker, int percentage)
    {
        bool isPlayerAttacking = Combat.player == attacker;
        float maxHp = isPlayerAttacking ? Combat.playerMaxHp : Combat.botMaxHp;
        float hpToRestore = percentage * maxHp / 100;
        float hpAfterLifesteal = attacker.hp + hpToRestore;
        attacker.hp = hpAfterLifesteal > maxHp ? maxHp : hpAfterLifesteal;
    }

    IEnumerator ReceiveDamageAnimation(Fighter defender, float secondsUntilHitMarker)
    {
        yield return new WaitForSeconds(secondsUntilHitMarker);
        Renderer defenderRenderer = defender.GetComponent<Renderer>();
        defenderRenderer.material.color = new Color(255, 1, 1);
        yield return new WaitForSeconds(.08f);
        defenderRenderer.material.color = new Color(1, 1, 1);
    }

    //FIXME: Only allow this if the fighter has the skill to perform it
    public bool IsAttackShielded()
    {
        int probabilityOfShielding = 100;
        return Probabilities.IsHappening(probabilityOfShielding);
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