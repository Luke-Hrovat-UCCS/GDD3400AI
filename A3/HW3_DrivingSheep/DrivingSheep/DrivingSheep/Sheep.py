import pygame
import Vector
import Agent
import Constants
import random

from pygame import *
from Vector import Vector
from Agent import *

class Sheep(Agent):
	def __init__(self, position, size, speed, color, image):
		super().__init__(position, size, speed, color, image)
		self.player = 0
		self.timer = 0

	def isPlayerClose(self):
		return (self.player.center - self.center).length() < Constants.MIN_ATTACK_DIST

	def update(self, deltaTime, bounds, player):
		print(self.timer)
		self.timer += 1
		self.player = player
		
		# Flee from the player
		if self.isPlayerClose():
			super().setDirection(self.center - player.center)
			super().calcAppliedForce(Constants.FLEE_WEIGHT,deltaTime,Constants.ENEMY_SPEED)
			
		# Wander
		elif self.timer == 30:
			perpendicularVec = Vector(-self.velocity.y, self.velocity.x)
			perpendicularVec = perpendicularVec.scale(random.uniform(-1, 1) * .5)
			self.setDirection(self.velocity + perpendicularVec)
			super().calcAppliedForce(Constants.WANDER_WEIGHT,deltaTime,Constants.ENEMY_SPEED)
			
		if self.timer == 30:
			self.timer = 0

		super().update(deltaTime, bounds)

	def draw(self, screen):
		super().draw(screen)
