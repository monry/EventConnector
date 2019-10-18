namespace UniFlow
{
    // ReSharper disable InconsistentNaming
    // ReSharper disable UnusedMember.Global
    public enum ConnectorType
    {
        LifecycleEvent            = 100,
        UIBehaviourEventTrigger,
        TransformEvent,
        RectTransformEvent,
        CameraEvent,
        ParticleEvent,
        MouseEvent,
        KeyEvent,

        PhysicsCollisionEvent     = 200,
        PhysicsCollision2DEvent,
        PhysicsTriggerEvent,
        PhysicsTrigger2DEvent,

        ActivationController      = 300,
        InstantiateGameObject,
        DestroyInstance,
        SimpleAnimationController,
        SimpleAnimationEvent,
        AnimatorTrigger,
        AnimationEvent,
        AudioController,
        AudioEvent,
        PlayableController,
        TimelineEvent,
        TimelineSignal,
        RaycasterController,
        RaycastTargetController,
        TransformSiblingController,
        MoveParentTransform,
        DontDestroyOnLoad,

        LoadScene                 = 400,
        LoadScene_Enum,
        LoadSceneEvent,
        UnloadScene,
        UnloadScene_Enum,
        UnloadSceneEvent,

        ValueProviderBool         = 1000,
        ValueProviderByte,
        ValueProviderInt,
        ValueProviderFloat,
        ValueProviderString,
        ValueProviderEnum,
        ValueProviderObject,
        ValueProviderGameObject,
        ValueProviderScriptableObject,
        ValueProviderTransform,
        ValueProviderRectTransform,
        ValueProviderVector2,
        ValueProviderVector3,
        ValueProviderVector4,
        ValueProviderQuaternion,
        ValueProviderVector2Int,
        ValueProviderVector3Int,
        ValueProviderColor,
        ValueProviderColor32,
        ValueProviderRuntimePlatform,
        CustomValueProvider,

        ValueComparerBool         = 2000,
        ValueComparerByte,
        ValueComparerInt,
        ValueComparerFloat,
        ValueComparerString,
        ValueComparerEnum,
        ValueComparerObject,
        ValueComparerGameObject,
        ValueComparerScriptableObject,
        ValueComparerTransform,
        ValueComparerRectTransform,
        ValueComparerVector2,
        ValueComparerVector3,
        ValueComparerVector4,
        ValueComparerQuaternion,
        ValueComparerVector2Int,
        ValueComparerVector3Int,
        ValueComparerColor,
        ValueComparerColor32,
        ValueComparerRuntimePlatform,
        CustomValueComparer,

        ValueInjectorImage        = 3000,
        ValueInjectorRawImage,
        ValueInjectorTimelineControlTrack,
        ValueInjectorTimelineAudioTrack,
        ValueInjectorTimelineAnimationTrack,

        MusicPlayer               = 4000,
        NumberImageRenderer,

        SignalPublisher           = 6000,
        StringSignalPublisher,
        ScriptableObjectSignalPublisher,

        SignalReceiver            = 6500,
        StringSignalReceiver,
        ScriptableObjectSignalReceiver,

        And                       = 8000,
        Or,
        Xor,
        Not,

        Timer                     = 9000,
        TimerFrame,
        Interval,
        IntervalFrame,
        TimeScaleController,
        Empty,

        Toss                      = 9100,
        Receive                   = 9101,

        Preset                    = 10000,

        Receiver                  = 20000,

        Custom                    = -1,
    }
}
