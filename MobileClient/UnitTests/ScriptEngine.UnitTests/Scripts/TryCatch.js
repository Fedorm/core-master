var x = 1;

try {
    throw "test";
    x = 0;
}
catch (e) {
    assert(1, x);
}

try {
    throw x;
}
catch (e) {

    assert(1, x);
    assert(e, x);
}
finally {
    x = 2;
}

assert(2, x);

// loops should not eat exceptions
catched = false;
try {
    for (x = 0; x < 10; x++) {
        var z = 1;
        if (x == 3) throw "loop";
    }
}
catch (e) {
    catched = true;
    assert("loop", e);
    assert(3, x);
}

istrue(catched);
assert(3, x);
assert('undefined', typeof z);

var error = false;
try {
    var foo = 'bar' in 'baz'; // in throws an exception if right member is not Object
    error = true;
}
catch (e) {
}

assert(false, error);