using UnityEngine;

namespace Embedded.Components {
    public class TargetFollow : Component {
        public GameObject Target;
        public float FollowSpeed;
        public bool ShouldFollow;

        public TargetFollow(GameObject target, float followSpeed) {
            Target = target;
            FollowSpeed = followSpeed;
            ShouldFollow = true;
        }
        
        public override void OnUpdate(float delta) {
            if(Target == null || !ShouldFollow) return;
            var sqrDist = (Target.Position - Owner.Position).sqrMagnitude;
            if(sqrDist > 0.01f)
                Owner.Position = Vector3.Lerp(Owner.Position, Target.Position, FollowSpeed * delta);
        }
    }
}