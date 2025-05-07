using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Dialogue/New Dialogue Line")]
public class DialogueLine : ScriptableObject
{
    public string speakerName;

    [TextArea(2, 5)]
    public string content;
    
    public string eventKey;
}