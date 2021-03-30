from Embedded.Data import Vector3
from Embedded.GameObject import Component

class TestComponent(Component):
    def OnStart(self):
        super(TestComponent, self).OnStart()
        print('Start called from TestComponent')
        self.Owner.Position = Vector3(3)
