using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class Mosquito : Enemy
{

    public Mosquito() //small custom features
    {
        health = 10;
        damagePush = 15;
    }

    //more custom features will need to be added when the enemy script becomes more generalized
}
