using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditorInternal;


[CustomPropertyDrawer(typeof(TachoShiftLampMeter.LampInfo))]
public class LampInfoDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position,
        SerializedProperty property, GUIContent label)
    {

        using (new EditorGUI.PropertyScope(position, label, property))
        {
            //サムネの領域を確保するためにラベル領域の幅を小さくする
            EditorGUIUtility.labelWidth = 50;
            position.height = EditorGUIUtility.singleLineHeight;
            var halfWidth = position.width * 0.5f;
            //各プロパティーの Rect を求める
            var litRect = new Rect(position)
            {
                width = 48,
                height = 48
            };
            var unlitRect = new Rect(position)
            {
                x = position.x + 48,
                width = 48,
                height = 48
            };
            var limitRect = new Rect(position)
            {
                width = position.width - 96,
                x = position.x + 96
            };
//            var image1Rect = new Rect(limitRect)
//            {
//                y = limitRect.y + EditorGUIUtility.singleLineHeight + 2
//            };
//            var image2Rect = new Rect(image1Rect)
//            {
//                y = image1Rect.y + EditorGUIUtility.singleLineHeight + 2
//            };
            //各プロパティーの SerializedProperty を求める
            var litProperty = property.FindPropertyRelative("litSprite");
            var unlitProperty = property.FindPropertyRelative("unlitSprite");
//            var image1Property = property.FindPropertyRelative("image1");
//            var image2Property = property.FindPropertyRelative("image2");
            var limitProperty = property.FindPropertyRelative("limit");

            //各プロパティーの GUI を描画
            litProperty.objectReferenceValue =
                EditorGUI.ObjectField(litRect,
                litProperty.objectReferenceValue, typeof(Sprite), false);

            unlitProperty.objectReferenceValue =
                EditorGUI.ObjectField(unlitRect,
                unlitProperty.objectReferenceValue, typeof(Sprite), false);

            limitProperty.floatValue =
                EditorGUI.FloatField(limitRect, limitProperty.floatValue);
//            image1Property.objectReferenceValue =
//                EditorGUI.ObjectField(image1Rect,
//                image1Property.objectReferenceValue, typeof(Image), true);
//            image2Property.objectReferenceValue =
//                EditorGUI.ObjectField(image2Rect,
//                image2Property.objectReferenceValue, typeof(Image), true);
        }

    } /* OnGUI */
}

[CustomPropertyDrawer(typeof(TachoShiftLampMeter.LampImageInfo))]
public class LampImageInfoDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position,
        SerializedProperty property, GUIContent label)
    {

        using (new EditorGUI.PropertyScope(position, label, property))
        {
            //サムネの領域を確保するためにラベル領域の幅を小さくする
            EditorGUIUtility.labelWidth = 50;
            position.height = EditorGUIUtility.singleLineHeight;
            var halfWidth = position.width * 0.5f;
            //各プロパティーの Rect を求める
            var image1Rect = new Rect(position)
            {
                height = EditorGUIUtility.singleLineHeight + 2
            };
            var image2Rect = new Rect(image1Rect)
            {
                y = image1Rect.y + EditorGUIUtility.singleLineHeight + 2
            };
            //各プロパティーの SerializedProperty を求める
                        var image1Property = property.FindPropertyRelative("image1");
                        var image2Property = property.FindPropertyRelative("image2");
            
            //各プロパティーの GUI を描画
            image1Property.objectReferenceValue =
                EditorGUI.ObjectField(image1Rect,
                image1Property.objectReferenceValue, typeof(Image), true);
            image2Property.objectReferenceValue =
                EditorGUI.ObjectField(image2Rect,
                image2Property.objectReferenceValue, typeof(Image), true);
        }

    } /* OnGUI */
}



[CustomEditor(typeof(TachoShiftLampMeter))]
public class TachoShiftLampMeterInspector : Editor
{
    ReorderableList reorderableLampsList;
    ReorderableList reorderableLampImagesList;
    void OnEnable()
    {
        var propLamps = serializedObject.FindProperty("lamps");
        reorderableLampsList = new ReorderableList(serializedObject, propLamps);
        reorderableLampsList.elementHeight = 52;
        reorderableLampsList.drawElementCallback =
            (rect, index, isActive, isFocused) => {
                var element = propLamps.GetArrayElementAtIndex(index);
                rect.height -= 4;
                rect.y += 2;
                EditorGUI.PropertyField(rect, element);
            };
        reorderableLampsList.drawHeaderCallback = (rect) =>
            EditorGUI.LabelField(rect, propLamps.displayName);

        var propLampImages = serializedObject.FindProperty("lampImages");
        reorderableLampImagesList = new ReorderableList(serializedObject, propLampImages);
        reorderableLampImagesList.elementHeight = 44;
        reorderableLampImagesList.drawElementCallback =
            (rect, index, isActive, isFocused) => {
                var element = propLampImages.GetArrayElementAtIndex(index);
                rect.height -= 4;
                rect.y += 2;
                EditorGUI.PropertyField(rect, element);
            };
        reorderableLampImagesList.drawHeaderCallback = (rect) =>
            EditorGUI.LabelField(rect, propLampImages.displayName);
    } /* OnEnable */
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("meterType"));
        EditorGUILayout.Separator();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("subMeter"));
        EditorGUILayout.Separator();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("text"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("format"));
        EditorGUILayout.Separator();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("lowValue"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("highValue"));
        EditorGUILayout.Separator();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("lowColor"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("normalColor"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("highColor"));
        EditorGUILayout.Separator();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("valueMin"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("valueMax"));
        EditorGUILayout.Separator();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("shiftValue"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("blinkTime"));
        EditorGUILayout.Separator();
        reorderableLampsList.DoLayoutList();
        reorderableLampImagesList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    } /* OnInspectorGUI */

} /* class */
