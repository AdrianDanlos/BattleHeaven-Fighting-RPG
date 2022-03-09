using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FighterAnimations : MonoBehaviour
{
    public enum AnimationNames
    {
        IDLE,
        RUN,
        ATTACK,
        JUMP,
        DEATH,
        HURT,
        KICK,
    }

    public static void ChangeAnimation(Fighter fighter, AnimationNames newAnimation)
    {
        fighter.GetComponent<Animator>().Play(newAnimation.ToString(), -1, 0f);
        fighter.currentAnimation = newAnimation.ToString();
    }

    public static void ResetToDefaultAnimation(Fighter player)
    {
        ChangeAnimation(player, FighterAnimations.AnimationNames.IDLE);
    }
}