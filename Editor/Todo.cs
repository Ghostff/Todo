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
        public string description;
    }

    public List<TodoDB> todo = new List<TodoDB>();
    public List<TodoDB> done = new List<TodoDB>();
    public List<TodoDB> deleted = new List<TodoDB>();
}

public class TodoEditor : EditorWindow
{
    private string dir = "Assets/Scripts/Editor/Resources/";
    private string branch = "origin master";
    private Todo target;
    private string description;
    private string errorMsg;
    private int editing = -1;
    private new string title;
    private int type = 1;
    private Vector2 scrollPos;
    private string commitMsg;
    private int show;
    private bool showCommit;

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

    private Color GetColor(int type, byte alpha = 255)
    {
        if (type == 1) //Feature
            return new Color32(255, 255, 255, alpha); //white
        else if (type == 2) // Enhance
            return new Color32(51, 153, 255, alpha); //sky blue
        else if (type == 3) // Modify
            return new Color32(10, 40, 90, alpha); // dark blue
        else if (type == 4) // Bug
            return new Color32(255, 0, 0, alpha); // red
        else if (type == 5) // Test
            return new Color32(0, 255, 0, alpha); // green
        else // active (first)
            return new Color32(255, 255, 0, alpha); //yellow
    }

    private string ConCat(int type, string title)
    {
        string t = "\n-";
        if (type == 1) //Feature
            t += "New: ";
        else if (type == 2) // Enhance
            t += "Enhanced: ";
        else if (type == 3) // Modify
            t += "Refactored: ";
        else if (type == 4) // Bug
            t += "Fixed: ";
        else if (type == 5) // Test
            t += "Tested: ";

        return t + title;
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
            }
        }

        string[] types = new string[6] {"All", "Feature", "Enhance", "Modify", "Bug", "Test"};

        GUIStyle margin = new GUIStyle(GUI.skin.button);
        margin.margin = new RectOffset(0, 0, 0, 0);

        EditorStyles.textField.wordWrap = true;
        GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height - 145));

            GUILayout.BeginVertical("box");

                GUILayout.BeginHorizontal();
                    GUILayout.Label("PENDING TASKS:", EditorStyles.boldLabel);
                    show = EditorGUILayout.Popup(show, types, GUILayout.Width(100));
                GUILayout.EndHorizontal();
                
                scrollPos = GUILayout.BeginScrollView(scrollPos,false,true);
                GUILayout.BeginVertical("box");
                    int size = target.todo.Count - 1;
                    for (int i = size; i >= 0; i--)
                    {
                        TD td = target.todo[i];
                        int color = (i == size) ? -1 : td.type;

                        GUILayout.BeginVertical("box");

                            GUILayout.BeginHorizontal();
                        
                                if (EditorGUILayout.Toggle(false, GUILayout.Width(20)))
                                {
                                    target.done.Add(td);
                                    target.todo.RemoveAt(i);
                                    return;
                                }
                                EditorGUILayout.LabelField(td.title, FontAndWith(12, 100, GetColor(color), FontStyle.Bold));
                                types[0] = null;
                                td.type = EditorGUILayout.Popup(td.type, types, GUILayout.Width(80));
                                margin.fixedWidth = 60;
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
                                
                            
                                if (size > 0)
                                {
                                    margin.fixedWidth = 20;
                                    if (i == size)
                                    {
                                        if(GUILayout.Button("⇓", margin))
                                        {
                                            target.todo[i] = target.todo[i - 1];
                                            target.todo[i - 1] = td;
                                        }
                                    }
                                    else if (i == 0)
                                    {
                                        if(GUILayout.Button("⇑", margin))
                                        {
                                            target.todo[i] = target.todo[i + 1];
                                            target.todo[i + 1] = td;
                                        }
                                    }
                                    else
                                    {
                                        if(GUILayout.Button("⇑", margin))
                                        {
                                            target.todo[i] = target.todo[i + 1];
                                            target.todo[i + 1] = td;
                                        }
                                        if(GUILayout.Button("⇓", margin))
                                        {
                                            target.todo[i] = target.todo[i - 1];
                                            target.todo[i - 1] = td;
                                        }
                                    }
                                }

                                if(GUILayout.Button(td.show ? "⊟" : "⊞", margin))
                                    td.show = ! td.show;

                                Color c = margin.normal.textColor;
                                margin.normal.textColor = Color.red;
                                if(GUILayout.Button("x", margin))
                                    if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete \"" + td.title + "\"",  "Yes", "No"))
                                    {
                                        target.todo.RemoveAt(i);
                                        return;
                                    }
                                margin.normal.textColor = c;

                                margin.fixedWidth = 44;
                            GUILayout.EndHorizontal();

                            if (td.show)
                            {
                                GUILayout.BeginVertical("box");
                                    EditorGUILayout.LabelField(td.description, FontAndWith(12, 0, GetColor(color), FontStyle.Bold, true));
                                GUILayout.EndVertical();
                            }

                        GUILayout.EndVertical();
                    }

                    if (size < 0)
                        EditorGUILayout.LabelField("No Task", EditorStyles.largeLabel);

                GUILayout.EndVertical();
                
                int completed = target.done.Count;
                if (completed > 0)
                {
                    GUILayout.Space(10);
                
                    GUILayout.BeginHorizontal();
                        GUILayout.Label("COMPLETED TASKS:", EditorStyles.boldLabel);
                        if(GUILayout.Button(showCommit ? "Hide commit message" : "Show commit message", GUILayout.Width(150)))
                            showCommit = ! showCommit;

                        if(GUILayout.Button("Delete Completed", GUILayout.Width(150)))
                        {
                            if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete all completed tasks?",  "Yes", "No"))
                            {
                                target.deleted.AddRange(target.done);
                                target.done.Clear();
                                return;
                            }
                        }
                    
                    GUILayout.EndHorizontal();
                    GUILayout.BeginVertical("box");
                        if (showCommit)
                        {
                            GUILayout.BeginVertical("box");
                                commitMsg = "git add . && git commit -m \"" + commitMsg + "\" && git push " + branch;
                                EditorGUILayout.SelectableLabel(commitMsg, GUILayout.MaxHeight((completed + 1) * 14));
                            GUILayout.EndVertical();
                        }
                        
                        commitMsg = "Changes:";
                        for (int i = 0; i < completed; i++)
                        {
                            TD td = target.done[i];
                            GUILayout.BeginVertical("box");
                            commitMsg += ConCat(td.type, td.title);

                                GUILayout.BeginHorizontal();
                                    if (! EditorGUILayout.Toggle(true, GUILayout.Width(20)))
                                    {
                                        target.todo.Add(td);
                                        target.done.RemoveAt(i);
                                        return;
                                    }
                                    EditorGUILayout.LabelField(td.title, FontAndWith(12, 100, GetColor(td.type, 100), FontStyle.Bold));
                                GUILayout.EndHorizontal();

                            GUILayout.EndVertical();
                        }

                    GUILayout.EndVertical();
                }

                GUILayout.EndScrollView();
            GUILayout.EndVertical();
        GUILayout.EndArea();
        
        GUILayout.FlexibleSpace();

        GUILayout.BeginVertical("box");

            bool edit = (editing > -1);
            GUILayout.BeginHorizontal();
                GUILayout.Label("CREATES TASK (⇑:move up, ⇓:move down, ⊞:show content, ⊟:hide content)");
                GUILayout.Label(errorMsg, C(Color.red));
            EditorGUILayout.EndHorizontal();
            GUILayout.BeginVertical("box");
                GUILayout.BeginHorizontal();
                    string label = edit ? "Label(EDITING): " : "Label: ";
                    title = EditorGUILayout.TextField(label, title);
                    type = EditorGUILayout.Popup(type, types, GUILayout.Width(80));
                EditorGUILayout.EndHorizontal();
                description = EditorGUILayout.TextArea(description, GUILayout.Height(40));
            GUILayout.EndVertical();

             GUILayout.BeginVertical("box");
                if(GUILayout.Button(edit ? "Update Task" : "Add New Task"))
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
                        type = 1;
                        title = "";
                        description = "";
                        GUI.FocusControl(null);
                    
                        if (! edit)
                            target.todo.Add(newTd);
                        else
                        {
                            target.todo[editing] = newTd;
                            editing = -1;
                        }
                    }
                }
             GUILayout.EndVertical();
        GUILayout.EndVertical();

    }
    
    void OnDestroy()
    {
        EditorUtility.SetDirty(target);
        AssetDatabase.SaveAssets();
    }
}
