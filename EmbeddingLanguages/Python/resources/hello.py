from .Program import *

first: Component
second: Component


def run():
    print("[ROOT] calling start methods")
    first.start()
    second.start()
    print("[ROOT] calling update methods")
    first.update()
    second.update()


if __name__ == '__main__':
    first = TestComponent()
    second = TestSecondComponent()
    run()
