using UnityEngine;

namespace UniFlow
{
    public interface IBaseGameObjectSpecifyable
    {
        GameObject BaseGameObject { get; }
        string TransformPath { get; }
    }

    public static class Extensions
    {
        private static TComponent GetOrAddComponent<TComponent>(this GameObject gameObject) where TComponent : Component
        {
            return gameObject.GetComponent<TComponent>() != default
                ? gameObject.GetComponent<TComponent>()
                : gameObject.AddComponent<TComponent>();
        }

        public static TComponent GetComponent<TComponent>(this IBaseGameObjectSpecifyable baseGameObjectSpecifyable) where TComponent : Component
        {
            return string.IsNullOrEmpty(baseGameObjectSpecifyable.TransformPath)
                ? baseGameObjectSpecifyable.BaseGameObject.GetComponent<TComponent>()
                : baseGameObjectSpecifyable.BaseGameObject.transform.Find(baseGameObjectSpecifyable.TransformPath).gameObject.GetComponent<TComponent>();
        }

        public static TComponent GetOrAddComponent<TComponent>(this IBaseGameObjectSpecifyable baseGameObjectSpecifyable) where TComponent : Component
        {
            return string.IsNullOrEmpty(baseGameObjectSpecifyable.TransformPath)
                ? baseGameObjectSpecifyable.BaseGameObject.GetOrAddComponent<TComponent>()
                : baseGameObjectSpecifyable.BaseGameObject.transform.Find(baseGameObjectSpecifyable.TransformPath).gameObject.GetOrAddComponent<TComponent>();
        }
    }
}
