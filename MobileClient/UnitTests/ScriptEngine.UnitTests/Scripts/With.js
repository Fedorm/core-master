var a, x, y;
var r = 10;
with (Math) {
    a = PI * r * r;
    x = r * cos(PI);
    y = r * sin(PI / 2);
}

assert(-10, x);
assert(10, y);
