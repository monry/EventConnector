using System.Collections.Generic;
using UnityEngine;

namespace UniFlow.Connector.ValueProvider
{
    [AddComponentMenu("UniFlow/ValueProvider/Quaternion", (int) ConnectorType.ValueProviderQuaternion)]
    public class QuaternionProvider : ProviderBase<Quaternion>, IMessageCollectable, IMessageComposable
    {
        [SerializeField] private FloatCollector xCollector = new FloatCollector();
        [SerializeField] private FloatCollector yCollector = new FloatCollector();
        [SerializeField] private FloatCollector zCollector = new FloatCollector();
        [SerializeField] private FloatCollector wCollector = new FloatCollector();
        [SerializeField] private Vector3Collector eulerAngleCollector = new Vector3Collector();

        private FloatCollector XCollector => xCollector;
        private FloatCollector YCollector => yCollector;
        private FloatCollector ZCollector => zCollector;
        private FloatCollector WCollector => wCollector;
        private Vector3Collector EulerAngleCollector => eulerAngleCollector;

        IEnumerable<ICollectableMessageAnnotation> IMessageCollectable.GetMessageCollectableAnnotations() =>
            new ICollectableMessageAnnotation[]
            {
                CollectableMessageAnnotation<float>.Create(XCollector, v => Value = new Quaternion(v, Value.y, Value.z, Value.w), "X"),
                CollectableMessageAnnotation<float>.Create(YCollector, v => Value = new Quaternion(Value.x, v, Value.z, Value.w), "Y"),
                CollectableMessageAnnotation<float>.Create(ZCollector, v => Value = new Quaternion(Value.x, Value.y, v, Value.w), "Z"),
                CollectableMessageAnnotation<float>.Create(WCollector, v => Value = new Quaternion(Value.x, Value.y, Value.z, v), "W"),
                CollectableMessageAnnotation<Vector3>.Create(EulerAngleCollector, v => Value = Quaternion.Euler(v), "EulerAngle"),
            };

        IEnumerable<IComposableMessageAnnotation> IMessageComposable.GetMessageComposableAnnotations() =>
            new IComposableMessageAnnotation[]
            {
                ComposableMessageAnnotation<Quaternion>.Create(() => Value),
                ComposableMessageAnnotation<Vector3>.Create(() => Value.eulerAngles),
            };
    }
}
