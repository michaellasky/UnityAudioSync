using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioWatcher : MonoBehaviour 
{
    public static AudioWatcher Instance { get { return instance; } }

    public  enum        Bands 
    {
        ULTRA_LOW   = 0,
        LOW         = 1,
        LOW_MID     = 2,
        MID         = 3,
        HIGH_MID    = 4,
        LOW_HIGH    = 5,
        HIGH        = 6,
        ULTRA_HIGH  = 7
    //     CURR_MAX    = 8,
    //    CURR_MIN    = 9,
    //    ANY         = 10,
    //    ALL         = 11
    }

    public  enum             EventTypes
    {
        BEAT       = 1,
        VIBRATION  = 2
    }

    public  int[]            channels;

    public  FFTWindow        fftWindowType   = FFTWindow.BlackmanHarris;

    private AudioSource      audioSource;
    
    [SerializeField]
    private AudioBand[]      bandData;
    private List<AudioActor> actors;
    private int[]            bandWidth;
    private float[]          spectrumData;
    private float[]          outputData;
    private int              baseBands;
    private int              numBands        = 8;
    private int              spectrumDensity = 2048;
    private bool             initialized     = false;
    private int[,]           bandOffsets;

    private static AudioWatcher instance;
    
    void Awake ()
    {   
        instance      = this;
        actors        = new List<AudioActor>();
        bandOffsets   = new int[channels.Length, numBands];
        spectrumData  = new float[spectrumDensity];
        outputData    = new float[spectrumDensity];
        audioSource   = gameObject.GetComponent<AudioSource>();
        baseBands     = (int) Mathf.Ceil(Mathf.Log(spectrumDensity));
    }

    void Start () 
    {
        CalcBandwidth();
        InitializeBandData();

        initialized = true;
    }

    void FixedUpdate () 
    {
        if (!initialized) { return; }

        PopulateBandData(); 
    }

    void InitializeBandData ()
    {
        bandData = new AudioBand[numBands * channels.Length];
        
        foreach (int c in channels)
        {
            for (int band = 0; band < numBands; band++)
            {
                int idx = band + (numBands * c);
                bandData[idx] = new AudioBand(bandWidth[band], c, (Bands) band);
                bandOffsets[c,band] = idx;
            }
        }
    }
    
    void CalcBandwidth ()
    {
        // A smarter person can do this with maths
        bandWidth = new int[numBands];
        for (int i = 1; i < spectrumDensity - 1; i++) 
        {
            int band = (int) Mathf.Floor(Mathf.Log(i));
            bandWidth[band]++;
        }
    }

    void PopulateBandData ()
    {
        foreach (int c in channels)
        {
            audioSource.GetSpectrumData(spectrumData, c, fftWindowType);
            audioSource.GetOutputData(outputData, c);
            
            int channelOffset = numBands * c;
            int prevBand   = -1;
            for (int i = 1; i < spectrumDensity - 1; i++) 
            {
                int loopBand = (int) Mathf.Floor(Mathf.Log(i)) + channelOffset;

                if (loopBand != prevBand) 
                { 
                    prevBand = loopBand; 
                    bandData[loopBand].Reset(); 
                }
                
                bandData[loopBand].AddSpectrumData(spectrumData[i]);
                bandData[loopBand].AddOutputData(outputData[i]);
            }
        } 
    }

    public AudioBand GetBand (Bands b, int c)
    {
        int channelOffset = numBands * c;
     
        return bandData[((int) b) + channelOffset];
    }

    public bool EventIsActive (Bands b, int c, EventTypes e)
    {
        if (e == EventTypes.BEAT)           
        { 
            return bandData[bandOffsets[c, (int) b]].Beat;
        } 
        else if (e == EventTypes.VIBRATION) 
        { 
            return bandData[bandOffsets[c, (int) b]].Vibration; 
        }

        return false;
    }

    public float CurrentSPL (int channel)
    {
        float splSum = 0;
        int   offset = channel * numBands;

        for (int band = 0; band < numBands; band++) 
        {
            splSum += bandData[band + offset].SPL;
        }

        return splSum / numBands;
    }
} 
