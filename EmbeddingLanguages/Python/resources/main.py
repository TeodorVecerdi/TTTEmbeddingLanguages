import clr

clr.AddReference("Shared")

import random
from typing import Optional, List
from .Components import EnemyAI, RandomWalk, SphereCollider, TargetFollow
from .Types import GameObject, Component, Transform
from .Managers import GameManager

game_manager: GameManager
game_objects: List[GameObject]


def initialize_systems():
    global game_manager, game_objects
    game_manager = GameManager()
    game_objects = List[GameObject]()


def initialize_game(enemy_count, random_walker_count, target_follow_count):
    global game_objects
    for i in range(enemy_count):
        enemy = GameObject()
        enemy.tag = "Enemy"
        game_objects.append(enemy)
        make_enemy(enemy)
    for i in range(random_walker_count):
        random_walker = GameObject()
        random_walker.tag = "Walker"
        game_objects.append(random_walker)
        make_walker(random_walker)
    for i in range(target_follow_count):
        target_follow = GameObject()
        make_follower(target_follow)
    game_objects.clear()


def start_game():
    global game_manager
    game_manager.start_game()


def run():
    global game_manager
    game_manager.update_game()


def make_enemy(enemy: GameObject):
    walk_comp = RandomWalk(random.uniform(1, 4), 100)
    follow_comp = TargetFollow(None, random.uniform(1, 2))
    collider_comp = SphereCollider(random.uniform(5, 10))
    enemy_ai_comp = EnemyAI()
    enemy.add_component(walk_comp)
    enemy.add_component(follow_comp)
    enemy.add_component(collider_comp)
    enemy.add_component(enemy_ai_comp)


def make_walker(walker: GameObject):
    walk_comp = RandomWalk(random.uniform(3, 5), 100)
    collider_comp = SphereCollider(random.uniform(1, 3))
    walker.add_component(walk_comp)
    walker.add_component(collider_comp)


def make_follower(follower: GameObject):
    global game_objects
    obj = random.choice(game_objects)
    follow_comp = TargetFollow(obj, random.uniform(1, 2))
    follower.add_component(follow_comp)
