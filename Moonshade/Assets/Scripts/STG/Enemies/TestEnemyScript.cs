using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEnemyScript : ShmupEnemyScript
{

    protected override void Patterns()
    {
        if(spellcard)
        {
            switch (patternIndex)
            {
                case 0:
                    ThirdEye();
                    break;

                case 1:

                    break;
            }
        }
        else
        {
            switch (patternIndex)
            {
                case 0:
                    SpawnOneMillionFuckingBullets();
                    break;
            }
        }
        
    }

    void SpawnOneMillionFuckingBullets()
    {
        if (bulletsFired.Length == 0)
        {
            bulletsFired = new int[1];
            bulletNextFireTime = new float[1];
        }

        RandomSmoothedMovement(100, 2, 120);

        if (bulletNextFireTime[0] <= 0)
        {
            Transform newBullet = Instantiate(nonData[0].bulletSpawnItems[0], thisTrans.position, new Quaternion(), shmupMaster.bulletContainer).transform;
            BaseBulletScript newScript = newBullet.GetComponent<BaseBulletScript>();
            newScript.ignoreRenderer = false;
            newScript.framesBetweenBulletReorientation = 1;
            newScript.turnAtStart = true;
            newScript.rotationBasedOnParent = false;
            newScript.spawnDelay = 15;
            newScript.updateSprites = true;
            newScript.regularKillzone = true;
            newScript.movementDirection = MathFunctions.CalculateCircle(bulletsFired[0] * 10);
            newScript.bulletSpeed = 1f;
            newScript.turnBullet = true;
            newScript.turnAfterWait = true;
            newScript.gradualSpeedChange = true;
            newScript.timeToSpeedUp = 15;
            newScript.startingBulletSpeed = 3;
            newScript.initialMoveDelay = 0;
            newScript.bulletSprite = (Sprite)nonData[0].misc[0];
            bulletsFired[0]++;
            if (bulletsFired[0] < 1000000000)
                bulletNextFireTime[0] += 1;
        }
        else
        {
            bulletNextFireTime[0] -= gameMaster.timeScale;
        }
    }

    void SpawnOneMillionBlueBullets()
    {
        RandomSmoothedMovement(100, 2, 120);

        if (bulletsFired.Length == 0)
        {
            bulletsFired = new int[1];
            bulletNextFireTime = new float[1];
        }

        if (bulletNextFireTime[0] <= 0)
        {
            Transform newBullet = Instantiate(nonData[1].bulletSpawnItems[0], thisTrans.position, new Quaternion(), shmupMaster.bulletContainer).transform;
            BaseBulletScript newScript = newBullet.GetComponent<BaseBulletScript>();
            newScript.ignoreRenderer = false;
            newScript.framesBetweenBulletReorientation = 1;
            newScript.turnAtStart = true;
            newScript.rotationBasedOnParent = false;
            newScript.spawnDelay = 15;
            newScript.updateSprites = true;
            newScript.regularKillzone = true;
            newScript.movementDirection = MathFunctions.CalculateCircle(bulletsFired[0] * -10);
            newScript.bulletSpeed = 1f;
            newScript.turnBullet = true;
            newScript.turnAfterWait = true;
            newScript.gradualSpeedChange = true;
            newScript.timeToSpeedUp = 15;
            newScript.startingBulletSpeed = 3;
            newScript.initialMoveDelay = 0;
            newScript.bulletSprite = (Sprite)nonData[1].misc[0];
            bulletsFired[0]++;
            if (bulletsFired[0] < 1000000000)
                bulletNextFireTime[0] += nonData[1].bulletSpawnDelay[0];
        }
        else
        {
            bulletNextFireTime[0] -= gameMaster.timeScale;
        }
    }

    void ThirdEye()
    {
        RandomSmoothedMovement(100, 2, 120);

        if (bulletsFired.Length == 0)
        {
            bulletsFired = new int[1];
            bulletNextFireTime = new float[1];
        }

        if (bulletNextFireTime[0] <= 0)
        {
            Transform newBullet = Instantiate(nonData[1].bulletSpawnItems[0], thisTrans.position, new Quaternion(), shmupMaster.bulletContainer).transform;
            BaseBulletScript newScript = newBullet.GetComponent<BaseBulletScript>();
            newScript.rotationBasedOnParent = false;
            newScript.spawnDelay = 15;
            newScript.updateSprites = true;
            newScript.regularKillzone = true;

            int frameDelay = 20;
            Vector2 directionToPlayer = MathFunctions.FindDirectionToSTGPlayer(thisTrans.position);
            Vector2 playerMovement = shmupMaster.playerTransform.GetComponent<ShmupChar>().movementVelocity * frameDelay;
            float angle = Vector2.SignedAngle(directionToPlayer, -playerMovement);
            float directionToPlayerLength = directionToPlayer.magnitude;
            float playerMovementLength = playerMovement.magnitude;

            if(playerMovementLength <= 0)
            {
                newScript.bulletSpeed = directionToPlayerLength;
                newScript.movementDirection = directionToPlayer;
            }
            else
            {
                newScript.bulletSpeed = Mathf.Sqrt(Mathf.Pow(directionToPlayerLength, 2) + Mathf.Pow(playerMovementLength, 2) -
                   (2 * directionToPlayerLength * playerMovementLength * Mathf.Cos(angle))) / frameDelay;


                Debug.Log(Mathf.Rad2Deg * Mathf.Asin((Mathf.Sin(Mathf.Deg2Rad * angle) / (newScript.bulletSpeed * frameDelay * 2)) * playerMovementLength));
                newScript.movementDirection = MathFunctions.CalculateCircle(MathFunctions.FindAngleToSTGPlayer(thisTrans.position) + (Mathf.Asin((Mathf.Sin(angle) / (newScript.bulletSpeed * frameDelay)) * playerMovementLength)));

            }



            newScript.turnBullet = true;
            newScript.turnAfterWait = true;
            newScript.gradualSpeedChange = true;
            newScript.timeToSpeedUp = 15;
            newScript.startingBulletSpeed = 3;
            newScript.initialMoveDelay = 0;
            newScript.bulletSprite = (Sprite)nonData[1].misc[0];
            bulletsFired[0]++;
            if (bulletsFired[0] < 1000000000)
                bulletNextFireTime[0] += nonData[1].bulletSpawnDelay[0];
        }
        else
        {
            bulletNextFireTime[0] -= gameMaster.timeScale;
        }
    }
}
