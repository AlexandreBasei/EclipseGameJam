using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EventAndResponse))]
public class EventAndResponseDrawer : PropertyDrawer
{
    private SerializedProperty _gameEvent;
    private SerializedProperty _response;
    private float _spacing;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);

        //filling properties
        _gameEvent = property.FindPropertyRelative("gameEvent");
        _response = property.FindPropertyRelative("response");
        string name = _gameEvent.objectReferenceValue?.name;
        name = name == null ? "NO EVENT" : name; 

        //drawing instruction
        Rect foldOutBox = new Rect(position.min.x, position.min.y, position.size.x, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(foldOutBox, property.isExpanded, label);

        position.min = position.min +  new Vector2(0,EditorGUIUtility.singleLineHeight);

        if (property.isExpanded)
        {
            _spacing = 0;
            DrawLabel(ref position, name);
            DrawEvent(ref position);
            DrawResponse(ref position);
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int lineCount = 1;
        float t_spacing = 0;
        float responseSize = 0;
        if (property.isExpanded)
        {
            lineCount += 1;
            t_spacing = 10f + 15f;
            responseSize = _response==null ? 0 :EditorGUI.GetPropertyHeight(_response, true);
        }
        
        

        return lineCount * EditorGUIUtility.singleLineHeight + responseSize + t_spacing;
    }

    void DrawLabel(ref Rect position, string name)
    {
        GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
        labelStyle.fontSize = 14;
        labelStyle.fontStyle = FontStyle.Bold;


        Rect labelRect = new Rect(
            position.x ,
            position.y,
            position.width,
            EditorGUIUtility.singleLineHeight
        );

        
        _spacing += 5f;

        EditorGUI.LabelField(labelRect, name , labelStyle);

        position.min += new Vector2(0,EditorGUIUtility.singleLineHeight);
    }

    void DrawEvent(ref Rect position)
    {
        float xPos = position.min.x;
        float yPos = position.min.y + _spacing;
        float width = position.size.x;
        float height = EditorGUIUtility.singleLineHeight;

        Rect drawArea = new Rect(xPos, yPos, width, height);
        EditorGUI.PropertyField(drawArea, _gameEvent, new GUIContent("Event"));

        position.min += new Vector2(0,EditorGUIUtility.singleLineHeight);
        _spacing+=5f;
    }

    void DrawResponse(ref Rect position)
    {
        float xPos = position.min.x;
        float yPos = position.min.y + _spacing;
        float width = position.size.x;
        float height = EditorGUI.GetPropertyHeight(_response, true);

        Rect drawArea = new Rect(xPos, yPos, width, height);
        EditorGUI.PropertyField(drawArea, _response, new GUIContent("Response"), true);
    }


}
