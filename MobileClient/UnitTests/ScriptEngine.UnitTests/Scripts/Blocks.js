{ 
    { 
        { 
            var i; 
            i = 1; 
            return i; 
        }
    }
}

assert(1, i);

{ 
    var i = 1; 
} 
{ 
    i = i + 1; 
} 
{ 
    return i; 
}

assert(2, i);