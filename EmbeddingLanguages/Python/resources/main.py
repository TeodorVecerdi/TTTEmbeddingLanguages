import clr
# noinspection PyUnresolvedReferences
from System.Collections.Generic import List
# noinspection PyUnresolvedReferences
from Embedded import GameManager, GameObject, Component
# noinspection PyUnresolvedReferences
from Embedded.Components import EnemyAI, RandomWalk, SphereCollider, TargetFollow

import random

game_manager: GameManager = None
game_objects: List[GameObject] = None


def initialize_systems():
    global game_manager, game_objects
    game_manager = GameManager()
    game_objects = List[GameObject]()


def initialize_game(enemy_count, random_walker_count, target_follow_count):
    global game_objects
    for i in range(enemy_count):
        enemy = GameObject()
        enemy.Tag = "Enemy"
        game_objects.Add(enemy)
        make_enemy(enemy)
    for i in range(random_walker_count):
        random_walker = GameObject()
        random_walker.Tag = "Walker"
        game_objects.Add(random_walker)
        make_walker(random_walker)
    target_followers = List[GameObject]()
    for i in range(target_follow_count):
        target_follow = GameObject()
        target_followers.Add(target_follow)
        make_follower(target_follow)

    game_objects.AddRange(target_followers)


def start_game():
    global game_manager
    game_manager.StartGame()


def run():
    global game_manager
    game_manager.UpdateGame()


def make_enemy(enemy: GameObject):
    walk_comp = RandomWalk(random.uniform(1, 4), random.uniform(5, 10))
    collider_comp = SphereCollider(random.uniform(5, 10))
    enemy_ai_comp = EnemyAI()
    enemy.AddComponent[RandomWalk](walk_comp)
    enemy.AddComponent[SphereCollider](collider_comp)
    enemy.AddComponent[EnemyAI](enemy_ai_comp)


def make_walker(walker: GameObject):
    walk_comp = RandomWalk(random.uniform(3, 5), random.uniform(5, 10))
    collider_comp = SphereCollider(random.uniform(1, 3))
    walker.AddComponent[RandomWalk](walk_comp)
    walker.AddComponent[SphereCollider](collider_comp)


def make_follower(follower: GameObject):
    global game_objects
    obj = random.choice(game_objects)
    follow_comp = TargetFollow(obj, random.uniform(1, 2))
    follower.AddComponent[TargetFollow](follow_comp)
