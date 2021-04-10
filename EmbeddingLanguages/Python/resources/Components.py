# noinspection PyUnresolvedReferences
from typing import Optional

from .Types import GameObject, Component
# noinspection PyUnresolvedReferences
from UnityEngine import Vector3
# noinspection PyUnresolvedReferences
from UnityCommons import Rand


class RandomWalk(Component):
    def __init__(self, move_speed: float, max_distance: float):
        super().__init__()
        self.move_speed: float = move_speed
        self.max_distance: float = max_distance
        self.should_walk: bool = True
        self.next_target: Optional[Vector3] = None

    def on_start(self):
        self.next_target = self.get_next_target()

    def on_update(self, delta: float):
        if not self.should_walk:
            return
        sqr_distance: float = (self.next_target - self.owner.position).sqrMagnitude
        if sqr_distance < 0.5:
            return
        self.owner.position = Vector3.MoveTowards(self.owner.position, self.next_target, self.move_speed * delta)

    def get_next_target(self) -> Vector3:
        return Rand.InsideUnitCircle * self.max_distance


class SphereCollider(Component):
    def __init__(self, radius: float):
        super().__init__()
        self.radius: float = radius


class TargetFollow(Component):
    def __init__(self, target: Optional[GameObject], follow_speed: float):
        super().__init__()
        self.target: GameObject = target
        self.follow_speed: float = follow_speed
        self.should_follow: bool = True

    def on_update(self, delta: float):
        if self.target is None or not self.should_follow:
            return
        sqr_distance: float = (self.target.position - self.owner.position).sqrMagnitude
        if sqr_distance < 0.01:
            self.owner.position = Vector3.Lerp(self.owner.position, self.target.position, self.follow_speed * delta)


class EnemyAI(Component):
    def __init__(self):
        super().__init__()
        self._target_follow: Optional[TargetFollow] = None
        self._random_walk: Optional[RandomWalk] = None

    def on_awake(self):
        self._target_follow: TargetFollow = self.owner.get_component(TargetFollow)
        self._random_walk: RandomWalk = self.owner.get_component(RandomWalk)

    def on_collision_enter(self, other: GameObject):
        if other.tag == 'Enemy':
            return
        if self._target_follow.target is None:
            self._target_follow.target = other
        self._random_walk.should_walk = False

    def on_collision_exit(self, other: GameObject):
        self._target_follow.target = None
        self._random_walk.should_walk = True

    def on_collision_stay(self, other: GameObject):
        if other.tag == 'Enemy':
            return
        if self._target_follow.target is None:
            self._target_follow.target = other
