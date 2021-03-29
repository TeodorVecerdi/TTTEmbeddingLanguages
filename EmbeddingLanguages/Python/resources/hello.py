from .Component import *

from Embedded.GameObject import GameObject, Component
from Embedded.Data import Vector3, Transform
from Embedded.Helper import TypeHelper

first: Component
second: Component

def run():
    gameObj = GameObject()
    gameObj.AddComponent(first)
    gameObj.AddComponent(second)
    print("[ROOT] calling start methods")
    first.OnStart()
    second.OnStart()
    print("[ROOT] calling update methods")
    first.OnUpdate()
    second.OnUpdate()


def CreateGameObject(transform: Transform):
    gameObj = GameObject()
    gameObj.Position = transform.Position
    gameObj.Scale = transform.Scale
    gameObj.EulerAngles = transform.EulerAngles
    return gameObj


if __name__ == '__main__':
    first = TestComponent()
    gameObj = GameObject()
    gameObj.AddComponent(first)
    print(gameObj.HasComponent(TestComponent))
    print(gameObj.GetComponent(TestComponent))
    gameObj.OnStart()
    gameObj.OnUpdate()
