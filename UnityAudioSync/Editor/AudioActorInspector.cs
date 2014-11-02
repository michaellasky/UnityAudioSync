using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(AudioActor))]
public class AudioActorInspector : Editor
{

    public  AudioActor          Target 
    {
        get { return (AudioActor) target; }
    }

    public GameObject           Go 
    {
        get { return Target.gameObject; }
    }

    private AudioActor          targetActor;
    private GameObject          go;
    private SerializedProperty  onTriggerEventList;

    public override void OnInspectorGUI ()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        if (Time.realtimeSinceStartup % 5 == 0)
        {
            Target.FindMethods(Go);
            Target.RegisterMethods();
        }
        
        DrawLayout();

        EditorUtility.SetDirty(Target);
    }

    void DrawLayout ()
    {
        GUILayoutOption expandWidth = GUILayout.ExpandWidth(false);
        int      removeMethod = -1;
        int[]    methodIdx    = Target.MethodIdx;
        string[] methodNames  = Target.MethodNames;

        for (int i = 0; i < Target.NumMethodsAttached; i++)
        {
            methodIdx[i] = EditorGUILayout.Popup(methodIdx[i], methodNames);
            
            if(GUILayout.Button("-", expandWidth)) { removeMethod = i; }
        }

        if (removeMethod != -1)
        {
            RemoveMethod(removeMethod);
            removeMethod = -1;
        }

        if (GUILayout.Button("+", expandWidth)) { AddMethod(); }
    }

    void AddMethod ()
    {
        int[] nMethodIdx = new int[Target.MethodIdx.Length + 1];
        Target.MethodIdx.CopyTo(nMethodIdx, 0);

        Target.NumMethodsAttached++;

        Target.MethodIdx = nMethodIdx;
    }

    void RemoveMethod (int mIdx)
    {
        int   newLength   = Target.MethodIdx.Length - 1;
        int   lowerLength = newLength - (newLength - mIdx);
        int   upperLength = newLength - mIdx;
        int[] nMethodIdx  = new int[newLength];
        int[] cMethodIdx  = Target.MethodIdx;

        if (mIdx != 0)
        {
            Array.Copy(cMethodIdx, 0, nMethodIdx, 0, lowerLength);    
        }

        Array.Copy(cMethodIdx, mIdx + 1, nMethodIdx, mIdx, upperLength);

        Target.NumMethodsAttached--;
        Target.MethodIdx = nMethodIdx;
    }
}
