using System;
using JetBrains.Annotations;
using UniFlow.Message;
using UniRx;
using UnityEngine;

namespace UniFlow.Connector
{
    [AddComponentMenu("UniFlow/AudioController", 303)]
    public class AudioController : ConnectorBase
    {
        [SerializeField] private AudioControlMethod audioControlMethod = default;
        private AudioControlMethod AudioControlMethod
        {
            get => audioControlMethod;
            [UsedImplicitly]
            set => audioControlMethod = value;
        }

        [SerializeField]
        [Tooltip("If you do not specify it will be obtained by AudioSource.clip")]
        private AudioClip audioClip = default;
        private AudioClip AudioClip
        {
            get => audioClip;
            [UsedImplicitly]
            set => audioClip = value;
        }

        private AudioSource audioSource = default;
        private AudioSource AudioSource
        {
            get =>
                audioSource != default
                    ? audioSource
                    : audioSource =
                        GetComponent<AudioSource>() != default
                            ? GetComponent<AudioSource>()
                            : gameObject.AddComponent<AudioSource>();
            [UsedImplicitly]
            set => audioSource = value;
        }

        private IDisposable Disposable { get; } = new CompositeDisposable();

        public override IObservable<EventMessage> OnConnectAsObservable()
        {
            return Observable
                .Create<EventMessage>(
                    observer =>
                    {
                        InvokeAudioSourceMethod();
                        observer.OnNext(EventMessage.Create(ConnectorType.AudioController, AudioSource, AudioControllerEventData.Create(AudioControlMethod)));
                        return Disposable;
                    }
                );
        }

        private void Awake()
        {
            if (AudioClip != default)
            {
                AudioSource.clip = AudioClip;
            }
        }

        private void InvokeAudioSourceMethod()
        {
            switch (AudioControlMethod)
            {
                case AudioControlMethod.Play:
                    AudioSource.Play();
                    break;
                case AudioControlMethod.Stop:
                    AudioSource.Stop();
                    break;
                case AudioControlMethod.Pause:
                    AudioSource.Pause();
                    break;
                case AudioControlMethod.UnPause:
                    AudioSource.UnPause();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnDestroy()
        {
            Disposable.Dispose();
        }
    }

    public enum AudioControlMethod
    {
        Play,
        Stop,
        Pause,
        UnPause,
    }
}