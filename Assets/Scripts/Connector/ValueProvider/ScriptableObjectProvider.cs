using UnityEngine;

namespace UniFlow.Connector.ValueProvider
{
    [AddComponentMenu("UniFlow/ValueProvider/ScriptableObject", (int) ConnectorType.ValueProviderScriptableObject)]
    public class ScriptableObjectProvider : ValueProviderBase<ScriptableObject, PublishScriptableObjectEvent>
    {
        protected override ScriptableObject Provide()
        {
            return Value;
        }
    }
}
