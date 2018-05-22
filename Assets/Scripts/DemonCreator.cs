using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonCreator : MonoBehaviour {

    public enum DemonTypes
    {
        BASIC_DEMON,
        HIGH_DEMON,
        RUSHING_DEMON
    }

    [Header("DemonStats")]
    [SerializeField] DemonStats basicDemonStats;
    [SerializeField] DemonStats highDemonStats;
    [SerializeField] DemonStats rushingDemonStats;

    [Header("DemonBody")]
    [SerializeField] GameObject demonPrefab;

    public void CreateDemon(Vector3 position, DemonTypes demonType)
    {
        GameObject demon = Instantiate(demonPrefab, position, demonPrefab.transform.rotation);
        demon.transform.parent = transform;
        Debug.Log("NIQUE TA MERE");

        DemonStats demonStats = basicDemonStats;
        switch(demonType)
        {
            case DemonTypes.BASIC_DEMON:
                demonStats = basicDemonStats;
                break;
            case DemonTypes.HIGH_DEMON:
                demonStats = highDemonStats;
                break;
            case DemonTypes.RUSHING_DEMON:
                demonStats = rushingDemonStats;
                break;
        }

        demon.GetComponent<DemonBehavior>().SetDemonStats(demonStats);
    }
}
