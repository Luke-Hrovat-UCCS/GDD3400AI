import Constants
import Vector
import pygame
import random
from Agent import Agent

class Enemy(Agent):
    #constructor that accepts a position, velocity, color, and size and initializes data members to represent those quantities
    def __init__(self,size=None,__pos=None,__velo=None,color=None):
        #default constructor
        if(size is None):
            size = Constants.ENEMY_SIZE
        if(__pos is None):
            __pos = Vector.Vector(random.randrange(0,Constants.WORLD_WIDTH-size),random.randrange(0,Constants.WORLD_HEIGHT-size))
        if(__velo is None):
            __velo = Vector.Vector(random.randrange(0,Constants.WORLD_WIDTH-size),random.randrange(0,Constants.WORLD_HEIGHT-size))
            __velo = __velo.normalize()
        if(color is None):
            color = Constants.ENEMY_COLOR
        #initialize data members
        self.__pos = __pos
        self.__velo = __velo
        self.size = size
        self.color = color
        self.target = None
        self.rect = pygame.Rect(self.__pos.a, self.__pos.b, self.size, self.size)
        super().__init__(size,__pos,__velo,color)
    
    #draws Agent and a line representing velocity
    def draw(self,screen):
        #draw self
        pygame.draw.rect(screen, self.color, self.rect)
        #draw line
        lstart = (self.center[0], self.center[1])
        lend = (self.center[0] + self.__velo.scale(self.size).a, self.center[1]+self.__velo.scale(self.size).b)
        pygame.draw.line(screen, (0,0,255), lstart, lend,3)
        if self.target is not None:
            pygame.draw.line(screen, (255,0,0), self.center, self.target.center,3)
        
    
    #updates position of enemy, Runs from player if player is in range, otherwise wanders, accepts a player object
    def update(self, player):
        self.rect = pygame.Rect(self.__pos.a, self.__pos.b, self.size, self.size)
        #protection for if the player doesn't exist    
        if player is None:
            return
        #if player is in range RUN AWAY
        #calculate vector from player to enemy
        playerDist = Vector.Vector(self.center[0],self.center[1])
        playerDist.__sub__(Vector.Vector(player.center[0],player.center[1]))

        d= ((player.center[0]-self.center[0])**2+(player.center[1]-self.center[1])**2)**.5
        #if length of that vector is less than the aggro range of the enemy, run away
        if(d < Constants.AGGRO_RANGE):
            #set velocity to the player vector and normalize    
            self.__velo = playerDist.normalize()   
            self.target = player
            #change direction if near the edge
            if (self.__pos.a + self.__velo.a < 0 or self.__pos.a + self.__velo.a > Constants.WORLD_WIDTH):
                self.__velo.a = -self.__velo.a
            if (self.__pos.b + self.__velo.b < 0 or self.__pos.b + self.__velo.b > Constants.WORLD_HEIGHT):
                self.__velo.b = -self.__velo.b
            #update position
            self.__pos.__add__(self.__velo.scale(Constants.ENEMY_SPEED))
            
        else:
            self.target = None
            #randomly wander a little bit    
            self.__velo.a +=  (random.randrange(-Constants.ENEMY_WANDER_RANGE,Constants.ENEMY_WANDER_RANGE )/Constants.FRAME_RATE)  
            self.__velo.b +=  (random.randrange(-Constants.ENEMY_WANDER_RANGE,Constants.ENEMY_WANDER_RANGE )/Constants.FRAME_RATE)
            self.__velo = self.__velo.normalize()

            #change direction if near the edge
            if (self.__pos.a + self.__velo.a < 0 or self.__pos.a + self.__velo.a > Constants.WORLD_WIDTH):
                self.__velo.a = -self.__velo.a
            if (self.__pos.b + self.__velo.b < 0 or self.__pos.b + self.__velo.b > Constants.WORLD_HEIGHT):
                self.__velo.b = -self.__velo.b
            #update position
            self.__pos.__add__(self.__velo.scale(Constants.ENEMY_SPEED))
        #update center
        self.center = Agent.calcCenter(self)
        #clamp enemy to screen
        self.rect = self.rect.clamp(Constants.BOUNDARY_RECT)
