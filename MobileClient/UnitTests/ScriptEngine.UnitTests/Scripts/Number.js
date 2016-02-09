assert(true, isNaN(NaN));
assert(true, isNaN("string"));
assert(false, isNaN("12"));
assert(false, isNaN(12));

assert(169, 0251);
assert(169, 0xA9);