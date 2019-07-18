using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShmupEnemyScript : MonoBehaviour
{
    public static System.Random shmupEnemyRandom;

    protected float healthbarTickDist = (72 / 2) - 3;
    [SerializeField] protected GameObject healthbarSpawn;
    [SerializeField] protected GameObject healthbarTickSpawn;

    public Character.WeaknessAndResistance[] weaknessAndResistances;

    public string enemyName;
    [Space]

    public float healthbarSize = 1;
    public int patternIndex;
    public bool spellcard;
    public bool failedSpell;
    public float health;
    [Space]

    protected SpriteMask healthbar;
    protected GameObject[] phaseMarkerObjects;

    public int[] bulletsFired;
    public float[] bulletSpawnWait;
    public float[] bulletNextFireTime;
    [Space]

    public float patternInitialCountdown;
    public float spellcardTimeRemaining;
    public Vector2 movementPosition;
    public float waitTimeRemaining;
    public float currentTime;
    public int fromPositionIndex;
    public int toPositionIndex;
    [Space]
    [Space]
    public PatternData[] nonData;
    [Space]
    [Space]
    [Space]
    public PatternData[] spellData;
    [Space]
    [Space]

    protected Transform thisTrans;

    protected Vector2 movementSmoothing;
    protected uint level;
    protected GameMasterScript gameMaster;
    protected ShmupManager shmupMaster;
    protected Vector2 prevPos;


    [SerializeField] protected AudioClip deathSound;

    public Vector2 colliderSize;
    public Vector2 colliderOffset;

    protected Vector2 velocity;
    protected float nextMoveTime;
    protected float percentBetweenPositions;

    [SerializeField] protected Animator m_Anim;

    [SerializeField] protected Vector2 enemyOffscreenPosition;
    [SerializeField] protected Vector2 enemyDefaultPosition;
    public float betweenPositionsCurrentTime;

    public float deathAnimationTimeStart;
    public float deathAnimationTime;
    public int spawnedDeathEffects;
    
    public bool displaySpellcard;
    public bool spellcardEnd;

    protected bool hitByBomb;

    [System.Serializable]
    public struct PatternData
    {
        public string[] spellName;
        public float[] nameBoxLength;
        public bool spellcard;
        public Sprite characterPortrait;
        public bool survivalSpell;
        public bool killOnlySpell;
        public float startingHealth;
        public float[] phaseMarkers;
        public float spellcardTimeStart;
        [Space]
        public int[] bulletInts;
        public float[] bulletFloats;
        public float[] bulletSpawnDelay;
        public GameObject[] bulletSpawnItems;
        [Space]
        public GameObject[] effects;
        public AudioClip[] sfx;
        public Object[] misc;
    }

    [System.Serializable]
    public struct BossPosition
    {
        public Vector2 relativePoint;
        [HideInInspector]
        public Vector2 globalPoint;
        //[HideInInspector]
        public float time;
        public float speed;
        public float waitTime;
    }

    protected void Awake()
    {
        gameMaster = GameMasterScript.gameMaster;
        shmupMaster = ShmupManager.shmupManager;
        thisTrans = transform;
    }

    private void Start()
    {
        m_Anim = GetComponentInChildren<Animator>();

        movementPosition = thisTrans.localPosition;
        SpawnHealthbar();

        SetPatternVariables();
        shmupMaster.patternTimeLeft = spellcardTimeRemaining;
    }

    private void Update()
    {
        if (!(shmupMaster.startingSTG || shmupMaster.endSTG))
        {
            if (patternInitialCountdown < 0)
            {
                if(!spellcard)
                    shmupMaster.spellCap = false;

                if (waitTimeRemaining > 0)
                {
                    waitTimeRemaining -= gameMaster.timeScale;
                }

                Patterns();

                BulletCollisions(!(spellcard ? spellData[patternIndex].survivalSpell : nonData[patternIndex].survivalSpell));

                UpdateHealthbar();

                UpdateAnimator();

                shmupMaster.patternTimeLeft = spellcardTimeRemaining;

                if (!(spellcard ? spellData[patternIndex].killOnlySpell : nonData[patternIndex].killOnlySpell))
                    spellcardTimeRemaining -= gameMaster.timeScale;
            }
            else
            {
                if(spellcard && !shmupMaster.spellcardDisplayed)
                {
                    shmupMaster.DeclareSpellcard(spellData[patternIndex].nameBoxLength[(int)gameMaster.languageSetting], 
                        spellData[patternIndex].spellName[(int)gameMaster.languageSetting], spellData[patternIndex].characterPortrait);
                }
                patternInitialCountdown -= gameMaster.timeScale;
            }
        }
    }

    protected void SetPatternVariables()
    {
        spellcardTimeRemaining = (spellcard) ? spellData[patternIndex].spellcardTimeStart : nonData[patternIndex].spellcardTimeStart;
        health = (spellcard) ? spellData[patternIndex].startingHealth : nonData[patternIndex].startingHealth;
    }

    protected virtual void Patterns()
    {
        //To be overwritten
    }

    protected void UpdateAnimator()
    {
        if (prevPos.x - thisTrans.position.x < -.001f)
        {
            m_Anim.SetBool("Idle", false);
            m_Anim.SetBool("Right", true);
        }
        else if (prevPos.x - thisTrans.position.x > .001f)
        {
            m_Anim.SetBool("Idle", false);
            m_Anim.SetBool("Right", false);
        }
        else if (prevPos.y - thisTrans.position.y < -0.01)
        {
            m_Anim.SetBool("Idle", false);
            m_Anim.SetBool("Right", true);
        }
        else if (prevPos.y - thisTrans.position.y > 0.01f)
        {
            m_Anim.SetBool("Idle", false);
            m_Anim.SetBool("Right", false);
        }
        else if ((prevPos - (Vector2)thisTrans.position).sqrMagnitude < 0.01)
        {
            m_Anim.SetBool("Idle", true);
            m_Anim.SetBool("Right", false);
        }
    }

    protected void SpawnHealthbar()
    {
        Transform healthbarTrans = Instantiate(healthbarSpawn, thisTrans.position, new Quaternion(), thisTrans).transform;
        healthbar = healthbarTrans.GetComponent<SpriteMask>();

        float[] phaseMarkers = (spellcard) ? spellData[patternIndex].phaseMarkers : nonData[patternIndex].phaseMarkers;
        phaseMarkerObjects = new GameObject[phaseMarkers.Length];
        for (int i = 0; i < phaseMarkers.Length; i++)
        {
            phaseMarkerObjects[i] = Instantiate(healthbarTickSpawn, thisTrans.position, new Quaternion(), healthbarTrans);
            phaseMarkerObjects[i].transform.localPosition = MathFunctions.CalculateCircle(Vector3.zero, healthbarTickDist, phaseMarkers[i] * -360);
            phaseMarkerObjects[i].transform.eulerAngles = new Vector3(0, 0, phaseMarkers[i] * 360);
        }

        healthbarTrans.localScale *= healthbarSize;
    }

    protected void UpdateHealthbar()
    {
        float[] phaseMarkers = (spellcard) ? spellData[patternIndex].phaseMarkers : nonData[patternIndex].phaseMarkers;
        float startingHealth = (spellcard) ? spellData[patternIndex].startingHealth : nonData[patternIndex].startingHealth;

        healthbar.alphaCutoff = 1 - (health / startingHealth);

        for (int i = 0; i < phaseMarkers.Length; i++)
        {
            if(health / startingHealth < phaseMarkers[i])
                phaseMarkerObjects[i].SetActive(false);
            else
                phaseMarkerObjects[i].SetActive(true);
        }

        shmupMaster.enemyHpChange = startingHealth - health;
    }

    protected void OnDrawGizmosSelected()
    {
        /*if (pattern >= 0 && pattern < patternVariables.Length && patternVariables[pattern].positions != null)
        {

            float size = 5f;
            for (int i = 0; i < patternVariables[pattern].positions.Length; i++)
            {
                Vector3 globalWaypointPosition = (Application.isPlaying) ? patternVariables[pattern].positions[i].globalPoint : patternVariables[pattern].positions[i].relativePoint + bossDefaultPosition;

                if (i != 0)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(globalWaypointPosition, (Application.isPlaying) ? patternVariables[pattern].positions[i - 1].globalPoint : patternVariables[pattern].positions[i - 1].relativePoint + bossDefaultPosition);
                }

                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(globalWaypointPosition - Vector3.up * size, globalWaypointPosition + Vector3.up * size);
                Gizmos.DrawLine(globalWaypointPosition - Vector3.left * size, globalWaypointPosition + Vector3.left * size);
            }
        }*/
        
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(enemyOffscreenPosition - Vector2.up * 5, enemyOffscreenPosition + Vector2.up * 5);
        Gizmos.DrawLine(enemyOffscreenPosition - Vector2.left * 5, enemyOffscreenPosition + Vector2.left * 5);

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(enemyDefaultPosition - Vector2.up * 5, enemyDefaultPosition + Vector2.up * 5);
        Gizmos.DrawLine(enemyDefaultPosition - Vector2.left * 5, enemyDefaultPosition + Vector2.left * 5);
        
    }

    protected void DamagelessBulletCollision()
    {
        /*Collider2D[] hits = Physics2D.OverlapBoxAll((Vector2)transform.position + spriteHitbox.offset, spriteHitbox.bounds.size, 0, GameMasterScript.gameMaster.playerBulletMask);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].gameObject.tag == "PlayerBullet")
            {
                hits[i].transform.parent.GetComponent<PlayerBulletBase>().BulletHit(this);
            }
            else if (hits[i].gameObject.tag == "PlayerLaser")
            {
                hits[i].transform.parent.GetComponent<BasicPlayerLaser>().BulletHit(transform.position, this);
            }
        }*/
    }

    protected void BulletCollisions(bool canBeDamaged)
    {
        List<PlayerBulletCollider> hits = new List<PlayerBulletCollider>();

        LinkedListNode<PlayerBulletCollider> node = PlayerBulletCollider.colliders.First;
        PlayerBulletCollider colliderScript = null;
        Vector2 point;
        //Vector2 distance;
        float distance;
        Vector2 boxDistance;
        for (int i = 0; i < PlayerBulletCollider.colliders.Count; i++)
        {
            if (node == null)
            {
                continue;
            }
            else
            {
                colliderScript = node.Value;

                point = (Vector2)colliderScript.colliderTransform.position;

                distance = Vector2.Distance((point + colliderScript.hitboxOffset), ((Vector2)thisTrans.position + colliderOffset));

                distance -= colliderScript.hitboxScale.x + colliderSize.x;

                point = (Vector2)colliderScript.colliderTransform.position + (colliderScript.hitboxOffset * colliderScript.colliderTransform.lossyScale);

                boxDistance = new Vector2(Mathf.Abs(point.x - (thisTrans.position.x + colliderOffset.x)),
                    Mathf.Abs(point.y - (thisTrans.position.y + colliderOffset.y)));
                boxDistance -= (colliderSize * thisTrans.lossyScale) + (colliderScript.hitboxScale * colliderScript.colliderTransform.lossyScale);


                if (boxDistance.x <= 0 && boxDistance.y <= 0)
                {
                    hits.Add(node.Value);
                    break;
                }
            }
            node = node.Next;
        }

        if (canBeDamaged)
        {
            BaseBulletScript bulletScript;
            for (int i = 0; i < hits.Count; i++)
            {
                if (health > 0)
                {
                    if (hits[i].enabled)
                    {
                        bulletScript = hits[i].colliderTransform.GetComponentInParent<BaseBulletScript>();
                        float multiplier = 1;
                        for (int k = 0; k < weaknessAndResistances.Length; k++)
                        {
                            if(weaknessAndResistances[k].damageType == bulletScript.damageType)
                            {
                                multiplier = weaknessAndResistances[k].damageMultiplier;
                                break;
                            }
                        }

                        health -= bulletScript.damage * multiplier;

                        Destroy(bulletScript.gameObject);
                    }
                }
                else
                {
                    break;
                }
            }
        }

        hitByBomb = false;
    }

    protected void SpawnDeathEffect(Vector3 positionAffectors)
    {
        /*bool spawnedEffect = false;
        foreach (ExplosionPopEffect effect in ExplosionPopEffect.popEffects)
        {
            if (!effect.effectPlaying)
            {
                effect.gameObject.SetActive(true);
                effect.transform.position = transform.position + positionAffectors;
                effect.playEffect = true;
                spawnedEffect = true;
            }
        }

        if (!spawnedEffect)
        {
            Instantiate(GameMasterScript.gameMaster.explosionPopEffect, transform.position + positionAffectors, new Quaternion(0, 0, 0, 0), GameMasterScript.gameMaster.effectContainer);
        }
        spawnedDeathEffects++;*/
    }

    protected void SpawnBurstEffect(Vector3 positionAffectors, float time)
    {
        /*bool spawnedEffect = false;
        foreach (ExplosionBurstEffect2 effect in ExplosionBurstEffect2.burstEffect2s)
        {
            if (!effect.gameObject.activeSelf)
            {
                effect.gameObject.SetActive(true);
                effect.transform.position = transform.position + positionAffectors;
                effect.time = time;
                effect.timeCurrent = 0;
                effect.radius = 2000;
                spawnedEffect = true;
                break;
            }
        }

        if (!spawnedEffect)
        {
            GameObject burstEffect = Instantiate(GameMasterScript.gameMaster.burstEffect2, transform.position + positionAffectors, new Quaternion(0, 0, 0, 0), GameMasterScript.gameMaster.effectContainer);
            burstEffect.GetComponent<ExplosionBurstEffect2>().radius = 2000;
            burstEffect.GetComponent<ExplosionBurstEffect2>().time = time;
        }

        AudioSource.PlayClipAtPoint(GameMasterScript.gameMaster.deathExplosion, GameMasterScript.gameMaster.mainCamera.position, gameMaster.masterVolume / 4f);*/
    }

    protected virtual void Death()
    {
        //To be replaced on a per-script basis
    }

    public virtual void ChangePattern()
    {
        //To be overwritten
    }

    protected void ReturnToDefaultPosition()
    {
        percentBetweenPositions = betweenPositionsCurrentTime / patternInitialCountdown - 0.5f;

        if (percentBetweenPositions >= 1)
        {
            percentBetweenPositions = 1;
        }

        velocity = Vector2.Lerp(transform.position, enemyDefaultPosition, Mathf.Clamp(percentBetweenPositions, 0.05f, 1)) - new Vector2(transform.position.x, transform.position.y);
        transform.GetChild(0).localPosition = Vector2.Lerp(transform.GetChild(0).localPosition, Vector2.zero, Mathf.Clamp(percentBetweenPositions, 0.05f, 1));
        if (velocity.sqrMagnitude > 0)
        {
            transform.Translate(velocity * GameMasterScript.gameMaster.timeScale);
        }
    }

    protected void ReturnToOffscreenPosition()
    {
        percentBetweenPositions = betweenPositionsCurrentTime / patternInitialCountdown - 0.5f;

        if (percentBetweenPositions >= 1)
        {
            percentBetweenPositions = 1;
        }

        velocity = Vector2.Lerp(transform.position, enemyOffscreenPosition, Mathf.Clamp(percentBetweenPositions, 0.05f, 1)) - new Vector2(transform.position.x, transform.position.y);
        if (velocity.sqrMagnitude > 0)
        {
            transform.Translate(velocity * GameMasterScript.gameMaster.timeScale);
        }
    }

    protected void RandomSmoothedMovement(float maxSpeed, float smoothSpeed, Vector2 extentsX, Vector2 extentsY)
    {
        if (Vector2.Distance(thisTrans.localPosition, movementPosition) < 5)
        {
            movementPosition = new Vector2(Random(extentsX.x, extentsX.y), Random(extentsY.x, extentsY.y));
        }
        else
        {
            thisTrans.localPosition = Vector2.SmoothDamp(thisTrans.localPosition, movementPosition, ref movementSmoothing, smoothSpeed, maxSpeed, GameMasterScript.frameTime * GameMasterScript.gameMaster.timeScale);
        }
    }

    protected void RandomSmoothedMovement(float maxSpeed, float smoothSpeed)
    {
        RandomSmoothedMovement(maxSpeed, smoothSpeed, new Vector2(-175, 175), new Vector2(200, 375));
    }

    protected void RandomSmoothedMovement(float maxSpeed, float smoothSpeed, float waitTime, Vector2 extentsX, Vector2 extentsY)
    {
        if (Vector2.Distance(thisTrans.localPosition, movementPosition) < 5)
        {
            movementPosition = new Vector2(Random(extentsX.x, extentsX.y), Random(extentsY.x, extentsY.y));

            waitTimeRemaining = waitTime;
        }
        else if (waitTimeRemaining <= 0)
        {
            thisTrans.localPosition = Vector2.SmoothDamp(thisTrans.localPosition, movementPosition, ref movementSmoothing, smoothSpeed, maxSpeed, GameMasterScript.frameTime * GameMasterScript.gameMaster.timeScale);
        }
        else
        {
            movementSmoothing = Vector2.zero;
            waitTimeRemaining -= gameMaster.timeScale;
        }
    }

    protected void RandomSmoothedMovement(float maxSpeed, float smoothSpeed, float waitTime)
    {
        RandomSmoothedMovement(maxSpeed, smoothSpeed, waitTime, new Vector2(-175, 175), new Vector2(200, 375));
    }

    protected void SpellcardEnd()
    {
        /*if (!patternVariables[pattern].failedSpell)
        {
            int addedScore = 50000 * Mathf.RoundToInt((patternVariables[pattern].survivalSpell) ? patternVariables[pattern].spellcardTimeStart + 10 : patternVariables[pattern].spellcardTimeRemaining + 10);

            {
                GameObject textObject = Instantiate(gameMaster.inGameTextObject, Vector3.up * 400, new Quaternion(0, 0, 0, 0), gameMaster.inGameTextCanvas);
                InGameTextScript _inGameTextScript = textObject.GetComponent<InGameTextScript>();

                textObject.transform.localScale *= 2.75f;

                _inGameTextScript.textString = "Captured Spell Card!";
                _inGameTextScript.textColor = new Color(0.5f, 0.65f, .75f);
                _inGameTextScript.velocity = new Vector2(0, 0);
                _inGameTextScript.survivalTime = 5;
            }

            {
                GameObject textObject = Instantiate(gameMaster.inGameTextObject, Vector3.up * 360, new Quaternion(0, 0, 0, 0), gameMaster.inGameTextCanvas);
                InGameTextScript _inGameTextScript = textObject.GetComponent<InGameTextScript>();

                textObject.transform.localScale *= 2f;

                string scoreString = addedScore.ToString();
                int scoreLength = scoreString.Length;
                string finalScore = "";
                int k = 0;

                for (int i = scoreLength; i > 0; i--)
                {
                    k++;

                    if (k < 3 || i == 1)
                    {
                        finalScore = scoreString[i - 1] + finalScore;
                    }
                    else
                    {
                        finalScore = "," + scoreString[i - 1] + finalScore;
                        k = 0;
                    }

                }

                _inGameTextScript.textString = finalScore;
                _inGameTextScript.textColor = new Color(0.6f, 0.2f, 1f);
                _inGameTextScript.velocity = new Vector2(0, 0);
                _inGameTextScript.survivalTime = 5;
            }

            gameMaster.score += addedScore;
        }
        else
        {
            {
                GameObject textObject = Instantiate(gameMaster.inGameTextObject, Vector3.up * 400, new Quaternion(0, 0, 0, 0), gameMaster.inGameTextCanvas);
                InGameTextScript _inGameTextScript = textObject.GetComponent<InGameTextScript>();

                textObject.transform.localScale *= 2.5f;

                _inGameTextScript.textString = "Bonus Failed...";
                _inGameTextScript.textColor = new Color(0.6f, 0.6f, 0.6f, 1);
                _inGameTextScript.velocity = new Vector2(0, 0);
                _inGameTextScript.survivalTime = 5;
            }

            CustomAudioSource.PlayClipAt(gameMaster.failedSpellcard, Vector3.zero, gameMaster.masterVolume);
        }

        {
            GameObject textObject = Instantiate(gameMaster.inGameTextObject, Vector3.up * 325, new Quaternion(0, 0, 0, 0), gameMaster.inGameTextCanvas);
            InGameTextScript _inGameTextScript = textObject.GetComponent<InGameTextScript>();

            textObject.transform.localScale *= 1.75f;

            string timeString = (Mathf.Floor((patternVariables[pattern].spellcardTimeStart - patternVariables[pattern].spellcardTimeRemaining) * 100) / 100).ToString();

            _inGameTextScript.textString = "Clear Time:     " + timeString + "s";
            _inGameTextScript.textColor = new Color(0.4f, 0.7f, 0.2f);
            _inGameTextScript.velocity = new Vector2(0, 0);
            _inGameTextScript.survivalTime = 5;
        }*/
    }


    public static void UpdateSeed(int newSeed)
    {
        shmupEnemyRandom = new System.Random(newSeed);
        Debug.Log("Boss's Random is set to " + newSeed);
    }

    public int Random(int lower, int upper)
    {
        return shmupEnemyRandom.Next(lower, upper);
    }

    public float Random(float lower, float upper)
    {

        return (((float)shmupEnemyRandom.NextDouble() * Mathf.Abs(lower - upper)) + lower);
    }
}
