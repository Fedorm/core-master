var d = new Date();

var k = 3;

function truc(x) {
    return k * x;
}

for (var i = 0; i < 100000; i++) {
    truc(i);
}

var e = new Date();
System.Console.WriteLine(e - d);