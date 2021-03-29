class Vector3:
    def __init__(self, x, y=None, z=None):
        self.x = x
        self.y = x if y is None else y
        self.z = x if z is None else z

    def __add__(self, other):
        assert isinstance(other, Vector3)
        return Vector3(self.x + other.x, self.y + other.y, self.z + other.z)

    def __sub__(self, other):
        assert isinstance(other, Vector3)
        return self + -other

    def __iadd__(self, other):
        assert isinstance(other, Vector3)
        self.x += other.x
        self.y += other.y
        self.z += other.z
        return self

    def __mul__(self, other):
        assert isinstance(other, (int, float, Vector3)), 'Cannot multiply Vector3 with a type other than [int, float, Vector3]'
        if isinstance(other, (int, float)):
            return Vector3(self.x * other, self.y * other, self.z * other)
        elif isinstance(other, Vector3):
            return Vector3(self.x * other.x, self.y * other.y, self.z * other.z)

    def __rmul__(self, other):
        return self * other

    def __imul__(self, other):
        assert isinstance(other, (int, float, Vector3)), 'Cannot multiply Vector3 with a type other than [int, float, Vector3]'
        mul: Vector3 = Vector3(other, other, other) if isinstance(other, (int, float)) else other
        self.x *= mul.x
        self.y *= mul.y
        self.z *= mul.z
        return self

    def __neg__(self):
        return Vector3(-self.x, -self.y, -self.z)

    def __eq__(self, other) -> bool:
        return isinstance(other, Vector3) and self.x == other.x and self.y == other.y and self.z == other.z

    def __ne__(self, other) -> bool:
        return not self.__eq__(other)

    def __str__(self) -> str:
        return f"[{self.x}, {self.y}, {self.z}]"

    def __repr__(self) -> str:
        return f"Vector3{self}"
