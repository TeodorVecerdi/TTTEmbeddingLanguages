using System.Collections.Generic;
using System.Linq;
using Embedded;
using Embedded.Components;

namespace Embedded {
    public class CollisionManager {
        private static Dictionary<GameObject, List<SphereCollider>> colliders = new();
        private static List<(GameObject, GameObject)> colliding = new();

        public static void Register(GameObject gameObject, SphereCollider collider) {
            if (!colliders.ContainsKey(gameObject)) colliders[gameObject] = new List<SphereCollider>();
            if (!colliders[gameObject].Contains(collider)) colliders[gameObject].Add(collider);
        }

        public static void Deregister(GameObject gameObject, SphereCollider collider) {
            if (!colliders.ContainsKey(gameObject) || colliders[gameObject] == null) return;
            colliders[gameObject].Remove(collider);
        }

        public static void Update() {
            var keys = colliders.Keys.ToList();
            for (var i = 0; i < keys.Count - 1; i++) {
                for (var j = i + 1; j < keys.Count; j++) {
                    var objectA = keys[i];
                    var objectB = keys[j];
                    
                    if (CheckCollision(objectA, objectB)) {
                        if (AreColliding(objectA, objectB))
                            UpdateCollision(objectA, objectB);
                        else SetCollision(objectA, objectB);
                    } else if (AreColliding(objectA, objectB)) {
                        RemoveCollision(objectA, objectB);
                    }
                }
            }
        }

        private static void RemoveCollision(GameObject objectA, GameObject objectB) {
            colliding.Remove((objectA, objectB));
            colliding.Remove((objectB, objectA));
            objectA.GetComponents().ForEach(component => component.OnCollisionExit(objectB));
            objectB.GetComponents().ForEach(component => component.OnCollisionExit(objectA));
        }

        private static void SetCollision(GameObject objectA, GameObject objectB) {
            colliding.Add((objectA, objectB));
            objectA.GetComponents().ForEach(component => component.OnCollisionEnter(objectB));
            objectB.GetComponents().ForEach(component => component.OnCollisionEnter(objectA));
        }

        private static void UpdateCollision(GameObject objectA, GameObject objectB) {
            objectA.GetComponents().ForEach(component => component.OnCollisionStay(objectB));
            objectB.GetComponents().ForEach(component => component.OnCollisionStay(objectA));
        }

        private static bool AreColliding(GameObject objectA, GameObject objectB) {
            return colliding.Contains((objectA, objectB)) || colliding.Contains((objectB, objectA));
        }

        private static bool CheckCollision(GameObject objectA, GameObject objectB) {
            var sqrDistance = (objectA.Position - objectB.Position).sqrMagnitude;
            
            return (
                from colliderA in objectA.GetComponents<SphereCollider>()
                from colliderB in  objectB.GetComponents<SphereCollider>()
                where sqrDistance < colliderA.Radius * colliderA.Radius + colliderB.Radius * colliderB.Radius
                select colliderA
            ).Any();
        }
    }
}