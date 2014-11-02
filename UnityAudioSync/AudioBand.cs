    using UnityEngine;
    using System;
    using System.Collections;

    [Serializable]
    public class AudioBand 
    {
        public  int      Channel        { get { return channel;             } }
        public  float    SpectrumMax    { get { return spectrumMax;         } }
        public  float    SpectrumMin    { get { return spectrumMin;         } }
        public  float    SpectrumMean   { get { return spectrumMean;        } }
        public  float    SpecMeanAvg    { get { return specMeanAvg;         } }
        public  float    SpecMedianAvg  { get { return specMedianAvg;       } }
        public  float    OutputMean     { get { return outputMean;          } }
        public  float    SPL            { get { return spl;                 } }
        public  float    SpectrumMedian { get { return spectrumMedian;      } }

        public  bool     Active 
        {
            get { return SpectrumMax > activationLevel; }
        }

        public bool      Vibration
        {
            get { return !wasActive && Active; }
        }

        public bool      Beat 
        {
            get 
            { 
                return SpectrumMedian > SpecMedianAvg * beatThreshold && Active; 
            }
        }

        public  float    OutputMedian 
        {   
            get 
            {   
                if (outputMedian == 0)   { outputMedian = GetMedian(output);  }
                return outputMedian; 
            }
        }

        public  AudioWatcher.Bands  band;

        private int      bandWidth;
        private int      channel;
        private int      specMeanHistoryLength   = 8;
        private int      specMedianHistoryLength = 8;

        [SerializeField]
        private float    spl;

        [SerializeField]
        private float    spectrumMax;
        
        [SerializeField]
        private float    spectrumMin;
        
        [SerializeField]
        private float    spectrumMedian = 0;
        
        [SerializeField]
        private float    spectrumMean = 0;

        [SerializeField]
        private float    outputMedian = 0;
        
        [SerializeField]
        private float    outputMean = 0;
        
        [SerializeField]
        private float    spectrumSum = 0;

        [SerializeField]
        private float    outputSum = 0;

        [SerializeField]
        private float[]  spectrum;

        [SerializeField]
        private float[]  output;

        [SerializeField]
        private int      spectrumCnt = 0;

        [SerializeField]
        private int      outputCnt = 0;

        [SerializeField]
        private float    activationLevel = 0.01f;

        [SerializeField]
        private float    beatThreshold   = 2.2f; // magic and arbitrary

        [SerializeField]
        private bool     wasActive;

        [SerializeField]
        private bool     beat;
        
        [SerializeField]
        private bool     active;

        [SerializeField]
        private float    specMeanAvg = 0;

        [SerializeField]
        private float    specMedianAvg = 0;

        [SerializeField]
        private float[]  specMeanHistory;

        [SerializeField]
        private float[]  specMedianHistory;

        public AudioBand(int bw, int c, AudioWatcher.Bands b)
        {
            bandWidth         = bw; 
            channel           = c;
            band              = b;
            specMeanHistory   = new float[specMeanHistoryLength];
            specMedianHistory = new float[specMedianHistoryLength];
            spectrum          = new float[bandWidth];
            output            = new float[bandWidth];
            
            Reset();
        }

        public void Reset ()
        {
            UpdateSpecMeanHistory();
            UpdateSpecMedianHistory();

            //tmp
            beat            = Beat;
            wasActive       = active;
            active          = Active;

            spl             = GetSPL();
            spectrumMedian  = GetMedian(spectrum);
            specMedianAvg   = GetSpecMedianAvg();
            specMeanAvg     = GetSpecMeanAvg();
            outputMean      = GetOutputMean(); 
            spectrumMean    = GetSpectrumMean();
            spectrumCnt     = 0;
            spectrumSum     = 0;
            outputCnt       = 0;
            outputSum       = 0;
            spectrumMax     = 0;
            outputMedian    = 0;
            spectrumMin     = Mathf.Infinity;
        }

        public void AddSpectrumData (float val)
        {
            spectrum[spectrumCnt++] = val;

            spectrumSum += val;

            if      (val > spectrumMax) { spectrumMax = val; }
            else if (val < spectrumMin) { spectrumMin = val; }
        }

        public void AddOutputData (float val)
        {
            output[outputCnt++] = val;
            outputSum += val;
        }

        void UpdateSpecMeanHistory ()
        {
            Array.Copy( specMeanHistory, 
                        0, 
                        specMeanHistory, 
                        1, 
                        specMeanHistoryLength - 1);

            specMeanHistory[0] = SpectrumMean;
        }

        void UpdateSpecMedianHistory ()
        {
            Array.Copy( specMedianHistory, 
                        0, 
                        specMedianHistory, 
                        1, 
                        specMedianHistoryLength - 1);

            specMedianHistory[0] = SpectrumMedian;
        }

        float GetMedian (float[] data)
        {
            // Just take a half value instead of sorting the array
            return (spectrumMax - spectrumMin) * 0.5f;
        }

        float GetSpectrumMean ()
        {
            spectrumMean = spectrumSum / bandWidth;
            
            return spectrumMean;
        }

        float GetSpecMeanAvg ()
        {
            float sum = 0;
            
            for (int i = 0; i < specMeanHistoryLength; i++)
            {
                sum += specMeanHistory[i];
            }
            
            return sum / specMeanHistoryLength;
        }

        float GetSpecMedianAvg ()
        {
            float sum = 0;

            for (int i = 0; i < specMedianHistoryLength; i++)
            {
                sum += specMedianHistory[i];
            }
            
            return sum / specMedianHistoryLength;
        }

        float GetSPL ()
        {
            float rms  = Mathf.Sqrt(OutputMean);
            float nSpl = 20f * Mathf.Log10(rms);

            if (nSpl < -(160f) ||  float.IsNaN(nSpl)) { nSpl = -(160f); }

            return nSpl;
        }

        float GetOutputMean ()
        {
            return  outputSum / bandWidth;;
        }
    }
