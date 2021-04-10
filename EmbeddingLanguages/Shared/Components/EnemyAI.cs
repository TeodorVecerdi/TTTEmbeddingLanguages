namespace Embedded.Components {
    public class EnemyAI : Component {
        private TargetFollow targetFollow;

        public override void OnAwake() {
            targetFollow = Owner.GetComponent<TargetFollow>();
        }

        public override void OnCollisionEnter(GameObject other) {
            if(other.Tag == "Enemies") return;
            targetFollow.Target ??= other;
        }

        public override void OnCollisionExit(GameObject other) {
            targetFollow.Target = null;
        }

        public override void OnCollisionStay(GameObject other) {
            if(other.Tag == "Enemies") return;
            targetFollow.Target ??= other;
        }
    }
}