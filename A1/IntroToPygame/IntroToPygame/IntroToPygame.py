
import pygame
import Player

#from Py Game Tutorial
#initialize game and player
pygame.init()
screen = pygame.display.set_mode((800, 600))
done = False
player = Player.Player(50)


clock = pygame.time.Clock()
while not done:
        #end condition    
        for event in pygame.event.get():
                if event.type == pygame.QUIT:
                        done = True
        
        #update and draw
        player.update()
        player.draw(screen)
        
        #at the end flip buffers
        pygame.display.flip()
        
        #lock framerate at 60
        clock.tick(60)



