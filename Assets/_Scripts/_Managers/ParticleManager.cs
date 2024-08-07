using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    [Header("References")]
    private Camera cam;

    [Header("Prefabs")]
    [SerializeField] private GameObject impactExplosion;    

    private void Awake()
    {
        cam = Camera.main;
    }

    public void ImpactExplosion(Vector3 spawnPosition, Quaternion spawnRotation)
    {
        GameObject newImpactExplosion = Instantiate(impactExplosion, spawnPosition, spawnRotation);
        newImpactExplosion.GetComponent<LookAtCamera>().target = cam.transform;
    }
    public void ImpactExplosion(Vector3 spawnPosition, Quaternion spawnRotation, bool lookAtCamera)
    {
        GameObject newImpactExplosion = Instantiate(impactExplosion, spawnPosition, spawnRotation);
        if (!lookAtCamera)
        {
            newImpactExplosion.GetComponent<LookAtCamera>().enabled = false;
            newImpactExplosion.GetComponentInChildren<SpriteRenderer>().sortingOrder = 1;
        }
    }
    public void SmokeExplosion(Vector3 spawnPosition, Quaternion spawnRotation)
    {
        GameObject newSmokeExplosion = Instantiate(impactExplosion, spawnPosition, spawnRotation);
        newSmokeExplosion.GetComponent<LookAtCamera>().target = cam.transform;
    }

}
