# noinspection PyUnresolvedReferences
from UnityEngine import Vector3
from typing import Optional, List, TypeVar, cast, Type
from .Components import SphereCollider
from .Managers import GameManager, CollisionManager


class Transform(object):
    def __init__(self, position: Optional[Vector3] = None, scale: Optional[Vector3] = None, euler_angles: Optional[Vector3] = None):
        self.position: Vector3 = Vector3(0, 0, 0) if position is None else position
        self.scale: Vector3 = Vector3(1, 1, 1) if scale is None else scale
        self.euler_angles: Vector3 = Vector3(0, 0, 0) if euler_angles is None else euler_angles

    def __str__(self):
        return f'Transform [\n  Position: {self.position},\n  Scale: {self.scale},\n  EulerAngles: {self.euler_angles}\n]'


class Component(object):
    def __init__(self):
        self.owner: Optional[GameObject] = None

    def on_awake(self): pass

    def on_start(self): pass

    def on_update(self, delta: float): pass

    def on_destroy(self): pass

    def on_collision_enter(self, other: 'GameObject'): pass

    def on_collision_stay(self, other: 'GameObject'): pass

    def on_collision_exit(self, other: 'GameObject'): pass

    @classmethod
    def destroy(cls, game_object: 'GameObject'):
        GameObject.destroy_object(game_object)

    @classmethod
    def find_game_object(cls, name: str) -> 'GameObject':
        return GameManager.instance.find_game_object(name)


TComp = TypeVar('TComp', bound=Component)


class GameObject(object):
    instance_count: int = 0

    @classmethod
    def destroy_object(cls, game_object: 'GameObject'):
        if game_object._is_destroying:
            return
        game_object._is_destroying = True
        GameManager.instance.queue_destroy(game_object)

    @classmethod
    def find_game_object(cls, name: str) -> 'GameObject':
        return GameManager.instance.find_game_object(name)

    def __init__(self, name: Optional[str] = None, tag: Optional[str] = None):
        self.name: str = f'GameObject_{GameObject.instance_count}' if name is None else name
        self.tag: str = 'Object' if tag is None else tag
        GameObject.instance_count += 1

        self._components: List[Component] = []
        self._transform: Transform = Transform()
        self._is_destroying: bool = False

    def awake(self):
        for component in self._components:
            component.on_awake()

    def start(self):
        for component in self._components:
            component.on_start()

    def update(self, delta: float):
        for component in self._components:
            component.on_update(delta)

    def destroy(self):
        for component in self._components:
            component.on_destroy()

    def add_component(self, component: TComp):
        component.owner = self
        self._components.append(component)

        if isinstance(component, SphereCollider):
            CollisionManager.register(self, cast(SphereCollider, component))

        if GameManager.instance.game_started:
            GameManager.instance.queue_component_initialization(component)

    def remove_component(self, component: TComp):
        try:
            self._components.remove(component)
        except ValueError:
            return
        component.owner = None
        if isinstance(component, SphereCollider):
            CollisionManager.deregister(self, cast(SphereCollider, component))

    def has_component(self, component_type: Type[TComp]) -> bool:
        for component in self._components:
            if isinstance(component, component_type):
                return True
        return False

    def get_component(self, component_type: Type[TComp]) -> Optional[TComp]:
        for component in self._components:
            if isinstance(component, component_type):
                return cast(component_type, component)
        return None

    def get_components(self, component_type: Type[TComp]) -> List[TComp]:
        comps: List[TComp] = []
        for component in self._components:
            if isinstance(component, component_type):
                comps.append(component)
        return comps

    def __str__(self):
        return f'{self.name}:\n{self._transform}'

    @property
    def components(self):
        return self._components

    @property
    def position(self) -> Vector3:
        return self._transform.position

    @position.setter
    def position(self, value: Vector3):
        self._transform.position = value

    @property
    def scale(self) -> Vector3:
        return self._transform.scale

    @scale.setter
    def scale(self, value: Vector3):
        self._transform.scale = value

    @property
    def euler_angles(self) -> Vector3:
        return self._transform.euler_angles

    @euler_angles.setter
    def euler_angles(self, value: Vector3):
        self._transform.euler_angles = value
