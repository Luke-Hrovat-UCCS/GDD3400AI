class Vector:
    #constructor that accepts the x and y for a vector
    def __init__(self, a, b):
        self.a = a
        self.b = b
    
    #method that converts a vector to a pretty string
    def __str__(self):
        print("Vector(",self.a, ", ", self.b, ")")
    
    #overloads the '+' operator to add two vectors
    def __add__(self,Vector1):
        self.a = Vector1.a + self.a
        self.b = Vector1.b + self.b

        
    #overloads the '-' operator to subtract two vectors
    def __sub__(self, Vector1):
        self.a = self.a - Vector1.a
        self.b = self.b - Vector1.b
    
    #method that returns the dot product of 2 vectors
    def dot(self, Vector1):
        x = Vector1.a * self.a
        y = Vector1.b * self.b
        return x+y
    
    #method that scales the vector by a float, returning the scaled vector
    def scale(self, s):
        x = s *self.a
        y = s *self.b
        newVector = Vector(x,y) 
        return newVector
    
    #method that returns the length of the vector, this is also called 'magnitude' of a vector
    def length(self):
        x = self.a**2
        y = self.b**2
        return (x+y)**.5

    #method that returns the normalized version of the vector
    def normalize(self):
        sLength = self.length()
        if(sLength > 0):
            x = self.a/sLength
            y = self.b/sLength
            newVector = Vector(x,y) 
            return newVector
        else:
            return self
        


