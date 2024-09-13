import Constants
import Vector
import pygame

class Enemy:
    #constructor that accepts a position, velocity, color, and size and initializes data members to represent those quantities
    def __init__(self,size=None,__pos=None,__velo=None,color=None):
        #default constructor
        if(__pos is None):
            __pos = Vector.Vector(Constants.PLAYER_STARTING_POS[0],Constants.PLAYER_STARTING_POS[1])
        if(__velo is None):
            __velo = Vector.Vector(0,0)
        if(size is None):
            size = Constants.ENEMY_SIZE
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
    def draw(self,screen):
        #draw self
        pygame.draw.rect(screen, self.color, pygame.Rect(self.__pos.a, self.__pos.b, self.size, self.size))
        #draw line
        lstart = (self.center.a, self.center.b)
        direction = self.center
        direction.__add__(self.__velo.scale(self.size))
        lend = (direction.a,direction.b)
        pygame.draw.line(screen, (0,0,255), lstart, lend,3)
    '''    
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
                d= ((enemy.center.a-self.center.a)**2+(enemy.center.b-self.center.b)**2)**.5
                #if distance is smaller than previous option set new potential target
                if d <= enemyDist:
                    enemyDist = d
                    potentialTarg = enemy
            #update target with closest enemy
            self.target = potentialTarg
        #if target exists chase target
        else:
            #calculate directon, and normalize
            enemyDirect = self.target.center
            enemyDirect.__sub__(self.center)
            self.velo = enemyDirect
            nVelo = self.__velo.normalize()
        
            #update position
            self.__pos.__add__(nVelo.scale(Constants.PLAYER_SPEED))
         ''' 
    #updates position of enemy, Runs from player if player is in range, otherwise wanders, accepts a player object
    def update(self, player):
        #protection for if the player doesn't exist    
        if player is None:
            return
        #if player is in range RUN AWAY
        #calculate vector from player to enemy
        playerDist = player.center
        playerDist.__sub__(self.center)
        #if length of that vector is less than the aggro range of the enemy, run away
        if(playerDist.length < Constants.AGGRO_RANGE):
            #set velocity to the player vector and normalize    
            self.__velo = playerDist   
            nVelo = self.__velo.normalize()
            
            #update position
            self.__pos.__add__(nVelo.scale(Constants.ENEMY_SPEED))
        else:
                
    #compute the center of the enemy object based on its position and size
    def calcCenter(self):
        cent = Vector.Vector(self.__pos.a + (self.size/2),self.__pos.b + (self.size/2))
        return cent