import Constants
import Vector
import pygame
import random

class Agent(object):
    
     #constructor that accepts a position, velocity, color, and size and initializes data members to represent those quantities
    def __init__(self,size=None,__pos=None,__velo=None,color=None):
        #initialize data members
        #"private" Python doesn't have anything that specifically locks something up, but you can do this to make it sort of private
        self.__pos = __pos
        self.__velo = __velo
        self.size = size
        self.color = color
        self.center = Agent.calcCenter(self)
        
    #display the player's size, position, velocity, and center for easy debugging    
    def __str__(self):
        print("Size: ",self.size)
        print("Position: (",self.__pos.a,", ",self.__pos.b,")")
        print("Velocity: (",self.__velo.a,", ",self.__velo.b,")")
        print("Center: (", self.center.a,",",self.center.b,")" )
       
    #draws Agent and a line representing velocity
    def draw(self,screen):
        #draw self
        pygame.draw.rect(screen, self.color, pygame.Rect(self.__pos.a, self.__pos.b, self.size, self.size))
        #draw line
        lstart = (self.center[0], self.center[1])
        lend = (self.center[0] + self.__velo.scale(self.size).a, self.center[1]+self.__velo.scale(self.size).b)
        pygame.draw.line(screen, (0,0,255), lstart, lend,3)    

        #updates position of player, chases enemies based on distance (closest),accepts a list of enemies
    def update(self,enemies):
       return
        
    #compute the center of the enemy object based on its position and size
    def calcCenter(self):
        cent = (self.__pos.a + (self.size/2),self.__pos.b + (self.size/2))
        return cent




