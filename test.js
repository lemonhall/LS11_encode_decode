const bitwise = require('bitwise');

function padBits(bitsArray){
	//console.log("Init length:",bitsArray.length);
	var init_length = bitsArray.length;
	if(bitsArray.length<8){
		for(var i=0;i<(8-init_length);i++){
			//console.log(bitsArray.length);
			bitsArray.unshift(0);
		}
	}
	console.log("补齐后的bitsArray=====");
	console.log(bitsArray);
	return bitsArray;
}

var a = [0,1];
var b = [1,1,1,1,1,1,1,0];
var c = [0,0,0,0,0,1,0,1];
var buf = bitwise.createBuffer(padBits(c));
console.log(buf.readUInt8());

