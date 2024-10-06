import pygame
import Vector
import Agent
import Constants
import random

from pygame import *
from Vector import Vector
from Agent import *

class Enemy(Agent):
	def __init__(self, position, size, speed, color):
		super().__init__(position, size, speed, color)
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
			super().setVelocity(self.center - player.center)
		# Wander
		elif self.timer == 30:
			perpendicularVec = Vector(-self.velocity.y, self.velocity.x)
			perpendicularVec = perpendicularVec.scale(random.uniform(-1, 1) * .5)
			self.setVelocity(self.velocity + perpendicularVec)
		
		if self.timer == 30:
			self.timer = 0

		super().update(deltaTime, bounds)

	def draw(self, screen):
		super().draw(screen)
