import pygame
import random
import Vector
import Sheep
import Agent
import Dog
import Constants

from pygame import *
from random import *
from Vector import Vector
from Sheep import *
from Agent import *
from Dog import *

pygame.init()

screen = pygame.display.set_mode((Constants.WORLD_WIDTH, Constants.WORLD_HEIGHT))
clock = pygame.time.Clock()
bounds = Vector(Constants.WORLD_WIDTH, Constants.WORLD_HEIGHT)

sheepImage = pygame.image.load('sheep.png')
dogImage = pygame.image.load('dog.png')

player = Dog(Vector(Constants.WORLD_WIDTH / 2, Constants.WORLD_HEIGHT / 2), 
				Constants.PLAYER_SIZE, Constants.PLAYER_SPEED, (255, 255, 255), dogImage)

enemies = []


for x in range(10):
	enemies += [Sheep(Vector(randrange(1,int(bounds.x + 1)), randrange(1,int(bounds.y + 1))), 
					  Constants.ENEMY_SIZE, Constants.ENEMY_SPEED, (0, 255, 0), sheepImage)]

# While the user has not selected quit
hasQuit = False

while not hasQuit:
	# Clear the screen
	screen.fill(Constants.BACKGROUND_COLOR)

	# Process all in-game events
	for event in pygame.event.get():
		if event.type == pygame.QUIT \
			or (event.type == pygame.KEYDOWN and event.key == pygame.K_ESCAPE):
			hasQuit = True

	deltaTime = clock.get_time() / 1000

	# Update the agents onscreen
	player.update(deltaTime, bounds, enemies)
	for enemy in enemies:
		enemy.update(deltaTime, bounds, player)

	# Draw the agents onscreen
	player.draw(screen)
	for enemy in enemies:
		enemy.draw(screen)

	# Double buffer
	pygame.display.flip()

	# Limit to 60 FPS
	clock.tick(Constants.FRAME_RATE)

# Quit
pygame.quit()