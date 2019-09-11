using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using UniRx;
using UniRx.Async;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace UniFlow.Connector.Controller
{
    public abstract class LoadSceneBase : ConnectorBase
    {
        [UsedImplicitly]
        public abstract IEnumerable<string> SceneNames { get; set; }

        public override IObservable<IMessage> OnConnectAsObservable(IMessage latestMessage)
        {
            return LoadScenes()
                .ToObservable()
                .Select(_ => CreateMessage());
        }

        protected abstract IMessage CreateMessage();

        private async UniTask LoadScenes()
        {
            foreach (var sceneName in SceneNames)
            {
                await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            }
        }
    }

    [AddComponentMenu("UniFlow/Controller/LoadScene", (int) ConnectorType.LoadScene)]
    public class LoadScene : LoadSceneBase
    {
        [SerializeField] private List<string> sceneNames = default;

        [UsedImplicitly] public override IEnumerable<string> SceneNames
        {
            get => sceneNames;
            set => sceneNames = value.ToList();
        }

        protected override IMessage CreateMessage()
        {
            return Message.Create(this);
        }

        public class Message : MessageBase<LoadScene>
        {
            public static Message Create(LoadScene sender)
            {
                return Create<Message>(ConnectorType.LoadScene, sender);
            }
        }
    }

    public abstract class LoadScene<TSceneName> : LoadSceneBase where TSceneName : Enum
    {
        [SerializeField] private List<TSceneName> sceneNames = default;

        [InjectOptional(Id = InjectId.SceneNamePrefix)] private string SceneNamePrefix { get; }

        [UsedImplicitly] public override IEnumerable<string> SceneNames
        {
            get => sceneNames.Select(x => $"{SceneNamePrefix}{x.ToString()}");
            set => sceneNames = value
                .Select(
                    x => (TSceneName) Enum
                        .Parse(
                            typeof(TSceneName),
                            Regex.Replace(x, $"^{SceneNamePrefix}", string.Empty)
                        )
                )
                .ToList();
        }

        protected override IMessage CreateMessage()
        {
            return Message.Create(this);
        }

        public class Message : MessageBase<LoadScene<TSceneName>>
        {
            public static Message Create(LoadScene<TSceneName> sender)
            {
                return Create<Message>(ConnectorType.LoadScene_Enum, sender);
            }
        }
    }
}