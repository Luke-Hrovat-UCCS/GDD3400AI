
import pygame
import Constants
import Player

#initialize game and player
pygame.init()
screen = pygame.display.set_mode((Constants.WORLD_WIDTH, Constants.WORLD_HEIGHT))
done = False
player = Player.Player()
enemies = []

clock = pygame.time.Clock()
while not done:
        #end condition    
        for event in pygame.event.get():
                if event.type == pygame.QUIT:
                        done = True
        
        screen.fill(Constants.BACKGROUND_COLOR)
        
        #update and draw
        player.update(enemies)
        player.draw(screen)

        #at the end flip buffers
        pygame.display.flip()
        
        #lock framerate at 60
        clock.tick(Constants.FRAME_RATE)
