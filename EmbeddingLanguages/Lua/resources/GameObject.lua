GameObject = {
    _instCount = 0,
    components = {},
    Transform = nil,
    Name = nil,
    AddComponent = function(self, component)
        if component ~= nil then
            table.insert(self.components, component)
            component.Owner = self
            print("Added component")
        else
            print("Component was nil")
        end
    end,
    RemoveComponent = function(self, component)
        for idx, value in ipairs(self.components) do
            if value == component then
                table.remove(self.components, idx)
                component.Owner = nil
                return true
            end
        end
        return false
    end,
    Start = function(self)
        for _, value in pairs(self.components) do
            value:OnStart()
        end
    end,
    Update = function(self)
        for _, value in pairs(self.components) do
            value:OnUpdate()
        end
    end,
    new = function(self, name, obj)
        obj = obj or {}
        setmetatable(obj, self)
        self.__index = self
        name = name or obj.Name or "GameObject_" .. GameObject._instCount
        obj.Name = tostring(name)
        GameObject._instCount = GameObject._instCount + 1;
        if obj.Transform == nil then obj.Transform = Transform() end
        return obj
    end
}

Component = {
    Owner = nil,
    OnStart = function(self)
        print("OnStart called from Component")
    end,
    OnUpdate = function(self)
        print("OnUpdate called from Component")
    end,
    new = function(self, obj)
        obj = obj or {}
        setmetatable(obj, self)
        self.__index = self
        return obj
    end
}