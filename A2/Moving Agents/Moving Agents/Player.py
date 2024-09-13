import Constants
import Vector
import pygame

class Player:
    #constructor that accepts a position, velocity, color, and size and initializes data members to represent those quantities
    def __init__(self,size=None,__pos=None,__velo=None,color=None):
        #default constructor
        if(__pos is None):
            __pos = Vector.Vector(Constants.PLAYER_STARTING_POS[0],Constants.PLAYER_STARTING_POS[1])
        if(__velo is None):
            __velo = Vector.Vector(0,0)
        if(size is None):
            size = Constants.PLAYER_SIZE
        if(color is None):
            color = Constants.PLAYER_COLOR
        #initialize data members
        #"private" Python doesn't have anything that specifically locks something up, but you can do this to make it sort of private
        self.__pos = __pos
        self.__velo = __velo
        
        self.size = size
        self.color = color
        self.target = None
        self.center = Player.calcCenter(self)

        
    #display the player's size, position, velocity, and center for easy debugging    
    def __str__(self):
        print("Size: ",self.size)
        print("Position: (",self.__pos.a,", ",self.__pos.b,")")
        print("Velocity: (",self.__velo.a,", ",self.__velo.b,")")
        print("Center: (", self.center.a,",",self.center.b,")" )
        
    #draws Player and a line representing velocity
    def draw(self,screen):
        #draw self
        pygame.draw.rect(screen, self.color, pygame.Rect(self.__pos.a, self.__pos.b, self.size, self.size))
        #draw line
        lstart = (self.center[0], self.center[1])
        lend = (self.center[0] + self.__velo.scale(self.size).a, self.center[1]+self.__velo.scale(self.size).b)
        pygame.draw.line(screen, (0,0,255), lstart, lend,3)
        
    #updates position of player, chases enemies based on distance (closest),accepts a list of enemies
    def update(self,enemies):
        #catch case where no enemies exist
        if(enemies == []):
            return
        #if no target exists find new target
        if(self.target is None):
            enemyDist = 10000
            potentialTarg = None
            #for each enemy in enemies
            for enemy in enemies:
                #calculate distance
                d= ((enemy.center[0]-self.center[0])**2+(enemy.center[1]-self.center[1])**2)**.5
                #if distance is smaller than previous option set new potential target
                if d <= enemyDist:
                    enemyDist = d
                    potentialTarg = enemy
            #update target with closest enemy
            self.target = potentialTarg
        #if target exists chase target
        elif (self.target != None) :
            #calculate directon, and normalize
            nVelo = Vector.Vector(self.target.center[0],self.target.center[1])
            nVelo.__sub__(self.__pos)
            self.__velo = nVelo.normalize()
        
            #update position
            self.__pos.__add__(self.__velo.scale(Constants.PLAYER_SPEED))
        #update center
        self.center = Player.calcCenter(self)
                
    #compute the center of the Player object based on its position and size
    def calcCenter(self):
        cent = (self.__pos.a + (self.size/2),self.__pos.b + (self.size/2))
        return cent



