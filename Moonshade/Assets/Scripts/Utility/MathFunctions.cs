using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathFunctions {

    public static Vector3 RandomCircle(Vector3 center, float radius)
    {
        float ang = GameMasterScript.Random(0f, 1f) * 360;
        Vector3 pos;
        pos.x = center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
        pos.y = center.y + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
        pos.z = center.z;
        return pos;
    }

    public static Vector3 RandomCircle()
    {
        float ang = GameMasterScript.Random(0f, 1f) * 360;
        Vector3 pos;
        pos.x = Mathf.Sin(ang * Mathf.Deg2Rad);
        pos.y = Mathf.Cos(ang * Mathf.Deg2Rad);
        pos.z = 0;
        return pos;
    }

    public static Vector3 CalculateCircle(Vector3 center, float radius, float angle)
    {
        Vector3 pos;
        pos.x = center.x + radius * Mathf.Sin(angle * Mathf.Deg2Rad);
        pos.y = center.y + radius * Mathf.Cos(angle * Mathf.Deg2Rad);
        pos.z = center.z;
        return pos;
    }

    public static Vector2 CalculateCircle(float angle)
    {
        Vector2 pos;
        pos.x = Mathf.Sin(angle * Mathf.Deg2Rad);
        pos.y = Mathf.Cos(angle * Mathf.Deg2Rad);

        return pos;
    }

    public static Vector2 CalculateLocalCircle(Vector2 center, float radius, float angle)
    {
        Vector2 pos;
        pos.x = center.x + radius * Mathf.Sin(angle * Mathf.Deg2Rad);
        pos.y = center.y + radius * Mathf.Cos(angle * Mathf.Deg2Rad);
        return pos - center;
    }

    public static Vector2 FindDirectionToCenter(Vector2 startingPosition)
    {
        return (new Vector2(0, 224) - new Vector2(startingPosition.x, startingPosition.y)).normalized;
    }

    public static float FindAngleToCenter(Vector2 startingPosition)
    {
        Vector2 directionToCenter = FindDirectionToCenter(startingPosition);
        return Vector2.SignedAngle(directionToCenter, Vector2.up);
    }


    //Requires ShmupManager
    public static Vector2 FindDirectionToSTGPlayer(Vector2 startingPosition)
    {
        return (new Vector2(ShmupManager.shmupManager.playerTransform.position.x, ShmupManager.shmupManager.playerTransform.position.y) - new Vector2(startingPosition.x, startingPosition.y)).normalized;
    }

    public static float FindAngleToSTGPlayer(Vector2 startingPosition)
    {
        Vector2 directionToPlayer = FindDirectionToSTGPlayer(startingPosition);
        return Vector2.SignedAngle(directionToPlayer, Vector2.up);
    }
    
    public static Vector2 FindDirectionToObject(Vector2 startingPosition, Transform otherObject)
    {
        return (new Vector2(otherObject.position.x, otherObject.position.y) - new Vector2(startingPosition.x, startingPosition.y)).normalized;
    }

    public static Vector3 FindDirectionToObject3D(Vector3 startingPosition, Transform otherObject)
    {
        return (new Vector3(otherObject.position.x, 0, otherObject.position.z) - new Vector3(startingPosition.x, 0, startingPosition.z)).normalized;
    }

    public static float FindAngleToObject(Vector2 startingPosition, Transform otherObject)
    {
        Vector2 directionToObject = FindDirectionToObject(startingPosition, otherObject);
        return Vector2.SignedAngle(directionToObject, Vector2.up);
    }

    /*public static float FindAngleToObject3D(Vector3 startingPosition, Transform otherObject, Vector3 compareVector)
    {
        Vector2 directionToObject = FindDirectionToObjectFlat(startingPosition, otherObject);
        return Vector2.SignedAngle(directionToObject, compareVector);
    }*/

    public static float FindAngle(Vector2 startPoint, Vector2 endPoint)
    {
        Vector2 direction = endPoint - startPoint;
        return Vector2.SignedAngle(direction, Vector2.up);
    }

    public static float FindAngleBetween(Vector2 startPoint, Vector2 endPoint)
    {
        return Vector2.SignedAngle(startPoint, endPoint);
    }

    public static float FindAngle(Vector2 direction)
    {
        return Vector2.SignedAngle(Vector2.up, direction);
    }

}
