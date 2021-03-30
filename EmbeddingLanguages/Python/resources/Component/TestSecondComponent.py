from Embedded.GameObject import Component


class TestSecondComponent(Component):
    def OnStart(self):
        super(self).OnStart()
        print('Start called from TestSecondComponent')

    def OnUpdate(self):
        super(self).OnUpdate()
        print(f'Update called from TestSecondComponent')
