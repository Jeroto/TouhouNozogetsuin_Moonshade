using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingShots : BaseBulletScript
{
    [Space]
    public float homingDist;
    public Transform target;

    int targetCooldown;

    protected override void AdditionalFunctions()
    {
        if (targetCooldown <= 0)
        {
            Transform enemy;
            float shortestDist = int.MaxValue;
            float currentDist;
            for (int i = 0; i < shmupMaster.enemyContainer.childCount; i++)
            {
                enemy = shmupMaster.enemyContainer.GetChild(i);
                currentDist = Vector2.Distance(enemy.position, thisTrans.position);
                if (currentDist < shortestDist)
                {
                    target = enemy;
                    shortestDist = currentDist;
                }
            }

            targetCooldown = 5;
        }
        else
            targetCooldown--;

        if(target != null && Vector2.Distance(target.position, thisTrans.position) < homingDist)
            movementDirection = MathFunctions.FindDirectionToObject(thisTrans.position, target);
    }
}