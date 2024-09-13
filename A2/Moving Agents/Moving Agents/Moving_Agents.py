
import pygame
import Constants
import Player
import Enemy

#initialize game and player
pygame.init()
screen = pygame.display.set_mode((Constants.WORLD_WIDTH, Constants.WORLD_HEIGHT))
done = False
player = Player.Player()
enemies = []
x =  range(Constants.ENEMY_NUMBERS)
for n in x:
    n = Enemy.Enemy()
    enemies.append(n)

clock = pygame.time.Clock()
while not done:
        #end condition    
        for event in pygame.event.get():
                if event.type == pygame.QUIT:
                        done = True
        
        screen.fill(Constants.BACKGROUND_COLOR)
        
        #update and draw
        for enemy in enemies:
            enemy.update(player)
        player.update(enemies)
        
        player.draw(screen)
        for enemy in enemies:
            enemy.draw(screen)
        #at the end flip buffers
        pygame.display.flip()
        
        #lock framerate at 60
        clock.tick(Constants.FRAME_RATE)
