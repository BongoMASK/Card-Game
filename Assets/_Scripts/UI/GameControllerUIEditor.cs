//using UnityEngine;
//using UnityEditor;
//using UnityEditor.SceneManagement;

//[CustomEditor(typeof(GameControllerUI))]
//public class GameControllerUIEditor : Editor {
//    public override void OnInspectorGUI() {
//        base.OnInspectorGUI();

//        if (GUILayout.Button("Set Card Placers")) {
//            SetCardPlacersID();
//        }
//    }

//    private void SetCardPlacersID() {
//        CardPlacer.lastId = 0;

//        CardPlacer[] cardPlacers = FindObjectsOfType<CardPlacer>();
//        foreach (var item in cardPlacers) {
//            item.id = CardPlacer.lastId++;

//            EditorUtility.SetDirty(item);
//        }
//        EditorSceneManager.MarkSceneDirty(((GameControllerUI)target).gameObject.scene);
//    }
//}
