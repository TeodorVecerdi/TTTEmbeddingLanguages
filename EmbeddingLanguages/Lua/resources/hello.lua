--- import('Assembly', 'Namespace')
import('Shared', 'Embedded.Data')
import('Shared', 'Embedded.GameObject')

function CreateVector(x, y, z)
    return Vector3(x, y, z)
end

AComponent = Component()
for key in AComponent do
    print(key)
end

--[[
function AComponent:new(obj) 
    obj = obj or {}
    setmetatable(obj, self)
    self.__index = self
    return obj
end

AComponent:OnStart()
]]
