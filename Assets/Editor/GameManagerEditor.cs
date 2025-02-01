using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Draws default Inspector fields

        GameManager gameManager = (GameManager)target;

        if (GUILayout.Button("Find and Register Monsters"))
        {
            gameManager.FindAndRegisterMonsters();
        }
    }
}