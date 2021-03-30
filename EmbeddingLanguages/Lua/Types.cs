using System;

namespace Embedded {
    public class HelloWorld {
        public string Name;
        public HelloWorld(string name) {
            Name = name;
        }
        public void Say() {
            Console.WriteLine($"HELLO! I am {Name}");
        }
    }
}