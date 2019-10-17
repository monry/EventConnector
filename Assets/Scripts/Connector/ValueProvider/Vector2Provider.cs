using System.Collections.Generic;
using UnityEngine;

namespace UniFlow.Connector.ValueProvider
{
    [AddComponentMenu("UniFlow/ValueProvider/Vector2", (int) ConnectorType.ValueProviderVector2)]
    public class Vector2Provider : ProviderBase<Vector2>, IMessageCollectable, IMessageComposable
    {
        [SerializeField] private FloatCollector xCollector = new FloatCollector();
        [SerializeField] private FloatCollector yCollector = new FloatCollector();

        private FloatCollector XCollector => xCollector;
        private FloatCollector YCollector => yCollector;

        IEnumerable<ICollectableMessageAnnotation> IMessageCollectable.GetMessageCollectableAnnotations() =>
            new ICollectableMessageAnnotation[]
            {
                CollectableMessageAnnotation<float>.Create(XCollector, v => Value = new Vector2(v, Value.y), "X"),
                CollectableMessageAnnotation<float>.Create(YCollector, v => Value = new Vector2(Value.x, v), "Y"),
            };

        IEnumerable<IComposableMessageAnnotation> IMessageComposable.GetMessageComposableAnnotations() =>
            new IComposableMessageAnnotation[]
            {
                ComposableMessageAnnotation<Vector2>.Create(() => Value),
            };
    }
}
