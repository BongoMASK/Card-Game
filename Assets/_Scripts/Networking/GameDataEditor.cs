//using UnityEngine;
//using UnityEditor;
//using UnityEditor.SceneManagement;
//using System.Collections.Generic;

//[CustomEditor(typeof(GameData))]
//public class GameDataEditor : Editor {
//    public override void OnInspectorGUI() {
//        base.OnInspectorGUI();

//        if (GUILayout.Button("Set Card Placers")) {
//            SetCardPlacers();
//        }
//    }

//    private void SetCardPlacers() {
//        GameData gameData = (GameData)target;

//        gameData.allCardPlacers = new List<CardPlacer>(FindObjectsOfType<CardPlacer>());
//        gameData.user1CardPlacers.Clear();
//        gameData.user2CardPlacers.Clear();

//        foreach (var item in gameData.allCardPlacers) {
//            if (item.id > 21)
//                gameData.user1CardPlacers.Add(item);
//            else
//                gameData.user2CardPlacers.Add(item);
//        }

//        EditorUtility.SetDirty(gameData);
//        EditorSceneManager.MarkSceneDirty(gameData.gameObject.scene);
//    }
//}