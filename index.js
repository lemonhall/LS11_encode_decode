/*
node.js 解码KONAMI的LS11格式文件的程序

*/

const fs 	= require('fs');
const bitwise = require('bitwise');

//用贴吧里的例子Kao.lzw
var raw_file_buf = fs.readFileSync("Kao.lzw");

console.log(raw_file_buf);

//参考文章
//https://tieba.baidu.com/p/2468272446?pn=2
//http://game.ali213.net/thread-5585948-1-1.html

var cursor = 0;

function readBuffer_from_file(desc_buf){
	raw_file_buf.copy(desc_buf, 0, cursor, cursor+desc_buf.length);
	cursor=cursor+desc_buf.length;
	console.log("cursor:",cursor);
}
//0x0000〜0x000F LS11文件标识，长度16个字节
//前4个字节是LS11（或LS12，LS10），剩下的12个字节为0x00
const file_meta = Buffer.allocUnsafe(16).fill('!');
readBuffer_from_file(file_meta);

//解码前四个字节为ASCII，即为LS11
const file_type = Buffer.allocUnsafe(4).fill('!');
	  raw_file_buf.copy(file_type, 0, 0, 4);
console.log("文件类型：",file_type.toString('ascii'),"\n==============");

//一个256位的数据字典
const dic = Buffer.allocUnsafe(256).fill('!');
readBuffer_from_file(dic);


//压缩后长度 4字节
const length_after_ziped = Buffer.allocUnsafe(4).fill('!');
readBuffer_from_file(length_after_ziped);
console.log(length_after_ziped);
console.log("length_after_ziped:",length_after_ziped.readInt32BE(),"\n==============");


//原始长度 4字节
var length_before_ziped = Buffer.allocUnsafe(4).fill('!');
readBuffer_from_file(length_before_ziped);
console.log(length_before_ziped);
console.log("length_before_ziped:",length_before_ziped.readInt32BE(),"\n==============");

//开始地址 4字节
var start_address = Buffer.allocUnsafe(4).fill('!');
readBuffer_from_file(start_address);
console.log(start_address);
console.log("start_address:",start_address,"\n==============");

//压缩后长度
var length_after_ziped_in_int = length_after_ziped.readInt32BE();
const file_content = Buffer.allocUnsafe(length_after_ziped_in_int).fill('!');
	  //这里强制将指针跳向start_address;
	  cursor = start_address.readInt32BE();
readBuffer_from_file(file_content);

//还是用KAO的第一段为例0D 14开始的内容为
//3C 93 F8 17 13 F8 3B 2F 13 F8 16 C3 …
//这就是了
//<Buffer 3c 93 f8 17 13 f8 3b 2f 13 f8 16 c3 0c b2 3
console.log(file_content);
console.log("file_content length:",file_content.length);

//2进制就是
//00111100 10010011 11111000 00010111 00010011 11111000….
//采样出来，用来方便比对文章用的
var byte_length = 8 ;
var ff = bitwise.readBuffer(file_content,0,byte_length);
//[ 0, 0, 1, 1, 1, 1, 0, 0 ] = 3c
console.log(ff);

var strConBinary = bitwise.readBuffer(file_content,0);
console.log("strConBinary length:",strConBinary.length);

var 	intPos 				= 0;
const 	lstDeConIndex 		= new Array();
var 	lstTempSeg1		 	= new Array();
var 	lstTempSeg2		 	= new Array();


function readInt32(buf){
   return buf.readUInt8();
}

function padBits(bitsArray){
	//console.log("Init length:",bitsArray.length);
	var init_length = bitsArray.length;
	if(bitsArray.length<8){
		for(var i=0;i<(8-init_length);i++){
			//console.log(bitsArray.length);
			bitsArray.unshift(0);
		}
	}
	//console.log("补齐后的bitsArray=====");
	//console.log(bitsArray);
	return bitsArray;
}
//这一段例程，在资料里出现了两种思路
//第一种，百度的，是边解析，边查表
//第二种，其实更简单，是先解析出来，然后一鼓作气直接再一次查表查完

while(intPos < strConBinary.length){
	var charValue = strConBinary[intPos];
		intPos++;
	lstTempSeg1.push(charValue);
	//console.log("charValue:",charValue);
	//遇0则段1结束
	if(charValue===0){
		   //取出对应长度段2
		   //console.log("读取了：",lstTempSeg1.length);
	       for (var i = 0; i < lstTempSeg1.length; i++){
		            lstTempSeg2.push(strConBinary[intPos]);
		            intPos++;
		   }
	   lstTempSeg1 = padBits(lstTempSeg1);
	   lstTempSeg2 = padBits(lstTempSeg2);
	   //console.log("lstTempSeg1:",lstTempSeg1);
	   //console.log("lstTempSeg2:",lstTempSeg2);
	   var lstTempSeg1_buf = bitwise.createBuffer(lstTempSeg1);
	   var lstTempSeg2_buf = bitwise.createBuffer(lstTempSeg2);
	   //https://msdn.microsoft.com/en-us/library/1k20k614(v=vs.110).aspx
	   //String strSeg1 = new String(lstTempSeg1.ToArray());
	   //int intItemValue = Convert.ToInt32(strSeg1, 2) + Convert.ToInt32(strSeg2, 2);
	   //C#版本里用了这种
	   //Converts the string representation of a number in a specified base to an equivalent 32-bit signed integer.
	    // public static int ToInt32(
		// 	string value,
		// 	int fromBase
		// )
		// Parameters
		// value
		// Type: System.String
		// A string that contains the number to convert.
		// fromBase
		// Type: System.Int32
		// The base of the number in value, which must be 2, 8, 10, or 16.
		// Return Value
		// Type: System.Int32
		// A 32-bit signed integer that is equivalent to the number in value, or 0 (zero) if value is null.
		// 这一段大概的意思是，从一个LIST===> Array==>String===>int32
		// 我呢，node这边，从一个array==>buf===>int

	   var Seg1_int 	   = readInt32(lstTempSeg1_buf)
	   var Seg2_int 	   = readInt32(lstTempSeg2_buf);
	   //console.log("Seg1_int:",Seg1_int);
	   //console.log("Seg2_int:",Seg2_int);

	   var intItemValue    = Seg1_int + Seg2_int;
	   
	   lstDeConIndex.push(intItemValue);
	   lstTempSeg1=[];
	   lstTempSeg2=[];
	}
}// END of while

console.log("lstDeConIndex_length:",lstDeConIndex.length);

//这个是存储最终结果的Array
var lstDeCon = new Array();
var length_before_ziped_int = length_before_ziped.readInt32BE();
//这个是字典
//dic

//region 遍历索引列表，从字典获取实际值
for (var i = 0; i < lstDeConIndex.length; i++)
{
	//if (lstDeCon.length >= intLen2) break;
	//intLen2原文代表着解压后长度
    if (lstDeCon.length >= length_before_ziped_int) break;
    var intIndex = lstDeConIndex[i];
    //console.log("intIndex:",intIndex);
    if (intIndex < 256)//字典范围内直接取值
    {
        lstDeCon.push(dic[intIndex]);
        //输出十六进制的最终结果单子
        //console.log(dic[intIndex].toString('16'));
        continue;
    }
    var intOffset = intIndex - 256;//大于255则向前复制取值
    //console.log("intOffset:",intOffset);
    var intLen = lstDeConIndex[i + 1] + 3;//下一个索引值加3为取值长度
    //console.log("取值长度：",intLen);
    i++;
    for (var j = 0; j < intLen; j++)
    {
    	var intTemp  = lstDeCon.length - intOffset;
    	//console.log("intTemp:",intTemp);
        var byteTemp = lstDeCon[intTemp];
        lstDeCon.push(byteTemp);
    }
}

//console.log(lstDeCon);

var result = Buffer.from(lstDeCon);
console.log("最终解压结果长度:",result.length);
// 打算用Buffer.from来还原，所以需要八进制
// Allocates a new Buffer using an array of octets.
// Creates a new Buffer containing UTF-8 bytes of the string 'buffer'
//  const buf = Buffer.from([0x62, 0x75, 0x66, 0x66, 0x65, 0x72]);



