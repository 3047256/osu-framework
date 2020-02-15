﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using ManagedBass;
using osuTK;

namespace osu.Framework.Audio.Sample
{
    public sealed class SampleChannelBass : SampleChannel, IBassAudio
    {
        private volatile int channel;
        private volatile bool playing;

        public override bool IsLoaded => Sample.IsLoaded;

        private float initialFrequency;

        public SampleChannelBass(Sample sample, Action<SampleChannel> onPlay)
            : base(sample, onPlay)
        {
        }

        void IBassAudio.UpdateDevice(int deviceIndex)
        {
            // Channels created from samples can not be migrated, so we need to ensure
            // a new channel is created after switching the device. We do not need to
            // manually free the channel, because our Bass.Free call upon switching devices
            // takes care of that.
            channel = 0;
        }

        private bool pausedDueToZeroFrequency;

        internal override void OnStateChanged()
        {
            base.OnStateChanged();

            if (channel == 0)
                return;

            Bass.ChannelSetAttribute(channel, ChannelAttribute.Volume, AggregateVolume.Value);
            Bass.ChannelSetAttribute(channel, ChannelAttribute.Pan, AggregateBalance.Value);
            Bass.ChannelSetAttribute(channel, ChannelAttribute.Frequency, bassFreq);

            // Handle channels with 0 frequencies due to BASS not supporting them (0 = original rate)
            // Documentation for the frequency limits: http://bass.radio42.com/help/html/ff7623f0-6e9f-6be8-c8a7-17d3a6dc6d51.htm
            if (AggregateFrequency.Value == 0 && !pausedDueToZeroFrequency)
            {
                pausedDueToZeroFrequency = true;
                Bass.ChannelPause(channel);
            }
            else if (AggregateFrequency.Value > 0 && pausedDueToZeroFrequency)
            {
                pausedDueToZeroFrequency = false;
                Bass.ChannelPlay(channel);
            }
        }

        private double bassFreq => MathHelper.Clamp(initialFrequency * AggregateFrequency.Value, 100, 100000);

        public override bool Looping
        {
            get => base.Looping;
            set
            {
                base.Looping = value;
                setLoopFlag(Looping);
            }
        }

        public override void Play(bool restart = true)
        {
            EnqueueAction(() =>
            {
                if (!IsLoaded)
                {
                    channel = 0;
                    return;
                }

                // Remove looping flag from previous channel to prevent endless playback
                setLoopFlag(false);

                // We are creating a new channel for every playback, since old channels may
                // be overridden when too many other channels are created from the same sample.
                channel = ((SampleBass)Sample).CreateChannel();
                Bass.ChannelSetAttribute(channel, ChannelAttribute.NoRamp, 1);
                Bass.ChannelGetAttribute(channel, ChannelAttribute.Frequency, out initialFrequency);
                setLoopFlag(Looping);
            });

            InvalidateState();

            // Needs to happen on the main thread such that
            // Played does not become true for a short moment.
            playing = true;

            base.Play(restart);

            if (AggregateFrequency.Value == 0)
            {
                pausedDueToZeroFrequency = true;
                return;
            }

            EnqueueAction(() =>
            {
                if (channel != 0)
                    Bass.ChannelPlay(channel, restart);
            });
        }

        protected override void UpdateState()
        {
            playing = channel != 0 && Bass.ChannelIsActive(channel) != 0;
            base.UpdateState();
        }

        public override void Stop()
        {
            if (channel == 0) return;

            base.Stop();

            EnqueueAction(() =>
            {
                Bass.ChannelStop(channel);
                // ChannelStop frees the channel.
                channel = 0;
            });
        }

        public override bool Playing => playing;

        private void setLoopFlag(bool value) => EnqueueAction(() =>
        {
            if (channel != 0)
                Bass.ChannelFlags(channel, value ? BassFlags.Loop : BassFlags.Default, BassFlags.Loop);
        });
    }
}
