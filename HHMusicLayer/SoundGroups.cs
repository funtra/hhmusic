using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace HHMusicLayer
{
    #region Data Structures Definitions

    [Serializable]
    public struct SoundGroup
    {
        //public AudioSource AudioSource;
        public AudioMixerGroup MixerGroup;
        public AudioMixerSnapshot Snapshot;
        public BackgroundMusicTrack[] tracks;

        public BackgroundMusicTrack RandomTrack() => tracks[UnityEngine.Random.Range(0, tracks.Length)];

        public bool Equals(SoundGroup other)
        {
            return Equals(MixerGroup, other.MixerGroup) && Equals(Snapshot, other.Snapshot) && Equals(tracks, other.tracks);
        }

        public override bool Equals(object obj)
        {
            return obj is SoundGroup other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (MixerGroup != null ? MixerGroup.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Snapshot != null ? Snapshot.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (tracks != null ? tracks.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(SoundGroup left, SoundGroup right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SoundGroup left, SoundGroup right)
        {
            return !left.Equals(right);
        }
    }

    [Serializable]
    public struct BackgroundMusicTrack
    {
        public AudioClip clip;
        // will be precomputed in editor and serialized for runtime
        public double duration;

        public double barsPerClip; // how many bars does this clip has - given we know the BPM and BeatsPerBar, this will depend on clip duration
        // If we don't want these as global - but rather per-track its possible to push these properties down and adjust the code to be per track
        // public float  bpm;
        // public double barLength;
        // public double beatLength;

    }

    #endregion
}