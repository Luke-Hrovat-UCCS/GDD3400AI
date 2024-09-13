import Constants
import Vector
import pygame
import random

class Enemy:
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
        #"private" Python doesn't have anything that specifically locks something up, but you can do this to make it sort of private
        self.__pos = __pos
        self.__velo = __velo
        self.size = size
        self.color = color
        
        self.center = Enemy.calcCenter(self)

        
    #display the enemy's size, position, velocity, and center for easy debugging    
    def __str__(self):
        print("Size: ",self.size)
        print("Position: (",self.__pos.a,", ",self.__pos.b,")")
        print("Velocity: (",self.__velo.a,", ",self.__velo.b,")")
        print("Center: (", self.center.a,",",self.center.b,")" )
        
    #draws enemy and a line representing velocity
    #draws Player and a line representing velocity
    def draw(self,screen):
        #draw self
        pygame.draw.rect(screen, self.color, pygame.Rect(self.__pos.a, self.__pos.b, self.size, self.size))
        #draw line
        lstart = (self.center[0], self.center[1])
        lend = (self.center[0] + self.__velo.scale(self.size).a, self.center[1]+self.__velo.scale(self.size).b)
        pygame.draw.line(screen, (0,0,255), lstart, lend,3)

    #updates position of enemy, Runs from player if player is in range, otherwise wanders, accepts a player object
    def update(self, player):
        #protection for if the player doesn't exist    
        if player is None:
            return
        #if player is in range RUN AWAY
        #calculate vector from player to enemy
        playerDist = Vector.Vector(player.center[0],player.center[1])
        playerDist.__add__(Vector.Vector(self.center[0],self.center[1]))

        #if length of that vector is less than the aggro range of the enemy, run away
        if(playerDist.length() < Constants.AGGRO_RANGE):
            #set velocity to the player vector and normalize    
            self.__velo = playerDist.normalize()   
            
            #update position
            self.__pos.__add__(self.__velo.scale(Constants.ENEMY_SPEED))
        else:
            #randomly wander a little bit    
            self.__velo.a +=  (random.randrange(-Constants.ENEMY_WANDER_RANGE,Constants.ENEMY_WANDER_RANGE )/Constants.FRAME_RATE)  
            self.__velo.b +=  (random.randrange(-Constants.ENEMY_WANDER_RANGE,Constants.ENEMY_WANDER_RANGE )/Constants.FRAME_RATE)
            self.__velo = self.__velo.normalize()

            #update position
            self.__pos.__add__(self.__velo.scale(Constants.ENEMY_SPEED))
        #update center
        self.center = Enemy.calcCenter(self)
    #compute the center of the enemy object based on its position and size
    def calcCenter(self):
        cent = (self.__pos.a + (self.size/2),self.__pos.b + (self.size/2))
        return cent