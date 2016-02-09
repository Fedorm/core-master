// 15.2.2

var a = new Object();
alert(a.toString());

assert(true, Object.hasOwnProperty.call({ foo: 3 }, 'foo'));
assert(false, Object.hasOwnProperty.call({ foo: 3 }, 'dummy'));

function foo() {

}

(function($foo) { $foo.bar = function() { }; })(foo)

assert('function', typeof foo.bar);
