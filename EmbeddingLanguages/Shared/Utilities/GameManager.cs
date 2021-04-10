using System;
using System.Collections.Generic;

namespace Embedded {
    public class GameManager {
        private static GameManager instance;
        public static GameManager Instance => instance;
        private readonly List<GameObject> objects = new();
        private readonly Queue<GameObject> destroyQueue = new();
        private readonly Queue<Component> componentInitializationQueue = new();
        private bool gameStarted;
        private DateTime lastTime;

        public GameManager() {
            instance = this;
        }
        
        public bool GameStarted => gameStarted;

        public void StartGame() {
            gameStarted = true;
            lastTime = DateTime.Now;
            objects.ForEach(o => o.Awake());
            objects.ForEach(o => o.Start());
            Console.WriteLine($"Started game. Ran Awake and Start on {objects.Count} objects");
        }

        public void UpdateGame() {
            var now = DateTime.Now;
            var timeSpan = now - lastTime;
            Console.WriteLine($"Running Update with a deltaTime of {timeSpan.TotalSeconds:F4} s {timeSpan.TotalMilliseconds:F2} ms");
            lastTime = now;
            
            while (componentInitializationQueue.Count > 0) {
                var comp = componentInitializationQueue.Dequeue();
                comp.OnStart();
                Console.WriteLine("Initialized component");
            }
            while (destroyQueue.Count > 0) {
                var obj = destroyQueue.Dequeue();
                obj.Destroy();
                objects.Remove(obj);
                Console.WriteLine("Destroyed object");
            }
            foreach (var gameObject in objects) {
                gameObject.Update((float) timeSpan.TotalSeconds);
            }
            CollisionManager.Update();
        }

        public void Register(GameObject gameObject) {
            objects.Add(gameObject);
        }

        public void QueueComponentInitialization<T>(T component) where T : Component {
            component.OnAwake();
            componentInitializationQueue.Enqueue(component);
        }

        public void QueueDestroy(GameObject gameObject) {
            destroyQueue.Enqueue(gameObject);
        }

        public GameObject FindGameObject(string name) {
            return objects.Find(o => string.Equals(o.Name, name, StringComparison.InvariantCulture));
        }
    }
}