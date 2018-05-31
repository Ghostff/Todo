using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using TD = Todo.TodoDB;

public class Todo : ScriptableObject
{
    [Serializable] public class TodoDB
    {
        public string title;
        public int type;
        public bool show;
        public bool done;
        public string description;
    }

    public List<TodoDB> todo = new List<TodoDB>();
}

public class TodoEditor : EditorWindow
{
    private string dir = "Assets/Scripts/Editor/Resources/";
    private Todo target;
    private string description;
    private string errorMsg;
    private int editing = -1;
    private new string title;
    private int type = 1;
    private Vector2 scrollPos;
    private bool edit;
    private int show;

    [MenuItem ("Window/Todo %l")]
    public static void Init()
    {
        // Get existing open window or if none, make a new one:
        TodoEditor window = (TodoEditor)GetWindow(typeof(TodoEditor), false, "My Todo List");
        window.autoRepaintOnSceneChange = false;
        window.Show();
    }

    private GUIStyle C(Color color)
    {
        GUIStyle s = new GUIStyle();
        s.normal.textColor = color;
        return s;
    }

    private float widthbefore;
    private GUIStyle FontAndWith(int font, int width, Color color, FontStyle fontStyle, bool wordWrap = false)
    {
        GUIStyle s = new GUIStyle();
        if (width > 0)
        {
            if (widthbefore == 0)
                widthbefore = s.fixedWidth;
            s.fixedWidth = width;
        }
        else
        {
            s.fixedWidth = widthbefore;
            widthbefore = 0;
        }
        s.wordWrap = wordWrap;
        s.fontSize = font;
        s.fontStyle = fontStyle;
        s.normal.textColor = color;
        return s;
    }

    private Color GetColor(int type)
    {
        if (type == 3) //urgent
            return Color.black;
        else if (type == 2) // InProgress
            return Color.yellow;
        else if (type == 1) // Normal
            return Color.white;
        else if (type == 5) // Test
            return Color.green;
        else // Bug
            return Color.red;
    }

    private void OnGUI()
    {
        if (target == null)
        {
            target = (Todo) EditorGUIUtility.Load(dir + "Todo.asset");
            if (target == null)
            {
                target = ScriptableObject.CreateInstance(typeof(Todo)) as Todo;
                System.IO.Directory.CreateDirectory(dir);
                AssetDatabase.CreateAsset(target, dir + "Todo.asset");

                target.todo.Add(new TD() {
                    title = "New!!",
                    type = 1,
                    description = "Go ahead and delete me. :)"
                });
                Save();
            }
            
        }

        string[] types = new string[6] {
            "All", "Normal", "InProgress", "Urgent", "bug", "Test"	
        };

        GUIStyle margin = new GUIStyle(GUI.skin.button);
        margin.margin = new RectOffset(0, 0, 0, 0);
        margin.fixedWidth = 50;

        EditorStyles.textField.wordWrap = true;
        GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height - 145));
            GUILayout.BeginVertical("box");

                int size = target.todo.Count - 1;
                int newSize = size;
                int completed= 0;
                bool hasTask = false;
                for (int i = 0; i <= newSize; i++)
                {
                    if (target.todo[i].done)
                        newSize--;
                }
                
                GUILayout.BeginHorizontal();
                    GUILayout.Label("PENDING TASKS:", EditorStyles.boldLabel);
                    show = EditorGUILayout.Popup(show, types, GUILayout.Width(100));
                GUILayout.EndHorizontal();
                
                scrollPos = GUILayout.BeginScrollView(scrollPos,false,true);
                
                GUILayout.BeginVertical("box");	
                    for (int i = 0; i <= size; i++)
                    {
                        TD td = target.todo[i];

                        if (td.done)
                        {
                            completed++;
                            newSize++;
                            continue;
                        }
                        else if (show != 0 && show != td.type)
                        {
                            continue;
                        }

                        hasTask = true;

                        GUILayout.BeginVertical("box");

                            GUILayout.BeginHorizontal();
                        
                                td.done = EditorGUILayout.Toggle(td.done, GUILayout.Width(20));
                                EditorGUILayout.LabelField(td.title, FontAndWith(12, 100, GetColor(td.type), FontStyle.Bold));
                                types[0] = null;
                                td.type = EditorGUILayout.Popup(td.type, types, GUILayout.Width(100));
                                if(GUILayout.Button((editing == i) ? "Cancel" : "Edit", margin))
                                {
                                    if (editing == i)
                                    {
                                        title = null;
                                        description = null;
                                        type = td.type;
                                        editing = -1;
                                    }
                                    else
                                    {
                                        title = td.title;
                                        description = td.description;
                                        type = td.type;
                                        editing = i;
                                    }
                                }
                                
                                TD next = target.todo.ElementAtOrDefault(i + 1);
                                TD prev = target.todo.ElementAtOrDefault(i - 1);
                                margin.fixedWidth = 20;

                                if(i == 0)
                                {
                                    if(GUILayout.Button("⇓", margin))
                                    {
                                        if (next != null)
                                        {
                                            target.todo[i] = next;
                                            target.todo[i + 1] = td;
                                            Save();
                                        }
                                    }
                                }
                                else if(i == newSize)
                                {
                                    if(GUILayout.Button("⇑", margin))
                                    {
                                        if (prev != null)
                                        {
                                            target.todo[i - 1] = td;
                                            target.todo[i] = prev;
                                            Save();
                                        }
                                    }
                                }
                                else
                                {
                                    if(GUILayout.Button("⇑", margin))
                                    {
                                        if (prev != null)
                                        {
                                            target.todo[i - 1] = td;
                                            target.todo[i] = prev;
                                            Save();
                                        }
                                    }
                                    if(GUILayout.Button("⇓", margin))
                                    {
                                        if (next != null)
                                        {
                                            target.todo[i + 1] = td;
                                            target.todo[i] = next;
                                            Save();
                                        }
                                    }
                                }

                                if(GUILayout.Button(td.show ? "⊟" : "⊞", margin))
                                    td.show = ! td.show;

                                Color c = margin.normal.textColor;
                                margin.normal.textColor = Color.red;
                                if(GUILayout.Button("x", margin))
                                {
                                    if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete \"" + td.title + "\"",  "Yes", "No"))
                                    {
                                        target.todo.RemoveAt(i);
                                        return;
                                    }
                                }
                                margin.normal.textColor = c;

                                margin.fixedWidth = 44;
                            GUILayout.EndHorizontal();

                            if (td.show)
                            {
                                GUILayout.BeginVertical("box");
                                    EditorGUILayout.LabelField(td.description, FontAndWith(12, 0, GetColor(td.type), FontStyle.Bold, true));
                                GUILayout.EndVertical();
                            }

                        GUILayout.EndVertical();
                    }

                    if (! hasTask)
                    {
                        EditorGUILayout.LabelField("No Task", EditorStyles.largeLabel);
                    }
                GUILayout.EndVertical();

                GUILayout.Label("COMPLETED TASKS:", EditorStyles.boldLabel);
                if (completed > 0)
                {
                    GUILayout.BeginVertical("box");
                        for (int i = 0; i < target.todo.Count; i++)
                        {
                            TD td = target.todo[i];
                            if (! td.done)
                                continue;

                            GUILayout.BeginVertical("box");

                                GUILayout.BeginHorizontal();
                                    td.done = EditorGUILayout.Toggle(td.done, GUILayout.Width(20));
                                    EditorGUILayout.LabelField(td.title, FontAndWith(12, 100, Color.grey, FontStyle.Bold));
                                GUILayout.EndHorizontal();

                            GUILayout.EndVertical();
                        }
                    GUILayout.EndVertical();
                }

                GUILayout.EndScrollView ();
            GUILayout.EndVertical();
        GUILayout.EndArea();
        


        GUILayout.FlexibleSpace();

        GUILayout.BeginVertical("box");

            GUILayout.BeginHorizontal();
                GUILayout.Label("CREATES TASK (⇑:move up, ⇓:move down, ⊞:show content, ⊟:hide content)");
                GUILayout.Label(errorMsg, C(Color.red));
            EditorGUILayout.EndHorizontal();
            GUILayout.BeginVertical("box");
                GUILayout.BeginHorizontal();
                    string label = (editing > -1) ? "Label(EDITING): " : "Label: ";
                    title = EditorGUILayout.TextField(label, title);
                    type = EditorGUILayout.Popup(type, types, GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();
                description = EditorGUILayout.TextArea(description, GUILayout.Height(40));
            GUILayout.EndVertical();

             GUILayout.BeginVertical("box");
                if(GUILayout.Button("Add New Task"))
                {
                    if (title.Trim() == "")
                    {
                        errorMsg = "Title or Description is required.";
                    }
                    else
                    {
                        errorMsg = null;
                        TD newTd = new TD() {
                            title = this.title,
                            type = this.type,
                            description = this.description
                        };
                        title = "";
                        description = "";
                        GUI.FocusControl(null);
                    
                        if (editing == -1)
                            target.todo.Add(newTd);
                        else
                        {
                            target.todo[editing] = newTd;
                            editing = -1;
                        }
                        Save();
                    }
                }
             GUILayout.EndVertical();
        GUILayout.EndVertical();
    }

    private void Save()
    {
        EditorUtility.SetDirty(target);
        AssetDatabase.SaveAssets();
        AssetDatabase.SaveAssets();
    }
    
    void OnDestroy()
    {
        Save();
    }	    
}
