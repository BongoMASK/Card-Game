//using UnityEngine;
//using UnityEditor;
//using UnityEditor.SceneManagement;
//using System.Collections.Generic;

//[CustomEditor(typeof(GameData))]
//public class GameDataEditor : Editor {
//    public override void OnInspectorGUI() {
//        base.OnInspectorGUI();

//        if (GUILayout.Button("Set Card Placers")) {
//            SetCardPlacersID();
//            SetCardPlacers();
//        }
//    }

//    private void SetCardPlacers() {
//        GameData gameData = (GameData)target;

//        gameData.allCardPlacers = new List<CardPlacer>(FindObjectsOfType<CardPlacer>());
//        gameData.user1CardPlacers.Clear();
//        gameData.user2CardPlacers.Clear();

//        foreach (var item in gameData.allCardPlacers) {
//            if (item.id > gameData.allCardPlacers.Count / 2 - 1) {
//                gameData.user1CardPlacers.Add(item);
//                item.owner = PlayerData.FindPlayerData(0);
//            }
//            else {
//                gameData.user2CardPlacers.Add(item);
//                item.owner = PlayerData.FindPlayerData(1);
//            }
//        }

//        EditorUtility.SetDirty(gameData);
//        EditorSceneManager.MarkSceneDirty(gameData.gameObject.scene);
//    }

//    private void SetCardPlacersID() {
//        CardPlacer.lastId = 0;

//        CardPlacer[] cardPlacers = FindObjectsOfType<CardPlacer>();

//        foreach (var item in cardPlacers) {
//            item.id = CardPlacer.lastId++;

//            EditorUtility.SetDirty(item);
//        }

//        EditorSceneManager.MarkSceneDirty(((GameData)target).gameObject.scene);
//    }
//}