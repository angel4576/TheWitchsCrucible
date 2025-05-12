using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAppearanceController : MonoBehaviour
{
    public GameObject boss;
    public Vector3 spawnPosition;
    public float animationDuration;
    private bool hasAppeared;

    [SerializeField] private bool canSetSwitch = false;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TriggerBossAppearance()
    {
        if(hasAppeared)
            return;
        
        hasAppeared = true;
        gameObject.SetActive(true);
        
        StartCoroutine(StartAnimationCountdown());
        
    }
    
    IEnumerator StartAnimationCountdown()
    {
        yield return new WaitForSeconds(animationDuration);
        
        // Animation over
        gameObject.SetActive(false);
        
        // Set boss position
        if (boss != null)
            boss.transform.position = spawnPosition;
        
        // Activate player ability
        if(canSetSwitch)
            WorldControl.Instance.SetCanSwitch(); // temp
    }
    
}
