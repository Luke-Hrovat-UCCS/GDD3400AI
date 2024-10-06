import pygame
import Vector
import random

from Vector import Vector

class Agent:

	def __init__(self, position, size, speed, color):
		self.position = position
		self.size = Vector(size, size)
		self.speed = speed
		self.velocity = Vector(random.uniform(-1, 1), random.uniform(-1, 1))
		self.target = Vector(0, 0)
		self.color = color
		self.updateCenter()
		self.updateRect()

	def __str__(self):
		return 'Agent (%d, %d, %d, %d)' % (self.size, self.position, self.velocity, self.center)

	def setVelocity(self, velocity):
		self.velocity = velocity.normalize()

	def updateCenter(self):
		self.center = self.position + self.size.scale(0.5)

	def updateRect(self):
		self.rect = pygame.Rect(self.position.x, self.position.y, self.size.x, self.size.y)

	def isInCollision(self, agent):
		if pygame.Rect.colliderect(self.rect, agent.rect):
			return True
		else:
			return False

	def clampToBounds(self, bounds):
		if self.position.x < 0 or self.position.x > bounds.x - self.size.x:
			self.velocity.x *= -1
		elif self.position.y < 0 or self.position.y > bounds.y - self.size.y:
			self.velocity.y *= -1
		self.position.x = max(0, min(self.position.x, bounds.x - self.size.x))
		self.position.y = max(0, min(self.position.y, bounds.y - self.size.y))
		self.setVelocity(self.velocity)


	def update(self, deltaTime, bounds):
		self.position = self.position + self.velocity.scale(self.speed * deltaTime)
		self.clampToBounds(bounds)
		self.updateCenter()
		self.updateRect()

	def draw(self, screen):
		pygame.draw.rect(screen, self.color, self.rect)
		pygame.draw.line(screen, (0, 0, 255), (self.center.x, self.center.y), 
				   (self.center.x + (self.velocity.x * self.size.x * 2), 
					self.center.y + (self.velocity.y * self.size.y * 2)), 3)

