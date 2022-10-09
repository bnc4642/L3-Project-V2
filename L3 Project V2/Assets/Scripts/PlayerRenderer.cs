using System.Collections;
using System.Collections.Generic;
using Aarthificial.Reanimation;
using UnityEngine;

public class PlayerRenderer : MonoBehaviour
{
    private static class Drivers
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

    private Player controller;
    private Reanimator reanimator;
    private ReanimatorListener healListener;
    private void Awake()
    {
        controller = GetComponent<Player>();
        reanimator = GetComponent<Reanimator>();
        reanimator.AddListener (
        "healFinished",
        () => controller.healingTime = 0
        );
    }

    private void Update()
    {
        //reanimator.Flip = controller.facingRight;
        reanimator.Set(Drivers.State, (int)controller.State);
        reanimator.Set(Drivers.IsGrounded, controller.grounded);
        reanimator.Set(Drivers.GroundMovement, controller.direction.x != 0);
        if (controller.healing && !controller.healCancelled && controller.healingTime > Time.time) reanimator.Set(Drivers.GroundMovement, 2);
        reanimator.Set(Drivers.JumpDirection, controller.rb.velocity.y > 0);
        reanimator.Set(Drivers.IsRunJumping, controller.rb.velocity.x != 0);
        reanimator.Set(Drivers.IsJumping, controller.walled);
        reanimator.Set(Drivers.AttackStyle, controller.attackStyle);
        reanimator.Set(Drivers.AttackNum, controller.doubleAtk);
        reanimator.Set(Drivers.BasicHitState, controller.attackingDirection);
    }


}
