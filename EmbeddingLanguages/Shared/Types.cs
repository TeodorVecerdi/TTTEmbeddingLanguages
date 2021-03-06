using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Embedded.Components;
using Embedded.Data;
using UnityCommons;
using Vector3 = UnityEngine.Vector3;

// ReSharper disable CheckNamespace
namespace Embedded {
    public class BigObject {
        public Transform Transform1;
        public Transform Transform2;
        public Transform Transform3;
        public Vector3 Vector11;
        public Vector3 Vector21;
        public Vector3 Vector31;
        public Vector3 Vector12;
        public Vector3 Vector22;
        public Vector3 Vector32;
        public Vector3 Vector13;
        public Vector3 Vector23;
        public Vector3 Vector33;

        public BigObject() {
            Vector11 = new Vector3(Rand.Float, Rand.Float, Rand.Float);
            Vector21 = new Vector3(Rand.Float, Rand.Float, Rand.Float);
            Vector31 = new Vector3(Rand.Float, Rand.Float, Rand.Float);
            Vector12 = new Vector3(Rand.Float, Rand.Float, Rand.Float);
            Vector22 = new Vector3(Rand.Float, Rand.Float, Rand.Float);
            Vector32 = new Vector3(Rand.Float, Rand.Float, Rand.Float);
            Vector13 = new Vector3(Rand.Float, Rand.Float, Rand.Float);
            Vector23 = new Vector3(Rand.Float, Rand.Float, Rand.Float);
            Vector33 = new Vector3(Rand.Float, Rand.Float, Rand.Float);
            Transform1 = new Transform(new Vector3(Rand.Float, Rand.Float, Rand.Float),
                                       new Vector3(Rand.Float, Rand.Float, Rand.Float),
                                       new Vector3(Rand.Float, Rand.Float, Rand.Float));
            Transform2 = new Transform(new Vector3(Rand.Float, Rand.Float, Rand.Float),
                                       new Vector3(Rand.Float, Rand.Float, Rand.Float),
                                       new Vector3(Rand.Float, Rand.Float, Rand.Float));
            Transform3 = new Transform(new Vector3(Rand.Float, Rand.Float, Rand.Float),
                                       new Vector3(Rand.Float, Rand.Float, Rand.Float),
                                       new Vector3(Rand.Float, Rand.Float, Rand.Float));
        }
    }

    public class BiggerObject {
        public Vector3[] Vectors;

        public BiggerObject() {
            const int count = 83333;
            Vectors = new Vector3[count];
            for (var i = 0; i < count; i++) {
                Vectors[i] = new Vector3(Rand.Float, Rand.Float, Rand.Float);
            }
        }
    }

    namespace Data {
        public struct Transform {
            public Vector3 Position;
            public Vector3 Scale;
            public Vector3 EulerAngles;

            public Transform(int a = 0) : this(new Vector3(0, 0, 0), new Vector3(1, 1, 1), new Vector3(0, 0, 0)) {
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

    public class GameObject {
        private static int instanceCount = 0;
        private Transform transform;
        private readonly List<Component> components = new();
        private bool isDestroying;

        public string Name;
        public string Tag;

        public List<Component> Components => components;

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

        public GameObject() : this($"{nameof(GameObject)}_{instanceCount}", "Object") {
        }

        private GameObject(string name, string tag) {
            Name = name;
            Tag = tag;
            instanceCount++;

            GameManager.Instance.Register(this);
        }

        public void Awake() {
            foreach (var component in components) {
                component.OnAwake();
            }
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

        public void Destroy() {
            foreach (var component in components) {
                component.OnDestroy();
            }
        }

        public void AddComponent<T>(T component) where T : Component {
            component.Owner = this;
            components.Add(component);

            if (typeof(T) == typeof(SphereCollider)) {
                CollisionManager.Register(this, component as SphereCollider);
            }

            if (GameManager.Instance.GameStarted) {
                GameManager.Instance.QueueComponentInitialization(component);
            }
        }

        public void RemoveComponent<T>(T component) where T : Component {
            if (!components.Remove(component)) return;
            component.Owner = null;
            if (typeof(T) == typeof(SphereCollider)) {
                CollisionManager.Deregister(this, component as SphereCollider);
            }
        }

        public bool HasComponent<T>() where T : Component {
            return components.Any(component => component is T);
        }

        public T GetComponent<T>() where T : Component {
            return components.FirstOrDefault(component => component is T) as T;
        }

        public IEnumerable<T> GetComponents<T>() where T : Component {
            return components.OfType<T>();
        }

        public static void Destroy(GameObject gameObject) {
            if (gameObject.isDestroying) return;
            gameObject.isDestroying = true;
            GameManager.Instance.QueueDestroy(gameObject);
        }

        public static GameObject FindGameObject(string name) => GameManager.Instance.FindGameObject(name);

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() {
            return $"{Name}:\n{transform}";
        }
    }

    public abstract class Component {
        public GameObject Owner;

        public virtual void OnAwake() {
        }

        public virtual void OnStart() {
        }

        public virtual void OnUpdate(float delta) {
        }

        public virtual void OnDestroy() {
        }

        public virtual void OnCollisionEnter(GameObject other) {
        }

        public virtual void OnCollisionStay(GameObject other) {
        }

        public virtual void OnCollisionExit(GameObject other) {
        }

        public static void Destroy(GameObject gameObject) => GameObject.Destroy(gameObject);
        public static GameObject FindGameObject(string name) => GameManager.Instance.FindGameObject(name);
    }

    namespace Helper {
        public static class TypeHelper {
            public static System.Type GetType(object obj) => obj.GetType();
            public static System.Type GetType<T>() => typeof(T);
        }
    }
}
// ReSharper restore CheckNamespace