import pygame
import Vector
import Agent
import Constants
import random

from pygame import *
from Vector import Vector
from Agent import *

class Enemy(Agent):
	def __init__(self, image, position, size, speed, color):
		super().__init__(image, position, size, speed, color)
		self.player = 0
		self.timer = 0
		self.angularSpeed = Constants.SHEEP_TURNING_SPEED

	def computeDogInfluence(self, player):
		vectToDog = self.position - player.position
		self.target = player
		if vectToDog.length() < Constants.MIN_ATTACK_DIST:
			self.drawDogInfluence = True
			return vectToDog
		else:
			self.drawDogInfluence = False
		return Vector(0, 0)

	def update(self, deltaTime, bounds, player):
		#print(self.timer)
		self.timer += 1
		self.player = player

		# Flee from the player
		if (self.position - player.position).length() < Constants.MIN_ATTACK_DIST:
			dogInfluence = self.computeDogInfluence(player).normalize()
			#print("dogInfluence", dogInfluence)
			self.targetVelocity = dogInfluence.scale(Constants.SHEEP_DOG_INFLUENCE_WEIGHT)	
			dogInfluence.scale(Constants.SHEEP_DOG_INFLUENCE_WEIGHT * int(Constants.ENABLE_DOG))

		boundsInfluence = self.computeBoundaryInfluence(bounds).normalize()
		#print("boundsInfluence", boundsInfluence)
		self.targetVelocity = self.targetVelocity + boundsInfluence.scale(Constants.SHEEP_BOUNDARY_INFLUENCE_WEIGHT)
		self.targetVelocity = self.targetVelocity.normalize()

		super().update(deltaTime, bounds)

	def draw(self, screen):
		super().draw(screen)
