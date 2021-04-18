using System.Collections.Generic;
using System.Linq;
using Embedded.Components;

namespace Embedded {
    public static class CollisionManager {
        private static readonly Dictionary<GameObject, List<SphereCollider>> colliders = new();
        private static readonly HashSet<int> colliding = new();

        public static void Register(GameObject gameObject, SphereCollider collider) {
            // Console.WriteLine("Registered collider");
            if (!colliders.ContainsKey(gameObject)) colliders[gameObject] = new List<SphereCollider>();
            if (!colliders[gameObject].Contains(collider)) colliders[gameObject].Add(collider);
        }

        public static void Deregister(GameObject gameObject, SphereCollider collider) {
            // Console.WriteLine("Deregistered collider");
            if (!colliders.ContainsKey(gameObject) || colliders[gameObject] == null) return;
            colliders[gameObject].Remove(collider);
        }

        public static void Update() {
            // Console.WriteLine("Began collision update");
            var keys = colliders.Keys.ToList();
            for (var i = 0; i < keys.Count - 1; i++) {
                for (var j = i + 1; j < keys.Count; j++) {
                    var objectA = keys[i];
                    var objectB = keys[j];
                    // Console.WriteLine("Checking collision");
                    if (CheckCollision(objectA, objectB)) {
                        // // Console.WriteLine("Are colliding");
                        if (AreColliding(objectA, objectB))
                            UpdateCollision(objectA, objectB);
                        else SetCollision(objectA, objectB);
                    } else if (AreColliding(objectA, objectB)) {
                        // Console.WriteLine("Were colliding");
                            RemoveCollision(objectA, objectB);
                    }
                }
            }
            // Console.WriteLine("Finished collision update");
        }

        private static void RemoveCollision(GameObject objectA, GameObject objectB) {
            colliding.Remove(HashCode(objectA, objectB));
            colliding.Remove(HashCode(objectB, objectA));
            foreach (var component in objectA.Components) {
                component.OnCollisionExit(objectB);
            }
            foreach (var component in objectB.Components) {
                component.OnCollisionExit(objectA);
            }
        }

        private static void SetCollision(GameObject objectA, GameObject objectB) {
            colliding.Add(HashCode(objectA, objectB));
            foreach (var component in objectA.Components) {
                component.OnCollisionEnter(objectB);
            }
            foreach (var component in objectB.Components) {
                component.OnCollisionEnter(objectA);
            }
        }

        private static void UpdateCollision(GameObject objectA, GameObject objectB) {
            foreach (var component in objectA.Components) {
                component.OnCollisionStay(objectB);
            }
            foreach (var component in objectB.Components) {
                component.OnCollisionStay(objectA);
            }
        }

        private static bool AreColliding(GameObject objectA, GameObject objectB) {
            return colliding.Contains(HashCode(objectA, objectB));
        }

        private static bool CheckCollision(GameObject objectA, GameObject objectB) {
            var x = objectA.Position.x - objectB.Position.x; 
            var y = objectA.Position.y - objectB.Position.y; 
            var z = objectA.Position.z - objectB.Position.z;
            var sqrDistance = x * x + y * y + z * z;

            foreach (var colliderA in colliders[objectA])
            foreach (var colliderB in colliders[objectB]) {
                if (sqrDistance < colliderA.Radius * colliderA.Radius + colliderB.Radius * colliderB.Radius) return true;
            }

            return false;
        }

        private static int HashCode(GameObject objA, GameObject objB) {
            unchecked {
                var hash = 17;
                hash = hash * 31 + objA.GetHashCode();
                hash = hash * 31 + objB.GetHashCode();
                return hash;
            }
        }
    }
}