using System.Collections;
using System.Collections.Generic;
using Aarthificial.Reanimation;
using UnityEngine;

public class PlayerRenderer : MonoBehaviour
{
    private static class Drivers //used to control ReAnimator
    {
        public const string IsGrounded = "isGrounded";
        public const string GroundMovement = "groundMovement";
        public const string JumpDirection = "jumpDirection";
        public const string State = "state";
        public const string IsRunJumping = "isRunJumping";
        public const string IsJumping = "isJumping";
        public const string AttackStyle = "attackStyle";
        public const string AttackNum = "attackNum";
        public const string BasicHitState = "basicHitState";
        public const string Jump = "jump";
    }

    //variables
    private Player controller;
    private Reanimator reanimator;

    private void Awake()
    {
        controller = GetComponent<Player>();
        reanimator = GetComponent<Reanimator>();
        reanimator.AddListener ("healFinished", () => controller.HealingTime = 0);
    }

    private void Update()
    {
        //reanimator.Flip = controller.facingRight;
        reanimator.Set(Drivers.State, (int)controller.State);
        reanimator.Set(Drivers.IsGrounded, controller.Grounded);
        reanimator.Set(Drivers.GroundMovement, controller.Direction.x != 0);
        reanimator.Set(Drivers.JumpDirection, controller.rb.velocity.y > 0);
        reanimator.Set(Drivers.IsRunJumping, controller.rb.velocity.x != 0);
        reanimator.Set(Drivers.IsJumping, controller.Walled);
        reanimator.Set(Drivers.AttackStyle, controller.AttackStyle);
        reanimator.Set(Drivers.AttackNum, controller.DoubleAtk);
        reanimator.Set(Drivers.BasicHitState, controller.AttackingDirection);
        if (controller.Healing && !controller.HealCancelled && controller.HealingTime > Time.time) { reanimator.Set(Drivers.GroundMovement, 2); } //sort out animation if heal is cancelled
    }


}
