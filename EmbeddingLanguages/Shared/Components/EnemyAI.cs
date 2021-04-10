using System;

namespace Embedded.Components {
    public class EnemyAI : Component {
        private TargetFollow targetFollow;
        private RandomWalk randomWalk;

        public override void OnAwake() {
            targetFollow = Owner.GetComponent<TargetFollow>();
            randomWalk = Owner.GetComponent<RandomWalk>();
        }

        public override void OnCollisionEnter(GameObject other) {
            // Console.WriteLine($"Enemy{Owner.Name}::OnCollisionEnter");
            if(other.Tag == "Enemy") return;
            targetFollow.Target ??= other;
            randomWalk.ShouldWalk = false;
        }

        public override void OnCollisionExit(GameObject other) {
            // Console.WriteLine($"Enemy{Owner.Name}::OnCollisionExit");
            targetFollow.Target = null;
            randomWalk.ShouldWalk = true;
        }

        public override void OnCollisionStay(GameObject other) {
            // Console.WriteLine($"Enemy{Owner.Name}::OnCollisionStay");
            if(other.Tag == "Enemy") return;
            targetFollow.Target ??= other;
            randomWalk.ShouldWalk = false;
        }
    }
}