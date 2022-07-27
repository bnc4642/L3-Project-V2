using System.Collections;
using System.Collections.Generic;
using Aarthificial.Reanimation;
using UnityEngine;

public class PlayerRenderer : MonoBehaviour
{
    private int attackDirection = 0;
    private static class Drivers
    {
        public const string IsGrounded = "isGrounded";
        public const string IsMoving = "isMoving";
        public const string JumpDirection = "jumpDirection";
        public const string State = "state";
        public const string IsRunJumping = "isRunJumping";
        public const string AttackDirection = "attackDirection";
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
        if (controller.State != PlayerState.Attack)
        {
            if (controller.direction.y < 0 && !controller.grounded)
                attackDirection = 0;
            else if (controller.direction.y > 0)
                attackDirection = 2;
            else
                attackDirection = 1;
        }
        //reanimator.Flip = controller.facingRight;
        reanimator.Set(Drivers.State, (int)controller.State);
        reanimator.Set(Drivers.IsGrounded, controller.grounded);
        reanimator.Set(Drivers.IsMoving, controller.direction.x != 0);
        reanimator.Set(Drivers.JumpDirection, controller.rb.velocity.y > 0);
        reanimator.Set(Drivers.IsRunJumping, controller.rb.velocity.x != 0);
        reanimator.Set(Drivers.AttackDirection, attackDirection);
    }


}
