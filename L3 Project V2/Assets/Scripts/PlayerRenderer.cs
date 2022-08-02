using System.Collections;
using System.Collections.Generic;
using Aarthificial.Reanimation;
using UnityEngine;

public class PlayerRenderer : MonoBehaviour
{
    private static class Drivers
    {
        public const string IsGrounded = "isGrounded";
        public const string IsMoving = "isMoving";
        public const string JumpDirection = "jumpDirection";
        public const string State = "state";
        public const string IsRunJumping = "isRunJumping";
        public const string AttackStyle = "attackStyle";
        public const string BasicHitState = "basicHitState";
    }

    private Player controller;
    private Reanimator reanimator;
    private void Awake()
    {
        controller = GetComponent<Player>();
        reanimator = GetComponent<Reanimator>();
    }

    private void Update()
    {
        //reanimator.Flip = controller.facingRight;
        reanimator.Set(Drivers.State, (int)controller.State);
        reanimator.Set(Drivers.IsGrounded, controller.grounded);
        reanimator.Set(Drivers.IsMoving, controller.direction.x != 0);
        reanimator.Set(Drivers.JumpDirection, controller.rb.velocity.y > 0);
        reanimator.Set(Drivers.IsRunJumping, controller.rb.velocity.x != 0);
        reanimator.Set(Drivers.AttackStyle, controller.attackStyle);
        reanimator.Set(Drivers.BasicHitState, controller.attackingDirection);
    }


}
