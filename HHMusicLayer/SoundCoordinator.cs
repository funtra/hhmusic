// using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;

namespace HHMusicLayer
{
    public abstract class SoundCoordinator : MonoBehaviour
    {
        private const double SchedulingThreshold = 2d;

        // These will be calculated and serialized by the precompute - derived from BPM and beatsPerBar
        [SerializeField]
        private int BPM;

        [SerializeField]
        private int beatsPerBar;

        [SerializeField]
        private double beatLength;

        [SerializeField]
        private double barLength;

        // Runtime variables
        protected double startTime;
        protected double nextStartTime;
        protected SoundGroup currentSoundGroup;
        protected BackgroundMusicTrack currentTrack;

        public abstract bool AutoPlay { get; }

        [SerializeField]
        private AudioSource[] audioSourceSwitch = new AudioSource[2];

        private int toggle = 0;

        #region Public Properties (some only for editor)
        public bool IsPlaying { get; private set; }
        public AudioSource CurrentSource => audioSourceSwitch?.Length > 0 ? audioSourceSwitch[1-toggle] : null;
        public double NextStartTime => nextStartTime;
        public int Toggle => toggle;
        public double BeatLength => beatLength;
        public double BarLength => barLength;

        public SoundGroup CurrentSoundGroup => currentSoundGroup;

        #endregion

        #region Missing Abstract Methods
        protected abstract void OnPrecomputeSoundValues();

        #endregion


        #region Public API

        public void Play()
        {
            PlayCurrent();
        }

        public void FadeOutAndStop(float fadeDuration = 1f)
        {
            var audioSource = audioSourceSwitch[1-toggle];
            // You'll need some tweening library like DOTween for this, or implement your own fading sequence
            // DOVirtual.Float(audioSource.volume, 0, fadeDuration, (newVolume) =>
            // {
            //     audioSource.volume = newVolume;
            // }).OnComplete(() =>
            // {
                Stop();
                audioSource.volume = 1f;
            // });

        }

        public void Stop()
        {
            audioSourceSwitch[0].Stop();
            audioSourceSwitch[1].Stop();
            // nextSoundGroup.AudioSource.Stop();
            toggle = 0;
            startTime = 0;
            nextStartTime = 0;
            IsPlaying = false;
        }

        public void NextTrack()
        {
            TransitionToGroup(currentSoundGroup);
        }

        #endregion

        #region Unity Lifecycle Events

        private void OnEnable()
        {
            Initialize();
        }

        private void OnValidate()
        {
            if (audioSourceSwitch.Length != 2)
            {
                Debug.LogWarning("Expecting exactly 2 local AudioSources");
            }
        }

        public void Reset()
        {
            Initialize();
        }

        private void Start()
        {
            // Play(); // if you have your own SoundManager trigger play when you're ready, or uncomment to have this auto play
        }

        private void Update()
        {
            if (!IsPlaying) return;

            // During normal operations, without any external factors - our update loop should simply transition to the next (random)
            // song/track in the current soundgroup.
            // when an external event happens it will do the transition so this just has to take care of moving in the soundgroup
            if (AudioSettings.dspTime > nextStartTime - SchedulingThreshold)
            {
                ScheduleTransitionToTrack(currentSoundGroup, nextStartTime);
            }

        }

        #endregion

        #region Private Implementation
        protected virtual void Initialize()
        {
            toggle = 0;
            nextStartTime = 0;
            IsPlaying = false;
        }

        protected void PlayCurrent()
        {
            startTime = AudioSettings.dspTime;
            currentTrack = currentSoundGroup.RandomTrack();

            if (currentSoundGroup.MixerGroup)
            {
                audioSourceSwitch[toggle].outputAudioMixerGroup = currentSoundGroup.MixerGroup;
                AudioMixer mixer = currentSoundGroup.MixerGroup.audioMixer;
                mixer.TransitionToSnapshots(new[] { currentSoundGroup.Snapshot }, new[] {1.0f}, 0f );
            }
            audioSourceSwitch[toggle].clip = currentTrack.clip;
            audioSourceSwitch[toggle].PlayScheduled(startTime);
            nextStartTime = startTime + currentTrack.duration;
            toggle = 1 - toggle;
            IsPlaying = true;
        }

        public virtual void TransitionToGroup(SoundGroup nextGroup)
        {
            var nextBarTime = NextBarTime();
            ScheduleTransitionToTrack(nextGroup, nextBarTime);
        }

        public void PlayGroup(SoundGroup nextGroup)
        {
            ScheduleTransitionToTrack(nextGroup, AudioSettings.dspTime);
        }

        protected void ScheduleTransitionToTrack(SoundGroup nextGroup, double atTime)
        {
            BackgroundMusicTrack nextTrack = nextGroup.RandomTrack();

            // first check if next audio switch is already scheduled but hasn't started.
            // 1-toggle is always currently playing, and toggle is the next source
            // if startTime is in the future - it means something has already been scheduled and the toggle switched
            // so if we switch it back - we're reschedule the next over the previously scheduled thing on the correct source
            // and not on what's currently playing.
            // it's also means that atTime == startTime as if something is already scheduled - its scheduled to the next bar, and so is this new requested transition
            // will either be now, or next bar
            if (startTime > AudioSettings.dspTime)
            {
                toggle = 1 - toggle; // switch toggle back - so the rest of the flow schedules the new group on the same "pending" audio source
            }

            // then set current playing source to end atTime
            audioSourceSwitch[1-toggle].SetScheduledEndTime(atTime);

            // And schedule the next source to start atTime
            audioSourceSwitch[toggle].clip = nextTrack.clip;
            if (nextGroup.MixerGroup)
            {
                audioSourceSwitch[toggle].outputAudioMixerGroup = nextGroup.MixerGroup;
                AudioMixer mixer = nextGroup.MixerGroup.audioMixer;
                mixer.TransitionToSnapshots(new[] { nextGroup.Snapshot }, new[] {1.0f}, 0f );
            }

            audioSourceSwitch[toggle].PlayScheduled(atTime);

            // Update state
            startTime = atTime;
            nextStartTime = atTime + nextTrack.duration;
            currentTrack = nextTrack;
            currentSoundGroup = nextGroup;
            // Switches the toggle to use the other Audio Source next
            toggle = 1 - toggle;
        }

        private double NextBarTime()
        {
            // note: if we ever have clips with different BPMs, we need to pass BarLength as parameter to NextBarTime. Right now use global as all songs share BPM
            var currentDSPTime = AudioSettings.dspTime;
            if (startTime > currentDSPTime)
            {
                // if startTime is in the future, greater than current DSP Time, it means something has already been scheduled to start
                // at the point in time in the future - this we can assume startTime is already on-beat
                return startTime;
            }
            double remainder = (currentDSPTime - startTime) % barLength;
            double nextBarTime = currentDSPTime + barLength - remainder;
            return nextBarTime;

        }

        #endregion

        #region Editor & Precopmuting

        public void PrecomputeSoundsValues()
        {
            // Now we calculate barLength and beatLength based on first track - since all clips share BPM and BeatsPerBar
            // but if we wanted this to vary between clips we can move it up into the loop and add fields for it
            // Note: beatLength, and more specifically, barLength need to be computed before
            // per-clip data is computed since it requires barLength as input
            beatLength = 60d / BPM;
            barLength = beatLength * beatsPerBar;
            OnPrecomputeSoundValues();
        }

        protected SoundGroup PrecomputeSoundGroup(SoundGroup sg)
        {
            if (sg == null || sg.tracks.Length == 0) return sg;

            for (var i = 0; i < sg.tracks.Length; i++)
            {
                BackgroundMusicTrack track = sg.tracks[i];
                if (!track.clip)
                {
                    Debug.LogWarning($"Empty clip found - skipping processing of SoundGroup {sg}");
                    return sg;
                }

                // Take first clip in the list, divide sample number by frequency
                track.duration = (double)track.clip.samples / track.clip.frequency;

                track.barsPerClip = track.duration / barLength;

                // To get length of each bar, we divide the length of the clip by bars-per-clip
                // track.barLength = track.duration / track.barsPerClip;

                // the length of each beat - is number of bars divided by beats-per-bar
                // track.beatLength = track.barLength / beatsPerBar;
                sg.tracks[i] = track; // These are structs, they change by value
            }

            return sg;
        }

        #endregion
    }

}