
import pygame
from Constants import AGENTHEIGHT, AGENTWIDTH, BOUNDARY_RADIUS, BOUNDARY_WEIGHT
import Vector
import random

from Vector import Vector

class Agent:


	def __init__(self, position, size, speed, color, image):
		self.position = position
		self.size = Vector(size, size)
		self.speed = speed
		self.velocity = Vector(random.uniform(-1, 1), random.uniform(-1, 1))
		self.target = Vector(0, 0)
		self.color = color
		self.updateCenter()
		self.updateRect()
		self.direction = self.velocity
		self.image = image
		self.angle = 0;
		self.upperLeft = Vector(0,0)
		self.boundaryForce = Vector(0,0)
		self.boundaries= [Vector(0,0),Vector(0,0),Vector(0,0),Vector(0,0)]

	def __str__(self):
		return 'Agent (%d, %d, %d, %d)' % (self.size, self.position, self.velocity, self.center)

	def setDirection(self, direction):
		self.direction = direction.normalize()
		
	def calcAppliedForce(self, weight, deltaTime, speed):
		self.boundaryForce.scale(BOUNDARY_WEIGHT)
		appliedForce = self.direction.scale(weight) + self.boundaryForce
		appliedForce = appliedForce.normalize()
		appliedForce = appliedForce.scale(deltaTime*speed)
		self.setVelocity(appliedForce)
		
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
		
	def checkBoundaries(self, bounds):
		
		if self.position.x < 0 + BOUNDARY_RADIUS:
			self.boundaries[0] = Vector(0,self.position.x)
		else:
			self.boundaries[0] = Vector(0,0)
		if self.position.x > bounds.x - BOUNDARY_RADIUS - self.size.x:
			self.boundaries[1] = Vector(0,(bounds.x-self.position.x-self.size.x))
		else:
			self.boundaries[1] = Vector(0,0)
		if self.position.y < 0 + BOUNDARY_RADIUS:
			self.boundaries[2] = Vector(self.position.y,0)
		else:
			self.boundaries[2] = Vector(0,0)
		if self.position.y > bounds.y - BOUNDARY_RADIUS - self.size.y:
			self.boundaries[3] = Vector((bounds.y-self.position.y-self.size.y),0)
		else:
			self.boundaries[3] = Vector(0,0)


	def update(self, deltaTime, bounds):
		self.position = self.position + self.velocity.scale(self.speed * deltaTime)
		self.clampToBounds(bounds)
		self.updateCenter()
		self.updateRect()
		

	def draw(self, screen):
		self.surf = pygame.transform.rotate(self.image, self.angle)
		self.upperLeft = Vector(self.position.x - self.surf.get_width() + AGENTWIDTH/2 ,self.position.y - self.surf.get_height()+AGENTHEIGHT/2)
		screen.blit(self.surf, [self.upperLeft.x, self.upperLeft.y])
		#pygame.draw.rect(screen, self.color, self.rect)
		pygame.draw.line(screen, (0, 0, 255), (self.center.x, self.center.y), 
				   (self.center.x + (self.velocity.x * self.size.x * 2), 
					self.center.y + (self.velocity.y * self.size.y * 2)), 3)
