from ..Data import Vector3
from .Component import Component


class TestComponent(Component):
    position: Vector3

    def start(self):
        self.position = Vector3(3)
        print('Start called from TestComponent')
