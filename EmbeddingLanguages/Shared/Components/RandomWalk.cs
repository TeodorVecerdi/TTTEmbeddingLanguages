using UnityCommons;
using UnityEngine;

namespace Embedded.Components {
    public class RandomWalk : Component {
        public float MoveSpeed;
        public float TargetDistance;
        public bool ShouldWalk;
        private Vector3 nextTarget;

        public RandomWalk(float moveSpeed, float targetDistance) {
            MoveSpeed = moveSpeed;
            TargetDistance = targetDistance;
            ShouldWalk = true;
        }

        public override void OnStart() {
            nextTarget = GetNextTarget();
        }

        public override void OnUpdate(float delta) {
            if(!ShouldWalk) return;
            var sqrDist = (nextTarget - Owner.Position).sqrMagnitude;
            if (sqrDist < 0.5f) {
                nextTarget = GetNextTarget();
            }

            Owner.Position = Vector3.MoveTowards(Owner.Position, nextTarget, MoveSpeed * delta);
        }

        private Vector3 GetNextTarget() {
            var randUnitCircle = Rand.InsideUnitCircle * TargetDistance;
            return Owner.Position + randUnitCircle;
        }
    }
}