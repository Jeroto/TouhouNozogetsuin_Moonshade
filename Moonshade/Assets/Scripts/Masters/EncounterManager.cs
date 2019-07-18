using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncounterManager : MonoBehaviour
{
    public static EncounterManager encounterManager;
    public ShmupManager shmupManager;
    public BattleManager battleManager;

    private void Awake()
    {
        encounterManager = this;
        shmupManager = GameObject.Find("ShmupMaster").GetComponent<ShmupManager>();
        battleManager = GameObject.Find("BattleMaster").GetComponent<BattleManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
