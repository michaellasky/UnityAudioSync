Unity provides a few useful ways of inspecting audio in real time, namely [AudioSource.GetOutputData](http://docs.unity3d.com/ScriptReference/AudioSource.GetOutputData.html) and [AudioSource.GetSpectrumData](http://docs.unity3d.com/ScriptReference/AudioSource.GetSpectrumData.html).  These methods are minimally documented, and I hadn’t seen many working examples besides a few small implementations on Unity Answers and forums, so I decided to do a working example of using this functionality in a generalized way in an actual game.  

Please keep in mind I’m not an Audio Engineer, just a nerd.  Feel free to let me know of any errors in logic, nomenclature, or morality expressed here.  


##MusicWars
The little demo game I made (originally for the first LawlessJam) is called MusicWars, you can try it out below (Unity Web Player required).  Control the ship with WASD, shoot with the primary mouse button.  Dodge enemy bullets, but don’t worry if you get hit because you can’t die.  When you get hit you only lose score.  Powerups will randomly spawn when enemies are shot.  The goal is to get the highest score you can during the selected song.  

    <img src="https://i.imgur.com/Lh3XN4Q.png" width="80%" />

####A few notes about the Demo
* It’s just a demo, and I wanted to keep dependencies to a minimum.  Therefor it uses the old crappy Unity GUI.
* The art was all done quickly in Blender and isn’t supposed to be great, with the exception of those awesome explosions which come from the [Detonator Explosion Framework](http://u3d.as/content/ben-throop/detonator-explosion-framework/1qK)
* The music included in the demo is sourced from the [Open Goldberg Variations](http://www.opengoldbergvariations.org/) and [NoCopyrightSounds](https://soundcloud.com/nocopyrightsounds) on [SoundCloud](http://soundcloud.com).  
* Code for the game itself was mostly thrown together to showcase the Audio Synchronization stuff, so don’t look to it as an example of Unity Best-Practices.  
# 

So how do we sync the firing to the music?  That’s what the code I’m releasing does, and it does so in a pretty generalized way which allows one to sync any behavior to in-game audio.

###AudioWatcher   

The core of the sync functionality is in the AudioWatcher class.  Attach an AudioWatcher to a GameObject somewhere in the scene.  Specific channels to watch can be specified, but generally the channels should just be set to [0, 1] for the L/R stereo channels. 

FFTWindowType will be passed to Unity’s FFT implementation.  You can generally keep this as BlackmanHarris, which should provide the best quality results.  I’ve never seen the window type become a bottleneck in performance, so the high quality BlackmanHarris envelope can be used without much issue.

The AudioWatcher will instantiate 8 AudioBand objects, and during each FixedUpdate step  will populate those AudioBand objects with band-relevant data from AudioSource.GetOuptputData and AudioSource.GetSpectrumData.  

The AudioWatcher also defines an enum with Event Types.  Currently there are two event types, but this can be expanded in the future.  The event detection is implemented in AudioBand.  The event types are currently:

* Beat: Active when the current Median of the spectrum values for the respective band is greater than the Median of the spectrum values in the past multipled by some constant (beatThreshold).   This works best with low-mid frequencies, and will generally fire in time with the “pulse” of that band.
* Vibration: In higher frequencies where the spectrum bandwidth is much greater, the Beat event can break down and not be so reliable.  In this case, the Vibration Event might better suffice.  It fires more on the “texture” of the audio.  Best for rhythmic cymbals like hi-hats or percussive shakers.  

###AudioBand

As data is being populated from the AudioWatcher, the AudioBands will do some simple processing on the data (mostly summing values for Mean calculations and such).  The AudioBand class also defines a boolean property for each AudioWatcher event type, which will be true if that event is considered active for that band.  

###AudioActor

The AudioActor is the main component one works with when defining behavior for music synced objects.  Once attached, the AudioActor component will constantly scan the GameObject’s components looking for methods which match the specific signature for audio events:  

       public void SomeMethod(AudioBand bandData)

Therefor, if one adds a method that matches that signature to any of the other components on the GameObject, it’ll show up as a selection in the AudioActor method list.  For MusicWars, the EWing enemy type defines the method: 

     public class EWing : MonoBehaviour 
     {
          <<...snipped code…>>

          public void Fire(AudioBand bandData)
          {
              <<...snipped code…>>
          }

          <<...snipped code…>>
     }


Once the EWing component has been attached to a GameObject along with an AudioActor object you’ll see:

     <img src="http://i.imgur.com/HebgHP9.png" />

# 
In this case it will call EWing::Fire anytime the Vibration event fires on the LOW, MID, HIGH_MID, or HIGH bands.  

One can attach one or more AudioActor components to a GameObject.  Choose which bands, and event types should trigger your methods, and then choose which method(s) to call when those events are active.  AudioActor will pass the AudioBand object that the event happened on to the event handling method, allowing one to do whatever they like with the data from there.  In the case of MusicWars, I’m using a derivative value of the current SPL of the music to decide how much power and velocity the enemy bullets should have.  

###Problems / Bugs / Gotchas / Todos / Shitty code
* In this release the AudioWatcher object is acting kind of like a Singleton.  There’s no good reason to have the Watcher be a singleton (though in most cases I’d assume that there would only be one in the scene).  The only reason it is currently implemented as a singleton is for my convenience while getting things working.  This will be changed in a followup release, and shouldn’t be hard to change on your own if you need multiple watchers in a scene.
* Event Type definitions and implementations are spread out among all three main classes, and should probably be decoupled and generalized.  This would allow more complex and interesting event types to be defined easily, and make the rest of the code a little cleaner by consolidating all the event stuff in one place.
* I’m using Unity Free, so currently I cannot profile this code.  That said, performance has been good, and the audio processing only takes a couple milliseconds.  
* Initially the number of bands was going to be configurable.  That was too complex and a stupid idea, so now the number of bands is hard set to 8.  There still are some remnants of the old configurable code though, so you might see me doing math when i shouldn’t have to somewhere.  
* The custom EditorWindow stuff is kind of buggy, and if you do something like remove all methods from an AudioActor it can get wacky and freak out.  For now, just remove the component and re-add it.  This will be adressed in a point release soon.  

##Licensing 
All the Audio Synchronization code and MusicWars code are released under the MIT License.  Any other code or assets included in the example project - including but not limited to Detonator Explosion Framework and Songs - are included for convenience and retain their original licenses.  

##Other Useful Resources
* [Unity Script Reference: AudioSource.GetOutputData](http://docs.unity3d.com/ScriptReference/AudioSource.GetOutputData.html)
* [Unity Script Reference: AudioSource.GetSpectrumData](http://docs.unity3d.com/ScriptReference/AudioSource.GetSpectrumData.html)
* [Simple example of using AudioSource.GetOutputData and AudioSource.GetSpectrumData](http://answers.unity3d.com/questions/157940/getoutputdata-and-getspectrumdata-they-represent-t.html)
* [Master Handbook of Acoustics](http://www.amazon.com/Master-Handbook-Acoustics-Alton-Everest/dp/0071603328?tag=donations09-20)

##Credits
* Music in the demo provided by [Open Goldberg Variations](http://www.opengoldbergvariations.org/) and [NoCopyrightSounds](https://soundcloud.com/nocopyrightsounds) 
* Awesome explosions from [Detonator Explosion Framework](http://u3d.as/content/ben-throop/detonator-explosion-framework/1qK)

##Download Source Code
* [Github: Full MusicWars Source](https://github.com/NuclearHorseStudios/MusicWars)
* [Github: Audio Code Only](https://github.com/NuclearHorseStudios/UnityAudioSync)