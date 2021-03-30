using System.Collections.Generic;
using System.Linq;
using Embedded.Data;

// ReSharper disable CheckNamespace
namespace Embedded {
    namespace Data {
        public record Vector3(float X, float Y, float Z) {
            public Vector3(float value) : this(value, value, value) {
            }

            public static Vector3 operator +(Vector3 first, Vector3 second) {
                return new(first.X + second.X, first.Y + second.Y, first.Z + second.Z);
            }

            public static Vector3 operator -(Vector3 first, Vector3 second) {
                return new(first.X - second.X, first.Y - second.Y, first.Z - second.Z);
            }

            public static Vector3 operator *(Vector3 first, Vector3 second) {
                return new(first.X * second.X, first.Y * second.Y, first.Z * second.Z);
            }

            public static Vector3 operator *(Vector3 first, float second) {
                return new(first.X * second, first.Y * second, first.Z * second);
            }

            public override string ToString() {
                return $"[{X}, {Y}, {Z}]";
            }
        }
        public class Transform {
            public Vector3 Position;
            public Vector3 Scale;
            public Vector3 EulerAngles;

            public Transform() : this(new Vector3(0), new Vector3(1), new Vector3(0)) {
            }

            /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
            public Transform(Vector3 position, Vector3 scale, Vector3 eulerAngles) {
                Position = position;
                Scale = scale;
                EulerAngles = eulerAngles;
            }

            /// <summary>Returns a string that represents the current object.</summary>
            /// <returns>A string that represents the current object.</returns>
            public override string ToString() {
                return $"Transform [\n  Position: {Position},\n  Scale: {Scale},\n  EulerAngles: {EulerAngles}\n]";
            }
        }
    }
    namespace GameObject {
        public class GameObject {
            private static int instanceCount = 0;
            private readonly Transform transform = new();
            private readonly List<Component> components = new();
            public string Name;

            public Vector3 Position {
                get => transform.Position;
                set => transform.Position = value;
            }
            public Vector3 Scale {
                get => transform.Scale;
                set => transform.Scale = value;
            }
            public Vector3 EulerAngles {
                get => transform.EulerAngles;
                set => transform.EulerAngles = value;
            }

            public GameObject() : this($"{nameof(GameObject)}_{instanceCount}"){
            }

            private GameObject(string name) {
                Name = name;
                instanceCount++;
            }

            public void Start() {
                foreach (var component in components) {
                    component.OnStart();
                }
            }

            public void Update(float delta) {
                foreach (var component in components) {
                    component.OnUpdate(delta);
                }
            }

            public void AddComponent<T>(T component) where T : Component {
                component.Owner = this;
                components.Add(component);
            }

            public void RemoveComponent<T>(T component) where T : Component {
                if (!components.Remove(component)) return;
                component.Owner = null;
            }

            public bool HasComponent<T>() where T : Component {
                return components.Any(component => component is T);
            }

            public T GetComponent<T>() where T : Component {
                return components.First(component => component is T) as T;
            }

            /// <summary>Returns a string that represents the current object.</summary>
            /// <returns>A string that represents the current object.</returns>
            public override string ToString() {
                return $"{Name}:\n{transform}";
            }
        }
        public abstract class Component {
            public GameObject Owner;

            public virtual void OnStart() {
            }

            public virtual void OnUpdate(float delta) {
            }
        }
    }

    namespace Helper {
        public static class TypeHelper {
            public static System.Type GetType(object obj) => obj.GetType();
            public static System.Type GetType<T>() => typeof(T);
        }
    }
}
// ReSharper restore CheckNamespace
