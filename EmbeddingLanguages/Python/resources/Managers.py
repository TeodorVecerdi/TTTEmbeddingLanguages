import time
from queue import Queue
from typing import Optional, List, Dict, Set
from .Types import GameObject, Component, TComp
from .Components import SphereCollider


class GameManager(object):
    instance: Optional['GameManager'] = None

    def __init__(self):
        GameManager.instance = self
        self._objects: List[GameObject] = []
        self._destroy_queue: Queue[GameObject] = Queue[GameObject]()
        self._component_initialization_queue: Queue[Component] = Queue[Component]()
        self._game_started: bool = False
        self._last_time: float = 0

    def start_game(self):
        self._game_started = True
        self._last_time = time.monotonic()
        for object in self._objects:
            object.awake()
        for object in self._objects:
            object.start()

    def update_game(self):
        now: float = time.monotonic()
        delta: float = now - self._last_time
        print(f'Running update with a deltaTime of {delta:.4f}s / {(delta * 1000):.2f}ms')
        self._last_time = now

        while not self._component_initialization_queue.empty():
            comp: Component = self._component_initialization_queue.get()
            comp.on_start()

        while not self._destroy_queue.empty():
            obj: GameObject = self._destroy_queue.get()
            obj.destroy()
            try:
                self._objects.remove(obj)
            except ValueError:
                pass

        for obj in self._objects:
            obj.update(delta)

        CollisionManager.update()

    def register(self, game_object: GameObject):
        self._objects.append(game_object)

    def queue_component_initialization(self, component: TComp):
        component.on_awake()
        self._component_initialization_queue.put(component)

    def queue_destroy(self, game_object: GameObject):
        self._destroy_queue.put(game_object)

    def find_game_object(self, name: str) -> Optional[GameObject]:
        for obj in self._objects:
            if obj.name == name:
                return obj
        return None

    @property
    def game_started(self):
        return self._game_started


class CollisionManager:
    _colliders: Dict[GameObject, List[SphereCollider]] = Dict[GameObject, List[SphereCollider]]()
    _colliding: Set[int] = Set[int]()

    @classmethod
    def register(cls, game_object: GameObject, collider: SphereCollider):
        if game_object not in cls._colliders:
            cls._colliders[game_object]: List[SphereCollider] = List[SphereCollider]()
        cls._colliders[game_object].append(collider)

    @classmethod
    def deregister(cls, game_object: GameObject, collider: SphereCollider):
        cls._colliders[game_object].remove(collider)

    @classmethod
    def update(cls):
        keys = [key for key in cls._colliders.keys()]
        for i in range(len(keys) - 1):
            for j in range(i + 1, len(keys)):
                object_a = keys[i]
                object_b = keys[j]
                if cls._check_collision(object_a, object_b):
                    if cls._are_colliding(object_a, object_b):
                        cls._update_collision(object_a, object_b)
                    else:
                        cls._set_collision(object_a, object_b)
                elif cls._are_colliding(object_a, object_b):
                    cls._remove_collision(object_a, object_b)

    @classmethod
    def _remove_collision(cls, object_a: GameObject, object_b: GameObject):
        cls._colliding.remove(cls._hash_code(object_a, object_b))
        for component in object_a.components:
            component.on_collision_exit(object_b)
        for component in object_b.components:
            component.on_collision_exit(object_a)

    @classmethod
    def _set_collision(cls, object_a: GameObject, object_b: GameObject):
        cls._colliding.add(cls._hash_code(object_a, object_b))
        for component in object_a.components:
            component.on_collision_enter(object_b)
        for component in object_b.components:
            component.on_collision_enter(object_a)

    @classmethod
    def _update_collision(cls, object_a: GameObject, object_b: GameObject):
        for component in object_a.components:
            component.on_collision_stay(object_b)
        for component in object_b.components:
            component.on_collision_stay(object_a)

    @classmethod
    def _are_colliding(cls, object_a: GameObject, object_b: GameObject) -> bool:
        return cls._hash_code(object_a, object_b) in cls._colliding

    @classmethod
    def _check_collision(cls, object_a: GameObject, object_b: GameObject) -> bool:
        x = object_a.position.x - object_b.position.x
        y = object_a.position.y - object_b.position.y
        z = object_a.position.z - object_b.position.z
        sqr_distance = x * x + y * y + z * z
        collider_a: SphereCollider
        collider_b: SphereCollider
        for collider_a in cls._colliders[object_a]:
            for collider_b in cls._colliders[object_b]:
                if sqr_distance < collider_a.radius * collider_a.radius + collider_b.radius * collider_b.radius:
                    return True
        return False

    @classmethod
    def _hash_code(cls, object_a: GameObject, object_b: GameObject) -> int:
        hash: int = 17
        hash = hash * 31 + object_a.__hash__()
        hash = hash * 31 + object_b.__hash__()
        return hash
