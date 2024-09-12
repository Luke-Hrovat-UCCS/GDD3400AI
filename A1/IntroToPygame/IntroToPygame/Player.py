import Vector
import pygame

class Player:
    #constructor that accepts a position, velocity, and size and initializes data members to represent those quantities
    def __init__(self,size,__pos=None,__velo=None):
        if(__pos is None):
            __pos = Vector.Vector(0,0)
        if(__velo is None):
            __velo = Vector.Vector(0,0)
        self.size = size
        #"private" Python doesn't have anything that specifically locks something up, but you can do this to make it sort of private
        self.__pos = __pos
        self.__velo = __velo
        
    #method that draws the Player on the screen
    def draw(self,screen):
        #set background to corn flower blue
        screen.fill((100,149,237))
        pygame.draw.rect(screen, (0, 0, 0), pygame.Rect(self.__pos.a, self.__pos.b, self.size, self.size))
        #find start and end points of line and draw it
        lstart = (self.__pos.a + (self.size/2),self.__pos.b + (self.size/2))
        lend = (self.__pos.a + (self.size/2) + self.__velo.scale(self.size).a, self.__pos.b+(self.size/2) + self.__velo.scale(self.size).b)
        pygame.draw.line(screen, (255,255,255), lstart, lend,3)


    #method that updates the position of the Player based on its velocity
    def update(self):
        x = 0
        y = 0
        #read inputs
        pressed = pygame.key.get_pressed()
        if pressed[pygame.K_w]: y -= 1
        if pressed[pygame.K_s]: y += 1
        if pressed[pygame.K_a]: x -= 1
        if pressed[pygame.K_d]: x += 1
        
        #apply inputs to velocity and normalize
        self.__velo.a = x
        self.__velo.b = y
        nVelo = self.__velo.normalize()
        
        #update position
        self.__pos.__add__(nVelo)
        
        
        
 









