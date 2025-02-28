using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAppearanceController : MonoBehaviour
{
    public GameObject monster;
    public float animationDuration;
    private bool hasAppeared;
    
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
        
        gameObject.SetActive(false);
        
        // show monster
        if(monster != null)
            monster.SetActive(true);
    }
    
}
