from ..Data import Vector3
from .Component import Component


class TestSecondComponent(Component):
    position: Vector3

    def start(self):
        self.position = Vector3(3)
        print('Start called from TestSecondComponent')

    def update(self):
        print(f'Update called from TestSecondComponent')
