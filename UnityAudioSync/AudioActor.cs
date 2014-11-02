using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic; 
using System.Reflection;

public class AudioActor : MonoBehaviour 
{
    public int                          channel = 0;
    public AudioWatcher                 audioWatcher;
    public AudioWatcher.Bands[]         bands;
    public AudioWatcher.EventTypes[]    eventTypes;

    public struct      DelegateData
    { 
        public string           methodName;
        public Component        component;
        public OnEventTrigger   method;

        public DelegateData(string n, Component c, OnEventTrigger m)
        {
            methodName = n;
            component  = c;
            method     = m;
        }
    }

    public string[]     MethodNames 
    {
        get { return methodNames;  }
        set { methodNames = value; }
    }

    public int[]        MethodIdx 
    {
        get { return methodIdx;  }
        set { methodIdx = value; }
    }

    public int NumMethodsAttached 
    {
        get { return numMethodsAttached;  }
        set { numMethodsAttached = value; }
    }

    public Dictionary<string, DelegateData> MethodDict 
    {
        get { return mDict;  }
        set { mDict = value; }
    }

    public List<OnEventTrigger> TriggerList
    {
        get { return onEventTrigger;  }
        set { onEventTrigger = value; }
    }

    [SerializeField, HideInInspector]
    private List<OnEventTrigger>        onEventTrigger;

    [SerializeField, HideInInspector]
    private string[] methodNames = new string[0] {};
    
    [SerializeField, HideInInspector]
    private int[]    methodIdx = new int[0] {};
    
    [SerializeField, HideInInspector]
    private int      numMethodsAttached = 0;
    
    [HideInInspector] 
    public Dictionary<string, DelegateData> mDict = 
            new Dictionary<string, DelegateData>();
     
    [Serializable]
    public delegate void OnEventTrigger(AudioBand bandData);

    void OnEnable ()
    {
        FindMethods(gameObject);
        RegisterMethods();
    }

    void FixedUpdate ()
    {
        if (audioWatcher == null) 
        {
            audioWatcher = AudioWatcher.Instance;
            return;
        }
        
        foreach (AudioWatcher.Bands b in bands)
        {
            foreach (AudioWatcher.EventTypes e in eventTypes)
            {
                InvokeEvents(b, e);
            }
        }
    }

    void InvokeEvents (AudioWatcher.Bands b, AudioWatcher.EventTypes e)
    {
        if (!audioWatcher.EventIsActive(b, channel, e)) { return; }

        foreach(OnEventTrigger f in onEventTrigger)
        {
            f.Invoke(audioWatcher.GetBand(b, channel));
        }
    }

    void AddMatchingMethods (Component comp)
    {
        foreach (MethodInfo method in comp.GetType().GetMethods())
        {
            if (MatchesAudioEventSignature(method)) { AddMethod(comp, method); }
        }
    }

    public void FindMethods (GameObject go)
    {
        MethodNames = new string[0] {};
        foreach (Component comp in go.GetComponents<Component>())
        {   
            AddMatchingMethods(comp);
        }
    }

    public void RegisterMethods ()
    {
        var tList = new List<AudioActor.OnEventTrigger>();

        for (int i = 0; i < methodIdx.Length; i++)
        {
            tList.Add(MethodDict[MethodNames[MethodIdx[i]]].method);
        }   

        RegisterEventMethods(tList);
    }

    public void AddMethod (Component c, MethodInfo method)
    {
        Type    type        = typeof(OnEventTrigger);
        string  name        = method.Name;
        string  strIdx      = c.name + "::" + method.Name;
        var     mDelegate   = (OnEventTrigger) 
                              Delegate.CreateDelegate(type, c, method);
        
        MethodDict[strIdx]  = new DelegateData(name, c, mDelegate);

        AddMethodName(strIdx);
    }

    public void RegisterEventMethods (List<OnEventTrigger> tList)
    {
        onEventTrigger = tList;
    }

    public void SetMethodNames (string[] names)
    {
        methodNames = names;
    }

    public void SetMethodIdx (int[] idx)
    {
        methodIdx = idx;
    }

    public void SetNumMethods (int n)
    {
        numMethodsAttached = n;
    }

    void AddMethodName (string name)
    {
        string[] nNames = new string[MethodNames.Length + 1];

        Array.Copy(MethodNames, 0, nNames, 0, MethodNames.Length);

        nNames[MethodNames.Length] = name;

        MethodNames = nNames;
    }

    static bool MatchesAudioEventSignature (MethodInfo method)
    {
        ParameterInfo[] pInfo = method.GetParameters();

        if (pInfo.Length != 1)                    { return false; }
        if (!(method.ReturnType == typeof(void))) { return false; }

        return pInfo[0].ParameterType == typeof(AudioBand);
    }
}
 