import pygame
import Vector
import Agent
import random
import Constants

from Vector import Vector
from Agent import *

isPlayer = False

class Dog(Agent):

	def __init__(self, position, size, speed, color):
		super().__init__(position, size, speed, color)
		self.targetEnemy = None

	def update(self, deltaTime, bounds, enemies):
		self.enemies = enemies

		# Let the user control the player
		if isPlayer:
			keys = Vector(0, 0)

			# adjust the velocity based on input from the user
			# Handle arrow keys and WASD
			pressed = pygame.key.get_pressed()
			if pressed[pygame.K_UP] or pressed[pygame.K_w]: 
				keys.y -= 1
			if pressed[pygame.K_DOWN] or pressed[pygame.K_s]: 
				keys.y += 1
			if pressed[pygame.K_LEFT] or pressed[pygame.K_a]: 
				keys.x -= 1
			if pressed[pygame.K_RIGHT] or pressed[pygame.K_d]: 
				keys.x += 1
			super().setVelocity(keys)
		else:
			# If we don't have a target enemy, select and enemy at random to chase
			if self.targetEnemy == None or self.isInCollision(self.targetEnemy):
				self.targetEnemy = self.enemies[random.randrange(0, len(self.enemies))]
			
			# Set our velocity to chase the enemy we've targetted
			super().setDirection(self.targetEnemy.position - self.position)
			super().calcAppliedForce(Constants.PLAYER_WEIGHT,deltaTime,Constants.PLAYER_SPEED)

		# Set the velocity and update position
		super().update(deltaTime, bounds)

	def draw(self, screen):
		super().draw(screen)
		pygame.draw.line(screen, (255, 0, 0), (self.center.x, self.center.y), 
			(self.targetEnemy.center.x, self.targetEnemy.center.y), 3)
