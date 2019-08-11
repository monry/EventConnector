using System;
using System.Linq;
using JetBrains.Annotations;
using UniFlow.Message;
using UniRx;
using UnityEngine;

namespace UniFlow.Connector
{
    [AddComponentMenu("UniFlow/SimpleAnimationController", 300)]
    public class SimpleAnimationController : ConnectorBase
    {
        [SerializeField] private SimpleAnimationControlMethod simpleAnimationControlMethod = default;
        private SimpleAnimationControlMethod SimpleAnimationControlMethod
        {
            get => simpleAnimationControlMethod;
            [UsedImplicitly]
            set => simpleAnimationControlMethod = value;
        }

        [SerializeField]
        [Tooltip("If you do not specify it will be used SimpleAnimation setting")]
        private AnimationClip animationClip = default;
        private AnimationClip AnimationClip
        {
            get => animationClip;
            [UsedImplicitly]
            set => animationClip = value;
        }

        [SerializeField] private AnimatorCullingMode cullingMode = AnimatorCullingMode.AlwaysAnimate;
        private AnimatorCullingMode CullingMode
        {
            get => cullingMode;
            [UsedImplicitly]
            set => cullingMode = value;
        }

        [SerializeField] private AnimatorUpdateMode updateMode = AnimatorUpdateMode.Normal;
        private AnimatorUpdateMode UpdateMode
        {
            get => updateMode;
            [UsedImplicitly]
            set => updateMode = value;
        }

        private Animator animator = default;
        private Animator Animator
        {
            get =>
                animator != default
                    ? animator
                    : animator =
                        GetComponent<Animator>() != default
                            ? GetComponent<Animator>()
                            : gameObject.AddComponent<Animator>();
            [UsedImplicitly]
            set => animator = value;
        }

        private SimpleAnimation simpleAnimation = default;
        private SimpleAnimation SimpleAnimation
        {
            get =>
                simpleAnimation != default
                    ? simpleAnimation
                    : simpleAnimation =
                        GetComponent<SimpleAnimation>() != default
                            ? GetComponent<SimpleAnimation>()
                            : gameObject.AddComponent<SimpleAnimation>()
            ;
            [UsedImplicitly]
            set => simpleAnimation = value;
        }

        private IDisposable Disposable { get; } = new CompositeDisposable();

        public override IObservable<EventMessage> OnConnectAsObservable()
        {
            return Observable
                .Create<EventMessage>(
                    observer =>
                    {
                        InvokeSimpleAnimationMethod();
                        observer.OnNext(EventMessage.Create(ConnectorType.SimpleAnimationController, SimpleAnimation, SimpleAnimationControllerEventData.Create(SimpleAnimationControlMethod)));
                        return Disposable;
                    }
                );
        }

        private void Awake()
        {
            // ReSharper disable once InvertIf
            // Automatic add components Animator and SimpleAnimation if AudioClip specified and Animator component does not exists.
            if (AnimationClip != default && Animator == default && SimpleAnimation.GetStates().All(x => x.clip != AnimationClip))
            {
                SimpleAnimation.AddClip(AnimationClip, AnimationClip.GetInstanceID().ToString());
                SimpleAnimation.cullingMode = CullingMode;
                Animator.updateMode = UpdateMode;
            }
        }

        private void InvokeSimpleAnimationMethod()
        {
            switch (SimpleAnimationControlMethod)
            {
                case SimpleAnimationControlMethod.Play:
                    if (AnimationClip == default)
                    {
                        SimpleAnimation.Rewind();
                        SimpleAnimation.Play();
                    }
                    else
                    {
                        SimpleAnimation.Rewind(AnimationClip.GetInstanceID().ToString());
                        SimpleAnimation.Play(AnimationClip.GetInstanceID().ToString());
                    }
                    break;
                case SimpleAnimationControlMethod.Stop:
                    if (AnimationClip == default)
                    {
                        SimpleAnimation.Stop();
                    }
                    else
                    {
                        SimpleAnimation.Stop(AnimationClip.GetInstanceID().ToString());
                    }
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

    public enum SimpleAnimationControlMethod
    {
        Play,
        Stop,
    }
}