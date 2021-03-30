--- import('Assembly', 'Namespace')
import('Shared', 'Embedded.Data')

TestCompOne = Component:new {
    OnStart = function(self)
        print("OnStart called from TestCompOne")
        self.Owner.Transform.Position = Vector3(145, 23, 44)
    end
}
TestCompTwo = Component:new {
    OnUpdate = function(self)
        print("OnUpdate called from TestCompTwo")
    end
}

obj = GameObject:new()
obj:AddComponent(TestCompOne:new())
obj:AddComponent(TestCompTwo:new())

print(obj.Transform)
obj:Start()
obj:Update()
print(obj.Transform)

function CreateVector(x, y, z)
    return Vector3(x, y, z)
end